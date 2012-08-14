﻿using RT.Util.Lingo;

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
        [LingoGroup("Value: Yes / No / Passthrough", "Strings used for the yes/no/passthrough drop-down")]
        BoolWithPassthrough,
    }

    [LingoStringClass]
    sealed class Translation : TranslationBase
    {
        public Translation() : base(Language.EnglishUK) { }

        public MainWindowTranslation MainWindow = new MainWindowTranslation();

        public LayerTranslation Layer = new LayerTranslation();
        public BkgDarkAgentLayerTranslation BkgDarkAgentLayer = new BkgDarkAgentLayerTranslation();

        public BoolWithPassthroughTranslation BoolWithPassthrough = new BoolWithPassthroughTranslation();

        [LingoInGroup(TranslationGroup.PropertyCategory)]
        public TrString CategoryGeneral = "General";
        [LingoInGroup(TranslationGroup.PropertyCategory)]
        public TrString CategorySettings = "Settings";
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.LayerAll)]
    sealed class LayerTranslation
    {
        public MemberDescriptionTr Visible = new MemberDescriptionTr { DisplayName = "Visible", Description = "Allows you to hide this layer without deleting it." };
        public MemberDescriptionTr VisibleFor = new MemberDescriptionTr { DisplayName = "Visible for", Description = "Specifies which types of tanks this layer should be visible for." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.LayerBkgDarkAgent)]
    sealed class BkgDarkAgentLayerTranslation
    {
        public TrString LayerName = "Background / Dark Agent";
        public TrString LayerDescription = "Draws a background using a glassy style inspired by Black_Spy’s icon set.";

        public MemberDescriptionTr BackColor = new MemberDescriptionTr { DisplayName = "Background color", Description = "Background color." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.BoolWithPassthrough)]
    sealed class BoolWithPassthroughTranslation
    {
        public TrString Yes = "Yes";
        public TrString No = "No";
        public TrString Passthrough = "Passthrough: use next By";

        public class Conv : LingoEnumConverter<BoolWithPassthrough, BoolWithPassthroughTranslation>
        {
            public Conv() : base(() => Program.Translation.BoolWithPassthrough) { }
        }
    }
}
