namespace TerminalServerPlugin.UI.Forms
{
    partial class Terminal
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
            this.commandPrompt1 = new System.CommandPrompt();
            this.SuspendLayout();
            // 
            // commandPrompt1
            // 
            this.commandPrompt1.BackColor = System.Drawing.Color.Black;
            this.commandPrompt1.Delimiters = new char[] {
        ' '};
            this.commandPrompt1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commandPrompt1.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.commandPrompt1.ForeColor = System.Drawing.Color.Gainsboro;
            this.commandPrompt1.Location = new System.Drawing.Point(0, 0);
            this.commandPrompt1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.commandPrompt1.MessageColor = System.Drawing.Color.Gainsboro;
            this.commandPrompt1.MinimumSize = new System.Drawing.Size(0, 17);
            this.commandPrompt1.Name = "commandPrompt1";
            this.commandPrompt1.PromptColor = System.Drawing.Color.Gainsboro;
            this.commandPrompt1.Size = new System.Drawing.Size(597, 292);
            this.commandPrompt1.TabIndex = 2;
            this.commandPrompt1.Command += new System.CommandPrompt.CommandEventHandler(this.CommandPrompt1_Command);
            // 
            // Terminal
            // 
            this.ClientSize = new System.Drawing.Size(597, 277);
            this.Controls.Add(this.commandPrompt1);
            this.Name = "Terminal";
            this.Text = "Command Prompt";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.CommandPrompt commandPrompt1;
    }
}