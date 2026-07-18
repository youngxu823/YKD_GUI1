using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ModbusLibrary;
using EasyModbus;
using System.Diagnostics.Eventing.Reader;
using YKD_GUI1.Models;
using YKD_GUI1.My_UserControl;

namespace YKD_GUI1
{
    public partial class Form1 : Form
    {

        //通讯地址设置
        private int _sysStartAddress = 200;


        //IO变量名称
        string[] _diName = {
    "急停按钮",
    "启动按钮",
    "停止按钮",
    "复位按钮",
    "左皮带前光电",
    "左皮带视觉处光电",
    "左皮带后光电",
    "右皮带前光电",
    "右皮带放料检测光电#1",
    "右皮带放料检测光电#2",
    "#1机器人#1夹爪原点开关",
    "#1机器人#1夹爪到位开关",
    "#1机器人#2夹爪原点开关",
    "#1机器人#2夹爪到位开关",
    "#1机器人允许抓取输送带物料",
    "#1机器人允许放置输送带物料",
    "#1机器人允许机床1物料换料",
    "#1机器人允许机床2物料换料",
    "#2机器人#1夹爪原点开关",
    "#2机器人#1夹爪到位开关",
    "#2机器人#2夹爪原点开关",
    "#2机器人#2夹爪到位开关",
    "#2机器人允许抓取输送带物料",
    "#2机器人允许放置输送带物料",
    "#2机器人允许机床1物料换料",
    "#2机器人允许机床2物料换料"
};
        string[] _doName = {
    "三色灯-红",
    "三色灯-绿",
    "三色灯-黄",
    "左皮带运行",
    "右皮带运行",
    "#1机器人#1夹爪",
    "#1机器人#2夹爪",
    "#2机器人#1夹爪",
    "#2机器人#2夹爪",
    "#1机器人吹气",
    "#2机器人吹气"
};



        private readonly GUI_ViewModel gUI = GUI_ViewModel.Instance;

        //private ModbusConnectionManager _modbusManager;
        private CancellationTokenSource _statusMonitorCts;
        private Task _statusMonitorTask;
        private const int StatusMonitorIntervalMilliseconds = 20;

        private ModbusManager _modbus = null;

        private bool Isconnection = false;


        public Form1()
        {
            InitializeComponent();
            EventArg();
            DataBinding();

            gUI.SetCount = 10;
        }

        /// <summary>
        /// 业务数据绑定处理
        /// </summary>
        private void DataBinding()
        {

            this.DataBindings.Add("Text", gUI, "SysConStatus");

            tabControl1.DataBindings.Add("SelectedIndex", gUI, "btn_Win");
            btn_Win1.DataBindings.Add("BackColor", gUI, "btn_Win1BackColor");
            btn_Win2.DataBindings.Add("BackColor", gUI, "btn_Win2BackColor");
            btn_Win3.DataBindings.Add("BackColor", gUI, "btn_Win3BackColor");

            lb_SysStatus.DataBindings.Add("Text", gUI, "SysStatusText");
            lb_SysStatus.DataBindings.Add("BackColor", gUI, "SysStatusBackColor");
            lb_SysStatus.DataBindings.Add("ForeColor", gUI, "SysStatusForeColor");

            lb_ARobStatus.DataBindings.Add("Text", gUI, "ARobStatusText");
            lb_ARobStatus.DataBindings.Add("BackColor", gUI, "ARobStatusBackColor");
            lb_ARobStatus.DataBindings.Add("ForeColor", gUI, "ARobStatusForeColor");

            lb_ANc1Status.DataBindings.Add("Text", gUI, "ANc1StatusText");
            lb_ANc1Status.DataBindings.Add("BackColor", gUI, "ANc1StatusBackColor");
            lb_ANc1Status.DataBindings.Add("ForeColor", gUI, "ANc1StatusForeColor");

            lb_ANc2Status.DataBindings.Add("Text", gUI, "ANc2StatusText");
            lb_ANc2Status.DataBindings.Add("BackColor", gUI, "ANc2StatusBackColor");
            lb_ANc2Status.DataBindings.Add("ForeColor", gUI, "ANc2StatusForeColor");

            lb_BRobStatus.DataBindings.Add("Text", gUI, "BRobStatusText");
            lb_BRobStatus.DataBindings.Add("BackColor", gUI, "BRobStatusBackColor");
            lb_BRobStatus.DataBindings.Add("ForeColor", gUI, "BRobStatusForeColor");

            lb_BNc1Status.DataBindings.Add("Text", gUI, "BNc1StatusText");
            lb_BNc1Status.DataBindings.Add("BackColor", gUI, "BNc1StatusBackColor");
            lb_BNc1Status.DataBindings.Add("ForeColor", gUI, "BNc1StatusForeColor");

            lb_BNc2Status.DataBindings.Add("Text", gUI, "BNc2StatusText");
            lb_BNc2Status.DataBindings.Add("BackColor", gUI, "BNc2StatusBackColor");
            lb_BNc2Status.DataBindings.Add("ForeColor", gUI, "BNc2StatusForeColor");


            lb_CurrentCount.DataBindings.Add("Text", gUI, "CurrentCount");
            lb_SetCount.DataBindings.Add("Text", gUI, "SetCount");


            lb_CoordX.DataBindings.Add("Text", gUI, "CoordX");
            lb_CoordY.DataBindings.Add("Text", gUI, "CoordY");
            lb_CoordZ.DataBindings.Add("Text", gUI, "CoordZ");
            lb_CoordRX.DataBindings.Add("Text", gUI, "CoordRX");
            lb_CoordRY.DataBindings.Add("Text", gUI, "CoordRY");
            lb_CoordRZ.DataBindings.Add("Text", gUI, "CoordRZ");
            lb_CoordType.DataBindings.Add("Text", gUI, "CoordTypeText");
        }

