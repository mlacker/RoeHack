namespace RoeHack.Forms
{
    partial class InjectorForm
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

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnInjectSwitch = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnInjectSwitch
            // 
            this.btnInjectSwitch.Location = new System.Drawing.Point(62, 30);
            this.btnInjectSwitch.Name = "btnInjectSwitch";
            this.btnInjectSwitch.Size = new System.Drawing.Size(75, 23);
            this.btnInjectSwitch.TabIndex = 0;
            this.btnInjectSwitch.Text = "开启";
            this.btnInjectSwitch.Click += new System.EventHandler(this.btnInjectSwitch_Click);
            // 
            // InjectorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(194, 95);
            this.Controls.Add(this.btnInjectSwitch);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InjectorForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "RoeHack";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.InjectorForm_FormClosing);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.InjectorForm_KeyUp);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnInjectSwitch;
    }
}

