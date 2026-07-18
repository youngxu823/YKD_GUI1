using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;


    /// <summary>
    /// 标准胶囊开关（滑块固定2px内边距、锁定横纵比例、滑块不溢出轨道）
    /// </summary>
    public class ToggleSwitch : Control
    {
        #region 状态枚举
        public enum SwitchState
        {
            /// <summary>关闭</summary>
            Off,
            /// <summary>开启</summary>
            On
        }
        #endregion

        #region 私有常量&字段
        // 固定滑块距离胶囊左右边缘间隙，永久2像素，不对外暴露
        private const int SliderMargin = 2;

        private SwitchState _state = SwitchState.Off;

        // 轨道颜色
        private Color _offTrackColor = Color.FromArgb(210, 230, 250);
        private Color _onTrackColor = Color.FromArgb(230, 120, 30);
        private Color _sliderColor = Color.White;

        // 右侧文字
        private string _offText = "关闭";
        private string _onText = "开启";
        private Color _offTextColor = Color.Black;
        private Color _onTextColor = Color.Black;
        private int _textGap = 12;
    #endregion



    #region 设计器公开属性
    [Category("配置")]
        public SwitchState State
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    _state = value;
                    Invalidate();
                    StateChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        [Category("配置")]
        public Color OffTrackColor
        {
            get => _offTrackColor;
            set { _offTrackColor = value; Invalidate(); }
        }

      [Category("配置")]
        public Color OnTrackColor
        {
            get => _onTrackColor;
            set { _onTrackColor = value; Invalidate(); }
        }

        [Category("配置")]
        public Color SliderColor
        {
            get => _sliderColor;
            set { _sliderColor = value; Invalidate(); }
        }

        [Category("配置")]
        public string OffText
        {
            get => _offText;
            set { _offText = value; Invalidate(); }
        }

       [Category("配置")]
        public string OnText
        {
            get => _onText;
            set { _onText = value; Invalidate(); }
        }

        [Category("配置")]
        [Description("开关关闭时右侧文字的颜色")]
        public Color OffTextColor
        {
            get => _offTextColor;
            set { _offTextColor = value; Invalidate(); }
        }

        [Category("配置")]
        [Description("开关开启时右侧文字的颜色")]
        public Color OnTextColor
        {
            get => _onTextColor;
            set { _onTextColor = value; Invalidate(); }
        }

        [Category("配置")]
        public int TextGap
        {
            get => _textGap;
            set { _textGap = value; Invalidate(); }
        }
        #endregion

        #region 状态变更事件
        [Category("控件事件"), Description("点击切换开关后触发")]
        public event EventHandler StateChanged;
        #endregion

        #region 构造函数
        public ToggleSwitch()
        {
            // 默认尺寸
            this.Size = new Size(120, 30);
            // 最小尺寸限制，防止拉伸过小绘制异常
            this.MinimumSize = new Size(60, 20);

            // 开启双缓冲防闪烁
            this.DoubleBuffered = true;
            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.SupportsTransparentBackColor,
                true);
            this.BackColor = Color.Transparent;
            this.UpdateStyles();
        }
        #endregion

        #region 鼠标点击切换状态
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            State = _state == SwitchState.Off ? SwitchState.On : SwitchState.Off;
        }
        #endregion

        #region 尺寸变化锁定比例（核心：保证胶囊不会变形）
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            // 强制轨道高度 = 控件完整高度，胶囊圆角永远等于高度，两端标准半圆
            Invalidate();
        }
        #endregion

        #region 核心绘制逻辑（滑块严格卡在胶囊内部，左右留2px）
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int ctrlHeight = this.Height;
            int ctrlWidth = this.Width;

            // 1. 计算文字占用宽度，预留文字区域
            string displayText = _state == SwitchState.On ? _onText : _offText;
            int textWidth = TextRenderer.MeasureText(displayText, this.Font).Width;

            // 胶囊轨道高度 = 控件完整高度，锁定纵向尺寸，保证胶囊两端半圆正常
            int trackHeight = ctrlHeight;
            // 轨道总宽度 = 总控件宽度 - 文字宽度 - 文字间距
            int trackWidth = ctrlWidth - textWidth - _textGap;
            trackWidth = Math.Max(trackWidth, trackHeight); // 轨道最小宽度至少等于高度，避免胶囊压扁

            // 胶囊轨道矩形：垂直填满控件，水平靠左
            Rectangle trackRect = new Rectangle(0, 0, trackWidth, trackHeight);

            // 滑块直径 = 轨道高度 - 左右各2像素间隙
            int sliderDiameter = trackHeight - SliderMargin * 2;
            sliderDiameter = Math.Max(sliderDiameter, 4); // 滑块最小尺寸保护

            // 计算滑块X坐标，左右固定预留2像素
            int sliderX;
            if (_state == SwitchState.Off)
            {
                // OFF：滑块靠左，左边距2
                sliderX = trackRect.X + SliderMargin;
            }
            else
            {
                // ON：滑块靠右，右边距2，不会超出胶囊
                sliderX = trackRect.Right - sliderDiameter - SliderMargin;
            }

            // 滑块矩形，垂直居中
            Rectangle sliderRect = new Rectangle(
                sliderX,
                trackRect.Y + SliderMargin,
                sliderDiameter,
                sliderDiameter
            );

            // 绘制纯色胶囊轨道背景
            Color trackFillColor = _state == SwitchState.On ? _onTrackColor : _offTrackColor;
            using (GraphicsPath capsulePath = GetCapsulePath(trackRect))
            using (SolidBrush trackBrush = new SolidBrush(trackFillColor))
            {
                g.FillPath(trackBrush, capsulePath);
            }

            // 绘制白色滑块（后绘制，显示在轨道上层）
            using (SolidBrush sliderBrush = new SolidBrush(_sliderColor))
            {
                g.FillEllipse(sliderBrush, sliderRect);
            }

            // 绘制右侧文字，垂直居中
            SizeF textSize = g.MeasureString(displayText, this.Font);
            float textDrawX = trackRect.Right + _textGap;
            float textDrawY = (ctrlHeight - textSize.Height) / 2;
            Color textColor = _state == SwitchState.On ? _onTextColor : _offTextColor;
            using (SolidBrush textBrush = new SolidBrush(textColor))
            {
                g.DrawString(displayText, this.Font, textBrush, textDrawX, textDrawY);
            }
        }
        #endregion

        #region 生成标准胶囊圆角路径（两端完美半圆）
        private GraphicsPath GetCapsulePath(Rectangle rect)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = rect.Height;

            // 左上圆弧
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            // 右上圆弧
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            // 右下圆弧
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            // 左下圆弧
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);

            path.CloseAllFigures();
            return path;
        }
        #endregion
    }