        /// <summary>
        /// 业务事件处理
        /// </summary>
        private void EventArg()
        {
            btn_Win1.Click += btn_Win1_Click;
            btn_Win2.Click += btn_Win2_Click;
            btn_Win3.Click += btn_Win3_Click;

            Load += Form1_Load;
            FormClosing += MainForm_FormClosing;

            btn_Clear.Click += Btn_Clear_Click;
            lb_SetCount.Click += Lb_SetCount_Click;

            btn_SysStart.MouseDown += Btn_Start_MouseDown;
            btn_SysStop.MouseDown += Btn_Stop_MouseDown;
            btn_SysReset.MouseDown += Btn_Reset_MouseDown;

            btn_SysStart.MouseUp += Btn_Start_MouseUp;
            btn_SysStop.MouseUp += Btn_Stop_MouseUp;
            btn_SysReset.MouseUp += Btn_Reset_MouseUp;

            btn_ARobStart.MouseDown += Btn_ARobStart_MouseDown;
            btn_ARobStop.MouseDown += Btn_ARobStop_MouseDown;
            btn_ARobReset.MouseDown += Btn_ARobReset_MouseDown;

            btn_ARobStart.MouseUp += Btn_ARobStart_MouseUp;
            btn_ARobStop.MouseUp += Btn_ARobStop_MouseUp;
            btn_ARobReset.MouseUp += Btn_ARobReset_MouseUp;

            btn_BRobStart.MouseDown += Btn_BRobStart_MouseDown;
            btn_BRobStop.MouseDown += Btn_BRobStop_MouseDown;
            btn_BRobReset.MouseDown += Btn_BRobReset_MouseDown;

            btn_BRobStart.MouseUp += Btn_BRobStart_MouseUp;
            btn_BRobStop.MouseUp += Btn_BRobStop_MouseUp;
            btn_BRobReset.MouseUp += Btn_BRobReset_MouseUp;


        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tabControl1.Size = tabControl1.Size + new Size(0, 20);
            LoadInputSignalControls();
            LoadOutputSignalControls();

        }


        private void LoadInputSignalControls()
        {

            int Count = _diName.Length;
            Size controlSize = new Size(550, 40);


            panel_DIList.AutoScroll = true;
            panel_DIList.AutoScrollMinSize = new Size(0, Count * controlSize.Height);
            panel_DIList.VerticalScroll.SmallChange = 40; // 每格控件高度

            panel_DIList.SuspendLayout();
            try
            {
                // Remove the design-time placeholder before adding the runtime controls.
                while (panel_DIList.Controls.Count > 0)
                {
                    Control control = panel_DIList.Controls[0];
                    panel_DIList.Controls.Remove(control);
                    control.Dispose();
                }

                for (int index = 0; index < Count; index++)
                {
                    var inputControl = new User_DI
                    {
                        Name = "userDI" + (index + 1),
                        Size = controlSize,
                        Location = new Point(10, index * controlSize.Height),
                        _DI_Index = (index + 1).ToString("D2"),
                        DI_Name = _diName[index],
                        DI_Status = false,
                        BackColor = Color.Transparent,
                        ForeColor = Color.LightGray,
                        BorderSize = 1
                    };

                    panel_DIList.Controls.Add(inputControl);
                }
            }
            finally
            {
                panel_DIList.ResumeLayout();
            }
        }

