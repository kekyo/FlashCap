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
            this.deviceLabel.Name = "deviceLabel";
            this.deviceLabel.Size = new System.Drawing.Size(113, 21);
            this.deviceLabel.TabIndex = 0;
            this.deviceLabel.Text = "deviceLabel";
            // 
            // characteristicsLabel
            // 
            this.characteristicsLabel.AutoSize = true;
            this.characteristicsLabel.BackColor = System.Drawing.Color.Transparent;
            this.characteristicsLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.characteristicsLabel.ForeColor = System.Drawing.Color.Yellow;
            this.characteristicsLabel.Location = new System.Drawing.Point(0, 21);
            this.characteristicsLabel.Name = "characteristicsLabel";
            this.characteristicsLabel.Size = new System.Drawing.Size(187, 21);
            this.characteristicsLabel.TabIndex = 1;
            this.characteristicsLabel.Text = "characteristicsLabel";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(1467, 788);
            this.Controls.Add(this.characteristicsLabel);
            this.Controls.Add(this.deviceLabel);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.Name = "MainForm";
            this.Text = "FlashCap.WindowsForms";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Label deviceLabel;
        private System.Windows.Forms.Label characteristicsLabel;
    }
}