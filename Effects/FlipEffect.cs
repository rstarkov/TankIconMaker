using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankIconMaker.Effects
{
    class FlipEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return "Flip"; } }
        public override string TypeDescription { get { return "Flips the layer horizontally and/or vertically."; } }

        [DisplayName("Flip horizontally")]
        [Description("Flips the layer horizontally, that is, swapping left and right.")]
        public bool FlipHorz { get; set; }
        [DisplayName("Flip vertically")]
        [Description("Flips the layer vertically, that is, swapping up and down.")]
        public bool FlipVert { get; set; }

        public FlipEffect()
        {
            FlipHorz = true;
            FlipVert = false;
        }

        public override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            if (FlipHorz)
                layer.FlipHorz();
            if (FlipVert)
                layer.FlipVert();
            return layer;
        }
    }
}
