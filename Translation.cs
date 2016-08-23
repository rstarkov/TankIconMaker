using RT.Util.Lingo;
using TankIconMaker.Effects;
using WpfCrutches;

namespace TankIconMaker
{
    public enum TranslationGroup
    {
        [LingoGroup("Main window", "Strings used directly in the main window interface.")]
        MainWindow,
        [LingoGroup("Add window", "Strings used directly in the \"Add layer\" and \"Add effect\" dialogs.")]
        AddWindow,
        [LingoGroup("Path template window", "Strings used directly in the \"Path template\" window.")]
        PathTemplateWindow,
        [LingoGroup("Property categories", "Strings used to group properties into categories in the property grid.")]
        PropertyCategory,
        [LingoGroup("Message box defaults", "Strings used in the message boxes by default for some of the messages.")]
        DlgMessage,
        [LingoGroup("Errors", "Various error messages.")]
        Errors,
        [LingoGroup("Prompts", "Mostly modal dialog prompts and buttons.")]
        Prompts,

        [LingoGroup("All layers/effects", "Strings used in the property grid for all layers and effects.")]
        LayerEffectAll,

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

        [LingoGroup("Effect: Clip edges", "Strings used in the property grid for the \"Clip edges\" effect.")]
        EffectClip,
        [LingoGroup("Effect: Colorize", "Strings used in the property grid for the \"Colorize\" effect.")]
        EffectColorize,
        [LingoGroup("Effect: Equalize (normalize)", "Strings used in the property grid for the \"Equalize (normalize)\" effect.")]
        EffectNormalize,
        [LingoGroup("Effect: Equalize brightness", "Strings used in the property grid for the \"Equalize brightness\" effect.")]
        EffectNormalizeBrightness,
        [LingoGroup("Effect: Flip", "Strings used in the property grid for the \"Flip\" effect.")]
        EffectFlip,
        [LingoGroup("Effect: Opacity", "Strings used in the property grid for the \"Opacity\" effect.")]
        EffectOpacity,
        [LingoGroup("Effect: Outline", "Strings used in the property grid for the \"Outline\" effect.")]
        EffectPixelOutline,
        [LingoGroup("Effect: Shadow", "Strings used in the property grid for the \"Shadow\" effect.")]
        EffectShadow,
        [LingoGroup("Effect: Shift", "Strings used in the property grid for the \"Shift\" effect.")]
        EffectShift,
        [LingoGroup("Effect: Mask layer", "Strings used in the property grid for the \"Mask layer\" effect.")]
        EffectMaskLayer,
        [LingoGroup("Effect: Size / Position", "Strings used in the property grid for the \"Size / Position\" effect.")]
        EffectSizePos,
        [LingoGroup("Effect: channels (shared)", "Strings used in the property grid for specifying the affected channels in various effects.")]
        EffectChannels,
        [LingoGroup("Effect: Brightness / Contrast", "Strings used in the property grid for the \"Brightness / Contrast\" effect.")]
        EffectBrightnessContrast,
        [LingoGroup("Effect: Hue / Saturation / Lightness", "Strings used in the property grid for the \"Hue / Saturation/ Lightness\" effect.")]
        EffectHueSaturationLightness,
        [LingoGroup("Effect: Gamma", "Strings used in the property grid for the \"Gamma\" effect.")]
        EffectGamma,
        [LingoGroup("Effect: Level", "Strings used in the property grid for the \"Level\" effect.")]
        EffectLevel,
        [LingoGroup("Effect: Invert", "Strings used in the property grid for the \"Invert\" effect.")]
        EffectInvert,
        [LingoGroup("Effect: Sharpen", "Strings used in the property grid for the \"Sharpen\" effect.")]
        EffectSharpen,
        [LingoGroup("Effect: Sharpen: adaptive", "Strings used in the property grid for the \"Sharpen: adaptive\" effect.")]
        EffectAdaptiveSharpen,
        [LingoGroup("Effect: Sharpen: unsharp mask", "Strings used in the property grid for the \"Sharpen: unsharp mask\" effect.")]
        EffectUnsharpMaskTranslation,
        [LingoGroup("Effect: Blur: Gaussian", "Strings used in the property grid for the \"Blur: Gaussian\" effect.")]
        EffectGaussianBlur,
        [LingoGroup("Effect: Blur: adaptive", "Strings used in the property grid for the \"Blur: adaptive\" effect.")]
        EffectAdaptiveBlur,
        [LingoGroup("Effect: Blur: selective", "Strings used in the property grid for the \"Blur: selective\" effect.")]
        EffectSelectiveBlur,
        [LingoGroup("Effect: Blur: motion", "Strings used in the property grid for the \"Blur: motion\" effect.")]
        EffectMotionBlur,
        [LingoGroup("Effect: Blur: radial", "Strings used in the property grid for the \"Blur: radial\" effect.")]
        EffectRadialBlur,
        [LingoGroup("Effect: Wave", "Strings used in the property grid for the \"Wave\" effect.")]
        EffectWave,
        [LingoGroup("Effect: Rotate", "Strings used in the property grid for the \"Rotate\" effect.")]
        EffectRotate,

        [LingoGroup("Selector", "Strings used in the property grid for selectors, which are expandable objects used for properties like Color, Visibility etc.")]
        Selector,
        [LingoGroup("Calculator", "Strings used in the expression calculator, mostly for error messages.")]
        Calculator,

        [LingoGroup("Value: Yes / No / Passthrough", "Strings used for the yes/no/passthrough drop-down.")]
        EnumBoolWithPassthrough,
        [LingoGroup("Value: built-in image style", "Strings used for the built-in image style drop-down.")]
        EnumImageBuiltInStyle,
        [LingoGroup("Value: Select By", "Strings used for the \"By\" drop-down.")]
        EnumSelectBy,
        [LingoGroup("Value: clip mode", "Strings used for the clip effect mode drop-down.")]
        EnumClipMode,
        [LingoGroup("Value: mask mode", "Strings used for the mask layer effect mask mode drop-down.")]
        EnumMaskMode,
        [LingoGroup("Value: size mode", "Strings used for the size/pos effect size mode drop-down.")]
        EnumSizeMode,
        [LingoGroup("Value: grow/shrink mode", "Strings used for the size/pos grow/shrink mode drop-down.")]
        EnumGrowShrinkMode,
        [LingoGroup("Value: filter", "Strings used for the size/pos filter drop-down.")]
        EnumFilter,
        [LingoGroup("Value: opacity style", "Strings used for the opacity effect style drop-down.")]
        EnumOpacityStyle,
        [LingoGroup("Value: blur edge mode", "Strings used for the blur effect edge-handling mode drop-downs.")]
        EnumBlurEdgeMode,
        [LingoGroup("Value: text smoothing mode", "Strings used for the text smoothing (anti-aliasing) mode drop-downs.")]
        EnumTextSmoothingStyle,
        [LingoGroup("Value: tank country", "Strings used for the tank country drop-downs.")]
        EnumTankCountry,
        [LingoGroup("Value: tank class", "Strings used for the tank class drop-downs.")]
        EnumTankClass,
        [LingoGroup("Value: tank availability", "Strings used for the tank availability drop-downs.")]
        EnumTankCategory,
    }

    /// <summary>WPF bindings can't access fields, so here's a hack around that, because Lingo can't access properties.</summary>
    static class WpfTranslations
    {
        public static string PropSource_Author { get { return App.Translation.Misc.PropSource_Author; } }
        public static string PropSource_TierRoman { get { return App.Translation.Misc.PropSource_TierRoman; } }
        public static string PropSource_TierArabic { get { return App.Translation.Misc.PropSource_TierArabic; } }
    }

    [LingoStringClass]
    sealed class Translation : TranslationBase
    {
        public Translation() : base(Language.EnglishUK) { }

        public TrString TranslationCredits = "English translation by Roman"; // never shown for English

