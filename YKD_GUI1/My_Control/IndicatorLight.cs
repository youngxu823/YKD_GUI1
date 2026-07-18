using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;


    /// <summary>
    /// 圆形状态指示灯（基类Control、永久透明背景、不可点击切换）
    /// </summary>
    public class IndicatorLight : Control
    {
        #region 状态枚举
        public enum LightState
        {
            /// <summary>关闭/未激活</summary>
            Off,
            /// <summary>开启/激活</summary>
            On
        }
        #endregion

        #region 私有字段
        private LightState _state = LightState.Off;
        private int _lightTextGap = 10;   // 指示灯与文字间距（可调节）
        private int _lightRadiusRatio = 80;// 指示灯占控件高度比例（百分比）

        // ON状态配置
        private Color _onLightColor = Color.Green;
        private string _onText = "ON";
        private Color _onTextColor = Color.Green;

        // OFF状态配置
        private Color _offLightColor = Color.LightGray;
        private string _offText = "OFF";
        private Color _offTextColor = Color.Gray;
        #endregion

        #region 重写隐藏BackColor，永久透明，外部不可修改
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color BackColor
        {
            get => Color.Transparent;
            set { /* 禁止外部修改，空实现 */ }
        }
        #endregion

        #region 设计器公开属性
        [Category("指示灯-状态"), Description("当前指示灯状态：Off关闭 / On开启")]
        public LightState State
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

        [Category("指示灯-布局"), Description("圆形指示灯和右侧文字之间的间距，可自定义调节")]
        public int LightTextGap
        {
            get => _lightTextGap;
            set { _lightTextGap = value; Invalidate(); }
        }

        [Category("指示灯-布局"), Description("圆形指示灯占控件高度的百分比（推荐70~90，范围50-100）")]
        public int LightRadiusRatio
        {
            get => _lightRadiusRatio;
            set
            {
                // 兼容.NET Framework，手动区间限制
                if (value < 50)
                    _lightRadiusRatio = 50;
                else if (value > 100)
                    _lightRadiusRatio = 100;
                else
                    _lightRadiusRatio = value;
                Invalidate();
            }
        }

        #region ON开启样式
        [Category("指示灯-ON开启样式"), Description("开启时圆形灯填充颜色")]
        public Color OnLightColor
        {
            get => _onLightColor;
            set { _onLightColor = value; Invalidate(); }
        }

        [Category("指示灯-ON开启样式"), Description("开启时右侧显示文字")]
        public string OnText
        {
            get => _onText;
            set { _onText = value; Invalidate(); }
        }

        [Category("指示灯-ON开启样式"), Description("开启时右侧文字颜色")]
        public Color OnTextColor
        {
            get => _onTextColor;
            set { _onTextColor = value; Invalidate(); }
        }
        #endregion

        #region OFF关闭样式
        [Category("指示灯-OFF关闭样式"), Description("关闭时圆形灯填充颜色")]
        public Color OffLightColor
        {
            get => _offLightColor;
            set { _offLightColor = value; Invalidate(); }
        }

        [Category("指示灯-OFF关闭样式"), Description("关闭时右侧显示文字")]
        public string OffText
        {
            get => _offText;
            set { _offText = value; Invalidate(); }
        }

        [Category("指示灯-OFF关闭样式"), Description("关闭时右侧文字颜色")]
        public Color OffTextColor
        {
            get => _offTextColor;
            set { _offTextColor = value; Invalidate(); }
        }
        #endregion
        #endregion

        #region 状态变更事件（仅代码修改State时触发）
        [Category("控件事件"), Description("代码修改指示灯状态后触发，无鼠标点击切换")]
        public event EventHandler StateChanged;
        #endregion

        #region 构造函数
        public IndicatorLight()
        {
            // 默认尺寸
            this.Size = new Size(80, 24);
            // 最小尺寸限制，防止缩放过小绘制错乱
            this.MinimumSize = new Size(40, 16);

            // 开启透明背景支持、双缓冲防闪烁
            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.SupportsTransparentBackColor,
                true);
            this.UpdateStyles();

            // 强制背景永久透明
            base.BackColor = Color.Transparent;
        }
        #endregion

        #region 控件缩放自动重绘
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
        #endregion

        #region 核心绘制逻辑
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int ctrlW = this.Width;
            int ctrlH = this.Height;

            // 根据比例计算圆形灯直径
            int lightDiameter = ctrlH * _lightRadiusRatio / 100;
            lightDiameter = Math.Max(lightDiameter, 6);

            // 获取当前状态对应的颜色与文字
            Color lightFill, textColor;
            string showText;
            if (_state == LightState.On)
            {
                lightFill = _onLightColor;
                showText = _onText;
                textColor = _onTextColor;
            }
            else
            {
                lightFill = _offLightColor;
                showText = _offText;
                textColor = _offTextColor;
            }

            // 1. 绘制左侧圆形指示灯（垂直居中）
            Rectangle lightRect = new Rectangle(
                0,
                (ctrlH - lightDiameter) / 2,
                lightDiameter,
                lightDiameter);
            using (SolidBrush lightBrush = new SolidBrush(lightFill))
            {
                g.FillEllipse(lightBrush, lightRect);
            }

            // 2. 绘制右侧状态文字（垂直居中）
            SizeF textSize = g.MeasureString(showText, this.Font);
            float textX = lightRect.Right + _lightTextGap;
            float textY = (ctrlH - textSize.Height) / 2;
            using (SolidBrush textBrush = new SolidBrush(textColor))
            {
                g.DrawString(showText, this.Font, textBrush, textX, textY);
            }
        }
        #endregion
    }
