﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows;

namespace TankIconMaker
{
    public enum ImageStyle { Contour, [Description("3D")] ThreeD }

    class LayerTankImage : LayerBaseWpf
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return "Tank Image"; } }
        public override string TypeDescription { get { return "Draws a tank in one of several styles."; } }

        public ImageStyle Style { get; set; }

        public override void Draw(Tank tank, DrawingContext dc)
        {
            var image = Style == ImageStyle.Contour ? tank.LoadImageContourWpf() : tank.LoadImage3DWpf();
            if (image == null)
            {
                tank.AddWarning("The image for this tank is missing.");
                return;
            }
            dc.DrawImage(image, new Rect(0, 0, image.Width / image.Height * 24.0, 24));
        }
    }
}