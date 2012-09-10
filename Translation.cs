﻿using RT.Util.Lingo;
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
        [LingoGroup("Effect: Flip", "Strings used in the property grid for the \"Flip\" effect.")]
        EffectFlip,
        [LingoGroup("Effect: Blur: Gaussian", "Strings used in the property grid for the \"Blur: Gaussian\" effect.")]
        EffectGaussianBlur,
        [LingoGroup("Effect: Opacity", "Strings used in the property grid for the \"Opacity\" effect.")]
        EffectOpacity,
        [LingoGroup("Effect: Outline", "Strings used in the property grid for the \"Outline\" effect.")]
        EffectPixelOutline,
        [LingoGroup("Effect: Shadow", "Strings used in the property grid for the \"Shadow\" effect.")]
        EffectShadow,
        [LingoGroup("Effect: Shift", "Strings used in the property grid for the \"Shift\" effect.")]
        EffectShift,
        [LingoGroup("Effect: Size / Position", "Strings used in the property grid for the \"Size / Position\" effect.")]
        EffectSizePos,

        [LingoGroup("Selector", "Strings used in the property grid for selectors, which are expandable objects used for properties like Color, Visibility etc.")]
        Selector,

        [LingoGroup("Value: Yes / No / Passthrough", "Strings used for the yes/no/passthrough drop-down.")]
        EnumBoolWithPassthrough,
        [LingoGroup("Value: built-in image style", "Strings used for the built-in image style drop-down.")]
        EnumImageBuiltInStyle,
        [LingoGroup("Value: Select By", "Strings used for the \"By\" drop-down.")]
        EnumSelectBy,
        [LingoGroup("Value: clip mode", "Strings used for the clip effect mode drop-down.")]
        EnumClipMode,
        [LingoGroup("Value: size mode", "Strings used for the size/pos effect size mode drop-down.")]
        EnumSizeMode,
        [LingoGroup("Value: grow/shrink mode", "Strings used for the size/pos grow/shrink mode drop-down.")]
        EnumGrowShrinkMode,
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

        public TrString TranslationCredits = "English translation by Romkyns"; // never shown for English

        public MainWindowTranslation MainWindow = new MainWindowTranslation();
        public AddWindowTranslation AddWindow = new AddWindowTranslation();

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
        public EffectFlipTranslation EffectFlip = new EffectFlipTranslation();
        public EffectGaussianBlurTranslation EffectGaussianBlur = new EffectGaussianBlurTranslation();
        public EffectOpacityTranslation EffectOpacity = new EffectOpacityTranslation();
        public EffectPixelOutlineTranslation EffectPixelOutline = new EffectPixelOutlineTranslation();
        public EffectShadowTranslation EffectShadow = new EffectShadowTranslation();
        public EffectShiftTranslation EffectShift = new EffectShiftTranslation();
        public EffectSizePosTranslation EffectSizePos = new EffectSizePosTranslation();

        public BoolWithPassthroughTranslation BoolWithPassthrough = new BoolWithPassthroughTranslation();
        public ImageBuiltInStyleTranslation ImageBuiltInStyle = new ImageBuiltInStyleTranslation();
        public SelectByTranslation SelectBy = new SelectByTranslation();
        public ClipModeTranslation ClipMode = new ClipModeTranslation();
        public SizeModeTranslation SizeMode = new SizeModeTranslation();
        public GrowShrinkModeTranslation GrowShrinkMode = new GrowShrinkModeTranslation();
        public OpacityStyleTranslation OpacityStyle = new OpacityStyleTranslation();
        public BlurEdgeModeTranslation BlurEdgeMode = new BlurEdgeModeTranslation();
        public TextSmoothingStyleTranslation TextSmoothingStyle = new TextSmoothingStyleTranslation();

        public CategoryTranslation Category = new CategoryTranslation();

        public SelectorTranslation Selector = new SelectorTranslation();
        public DlgMessageTranslation DlgMessage = new DlgMessageTranslation();
        public PromptTranslation Prompt = new PromptTranslation();
        public ErrorTranslation Error = new ErrorTranslation();
        public MiscTranslation Misc = new MiscTranslation();
    }

    [LingoStringClass]
    sealed class MiscTranslation
    {
        public TrString ProgramVersion = "Version {0}";

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
        public TrString TextSource = "Text source";
        public TrString Debug = "Debug";
        public TrString Clip = "Clip";
        public TrString Blur = "Blur";
        public TrString Shift = "Shift";
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

    [LingoStringClass, LingoInGroup(TranslationGroup.LayerEffectAll)]
    sealed class LayerAndEffectTranslation
    {
        public MemberDescriptionTr LayerVisible = new MemberDescriptionTr { DisplayName = "Visible", Description = "Allows you to hide this layer temporarily without deleting it." };
        public MemberDescriptionTr LayerVisibleFor = new MemberDescriptionTr { DisplayName = "Visible for", Description = "Allows you to hide this layer for some of the tanks, depending on their properties." };
        public MemberDescriptionTr EffectVisible = new MemberDescriptionTr { DisplayName = "Visible", Description = "Allows you to hide this effect temporarily without deleting it." };
        public MemberDescriptionTr EffectVisibleFor = new MemberDescriptionTr { DisplayName = "Visible for", Description = "Allows you to hide this effect for some of the tanks, depending on their properties." };
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
        public TrString LayerDescription = "Draws an image loaded from a file whose name is selected based on tank properties.";

        public MemberDescriptionTr ImageFile = new MemberDescriptionTr { DisplayName = "Image file", Description = "Specifies a path to an image file. Relative names are allowed and are searched for first in the program directory, then in the WoT's version-specific mods directory, and then in the WoT install directory." };

        [LingoNotes("The string \"{0}\" is replaced with the filename of the missing image. ")]
        public TrString MissingImageWarning = "The image {0} could not be found.";
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.LayerFilenamePatternImage)]
    sealed class FilenamePatternImageLayerTranslation
    {
        public TrString LayerName = "Image / By filename template";
        public TrString LayerDescription = "Draws an image loaded from a file whose name is generated by substituting various tank properties into a filename template.";

        public MemberDescriptionTr Pattern = new MemberDescriptionTr { DisplayName = "Template", Description = "Filename template. The following placeholders are available: {tier}, {country}, {class}, {category}, {id}." };

        [LingoNotes("The string \"{0}\" is replaced with the filename of the missing image. ")]
        public TrString MissingImageWarning = "The image {0} could not be found.";
    }

    [LingoStringClass, LingoInGroup(TranslationGroup.LayerText)]
    sealed class TextLayerTranslation
    {
        public MemberDescriptionTr FontSmoothing = new MemberDescriptionTr { DisplayName = "Smoothing", Description = "Determines how to smooth the text (anti-aliasing)." };
        public MemberDescriptionTr FontFamily = new MemberDescriptionTr { DisplayName = "Family", Description = "Font family." };
        public MemberDescriptionTr FontSize = new MemberDescriptionTr { DisplayName = "Size", Description = "Font size." };
        public MemberDescriptionTr FontBold = new MemberDescriptionTr { DisplayName = "Bold", Description = "Makes the text bold." };
        public MemberDescriptionTr FontItalic = new MemberDescriptionTr { DisplayName = "Italic", Description = "Makes the text italic." };
        public MemberDescriptionTr FontColor = new MemberDescriptionTr { DisplayName = "Color", Description = "Specifies the text color." };
        public MemberDescriptionTr Left = new MemberDescriptionTr { DisplayName = "Left", Description = "X coordinate of the leftmost text pixel. Ignored if \"Left anchor\" is unchecked." };
        public MemberDescriptionTr Right = new MemberDescriptionTr { DisplayName = "Right", Description = "X coordinate of the rightmost text pixel. Ignored if \"Right anchor\" is unchecked." };
        public MemberDescriptionTr Top = new MemberDescriptionTr { DisplayName = "Top", Description = "Y coordinate of the topmost text pixel (but see also \"Align baselines\"). Ignored if \"Top anchor\" is unchecked." };
        public MemberDescriptionTr Bottom = new MemberDescriptionTr { DisplayName = "Bottom", Description = "Y coordinate of the bottommost text pixel (but see also \"Align baselines\"). Ignored if \"Bottom anchor\" is unchecked." };
        public MemberDescriptionTr LeftAnchor = new MemberDescriptionTr { DisplayName = "Left anchor", Description = "When checked, the leftmost pixel of the text is anchored at the X coordinate specified by \"Left\". When \"Right anchor\" is also checked, the text is centered between \"Left\" and \"Right\"." };
        public MemberDescriptionTr RightAnchor = new MemberDescriptionTr { DisplayName = "Right anchor", Description = "When checked, the rightmost pixel of the text is anchored at the X coordinate specified by \"Right\". When \"Left anchor\" is also checked, the text is centered between \"Left\" and \"Right\"." };
        public MemberDescriptionTr TopAnchor = new MemberDescriptionTr { DisplayName = "Top anchor", Description = "When checked, the topmost pixel of the text is anchored at the Y coordinate specified by \"Top\". When \"Bottom anchor\" is also checked, the text is centered between \"Top\" and \"Bottom\"." };
        public MemberDescriptionTr BottomAnchor = new MemberDescriptionTr { DisplayName = "Bottom anchor", Description = "When checked, the bottommost pixel of the text is anchored at the Y coordinate specified by \"Bottom\". When \"Top anchor\" is also checked, the text is centered between \"Top\" and \"Bottom\"." };
        public MemberDescriptionTr Baseline = new MemberDescriptionTr { DisplayName = "Align baselines", Description = "Consider the words \"more\" and \"More\", top-anchored at pixel 0. If \"Align baselines\" is false, the word \"more\" will be displayed slightly higher, so as to touch pixel 0. If true, the baselines will align instead, and the topmost pixel of \"more\" will actually be below pixel 0." };
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

    [LingoStringClass, LingoInGroup(TranslationGroup.EffectSizePos)]
    sealed class EffectSizePosTranslation
    {
        public TrString EffectName = "Size / Position";
        public TrString EffectDescription = "Adjusts this layer's size and/or position. This effect is always applied before any other effects.";

        public MemberDescriptionTr PositionByPixels = new MemberDescriptionTr { DisplayName = "Use pixels", Description = "When checked, the edge of the layer is defined by the visible pixels (see also \"Transparency threshold\"). Otherwise the complete layer size is used." };

        public MemberDescriptionTr Left = new MemberDescriptionTr { DisplayName = "Left", Description = "X coordinate of the left edge. Ignored if \"Left anchor\" is unchecked." };
        public MemberDescriptionTr Right = new MemberDescriptionTr { DisplayName = "Right", Description = "X coordinate of the right edge. Ignored if \"Right anchor\" is unchecked." };
        public MemberDescriptionTr Top = new MemberDescriptionTr { DisplayName = "Top", Description = "Y coordinate of the top edge. Ignored if \"Top anchor\" is unchecked." };
        public MemberDescriptionTr Bottom = new MemberDescriptionTr { DisplayName = "Bottom", Description = "Y coordinate of the bottom edge. Ignored if \"Bottom anchor\" is unchecked." };

        public MemberDescriptionTr LeftAnchor = new MemberDescriptionTr { DisplayName = "Left anchor", Description = "When checked, the left edge is anchored at the X coordinate specified by \"Left\". When \"Right anchor\" is also checked, the layer is centered between \"Left\" and \"Right\"." };
        public MemberDescriptionTr RightAnchor = new MemberDescriptionTr { DisplayName = "Right anchor", Description = "When checked, the right edge is anchored at the X coordinate specified by \"Right\". When \"Left anchor\" is also checked, the layer is centered between \"Left\" and \"Right\"." };
        public MemberDescriptionTr TopAnchor = new MemberDescriptionTr { DisplayName = "Top anchor", Description = "When checked, the top edge is anchored at the Y coordinate specified by \"Top\". When \"Bottom anchor\" is also checked, the layer is centered between \"Top\" and \"Bottom\"." };
        public MemberDescriptionTr BottomAnchor = new MemberDescriptionTr { DisplayName = "Bottom anchor", Description = "When checked, the bottom edge is anchored at the Y coordinate specified by \"Bottom\". When \"Top anchor\" is also checked, the layer is centered between \"Top\" and \"Bottom\"." };

        public MemberDescriptionTr SizeByPixels = new MemberDescriptionTr { DisplayName = "Use pixels", Description = "If checked, transparent areas on the outside of the image will be ignored in size calculations. See also \"Transparency threshold\"." };

        public MemberDescriptionTr Percentage = new MemberDescriptionTr { DisplayName = "Resize %", Description = "When Mode is \"By %\", selects the desired resize percentage." };
        public MemberDescriptionTr Width = new MemberDescriptionTr { DisplayName = "Resize to width", Description = "When Mode is one of \"By size\" modes, selects the desired width, in pixels." };
        public MemberDescriptionTr Height = new MemberDescriptionTr { DisplayName = "Resize to height", Description = "When Mode is one of \"By size\" modes, selects the desired height, in pixels." };
        public MemberDescriptionTr SizeMode = new MemberDescriptionTr { DisplayName = "Mode", Description = "Selects one of several different resize modes, which determines how the image size is calculated." };
        public MemberDescriptionTr GrowShrinkMode = new MemberDescriptionTr { DisplayName = "Grow/shrink", Description = "Specifies whether the image size is allowed to increase, decrease, or both, as a result of the resize." };

        public MemberDescriptionTr PixelAlphaThreshold = new MemberDescriptionTr { DisplayName = "Transparency threshold", Description = "When sizing or positioning by pixels, determines the maximum alpha value which is still deemed as \"transparent\". Range 0..255." };

        public MemberDescriptionTr ShowLayerBorders = new MemberDescriptionTr { DisplayName = "Show layer borders", Description = "If enabled, draws a rectangle to show the layer borders. These borders are used when \"Use pixels\" is disabled." };
        public MemberDescriptionTr ShowPixelBorders = new MemberDescriptionTr { DisplayName = "Show pixel borders", Description = "If enabled, draws a rectangle to show the pixel borders of the layer. Adjust the sensitivity using \"Transparency threshold\". These borders are used when \"Use pixels\" is enabled." };
        public MemberDescriptionTr ShowTargetBorders = new MemberDescriptionTr { DisplayName = "Show target borders", Description = "If enabled, draws a rectangle to show the selected target position for the layer. Anchored borders are drawn as solid lines, non-anchored are dotted." };
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

    [LingoStringClass, LingoInGroup(TranslationGroup.EnumSizeMode), LingoInGroup(TranslationGroup.EffectSizePos)]
    sealed class SizeModeTranslation
    {
        public TrString NoChange = "No change";
        public TrString ByPercentage = "By %";
        public TrString BySizeWidthOnly = "By size: width only";
        public TrString BySizeHeightOnly = "By size: height only";
        public TrString BySizeWidthHeightStretch = "By size: stretch";
        public TrString ByPosLeftRight = "By pos: left/right";
        public TrString ByPosTopBottom = "By pos: top/bottom";
        public TrString ByPosAllFit = "By pos: fit inside";
        public TrString ByPosAllStretch = "By pos: stretch";

        public class Conv : LingoEnumConverter<SizeMode, SizeModeTranslation>
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
        public TrString DataFile_UnrecognizedCountry = "Unrecognized country: \"{0}\". Allowed values: {1}.";
        public TrString DataFile_UnrecognizedClass = "Unrecognized class: \"{0}\". Allowed values: {1}.";
        public TrString DataFile_UnrecognizedCategory = "Unrecognized availability: \"{0}\". Allowed values: {1}.";
        public TrString DataFile_TankTierValue = "The tier field is not a whole number or outside of the 1..10 range: \"{0}\"";
        public TrStringNum DataFile_TooFewFields = new TrStringNum("Expected at least 1 field.", "Expected at least {0} fields.");
        public TrString DataFile_EmptyFile = "Expected at least one line.";
        public TrString DataFile_TooFewFieldsFirstLine = "Expected at least two columns in the first row.";
        public TrString DataFile_ExpectedSignature = "Expected \"{0}\" in the first column of the first row.";
        public TrString DataFile_ExpectedV1 = "The second column of the first row must be \"1\" (format version)";
        [LingoNotes("The string \"{0}\" is replaced with the line number where the error occurred. \"{1}\" is replaced with the error message.")]
        public TrString DataFile_LineNum = "Line {0}: {1}";
        public TrString DataFile_CsvParse = "Couldn't parse line {0}.";

        public TrString DataDir_NoFilesAvailable = "Could not load any game version data files and/or any built-in property data files.";
        public TrString DataDir_Skip_NoBuiltIns = "Skipped version {0} because there are no built-in property data files for it to use. Delete the \"{1}\" file to get rid of this warning.";
        public TrString DataDir_Skip_WrongParts = "Skipped \"{0}\" because it has the wrong number of filename parts (expected: {1}, actual {2}).";
        [LingoNotes("The DataFile_* errors are interpolated into this string in place of \"{1}\".")]
        public TrString DataDir_Skip_FileError = "Skipped \"{0}\" because the file could not be parsed: {1}";
        public TrString DataDir_Skip_GameVersion = "Skipped \"{0}\" because it has an unparseable game version part in the filename: \"{1}\".";
        public TrString DataDir_Skip_FileVersion = "Skipped \"{0}\" because it has an unparseable file version part in the filename: \"{1}\" (or it isn't exactly 3 digits long).";
        public TrString DataDir_Skip_Author = "Skipped \"{0}\" because it has an empty author part in the filename.";
        public TrString DataDir_Skip_PropName = "Skipped \"{0}\" because it has an empty property name part in the filename.";
        public TrString DataDir_Skip_Lang = "Skipped \"{0}\" because its language name part in the filename (\"{1}\") is not a valid language code, nor \"X\" for language-less files. Did you mean En, Ru, Zh, Es, Fr, De, Ja? Full list of ISO-639-1 codes is available on Wikipedia.";

        public TrString DataDir_Skip_InhNoProp = "Skipped \"{0}\" because there are no data files for the property \"{1}\" (from which it inherits values).";
        public TrString DataDir_Skip_InhNoLang = "Skipped \"{0}\" because no data files for the property \"{1}\" (from which it inherits values) are in language \"{2}\".";
        public TrString DataDir_Skip_InhNoAuth = "Skipped \"{0}\" because no data files for the property \"{1}\" (from which it inherits values) are by author \"{2}\".";
        public TrString DataDir_Skip_InhNoGameVer = "Skipped \"{0}\" because no data files for the property \"{1}\"/\"{2}\" (from which it inherits values) have game version \"{3}\" or below.";
        public TrString DataDir_Skip_InhCircular = "Skipped \"{0}\" due to a circular dependency.";

        public TrString NoDataFilesWarning = "Found no version files and/or no built-in property data files. Make sure the files are available under the following path:\n\n{0}";

        public TrString RenderWithErrors = "Some of the tank icons did not render correctly; make sure you view \"All tanks\" and click each broken image for details.";
        public TrString RenderWithWarnings = "Some of the tank icons rendered with warnings; make sure you view \"All tanks\" and click each image with a warning icon for details.";
        public TrString RenderIconOK = "This icon rendered without any problems.";
        public TrString RenderIconFail = "Could not render this icon: {0}";
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

        [LingoNotes("A generic Cancel button text used in some modal dialogs to cancel whatever action is being done without making any changes. Do not use hotkeys (because the required prefix varies).")]
        public TrString Cancel = "Cancel";
        public TrString PromptWindowOK = "_OK";
        public TrString GamePathRequired = "Please add a game path first (top left, green plus button) so that TankIconMaker knows where to save the icons.";
        [LingoNotes("\"{1}\" is replaced with the extension of the image files being saved.")]
        public TrString OverwriteIcons_Prompt = "Would you like to overwrite your current icons?\n\nPath: {0}\n\nWarning: ALL *.{1} files in this path will be overwritten, and there is NO UNDO for this!";
        public TrString OverwriteIcons_Yes = "&Yes, overwrite all files";
        public TrString GameNotFound_Prompt = "This directory does not appear to contain a supported version of World Of Tanks. Are you sure you want to use it anyway?";
        public TrString GameNotFound_Ignore = "&Use anyway";
        public TrString IconsSaved = "Saved!\nEnjoy.";
        public TrStringNum IconsSaveSkipped = new TrStringNum("Note that 1 image was skipped due to errors.", "Note that {0} images were skipped due to errors.");
        public TrString Upvote_BuiltInOnly = "For security reasons, only built-in styles can be upvoted.";
        public TrString Upvote_NotAvailable = "This style does not currently have an associated post on World of Tanks forums.";
        public TrString Upvote_Prompt = "To thank {0} for designing this style, please upvote the following topic on the World of Tanks forum:\n\n{1}";
        public TrString Upvote_Open = "&Open in browser";
        public TrString DeleteLayerEffect_Prompt = "Delete the selected layer/effect?";
        public TrString DeleteLayerEffect_Yes = "&Delete";
        public TrString DeleteStyle_Prompt = "Delete this style?\r\n\r\n{0}";
        public TrString DeleteStyle_Yes = "&Delete";
        public TrString StyleImport_Fail = "Could not load the file for some reason. It might be of the wrong format.";
        public TrString StyleExport_Success = "The style has been exported.";
        public TrString ExceptionGlobal = "An error has occurred. This is not your fault; the programmer has messed up!\n\nPlease send an error report to the programmer so that this can be fixed.";
        public TrString ExceptionInRender = "A layer or an effect threw an exception while rendering this image. This is a bug in the program; please report it.";
        public TrString ErrorToClipboard_Copy = "Copy report to &clipboard";
        public TrString ErrorToClipboard_OK = "OK";
        public TrString ErrorToClipboard_Copied = "Information about the error is now in your clipboard.";
        public TrString ErrorToClipboard_CopyFail = "Sorry, couldn't copy the error info to clipboard for some reason.";
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

        public TrString CountryUSSR = "Country: USSR";
        public TrString CountryGermany = "Country: Germany";
        public TrString CountryUSA = "Country: USA";
        public TrString CountryFrance = "Country: France";
        public TrString CountryChina = "Country: China";
        public TrString CountryUK = "Country: UK";

        public TrString CategNormal = "Avail.: Normal";
        public TrString CategPremium = "Avail.: Premium";
        public TrString CategSpecial = "Avail.: Special";

        public TrString Single = "Not varied";
        public TrString SingleDescription = "When \"Vary by\" is set to \"No variation\", specifies the value to use. Use this when the value does not need to vary by tank properties.";

        public TrString TierN = "Tier: {0,2}";
    }
}
