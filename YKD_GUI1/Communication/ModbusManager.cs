using EasyModbus;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public class ModbusManager
{
    private ModbusClient _client;
    private readonly string _ip;
    private readonly int _port;

    private readonly object _locker = new object();
    private CancellationTokenSource _connectCts;
    private Task _connectTask;

    private CancellationTokenSource _readCts;
    private Task _readTask;

    private readonly ConcurrentDictionary<int, int> _cache = new ConcurrentDictionary<int, int>();
    private readonly ConcurrentDictionary<int, float> _cache1 = new ConcurrentDictionary<int, float>();



    /// <summary>
    /// 是否保持连接
    /// </summary>
    private volatile bool _running;

    /// <summary>
    /// PLC连接状态变化事件
    /// </summary>
    public event Action<ConnectionState> ConnectionStateChanged;

    /// <summary>
    /// 当前连接状态
    /// </summary>
    public bool IsConnected
    {
        get
        {
            return _client != null && _client.Connected;
        }
    }

    public ModbusManager(string ip = "127.0.0.1", int port = 502)
    {
        _ip = ip;
        _port = port;
    }

    #region 辅助函数

    /// <summary>
    /// 数据功能辅助函数
    /// </summary>
    private T Execute<T>(Func<T> action)
    {
        lock (_locker)
        {
            try
            {
                EnsureConnected();
                return action();
            }
            catch
            {
                Disconnect();
                throw;
            }
        }
    }

    private void Execute(Action action)
    {
        lock (_locker)
        {
            try
            {
                EnsureConnected();
                action();
            }
            catch
            {
                throw;
            }
        }
    }

    private void EnsureConnected()
    {
        if (!IsConnected)
            throw new Exception("PLC未连接");
    }

    #endregion

    #region 任务控制

    /// <summary>
    /// 开始保持连接
    /// </summary>
    public void Start()
    {
        if (_running)
            return;

        _running = true;

        //检查连接线程
        _connectCts = new CancellationTokenSource();
        _connectTask = Task.Run(() => ConnectionLoop(_connectCts.Token));





    }

    /// <summary>
    /// 停止连接
    /// </summary>
    public void Stop()
    {
        _running = false;

        _connectCts?.Cancel();

        try
        {
            _connectTask?.Wait();


        }
        catch
        {
        }
        Disconnect();
    }


    private void ConnectionLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                if (!IsConnected)
                {
                    Connect();
                    _readCts = new CancellationTokenSource();
                    _readTask = Task.Run(() => Poll(_readCts.Token));
                }
                else
                {
                    // 心跳检测
                    try
                    {
                        _client.ReadHoldingRegisters(0, 1);
                    }
                    catch
                    {
                        Console.WriteLine($"[{DateTime.Now}] PLC心跳失败，准备重连");

                        Disconnect();
                        _readCts?.Cancel();
                        _readTask?.Wait();

                    }
                }
            }
            catch
            {
            }

            token.WaitHandle.WaitOne(1000);
        }
    }



    #endregion

    #region 连接控制

    /// <summary>
    /// 建立连接
    /// </summary>
    private bool Connect()
    {
        lock (_locker)
        {
            try
            {
                if (_client != null)
                {
                    try
                    {
                        _client.Disconnect();
                    }
                    catch
                    {
                    }
                }


                _client = new ModbusClient(_ip, _port);
                _client.ConnectionTimeout = 1000;

                ConnectionStateChanged?.Invoke(ConnectionState.Connecting);
                _client.Connect();
                ConnectionStateChanged?.Invoke(ConnectionState.Connected);
                Console.WriteLine($"[{DateTime.Now}] PLC连接成功");



                return true;
            }
            catch (Exception ex)
            {
                ConnectionStateChanged?.Invoke(ConnectionState.ConnectFailed);
                Console.WriteLine($"[{DateTime.Now}] PLC连接失败：{ex.Message}");

                return false;
            }
        }
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    private void Disconnect()
    {
        lock (_locker)
        {
            try
            {
                if (_client != null)
                {
                    _client.Disconnect();

                   

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            ConnectionStateChanged?.Invoke(ConnectionState.Disconnected);

            Console.WriteLine($"[{DateTime.Now}] PLC已断开");
        }
    }

    #endregion

    #region 数据功能函数

    /// <summary>
    /// FC01读取线圈
    /// </summary>
    public bool[] ReadCoils(int startAddress, int count)
    {
        return Execute(() =>
        {
            return _client.ReadCoils(startAddress, count);
        });
    }

    /// <summary>
    /// FC02读取离散型输入
    /// </summary>
    public bool[] ReadDiscreteInputs(int startAddress, int count)
    {
        return Execute(() =>
        {
            return _client.ReadDiscreteInputs(startAddress, count);
        });
    }

    /// <summary>
    /// FC03读取保持寄存器
    /// </summary>
    public  int[] ReadHoldingRegisters_INT16(int startAddress, int count)
    {
        return Execute(() =>
        {

            return _client.ReadHoldingRegisters(startAddress, count);
        });
    }
    public int[] ReadHoldingRegisters_INT32(int startAddress, int count)
    {
        return Execute(() =>
        {
            int[] s = _client.ReadHoldingRegisters(startAddress, count * 2);
            int[] e = new int[count];
            for (int i = 0; i < s.Length; i += 2)
            {
                e[i] = ModbusClient.ConvertRegistersToInt(new int[] { s[i], s[i + 1] });
            }

            return e;
        });
    }
    public float[] ReadHoldingRegisters_Float(int startAddress, int count)
    {
        return Execute(() =>
        {

            int[] s = _client.ReadHoldingRegisters(startAddress, count * 2);
            float[] e = new float[count];
            int count1 = 0;
            for (int i = 0; i < s.Length; i += 2)
            {
                e[count1] = ModbusClient.ConvertRegistersToFloat(new int[] { s[i], s[i + 1] });
                count1++;
            }

            return e;
        });
    }


    /// <summary>
    /// FC04读取输入寄存器
    /// </summary>
    public int[] ReadInputRegisters_INT16(int startAddress, int count)
    {
        return Execute(() =>
        {
            return _client.ReadInputRegisters(startAddress, count);
        });
    }
    public int[] ReadInputRegisters_INT32(int startAddress, int count)
    {
        return Execute(() =>
        {
            int[] s = _client.ReadInputRegisters(startAddress, count * 2);
            int[] e = new int[count];
            for (int i = 0; i < s.Length; i += 2)
            {
                e[i / 2] = ModbusClient.ConvertRegistersToInt(new int[] { s[i], s[i + 1] });
            }

            return e;
        });
    }
    public float[] ReadInputRegisters_Float(int startAddress, int count)
    {
        return Execute(() =>
        {
            int[] s = _client.ReadInputRegisters(startAddress, count * 2);
            float[] e = new float[count];
            for (int i = 0; i < s.Length; i += 2)
            {
                e[i] = ModbusClient.ConvertRegistersToFloat(new int[] { s[i], s[i + 1] });
            }

            return e;
        });
    }


    /// <summary>
    /// FC05写入单个线圈
    /// </summary>
    public void WriteSingleCoil(int startAddress, bool value)
    {
        Execute(() =>
       {
           _client.WriteSingleCoil(startAddress, value);
           return;
       });
    }
    /// <summary>
    /// FC15写入多个线圈
    /// </summary>
    public void WriteMultipleCoils(int startAddress, bool[] value)
    {
        Execute(() =>
        {
            _client.WriteMultipleCoils(startAddress, value);
            return;
        });
    }


    /// <summary>
    /// FC06写入单个寄存器
    /// </summary>
    public void WriteSingleRegister(int startAddress, int value)
    {
        Execute(() =>
        {
            _client.WriteSingleRegister(startAddress, value);
            return;
        });
    }
    /// <summary>
    /// FC16写入单个寄存器
    /// </summary>
    public void WriteMultipleRegisters(int startAddress, int[] value)
    {
        Execute(() =>
        {
            _client.WriteMultipleRegisters(startAddress, value);
            return;
        });
    }

    #endregion

    #region 数据缓存
    private  void Poll(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (IsConnected)
            {
                try
                {
                    int[] data0 = ReadHoldingRegisters_INT16(2000, 2);
                    int[] data0_1 = ReadHoldingRegisters_INT16(200, 7);

                    float[] data1 =  ReadHoldingRegisters_Float(220, 6);

                    _cache[0] = data0[0];
                    _cache[1] = data0[1];

                    for (int i = 0; i < data0_1.Length; i++)
                    {
                        _cache[i+2] = data0_1[i];
                    }

                    for (int i = 0; i < data1.Length; i++)
                    {
                        _cache1[i] = data1[i];
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            token.WaitHandle.WaitOne(10);
        }

    }

    public int Read_INT(int address)
    {
        if (_cache.TryGetValue(address, out int value))
        {
            return value;
        }

        return 0;
    }
    public float Read_Float(int address)
    {
        if (_cache1.TryGetValue(address, out float value))
        {
            return value;
        }

        return 0;
    }

    #endregion

}