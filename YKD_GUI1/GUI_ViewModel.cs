using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;
using System.Timers;
using System.Drawing;
    
public class GUI_ViewModel : INotifyPropertyChanged
{
    // 全局唯一静态实例
    public static GUI_ViewModel Instance { get; } = new GUI_ViewModel();

    #region 事件通知函数
    private readonly SynchronizationContext _ui;

    // 捕获当前线程（通常是UI线程）的同步上下文
    private GUI_ViewModel()
    {
        _ui = SynchronizationContext.Current;
    }


    ///// 线程安全的属性变更通知
    ///// </summary>
    //protected void OnPropertyChanged([CallerMemberName] string name = null)
    //{
    //    if (_ui != null && SynchronizationContext.Current != _ui)
    //    {
    //        _ui.Post(_ => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)), null);
    //    }
    //    else
    //    {
    //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    //    }
    //}

    //private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propName = null)
    //{
    //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    //}

    /// <summary>
    /// 线程安全的属性变更通知
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        var targetContext = _ui ?? SynchronizationContext.Current; // 降级策略
        var currentContext = SynchronizationContext.Current;

        if (targetContext != null && currentContext != null && targetContext != currentContext)
        {
            targetContext.Post(_ => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)), null);
        }
        else
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }



    /// <summary>
    /// 事件
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    #endregion


    #region Robot


    #endregion



    #region  Software


    /// <summary>
    /// 下位机通讯状态
    /// </summary>
    private string _SysConStatus = "亿凯达自动上下料监控系统 (Disconnected)";
    public string SysConStatus
    {
        get => _SysConStatus;
        set
        {
            _SysConStatus = $"亿凯达自动上下料监控系统 ({value})";
            OnPropertyChanged();
        }
    }


    /// <summary>
    /// 切换按钮颜色状态
    /// </summary>

    private static readonly Color ActiveColor = Color.FromArgb(77, 140, 216);
    private static readonly Color InactiveColor = Color.FromArgb(28, 102, 197);
    public Color btn_Win1BackColor => btn_Win == 0 ? ActiveColor : InactiveColor;
    public Color btn_Win2BackColor => btn_Win == 1 ? ActiveColor : InactiveColor;
    public Color btn_Win3BackColor => btn_Win == 2 ? ActiveColor : InactiveColor;

    private int _btn_Win = 0;
    public int btn_Win
    {
        get => _btn_Win;
        set
        {
            _btn_Win = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(btn_Win1BackColor));
            OnPropertyChanged(nameof(btn_Win2BackColor));
            OnPropertyChanged(nameof(btn_Win3BackColor));
        }
    }

    private static readonly string[] DeviceSysStatus = { "系统停止中", "系统待机中", "系统运行中...", "系统故障" };
    private static readonly Color[] DeviceSysStatusBackColor = { Color.FromArgb(254, 226, 226), Color.FromArgb(254, 243, 199), Color.FromArgb(209, 250, 229), Color.FromArgb(254, 226, 226) };
    private static readonly Color[] DeviceSysStatusForeColor = { Color.FromArgb(178, 76, 76), Color.FromArgb(159, 86, 36), Color.FromArgb(9, 156, 109), Color.FromArgb(178, 76, 76) };
    //系统状态
    public string SysStatusText => DeviceSysStatus[SysStatus];
    public Color SysStatusBackColor => DeviceSysStatusBackColor[SysStatus];
    public Color SysStatusForeColor => DeviceSysStatusForeColor[SysStatus];

    private int _SysStatus = 0;
    public int SysStatus
    {
        get => _SysStatus;
        set
        {
            _SysStatus = value;
            OnPropertyChanged(nameof(SysStatusText));
            OnPropertyChanged(nameof(SysStatusBackColor));
            OnPropertyChanged(nameof(SysStatusForeColor));
        }
    }


    #region 首页

    /// <summary>
    /// 运行设备状态
    /// </summary>
    private static readonly string[] DeviceStatus = { "离线", "待机", "运行中", "报错" };
    private static readonly Color[] DeviceStatusBackColor = { Color.FromArgb(254, 226, 226), Color.FromArgb(254, 243, 199), Color.FromArgb(209, 250, 229) };
    private static readonly Color[] DeviceStatusForeColor = { Color.FromArgb(178, 76, 76), Color.FromArgb(159, 86, 36), Color.FromArgb(9, 156, 109) };



    //A区机器人
    public string ARobStatusText => DeviceStatus[ARobStatus];
    public Color ARobStatusBackColor => DeviceStatusBackColor[ARobStatus];
    public Color ARobStatusForeColor => DeviceStatusForeColor[ARobStatus];

    private int _ARobStatus = 0;
    public int ARobStatus
    {
        get => _ARobStatus;
        set
        {
            _ARobStatus = value;
            OnPropertyChanged(nameof(ARobStatusText));
            OnPropertyChanged(nameof(ARobStatusBackColor));
            OnPropertyChanged(nameof(ARobStatusForeColor));
        }
    }

    //A区机床1
    public string ANc1StatusText => DeviceStatus[ANc1Status];
    public Color ANc1StatusBackColor => DeviceStatusBackColor[ANc1Status];
    public Color ANc1StatusForeColor => DeviceStatusForeColor[ANc1Status];

    private int _ANc1Status = 0;
    public int ANc1Status
    {
        get => _ANc1Status;
        set
        {
            _ANc1Status = value;
            OnPropertyChanged(nameof(ANc1StatusText));
            OnPropertyChanged(nameof(ANc1StatusBackColor));
            OnPropertyChanged(nameof(ANc1StatusForeColor));
        }
    }

    //A区机床2
    public string ANc2StatusText => DeviceStatus[ANc2Status];
    public Color ANc2StatusBackColor => DeviceStatusBackColor[ANc2Status];
    public Color ANc2StatusForeColor => DeviceStatusForeColor[ANc2Status];

    private int _ANc2Status = 0;
    public int ANc2Status
    {
        get => _ANc2Status;
        set
        {
            _ANc2Status = value;
            OnPropertyChanged(nameof(ANc2StatusText));
            OnPropertyChanged(nameof(ANc2StatusBackColor));
            OnPropertyChanged(nameof(ANc2StatusForeColor));
        }
    }

    //B区机器人
    public string BRobStatusText => DeviceStatus[BRobStatus];
    public Color BRobStatusBackColor => DeviceStatusBackColor[BRobStatus];
    public Color BRobStatusForeColor => DeviceStatusForeColor[BRobStatus];

    private int _BRobStatus = 0;
    public int BRobStatus
    {
        get => _BRobStatus;
        set
        {
            _BRobStatus = value;
            OnPropertyChanged(nameof(BRobStatusText));
            OnPropertyChanged(nameof(BRobStatusBackColor));
            OnPropertyChanged(nameof(BRobStatusForeColor));
        }
    }

    //B区机床1
    public string BNc1StatusText => DeviceStatus[BNc1Status];
    public Color BNc1StatusBackColor => DeviceStatusBackColor[BNc1Status];
    public Color BNc1StatusForeColor => DeviceStatusForeColor[BNc1Status];

    private int _BNc1Status = 0;
    public int BNc1Status
    {
        get => _BNc1Status;
        set
        {
            _BNc1Status = value;
            OnPropertyChanged(nameof(BNc1StatusText));
            OnPropertyChanged(nameof(BNc1StatusBackColor));
            OnPropertyChanged(nameof(BNc1StatusForeColor));
        }
    }

    //B区机床2
    public string BNc2StatusText => DeviceStatus[BNc2Status];
    public Color BNc2StatusBackColor => DeviceStatusBackColor[BNc2Status];
    public Color BNc2StatusForeColor => DeviceStatusForeColor[BNc2Status];

    private int _BNc2Status = 0;
    public int BNc2Status
    {
        get => _BNc2Status;
        set
        {
            _BNc2Status = value;
            OnPropertyChanged(nameof(BNc2StatusText));
            OnPropertyChanged(nameof(BNc2StatusBackColor));
            OnPropertyChanged(nameof(BNc2StatusForeColor));
        }
    }

    /// <summary>
    /// 当前产量总数
    /// </summary>
    private int _CurrentCount = 0;
    public int CurrentCount
    {
        get => _CurrentCount;
        set
        {
            _CurrentCount = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 设置目标产量
    /// </summary>
    private int _SetCount = 0;
    public int SetCount
    {
        get => _SetCount;
        set
        {
            _SetCount = value;
            OnPropertyChanged();
        }
    }

    public string CoordTypeText { get; set; } = "未知物料";
    public float CoordX { get; set; } = 1.1f;
    public float CoordY { get; set; } = 2.2f;
    public float CoordZ { get; set; } = 3.3f;
    public float CoordRX { get; set; } = 4.4f;
    public float CoordRY { get; set; } = 5.5f;
    public float CoordRZ { get; set; } = 6.6f;


    private int _CoordType = 0;
    public int CoordType
    {
        get => _CoordType;
        set
        {
            _CoordType = value;
            CoordTypeText = _CoordType == 1 ? "A区物料" : _CoordType == 2 ? "B区物料" :"未知物料";
            OnPropertyChanged(nameof(CoordTypeText));
            OnPropertyChanged(nameof(CoordX));
            OnPropertyChanged(nameof(CoordY));
            OnPropertyChanged(nameof(CoordZ));
            OnPropertyChanged(nameof(CoordX));
            OnPropertyChanged(nameof(CoordRX));
            OnPropertyChanged(nameof(CoordRY));
            OnPropertyChanged(nameof(CoordRZ));


        }
    }


    #endregion

    #endregion



}

