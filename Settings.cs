using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RT.Util;
using RT.Util.Forms;

namespace TankIconMaker
{
    [Settings("TankIconMaker", SettingsKind.UserSpecific)]
    sealed class Settings : SettingsThreadedBase
    {
        public ManagedWindow.Settings MainWindow = new ManagedWindow.Settings();
        public double? LeftColumnWidth = null;
        public double? NameColumnWidth = null;
        public int? DisplayMode = null;

        public string SelectedMakerType;
        public string SelectedMakerName;

        public string SelectedGamePath = Ut.FindTanksDirectory();

        /// <summary>Settings that are specific to a game installation path.</summary>
        public List<GameInstallationSettings> GameInstalls = new List<GameInstallationSettings>();

        protected override SettingsThreadedBase CloneForSaveThreaded()
        {
            var result = (Settings) MemberwiseClone();
            return result;
        }
    }

    sealed class GameInstallationSettings
    {
        /// <summary>Absolute path to the root of this game installation.</summary>
        public string Path { get; set; }
        /// <summary>The version of the game that is located at this path.</summary>
        public string GameVersion;

        public override string ToString()
        {
            return "[{0}] {1}".Fmt(GameVersion, Path);
        }
    }
}
