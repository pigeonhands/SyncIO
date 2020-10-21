namespace SocketNet
{
    using System;
    using System.Windows.Forms;

    using SocketNet.UI.Forms;

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}