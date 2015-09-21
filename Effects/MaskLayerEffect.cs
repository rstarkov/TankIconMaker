using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using ImageMagick;
using RT.Util.Lingo;
using RT.Util.Serialization;

namespace TankIconMaker.Effects
{
    [TypeConverter(typeof(MaskModeTranslation.Conv))]
    enum MaskMode
    {
        Combinated,
        Opacity,
        Grayscale
    }

    class MaskLayerEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.EffectMaskLayer.EffectName; } }
        public override string TypeDescription { get { return App.Translation.EffectMaskLayer.EffectDescription; } }

        public string MaskLayerId { get; set; }
        public static MemberTr MaskLayerIdTr(Translation tr) { return new MemberTr(tr.Category.Mask, tr.EffectMaskLayer.MaskLayerId); }

        public MaskMode MaskMode { get; set; }
        public static MemberTr MaskModeTr(Translation tr) { return new MemberTr(tr.Category.Mask, tr.EffectMaskLayer.MaskMode); }

        public bool Invert { get; set; }
        public static MemberTr InvertTr(Translation tr) { return new MemberTr(tr.Category.Mask, tr.EffectMaskLayer.InvertTr); }

        public MaskLayerEffect()
        {
            MaskLayerId = string.Empty;
            MaskMode = Effects.MaskMode.Combinated;
            Invert = false;
        }
        
        public unsafe override BitmapBase Apply(RenderTask renderTask, BitmapBase layer)
        {
            Tank tank = renderTask.Tank;
            LayerBase maskLayer;
            if (string.IsNullOrEmpty(MaskLayerId))
                return layer;
            maskLayer = renderTask.layers.FirstOrDefault(x => x.Id == MaskLayerId);
            if (maskLayer == null)
                throw new Exception("No layer with correspinding Id found");
            var maskImg = renderTask.RenderLayer(maskLayer);
            using (layer.UseWrite())
            {
                using (maskImg.UseRead())
                {
                    for (int i = 0; i < layer.Width; ++i)
                    {
                        for (int j = 0; j < layer.Height; ++j)
                        {
                            decimal alpha = 0;
                            if (i < maskImg.Width && j < maskImg.Height)
                            {
                                switch (MaskMode)
                                {
                                    case Effects.MaskMode.Opacity:
                                        alpha = maskImg.Data[i * 4 + maskImg.Stride * j + 3];
                                        break;
                                    case Effects.MaskMode.Grayscale:
                                        alpha = (maskImg.Data[i * 4 + maskImg.Stride * j]
                                            + maskImg.Data[i * 4 + maskImg.Stride * j + 1]
                                            + maskImg.Data[i * 4 + maskImg.Stride * j + 2]
                                            ) / 3;
                                        break;
                                    case Effects.MaskMode.Combinated:
                                        alpha = (maskImg.Data[i * 4 + maskImg.Stride * j]
                                            + maskImg.Data[i * 4 + maskImg.Stride * j + 1]
                                            + maskImg.Data[i * 4 + maskImg.Stride * j + 2]
                                            ) / 3 * maskImg.Data[i * 4 + maskImg.Stride * j + 3] / 255m;
                                        break;
                                }
                            }
                            if (Invert)
                                alpha = 255m - alpha;
                            var opacity = layer.Data[i * 4 + maskImg.Stride * j + 3] * (alpha / 255m);
                            layer.Data[i * 4 + maskImg.Stride * j + 3] = (byte)opacity;
                        }
                    }
                }
            }
            return layer;
        }
    }
}
