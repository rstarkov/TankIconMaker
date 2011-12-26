using System.Drawing;
using System.Drawing.Text;

namespace TankIconMaker
{
    class Test1Maker : IconMaker
    {
        public override string Name { get { return "Test 1"; } }
        public override string Author { get { return "Romkyns"; } }
        public override int Version { get { return 1; } }

        public Color NameColor { get; set; }
        public Color NameColorPremium { get; set; }
        public Color NameColorSpecial { get; set; }

        public override BytesBitmap DrawTank(Tank tank)
        {
            var nameFont = new Font("Arial", 9f);
            var nameBrush = new SolidBrush(tank.Category == Category.Normal ? Color.White : tank.Category == Category.Premium ? Color.Yellow : Color.FromArgb(255, 100, 50));
            var backBrush = new SolidBrush(Color.FromArgb(128, 100, 150, 255));
            var result = NewBitmap();
            using (var g = Graphics.FromImage(result.Bitmap))
            {
                g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
                g.FillRectangle(backBrush, new Rectangle(1, 1, 78, 22));
                g.DrawString(tank["OfficialName"] ?? "-", nameFont, nameBrush, new Point(2, 2), StringFormat.GenericTypographic);
            }
            return result;
        }
    }

    class Test2Maker : IconMaker
    {
        public override string Name { get { return "Test 2"; } }
        public override string Author { get { return "Romkyns"; } }
        public override int Version { get { return 1; } }

        public override BytesBitmap DrawTank(Tank tank)
        {
            var result = NewBitmap();
            var bits = result.Bits;
            for (int i = 0; i < bits.Length; i++)
                bits[i] = (byte) (i * 5);
            return result;
        }
    }
}
