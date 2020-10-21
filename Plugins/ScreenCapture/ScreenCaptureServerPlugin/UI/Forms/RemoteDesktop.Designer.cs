namespace ScreenCaptureServerPlugin.UI.Forms
{
    partial class RemoteDesktop
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RemoteDesktop));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnSingle = new System.Windows.Forms.ToolStripButton();
            this.btnAutomatic = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnSave = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnSendKeys = new System.Windows.Forms.ToolStripSplitButton();
            this.sendCtrlAltDelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.btnMouse = new System.Windows.Forms.ToolStripButton();
            this.btnKeyboard = new System.Windows.Forms.ToolStripButton();
            this.btnToggleFullScreen = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.lblSize = new System.Windows.Forms.ToolStripLabel();
            this.lblFPS = new System.Windows.Forms.ToolStripLabel();
            this.pbSettings = new System.Windows.Forms.PictureBox();
            this.gdiScreen1 = new ScreenCaptureServerPlugin.UI.Controls.GdiScreen();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbSettings)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.AutoSize = false;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnSingle,
            this.btnAutomatic,
            this.toolStripSeparator1,
            this.btnSave,
            this.toolStripSeparator2,
            this.toolStripLabel1,
            this.toolStripSeparator3,
            this.btnSendKeys,
            this.toolStripSeparator4,
            this.btnMouse,
            this.btnKeyboard,
            this.btnToggleFullScreen,
            this.toolStripSeparator5,
            this.lblSize,
            this.lblFPS});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(723, 25);
            this.toolStrip1.TabIndex = 5;
            // 
            // btnSingle
            // 
            this.btnSingle.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSingle.Image = ((System.Drawing.Image)(resources.GetObject("btnSingle.Image")));
            this.btnSingle.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSingle.Margin = new System.Windows.Forms.Padding(10, 1, 0, 2);
            this.btnSingle.Name = "btnSingle";
            this.btnSingle.Size = new System.Drawing.Size(34, 22);
            this.btnSingle.Text = "Single Screenshot";
            this.btnSingle.Click += new System.EventHandler(this.BtnSingle_Click);
            // 
            // btnAutomatic
            // 
            this.btnAutomatic.CheckOnClick = true;
            this.btnAutomatic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAutomatic.Image = ((System.Drawing.Image)(resources.GetObject("btnAutomatic.Image")));
            this.btnAutomatic.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAutomatic.Margin = new System.Windows.Forms.Padding(10, 1, 10, 2);
            this.btnAutomatic.Name = "btnAutomatic";
            this.btnAutomatic.Size = new System.Drawing.Size(34, 22);
            this.btnAutomatic.Text = "Automatic Screenshots";
            this.btnAutomatic.Click += new System.EventHandler(this.BtnAutomatic_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnSave
            // 
            this.btnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSave.Image = ((System.Drawing.Image)(resources.GetObject("btnSave.Image")));
            this.btnSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSave.Margin = new System.Windows.Forms.Padding(10, 1, 10, 2);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(34, 22);
            this.btnSave.Text = "Save Screenshot";
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.BackColor = System.Drawing.Color.Transparent;
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(72, 20);
            this.toolStripLabel1.Text = "            ";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // btnSendKeys
            // 
            this.btnSendKeys.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSendKeys.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sendCtrlAltDelToolStripMenuItem});
            this.btnSendKeys.Image = ((System.Drawing.Image)(resources.GetObject("btnSendKeys.Image")));
            this.btnSendKeys.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSendKeys.Margin = new System.Windows.Forms.Padding(10, 1, 10, 2);
            this.btnSendKeys.Name = "btnSendKeys";
            this.btnSendKeys.Size = new System.Drawing.Size(45, 22);
            this.btnSendKeys.Text = "Send Key(s)";
            // 
            // sendCtrlAltDelToolStripMenuItem
            // 
            this.sendCtrlAltDelToolStripMenuItem.Name = "sendCtrlAltDelToolStripMenuItem";
            this.sendCtrlAltDelToolStripMenuItem.Size = new System.Drawing.Size(258, 34);
            this.sendCtrlAltDelToolStripMenuItem.Text = "Send Ctrl+Alt+Del";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // btnMouse
            // 
            this.btnMouse.CheckOnClick = true;
            this.btnMouse.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnMouse.Image = ((System.Drawing.Image)(resources.GetObject("btnMouse.Image")));
            this.btnMouse.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnMouse.Margin = new System.Windows.Forms.Padding(10, 1, 10, 2);
            this.btnMouse.Name = "btnMouse";
            this.btnMouse.Size = new System.Drawing.Size(34, 22);
            this.btnMouse.Text = "Mouse Input";
            // 
            // btnKeyboard
            // 
            this.btnKeyboard.CheckOnClick = true;
            this.btnKeyboard.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnKeyboard.Image = ((System.Drawing.Image)(resources.GetObject("btnKeyboard.Image")));
            this.btnKeyboard.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnKeyboard.Margin = new System.Windows.Forms.Padding(0, 1, 10, 2);
            this.btnKeyboard.Name = "btnKeyboard";
            this.btnKeyboard.Size = new System.Drawing.Size(34, 22);
            this.btnKeyboard.Text = "Keyboard Input";
            // 
            // btnToggleFullScreen
            // 
            this.btnToggleFullScreen.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.btnToggleFullScreen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnToggleFullScreen.Image = ((System.Drawing.Image)(resources.GetObject("btnToggleFullScreen.Image")));
            this.btnToggleFullScreen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnToggleFullScreen.Margin = new System.Windows.Forms.Padding(10, 1, 10, 2);
            this.btnToggleFullScreen.Name = "btnToggleFullScreen";
            this.btnToggleFullScreen.Size = new System.Drawing.Size(34, 22);
            this.btnToggleFullScreen.Text = "toolStripButton1";
            this.btnToggleFullScreen.Click += new System.EventHandler(this.BtnToggleFullScreen_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // lblSize
            // 
            this.lblSize.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.lblSize.BackColor = System.Drawing.Color.Transparent;
            this.lblSize.Margin = new System.Windows.Forms.Padding(0, 1, 10, 2);
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(69, 22);
            this.lblSize.Text = "0 Bytes";
            // 
            // lblFPS
            // 
            this.lblFPS.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.lblFPS.BackColor = System.Drawing.Color.Transparent;
            this.lblFPS.Margin = new System.Windows.Forms.Padding(0, 1, 10, 2);
            this.lblFPS.Name = "lblFPS";
            this.lblFPS.Size = new System.Drawing.Size(56, 22);
            this.lblFPS.Text = "0 FPS";
            // 
            // pbSettings
            // 
            this.pbSettings.BackColor = System.Drawing.SystemColors.Control;
            this.pbSettings.Image = ((System.Drawing.Image)(resources.GetObject("pbSettings.Image")));
            this.pbSettings.Location = new System.Drawing.Point(140, 2);
            this.pbSettings.Name = "pbSettings";
            this.pbSettings.Size = new System.Drawing.Size(23, 22);
            this.pbSettings.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbSettings.TabIndex = 11;
            this.pbSettings.TabStop = false;
            this.pbSettings.Click += new System.EventHandler(this.PopupSettings_Click);
            this.pbSettings.MouseEnter += new System.EventHandler(this.PopupSettings_MouseEnter);
            this.pbSettings.MouseLeave += new System.EventHandler(this.PopupSettings_MouseLeave);
            // 
            // gdiScreen1
            // 
            this.gdiScreen1.BorderColor = System.Drawing.Color.DodgerBlue;
            this.gdiScreen1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gdiScreen1.Location = new System.Drawing.Point(0, 25);
            this.gdiScreen1.Name = "gdiScreen1";
            this.gdiScreen1.Screen = ((System.Drawing.Bitmap)(resources.GetObject("gdiScreen1.Screen")));
            this.gdiScreen1.ShowBorders = true;
            this.gdiScreen1.Size = new System.Drawing.Size(723, 402);
            this.gdiScreen1.TabIndex = 12;
            this.gdiScreen1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gdiScreen1_MouseDown);
            this.gdiScreen1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gdiScreen1_MouseMove);
            this.gdiScreen1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gdiScreen1_MouseUp);
            // 
            // RemoteDesktop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(723, 427);
            this.Controls.Add(this.gdiScreen1);
            this.Controls.Add(this.pbSettings);
            this.Controls.Add(this.toolStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.Name = "RemoteDesktop";
            this.Text = "Remote Desktop Connection";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RemoteDesktop_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.RemoteDesktop_KeyUp);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbSettings)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnSingle;
        private System.Windows.Forms.ToolStripButton btnAutomatic;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnSave;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSplitButton btnSendKeys;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton btnMouse;
        private System.Windows.Forms.ToolStripButton btnKeyboard;
        private System.Windows.Forms.ToolStripMenuItem sendCtrlAltDelToolStripMenuItem;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.PictureBox pbSettings;
        private System.Windows.Forms.ToolStripButton btnToggleFullScreen;
        private Controls.GdiScreen gdiScreen1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripLabel lblFPS;
        private System.Windows.Forms.ToolStripLabel lblSize;
    }
}