        private void LoadOutputSignalControls()
        {
             int Count = _doName.Length;
            Size controlSize = new Size(550, 40);


            panel_DOList.AutoScroll = true;
            panel_DOList.AutoScrollMinSize = new Size(0, Count * controlSize.Height);
            panel_DOList.VerticalScroll.SmallChange = 40; // 每格控件高度

            panel_DOList.SuspendLayout();
            try
            {
                // Remove the design-time placeholder before adding the runtime controls.
                while (panel_DOList.Controls.Count > 0)
                {
                    Control control = panel_DOList.Controls[0];
                    panel_DOList.Controls.Remove(control);
                    control.Dispose();
                }

                for (int index = 0; index < Count; index++)
                {
                    var inputControl = new User_DO
                    {
                        Name = "userDO" + (index + 1),
                        Size = controlSize,
                        Location = new Point(10, index * controlSize.Height),
                        _DO_Index = (index + 1).ToString("D2"),
                        DO_Name = _doName[index],
                        DO_Status = false,
                        BackColor = Color.Transparent,
                        ForeColor = Color.LightGray,
                        BorderSize = 1
                    };

                    panel_DOList.Controls.Add(inputControl);
                }
            }
            finally
            {
                panel_DOList.ResumeLayout();
            }
        }




        private void Lb_SetCount_Click(object sender, EventArgs e)
        {
            // 弹出输入框，接收返回值
            var result = FormTool.ShowNumInputPopup(this, gUI.SetCount);

            // 判断确认后写入寄存器
            if (result.IsConfirm)
            {
                _modbus?.WriteSingleRegister(2001, result.InputNum);
            }



        }

        private void btn_Win1_Click(object sender, EventArgs e)
        {
            gUI.btn_Win = 0;
        }
        private void btn_Win2_Click(object sender, EventArgs e)
        {
            gUI.btn_Win = 1;
        }
        private void btn_Win3_Click(object sender, EventArgs e)
        {
            gUI.btn_Win = 2;
        }

        private void Btn_Clear_Click(object sender, EventArgs e)
        {
            btn_Clear.Enabled = false;
            try
            {
                _modbus?.WriteSingleRegister(2000, 0);
                lb_Status.Text = "启动命令1已发送";
            }
            catch (Exception ex)
            {
                lb_Status.Text = "启动失败: " + ex.Message;
            }
            finally
            {
                btn_Clear.Enabled = true;
            }
        }

        private void Btn_Reset_MouseDown(object sender, EventArgs e)
        {
            _modbus?.WriteSingleCoil(202, true);
        }
        private void Btn_Reset_MouseUp(object sender, EventArgs e)
        {
            _modbus?.WriteSingleCoil(202, false);
        }


        private void Btn_Stop_MouseDown(object sender, EventArgs e)
        {
            _modbus?.WriteSingleCoil(201, true);
        }
        private void Btn_Stop_MouseUp(object sender, EventArgs e)
        {
            _modbus?.WriteSingleCoil(201, false);
        }

        private void Btn_Start_MouseDown(object sender, EventArgs e)
        {
            _modbus?.WriteSingleCoil(200, true);
        }
        private void Btn_Start_MouseUp(object sender, EventArgs e)
        {
            _modbus?.WriteSingleCoil(200, false);
        }

        private void Btn_ARobStart_MouseDown(object sender, EventArgs e)
        {
            _modbus?.WriteSingleCoil(203, true);
        }
        private void Btn_ARobStart_MouseUp(object sender, EventArgs e)
        {
            _modbus?.WriteSingleCoil(203, false);
        }
        private void Btn_ARobStop_MouseDown(object sender, EventArgs e)
        {
            _modbus?.WriteSingleCoil(204, true);
        }
        private void Btn_ARobStop_MouseUp(object sender, EventArgs e)
        {
            _modbus?.WriteSingleCoil(204, false);
        }
        private void Btn_ARobReset_MouseDown(object sender, EventArgs e)
        {
            _modbus?.WriteSingleCoil(205, true);
        }
        private void Btn_ARobReset_MouseUp(object sender, EventArgs e)
        {
            _modbus?.WriteSingleCoil(205, false);
        }
        private void Btn_BRobStart_MouseDown(object sender, EventArgs e)
        {
            _modbus?.WriteSingleCoil(206, true);
        }
        private void Btn_BRobStart_MouseUp(object sender, EventArgs e)
        {
            _modbus?.WriteSingleCoil(206, false);
        }
        private void Btn_BRobStop_MouseDown(object sender, EventArgs e)
        {
            _modbus?.WriteSingleCoil(207, true);
        }
        private void Btn_BRobStop_MouseUp(object sender, EventArgs e)
        {
            _modbus?.WriteSingleCoil(207, false);
        }
        private void Btn_BRobReset_MouseDown(object sender, EventArgs e)
        {
            _modbus?.WriteSingleCoil(208, true);
        }
        private void Btn_BRobReset_MouseUp(object sender, EventArgs e)
        {
            _modbus?.WriteSingleCoil(208, false);
        }


