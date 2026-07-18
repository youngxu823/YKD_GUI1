using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YKD_GUI1.My_UserControl
{
    public partial class User_DO : UserControl
    {
        private readonly int labelLeftMargin;
        private readonly int indicatorRightMargin;
        private bool layoutInitialized;
        private int borderSize;

        public User_DO()
        {
            InitializeComponent();
            labelLeftMargin = label1.Left;
            indicatorRightMargin = Width - toggleSwitch1.Right;
            layoutInitialized = true;
            LayoutChildControls();
        }

        /// <summary>
        /// Keeps the signal name on the left and the indicator on the right whenever this
        /// user control is resized in the designer or at run time.
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (layoutInitialized) LayoutChildControls();
        }

        private void LayoutChildControls()
        {
            if (label1 == null || toggleSwitch1 == null) return;

            label1.Left = labelLeftMargin;
            label1.Top = Math.Max(0, (ClientSize.Height - label1.Height) / 2);

            toggleSwitch1.Left = Math.Max(0, ClientSize.Width - indicatorRightMargin - toggleSwitch1.Width);
            toggleSwitch1.Top = Math.Max(0, (ClientSize.Height - toggleSwitch1.Height) / 2);
        }

        /// <summary>
        /// 底部边框宽度（像素）。设置为 0 时不显示边框；边框颜色使用 ForeColor。
        /// </summary>
        [Category("外观")]
        [Description("底部边框宽度（像素）。设置为 0 时不显示边框。")]
        [DefaultValue(0)]
        public int BorderSize
        {
            get { return borderSize; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value", "边框宽度不能小于 0。");
                if (borderSize == value) return;
                borderSize = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (borderSize <= 0) return;

            int actualBorderSize = Math.Min(borderSize, ClientSize.Height);
            using (var borderBrush = new SolidBrush(ForeColor))
            {
                e.Graphics.FillRectangle(
                    borderBrush,
                    0,
                    ClientSize.Height - actualBorderSize,
                    ClientSize.Width,
                    actualBorderSize);
            }
        }


        [Category("信号配置")]
        public string _DO_Index { get; set; } = "01";


        private string _DO_Name = "输出信号名称";
        [Category("信号配置")]
        public string DO_Name
        {
            get => _DO_Name;
            set
            {
                _DO_Name = value;
                label1.Text = $"{_DO_Index}          {_DO_Name}";
            }
        }


        private bool _DO_Status = false;
        [Category("信号配置")]
        public bool DO_Status
        {
            get => _DO_Status;
            set
            {
                _DO_Status = value;
                toggleSwitch1.State = _DO_Status ? ToggleSwitch.SwitchState.On : ToggleSwitch.SwitchState.Off;
            }

        }

    }
}
