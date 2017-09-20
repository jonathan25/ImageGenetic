using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageGenetic.Core {
    public class ImageProcessing
    {

        static byte[] GetImagePNG(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                stream.Flush();
                return stream.ToArray();
            }
        }

        static void CombineImages(ref Bitmap first, ref Bitmap second)
        {

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
            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
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
            byte[,] bitmapSquareArray = new byte[squareResolution, squareResolution];
            return bitmapSquareArray;
        }
        
        public static byte[,] GetSquareArray(Bitmap bitmap)
        {
            byte[,] bitmapArraySquare = new byte[bitmap.Width * 4, bitmap.Height];
            byte[] bitmapArray = GetBitmapArray(bitmap);
            int k = 0;
            for (int i = 0; i < bitmap.Height; i++)
            {
                for (int j = 0; j < bitmap.Width * 4; j++)
                {
                    bitmapArraySquare[i, j] = bitmapArray[k];
                    k++;
                }
            }
            return bitmapArraySquare;
        }

        public static Bitmap GetBitmap(byte[,] bitmapArraySquare)
        {
            Bitmap bitmap = new Bitmap(bitmapArraySquare.GetUpperBound(0)+1, bitmapArraySquare.GetUpperBound(1)+1);
            return bitmap;
        }

    }
}