namespace ChatServerPlugin.UI.Forms
{
    partial class GroupChat
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lvClients = new Controls.ListViewEx();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnChatLogsArchive = new System.Windows.Forms.Button();
            this.chkSaveGroupChatLogs = new System.Windows.Forms.CheckBox();
            this.chkSendOnEnter = new System.Windows.Forms.CheckBox();
            this.chkAllowFormClose = new System.Windows.Forms.CheckBox();
            this.btnNudge = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.txtMessage = new Controls.CueTextBox();
            this.rtbChat = new System.Windows.Forms.RichTextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblTotalClients = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lvClients);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.btnChatLogsArchive);
            this.splitContainer1.Panel2.Controls.Add(this.chkSaveGroupChatLogs);
            this.splitContainer1.Panel2.Controls.Add(this.chkSendOnEnter);
            this.splitContainer1.Panel2.Controls.Add(this.chkAllowFormClose);
            this.splitContainer1.Panel2.Controls.Add(this.btnNudge);
            this.splitContainer1.Panel2.Controls.Add(this.btnSend);
            this.splitContainer1.Panel2.Controls.Add(this.txtMessage);
            this.splitContainer1.Panel2.Controls.Add(this.rtbChat);
            this.splitContainer1.Size = new System.Drawing.Size(797, 303);
            this.splitContainer1.SplitterDistance = 265;
            this.splitContainer1.TabIndex = 1;
            // 
            // lvClients
            // 
            this.lvClients.CheckBoxes = true;
            this.lvClients.ColumnIndex = -1;
            this.lvClients.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.lvClients.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvClients.FullRowSelect = true;
            this.lvClients.GridLines = true;
            this.lvClients.Location = new System.Drawing.Point(0, 0);
            this.lvClients.Name = "lvClients";
            this.lvClients.Size = new System.Drawing.Size(265, 303);
            this.lvClients.TabIndex = 0;
            this.lvClients.UseCompatibleStateImageBehavior = false;
            this.lvClients.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Username@Computer";
            this.columnHeader1.Width = 136;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "IPv4 Address";
            this.columnHeader2.Width = 104;
            // 
            // btnChatLogsArchive
            // 
            this.btnChatLogsArchive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChatLogsArchive.Location = new System.Drawing.Point(468, 275);
            this.btnChatLogsArchive.Name = "btnChatLogsArchive";
            this.btnChatLogsArchive.Size = new System.Drawing.Size(57, 23);
            this.btnChatLogsArchive.TabIndex = 8;
            this.btnChatLogsArchive.Text = "Archives";
            this.btnChatLogsArchive.UseVisualStyleBackColor = true;
            this.btnChatLogsArchive.Click += new System.EventHandler(this.btnChatLogsArchive_Click);
            // 
            // chkSaveGroupChatLogs
            // 
            this.chkSaveGroupChatLogs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkSaveGroupChatLogs.AutoSize = true;
            this.chkSaveGroupChatLogs.Checked = true;
            this.chkSaveGroupChatLogs.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSaveGroupChatLogs.Location = new System.Drawing.Point(3, 279);
            this.chkSaveGroupChatLogs.Name = "chkSaveGroupChatLogs";
            this.chkSaveGroupChatLogs.Size = new System.Drawing.Size(259, 17);
            this.chkSaveGroupChatLogs.TabIndex = 7;
            this.chkSaveGroupChatLogs.Text = "Save group chat logs to the database on exit.";
            this.chkSaveGroupChatLogs.UseVisualStyleBackColor = true;
            // 
            // chkSendOnEnter
            // 
            this.chkSendOnEnter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chkSendOnEnter.AutoSize = true;
            this.chkSendOnEnter.Checked = true;
            this.chkSendOnEnter.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSendOnEnter.Location = new System.Drawing.Point(219, 256);
            this.chkSendOnEnter.Name = "chkSendOnEnter";
            this.chkSendOnEnter.Size = new System.Drawing.Size(245, 17);
            this.chkSendOnEnter.TabIndex = 6;
            this.chkSendOnEnter.Text = "Pressing the \'Enter\' key sends the message";
            this.chkSendOnEnter.UseVisualStyleBackColor = true;
            // 
            // chkAllowFormClose
            // 
            this.chkAllowFormClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkAllowFormClose.AutoSize = true;
            this.chkAllowFormClose.Location = new System.Drawing.Point(3, 256);
            this.chkAllowFormClose.Name = "chkAllowFormClose";
            this.chkAllowFormClose.Size = new System.Drawing.Size(199, 17);
            this.chkAllowFormClose.TabIndex = 5;
            this.chkAllowFormClose.Text = "Allow client to close chat window";
            this.chkAllowFormClose.UseVisualStyleBackColor = true;
            this.chkAllowFormClose.CheckedChanged += new System.EventHandler(this.chkAllowFormClose_CheckedChanged);
            // 
            // btnNudge
            // 
            this.btnNudge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNudge.Location = new System.Drawing.Point(468, 252);
            this.btnNudge.Name = "btnNudge";
            this.btnNudge.Size = new System.Drawing.Size(57, 23);
            this.btnNudge.TabIndex = 4;
            this.btnNudge.Text = "Nudge";
            this.btnNudge.UseVisualStyleBackColor = true;
            this.btnNudge.Click += new System.EventHandler(this.btnNudge_Click);
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.Location = new System.Drawing.Point(468, 229);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(57, 23);
            this.btnSend.TabIndex = 3;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // txtMessage
            // 
            this.txtMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMessage.CueText = null;
            this.txtMessage.CueTextFocus = false;
            this.txtMessage.Location = new System.Drawing.Point(3, 229);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(459, 23);
            this.txtMessage.TabIndex = 2;
            this.txtMessage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtMessage_KeyDown);
            // 
            // rtbChat
            // 
            this.rtbChat.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbChat.Location = new System.Drawing.Point(3, 3);
            this.rtbChat.Name = "rtbChat";
            this.rtbChat.ReadOnly = true;
            this.rtbChat.Size = new System.Drawing.Size(522, 225);
            this.rtbChat.TabIndex = 2;
            this.rtbChat.Text = "";
            this.rtbChat.TextChanged += new System.EventHandler(this.rtbChat_TextChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.viewToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(797, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblTotalClients});
            this.statusStrip1.Location = new System.Drawing.Point(0, 327);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(797, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblTotalClients
            // 
            this.lblTotalClients.Name = "lblTotalClients";
            this.lblTotalClients.Size = new System.Drawing.Size(84, 17);
            this.lblTotalClients.Text = "Total Clients: 0";
            // 
            // GroupChat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(797, 349);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "GroupChat";
            this.Text = "Group Chat";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_FormClosing);
            this.Load += new System.EventHandler(this.Form_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.ListViewEx lvClients;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnNudge;
        private System.Windows.Forms.Button btnSend;
        private Controls.CueTextBox txtMessage;
        private System.Windows.Forms.CheckBox chkSendOnEnter;
        private System.Windows.Forms.CheckBox chkAllowFormClose;
        internal System.Windows.Forms.RichTextBox rtbChat;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.CheckBox chkSaveGroupChatLogs;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblTotalClients;
        private System.Windows.Forms.Button btnChatLogsArchive;
    }
}