﻿namespace ChatServer
{
    using System;
    using System.Windows.Forms;

    class Program
    {
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);         
            Application.Run(new Form1());
        }
    }
}