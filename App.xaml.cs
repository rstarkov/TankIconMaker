using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;
using RT.Util;

namespace TankIconMaker
{
    partial class App : Application
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
        public static ObservableSortedList<DataSourceInfo> DataSources = new ObservableSortedList<DataSourceInfo>(
            items: new[] { new DataSourceNone() },
            comparer: CustomComparer<DataSourceInfo>.By(ds => ds is DataSourceNone ? 0 : 1)
                .ThenBy(ds => ds.Name).ThenBy(ds => ds.Language).ThenBy(ds => ds.Author).ThenBy(ds => ds.GameVersion));
    }
}
