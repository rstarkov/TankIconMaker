using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;
using RT.Util;

namespace TankIconMaker
{
    public partial class App : Application
    {
        public App() { Program.App = this; }

        protected override void OnStartup(StartupEventArgs e)
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            base.OnStartup(e);
            SettingsUtil.LoadSettings(out Program.Settings);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Program.Settings.SaveQuiet();
            base.OnExit(e);
        }
    }

    static class Program
    {
        public static App App;
        public static Settings Settings;
    }
}
