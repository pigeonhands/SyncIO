namespace ScreenCaptureServerPlugin.UI.Controls
{
    partial class DesktopSettings
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.tbQuality = new System.Windows.Forms.TrackBar();
            this.lblQuality = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.tbFramesPerSecond = new System.Windows.Forms.TrackBar();
            this.lblMaxFPS = new System.Windows.Forms.Label();
            this.chkInverseImage = new System.Windows.Forms.CheckBox();
            this.chkAutoSave = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.tbQuality)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbFramesPerSecond)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 23);
            this.label1.TabIndex = 7;
            this.label1.Text = "Video Device:";
            // 
            // tbQuality
            // 
            this.tbQuality.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbQuality.LargeChange = 10;
            this.tbQuality.Location = new System.Drawing.Point(23, 125);
            this.tbQuality.Maximum = 100;
            this.tbQuality.Name = "tbQuality";
            this.tbQuality.Size = new System.Drawing.Size(280, 69);
            this.tbQuality.SmallChange = 10;
            this.tbQuality.TabIndex = 11;
            this.tbQuality.TickFrequency = 10;
            this.tbQuality.Value = 50;
            this.tbQuality.Scroll += new System.EventHandler(this.tbQuality_Scroll);
            // 
            // lblQuality
            // 
            this.lblQuality.AutoSize = true;
            this.lblQuality.Location = new System.Drawing.Point(15, 109);
            this.lblQuality.Name = "lblQuality";
            this.lblQuality.Size = new System.Drawing.Size(158, 23);
            this.lblQuality.TabIndex = 10;
            this.lblQuality.Text = "Image Quality: 50%";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(233, 292);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(152, 292);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 8;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // tbFramesPerSecond
            // 
            this.tbFramesPerSecond.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFramesPerSecond.Location = new System.Drawing.Point(23, 176);
            this.tbFramesPerSecond.Maximum = 30;
            this.tbFramesPerSecond.Minimum = 1;
            this.tbFramesPerSecond.Name = "tbFramesPerSecond";
            this.tbFramesPerSecond.Size = new System.Drawing.Size(280, 69);
            this.tbFramesPerSecond.TabIndex = 13;
            this.tbFramesPerSecond.Value = 5;
            this.tbFramesPerSecond.Scroll += new System.EventHandler(this.tbFramesPerSecond_Scroll);
            // 
            // lblMaxFPS
            // 
            this.lblMaxFPS.AutoSize = true;
            this.lblMaxFPS.Location = new System.Drawing.Point(15, 160);
            this.lblMaxFPS.Name = "lblMaxFPS";
            this.lblMaxFPS.Size = new System.Drawing.Size(209, 23);
            this.lblMaxFPS.TabIndex = 12;
            this.lblMaxFPS.Text = "Max Frames Per Second: 5";
            // 
            // chkInverseImage
            // 
            this.chkInverseImage.AutoSize = true;
            this.chkInverseImage.Location = new System.Drawing.Point(18, 217);
            this.chkInverseImage.Name = "chkInverseImage";
            this.chkInverseImage.Size = new System.Drawing.Size(143, 27);
            this.chkInverseImage.TabIndex = 14;
            this.chkInverseImage.Text = "Inverse Image";
            this.chkInverseImage.UseVisualStyleBackColor = true;
            // 
            // chkAutoSave
            // 
            this.chkAutoSave.AutoSize = true;
            this.chkAutoSave.Location = new System.Drawing.Point(18, 240);
            this.chkAutoSave.Name = "chkAutoSave";
            this.chkAutoSave.Size = new System.Drawing.Size(344, 27);
            this.chkAutoSave.TabIndex = 15;
            this.chkAutoSave.Text = "Automatically save each captured image";
            this.chkAutoSave.UseVisualStyleBackColor = true;
            // 
            // DesktopSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblQuality);
            this.Controls.Add(this.lblMaxFPS);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.tbQuality);
            this.Controls.Add(this.chkAutoSave);
            this.Controls.Add(this.chkInverseImage);
            this.Controls.Add(this.tbFramesPerSecond);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "DesktopSettings";
            this.Size = new System.Drawing.Size(323, 331);
            ((System.ComponentModel.ISupportInitialize)(this.tbQuality)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbFramesPerSecond)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblQuality;
        internal System.Windows.Forms.Button btnCancel;
        internal System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblMaxFPS;
        private System.Windows.Forms.CheckBox chkInverseImage;
        private System.Windows.Forms.CheckBox chkAutoSave;
        private System.Windows.Forms.TrackBar tbQuality;
        private System.Windows.Forms.TrackBar tbFramesPerSecond;
    }
}