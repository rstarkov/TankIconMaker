using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankIconMaker
{
    static class BitmapResampler
    {
        private struct Contributor
        {
            public int Pixel;
            public double Weight;
        }

        private struct ContributorEntry
        {
            public int N;
            public Contributor[] P;
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
                if (scaleWidth < 1f)
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

            #region horizontal resampling
            if (scaleWidth == 1)
            {
                afterHorzResample = source;
            }
            else
            {
                afterHorzResample = new BitmapRam(outWidth, pureImg.Height);
                var contrib = new ContributorEntry[outWidth];

                if (scaleWidth < 1f)
                {
                    #region downsampling
                    double width = filter.Radius / scaleWidth;

                    for (int i = 0; i < outWidth; ++i)
                    {
                        contrib[i].N = 0;
                        contrib[i].P = new Contributor[(int) Math.Floor(2 * width + 1)];
                        double center = ((i + 0.5) / scaleWidth);
                        int left = (int) (center - width);
                        int right = (int) (center + width);

                        for (int j = left; j <= right; j++)
                        {
                            double weight = filter.GetValue((center - j - 0.5) * scaleWidth);

                            if ((weight == 0) || (j < 0) || (j >= pureImg.Width))
                                continue;

                            contrib[i].P[contrib[i].N].Pixel = j;
                            contrib[i].P[contrib[i].N].Weight = weight;
                            contrib[i].N++;
                        }
                    }
                    #endregion
                }
                else
                {
                    #region upsampling
                    for (int i = 0; i < outWidth; i++)
                    {
                        contrib[i].N = 0;
                        contrib[i].P = new Contributor[(int) Math.Floor(2 * filter.Radius + 1)];
                        double center = ((i + 0.5) / scaleWidth);
                        int left = (int) Math.Floor(center - filter.Radius);
                        int right = (int) Math.Ceiling(center + filter.Radius);

                        for (int j = left; j <= right; j++)
                        {
                            double weight = filter.GetValue(center - j - 0.5);

                            if ((weight == 0) || (j < 0) || (j >= pureImg.Width))
                                continue;

                            contrib[i].P[contrib[i].N].Pixel = j;
                            contrib[i].P[contrib[i].N].Weight = weight;
                            contrib[i].N++;
                        }
                    }
                    #endregion
                }

                #region redrawing
                using (source.UseRead())
                using (afterHorzResample.UseWrite())
                {
                    byte* srcBytes = source.Data + transparentOffset;
                    for (int srcY = 0; srcY < pureImg.Height; ++srcY)
                    {
                        for (int i = 0; i < outWidth; ++i)
                        {
                            for (int channel = 0; channel < 4; ++channel)
                            {
                                double intensity = 0;
                                double wsum = 0;

                                for (int j = 0; j < contrib[i].N; ++j)
                                {
                                    double weight = contrib[i].P[j].Weight;

                                    if (channel != 3)
                                        weight *= srcBytes[contrib[i].P[j].Pixel * 4 + srcY * source.Stride + 3] / 255d;

                                    if (weight == 0)
                                        continue;

                                    wsum += weight;
                                    intensity += (srcBytes[contrib[i].P[j].Pixel * 4 + srcY * source.Stride + channel] * weight);
                                }

                                afterHorzResample.Data[i * 4 + srcY * afterHorzResample.Stride + channel] = (byte) Math.Min(Math.Max(intensity / wsum, byte.MinValue), byte.MaxValue);
                            }
                        }
                    }
                }
                #endregion
                transparentOffset = 0;
            }
            #endregion

            #region vertical resampling
            if (scaleHeight == 1)
            {
                afterVertResample = afterHorzResample;
            }
            else
            {
                afterVertResample = new BitmapRam(outWidth, outHeight);
                var contrib = new ContributorEntry[outHeight];

                if (scaleHeight < 1f)
                {
                    #region downsampling
                    double height = filter.Radius / scaleHeight;

                    for (int i = 0; i < outHeight; i++)
                    {
                        contrib[i].N = 0;
                        contrib[i].P = new Contributor[(int) Math.Floor(2 * height + 1)];
                        double center = ((i + 0.5) / scaleHeight);
                        int top = (int) (center - height);
                        int bottom = (int) (center + height);

                        for (int j = top; j <= bottom; j++)
                        {
                            double weight = filter.GetValue((center - j - 0.5) * scaleHeight);

                            if ((weight == 0) || (j < 0) || (j >= pureImg.Height))
                                continue;

                            contrib[i].P[contrib[i].N].Pixel = j;
                            contrib[i].P[contrib[i].N].Weight = weight;
                            contrib[i].N++;
                        }
                    }
                    #endregion
                }
                else
                {
                    #region upsampling
                    for (int i = 0; i < outHeight; i++)
                    {
                        contrib[i].N = 0;
                        contrib[i].P = new Contributor[(int) Math.Floor(2 * filter.Radius + 1)];
                        double center = ((i + 0.5) / scaleHeight);
                        int left = (int) (center - filter.Radius);
                        int right = (int) (center + filter.Radius);

                        for (int j = left; j <= right; j++)
                        {
                            double weight = filter.GetValue(center - j - 0.5);

                            if ((weight == 0) || (j < 0) || (j >= pureImg.Height))
                                continue;

                            contrib[i].P[contrib[i].N].Pixel = j;
                            contrib[i].P[contrib[i].N].Weight = weight;
                            contrib[i].N++;
                        }
                    }
                    #endregion
                }

                #region redrawing
                using (afterHorzResample.UseRead())
                using (afterVertResample.UseWrite())
                {
                    byte* srcBytes = afterHorzResample.Data + transparentOffset;
                    for (int srcX = 0; srcX < outWidth; ++srcX)
                    {
                        for (int i = 0; i < outHeight; ++i)
                        {
                            for (int channel = 0; channel < 4; ++channel)
                            {
                                double intensity = 0;
                                double wsum = 0;

                                for (int j = 0; j < contrib[i].N; j++)
                                {
                                    double weight = contrib[i].P[j].Weight;

                                    if (channel != 3)
                                        weight *= srcBytes[srcX * 4 + contrib[i].P[j].Pixel * afterHorzResample.Stride + 3] / 255d;

                                    if (weight == 0)
                                        continue;

                                    wsum += weight;
                                    intensity += (srcBytes[srcX * 4 + contrib[i].P[j].Pixel * afterHorzResample.Stride + channel] * weight);
                                }

                                afterVertResample.Data[srcX * 4 + i * afterVertResample.Stride + channel] = (byte) Math.Min(Math.Max(intensity / wsum, byte.MinValue), byte.MaxValue);
                            }
                        }
                    }
                }
                #endregion
            }
            #endregion

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

        /// <summary>Implements a resampling filter.</summary>
        public abstract class Filter
        {
            public double Radius { get; protected set; }
            public abstract double GetValue(double x);
        }

        /// <summary>
        ///     Implements filters based on the Lanczos kernel. This includes the filters commonly known as "lanczos3" (radius
        ///     = 3) and "sinc256" (radius = 8).</summary>
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
