namespace FileManagerServerPlugin.UI.Dialogs
{
    class InputBoxDialog : global::System.Windows.Forms.Form
    {
        #region Variables

        global::System.Windows.Forms.Button btnOkay;
        global::System.Windows.Forms.Button btnCancel;
        global::System.Windows.Forms.TextBox txtInput;
        global::System.Windows.Forms.Label lblMessage;
        global::System.ComponentModel.Container components = null;

        #endregion

        #region Properties

        public string Input
        {
            get { return txtInput.Text; }
        }

        #endregion

        #region Constructor

        private InputBoxDialog()
        {
            InitializeComponent();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates an instance of PasswordDialog and uses it to obtain the user's password.
        /// </summary>
        /// <returns>Returns the user's password as entered, or null if he/she clicked Cancel.</returns>
        public static string GetInput(string message, string title, string defaultValue = null)
        {
            using (InputBoxDialog ibDialog = new InputBoxDialog())
            {
                ibDialog.Text = title;
                ibDialog.lblMessage.Text = message;
                ibDialog.txtInput.Text = defaultValue;
                if (ibDialog.ShowDialog() == global::System.Windows.Forms.DialogResult.OK)
                    return ibDialog.Input;
                else
                    return null; // If the user clicks Cancel, return null and not the empty string.
            }
        }

        #endregion

        #region Private Methods

        private void InitializeComponent()
        {
            this.txtInput = new global::System.Windows.Forms.TextBox();
            this.btnOkay = new global::System.Windows.Forms.Button();
            this.btnCancel = new global::System.Windows.Forms.Button();
            this.lblMessage = new global::System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtInput
            // 
            this.txtInput.Location = new global::System.Drawing.Point(12, 99);
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new global::System.Drawing.Size(340, 20);
            this.txtInput.TabIndex = 0;
            // 
            // btnOkay
            // 
            this.btnOkay.DialogResult = global::System.Windows.Forms.DialogResult.OK;
            this.btnOkay.Location = new global::System.Drawing.Point(277, 12);
            this.btnOkay.Name = "btnOkay";
            this.btnOkay.Size = new global::System.Drawing.Size(75, 23);
            this.btnOkay.TabIndex = 1;
            this.btnOkay.Text = "OK";
            this.btnOkay.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = global::System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new global::System.Drawing.Point(277, 41);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new global::System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // lblMessage
            // 
            this.lblMessage.Location = new global::System.Drawing.Point(12, 17);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new global::System.Drawing.Size(259, 64);
            this.lblMessage.TabIndex = 3;
            this.lblMessage.Text = "null";
            // 
            // InputBox
            // 
            this.AcceptButton = this.btnOkay;
            this.CancelButton = this.btnCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new global::System.Drawing.Size(364, 131);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOkay);
            this.Controls.Add(this.txtInput);
            this.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InputBox";
            this.ShowIcon = false;
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}