using RT.Util.ExtensionMethods;
using RT.Util.Lingo;
using WotDataLib;

namespace TankIconMaker.Effects
{
    class NormalizeEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.EffectNormalize.EffectName; } }
        public override string TypeDescription { get { return App.Translation.EffectNormalize.EffectDescription; } }

        public bool Grayscale { get; set; }
        public static MemberTr GrayscaleTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectNormalize.Grayscale); }

        public bool NormalizeBrightness { get; set; }
        public static MemberTr NormalizeBrightnessTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectNormalize.NormalizeBrightness); }

        public bool NormalizeAlpha { get; set; }
        public static MemberTr NormalizeAlphaTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectNormalize.NormalizeAlpha); }

        public int MaxBrightness { get; set; }
        public static MemberTr MaxBrightnessTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectNormalize.MaxBrightness); }

        public int MaxAlpha { get; set; }
        public static MemberTr MaxAlphaTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectNormalize.MaxAlpha); }

        public NormalizeEffect()
        {
            Grayscale = true;
            NormalizeBrightness = true;
            NormalizeAlpha = true;
            MaxBrightness = 255;
            MaxAlpha = 196;
        }

        public override EffectBase Clone()
        {
            var result = (NormalizeEffect) base.Clone();
            return result;
        }

        public unsafe override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            using (layer.UseWrite())
            {
                // Just scale the brightness and alpha channels so as to normalize the maximum value.
                // This is crude but gives good results; a better algorithm would try to fit the histogram
                // to a predefined standard by scaling non-linearly.
                double maxBrightness = -1;
                double maxAlpha = -1;
                for (int y = 0; y < layer.Height; y++)
                {
                    byte* ptr = layer.Data + y * layer.Stride;
                    byte* end = ptr + layer.Width * 4;
                    while (ptr < end)
                    {
                        byte alpha = *(ptr + 3);
                        if (alpha > 0) // there are a lot of non-black pixels in the fully-transparent regions
                        {
                            if (NormalizeBrightness)
                            {
                                double brightness = *(ptr + 0) * 0.0722 + *(ptr + 1) * 0.7152 + *(ptr + 2) * 0.2126;
                                if (brightness > maxBrightness)
                                    maxBrightness = brightness;
                            }
                            if (NormalizeAlpha)
                            {
                                if (alpha > maxAlpha)
                                    maxAlpha = alpha;
                            }
                        }
                        ptr += 4;
                    }
                }

                double scaleBrightness = (double) MaxBrightness / maxBrightness;
                double scaleAlpha = (double) MaxAlpha / maxAlpha;
                for (int y = 0; y < layer.Height; y++)
                {
                    byte* ptr = layer.Data + y * layer.Stride;
                    byte* end = ptr + layer.Width * 4;
                    while (ptr < end)
                    {
                        byte alpha = *(ptr + 3);
                        if (alpha > 0)
                        {
                            if (NormalizeBrightness)
                            {
                                if (Grayscale)
                                {
                                    double brightness = *(ptr + 0) * 0.0722 + *(ptr + 1) * 0.7152 + *(ptr + 2) * 0.2126;
                                    *(ptr + 0) = *(ptr + 1) = *(ptr + 2) = (byte) (brightness * scaleBrightness).ClipMax(255);
                                }
                                else
                                {
                                    // TODO: the clipping here alters the hue. Ideally the color should be clipped without altering hue, by increasing brightness until white.
                                    *(ptr + 0) = (byte) (*(ptr + 0) * scaleBrightness).ClipMax(255);
                                    *(ptr + 1) = (byte) (*(ptr + 1) * scaleBrightness).ClipMax(255);
                                    *(ptr + 2) = (byte) (*(ptr + 2) * scaleBrightness).ClipMax(255);
                                }
                            }
                            else if (Grayscale)
                            {
                                double brightness = *(ptr + 0) * 0.0722 + *(ptr + 1) * 0.7152 + *(ptr + 2) * 0.2126;
                                *(ptr + 0) = *(ptr + 1) = *(ptr + 2) = (byte) brightness;
                            }
                            if (NormalizeAlpha)
                            {
                                *(ptr + 3) = (byte) (alpha * scaleAlpha);
                            }
                        }
                        ptr += 4;
                    }
                }
            }
            return layer;
        }
    }
}
