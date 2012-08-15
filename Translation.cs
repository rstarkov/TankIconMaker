﻿using RT.Util.Lingo;

namespace TankIconMaker
{
    public enum TranslationGroup
    {
        [LingoGroup("Main window", "Strings used directly in the main window interface.")]
        MainWindow,
        [LingoGroup("Add window", "Strings used directly in the \"Add layer\" and \"Add effect\" dialogs.")]
        AddWindow,
        [LingoGroup("Property categories", "Strings used to group properties into categories in the property grid.")]
        PropertyCategory,

        [LingoGroup("Layer: all", "Strings used in the property grid for all layers.")]
        LayerAll,
        [LingoGroup("Layer: Background / Dark Agent", "Strings used in the property grid for the 'Background / Dark Agent' layer.")]
        LayerBkgDarkAgent,
        [LingoGroup("Layer: Image / Standard", "Strings used in the property grid for the 'Image / Standard' layer.")]
        LayerTankImage,
        [LingoGroup("Layer: Image / Current", "Strings used in the property grid for the 'Image / Current' layer.")]
        LayerCurrentImage,
        [LingoGroup("Layer: Image / By properties", "Strings used in the property grid for the 'Image / By properties' layer.")]
        LayerCustomImage,
        [LingoGroup("Layer: Image / By filename pattern", "Strings used in the property grid for the 'Image / By filename pattern' layer.")]
        LayerFilenamePatternImage,
        [LingoGroup("Layer: Text (all)", "Strings used in the property grid for all text layers.")]
        LayerText,
        [LingoGroup("Layer: Text / Property", "Strings used in the property grid for the 'Text / Property' layer.")]
        LayerPropertyText,
        [LingoGroup("Layer: Text / Custom", "Strings used in the property grid for the 'Text / Custom' layer.")]
        LayerCustomText,

        [LingoGroup("Value: Yes / No / Passthrough", "Strings used for the yes/no/passthrough drop-down.")]
        BoolWithPassthrough,
        [LingoGroup("Value: Select By", "Strings used for the \"By\" drop-down.")]
        SelectBy,
    }

    [LingoStringClass]
    sealed class Translation : TranslationBase
    {
        public Translation() : base(Language.EnglishUK) { }

        public MainWindowTranslation MainWindow = new MainWindowTranslation();
        public AddWindowTranslation AddWindow = new AddWindowTranslation();

        public LayerTranslation Layer = new LayerTranslation();
        public BkgDarkAgentLayerTranslation BkgDarkAgentLayer = new BkgDarkAgentLayerTranslation();
        public TankImageLayerTranslation TankImageLayer = new TankImageLayerTranslation();
        public CurrentImageLayerTranslation CurrentImageLayer = new CurrentImageLayerTranslation();
        public CustomImageLayerTranslation CustomImageLayer = new CustomImageLayerTranslation();
        public FilenamePatternImageLayerTranslation FilenamePatternImageLayer = new FilenamePatternImageLayerTranslation();
        public TextLayerTranslation TextLayer = new TextLayerTranslation();
        public PropertyTextLayerTranslation PropertyTextLayer = new PropertyTextLayerTranslation();
        public CustomTextLayerTranslation CustomTextLayer = new CustomTextLayerTranslation();

        public BoolWithPassthroughTranslation BoolWithPassthrough = new BoolWithPassthroughTranslation();
        public ImageBuiltInStyleTranslation ImageBuiltInStyle = new ImageBuiltInStyleTranslation();
        public SelectByTranslation SelectBy = new SelectByTranslation();

