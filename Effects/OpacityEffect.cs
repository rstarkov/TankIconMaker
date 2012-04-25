using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace TankIconMaker.Effects
{
    class OpacityEffect : EffectBaseWpf
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return "Opacity"; } }
        public override string TypeDescription { get { return "Increases or decreases the layer’s opacity."; } }

        [Category("Opacity")]
        [Description("The opacity multiplier. Negative makes a layer more transparent, positive makes it more opaque.")]
        public double Opacity { get; set; }
        [Category("Opacity")]
        [Description("Selects one of several different curves for adjusting the opacity. \"Auto\" uses \"Additive\" for increasing opacity and \"Move endpoint\" for decreasing.")]
        public OpacityStyle Style { get; set; }

        public OpacityEffect()
        {
            Opacity = -1.5;
            Style = OpacityStyle.Auto;
        }

        public override WriteableBitmap Apply(Tank tank, WriteableBitmap layer)
        {
            var result = layer.Clone();
            result.ScaleOpacity(Opacity, Style);
            return result;
        }
    }
}
