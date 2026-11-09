namespace FlashCap.WindowsForms
{
    partial class CaptureDeviceControlView
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

        #region Component Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.layoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.nameLabel = new System.Windows.Forms.Label();
            this.autoCheckBox = new System.Windows.Forms.CheckBox();
            this.valueSlider = new System.Windows.Forms.TrackBar();
            this.onCheckBox = new System.Windows.Forms.CheckBox();
            this.choiceComboBox = new System.Windows.Forms.ComboBox();
            this.layoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.valueSlider)).BeginInit();
            this.SuspendLayout();
            // 
            // layoutPanel
            // 
            this.layoutPanel.AutoSize = true;
            this.layoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.layoutPanel.ColumnCount = 2;
            this.layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.layoutPanel.Controls.Add(this.nameLabel, 0, 0);
            this.layoutPanel.Controls.Add(this.autoCheckBox, 1, 0);
            this.layoutPanel.Controls.Add(this.valueSlider, 0, 1);
            this.layoutPanel.Controls.Add(this.onCheckBox, 0, 2);
            this.layoutPanel.Controls.Add(this.choiceComboBox, 0, 3);
            this.layoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.layoutPanel.Location = new System.Drawing.Point(0, 0);
            this.layoutPanel.Name = "layoutPanel";
            this.layoutPanel.RowCount = 4;
            this.layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layoutPanel.Size = new System.Drawing.Size(280, 70);
            this.layoutPanel.TabIndex = 0;
            // 
            // nameLabel
            // 
            this.nameLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nameLabel.AutoSize = true;
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.TabIndex = 0;
            this.nameLabel.Text = "nameLabel";
            // 
            // autoCheckBox
            // 
            this.autoCheckBox.AutoSize = true;
            this.autoCheckBox.Name = "autoCheckBox";
            this.autoCheckBox.TabIndex = 1;
            this.autoCheckBox.Text = "Auto";
            this.autoCheckBox.UseVisualStyleBackColor = true;
            this.autoCheckBox.CheckedChanged += new System.EventHandler(this.autoCheckBox_CheckedChanged);
            // 
            // valueSlider
            // 
            this.layoutPanel.SetColumnSpan(this.valueSlider, 2);
            this.valueSlider.Dock = System.Windows.Forms.DockStyle.Fill;
            this.valueSlider.Name = "valueSlider";
            this.valueSlider.TabIndex = 2;
            this.valueSlider.TickStyle = System.Windows.Forms.TickStyle.None;
            this.valueSlider.ValueChanged += new System.EventHandler(this.valueSlider_ValueChanged);
            // 
            // onCheckBox
            // 
            this.onCheckBox.AutoSize = true;
            this.layoutPanel.SetColumnSpan(this.onCheckBox, 2);
            this.onCheckBox.Name = "onCheckBox";
            this.onCheckBox.TabIndex = 3;
            this.onCheckBox.Text = "On";
            this.onCheckBox.UseVisualStyleBackColor = true;
            this.onCheckBox.CheckedChanged += new System.EventHandler(this.onCheckBox_CheckedChanged);
            // 
            // choiceComboBox
            // 
            this.layoutPanel.SetColumnSpan(this.choiceComboBox, 2);
            this.choiceComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.choiceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.choiceComboBox.Name = "choiceComboBox";
            this.choiceComboBox.TabIndex = 4;
            this.choiceComboBox.SelectedIndexChanged += new System.EventHandler(this.choiceComboBox_SelectedIndexChanged);
            // 
            // CaptureDeviceControlView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.layoutPanel);
            this.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.MinimumSize = new System.Drawing.Size(280, 0);
            this.Name = "CaptureDeviceControlView";
            this.Size = new System.Drawing.Size(280, 70);
            this.layoutPanel.ResumeLayout(false);
            this.layoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.valueSlider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel layoutPanel;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.CheckBox autoCheckBox;
        private System.Windows.Forms.TrackBar valueSlider;
        private System.Windows.Forms.CheckBox onCheckBox;
        private System.Windows.Forms.ComboBox choiceComboBox;
    }
}