        [LingoInGroup(TranslationGroup.PropertyCategory)]
        public TrString CategoryGeneral = "General";
        [LingoInGroup(TranslationGroup.PropertyCategory)]
        public TrString CategorySettings = "Settings";
        [LingoInGroup(TranslationGroup.PropertyCategory)]
        public TrString CategoryImage = "Image";
        [LingoInGroup(TranslationGroup.PropertyCategory)]
        public TrString CategoryFont = "Font";
        [LingoInGroup(TranslationGroup.PropertyCategory)]
        public TrString CategoryPosition = "Position";
        [LingoInGroup(TranslationGroup.PropertyCategory)]
        public TrString CategoryTextSource = "Text source";
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.AddWindow)]
    sealed class AddWindowTranslation
    {
        public TrString AddLayerTitle = "Add layer";
        public TrString LayerName = "Layer _name:";
        public TrString LayerType = "Layer _type:";

        public TrString AddEffectTitle = "Add effect";
        public TrString EffectName = "Effect _name:";
        public TrString EffectType = "Effect _type:";

        public TrString BtnAdd = "_Add";
        public TrString BtnCancel = "_Cancel";
    }

    #region Layer translations

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

    [LingoStringClass, LingoInGroup(TranslationGroup.LayerTankImage)]
    sealed class TankImageLayerTranslation
    {
        public TrString LayerName = "Image / Standard";
        public TrString LayerDescription = "Draws one of the several types of standard images for this tank.";

        public MemberDescriptionTr Style = new MemberDescriptionTr { DisplayName = "Type", Description = "Specifies which of the standard image types to draw." };

        public TrString MissingImageWarning = "The image for this tank is missing.";
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.LayerCurrentImage)]
    sealed class CurrentImageLayerTranslation
    {
        public TrString LayerName = "Image / Current";
        public TrString LayerDescription = "Draws the icon that’s currently saved in the output directory.";

        public TrString MissingImageWarning = "There is no current image for this tank.";
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.LayerCustomImage)]
    sealed class CustomImageLayerTranslation
    {
        public TrString LayerName = "Image / By properties";
        public TrString LayerDescription = "Draws an arbitrary, user-supplied image.";

        [LingoNotes("The string \"{0}\" is replaced with the filename of the missing image. ")]
        public TrString MissingImageWarning = "The image {0} could not be found.";
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.LayerFilenamePatternImage)]
    sealed class FilenamePatternImageLayerTranslation
    {
        public TrString LayerName = "Image / By filename pattern";
        public TrString LayerDescription = "Draws an arbitrary, user-supplied image, loaded from a file whose name is generated by substituting various tank properties into placeholders.";

        public MemberDescriptionTr Pattern = new MemberDescriptionTr { DisplayName = "Pattern", Description = "Filename pattern. Use the following placeholders: {tier}, {country}, {class}, {category}, {id}." };

        [LingoNotes("The string \"{0}\" is replaced with the filename of the missing image. ")]
        public TrString MissingImageWarning = "The image {0} could not be found.";
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.LayerText)]
    sealed class TextLayerTranslation
    {
        public MemberDescriptionTr FontSmoothing = new MemberDescriptionTr { DisplayName = "Smoothing", Description = "Determines how the text should be smoothed." };
        public MemberDescriptionTr FontFamily = new MemberDescriptionTr { DisplayName = "Family", Description = "Font family." };
        public MemberDescriptionTr FontSize = new MemberDescriptionTr { DisplayName = "Size", Description = "Font size." };
        public MemberDescriptionTr FontBold = new MemberDescriptionTr { DisplayName = "Bold", Description = "Makes the text bold." };
        public MemberDescriptionTr FontItalic = new MemberDescriptionTr { DisplayName = "Italic", Description = "Makes the text italic." };
        public MemberDescriptionTr FontColor = new MemberDescriptionTr { DisplayName = "Color", Description = "Specifies the text color." };
        public MemberDescriptionTr Left = new MemberDescriptionTr { DisplayName = "Left", Description = "X coordinate of the leftmost text pixel. Ignored if \"Left anchor\" is false." };
        public MemberDescriptionTr Right = new MemberDescriptionTr { DisplayName = "Right", Description = "X coordinate of the rightmost text pixel. Ignored if \"Right anchor\" is false." };
        public MemberDescriptionTr Top = new MemberDescriptionTr { DisplayName = "Top", Description = "Y coordinate of the topmost text pixel (but see also \"Align baselines\"). Ignored if \"Top anchor\" is false." };
        public MemberDescriptionTr Bottom = new MemberDescriptionTr { DisplayName = "Bottom", Description = "Y coordinate of the bottommost text pixel (but see also \"Align baselines\"). Ignored if \"Bottom anchor\" is false." };
        public MemberDescriptionTr LeftAnchor = new MemberDescriptionTr { DisplayName = "Left anchor", Description = "If true, the leftmost pixel of the text is anchored at the X coordinate specified by \"Left\". If \"Right anchor\" is also true, the text is centered between \"Left\" and \"Right\"." };
        public MemberDescriptionTr RightAnchor = new MemberDescriptionTr { DisplayName = "Right anchor", Description = "If true, the rightmost pixel of the text is anchored at the X coordinate specified by \"Right\". If \"Left anchor\" is also true, the text is centered between \"Left\" and \"Right\"." };
        public MemberDescriptionTr TopAnchor = new MemberDescriptionTr { DisplayName = "Top anchor", Description = "If true, the topmost pixel of the text is anchored at the Y coordinate specified by \"Top\". If \"Bottom anchor\" is also true, the text is centered between \"Top\" and \"Bottom\"." };
        public MemberDescriptionTr BottomAnchor = new MemberDescriptionTr { DisplayName = "Bottom anchor", Description = "If true, the bottommost pixel of the text is anchored at the Y coordinate specified by \"Bottom\". If \"Top anchor\" is also true, the text is centered between \"Top\" and \"Bottom\"." };
        public MemberDescriptionTr Baseline = new MemberDescriptionTr { DisplayName = "Align baselines", Description = "Consider the words \"more\" and \"type\", top-anchored at pixel 0. If \"Align baselines\" is false, the word \"more\" will be displayed slightly higher, so as to touch pixel 0. If true, the baselines will align instead, and the topmost pixel of \"more\" will actually be below pixel 0." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.LayerPropertyText)]
    sealed class PropertyTextLayerTranslation
    {
        public TrString LayerName = "Text / Property";
        public TrString LayerDescription = "Draws a specified property of a tank as text.";

        public MemberDescriptionTr Property = new MemberDescriptionTr { DisplayName = "Property", Description = "Specifies the property to be used as the text source." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.LayerCustomText)]
    sealed class CustomTextLayerTranslation
    {
        public TrString LayerName = "Text / Custom";
        public TrString LayerDescription = "Draws a fixed string based on a specified property of a tank.";

        public MemberDescriptionTr Text = new MemberDescriptionTr { DisplayName = "Text", Description = "The string to be displayed." };
    }

    #endregion

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

    [LingoStringClass, LingoInGroup(TranslationGroup.LayerTankImage)]
    sealed class ImageBuiltInStyleTranslation
    {
        public TrString Contour = "Contour";
        public TrString ThreeD = "3D";
        public TrString ThreeDLarge = "3D (large)";
        public TrString Country = "Country";
        public TrString Class = "Class";

        public class Conv : LingoEnumConverter<ImageBuiltInStyle, ImageBuiltInStyleTranslation>
        {
            public Conv() : base(() => Program.Translation.ImageBuiltInStyle) { }
        }
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.SelectBy)]
    sealed class SelectByTranslation
    {
        public TrString Class = "Artillery • Destroyer • Light • etc";
        public TrString Country = "USSR • Germany • USA • etc";
        public TrString Category = "Normal • premium • special";
        public TrString Tier = "Tier (1 .. 10)";
        public TrString Single = "Single value";

        public class Conv : LingoEnumConverter<SelectBy, SelectByTranslation>
        {
            public Conv() : base(() => Program.Translation.SelectBy) { }
        }
    }
}
