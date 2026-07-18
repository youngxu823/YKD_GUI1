namespace YKD_GUI1.My_UserControl
{
    partial class User_DI
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.indicatorLight1 = new IndicatorLight();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(128)))), ((int)(((byte)(227)))));
            this.label1.Location = new System.Drawing.Point(6, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(160, 24);
            this.label1.TabIndex = 31;
            this.label1.Text = "01    输入信号名称";
            // 
            // indicatorLight1
            // 
            this.indicatorLight1.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F);
            this.indicatorLight1.LightRadiusRatio = 80;
            this.indicatorLight1.LightTextGap = 30;
            this.indicatorLight1.Location = new System.Drawing.Point(247, 3);
            this.indicatorLight1.MinimumSize = new System.Drawing.Size(40, 16);
            this.indicatorLight1.Name = "indicatorLight1";
            this.indicatorLight1.OffLightColor = System.Drawing.Color.LightGray;
            this.indicatorLight1.OffText = "OFF";
            this.indicatorLight1.OffTextColor = System.Drawing.Color.DarkGray;
            this.indicatorLight1.OnLightColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(162)))), ((int)(((byte)(113)))));
            this.indicatorLight1.OnText = "ON";
            this.indicatorLight1.OnTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(162)))), ((int)(((byte)(113)))));
            this.indicatorLight1.Size = new System.Drawing.Size(98, 33);
            this.indicatorLight1.State = IndicatorLight.LightState.Off;
            this.indicatorLight1.TabIndex = 32;
            // 
            // User_DI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.indicatorLight1);
            this.Controls.Add(this.label1);
            this.Name = "User_DI";
            this.Size = new System.Drawing.Size(348, 40);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private IndicatorLight indicatorLight1;
        private System.Windows.Forms.Label label1;
    }
}
