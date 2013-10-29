using System;
using System.Drawing;
using WotDataLib;

namespace TankIconMaker.Layers
{
#if DEBUG
    sealed class TestLayer : LayerBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return "Test layer"; } }
        public override string TypeDescription { get { return "Draws a pseudorandom test rectangle"; } }

        public Color Color { get; set; }
        public bool SmallWithBorders { get; set; }
        public bool LargeWithBorders { get; set; }
        public bool WithoutBorders { get; set; }
        public int _X { get; set; }
        public int _Y { get; set; }
        public int _RectWidth { get; set; }
        public int _RectHeight { get; set; }
        public int _LayerWidth { get; set; }
        public int _LayerHeight { get; set; }

        public TestLayer()
        {
            Color = Color.FromArgb(0x17, 0x34, 0x89);
            _LayerWidth = _LayerHeight = 16;
            _X = _Y = 4;
            _RectWidth = _RectHeight = 8;
        }

        public override BitmapBase Draw(Tank tank)
        {
            var rnd = new Random(tank.TankId.GetHashCode());
            int rectWidth = _RectWidth, rectHeight = _RectHeight;
            int x = _X, y = _Y;
            int layerWidth = _LayerWidth, layerHeight = _LayerHeight;
            if (SmallWithBorders && rnd.NextDouble() < 0.33)
            {
                rectWidth = rnd.Next(1, 70);
                rectHeight = rnd.Next(1, 20);
                x = rnd.Next(0, ParentStyle.IconWidth - rectWidth);
                y = rnd.Next(0, ParentStyle.IconHeight - rectHeight);
                layerWidth = x + rectWidth + rnd.Next(0, 10);
                layerHeight = y + rectHeight + rnd.Next(0, 10);
            }
            else if (WithoutBorders && rnd.NextDouble() < 0.5)
            {
                rectWidth = layerWidth = rnd.Next(1, 70);
                rectHeight = layerHeight = rnd.Next(1, 20);
                x = y = 0;
            }
            else if (LargeWithBorders)
            {
                rectWidth = rnd.Next(70, 200);
                rectHeight = rnd.Next(20, 70);
                x = rnd.Next(0, 10);
                y = rnd.Next(0, 10);
                layerWidth = x + rectWidth + rnd.Next(0, 10);
                layerHeight = y + rectHeight + rnd.Next(0, 10);
            }

            var result = new BitmapGdi(layerWidth, layerHeight);
            using (var brush = new SolidBrush(Color))
            using (var g = Graphics.FromImage(result.Bitmap))
                g.FillRectangle(brush, x, y, rectWidth, rectHeight);
            return result;
        }
    }
#endif
}
