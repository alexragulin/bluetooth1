﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace bluetooth
{
    static class Program
    {
        public static string PortName { get; internal set; }

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form());
        }
    }
}
