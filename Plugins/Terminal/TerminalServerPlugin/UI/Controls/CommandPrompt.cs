using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace System
{
    public partial class CommandPrompt : UserControl
    {
        #region Variables

        // Stores the current line
        // Used while navigating the previous commands list
        private int _currentLine = 0;

        // Stores the previous messages
        private readonly ArrayList _prevMessages = new ArrayList();

        #endregion

        #region Properties

        [Description("Indicates text completion behaviour at prompt")]
        [DefaultValue(AutoCompleteMode.None)]
        public AutoCompleteMode AutoComplete
        {
            get { return txtInput.AutoCompleteMode; }
            set
            {
                txtInput.AutoCompleteMode = value;
                txtInput.AutoCompleteSource = AutoCompleteSource.CustomSource;
            }
        }

        private bool autoCompleteStore = false;
        [Description("Store messages for AutoComplete feature")]
        [DefaultValue(false)]
        public bool AutoCompleteStore
        {
            get { return autoCompleteStore; }
            set { autoCompleteStore = value; }
        }

        private BorderStyle borderInput = BorderStyle.None;
        [Description("Border around the input box")]
        [DefaultValue(BorderStyle.None)]
        public BorderStyle BorderInput
        {
            get { return borderInput; }
            set
            {
                borderInput = value;
                panelBottom.BorderStyle = borderInput;
            }
        }

        [Description("Indicates whether input area is cleared on pressing Escape")]
        [DefaultValue(true)]
        public bool CancelOnEsc { get; set; } = true;

        [Description("String of characters which act as delimiters between parameters")]
        [DefaultValue(new char[] { ' ' })]
        public char[] Delimiters { get; set; } = new char[] { ' ' };

        [Description("Determines whether event will fire for blank commands")]
        [DefaultValue(true)]
        public bool IgnoreBlankCommands { get; set; } = true;

        [Description("Color of the messages added using AddMessage method")]
        public Color MessageColor { get; set; } = Color.Gainsboro;

        [Description("Forecolor of the prompt")]
        public Color PromptColor
        {
            get { return lblPrompt.ForeColor; }
            set { lblPrompt.ForeColor = value; }
        }

        private string promptString = ">";
        [Description("String showed as the prompt")]
        [DefaultValue(">")]
        public string PromptString
        {
            get { return promptString; }
            set
            {
                promptString = value;
                lblPrompt.Text = promptString;
            }
        }

        private bool showMessages = true;
        [Description("Show the previous commands")]
        [DefaultValue(true)]
        public bool ShowMessages
        {
            get { return showMessages; }
            set
            {
                if (showMessages != value)
                {
                    if (value)
                    {
                        Height = panelBottom.Height + txtInput.PreferredHeight * 5;
                        rtbMessages.Visible = true;
                    }
                    else
                    {
                        rtbMessages.Visible = false;
                        Height = txtInput.Height;
                    }
                }
                showMessages = value;
            }
        }

        #endregion

        #region Delegates, Events

        public delegate void CommandEventHandler(object sender, CommandEventArgs e);
        [Description("Event raised when the user enters a command and presses the Enter key")]
        public event CommandEventHandler Command;

        protected virtual void OnCommand(CommandEventArgs e)
        {
            Command?.Invoke(this, e);
        }

        public delegate void CommandEnteringEventHandler(object sender, CommandEnteringEventArgs e);
        [Description("Event raised on KeyPress in input area")]
        public event CommandEnteringEventHandler CommandEntering;

        protected virtual void OnCommandEntering(CommandEnteringEventArgs e)
        {
            CommandEntering?.Invoke(this, e);
        }

        public delegate void ParameterEnteredEventHandler(object sender, ParameterEnteredEventArgs e);
        [Description("Event raised when user enters a parameter")]
        public event ParameterEnteredEventHandler ParameterEntered;

        protected virtual void OnParameterEntered(ParameterEnteredEventArgs e)
        {
            ParameterEntered?.Invoke(this, e);
        }

        #endregion

        #region Constructor

        public CommandPrompt()
        {
            BackColor = Color.Black;
            InitializeComponent();
            rtbMessages.Height = 0;
        }

        #endregion

        #region Form, Control Events

        private void Prompt_Load(object sender, EventArgs e)
        {
            txtInput.Height = txtInput.PreferredHeight;
        }
        private void Prompt_FontChanged(object sender, EventArgs e)
        {
            txtInput.Font = Font;
        }
        private void Prompt_ForeColorChanged(object sender, EventArgs e)
        {
            txtInput.ForeColor = ForeColor;
            rtbMessages.ForeColor = ForeColor;
        }
        private void Prompt_BackColorChanged(object sender, EventArgs e)
        {
            rtbMessages.BackColor = BackColor;
            txtInput.BackColor = BackColor;
            panelBottom.BackColor = BackColor;
            lblPrompt.BackColor = BackColor;
        }
        private void Prompt_Resize(object sender, EventArgs e)
        {
            if (showMessages)
            {
                if ((Height - panelBottom.Height) % txtInput.PreferredHeight != 0)
                {
                    Height = ((Height - panelBottom.Height) / txtInput.PreferredHeight + 1)
                        * txtInput.PreferredHeight + panelBottom.Height;
                }
            }
        }

        private void TxtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                if (!string.IsNullOrEmpty(txtInput.Text) && IgnoreBlankCommands)
                {
                    SuspendLayout();

                    string prevPrompt = lblPrompt.Text;
                    Color prevPromptColor = PromptColor;
                    // Raise the parameter entered event
                    ParameterEnteredEventArgs parameterEnteredArgs = new ParameterEnteredEventArgs(txtInput.Text, ParseInput(), true);
                    OnParameterEntered(parameterEnteredArgs);

                    // Raise the command event
                    CommandEventArgs args = new CommandEventArgs(txtInput.Text, ParseInput());
                    OnCommand(args);

                    if (!args.Cancel)
                    {
                        if (rtbMessages.Lines.Length > 0)
                            rtbMessages.AppendText("\r\n");

                        AddCommand(prevPrompt, prevPromptColor, txtInput.Text);

                        if (!string.IsNullOrEmpty(args.Message))
                            AddMessage(args.Message);

                        rtbMessages.ScrollToCaret();
                        _prevMessages.Add(txtInput.Text);
                        if (autoCompleteStore && args.Record)
                            txtInput.AutoCompleteCustomSource.Add(txtInput.Text);

                        _currentLine = _prevMessages.Count - 1;
                    }

                    txtInput.Text = string.Empty;
                    ResumeLayout();
                }
                e.Handled = true;
                HideToolTip();
                return;
            }

            if (e.KeyCode == Keys.Up)
            {
                if (_currentLine >= 0 && _prevMessages.Count > 0)
                {
                    txtInput.Text = _prevMessages[_currentLine].ToString();
                    txtInput.SelectionLength = 0;
                    txtInput.SelectionStart = txtInput.Text.Length;

                    _currentLine--;
                }

                e.Handled = true;
                HideToolTip();
                return;
            }

            if (e.KeyCode == Keys.Down)
            {
                if (_currentLine < _prevMessages.Count - 2)
                {
                    _currentLine++;

                    txtInput.Text = _prevMessages[_currentLine + 1].ToString();
                    txtInput.SelectionLength = 0;
                    txtInput.SelectionStart = txtInput.Text.Length;
                }

                e.Handled = true;
                HideToolTip();
                return;
            }

            if (e.KeyCode == Keys.Escape)
            {
                if (txtInput.SelectionLength > 0 && txtInput.AutoCompleteMode != AutoCompleteMode.None)
                {
                    txtInput.Text = txtInput.Text.Substring(0, txtInput.SelectionStart);
                    txtInput.SelectionStart = txtInput.Text.Length;
                }
                else
                {
                    if (!toolTipCommand.Active && CancelOnEsc)
                        txtInput.Text = string.Empty;
                }
                HideToolTip();
                e.Handled = true;
                return;
            }
        }
        private void TxtInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;
                return;
            }

            if (e.KeyChar == (char)Keys.Back)
            {
                // Hide tooltip if character to be deleted is a delimiter
                if (txtInput.Text.Length > 0)
                {
                    char del = txtInput.Text[txtInput.Text.Length - 1];
                    foreach (char ch in Delimiters)
                    {
                        if (ch == del)
                        {
                            bool flag = true;
                            // Check if previous character is delimiter
                            if (txtInput.Text.Length > 1)
                            {
                                char temp = txtInput.Text[txtInput.Text.Length - 2];
                                foreach (char c in Delimiters)
                                {
                                    if (c == temp)
                                    {
                                        flag = false;
                                        break;
                                    }
                                }
                            }

                            if (flag)
                                HideToolTip();
                            break;
                        }
                    }
                }
                return;
            }

            // Fire the ParameterEntered event
            bool paramFlag = false;
            foreach (char ch in Delimiters)
            {
                if (e.KeyChar == ch)
                {
                    paramFlag = true;
                    break;
                }
            }

            if (paramFlag)
            {
                ParameterEnteredEventArgs parameterEnteredArgs = new ParameterEnteredEventArgs(txtInput.Text, ParseInput());
                OnParameterEntered(parameterEnteredArgs);

                if (!string.IsNullOrEmpty(parameterEnteredArgs.ToolTip))
                {
                    HideToolTip();
                    ShowToolTip(parameterEnteredArgs.ToolTip);
                }
            }
        }
        private void TxtInput_TextChanged(object sender, EventArgs e)
        {
            // Fire the CommandEntering event first
            CommandEnteringEventArgs commandEnteringArgs = new CommandEnteringEventArgs(txtInput.Text);
            OnCommandEntering(commandEnteringArgs);

            if (!string.IsNullOrEmpty(commandEnteringArgs.ToolTip))
            {
                HideToolTip();
                ShowToolTip(commandEnteringArgs.ToolTip);
            }
        }

        private void RtbMessages_TextChanged(object sender, EventArgs e)
        {
            if (showMessages)
            {
                if (rtbMessages.Height < Height - panelBottom.Height)
                    rtbMessages.Height = rtbMessages.Lines.Length * txtInput.PreferredHeight;
                else
                    rtbMessages.ScrollBars = RichTextBoxScrollBars.Both;

                if (rtbMessages.Height > Height - panelBottom.Height)
                {
                    rtbMessages.Height = Height - panelBottom.Height;
                    rtbMessages.ScrollBars = RichTextBoxScrollBars.Both;
                }
                rtbMessages.ScrollToCaret();
            }
        }
        private void RtbMessages_Click(object sender, EventArgs e)
        {
            if (rtbMessages.SelectionLength == 0)
                txtInput.Focus();
        }

        #endregion

        #region Public Methods

        [Description("Adds a message to the message RichTextBox")]
        public void AddMessage(string msg)
        {
            int prevLength = rtbMessages.Text.Length;

            if (rtbMessages.Lines.Length > 0)
                rtbMessages.AppendText("\n" + msg);
            else
                rtbMessages.AppendText(msg);

            rtbMessages.SelectionStart = prevLength;
            rtbMessages.SelectionLength = rtbMessages.Text.Length - prevLength;
            rtbMessages.SelectionColor = MessageColor;

            rtbMessages.SelectionStart = rtbMessages.Text.Length;
            rtbMessages.SelectionLength = 0;
            rtbMessages.SelectionColor = rtbMessages.ForeColor;
            rtbMessages.ScrollToCaret();

            txtInput.Focus();
        }

        [Description("Clear all messages from RichTextBox")]
        public void ClearMessages()
        {
            rtbMessages.Clear();
            rtbMessages.Height = 0;
            rtbMessages.ScrollBars = RichTextBoxScrollBars.None;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds a command to the messages list
        /// </summary>
        /// <param name="prompt">The prompt string</param>
        /// <param name="command">The command entered</param>
        private void AddCommand(string prompt, Color color, string command)
        {
            try
            {
                int prevLength = rtbMessages.Text.Length;

                rtbMessages.AppendText(prompt + " " + command);

                rtbMessages.SelectionStart = prevLength;
                rtbMessages.SelectionLength = prompt.Length;
                rtbMessages.SelectionColor = color;

                rtbMessages.SelectionLength = 0;
                rtbMessages.SelectionStart = rtbMessages.Text.Length;
                rtbMessages.ScrollToCaret();

                txtInput.Focus();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR.CommandPrompt:AddCommand: {0}", ex);
            }
        }

        private string[] ParseInput()
        {
            string temp = txtInput.Text;
            return temp.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries);
        }

        #endregion

        #region AutoComplete Methods

        [Description("Add AutoComplete data")]
        public void AutoCompleteAdd(string str)
        {
            txtInput.AutoCompleteCustomSource.Add(str);
        }

        [Description("Clear all AutoComplete data")]
        public void AutoCompleteClear()
        {
            txtInput.AutoCompleteCustomSource.Clear();
        }

        [Description("Save the messages to a file")]
        public bool SaveMessages(string FileName)
        {
            try
            {
                rtbMessages.SaveFile(FileName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Description("Load messages from a file")]
        public bool LoadMessages(string FileName)
        {
            try
            {
                rtbMessages.LoadFile(FileName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region ToolTip Methods

        public void ShowToolTip(string tip)
        {
            Point pos = txtInput.GetPositionFromCharIndex(txtInput.Text.Length - 1);
            if (txtInput.Text.Length > 1)
                pos.X += TextRenderer.MeasureText(txtInput.Text.Substring(txtInput.Text.Length - 1, 1), txtInput.Font).Width;
            int height = TextRenderer.MeasureText(tip, new Font("Tahoma", 9)).Height + 2;
            if (rtbMessages.Size.Height + panelBottom.Height + height > Height)
                pos.Y -= height;
            else
                pos.Y += TextRenderer.MeasureText("test", txtInput.Font).Height;
            toolTipCommand.Show(tip, txtInput, pos);
        }
        public void HideToolTip()
        {
            if (toolTipCommand.Active)
                toolTipCommand.Hide(txtInput);
        }

        #endregion
    }

    partial class CommandPrompt
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            this.components = new Container();
            this.panelBottom = new Panel();
            this.txtInput = new TextBox();
            this.lblPrompt = new Label();
            this.rtbMessages = new RichTextBox();
            this.toolTipCommand = new ToolTip(this.components);
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelBottom
            // 
            this.panelBottom.BackColor = Color.Black;
            this.panelBottom.Controls.Add(this.txtInput);
            this.panelBottom.Controls.Add(this.lblPrompt);
            this.panelBottom.Dock = DockStyle.Top;
            this.panelBottom.Location = new Point(0, 78);
            this.panelBottom.Margin = new Padding(0);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new Size(200, 17);
            this.panelBottom.TabIndex = 0;
            // 
            // txtInput
            // 
            this.txtInput.BackColor = Color.Black;
            this.txtInput.BorderStyle = BorderStyle.None;
            this.txtInput.Dock = DockStyle.Fill;
            this.txtInput.Location = new Point(13, 0);
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new Size(187, 13);
            this.txtInput.TabIndex = 1;
            this.txtInput.TextChanged += new EventHandler(this.TxtInput_TextChanged);
            this.txtInput.KeyDown += new KeyEventHandler(this.TxtInput_KeyDown);
            this.txtInput.KeyPress += new KeyPressEventHandler(this.TxtInput_KeyPress);
            // 
            // lblPrompt
            // 
            this.lblPrompt.AutoSize = true;
            this.lblPrompt.BackColor = Color.Black;
            this.lblPrompt.Dock = DockStyle.Left;
            this.lblPrompt.Location = new Point(0, 0);
            this.lblPrompt.Margin = new Padding(0);
            this.lblPrompt.Name = "lblPrompt";
            this.lblPrompt.Size = new Size(13, 13);
            this.lblPrompt.TabIndex = 0;
            this.lblPrompt.Text = ">";
            // 
            // rtbMessages
            // 
            this.rtbMessages.BackColor = Color.Black;
            this.rtbMessages.BorderStyle = BorderStyle.None;
            this.rtbMessages.Dock = DockStyle.Top;
            this.rtbMessages.Location = new Point(0, 0);
            this.rtbMessages.Name = "rtbMessages";
            this.rtbMessages.ReadOnly = true;
            this.rtbMessages.ScrollBars = RichTextBoxScrollBars.None;
            this.rtbMessages.Size = new Size(200, 78);
            this.rtbMessages.TabIndex = 2;
            this.rtbMessages.TabStop = false;
            this.rtbMessages.Text = "";
            this.rtbMessages.Click += new EventHandler(this.RtbMessages_Click);
            this.rtbMessages.TextChanged += new EventHandler(this.RtbMessages_TextChanged);
            // 
            // toolTipCommand
            // 
            this.toolTipCommand.AutoPopDelay = 5000;
            this.toolTipCommand.InitialDelay = 0;
            this.toolTipCommand.ReshowDelay = 100;
            // 
            // CommandPrompt
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.rtbMessages);
            this.MinimumSize = new Size(0, 17);
            this.Name = "CommandPrompt";
            this.Size = new Size(200, 95);
            this.Load += new EventHandler(this.Prompt_Load);
            this.BackColorChanged += new EventHandler(this.Prompt_BackColorChanged);
            this.FontChanged += new EventHandler(this.Prompt_FontChanged);
            this.ForeColorChanged += new EventHandler(this.Prompt_ForeColorChanged);
            this.Resize += new EventHandler(this.Prompt_Resize);
            this.panelBottom.ResumeLayout(false);
            this.panelBottom.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private Panel panelBottom;
        private Label lblPrompt;
        private TextBox txtInput;
        private RichTextBox rtbMessages;
        private ToolTip toolTipCommand;
    }

    public class CommandEventArgs : EventArgs
    {
        public CommandEventArgs(string cmd, string[] param)
        {
            Command = cmd;
            Message = string.Empty;
            Record = true;
            Parameters = param;
        }

        [Description("Indicates whether command will be displayed in messages list")]
        public bool Cancel { get; set; } = false;

        [Description("The command entered by the user")]
        public string Command { get; set; }

        [Description("Message to be displayed to user in response to command")]
        public string Message { get; set; }

        [Description("Indicates whether the command should be stored for AutoComplete")]
        public bool Record { get; set; }

        [Description("Parameters of the command")]
        public string[] Parameters { get; set; }
    }

    public class CommandEnteringEventArgs : EventArgs
    {
        public CommandEnteringEventArgs(string comm)
        {
            Command = comm;
            ToolTip = string.Empty;
        }

        [Description("Command in the input area")]
        public string Command { get; set; }

        [Description("Tooltip to be displayed")]
        public string ToolTip { get; set; }
    }

    public class ParameterEnteredEventArgs : EventArgs
    {
        public ParameterEnteredEventArgs(string cmd, string[] list)
        {
            command = cmd;
            parameters = list;
            enterKey = false;
            toolTip = "";
        }

        public ParameterEnteredEventArgs(string cmd, string[] list, bool enter)
        {
            command = cmd;
            parameters = list;
            enterKey = enter;
            toolTip = "";
        }

        private string command;
        [Description("Command entered currently")]
        public string Command
        {
            get { return command; }
            set { command = value; }
        }


        private string[] parameters;
        [Description("List of paramters entered")]
        public string[] Parameters
        {
            get { return parameters; }
            set { parameters = value; }
        }

        private bool enterKey;
        [Description("Determines whether the enter key has been pressed")]
        public bool EnterKey
        {
            get { return enterKey; }
            set { enterKey = value; }
        }

        private string toolTip;
        [Description("ToolTip to be shown")]
        public string ToolTip
        {
            get { return toolTip; }
            set { toolTip = value; }
        }
    }
}