using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RT.Util;
using RT.Util.Forms;

namespace TankIconMaker
{
    [Settings("TankIconMaker", SettingsKind.UserSpecific)]
    class Settings : SettingsThreadedBase
    {
        public ManagedWindow.Settings MainWindow = new ManagedWindow.Settings();

        public string SelectedMakerType;
        public string SelectedMakerName;

        protected override SettingsThreadedBase CloneForSaveThreaded()
        {
            var result = (Settings) MemberwiseClone();
            return result;
        }
    }
}
