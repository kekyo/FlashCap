namespace FlashCap.WindowsForms
{
    partial class CaptureDeviceControlsForm
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
            this.controlsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // controlsPanel
            // 
            this.controlsPanel.AutoScroll = true;
            this.controlsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.controlsPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.controlsPanel.Location = new System.Drawing.Point(0, 0);
            this.controlsPanel.Name = "controlsPanel";
            this.controlsPanel.Padding = new System.Windows.Forms.Padding(8);
            this.controlsPanel.Size = new System.Drawing.Size(320, 560);
            this.controlsPanel.TabIndex = 0;
            this.controlsPanel.WrapContents = false;
            // 
            // CaptureDeviceControlsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(320, 560);
            this.Controls.Add(this.controlsPanel);
            this.Name = "CaptureDeviceControlsForm";
            this.Text = "Camera controls";
            this.Load += new System.EventHandler(this.CaptureDeviceControlsForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel controlsPanel;
    }
}
