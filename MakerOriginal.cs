using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace TankIconMaker
{
    class MakerOriginal : MakerBaseWpf
    {
        public override string Name { get { return "Original"; } }
        public override string Author { get { return "Romkyns"; } }
        public override int Version { get { return 1; } }
        public override string Description { get { return "Original WoT tank icons."; } }

        public override void DrawTank(Tank tank, DrawingContext dc)
        {
            var image = tank.LoadImageContourWpf();
            if (image == null)
                tank.AddWarning("Could not load the contour image for this tank.");
            else
                dc.DrawImage(image);
        }
    }
}
