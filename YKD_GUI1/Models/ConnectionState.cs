using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    /// <summary>
    /// PLC连接状态
    /// </summary>
    public enum ConnectionState
    {
        /// <summary>
        /// 未连接
        /// </summary>
        Disconnected,

        /// <summary>
        /// 正在连接
        /// </summary>
        Connecting,

        /// <summary>
        /// 已连接
        /// </summary>
        Connected,

        /// <summary>
        /// 连接失败
        /// </summary>
        ConnectFailed,

        /// <summary>
        /// 正在重连
        /// </summary>
        Reconnecting

        

    }

