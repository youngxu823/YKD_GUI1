namespace YKD_GUI1.My_UserControl
{
    partial class User_DO
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
            this.toggleSwitch1 = new ToggleSwitch();
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
            this.label1.Text = "01    输出信号名称";
            // 
            // toggleSwitch1
            // 
            this.toggleSwitch1.BackColor = System.Drawing.Color.Transparent;
            this.toggleSwitch1.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F);
            this.toggleSwitch1.Location = new System.Drawing.Point(236, 4);
            this.toggleSwitch1.MinimumSize = new System.Drawing.Size(60, 20);
            this.toggleSwitch1.Name = "toggleSwitch1";
            this.toggleSwitch1.OffText = "OFF";
            this.toggleSwitch1.OffTextColor = System.Drawing.Color.DarkGray;
            this.toggleSwitch1.OffTrackColor = System.Drawing.Color.LightGray;
            this.toggleSwitch1.OnText = "ON";
            this.toggleSwitch1.OnTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(162)))), ((int)(((byte)(113)))));
            this.toggleSwitch1.OnTrackColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(162)))), ((int)(((byte)(113)))));
            this.toggleSwitch1.Size = new System.Drawing.Size(112, 30);
            this.toggleSwitch1.SliderColor = System.Drawing.Color.White;
            this.toggleSwitch1.State = ToggleSwitch.SwitchState.Off;
            this.toggleSwitch1.TabIndex = 32;
            this.toggleSwitch1.Text = "toggleSwitch1";
            this.toggleSwitch1.TextGap = 12;
            // 
            // User_DO
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.toggleSwitch1);
            this.Controls.Add(this.label1);
            this.Name = "User_DO";
            this.Size = new System.Drawing.Size(348, 40);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private ToggleSwitch toggleSwitch1;
    }
}
