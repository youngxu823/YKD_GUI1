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
    public partial class User_DI : UserControl
    {
        private readonly int labelLeftMargin;
        private readonly int indicatorRightMargin;
        private bool layoutInitialized;
        private int borderSize;

        public User_DI()
        {
            InitializeComponent();
            labelLeftMargin = label1.Left;
            indicatorRightMargin = Width - indicatorLight1.Right;
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
            if (label1 == null || indicatorLight1 == null) return;

            label1.Left = labelLeftMargin;
            label1.Top = Math.Max(0, (ClientSize.Height - label1.Height) / 2);

            indicatorLight1.Left = Math.Max(0, ClientSize.Width - indicatorRightMargin - indicatorLight1.Width);
            indicatorLight1.Top = Math.Max(0, (ClientSize.Height - indicatorLight1.Height) / 2);
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
        public string _DI_Index { get; set; }  = "01";

        private string _DI_Name = "输入信号名称";
        [Category("信号配置")]
        public string DI_Name
        {
            get => _DI_Name;
            set 
            { 
                _DI_Name = value;
                label1.Text = $"{_DI_Index}          {_DI_Name}";
            }
        }


        private bool _DI_Status = false;
        [Category("信号配置")]
        public bool DI_Status
        {
            get => _DI_Status;
            set
            {
                _DI_Status = value;
                indicatorLight1.State = _DI_Status ? IndicatorLight.LightState.On: IndicatorLight.LightState.Off;
            }

        }

    }
}