        public MainWindowTranslation MainWindow = new MainWindowTranslation();
        public AddWindowTranslation AddWindow = new AddWindowTranslation();
        public PathTemplateWindowTranslation PathTemplateWindow = new PathTemplateWindowTranslation();
        public BulkSaveSettingsWindowTranslation BulkSaveSettingsWindow = new BulkSaveSettingsWindowTranslation();

        public LayerAndEffectTranslation LayerAndEffect = new LayerAndEffectTranslation();

        public BkgDarkAgentLayerTranslation BkgDarkAgentLayer = new BkgDarkAgentLayerTranslation();
        public TankImageLayerTranslation TankImageLayer = new TankImageLayerTranslation();
        public CurrentImageLayerTranslation CurrentImageLayer = new CurrentImageLayerTranslation();
        public CustomImageLayerTranslation CustomImageLayer = new CustomImageLayerTranslation();
        public FilenamePatternImageLayerTranslation FilenamePatternImageLayer = new FilenamePatternImageLayerTranslation();
        public TextLayerTranslation TextLayer = new TextLayerTranslation();
        public PropertyTextLayerTranslation PropertyTextLayer = new PropertyTextLayerTranslation();
        public CustomTextLayerTranslation CustomTextLayer = new CustomTextLayerTranslation();

        public EffectClipTranslation EffectClip = new EffectClipTranslation();
        public EffectColorizeTranslation EffectColorize = new EffectColorizeTranslation();
        public EffectNormalizeTranslation EffectNormalize = new EffectNormalizeTranslation();
        public EffectFlipTranslation EffectFlip = new EffectFlipTranslation();
        public EffectGaussianBlurTranslation EffectGaussianBlur = new EffectGaussianBlurTranslation();
        public EffectOpacityTranslation EffectOpacity = new EffectOpacityTranslation();
        public EffectPixelOutlineTranslation EffectPixelOutline = new EffectPixelOutlineTranslation();
        public EffectShadowTranslation EffectShadow = new EffectShadowTranslation();
        public EffectShiftTranslation EffectShift = new EffectShiftTranslation();
        public EffectMaskLayerTranslation EffectMaskLayer = new EffectMaskLayerTranslation();
        public EffectSizePosTranslation EffectSizePos = new EffectSizePosTranslation();
        public EffectBrightnessContrastTranslation EffectBrightnessContrast = new EffectBrightnessContrastTranslation();
        public EffectHueSaturationLightnessTranslation EffectHueSaturationLightness = new EffectHueSaturationLightnessTranslation();
        public EffectGammaTranslation EffectGamma = new EffectGammaTranslation();
        public EffectLevelTranslation EffectLevel = new EffectLevelTranslation();
        public EffectInvertTranslation EffectInvert = new EffectInvertTranslation();
        public EffectAdaptiveBlurTranslation EffectAdaptiveBlur = new EffectAdaptiveBlurTranslation();
        public EffectAdaptiveSharpenTranslation EffectAdaptiveSharpen = new EffectAdaptiveSharpenTranslation();
        public EffectSharpenTranslation EffectSharpen = new EffectSharpenTranslation();
        public EffectUnsharpMaskTranslation EffectUnsharpMask = new EffectUnsharpMaskTranslation();
        public EffectSelectiveBlurTranslation EffectSelectiveBlur = new EffectSelectiveBlurTranslation();
        public EffectMotionBlurTranslation EffectMotionBlur = new EffectMotionBlurTranslation();
        public EffectRadialBlurTranslation EffectRadialBlur = new EffectRadialBlurTranslation();
        public EffectWaveTranslation EffectWave = new EffectWaveTranslation();
        public EffectRotateTranslation EffectRotate = new EffectRotateTranslation();
        public EffectNormalizeBrightnessTranslation EffectNormalizeBrightness = new EffectNormalizeBrightnessTranslation();

        public BoolWithPassthroughTranslation BoolWithPassthrough = new BoolWithPassthroughTranslation();
        public ImageBuiltInStyleTranslation ImageBuiltInStyle = new ImageBuiltInStyleTranslation();
        public SelectByTranslation SelectBy = new SelectByTranslation();
        public ClipModeTranslation ClipMode = new ClipModeTranslation();
        public SizeModeTranslation SizeMode = new SizeModeTranslation();
        public MaskModeTranslation MaskMode = new MaskModeTranslation();
        public GrowShrinkModeTranslation GrowShrinkMode = new GrowShrinkModeTranslation();
        public FilterTranslation Filter = new FilterTranslation();
        public OpacityStyleTranslation OpacityStyle = new OpacityStyleTranslation();
        public BlurEdgeModeTranslation BlurEdgeMode = new BlurEdgeModeTranslation();
        public TextSmoothingStyleTranslation TextSmoothingStyle = new TextSmoothingStyleTranslation();

        public CategoryTranslation Category = new CategoryTranslation();

        public SelectorTranslation Selector = new SelectorTranslation();
        public DlgMessageTranslation DlgMessage = new DlgMessageTranslation();
        public PromptTranslation Prompt = new PromptTranslation();
        public ErrorTranslation Error = new ErrorTranslation();
        public MiscTranslation Misc = new MiscTranslation();
        public CalculatorTranslation Calculator = new CalculatorTranslation();
    }

    [LingoStringClass]
    sealed class MiscTranslation
    {
        public TrString ProgramVersion = "Version {0} (build {1})";

        public TrString GlobalStatus_Loading = "Loading...";
        public TrString GlobalStatus_Saving = "Saving...";

        public TrString Filter_FilenameEditor = "Image files|*.png;*.jpg;*.tga|All files|*.*";
        public TrString Filter_ImportExportStyle = "Icon styles|*.xml|All files|*.*";

        public TrString NameOfCopied = "{0} (copy)";
        public TrString NameOfNewStyle = "New style";
        public TrString NameOfTankImageLayer = "Tank image";
        [LingoNotes("New styles have the author set to this value by default. Should fit the context of \"<style name> (by <author>)\".")]
        public TrString NameOfNewStyleAuthor = "me";

        public TrString StyleDisplay_Original = "[original]";
        public TrString StyleDisplay_Current = "[current]";
        [LingoNotes("\"{0}\" is the style name; \"{1}\" is the author.")]
        public TrString StyleDisplay_BuiltIn = "[built-in] {0} (by {1})";
        [LingoNotes("\"{0}\" is the style name; \"{1}\" is the author.")]
        public TrString StyleDisplay_Normal = "{0} (by {1})";

        public TrString PropSource_Author = "Author:";
        public TrString PropSource_TierRoman = "Tier (Roman)";
        public TrString PropSource_TierArabic = "Tier (Arabic)";

