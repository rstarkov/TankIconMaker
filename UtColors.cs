using System;
using System.Windows.Media;
using D = System.Drawing;

namespace TankIconMaker
{
    static partial class Ut
    {
        public static Color BlendColors(Color left, Color right, double rightAmount)
        {
            return Color.FromArgb(
                a: (byte) Math.Round(left.A * (1 - rightAmount) + right.A * rightAmount),
                r: (byte) Math.Round(left.R * (1 - rightAmount) + right.R * rightAmount),
                g: (byte) Math.Round(left.G * (1 - rightAmount) + right.G * rightAmount),
                b: (byte) Math.Round(left.B * (1 - rightAmount) + right.B * rightAmount));
        }

        public static D.Color ToColorGdi(this Color color)
        {
            return D.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static Color ToColorWpf(this D.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static Color WithAlpha(this Color color, int alpha)
        {
            if (alpha < 0 || alpha > 255) throw new ArgumentOutOfRangeException("alpha");
            return Color.FromArgb((byte) alpha, color.R, color.G, color.B);
        }

    }

    /// <summary>Adapted from Paint.NET and thus exactly compatible in the RGB/HSV conversion (apart from hue 360, which must be 0 instead)</summary>
    struct ColorHSV
    {
        /// <summary>Hue, 0..359</summary>
        public int Hue { get; private set; }
        /// <summary>Saturation, 0..100</summary>
        public int Saturation { get; private set; }
        /// <summary>Value, 0..100</summary>
        public int Value { get; private set; }
        /// <summary>Alpha, range 0..255</summary>
        public int Alpha { get; private set; }

        private ColorHSV(int hue, int saturation, int value, int alpha)
            : this()
        {
            if (hue < 0 || hue > 359) throw new ArgumentException("hue");
            if (saturation < 0 || saturation > 100) throw new ArgumentException("saturation");
            if (value < 0 || value > 100) throw new ArgumentException("value");
            if (alpha < 0 || alpha > 255) throw new ArgumentException("alpha");
            Hue = hue;
            Saturation = saturation;
            Value = value;
            Alpha = alpha;
        }

        public static ColorHSV FromHSV(int hue, int saturation, int value, int alpha = 255)
        {
            return new ColorHSV(hue, saturation, value, alpha);
        }

        public D.Color ToColorGdi()
        {
            return ToColorWpf().ToColorGdi();
        }

        public Color ToColorWpf()
        {
            double h;
            double s;
            double v;

            double r = 0;
            double g = 0;
            double b = 0;

            // Scale Hue to be between 0 and 360. Saturation
            // and value scale to be between 0 and 1.
            h = (double) Hue % 360;
            s = (double) Saturation / 100;
            v = (double) Value / 100;

            if (s == 0)
            {
                // If s is 0, all colors are the same.
                // This is some flavor of gray.
                r = v;
                g = v;
                b = v;
            }
            else
            {
                double p;
                double q;
                double t;

                double fractionalSector;
                int sectorNumber;
                double sectorPos;

                // The color wheel consists of 6 sectors.
                // Figure out which sector you're in.
                sectorPos = h / 60;
                sectorNumber = (int) (Math.Floor(sectorPos));

                // get the fractional part of the sector.
                // That is, how many degrees into the sector
                // are you?
                fractionalSector = sectorPos - sectorNumber;

                // Calculate values for the three axes
                // of the color. 
                p = v * (1 - s);
                q = v * (1 - (s * fractionalSector));
                t = v * (1 - (s * (1 - fractionalSector)));

                // Assign the fractional colors to r, g, and b
                // based on the sector the angle is in.
                switch (sectorNumber)
                {
                    case 0: r = v; g = t; b = p; break;
                    case 1: r = q; g = v; b = p; break;
                    case 2: r = p; g = v; b = t; break;
                    case 3: r = p; g = q; b = v; break;
                    case 4: r = t; g = p; b = v; break;
                    case 5: r = v; g = p; b = q; break;
                }
            }
            return Color.FromArgb((byte) Alpha, (byte) (r * 255), (byte) (g * 255), (byte) (b * 255));
        }

        public static ColorHSV FromColor(Color color)
        {
            // In this function, R, G, and B values must be scaled 
            // to be between 0 and 1.
            // HsvColor.Hue will be a value between 0 and 360, and 
            // HsvColor.Saturation and value are between 0 and 1.

            double min;
            double max;
            double delta;

            double r = (double) color.R / 255;
            double g = (double) color.G / 255;
            double b = (double) color.B / 255;

            double h;
            double s;
            double v;

            min = Math.Min(Math.Min(r, g), b);
            max = Math.Max(Math.Max(r, g), b);
            v = max;
            delta = max - min;

            if (max == 0 || delta == 0)
            {
                // R, G, and B must be 0, or all the same.
                // In this case, S is 0, and H is undefined.
                // Using H = 0 is as good as any...
                s = 0;
                h = 0;
            }
            else
            {
                s = delta / max;
                if (r == max)
                {
                    // Between Yellow and Magenta
                    h = (g - b) / delta;
                }
                else if (g == max)
                {
                    // Between Cyan and Yellow
                    h = 2 + (b - r) / delta;
                }
                else
                {
                    // Between Magenta and Cyan
                    h = 4 + (r - g) / delta;
                }

            }
            // Scale h to be between 0 and 360. 
            // This may require adding 360, if the value
            // is negative.
            h *= 60;

            if (h < 0)
            {
                h += 360;
            }

            // Scale to the requirements of this 
            // application. All values are between 0 and 255.
            return FromHSV((int) h, (int) (s * 100), (int) (v * 100), color.A);
        }

        public ColorHSV ScaleValue(double scale)
        {
            return FromHSV(Hue, Saturation, Math.Max(0, Math.Min(100, (int) Math.Round(Value * scale))), Alpha);
        }

        public ColorHSV WithAlpha(int alpha)
        {
            return FromHSV(Hue, Saturation, Value, Alpha);
        }
    }


}
