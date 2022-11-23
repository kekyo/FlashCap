namespace FlashCap.WindowsForms
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.deviceLabel = new System.Windows.Forms.Label();
            this.characteristicsLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // deviceLabel
            // 
            this.deviceLabel.AutoSize = true;
            this.deviceLabel.BackColor = System.Drawing.Color.Transparent;
            this.deviceLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.deviceLabel.ForeColor = System.Drawing.Color.Yellow;
            this.deviceLabel.Location = new System.Drawing.Point(0, 0);
            this.deviceLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.deviceLabel.Name = "deviceLabel";
            this.deviceLabel.Size = new System.Drawing.Size(65, 13);
            this.deviceLabel.TabIndex = 0;
            this.deviceLabel.Text = "deviceLabel";
            // 
            // characteristicsLabel
            // 
            this.characteristicsLabel.AutoSize = true;
            this.characteristicsLabel.BackColor = System.Drawing.Color.Transparent;
            this.characteristicsLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.characteristicsLabel.ForeColor = System.Drawing.Color.Yellow;
            this.characteristicsLabel.Location = new System.Drawing.Point(0, 13);
            this.characteristicsLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.characteristicsLabel.Name = "characteristicsLabel";
            this.characteristicsLabel.Size = new System.Drawing.Size(101, 13);
            this.characteristicsLabel.TabIndex = 1;
            this.characteristicsLabel.Text = "characteristicsLabel";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(800, 488);
            this.Controls.Add(this.characteristicsLabel);
            this.Controls.Add(this.deviceLabel);
            this.DoubleBuffered = true;
            this.Name = "MainForm";
            this.Text = "FlashCap.WindowsForms";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Label deviceLabel;
        private System.Windows.Forms.Label characteristicsLabel;
    }
}