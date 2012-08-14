﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using RT.Util.ExtensionMethods;
using RT.Util.Lingo;

namespace TankIconMaker
{
    public enum TranslationGroup
    {
        [LingoGroup("Main window", "Strings used directly in the main window interface.")]
        MainWindow,
        [LingoGroup("Property categories", "Strings used to group properties into categories in the property grid.")]
        PropertyCategory,
        [LingoGroup("Layer: all", "Strings used in the property grid for all layers")]
        LayerAll,
        [LingoGroup("Layer: Background / Dark Agent", "Strings used in the property grid for the Background / Dark Agent layer")]
        LayerBkgDarkAgent,
    }

    [LingoStringClass]
    sealed class Translation : TranslationBase
    {
        public Translation() : base(Language.EnglishUK) { }

        public MainWindowTranslation MainWindow = new MainWindowTranslation();

        public LayerTranslation Layer = new LayerTranslation();
        public BkgDarkAgentLayerTranslation BkgDarkAgentLayer = new BkgDarkAgentLayerTranslation();

        [LingoInGroup(TranslationGroup.PropertyCategory)]
        public TrString CategoryGeneral = "General";
        [LingoInGroup(TranslationGroup.PropertyCategory)]
        public TrString CategorySettings = "Settings";
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.LayerAll)]
    sealed class LayerTranslation
    {
        public TypeDescriptorTr Visible = new TypeDescriptorTr { DisplayName = "Visible", Description = "Allows you to hide this layer without deleting it." };
        public TypeDescriptorTr VisibleFor = new TypeDescriptorTr { DisplayName = "Visible for", Description = "Specifies which types of tanks this layer should be visible for." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.LayerBkgDarkAgent)]
    sealed class BkgDarkAgentLayerTranslation
    {
        public TrString LayerName = "Background / Dark Agent";
        public TrString LayerDescription = "Draws a background using a glassy style inspired by Black_Spy’s icon set.";

        public TypeDescriptorTr BackColor = new TypeDescriptorTr { DisplayName = "Background color", Description = "Background color." };
    }
}
