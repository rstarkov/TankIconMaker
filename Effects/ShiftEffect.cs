﻿using System;
using RT.Util.Lingo;

namespace TankIconMaker.Effects
{
    class ShiftEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.EffectShift.EffectName; } }
        public override string TypeDescription { get { return App.Translation.EffectShift.EffectDescription; } }

        public int ShiftX { get; set; }
        public static MemberTr ShiftXTr(Translation tr) { return new MemberTr(tr.Category.Shift, tr.EffectShift.ShiftX); }

        public int ShiftY { get; set; }
        public static MemberTr ShiftYTr(Translation tr) { return new MemberTr(tr.Category.Shift, tr.EffectShift.ShiftY); }

        public override BitmapBase Apply(RenderTask renderTask, BitmapBase layer)
        {
            Tank tank = renderTask.Tank;
            if (ShiftX == 0 && ShiftY == 0)
                return layer;
            var result = new BitmapRam(Math.Max(1, layer.Width + ShiftX), Math.Max(1, layer.Height + ShiftY));
            result.DrawImage(layer, ShiftX, ShiftY);
            return result;
        }
    }
}
