using System;
using System.Collections.Generic;
using System.ComponentModel;
using RT.Util;
using RT.Util.Forms;

namespace TankIconMaker
{
    /// <summary>
    /// Stores all the settings for TankIconMaker. All the defaults set in the field initializer or the constructor are preserved
    /// whenever they are missing from the settings XML file. Background saves are supported efficiently via
    /// <see cref="SaveThreaded"/>, which is designed to be callable efficiently multiple times in a row.
    /// </summary>
    [Settings("TankIconMaker", SettingsKind.UserSpecific)]
    sealed class Settings : SettingsThreadedBase
    {
        /// <summary>
        /// MainWindow settings, such as position, size and maximized state. Given that TankIconMaker is essentially a single-window
        /// application, all the other MainWindow-specific settings are stored directly in <see cref="Settings"/> for simplicity.
        /// </summary>
        public ManagedWindow.Settings MainWindow = new ManagedWindow.Settings();
        /// <summary>The width of the sidebar in the main window; null to use the width set at design time.</summary>
        public double? LeftColumnWidth = null;
        /// <summary>The width of the name column in the maker property editor; null to use the width set at design time.</summary>
        public double? NameColumnWidth = null;
        /// <summary>The last used index of the display mode dropdown.</summary>
        public int? DisplayMode = null;

        /// <summary>The type name of the last used maker.</summary>
        public string SelectedMakerType;
        /// <summary>The user-friendly name of the last used maker - fallback if finding it by type should fail.</summary>
        public string SelectedMakerName;

        /// <summary>
        /// A list of all the available makers and their settings. This list is here only to keep the settings; the actual list of
        /// available makers is constructed during startup using reflection.
        /// </summary>
        public List<MakerBase> Makers = new List<MakerBase>();

        /// <summary>The last selected game install location.</summary>
        public string SelectedGamePath = Ut.FindTanksDirectory();

        /// <summary>Program language (in the future), and also the default language for property values when not specified.</summary>
        public string Language = "Ru";
        /// <summary>If a maker requests a property by name only, this author is given preference.</summary>
        public string DefaultPropertyAuthor = "Romkyns";

        /// <summary>Settings that are specific to a game installation path.</summary>
        public ObservableSortedList<GameInstallationSettings> GameInstalls = new ObservableSortedList<GameInstallationSettings>();

        protected override SettingsThreadedBase CloneForSaveThreaded()
        {
            var result = (Settings) MemberwiseClone();
            return result;
        }
    }

    /// <summary>
    /// Encapsulates the settings for a specific game install location.
    /// </summary>
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