        private void btn_Connect_Click(object sender, EventArgs e)
        {
            string ipPort = tb_IP.Text.Trim();

            string[] endpoint = ipPort.Split(':');
            _modbus = new ModbusManager(endpoint[0], Int32.Parse(endpoint[1]));

            _modbus.ConnectionStateChanged += Modbus_ConnectionStateChanged;

            _modbus.Start();
            StartStatusMonitoring();





        }

        private async void btn_Disconnect_Click(object sender, EventArgs e)
        {
            await StopStatusMonitoringAsync();
            _modbus?.Stop();
            DisconnectedClear();

        }

        /// <summary>Starts non-blocking polling of Modbus holding register 0.</summary>
        private void StartStatusMonitoring()
        {
            _statusMonitorCts = new CancellationTokenSource();
            _statusMonitorTask = Task.Run(() => MonitorStatus(_statusMonitorCts.Token));

        }

        private async Task StopStatusMonitoringAsync()
        {
            CancellationTokenSource cts = _statusMonitorCts;
            Task monitorTask = _statusMonitorTask;
            _statusMonitorCts = null;
            _statusMonitorTask = null;

            if (cts == null) return;
            cts.Cancel();
            try
            {
                if (monitorTask != null) await monitorTask;
            }
            catch (OperationCanceledException) { }
            finally { cts.Dispose(); }
        }

        /// <summary>Updates the view model whenever the monitored register changes.</summary>
        private void MonitorStatus(CancellationToken cancellationToken)
        {

            while (!cancellationToken.IsCancellationRequested)
            {
                if (Isconnection)
                {
                    BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            gUI.CurrentCount = _modbus.Read_INT(0);
                            gUI.SetCount = _modbus.Read_INT(1);
                            gUI.CoordType = _modbus.Read_INT(2);
                            gUI.ARobStatus = _modbus.Read_INT(3);
                            gUI.ANc1Status = _modbus.Read_INT(4);
                            gUI.ANc2Status = _modbus.Read_INT(5);
                            gUI.BRobStatus = _modbus.Read_INT(6);
                            gUI.BNc1Status = _modbus.Read_INT(7);
                            gUI.BNc2Status = _modbus.Read_INT(8);
                            gUI.CoordX = _modbus.Read_Float(0);
                            gUI.CoordY = _modbus.Read_Float(1);
                            gUI.CoordZ = _modbus.Read_Float(2);
                            gUI.CoordRX = _modbus.Read_Float(3);
                            gUI.CoordRY = _modbus.Read_Float(4);
                            gUI.CoordRZ = _modbus.Read_Float(5);
                        }
                        catch
                        {


                        }

                    }));
                }
                else
                {
                    DisconnectedClear();
                }
                cancellationToken.WaitHandle.WaitOne(1000);
            }
        }


        private void Modbus_ConnectionStateChanged(ConnectionState connected)
        {
            BeginInvoke(new Action(() =>
            {
                gUI.SysConStatus = connected.ToString();
                Isconnection = ConnectionState.Connected == connected;

            }));
        }

        private void DisconnectedClear()
        {

            BeginInvoke(new Action(() =>
            {
                gUI.CurrentCount = 0;
                gUI.ARobStatus = 0;
                gUI.ANc1Status = 0;
                gUI.ANc2Status = 0;
                gUI.BRobStatus = 0;
                gUI.BNc1Status = 0;
                gUI.BNc2Status = 0;
                gUI.CoordX = 0;
                gUI.CoordY = 0;
                gUI.CoordZ = 0;
                gUI.CoordRX = 0;
                gUI.CoordRY = 0;
                gUI.CoordRZ = 0;
            }));
        }


        // 窗体关闭时释放资源
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _statusMonitorCts?.Cancel();
            _modbus?.Stop();
        }

        private void user_DI2_Load(object sender, EventArgs e)
        {

        }
    }
}
