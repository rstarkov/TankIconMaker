using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RT.Util.Xml;

namespace TankIconMaker.Effects
{
    class ShadowEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return "Shadow"; } }
        public override string TypeDescription { get { return "Adds a shadow as if cast by the current layer."; } }

        [Category("Shadow")]
        [Description("Shadow radius.")]
        public double Radius { get { return _Radius; } set { _Radius = Math.Max(1.0, value); } }
        private double _Radius = 3.5;
        [Category("Shadow")]
        [Description("Shadow spread controls the strength of the shadow, but does not affect its maximum size.")]
        public double Spread { get { return _Spread; } set { _Spread = Math.Max(0, value); } }
        private double _Spread = 5;
        [Category("Shadow")]
        [Description("Shadow color. Use bright colors for glow. Adjust the Alpha channel to control final shadow transparency.")]
        public ColorSelector Color { get; set; }
        [Category("Shadow"), DisplayName("Shift: X")]
        [Description("Amount of horizontal shift in pixels.")]
        public int ShiftX { get; set; }
        [Category("Shadow"), DisplayName("Shift: Y")]
        [Description("Amount of vertical shift in pixels.")]
        public int ShiftY { get; set; }

        [XmlIgnore]
        private GaussianBlur _blur;

        public ShadowEffect()
        {
            Color = new ColorSelector(Colors.Black);
        }

        public override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            if (_blur == null || _blur.Radius != Radius)
                lock (this)
                    if (_blur == null || _blur.Radius != Radius)
                        _blur = new GaussianBlur(Radius);

            var shadow = layer.Clone();
            var color = Color.GetColorWpf(tank);
            shadow.SetColor(color);
            shadow = _blur.Blur(shadow, BlurEdgeMode.Transparent);
            shadow.ScaleOpacity(Spread, OpacityStyle.Additive);
            shadow.Transparentize(color.A);
            return Ut.NewBitmapWpf(dc =>
            {
                dc.DrawImage(shadow, new Rect(ShiftX, ShiftY, shadow.PixelWidth, shadow.PixelHeight));
                dc.DrawImage(layer);
            }).ToWpfWriteable();
        }
    }
}
