using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageGenetic.Core
{
    public class ImageProcessing
    {
        static Random random = new Random();
        public const double GOLDEN_ANGLE = 137.50776405;
        public static byte[] GetImagePNG(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                stream.Flush();
                return stream.ToArray();
            }
        }

        public static (Bitmap, Bitmap) CombineImages(Bitmap first, Bitmap second)
        {
            byte[] arrayFirst = GetBitmapArray(first);
            byte[] arraySecond = GetBitmapArray(first);

            int height = first.Height;

            var imagesArrays = CombineImages(arrayFirst, arraySecond, height);

            return (GetBitmap(imagesArrays.Item1, height), GetBitmap(imagesArrays.Item2, height));
        }

        public static (byte[], byte[]) CombineImages(byte[] first, byte[] second, int resolutionSquare)
        {
            byte[] arrayFirstFinal = first;
            byte[] arraySecondFinal = second;

            int k = 3;
            for (int i = 0; i < resolutionSquare * resolutionSquare * 4; i += resolutionSquare * 4)
            {
                for (int j = i; j < (k + i); j += 4)
                {
                    arrayFirstFinal[j] = second[j];
                    arrayFirstFinal[j + 1] = second[j + 1];
                    arrayFirstFinal[j + 2] = second[j + 2];

                    arraySecondFinal[j] = first[j];
                    arraySecondFinal[j + 1] = first[j + 1];
                    arraySecondFinal[j + 2] = first[j + 2];
                }
                k += 4;
            }

            return (arrayFirstFinal, arraySecondFinal);
        }

        public static Bitmap ChangeHueHalf(Bitmap bitmap, bool half, double degrees)
        {
            int height = bitmap.Height;
            byte[] bitmapArray = GetBitmapArray(bitmap);
            ChangeHueHalf(bitmapArray, height, half, degrees);

            return GetBitmap(bitmapArray, height);
        }

        public static byte[] ChangeHueHalf(byte[] bitmapArray, int resolutionSquare, bool half, double degrees)
        {
            int k = 3;
            for (int i = 0; i < resolutionSquare * resolutionSquare * 4; i += resolutionSquare * 4)
            {
                if (half)
                {
                    for (int j = i; j < (k + i); j += 4)
                    {
                        var hsl = FromRGBToHSL(bitmapArray[j + 2], bitmapArray[j + 1], bitmapArray[j]);
                        if (hsl.Item2 != 0 && hsl.Item3 != 0)
                        {
                            hsl.Item1 = (hsl.Item1 + degrees / 360.0) % 1;
                            var rgb = FromHSLtoRGB(hsl.Item1, hsl.Item2, hsl.Item3);
                            bitmapArray[j] = rgb.Item3;
                            bitmapArray[j + 1] = rgb.Item2;
                            bitmapArray[j + 2] = rgb.Item1;
                        }

                    }
                    k += 4;
                }
                else
                {
                    for (int j = i + k + 1; j < i + resolutionSquare * 4; j += 4)
                    {
                        var hsl = FromRGBToHSL(bitmapArray[j + 2], bitmapArray[j + 1], bitmapArray[j]);
                        if (hsl.Item2 != 0 && hsl.Item3 != 0)
                        {
                            hsl.Item1 = (hsl.Item1 + degrees / 360.0) % 1;
                            var rgb = FromHSLtoRGB(hsl.Item1, hsl.Item2, hsl.Item3);
                            bitmapArray[j] = rgb.Item3;
                            bitmapArray[j + 1] = rgb.Item2;
                            bitmapArray[j + 2] = rgb.Item1;
                        }
                    }
                }
            }

            return bitmapArray;
        }

        public static Bitmap LuminosityRandom(Bitmap bitmap)
        {
            int height = bitmap.Height;
            byte[] bitmapArray = GetBitmapArray(bitmap);
            bitmapArray = LuminosityRandom(bitmapArray, height);
            return GetBitmap(bitmapArray, height);
        }

        public static byte[] LuminosityRandom(byte[] bitmapArray, int resolutionSquare)
        {
            int k = 4;
            for (int i = 0; i < resolutionSquare * resolutionSquare * 4; i += resolutionSquare * 4)
            {
                for (int j = i; j < (k + i); j += 4)
                {
                    if (random.NextDouble() < 0.5)
                    {
                        var hsl = FromRGBToHSL(bitmapArray[j + 2], bitmapArray[j + 1], bitmapArray[j]);
                        var pixel = FromHSLtoRGB(random.NextDouble(), hsl.Item2, hsl.Item3);

                        bitmapArray[j] = pixel.Item3;
                        bitmapArray[j + 1] = pixel.Item2;
                        bitmapArray[j + 2] = pixel.Item1;
                    }
                    if ((j + 4) < i + k)
                    {
                        var hsl1 = FromRGBToHSL(bitmapArray[j + 2], bitmapArray[j + 1], bitmapArray[j]);
                        var hsl2 = FromRGBToHSL(bitmapArray[j + 2 + 4], bitmapArray[j + 1 + 4], bitmapArray[j + 4]);

                        if (hsl1.Item1 > hsl2.Item1)
                        {
                            byte b1 = bitmapArray[j];
                            byte g1 = bitmapArray[j + 1];
                            byte r1 = bitmapArray[j + 2];

                            byte b2 = bitmapArray[j + 4];
                            byte g2 = bitmapArray[j + 4 + 1];
                            byte r2 = bitmapArray[j + 4 + 2];
                            bitmapArray[j] = b2;
                            bitmapArray[j + 1] = g2;
                            bitmapArray[j + 2] = r2;

                            bitmapArray[j + 4] = b1;
                            bitmapArray[j + 1 + 4] = g1;
                            bitmapArray[j + 2 + 4] = r1;
                        }
                    }
                }
                for (int j = i + k; j < i + resolutionSquare * 4; j += 4)
                {
                    if (random.NextDouble() < 0.5)
                    {
                        var hsl = FromRGBToHSL(bitmapArray[j + 2], bitmapArray[j + 1], bitmapArray[j]);
                        var pixel = FromHSLtoRGB(random.NextDouble(), hsl.Item2, hsl.Item3);

                        bitmapArray[j] = pixel.Item3;
                        bitmapArray[j + 1] = pixel.Item2;
                        bitmapArray[j + 2] = pixel.Item1;
                    }
                    if ((j + 4) < i + resolutionSquare * 4)
                    {
                        var hsl1 = FromRGBToHSL(bitmapArray[j + 2], bitmapArray[j + 1], bitmapArray[j]);
                        var hsl2 = FromRGBToHSL(bitmapArray[j + 2 + 4], bitmapArray[j + 1 + 4], bitmapArray[j + 4]);

                        if (hsl2.Item1 > hsl1.Item1)
                        {
                            byte b1 = bitmapArray[j];
                            byte g1 = bitmapArray[j + 1];
                            byte r1 = bitmapArray[j + 2];

                            byte b2 = bitmapArray[j + 4];
                            byte g2 = bitmapArray[j + 4 + 1];
                            byte r2 = bitmapArray[j + 4 + 2];
                            bitmapArray[j] = b2;
                            bitmapArray[j + 1] = g2;
                            bitmapArray[j + 2] = r2;

                            bitmapArray[j + 4] = b1;
                            bitmapArray[j + 4 + 1] = g1;
                            bitmapArray[j + 4 + 2] = r1;
                        }
                    }
                }
                k += 4;
            }

            return bitmapArray;
        }

        public static ((byte, byte, byte), (byte, byte, byte)) GetAverages(Bitmap bitmap)
        {
            int height = bitmap.Height;
            byte[] bitmapArray = GetBitmapArray(bitmap);
            return GetAverages(bitmapArray, height);
        }

        public static ((byte, byte, byte), (byte, byte, byte)) GetAverages(byte[] bitmapArray, int resolutionSquare)
        {
            (long, long, long) sum1 = (0, 0, 0);
            (long, long, long) sum2 = (0, 0, 0);

            (byte, byte, byte) average1 = (0, 0, 0);
            (byte, byte, byte) average2 = (0, 0, 0);

            int k = 3;
            for (int i = 0; i < resolutionSquare * resolutionSquare * 4; i += resolutionSquare * 4)
            {
                for (int j = i; j < (k + i); j += 4)
                {
                    sum1.Item3 += bitmapArray[j];
                    sum1.Item2 += bitmapArray[j + 1];
                    sum1.Item1 += bitmapArray[j + 2];
                }
                for (int j = i + k + 1; j < i + resolutionSquare * 4; j += 4)
                {
                    sum2.Item3 += bitmapArray[j];
                    sum2.Item2 += bitmapArray[j + 1];
                    sum2.Item1 += bitmapArray[j + 2];
                }
                k += 4;
            }
            double totalPixels = resolutionSquare * resolutionSquare / 2.0;
            average1.Item1 = (byte)Math.Round(sum1.Item1 / (totalPixels));
            average1.Item2 = (byte)Math.Round(sum1.Item2 / (totalPixels));
            average1.Item3 = (byte)Math.Round(sum1.Item3 / (totalPixels));
            average2.Item1 = (byte)Math.Round(sum2.Item1 / (totalPixels));
            average2.Item2 = (byte)Math.Round(sum2.Item2 / (totalPixels));
            average2.Item3 = (byte)Math.Round(sum2.Item3 / (totalPixels));

            return (average1, average2);
        }

        public static (double, double) GetAveragesHue(Bitmap bitmap)
        {
            double average1 = 0;
            double average2 = 0;

            double height = bitmap.Height;
            int width = bitmap.Width;
            byte[] bitmapArray = GetBitmapArray(bitmap);
            int k = 3;
            for (int i = 0; i < height * width * 4; i += width * 4)
            {
                for (int j = i; j < (k + i); j += 4)
                {
                    byte b = bitmapArray[j];
                    byte g = bitmapArray[j + 1];
                    byte r = bitmapArray[j + 2];
                    var hue = GetHue(r, g, b);
                    average1 += hue;
                }
                for (int j = i + k + 1; j < i + width * 4; j += 4)
                {
                    byte b = bitmapArray[j];
                    byte g = bitmapArray[j + 1];
                    byte r = bitmapArray[j + 2];
                    var hue = GetHue(r, g, b);
                    average2 += hue;
                }
                k += 4;
            }
            double half = (height * width / 2);
            average1 = average1 / half;
            average2 = average2 / half;

            return (average1, average2);
        }

        public static (double, double) GetAveragesHue(byte[] bitmapArray, int resolutionSquare)
        {
            double average1 = 0;
            double average2 = 0;

            int k = 3;
            for (int i = 0; i < resolutionSquare * resolutionSquare * 4; i += resolutionSquare * 4)
            {
                for (int j = i; j < (k + i); j += 4)
                {
                    byte b = bitmapArray[j];
                    byte g = bitmapArray[j + 1];
                    byte r = bitmapArray[j + 2];
                    var hue = GetHue(r, g, b);
                    average1 += hue;
                }
                for (int j = i + k + 1; j < i + resolutionSquare * 4; j += 4)
                {
                    byte b = bitmapArray[j];
                    byte g = bitmapArray[j + 1];
                    byte r = bitmapArray[j + 2];
                    var hue = GetHue(r, g, b);
                    average2 += hue;
                }
                k += 4;
            }
            double half = (resolutionSquare * resolutionSquare / 2);
            average1 = average1 / half;
            average2 = average2 / half;

            return (average1, average2);
        }

        public static ((double, double, double), (double, double, double)) GetAveragesHSL(Bitmap bitmap)
        {
            (double, double, double) average1 = (0, 0, 0);
            (double, double, double) average2 = (0, 0, 0);

            double height = bitmap.Height;
            int width = bitmap.Width;
            byte[] bitmapArray = GetBitmapArray(bitmap);
            int k = 3;
            for (int i = 0; i < height * width * 4; i += width * 4)
            {
                for (int j = i; j < (k + i); j += 4)
                {
                    byte b = bitmapArray[j];
                    byte g = bitmapArray[j + 1];
                    byte r = bitmapArray[j + 2];
                    var hsl = FromRGBToHSL(r, g, b);
                    average1.Item1 += hsl.Item1;
                    average1.Item2 += hsl.Item2;
                    average1.Item3 += hsl.Item3;
                }
                for (int j = i + k + 1; j < i + width * 4; j += 4)
                {
                    byte b = bitmapArray[j];
                    byte g = bitmapArray[j + 1];
                    byte r = bitmapArray[j + 2];
                    var hsl = FromRGBToHSL(r, g, b);
                    average2.Item1 += hsl.Item1;
                    average2.Item2 += hsl.Item2;
                    average2.Item3 += hsl.Item3;
                }
                k += 4;
            }
            double half = (height * width / 2);
            average1.Item1 = average1.Item1 / half;
            average1.Item2 = average1.Item2 / half;
            average1.Item3 = average1.Item3 / half;
            average2.Item1 = average2.Item1 / half;
            average2.Item2 = average2.Item2 / half;
            average2.Item3 = average2.Item3 / half;

            return (average1, average2);
        }

        public static byte[] GenerateRandomImageArray(int resolutionSquare)
        {
            byte[] bitmapArray = new byte[resolutionSquare * resolutionSquare * 4];
            random.NextBytes(bitmapArray);
            for (int i = 3; i < bitmapArray.Length; i += 4)
            {
                bitmapArray[i] = 255;
            }
            return bitmapArray;
        }

        public static Bitmap GenerateRandomImage(int resolutionSquare)
        {
            byte[] bitmapArray = new byte[resolutionSquare * resolutionSquare * 4];
            random.NextBytes(bitmapArray);
            for (int i = 3; i < bitmapArray.Length; i += 4)
            {
                bitmapArray[i] = 255;
            }
            return GetBitmap(bitmapArray, resolutionSquare);
        }

        public static byte[] GetBitmapArray(int squareResolution)
        {
            Bitmap bitmap = new Bitmap(squareResolution, squareResolution);
            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, squareResolution, squareResolution),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            byte[] bitmapArray = new byte[data.Stride * data.Height];
            Marshal.Copy(data.Scan0, bitmapArray, 0, bitmapArray.Length);
            bitmap.UnlockBits(data);
            bitmap.Dispose();

            return bitmapArray;
        }

        public static byte[] GetBitmapArray(Bitmap bitmap)
        {
            bitmap = new Bitmap(bitmap);
            int height = bitmap.Height;
            int width = bitmap.Width;
            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            byte[] bitmapArray = new byte[data.Stride * data.Height];
            Marshal.Copy(data.Scan0, bitmapArray, 0, bitmapArray.Length);
            bitmap.UnlockBits(data);
            bitmap.Dispose();

            return bitmapArray;
        }

        public static byte[,] GetSquareArray(int squareResolution)
        {
            byte[,] bitmapSquareArray = new byte[squareResolution, squareResolution * 4];
            return bitmapSquareArray;
        }

        public static byte[,] GetSquareArray(Bitmap bitmap)
        {
            int height = bitmap.Height;
            int width = bitmap.Width * 4;
            byte[,] bitmapArraySquare = new byte[height, width];
            byte[] bitmapArray = GetBitmapArray(bitmap);
            int k = 0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    bitmapArraySquare[i, j] = bitmapArray[k];
                    k++;
                }
            }
            return bitmapArraySquare;
        }

        public static Bitmap GetBitmap(byte[,] bitmapArraySquare)
        {
            byte[] bitmapArray = new byte[bitmapArraySquare.Length];
            int k = 0;
            for (int i = 0; i < bitmapArray.GetUpperBound(0); i++)
            {
                for (int j = 0; j < bitmapArray.GetUpperBound(1); j++)
                {
                    bitmapArray[k] = bitmapArraySquare[i, j];
                    k++;
                }
            }
            return GetBitmap(bitmapArray, bitmapArray.GetUpperBound(0));
        }

        public static Bitmap GetBitmap(byte[] bitmapArray, int resolutionSquare)
        {
            Bitmap bitmap = new Bitmap(resolutionSquare, resolutionSquare);
            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, resolutionSquare, resolutionSquare),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            Marshal.Copy(bitmapArray, 0, data.Scan0, resolutionSquare * resolutionSquare * 4);
            bitmap.UnlockBits(data);

            return bitmap;
        }

        /*public static Dictionary<uint, int> GetHistogram(Bitmap bitmap)
        {
            
            Dictionary<uint, int> histogram = new Dictionary<uint, int>();
            byte[] bitmapArray = GetBitmapArray(bitmap);
            for (int i = 0; i < bitmapArray.Length; i += 4)
            {
                uint pixel = BitConverter.ToUInt32(bitmapArray, i);
                if (histogram.ContainsKey(pixel))
                {
                    histogram[pixel] = histogram[pixel] + 1;
                }
                else
                {
                    histogram.Add(pixel, 1);
                }
            }
            return histogram;
        }*/

        private static double HueToRGBChannel(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1 / 6.0) return p + (q - p) * 6 * t;
            if (t < 1 / 2.0) return q;
            if (t < 2 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6;
            return p;
        }

        public static (byte, byte, byte) FromHSLtoRGB(double h, double s, double l)
        {
            double r, g, b;

            if (s == 0)
            {
                r = g = b = l; // achromatic
            }
            else
            {
                var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                var p = 2 * l - q;
                r = HueToRGBChannel(p, q, h + 1.0 / 3.0);
                g = HueToRGBChannel(p, q, h);
                b = HueToRGBChannel(p, q, h - 1.0 / 3.0);
            }

            byte R = (byte)Math.Round(r * 255);
            byte G = (byte)Math.Round(g * 255);
            byte B = (byte)Math.Round(b * 255);

            return (R, G, B);
        }

        public static (double, double, double) FromRGBToHSL((byte, byte, byte) rgb)
        {
            return FromRGBToHSL(rgb.Item1, rgb.Item2, rgb.Item3);
        }

        public static (double, double, double) FromRGBToHSL(byte R, byte G, byte B)
        {
            double r = R / 255.0;
            double g = G / 255.0;
            double b = B / 255.0;
            var max = Math.Max(Math.Max(r, g), b);
            var min = Math.Min(Math.Min(r, g), b);
            double h = 0;
            double s;
            double l = (max + min) / 2.0;

            if (max == min)
            {
                h = s = 0; // achromatic
            }
            else
            {
                var d = max - min;
                s = l > 0.5 ? d / (2 - max - min) : d / (max + min);

                if (max == r)
                {
                    h = (g - b) / d + (g < b ? 6 : 0);
                }
                else if (max == g)
                {
                    h = (b - r) / d + 2;
                }
                else if (max == b)
                {
                    h = (r - g) / d + 4;
                }

                h = h / 6.0;
            }

            return (h, s, l);
        }

        public static double GetHue(byte r, byte g, byte b)
        {
            var max = Math.Max(Math.Max(r, g), b);
            var min = Math.Min(Math.Min(r, g), b);
            double h = 0;
            if (max == min)
            {
                h = 0; // achromatic
            }
            else
            {
                var d = max - min;
                if (max == r)
                {
                    h = (g - b) / d + (g < b ? 6 : 0);
                }
                else if (max == g)
                {
                    h = (b - r) / d + 2;
                }
                else if (max == b)
                {
                    h = (r - g) / d + 4;
                }

                h = h / 6.0;
            }
            return h;
        }

        public static Histogram GetHistogram(Bitmap bitmap)
        {
            Histogram histogram = new Histogram();

            byte[] bitmapArray = GetBitmapArray(bitmap);
            for (int i = 0; i < bitmapArray.Length; i += 4)
            {
                histogram.Blue[bitmapArray[i]] = histogram.Blue[bitmapArray[i]] + 1;
                histogram.Green[bitmapArray[i + 1]] = histogram.Blue[bitmapArray[i + 1]] + 1;
                histogram.Red[bitmapArray[i + 2]] = histogram.Blue[bitmapArray[i + 2]] + 1;
            }
            return histogram;
        }

        public class Histogram
        {
            public Dictionary<byte, int> Red { get; set; }
            public Dictionary<byte, int> Green { get; set; }
            public Dictionary<byte, int> Blue { get; set; }

            public Histogram()
            {
                Red = new Dictionary<byte, int>();
                Green = new Dictionary<byte, int>();
                Blue = new Dictionary<byte, int>();
                for (int i = 0; i <= 255; i++)
                {
                    Red.Add((byte)i, 0);
                    Green.Add((byte)i, 0);
                    Blue.Add((byte)i, 0);
                }
            }
        }

        /*public static Bitmap GetBitmap(byte[,] bitmapArraySquare)
        {
            Bitmap bitmap = new Bitmap(bitmapArraySquare.GetUpperBound(0) + 1, bitmapArraySquare.GetUpperBound(1) + 1);
            return bitmap;
        }*/

    }
}