        public TrString ExpandablePropertyDesc = "(expand to edit)";
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.PropertyCategory)]
    sealed class CategoryTranslation
    {
        public TrString General = "General";
        public TrString Settings = "Settings";
        public TrString Image = "Image";
        public TrString Font = "Font";
        public TrString Position = "Position";
        public TrString Size = "Size";
        public TrString Mask = "Mask";
        public TrString TextSource = "Text source";
        public TrString Debug = "Debug";
        public TrString Clip = "Clip";
        public TrString Shift = "Shift";
        public TrString Channels = "Channels";
    }

    partial class MainWindowTranslation
    {
        public TrString BackgroundCheckered = "_Checkered";
        public TrString BackgroundSolidColor = "_Solid color";
        public TrString BackgroundChangeCheckered1 = "C_hange checkered color #1...";
        public TrString BackgroundChangeCheckered2 = "Ch_ange checkered color #2...";
        public TrString BackgroundChangeSolid = "C_hange solid color...";
        public TrString BackgroundRestoreDefaults = "_Restore defaults";

        public TrString PathTemplate_Standard = "(standard)";

        public TrString ErrorConflictingId = "Two or more layers have the same ID “{0}”. Layer IDs must be unique.";
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

    partial class PathTemplateWindowTranslation
    {
        public TrString Title = "Edit Icon Path Template";
    }

    class BulkSaveSettingsWindowTranslation
    {
        public TrString ctEnabledHeader = "Enabled";
        public TrString ctModifyHeader = "Modify";
        public TrString ctPathHeader = "Path";
    }
    

    #region Layer translations

    [LingoStringClass, LingoInGroup(TranslationGroup.LayerEffectAll)]
    sealed class LayerAndEffectTranslation
    {
        public MemberDescriptionTr LayerId = new MemberDescriptionTr { DisplayName = "Identifier", Description = "Sets the identifier for use in some effects." };
        public MemberDescriptionTr LayerVisible = new MemberDescriptionTr { DisplayName = "Visible", Description = "Allows you to hide this layer temporarily without deleting it." };
        public MemberDescriptionTr LayerVisibleFor = new MemberDescriptionTr { DisplayName = "Visible for", Description = "Allows you to hide this layer for some of the tanks, depending on their properties." };
        public MemberDescriptionTr EffectVisible = new MemberDescriptionTr { DisplayName = "Visible", Description = "Allows you to hide this effect temporarily without deleting it." };
        public MemberDescriptionTr EffectVisibleFor = new MemberDescriptionTr { DisplayName = "Visible for", Description = "Allows you to hide this effect for some of the tanks, depending on their properties." };

        public MemberDescriptionTr ChannelA = new MemberDescriptionTr { DisplayName = "Alpha", Description = "Apply the effect to the alpha channel (image opacity)." };
        public MemberDescriptionTr ChannelR = new MemberDescriptionTr { DisplayName = "Red", Description = "Apply the effect to the red channel." };
        public MemberDescriptionTr ChannelG = new MemberDescriptionTr { DisplayName = "Green", Description = "Apply the effect to the green channel." };
        public MemberDescriptionTr ChannelB = new MemberDescriptionTr { DisplayName = "Blue", Description = "Apply the effect to the blue channel." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.LayerBkgDarkAgent)]
    sealed class BkgDarkAgentLayerTranslation
    {
        public TrString LayerName = "Background: Dark Agent";
        public TrString LayerDescription = "Draws a background using a glassy style inspired by Black_Spy’s icon set.";

        public MemberDescriptionTr BackColor = new MemberDescriptionTr { DisplayName = "Background color", Description = "Background color." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.LayerTankImage)]
    sealed class TankImageLayerTranslation
    {
        public TrString LayerName = "Image: standard";
        public TrString LayerDescription = "Draws one of the several types of standard images for this tank.";

        public MemberDescriptionTr Style = new MemberDescriptionTr { DisplayName = "Type", Description = "Specifies which of the standard image types to draw." };

        public TrString MissingImageWarning = "The image for this tank is missing.";
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.LayerCurrentImage)]
    sealed class CurrentImageLayerTranslation
    {
        public TrString LayerName = "Image: current";
        public TrString LayerDescription = "Draws the icon that’s currently saved in the output directory.";

        public TrString MissingImageWarning = "There is no current image for this tank.";
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.LayerCustomImage)]
    sealed class CustomImageLayerTranslation
    {
        public TrString LayerName = "Image: by properties";
        public TrString LayerDescription = "Draws an image loaded from a file whose name is selected based on tank properties.";

        public MemberDescriptionTr ImageFile = new MemberDescriptionTr { DisplayName = "Image file", Description = "Specifies a path to an image file. Relative names are allowed and are searched for first in the program directory, then in the WoT's version-specific mods directory, and then in the WoT installation directory." };

        [LingoNotes("The string \"{0}\" is replaced with the filename of the missing image. ")]
        public TrString MissingImageWarning = "The image {0} could not be found.";
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.LayerFilenamePatternImage)]
    sealed class FilenamePatternImageLayerTranslation
    {
        public TrString LayerName = "Image: by filename template";
        public TrString LayerDescription = "Draws an image loaded from a file whose name is generated by substituting various tank properties into a filename template.";

        public MemberDescriptionTr Pattern = new MemberDescriptionTr { DisplayName = "Template", Description = "Filename template. The following placeholders are available: {tier}, {country}, {class}, {category}, {id}.\n\nExtra property names are also supported, for example {NameShort}. The property name may also include the language, the author, or both, for example {NameShort/En}, {NameShort/Wargaming} or {NameShort/En/Wargaming}." };

        [LingoNotes("The string \"{0}\" is replaced with the filename of the missing image. ")]
        public TrString MissingImageWarning = "The image {0} could not be found.";
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.LayerText)]
    sealed class TextLayerTranslation
    {
        public MemberDescriptionTr FontSmoothing = new MemberDescriptionTr { DisplayName = "Smoothing", Description = "Determines how to smooth the text (anti-aliasing)." };
        public MemberDescriptionTr FontFamily = new MemberDescriptionTr { DisplayName = "Family", Description = "Font family." };
        public MemberDescriptionTr FontSize = new MemberDescriptionTr { DisplayName = "Size", Description = "Specifies the font size. If “Width” and/or “Height” are specified, this is the maximum font size; the text may be made smaller to fit." };
        public MemberDescriptionTr FontBold = new MemberDescriptionTr { DisplayName = "Bold", Description = "Makes the text bold." };
        public MemberDescriptionTr FontItalic = new MemberDescriptionTr { DisplayName = "Italic", Description = "Makes the text italic." };
        public MemberDescriptionTr FontColor = new MemberDescriptionTr { DisplayName = "Color", Description = "Specifies the text color." };
        public MemberDescriptionTr Baseline = new MemberDescriptionTr { DisplayName = "Align baselines", Description = "Consider the words “more” and “More”, top-anchored at pixel 0. If “Align baselines” is false, the word “more” will be displayed slightly higher, so as to touch pixel 0. If true, the baselines will align instead, and the topmost pixel of “more” will actually be below pixel 0." };
        public MemberDescriptionTr Anchor = new MemberDescriptionTr { DisplayName = "Anchor", Description = "Specifies where the text should be positioned relative to the point specified by the “X” and “Y” values." };
        public MemberDescriptionTr X = new MemberDescriptionTr { DisplayName = "X", Description = "Specifies the horizontal coordinate of the anchor point." };
        public MemberDescriptionTr Y = new MemberDescriptionTr { DisplayName = "Y", Description = "Specifies the vertical coordinate of the anchor point. “Baseline” specifies whether this is pixel-perfect or baseline-consistent." };
        public MemberDescriptionTr Width = new MemberDescriptionTr { DisplayName = "Width", Description = "When greater than zero, specifies that the text must not be wider than this value, in pixels. The text starts off at the specified font size, and is shrunk to fit if necessary – but never grown." };
        public MemberDescriptionTr Height = new MemberDescriptionTr { DisplayName = "Height", Description = "When greater than zero, specifies that the text must not be taller than this value, in pixels. The text starts off at the specified font size, and is shrunk to fit if necessary – but never grown." };
        public MemberDescriptionTr Format = new MemberDescriptionTr { DisplayName = "Format", Description = "A standard .NET format string to tweak the displayed value. Examples: \"{0} seconds\" appends the word \"seconds\" to the value. \"{0:0.00}\" rounds numeric values to 2 d.p. \"{0:#,0}\" adds thousands separators and rounds to whole numbers. \"{0:0000}\" pads the number with zeroes." };

        public TrString FormatStringInvalidNum = "The format string \"{0}\" is not valid, or not compatible with the numeric value \"{1}\".";
        public TrString FormatStringInvalid = "The format string \"{0}\" is not valid.";
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.LayerPropertyText)]
    sealed class PropertyTextLayerTranslation
    {
        public TrString LayerName = "Text: property";
        public TrString LayerDescription = "Draws a specified property of a tank as text.";

        public MemberDescriptionTr Property = new MemberDescriptionTr { DisplayName = "Property", Description = "Specifies the property to be used as the text source." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.LayerCustomText)]
    sealed class CustomTextLayerTranslation
    {
        public TrString LayerName = "Text: custom";
        public TrString LayerDescription = "Draws a fixed string based on a specified property of a tank.";

        public MemberDescriptionTr Text = new MemberDescriptionTr { DisplayName = "Text", Description = "The string to be displayed." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EffectClip)]
    sealed class EffectClipTranslation
    {
        public TrString EffectName = "Clip edges";
        public TrString EffectDescription = "Clips the specified edges of this layer.";

        public MemberDescriptionTr Mode = new MemberDescriptionTr { DisplayName = "Mode", Description = "Selects whether the clip boundaries are relative to the icon edges, this layer's edges, or this layer's content (pixels). See also \"Transparency threshold\"." };
        public MemberDescriptionTr PixelAlphaThreshold = new MemberDescriptionTr { DisplayName = "Transparency threshold", Description = "When clipping by pixels, determines the maximum alpha value which is still deemed as \"transparent\". Range 0..255." };
        public MemberDescriptionTr ClipLeft = new MemberDescriptionTr { DisplayName = "Left", Description = "The number of pixels to clip on the left edge of the layer." };
        public MemberDescriptionTr ClipTop = new MemberDescriptionTr { DisplayName = "Top", Description = "The number of pixels to clip on the top edge of the layer." };
        public MemberDescriptionTr ClipRight = new MemberDescriptionTr { DisplayName = "Right", Description = "The number of pixels to clip on the right edge of the layer." };
        public MemberDescriptionTr ClipBottom = new MemberDescriptionTr { DisplayName = "Bottom", Description = "The number of pixels to clip on the bottom edge of the layer." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EffectColorize)]
    sealed class EffectColorizeTranslation
    {
        public TrString EffectName = "Colorize";
        public TrString EffectDescription = "Colorizes the layer according to one of the tank properties.";

        public MemberDescriptionTr Color = new MemberDescriptionTr { DisplayName = "Color", Description = "Specifies which color to use. Use the Alpha channel to adjust the strength of the effect." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EffectNormalize)]
    sealed class EffectNormalizeTranslation
    {
        public TrString EffectName = "Equalize (normalize)";
        public TrString EffectDescription = "Normalizes the brightness or alpha channel of each image, to make them similar across all icons regardless of the original image brightness. This is the older Equalize effect and is less effective.";

        public MemberDescriptionTr Grayscale = new MemberDescriptionTr { DisplayName = "Grayscale", Description = "When enabled, the image is made grayscale (i.e. fully desaturated)" };
        public MemberDescriptionTr NormalizeBrightness = new MemberDescriptionTr { DisplayName = "Normalize brightness", Description = "When enabled, the brightness in the layer is normalized such that the brightest pixel receives the “Max brightness” value. This is achieved by adjusting the layer contrast accordingly." };
        public MemberDescriptionTr NormalizeAlpha = new MemberDescriptionTr { DisplayName = "Normalize alpha", Description = "When enabled, the alpha (opacity) in the layer is normalized such that the most opaque pixel receives the “Max alpha” value." };
        public MemberDescriptionTr MaxBrightness = new MemberDescriptionTr { DisplayName = "Max. brightness", Description = "The value to which the brightness of the brightest pixel is normalized. Normally 0 to 255. Values above 255 can be used if clipping to white is acceptable." };
        public MemberDescriptionTr MaxAlpha = new MemberDescriptionTr { DisplayName = "Max. alpha", Description = "The value to which the alpha (opacity) of the most opaque pixel is normalized. Normally 0 to 255. Values above 255 can be used if clipping to full opacity is acceptable." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EffectFlip)]
    sealed class EffectFlipTranslation
    {
        public TrString EffectName = "Flip";
        public TrString EffectDescription = "Flips the layer horizontally and/or vertically.";

        public MemberDescriptionTr FlipHorz = new MemberDescriptionTr { DisplayName = "Flip horizontally", Description = "Flips the layer horizontally, that is, swapping left and right." };
        public MemberDescriptionTr FlipVert = new MemberDescriptionTr { DisplayName = "Flip vertically", Description = "Flips the layer vertically, that is, swapping up and down." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EffectGaussianBlur)]
    sealed class EffectGaussianBlurTranslation
    {
        public TrString EffectName = "Blur: Gaussian";
        public TrString EffectDescription = "Blurs the current layer using Gaussian blur.";

        public MemberDescriptionTr Radius = new MemberDescriptionTr { DisplayName = "Radius", Description = "Blur radius. Larger values result in more blur." };
        public MemberDescriptionTr Edge = new MemberDescriptionTr { DisplayName = "Edge", Description = "Specifies how to sample around the edges: assume the image beyond the edges is transparent, wrap to the other side, or use the same pixel color that touches the edge." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EffectOpacity)]
    sealed class EffectOpacityTranslation
    {
        public TrString EffectName = "Opacity";
        public TrString EffectDescription = "Increases or decreases the layer’s opacity.";

        public MemberDescriptionTr Opacity = new MemberDescriptionTr { DisplayName = "Opacity", Description = "The opacity multiplier. Negative makes a layer more transparent, positive makes it more opaque." };
        public MemberDescriptionTr Style = new MemberDescriptionTr { DisplayName = "Style", Description = "Selects one of several different curves for adjusting the opacity. \"Auto\" uses \"Additive\" for increasing opacity and \"Move endpoint\" for decreasing." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EffectPixelOutline)]
    sealed class EffectPixelOutlineTranslation
    {
        public TrString EffectName = "Outline";
        public TrString EffectDescription = "Adds a 1 pixel outline around the image. Not suitable for layers with soft outlines.";

        public MemberDescriptionTr Color = new MemberDescriptionTr { DisplayName = "Color", Description = "Specifies which color to use. Use the Alpha channel to adjust the strength of the effect." };
        public MemberDescriptionTr Threshold = new MemberDescriptionTr { DisplayName = "Threshold", Description = "Specifies the alpha channel threshold at which the outline is to be drawn." };
        public MemberDescriptionTr Inside = new MemberDescriptionTr { DisplayName = "Inside", Description = "When unchecked, the outline is drawn on the transparent pixels, leaving pixels more opaque than the Threshold fully visible. Otherwise, the outline is drawn over the opaque pixels, leaving the transparent ones fully visible." };
        public MemberDescriptionTr KeepImage = new MemberDescriptionTr { DisplayName = "Keep image", Description = "Specifies whether to leave the image on which the outline is based, or discard it and show just the outline." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EffectShadow)]
    sealed class EffectShadowTranslation
    {
        public TrString EffectName = "Shadow";
        public TrString EffectDescription = "Adds a shadow as if cast by the current layer.";

        public MemberDescriptionTr Radius = new MemberDescriptionTr { DisplayName = "Radius", Description = "Shadow radius controls the maximum distance between a pixel producing the shadow and the edge of the resulting shadow." };
        public MemberDescriptionTr Spread = new MemberDescriptionTr { DisplayName = "Spread", Description = "Shadow spread controls the strength of the shadow, but does not affect its maximum size." };
        public MemberDescriptionTr Color = new MemberDescriptionTr { DisplayName = "Color", Description = "Shadow color. Use bright colors for glow. Adjust the Alpha channel to control final shadow transparency." };
        public MemberDescriptionTr ShiftX = new MemberDescriptionTr { DisplayName = "Shift: X", Description = "Amount of horizontal shift in pixels." };
        public MemberDescriptionTr ShiftY = new MemberDescriptionTr { DisplayName = "Shift: Y", Description = "Amount of vertical shift in pixels." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EffectShift)]
    sealed class EffectShiftTranslation
    {
        public TrString EffectName = "Shift";
        public TrString EffectDescription = "Shifts the layer by a specified number of pixels.";

        public MemberDescriptionTr ShiftX = new MemberDescriptionTr { DisplayName = "Horizontal", Description = "Horizontal shift amount, in pixels." };
        public MemberDescriptionTr ShiftY = new MemberDescriptionTr { DisplayName = "Vertical", Description = "Vertical shift amount, in pixels." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EffectBrightnessContrast)]
    sealed class EffectBrightnessContrastTranslation
    {
        public TrString EffectName = "Brightness / Contrast";
        public TrString EffectDescription = "Adjusts the brightness and contrast of the target layer.";

        public MemberDescriptionTr Brightness = new MemberDescriptionTr { DisplayName = "Brightness %", Description = "Brightness shift, from −100% to +100%. Zero means no change. Unlike with the Hue / Saturation / Lightness effect, a large brightness shift may distort the image contrast detrimentally." };
        public MemberDescriptionTr Contrast = new MemberDescriptionTr { DisplayName = "Contrast %", Description = "Contrast adjustment, from −100% to +100%. Zero means no change." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EffectHueSaturationLightness)]
    sealed class EffectHueSaturationLightnessTranslation
    {
        public TrString EffectName = "Hue / Saturation / Lightness";
        public TrString EffectDescription = "Adjusts the hue, saturation and lightness of the target layer.";

        public MemberDescriptionTr Hue = new MemberDescriptionTr { DisplayName = "Hue shift", Description = "Specifies how much the hue should be shifted, in degrees. 0, +360 and −360 mean no change, while +180 or −180 change each color to its opposite." };
        public MemberDescriptionTr Saturation = new MemberDescriptionTr { DisplayName = "Saturation %", Description = "Saturation scale, expressed as a percentage of the original saturation. 0 scales the saturation to zero (grayscale), 100 means no change, larger values scale it accordingly." };
        public MemberDescriptionTr Lightness = new MemberDescriptionTr { DisplayName = "Lightness %", Description = "Lightness scale, expressed as a percentage of the original lightness. 0 scales the lightness to zero (black), 100 means no change, larger values scale it accordingly." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EffectGamma)]
    sealed class EffectGammaTranslation
    {
        public TrString EffectName = "Gamma correction";
        public TrString EffectDescription = "Adjusts the gamma value (brightness curve) of each channel within the layer.";

        public MemberDescriptionTr GammaRed = new MemberDescriptionTr { DisplayName = "Red gamma", Description = "Gamma adjustment for the red channel. Reasonable values are between approximately 0.8 to 2.3." };
        public MemberDescriptionTr GammaGreen = new MemberDescriptionTr { DisplayName = "Green gamma", Description = "Gamma adjustment for the green channel. Reasonable values are between approximately 0.8 to 2.3." };
        public MemberDescriptionTr GammaBlue = new MemberDescriptionTr { DisplayName = "Blue gamma", Description = "Gamma adjustment for the blue channel. Reasonable values are between approximately 0.8 to 2.3." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EffectLevel)]
    sealed class EffectLevelTranslation
    {
        public TrString EffectName = "Levels";
        public TrString EffectDescription = "Remaps the brightness of all pixels according to a curve specified by three configurable points.";

        public MemberDescriptionTr BlackPoint = new MemberDescriptionTr { DisplayName = "Black Point %", Description = "All pixels with this value or less will be black." };
        public MemberDescriptionTr WhitePoint = new MemberDescriptionTr { DisplayName = "White Point %", Description = "All pixels with this value or higher will be white." };
        public MemberDescriptionTr MidPoint = new MemberDescriptionTr { DisplayName = "Mid Point", Description = "Selects output image gamma." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EffectInvert)]
    sealed class EffectInvertTranslation
    {
        public TrString EffectName = "Invert";
        public TrString EffectDescription = "Inverts the colors within the layer.";
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EffectAdaptiveSharpen)]
    sealed class EffectAdaptiveSharpenTranslation
    {
        public TrString EffectName = "Sharpen: adaptive";
        public TrString EffectDescription = "Sharpens the layer in areas that contain sharp edges. Smooth areas remain unaffected.";

        public MemberDescriptionTr Radius = new MemberDescriptionTr { DisplayName = "Radius", Description = "Specifies the processing radius. Recommended value: 0, which automatically selects the optimal radius." };
        public MemberDescriptionTr Sigma = new MemberDescriptionTr { DisplayName = "Strength", Description = "Specifies the strength of the sharpening effect. Fractional values are permitted." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EffectSharpen)]
    sealed class EffectSharpenTranslation
    {
        public TrString EffectName = "Sharpen";
        public TrString EffectDescription = "Sharpens the layer.";

        public MemberDescriptionTr Radius = new MemberDescriptionTr { DisplayName = "Radius", Description = "Specifies the processing radius. Recommended value: 0, which automatically selects the optimal radius." };
        public MemberDescriptionTr Sigma = new MemberDescriptionTr { DisplayName = "Strength", Description = "Specifies the strength of the sharpening effect. Fractional values are permitted." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EffectUnsharpMaskTranslation)]
    sealed class EffectUnsharpMaskTranslation
    {
        public TrString EffectName = "Sharpen: unsharp mask";
        public TrString EffectDescription = "Sharpens the layer using unsharp mask filter.";

        public MemberDescriptionTr Radius = new MemberDescriptionTr { DisplayName = "Radius", Description = "Specifies the processing radius. Recommended value: 0, which automatically selects the optimal radius." };
        public MemberDescriptionTr Sigma = new MemberDescriptionTr { DisplayName = "Strength", Description = "Specifies the strength of the sharpening effect. Fractional values are permitted." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EffectAdaptiveBlur)]
    sealed class EffectAdaptiveBlurTranslation
    {
        public TrString EffectName = "Blur: adaptive";
        public TrString EffectDescription = "Blurs the layer in areas that don’t contain sharp edges.";

        public MemberDescriptionTr Radius = new MemberDescriptionTr { DisplayName = "Radius", Description = "Specifies the processing radius. Recommended value: 0, which automatically selects the optimal radius." };
        public MemberDescriptionTr Sigma = new MemberDescriptionTr { DisplayName = "Strength", Description = "Specifies the strength of the blur. Higher values result in more blur. Fractional values are permitted." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EffectSelectiveBlur)]
    sealed class EffectSelectiveBlurTranslation
    {
        public TrString EffectName = "Blur: selective";
        public TrString EffectDescription = "Blurs the layer in areas that don’t contain sharp edges or high contrast.";

        public MemberDescriptionTr Radius = new MemberDescriptionTr { DisplayName = "Radius", Description = "Specifies the processing radius. Recommended value: 0, which automatically selects the optimal radius." };
        public MemberDescriptionTr Sigma = new MemberDescriptionTr { DisplayName = "Strength", Description = "Specifies the strength of the blur. Higher values result in more blur. Fractional values are permitted." };
        public MemberDescriptionTr Threshold = new MemberDescriptionTr { DisplayName = "Threshold", Description = "Specifies the contrast threshold, which determines what areas of the image are blurred." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EffectMotionBlur)]
    sealed class EffectMotionBlurTranslation
    {
        public TrString EffectName = "Blur: motion";
        public TrString EffectDescription = "Blurs the layer, simulating linear motion blur.";

        public MemberDescriptionTr Radius = new MemberDescriptionTr { DisplayName = "Radius", Description = "Specifies the processing radius. Recommended value: 0, which automatically selects the optimal radius." };
        public MemberDescriptionTr Sigma = new MemberDescriptionTr { DisplayName = "Distance", Description = "Specifies the length of the motion blur trail. Higher values result in more blur. Fractional values are permitted." };
        public MemberDescriptionTr Angle = new MemberDescriptionTr { DisplayName = "Angle", Description = "Specifies the direction of the motion blur trail, in degrees." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EffectRadialBlur)]
    sealed class EffectRadialBlurTranslation
    {
        public TrString EffectName = "Blur: radial";
        public TrString EffectDescription = "Blurs the layer, simulating rotational motion blur";

        public MemberDescriptionTr Angle = new MemberDescriptionTr { DisplayName = "Angle", Description = "Blur rotation angle, in degrees." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EffectWave)]
    sealed class EffectWaveTranslation
    {
        public TrString EffectName = "Wave";
        public TrString EffectDescription = "Distorts the layer in the shape of a wave.";

        public MemberDescriptionTr Amplitude = new MemberDescriptionTr { DisplayName = "Amplitude", Description = "Selects wave amplitude." };
        public MemberDescriptionTr Length = new MemberDescriptionTr { DisplayName = "Length", Description = "Selects wave length." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EffectRotate)]
    sealed class EffectRotateTranslation
    {
        public TrString EffectName = "Rotate";
        public TrString EffectDescription = "Rotates the layer.";

        public MemberDescriptionTr Angle = new MemberDescriptionTr { DisplayName = "Angle", Description = "Rotation angle in degrees." };
        public MemberDescriptionTr RotateX = new MemberDescriptionTr { DisplayName = "X", Description = "The X coordinate of the center of rotation." };
        public MemberDescriptionTr RotateY = new MemberDescriptionTr { DisplayName = "Y", Description = "The Y coordinate of the center of rotation." };
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EffectNormalizeBrightness)]
    sealed class EffectNormalizeBrightnessTranslation
    {
        public TrString EffectName = "Equalize brightness (improved)";
        public TrString EffectDescription = "Normalizes the brightness of each image to the specified value, so that the brightness is identical across all icons regardless of the original image brightness. This is the new and improved variant of the Equalize effect.";

        public MemberDescriptionTr Strength = new MemberDescriptionTr { DisplayName = "Strength %", Description = "Effect strength. Values below 100% do not achieve full brightness equalization and preserve the original variation to some extent." };
        public MemberDescriptionTr Brightness = new MemberDescriptionTr { DisplayName = "Brightness %", Description = "Desired average image brightness. The image brightness is adjusted so that the average brightness of all pixels is equal to this value." };
        public MemberDescriptionTr Saturation = new MemberDescriptionTr { DisplayName = "Saturation", Description = "Saturation adjustment mode. “Reduce” tones down the saturation somewhat, but only when the layer brightness is being reduced by this effect, to improve the appearance of the brighter areas of the image." };

        public SaturationModeTranslation SaturationMode = new SaturationModeTranslation();

        [LingoStringClass]
        public sealed class SaturationModeTranslation
        {
            public TrString NoChange = "No change";
            public TrString Reduce = "Reduce";
            public TrString Zero = "Zero (grayscale)";

            public class Conv : LingoEnumConverter<NormalizeBrightnessEffect.SaturationMode, SaturationModeTranslation>
            {
                public Conv() : base(() => App.Translation.EffectNormalizeBrightness.SaturationMode) { }
            }
        }
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EffectMaskLayer)]
    sealed class EffectMaskLayerTranslation
    {
        public TrString EffectName = "Mask Layer";
        public TrString EffectDescription = "Adjusts layer opacity using another layer as mask.";

        public MemberDescriptionTr MaskLayerId = new MemberDescriptionTr { DisplayName = "Mask layer ID", Description = "Sets the mask layer identifier." };
        public MemberDescriptionTr MaskMode = new MemberDescriptionTr { DisplayName = "Mode", Description = "Selects the mask layer mode." };
        public MemberDescriptionTr InvertTr = new MemberDescriptionTr { DisplayName = "Invert", Description = "Specifies whether to invert the mask or not." };

        public TrString ErrorInvalidId = "No layer with ID “{0}” found.";
        public TrString ErrorRecursiveLayerReference = "Recursive layer reference: “{0}”.";
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EffectSizePos)]
    sealed class EffectSizePosTranslation
    {
        public TrString EffectName = "Size / Position";
        public TrString EffectDescription = "Adjusts this layer's size and/or position. This effect is always applied before any other effects.";

        public MemberDescriptionTr Anchor = new MemberDescriptionTr { DisplayName = "Anchor", Description = "Specifies how to position the layer relative to the “X” and “Y” co-ordinates." };
        public MemberDescriptionTr X = new MemberDescriptionTr { DisplayName = "X", Description = "Specifies the horizontal coordinate of the anchor point." };
        public MemberDescriptionTr Y = new MemberDescriptionTr { DisplayName = "Y", Description = "Specifies the vertical coordinate of the anchor point." };

        public MemberDescriptionTr PositionByPixels = new MemberDescriptionTr { DisplayName = "Use pixels", Description = "When checked, the edge of the layer is defined by the visible pixels (see also \"Transparency threshold\"). Otherwise the complete layer size is used." };
        public MemberDescriptionTr SizeByPixels = new MemberDescriptionTr { DisplayName = "Use pixels", Description = "If checked, transparent areas on the outside of the image will be ignored in size calculations. See also \"Transparency threshold\"." };

        public MemberDescriptionTr Percentage = new MemberDescriptionTr { DisplayName = "Resize %", Description = "When Mode is \"By %\", selects the desired resize percentage." };
        public MemberDescriptionTr Width = new MemberDescriptionTr { DisplayName = "Resize to width", Description = "When Mode is one of \"By size\" modes, selects the desired width, in pixels." };
        public MemberDescriptionTr Height = new MemberDescriptionTr { DisplayName = "Resize to height", Description = "When Mode is one of \"By size\" modes, selects the desired height, in pixels." };
        public MemberDescriptionTr SizeMode = new MemberDescriptionTr { DisplayName = "Mode", Description = "Selects one of several different resize modes, which determines how the image size is calculated." };
        public MemberDescriptionTr GrowShrinkMode = new MemberDescriptionTr { DisplayName = "Grow/shrink", Description = "Specifies whether the image size is allowed to increase, decrease, or both, as a result of the resize." };
        public MemberDescriptionTr Filter = new MemberDescriptionTr { DisplayName = "Filter", Description = "Specifies the resizing filter." };

        public MemberDescriptionTr PixelAlphaThreshold = new MemberDescriptionTr { DisplayName = "Transparency threshold", Description = "When sizing or positioning by pixels, determines the maximum alpha value which is still deemed as \"transparent\". Range 0..255." };

        public MemberDescriptionTr ShowLayerBorders = new MemberDescriptionTr { DisplayName = "Show layer borders", Description = "If enabled, draws a rectangle to show the layer borders. These borders are used when \"Use pixels\" is disabled." };
        public MemberDescriptionTr ShowPixelBorders = new MemberDescriptionTr { DisplayName = "Show pixel borders", Description = "If enabled, draws a rectangle to show the pixel borders of the layer. Adjust the sensitivity using \"Transparency threshold\". These borders are used when \"Use pixels\" is enabled." };
        public MemberDescriptionTr ShowAnchor = new MemberDescriptionTr { DisplayName = "Show anchor", Description = "If enabled, draws a cross-hair at the anchor position." };
    }

    #endregion

    #region Enum translations

    [LingoStringClass, LingoInGroup(TranslationGroup.EnumBoolWithPassthrough)]
    sealed class BoolWithPassthroughTranslation
    {
        public TrString Yes = "Yes";
        public TrString No = "No";
        public TrString Passthrough = "Passthrough: to next \"Vary by\"";

        public class Conv : LingoEnumConverter<BoolWithPassthrough, BoolWithPassthroughTranslation>
        {
            public Conv() : base(() => App.Translation.BoolWithPassthrough) { }
        }
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EnumImageBuiltInStyle), LingoInGroup(TranslationGroup.LayerTankImage)]
    sealed class ImageBuiltInStyleTranslation
    {
        public TrString Contour = "Contour";
        public TrString ThreeD = "3D";
        public TrString ThreeDLarge = "3D (large)";
        public TrString Country = "Country";
        public TrString Class = "Class";

        public class Conv : LingoEnumConverter<ImageBuiltInStyle, ImageBuiltInStyleTranslation>
        {
            public Conv() : base(() => App.Translation.ImageBuiltInStyle) { }
        }
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EnumSelectBy)]
    sealed class SelectByTranslation
    {
        public TrString Class = "Class (Artillery • Destroyer • Light • etc)";
        public TrString Country = "Country (USSR • Germany • USA • etc)";
        public TrString Category = "Availability (normal • premium • special)";
        public TrString Tier = "Tier (1 .. 10)";
        public TrString Single = "Single value";

        public class Conv : LingoEnumConverter<SelectBy, SelectByTranslation>
        {
            public Conv() : base(() => App.Translation.SelectBy) { }
        }
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EnumClipMode), LingoInGroup(TranslationGroup.EffectClip)]
    sealed class ClipModeTranslation
    {
        public TrString ByPixels = "By pixels";
        public TrString ByLayerBounds = "By layer bounds";
        public TrString ByIconBounds = "By icon bounds";

        public class Conv : LingoEnumConverter<ClipMode, ClipModeTranslation>
        {
            public Conv() : base(() => App.Translation.ClipMode) { }
        }
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EnumMaskMode), LingoInGroup(TranslationGroup.EffectMaskLayer)]
    sealed class MaskModeTranslation
    {
        public TrString Combined = "Combined";
        public TrString Opacity = "Opacity";
        public TrString Grayscale = "Grayscale";

        public class Conv : LingoEnumConverter<MaskMode, MaskModeTranslation>
        {
            public Conv() : base(() => App.Translation.MaskMode) { }
        }
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EnumSizeMode), LingoInGroup(TranslationGroup.EffectSizePos)]
    sealed class SizeModeTranslation
    {
        public TrString NoChange = "No change";
        public TrString ByPercentage = "By %";
        public TrString BySizeWidthOnly = "By size: width";
        public TrString BySizeHeightOnly = "By size: height";
        public TrString BySizeFit = "By size: fit";
        public TrString BySizeStretch = "By size: stretch";

        public class Conv : LingoEnumConverter<SizeMode2, SizeModeTranslation>
        {
            public Conv() : base(() => App.Translation.SizeMode) { }
        }
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EnumGrowShrinkMode), LingoInGroup(TranslationGroup.EffectSizePos)]
    sealed class GrowShrinkModeTranslation
    {
        public TrString GrowAndShrink = "Grow and shrink";
        public TrString GrowOnly = "Grow only";
        public TrString ShrinkOnly = "Shrink only";

        public class Conv : LingoEnumConverter<GrowShrinkMode, GrowShrinkModeTranslation>
        {
            public Conv() : base(() => App.Translation.GrowShrinkMode) { }
        }
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EnumFilter), LingoInGroup(TranslationGroup.EffectSizePos)]
    sealed class FilterTranslation
    {
        public TrString Auto = "Auto";
        public TrString Lanczos = "Lanczos";
        public TrString Mitchell = "Mitchell";
        public TrString Bicubic = "Bicubic";
        public TrString Sinc256 = "Sinc256";
        public TrString Sinc1024 = "Sinc1024";

        public class Conv : LingoEnumConverter<Filter, FilterTranslation>
        {
            public Conv() : base(() => App.Translation.Filter) { }
        }
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EnumOpacityStyle), LingoInGroup(TranslationGroup.EffectOpacity)]
    sealed class OpacityStyleTranslation
    {
        public TrString Auto = "Auto";
        public TrString MoveEndpoint = "Move endpoint";
        public TrString MoveMidpoint = "Move midpoint";
        public TrString Additive = "Additive";

        public class Conv : LingoEnumConverter<OpacityStyle, OpacityStyleTranslation>
        {
            public Conv() : base(() => App.Translation.OpacityStyle) { }
        }
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EnumBlurEdgeMode), LingoInGroup(TranslationGroup.EffectGaussianBlur)]
    sealed class BlurEdgeModeTranslation
    {
        public TrString Transparent = "Transparent pixels";
        public TrString Same = "Nearest pixel color";
        public TrString Mirror = "Mirror along edge";
        public TrString Wrap = "Wrap to opposite edge";

        public class Conv : LingoEnumConverter<BlurEdgeMode, BlurEdgeModeTranslation>
        {
            public Conv() : base(() => App.Translation.BlurEdgeMode) { }
        }
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.EnumTextSmoothingStyle), LingoInGroup(TranslationGroup.LayerText)]
    sealed class TextSmoothingStyleTranslation
    {
        public TrString Aliased = "Aliased";
        public TrString AntiAliasGDI = "Anti-aliased (hinted)";
        public TrString UnhintedGDI = "Anti-aliased (unhinted)";
        public TrString ClearType = "ClearType";

        public class Conv : LingoEnumConverter<TextSmoothingStyle, TextSmoothingStyleTranslation>
        {
            public Conv() : base(() => App.Translation.TextSmoothingStyle) { }
        }
    }

    #endregion

    [LingoStringClass, LingoInGroup(TranslationGroup.Errors)]
    sealed class ErrorTranslation
    {
        public TrString DataMissing_NoWotInstallation = "Failed to detect a supported World of Tanks installation at this path.";
        public TrString DataMissing_WotVersionTooOld = "This version of World of Tanks ({0}) is not supported because no suitable configuration files are available.";
        public TrString DataMissing_NoInstallationSelected = "Select a game installation above or add a new one using the green [+] button.";
        public TrString DataMissing_DirNotFound = "This directory does not exist.";

        public TrString RenderWithErrors = "Some of the tank icons did not render correctly; make sure you view \"All tanks\" and click each broken image for details.";
        public TrString RenderWithWarnings = "Some of the tank icons rendered with warnings; make sure you view \"All tanks\" and click each image with a warning icon for details.";
        public TrString RenderIconOK = "This icon rendered without any problems.";
        public TrString RenderIconFail = "Could not render this icon: {0}";

        public TrString ClipboardError = "Could not copy to clipboard: another application is probably using the clipboard right now.\n\nTry again?";
        public TrString ClipboardError_Retry = "&Retry";

        public TrString ExceptionGlobal = "An error has occurred. This is a bug in the program.\n\nPlease send an error report to the programmer so that this can be fixed.";
        public TrString ExceptionLoadingGameData = "Could not load game data. This is probably a bug in the program.\n\nPlease send an error report to the programmer so that this can be fixed.";
        public TrString ExceptionInRender = "A layer or an effect threw an exception while rendering this image. This is a bug in the program; please report it.";
        public TrString ErrorToClipboard_Copy = "Copy report to &clipboard";
        [LingoNotes("The button that acknowledges the error report and closes it without copying to clipboard.")]
        public TrString ErrorToClipboard_OK = "OK";
        public TrString ErrorToClipboard_Copied = "Information about the error is now in your clipboard.";
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.Prompts)]
    sealed class PromptTranslation
    {
        public TrString RenameLayer_Title = "Rename layer";
        public TrString RenameLayer_Label = "Layer _name:";
        public TrString RenameEffect_Title = "Rename effect";
        public TrString RenameEffect_Label = "Effect _name:";
        public TrString CreateStyle_Title = "Create style";
        public TrString CreateStyle_Label = "New style _name:";
        public TrString RenameStyle_Title = "Change style name";
        public TrString RenameStyle_Label = "New style _name:";
        public TrString ChangeAuthor_Title = "Change style author";
        public TrString ChangeAuthor_Label = "New style _author:";
        public TrString DuplicateStyle_Title = "Duplicate style";
        public TrString DuplicateStyle_Label = "New style _name:";
        public TrString IconDims_Title = "Icon dimensions";
        public TrString IconDims_Width = "Please enter the width of the icons, in pixels:";
        public TrString IconDims_Height = "Please enter the height of the icons, in pixels:";
        public TrString IconDims_NumberError = "Please enter a whole number greater than zero.";
        public TrString Centerable_Prompt = "Please select one of the two icon width modes:\n\n• Centerable icons can be horizontally centered above tanks. The width of the saved icons varies accordingly.\n• Fixed width icons all have the same width, but will not center correctly unless the tank image is already centered within the icon boundaries.\n\nNote that this setting has no visible effect within Tank Icon Maker, because all icons are left-aligned. The effect will be visible in-game.\n\nCurrent width mode selected for this style: {0}.";
        public TrString Centerable_Yes = "Centerable";
        public TrString Centerable_No = "Fixed-width";
        public TrString ExportFormat_Title = "Export styles";
        public TrString ExportFormat_Label = "File name template (subfolders supported):";

        [LingoNotes("A generic Cancel button text used in some modal dialogs to cancel whatever action is being done without making any changes. Do not use hotkeys (because the required prefix varies).")]
        public TrString Cancel = "Cancel";
        public TrString PromptWindowOK = "_OK";
        [LingoNotes("\"{1}\" is replaced with the extension of the image files being saved.")]
        public TrString OverwriteIcons_Prompt = "Would you like to overwrite your current icons?\n\nPath: {0}\n\nWarning: ALL {1} files in this path will be overwritten, and there is NO UNDO for this!";
        public TrString OverwriteIcons_Yes = "&Yes, overwrite all files";
        public TrString GameNotFound_Prompt = "This directory does not appear to contain a supported version of World Of Tanks. Are you sure you want to use it anyway?";
        public TrString GameNotFound_Ignore = "&Use anyway";
        [LingoNotes("The save path is substituted for \"{0}\".")]
        public TrString IconsSaved = "Icons saved to “{0}”!\n\n• Icons with text may show mirrored.\n• The game may overlay tank tiers on top of your icons.\n\nTo find out how these issues can be fixed please refer to the Tank Icon Maker website.";
        public TrString IconsSavedGoToForum = "Open &website in browser";
        public TrStringNum IconsSaveSkipped = new TrStringNum("Note that 1 image was skipped due to errors.", "Note that {0} images were skipped due to errors.");
        public TrString IconsSaveError = "The icons could not be saved due to an error.\n\nError message:\n • {0}";
        public TrString VisitWebsiteBtn = "Visit project website";
        public TrString Upvote_BuiltInOnly = "For security reasons, only built-in styles can be upvoted.";
        public TrString Upvote_NotAvailable = "This style does not currently have an associated post on World of Tanks forums.";
        public TrString Upvote_Prompt = "To thank {0} for designing this style, please upvote the following topic on the World of Tanks forum:\n\n{1}";
        public TrString Upvote_Open = "&Open in browser";
        public TrString PasteLayerEffect_Error = "Cannot paste layer or effect. If you are pasting raw XML, there is probably an error in it.";

        public TrString DeleteLayerEffect_Prompt = "Delete the selected layer/effect?";
        public TrString DeleteLayerEffect_Yes = "&Delete";
        public TrString BulkStyles_ColumnTitle = "Style";
        public TrString BulkStyles_PathColumn = "Icon save path";
        public TrString DeleteStyle_Prompt = "Select styles to delete:";
        public TrString DeleteStyle_Yes = "_Delete";
        public TrString DeleteStyle_PromptSure = "Are you sure you wish to delete these styles?";
        public TrStringNum DeleteStyle_Success = new TrStringNum("Style deleted.", "{0} styles deleted.");
        public TrString StyleImport_Fail = "Could not load the file for some reason. It might be of the wrong format.";
        public TrString StyleExport_Prompt = "Select styles to export:";
        public TrString StyleExport_Yes = "_Export";
        public TrStringNum StyleExport_Success = new TrStringNum("The style has been exported.", "{0} styles have been exported.");
        public TrString BulkSave_Prompt = "You are about to save tank icons for multiple styles. Any existing icons will be overwritten without further confirmation!\n\nSelect styles you wish to save the icons for:";
        public TrString BulkSave_Yes = "_Save";
        public TrString BulkSave_Progress = "Saving icons...";
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.DlgMessage)]
    sealed class DlgMessageTranslation
    {
        public TrString OK = "&OK";
        public TrString CaptionInfo = "Information";
        public TrString CaptionQuestion = "Question";
        public TrString CaptionWarning = "Warning";
        public TrString CaptionError = "Error";
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.Selector)]
    sealed class SelectorTranslation
    {
        [LingoNotes("It is very important for the usability of these properties that this property is sorted to the top. The only way to achieve that at the moment is by prefixing it with a space...")]
        public TrString By = " Vary by";
        public TrString ByN = " Vary by (#{0})";
        public TrString By_Color_Description = "Specifies a tank property by which the color is varied. The colors for any tanks whose color is set to transparent black (#00000000) are varied according to \"Vary by (#{0})\" instead.";
        public TrString By_Color_DescriptionLast = "Specifies a tank property by which the color is varied.";
        public TrString By_Bool_Description = "Specifies a tank property by which the setting is varied. The setting for any tanks whose setting is set to \"Passthrough\" are varied according to \"Vary by (#{0})\" instead.";
        public TrString By_Bool_DescriptionLast = "Specifies a tank property by which the setting is varied.";
        public TrString By_String_Description = "Specifies a tank property by which the text is varied. The text for any tanks whose text is blank is varied according to \"Vary by (#{0})\" instead.";
        public TrString By_String_DescriptionLast = "Specifies a tank property by which the text is varied.";
        public TrString By_Filename_Description = "Specifies a tank property by which the filename is varied. The filename for any tanks whose filename is blank is varied according to \"Vary by (#{0})\" instead.";
        public TrString By_Filename_DescriptionLast = "Specifies a tank property by which the filename is varied.";

        public TrString ClassLight = "Class: Light tank";
        public TrString ClassMedium = "Class: Medium tank";
        public TrString ClassHeavy = "Class: Heavy tank";
        public TrString ClassDestroyer = "Class: Destroyer";
        public TrString ClassArtillery = "Class: Artillery";
        public TrString ClassNone = "Class:  None";

        public TrString CountryUSSR = "Country: USSR";
        public TrString CountryGermany = "Country: Germany";
        public TrString CountryUSA = "Country: USA";
        public TrString CountryFrance = "Country: France";
        public TrString CountryChina = "Country: China";
        public TrString CountryUK = "Country: UK";
        public TrString CountryJapan = "Country: Japan";
        public TrString CountryCzech = "Country: Czech";
        public TrString CountrySweden = "Country: Sweden";
        public TrString CountryNone = "Country:  None";

        public TrString CategNormal = "Avail.: Normal";
        public TrString CategPremium = "Avail.: Premium";
        public TrString CategSpecial = "Avail.: Special";

        public TrString Single = "Not varied";
        public TrString SingleDescription = "When \"Vary by\" is set to \"No variation\", specifies the value to use. Use this when the value does not need to vary by tank properties.";

        public TrString TierN = "Tier: {0,2}";
        public TrString TierNone = "Tier: None";
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.Calculator)]
    sealed class CalculatorTranslation
    {
        public TrString CouldNotParseExpression = "could not parse expression";

        public TrString ErrLabel_Error = "Error";
        public TrString ErrLabel_Expression = "Expression";
        public TrString ErrLabel_Layer = "Layer";
        public TrString ErrLabel_Effect = "Effect";
        public TrString ErrLabel_Property = "Property";

        public TrString Err_LocationMarker = "HERE";
        public TrString Err_ExpectedEndOfExpression = "expected end of expression";
        public TrString Err_UnexpectedEndOfExpression = "unexpected end of expression";
        public TrString Err_ExpectedParenthesisOrOperator = "expected “)” or operator";
        public TrString Err_ExpectedCommaOrParenthesis = "expected “,” or “)”";
        public TrString Err_UnexpectedCharacter = "unexpected character: “{0}”";
        public TrString Err_CannotParseNumber = "cannot parse number: “{0}”";
        public TrString Err_UnknownVariable = "unknown variable: “{0}”";
        public TrString Err_UnknownFunction = "unknown function: “{0}”";
        public TrString Err_UnknownLayerProperty = "unknown layer property: “{0}”";
        public TrString Err_ResultInfinite = "calculation result is infinite (division by zero?)";
        public TrString Err_ResultNaN = "calculation result is not-a-number (zero divided by zero?)";
        public TrStringNum Err_FunctionParamCountExact = new TrStringNum(new[] { "function “{0}” requires {1} parameter; got {2}", "function “{0}” requires {1} parameters; got {2}" }, new[] { false, true, false });
        public TrStringNum Err_FunctionParamCountAtLeast = new TrStringNum(new[] { "function “{0}” requires at least {1} parameter; got {2}", "function “{0}” requires at least {1} parameters; got {2}" }, new[] { false, true, false });
        public TrStringNum Err_FunctionParamCountAtMost = new TrStringNum(new[] { "function “{0}” requires at most {1} parameter; got {2}", "function “{0}” requires at most {1} parameters; got {2}" }, new[] { false, true, false });
        public TrString Err_NoLayerWithId = "unknown layer ID: “{0}”";
        public TrString Err_RecursiveLayerReference = "recursive layer reference: “{0}”";
    }
}
