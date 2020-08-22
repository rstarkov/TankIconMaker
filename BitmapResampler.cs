using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankIconMaker
{
    static class BitmapResampler
    {
        public struct Contributor
        {
            /// <summary>X or Y coordinate of the contributing pixel in the source.</summary>
            public int Coord;
            public double Weight;
        }

        public struct ContributorEntry
        {
            /// <summary>Number of entries in <see cref="SrcPixel"/>.</summary>
            public int SrcPixelCount;
            /// <summary>
            ///     All the pixels in the source image which contribute to the destination pixel, and the associated weight.
            ///     Some of the entries at the end of the array may be unused/unpopulated; see <see cref="SrcPixelCount"/> for
            ///     the actual count.</summary>
            public Contributor[] SrcPixel;
        }

        public static unsafe BitmapBase SizePos(BitmapBase source, double scaleWidth, double scaleHeight, int inX, int inY, int outX, int outY, int maxWidth = 0, int maxHeight = 0, Filter filter = null)
        {
            if (source.Width <= 0 || source.Height <= 0)
                return source.ToBitmapSame();

            PixelRect pureImg = source.PreciseSize(0);
            if (pureImg.Width <= 0 || pureImg.Height <= 0)
                return source.ToBitmapSame();

            int outWidth = (int) Math.Round(pureImg.Width * scaleWidth);
            int outHeight = (int) Math.Round(pureImg.Height * scaleHeight);

            if (scaleWidth == 1 && scaleHeight == 1)
            {
                //no resize needed
                if (inX != outX || inY != outY)
                {
                    BitmapBase result;
                    if (maxWidth == 0 && maxHeight == 0)
                        result = new BitmapRam(outX - inX + source.Width, outY - inY + source.Height);
                    else
                        result = new BitmapRam(Math.Min(outX - inX + source.Width, maxWidth), Math.Min(outY - inY + source.Height, maxHeight));

                    result.DrawImage(source, outX - inX, outY - inY);
                    return result;
                }
                else
                    return source.ToBitmapSame();
            }

            if (filter == null)
            {
                if (scaleWidth < 1)
                    filter = new LanczosFilter();
                else
                    filter = new MitchellFilter();
            }

            int transparentOffset;
            if (pureImg.Left != 0 || pureImg.Top != 0)
            {
                transparentOffset = pureImg.Left * 4 + pureImg.Top * source.Stride;
                // Resample looks better if transprent pixels is cropped. Especially if the image is square
                // Data+DataOffset, pureImg.Width, pureImg.Height instead of Data, Width, Height works like left-top cropping
            }
            else
            {
                transparentOffset = 0;
            }

            BitmapBase afterHorzResample, afterVertResample;

            // Horizontal resampling
            if (scaleWidth == 1)
            {
                afterHorzResample = source;
            }
            else
            {
                afterHorzResample = new BitmapRam(outWidth, pureImg.Height);
                ContributorEntry[] contrib = filter.PrecomputeResample(scaleWidth, pureImg.Width, outWidth);
                Resample1D(afterHorzResample, source, transparentOffset, contrib, outWidth, pureImg.Height, true);
                transparentOffset = 0;
            }

            // Vertical resampling
            if (scaleHeight == 1)
            {
                afterVertResample = afterHorzResample;
            }
            else
            {
                afterVertResample = new BitmapRam(outWidth, outHeight);
                ContributorEntry[] contrib = filter.PrecomputeResample(scaleHeight, pureImg.Height, outHeight);
                Resample1D(afterVertResample, afterHorzResample, transparentOffset, contrib, outHeight, outWidth, false);
            }

            BitmapBase final;
            //At this point image will be resized and moved to another BitmapBase anyway
            int drawX = outX - (int) Math.Round((inX - pureImg.Left) * scaleWidth);
            int drawY = outY - (int) Math.Round((inY - pureImg.Top) * scaleHeight);
            if (maxWidth == 0 && maxHeight == 0)
                final = new BitmapRam(Math.Max(drawX + outWidth, maxWidth), Math.Max(drawY + outHeight, maxHeight));
            else
                final = new BitmapRam(Math.Max(drawX + outWidth, maxWidth), Math.Max(drawY + outHeight, maxHeight));
            final.DrawImage(afterVertResample, drawX, drawY);
            return final;
        }

        unsafe private static void Resample1D(BitmapBase bmpDest, BitmapBase bmpSrc, int transparentOffset, ContributorEntry[] contrib, int alongSize, int crossSize, bool horz)
        {
            using (bmpSrc.UseRead())
            using (bmpDest.UseWrite())
            {
                byte* srcBytes = bmpSrc.Data + transparentOffset;
                for (int crossCoord = 0; crossCoord < crossSize; ++crossCoord)
                {
                    for (int alongCoord = 0; alongCoord < alongSize; ++alongCoord)
                    {
                        for (int channel = 0; channel < 4; ++channel)
                        {
                            double intensity = 0;
                            double wsum = 0;

                            for (int j = 0; j < contrib[alongCoord].SrcPixelCount; j++)
                            {
                                int contribCoord = contrib[alongCoord].SrcPixel[j].Coord;
                                int contribOffset = (horz ? contribCoord : crossCoord) * 4 + (horz ? crossCoord : contribCoord) * bmpSrc.Stride;
                                double weight = contrib[alongCoord].SrcPixel[j].Weight;

                                if (channel != 3)
                                    weight *= srcBytes[contribOffset + 3] / 255d;

                                if (weight == 0)
                                    continue;

                                wsum += weight;
                                intensity += srcBytes[contribOffset + channel] * weight;
                            }

                            bmpDest.Data[(horz ? alongCoord : crossCoord) * 4 + (horz ? crossCoord : alongCoord) * bmpDest.Stride + channel] =
                                (byte) Math.Min(Math.Max(intensity / wsum, byte.MinValue), byte.MaxValue);
                        }
                    }
                }
            }
        }

        /// <summary>Implements a resampling filter.</summary>
        public abstract class Filter
        {
            public double Radius { get; protected set; }
            public abstract double GetValue(double x);

            public ContributorEntry[] PrecomputeResample(double scale, int srcWidth, int destWidth)
            {
                // all variables are named as if we're scaling horizontally for the sake of readability, but the results work for both orientations
                var dest = new ContributorEntry[destWidth]; // one entry for every pixel in the resulting (destination) row of pixels
                double r = scale < 1 ? Radius / scale : Radius; // filter radius in terms of source image pixels
                double s = scale < 1 ? scale : 1; // filter scale relative to source pixels

                for (int destX = 0; destX < destWidth; destX++)
                {
                    dest[destX].SrcPixelCount = 0;
                    dest[destX].SrcPixel = new Contributor[(int) Math.Floor(2 * r + 1)];
                    double center = (destX + 0.5) / scale;
                    int srcFromX = (int) Math.Floor(center - r);
                    int srcToX = (int) Math.Ceiling(center + r);

                    for (int srcX = srcFromX; srcX <= srcToX; srcX++)
                    {
                        double weight = GetValue((center - srcX - 0.5) * s);

                        if ((weight == 0) || (srcX < 0) || (srcX >= srcWidth))
                            continue;

                        dest[destX].SrcPixel[dest[destX].SrcPixelCount].Coord = srcX;
                        dest[destX].SrcPixel[dest[destX].SrcPixelCount].Weight = weight;
                        dest[destX].SrcPixelCount++;
                    }
                }
                return dest;
            }
        }

        /// <summary>
        /// Implements filters based on the Lanczos kernel.
        /// This includes the filters commonly known as "lanczos3" (radius = 3) and "sinc256" (radius = 8).
        /// </summary>
        public class LanczosFilter : Filter
        {
            public LanczosFilter(int radius = 3)
            {
                Radius = radius;
            }

            private double sinc(double x)
            {
                if (x == 0)
                    return 1;
                x *= Math.PI;
                return Math.Sin(x) / x;
            }

            public override double GetValue(double x)
            {
                if (x < 0)
                    x = -x;
                if (x >= Radius)
                    return 0;
                return sinc(x) * sinc(x / Radius);
            }
        }

        /// <summary>
        ///     Implements the family of bicubic filters, specifically the Mitchell-Netravali filters with two parameters,
        ///     which is a generalization of the Keys cubic filters. All of these have a radius of 2.</summary>
        /// <remarks>
        ///     For more information see: http://www.imagemagick.org/Usage/filter/#cubics and
        ///     http://entropymine.com/imageworsener/bicubic/</remarks>
        public class BicubicFilter : Filter
        {
            public double B { get; private set; }
            public double C { get; private set; }

            /// <summary>
            ///     Constructor. See Remarks for common values for B and C. See also <see cref="CatmullRomFilter"/> and <see
            ///     cref="MitchellFilter"/>.</summary>
            /// <remarks>
            ///     <para>
            ///         Common values for B and C:</para>
            ///     <list type="bullet">
            ///         <item>Catmull_Rom, GIMP: B = 0, C = 1/2 (implemented as <see cref="CatmullRomFilter"/>)</item>
            ///         <item>Mitchell: B = 1/3, C = 1/3 (implemented as <see cref="MitchellFilter"/>)</item>
            ///         <item>Photoshop: B = 0, C = 3/4</item>
            ///         <item>B-Spline: B = 1, C = 0 (aka "spline")</item>
            ///         <item>Faststone: B = 0, C = 1</item></list></remarks>
            public BicubicFilter(double b, double c)
            {
                Radius = 2;
                B = b;
                C = c;
            }

            public override double GetValue(double x)
            {
                if (x < 0)
                    x = -x;
                double x2 = x * x;
                if (x < 1)
                    return (((12 - 9 * B - 6 * C) * (x * x2)) + ((-18 + 12 * B + 6 * C) * x2) + (6 - 2 * B)) / 6;
                if (x < 2)
                    return (((-B - 6 * C) * (x * x2)) + ((6 * B + 30 * C) * x2) + ((-12 * B - 48 * C) * x) + (8 * B + 24 * C)) / 6;
                return 0;
            }
        }

        public class CatmullRomFilter : BicubicFilter
        {
            public CatmullRomFilter()
                : base(0, 0.5)
            {
            }
        }

        public class MitchellFilter : BicubicFilter
        {
            public MitchellFilter()
                : base(1 / 3.0, 1 / 3.0)
            {
            }
        }

    }
}
