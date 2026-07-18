using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EasyModbus;



namespace ModbusLibrary
{
    /// <summary>Represents the current state of a Modbus TCP connection.</summary>
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Reconnecting,
        Faulted
    }

    /// <summary>Supplies information when the Modbus connection state changes.</summary>
    public sealed class ConnectionStateChangedEventArgs : EventArgs
    {
        public ConnectionStateChangedEventArgs(ConnectionState state, Exception exception)
        {
            State = state;
            Exception = exception;
        }

        public ConnectionState State { get; private set; }
        public Exception Exception { get; private set; }
    }

    /// <summary>Thrown when a TCP connection cannot be established or restored.</summary>
    public class ModbusConnectionException : Exception
    {
        public ModbusConnectionException(string message) : base(message) { }
        public ModbusConnectionException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>Thrown when a Modbus request cannot be completed.</summary>
    public class ModbusCommunicationException : Exception
    {
        public ModbusCommunicationException(string message) : base(message) { }
        public ModbusCommunicationException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>Optional logging abstraction for the connection manager.</summary>
    public interface ILogger
    {
        void Log(string message, Exception exception = null);
    }

    /// <summary>
    /// Thread-safe Modbus TCP master. All operations use one semaphore because an EasyModbus
    /// master and its underlying TCP stream must not serve concurrent requests.
    /// </summary>
    public class ModbusConnectionManager : IDisposable
    {
        private readonly SemaphoreSlim modbusSemaphore = new SemaphoreSlim(1, 1);
        private readonly object stateSync = new object();
        private readonly ILogger logger;
        private ModbusClient modbusClient;
        private string ipAddress;
        private int port;
        private int defaultSlaveId;
        private int timeoutMilliseconds;
        private int reconnectMaxAttempts;
        private int reconnectIntervalMilliseconds;
        private ConnectionState connectionState = ConnectionState.Disconnected;
        private bool disposed;

        public ModbusConnectionManager(
            int timeoutMilliseconds = 5000,
            int reconnectMaxAttempts = 3,
            int reconnectIntervalMilliseconds = 1000,
            int defaultSlaveId = 1,
            ILogger logger = null)
        {
            ValidateConfiguration(timeoutMilliseconds, reconnectMaxAttempts, reconnectIntervalMilliseconds, defaultSlaveId);
            this.timeoutMilliseconds = timeoutMilliseconds;
            this.reconnectMaxAttempts = reconnectMaxAttempts;
            this.reconnectIntervalMilliseconds = reconnectIntervalMilliseconds;
            this.defaultSlaveId = defaultSlaveId;
            this.logger = logger;
        }

        public event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;

        public bool IsConnected
        {
            get { lock (stateSync) { return connectionState == ConnectionState.Connected; } }
        }

        public int TimeoutMilliseconds
        {
            get { lock (stateSync) { return timeoutMilliseconds; } }
            set { ThrowIfDisposed(); if (value <= 0) throw new ArgumentOutOfRangeException("value"); lock (stateSync) { timeoutMilliseconds = value; } }
        }

        public int ReconnectMaxAttempts
        {
            get { lock (stateSync) { return reconnectMaxAttempts; } }
            set { ThrowIfDisposed(); if (value <= 0) throw new ArgumentOutOfRangeException("value"); lock (stateSync) { reconnectMaxAttempts = value; } }
        }

        public int ReconnectIntervalMilliseconds
        {
            get { lock (stateSync) { return reconnectIntervalMilliseconds; } }
            set { ThrowIfDisposed(); if (value < 0) throw new ArgumentOutOfRangeException("value"); lock (stateSync) { reconnectIntervalMilliseconds = value; } }
        }

        public int DefaultSlaveId
        {
            get { lock (stateSync) { return defaultSlaveId; } }
            set { ThrowIfDisposed(); ValidateSlaveId(value, "value"); lock (stateSync) { defaultSlaveId = value; } }
        }

        /// <summary>Connects to a Modbus TCP server.</summary>
        public async Task ConnectAsync(string ipAddress, int port, int slaveId = 1, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            ValidateEndpoint(ipAddress, port);
            ValidateSlaveId(slaveId, "slaveId");
            var notifications = new List<ConnectionStateChangedEventArgs>();
            await modbusSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                ThrowIfDisposed();
                AddStateChange(notifications, ConnectionState.Connecting, null);
                CloseConnectionCore();
                this.ipAddress = ipAddress;
                this.port = port;
                lock (stateSync) { this.defaultSlaveId = slaveId; }
                try
                {
                    await OpenConnectionCoreAsync(cancellationToken).ConfigureAwait(false);
                    AddStateChange(notifications, ConnectionState.Connected, null);
                }
                catch (OperationCanceledException)
                {
                    CloseConnectionCore();
                    AddStateChange(notifications, ConnectionState.Disconnected, null);
                    throw;
                }
                catch (Exception ex)
                {
                    CloseConnectionCore();
                    var wrapped = new ModbusConnectionException("Unable to connect to Modbus TCP server " + ipAddress + ":" + port + ".", ex);
                    AddStateChange(notifications, ConnectionState.Faulted, wrapped);
                    Log(wrapped.Message, wrapped);
                    throw wrapped;
                }
            }
            finally { modbusSemaphore.Release(); RaiseNotifications(notifications); }
        }

        /// <summary>Closes the TCP connection and releases its Modbus resources.</summary>
        public async Task DisconnectAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            var notifications = new List<ConnectionStateChangedEventArgs>();
            await modbusSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try { CloseConnectionCore(); AddStateChange(notifications, ConnectionState.Disconnected, null); }
            finally { modbusSemaphore.Release(); }
            RaiseNotifications(notifications);
        }

        /// <summary>Reconnects using the last successfully requested endpoint.</summary>
        public async Task ReconnectAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            var notifications = new List<ConnectionStateChangedEventArgs>();
            await modbusSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (String.IsNullOrWhiteSpace(ipAddress) || port <= 0)
                    throw new ModbusConnectionException("Reconnect was requested before an endpoint was configured.");

                AddStateChange(notifications, ConnectionState.Reconnecting, null);
                CloseConnectionCore();
                Exception lastException = null;
                int attempts;
                lock (stateSync) { attempts = reconnectMaxAttempts; }
                for (int attempt = 1; attempt <= attempts; attempt++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        await OpenConnectionCoreAsync(cancellationToken).ConfigureAwait(false);
                        AddStateChange(notifications, ConnectionState.Connected, null);
                        return;
                    }
                    catch (OperationCanceledException)
                    {
                        CloseConnectionCore();
                        AddStateChange(notifications, ConnectionState.Disconnected, null);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                        CloseConnectionCore();
                        Log("Modbus reconnect attempt " + attempt + " failed for " + ipAddress + ":" + port + ".", ex);
                        if (attempt < attempts)
                        {
                            int interval;
                            lock (stateSync) { interval = reconnectIntervalMilliseconds; }
                            await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
                var wrapped = new ModbusConnectionException("Failed to reconnect to Modbus TCP server " + ipAddress + ":" + port + " after " + attempts + " attempts.", lastException);
                AddStateChange(notifications, ConnectionState.Faulted, wrapped);
                throw wrapped;
            }
            finally { modbusSemaphore.Release(); RaiseNotifications(notifications); }
        }

        public async Task<ushort[]> ReadHoldingRegistersAsync(ushort startAddress, ushort count, int slaveId = -1, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (count == 0 || count > 125) throw new ArgumentOutOfRangeException("count", "A Modbus holding-register read must contain 1 to 125 registers.");
            int resolvedSlaveId = ResolveSlaveId(slaveId);
            return await ExecuteCommunicationAsync("read holding registers", resolvedSlaveId, cancellationToken,
                client => ConvertToUShorts(client.ReadHoldingRegisters(startAddress, count))).ConfigureAwait(false);
        }

        public Task WriteSingleHoldingRegisterAsync(ushort address, ushort value, int slaveId = -1, CancellationToken cancellationToken = default(CancellationToken))
        {
            int resolvedSlaveId = ResolveSlaveId(slaveId);
            return ExecuteCommunicationAsync("write single holding register", resolvedSlaveId, cancellationToken, master =>
            {
                master.WriteSingleRegister(address, value);
                return true;
            });
        }

        public Task WriteMultipleHoldingRegistersAsync(ushort startAddress, ushort[] values, int slaveId = -1, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (values == null) throw new ArgumentNullException("values");
            if (values.Length == 0 || values.Length > 123) throw new ArgumentOutOfRangeException("values", "A Modbus multiple-register write must contain 1 to 123 registers.");
            int resolvedSlaveId = ResolveSlaveId(slaveId);
            return ExecuteCommunicationAsync("write multiple holding registers", resolvedSlaveId, cancellationToken, master =>
            {
                var registerValues = new int[values.Length];
                for (int i = 0; i < values.Length; i++) registerValues[i] = values[i];
                master.WriteMultipleRegisters(startAddress, registerValues);
                return true;
            });
        }

        private async Task<T> ExecuteCommunicationAsync<T>(string operation, int slaveId, CancellationToken cancellationToken, Func<ModbusClient, T> action)
        {
            ThrowIfDisposed();
            var notifications = new List<ConnectionStateChangedEventArgs>();
            await modbusSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                ThrowIfDisposed();
                if (modbusClient == null || !modbusClient.Connected || !IsConnected)
                    throw new ModbusCommunicationException("Cannot " + operation + " because Modbus TCP server " + EndpointDescription() + " is not connected.");
                try
                {
                    return await Task.Run(() =>
                    {
                        modbusClient.UnitIdentifier = (byte)slaveId;
                        return action(modbusClient);
                    }, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    CloseConnectionCore();
                    var wrapped = new ModbusCommunicationException("Failed to " + operation + " on Modbus TCP server " + EndpointDescription() + " (slave " + slaveId + ").", ex);
                    AddStateChange(notifications, ConnectionState.Faulted, wrapped);
                    Log(wrapped.Message, wrapped);
                    throw wrapped;
                }
            }
            finally { modbusSemaphore.Release(); RaiseNotifications(notifications); }
        }

        private async Task OpenConnectionCoreAsync(CancellationToken cancellationToken)
        {
            ModbusClient client = null;
            try
            {
                int timeout;
                int slaveId;
                lock (stateSync) { timeout = timeoutMilliseconds; slaveId = defaultSlaveId; }
                cancellationToken.ThrowIfCancellationRequested();
                client = new ModbusClient(ipAddress, port);
                client.ConnectionTimeout = timeout;
                client.UnitIdentifier = (byte)slaveId;
                // EasyModbus exposes synchronous I/O. Keep the semaphore during this
                // worker-pool operation so no other caller can use the client concurrently.
                await Task.Run(() => client.Connect(), cancellationToken).ConfigureAwait(false);
                modbusClient = client;
            }
            catch
            {
                if (client != null && client.Connected) client.Disconnect();
                throw;
            }
        }

        private void CloseConnectionCore()
        {
            if (modbusClient != null)
            {
                try { if (modbusClient.Connected) modbusClient.Disconnect(); }
                catch (Exception ex) { Log("Error while closing Modbus TCP client.", ex); }
                finally { modbusClient = null; }
            }
        }

        private int ResolveSlaveId(int slaveId)
        {
            int result = slaveId;
            if (result == -1) lock (stateSync) { result = defaultSlaveId; }
            ValidateSlaveId(result, "slaveId");
            return result;
        }

        private static ushort[] ConvertToUShorts(int[] registerValues)
        {
            var values = new ushort[registerValues.Length];
            for (int i = 0; i < registerValues.Length; i++) values[i] = unchecked((ushort)registerValues[i]);
            return values;
        }

        private void AddStateChange(List<ConnectionStateChangedEventArgs> notifications, ConnectionState newState, Exception exception)
        {
            lock (stateSync)
            {
                if (connectionState == newState) return;
                connectionState = newState;
                notifications.Add(new ConnectionStateChangedEventArgs(newState, exception));
            }
        }

        private void RaiseNotifications(IEnumerable<ConnectionStateChangedEventArgs> notifications)
        {
            foreach (var notification in notifications)
            {
                var handler = ConnectionStateChanged;
                if (handler == null) continue;
                try { handler(this, notification); }
                catch (Exception ex) { Log("A ConnectionStateChanged event handler threw an exception.", ex); }
            }
        }

        private string EndpointDescription() { return String.IsNullOrEmpty(ipAddress) ? "<not configured>" : ipAddress + ":" + port; }
        private void Log(string message, Exception exception = null) { if (logger != null) logger.Log(message, exception); else Debug.WriteLine(message + (exception == null ? String.Empty : " " + exception)); }
        private void ThrowIfDisposed() { lock (stateSync) { if (disposed) throw new ObjectDisposedException(GetType().FullName); } }
        private static void ValidateEndpoint(string address, int endpointPort) { if (String.IsNullOrWhiteSpace(address)) throw new ArgumentException("An IP address or host name is required.", "ipAddress"); if (endpointPort < 1 || endpointPort > 65535) throw new ArgumentOutOfRangeException("port"); }
        private static void ValidateSlaveId(int slaveId, string parameterName) { if (slaveId < 1 || slaveId > 247) throw new ArgumentOutOfRangeException(parameterName, "The Modbus slave ID must be from 1 to 247."); }
        private static void ValidateConfiguration(int timeout, int attempts, int interval, int slaveId) { if (timeout <= 0) throw new ArgumentOutOfRangeException("timeoutMilliseconds"); if (attempts <= 0) throw new ArgumentOutOfRangeException("reconnectMaxAttempts"); if (interval < 0) throw new ArgumentOutOfRangeException("reconnectIntervalMilliseconds"); ValidateSlaveId(slaveId, "defaultSlaveId"); }

        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            lock (stateSync) { if (disposed) return; disposed = true; }
            modbusSemaphore.Wait();
            try { CloseConnectionCore(); lock (stateSync) { connectionState = ConnectionState.Disconnected; } }
            finally { modbusSemaphore.Release(); modbusSemaphore.Dispose(); }
        }
    }
}



