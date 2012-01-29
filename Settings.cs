using System;
using System.Collections.Generic;
using System.ComponentModel;
using RT.Util;

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

        public List<MakerBase> Makers = new List<MakerBase>();

        /// <summary>Settings that are specific to a game installation path.</summary>
        public ObservableSortedList<GameInstallationSettings> GameInstalls = new ObservableSortedList<GameInstallationSettings>();

        protected override SettingsThreadedBase CloneForSaveThreaded()
        {
            var result = (Settings) MemberwiseClone();
            return result;
        }
    }

    sealed class GameInstallationSettings : IComparable<GameInstallationSettings>, INotifyPropertyChanged
    {
        /// <summary>Absolute path to the root of this game installation.</summary>
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Path"));
                PropertyChanged(this, new PropertyChangedEventArgs("DisplayName"));
            }
        }
        private string _path;

        /// <summary>The version of the game that is located at this path.</summary>
        public string GameVersion
        {
            get { return _gameVersion; }
            set
            {
                _gameVersion = value;
                PropertyChanged(this, new PropertyChangedEventArgs("GameVersion"));
                PropertyChanged(this, new PropertyChangedEventArgs("DisplayName"));
            }
        }
        private string _gameVersion;

        /// <summary>The value displayed in the drop-down.</summary>
        public string DisplayName { get { return _gameVersion + ":  " + _path; } }

        public int CompareTo(GameInstallationSettings other)
        {
            if (other == null) return 1;
            int result = -string.Compare(GameVersion, other.GameVersion);
            if (result != 0)
                return result;
            return string.Compare(Path, other.Path);
        }

        public event PropertyChangedEventHandler PropertyChanged = (_, __) => { };
    }
}
