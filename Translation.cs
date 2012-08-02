﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RT.Util.Lingo;

namespace TankIconMaker
{
    public enum TranslationGroup
    {
        [LingoGroup("Main window", "Contains strings used directly in the main window interface.")]
        MainWindow,
    }

    [LingoStringClass]
    sealed class Translation : TranslationBase
    {
        public Translation() : base(Language.EnglishUK) { }

        public MainWindowTranslation MainWindow = new MainWindowTranslation();
    }
}
