using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace DotNet.Tools.Graphics
{
    public static class BitmapHelper
    {
        public static Bitmap Resize(Bitmap source, Size newSize)
        {
            return new Bitmap(source, newSize);
        }

        /// <summary>
        /// Compares two Bitmap objects with use of memory comparsion using BitmapData class - very fast but thread unsafe.
        /// </summary>
        public static int CompareByLockBits(Bitmap bmp1, Bitmap bmp2)
        {
            int bytes = bmp1.Width * bmp1.Height * (Image.GetPixelFormatSize(bmp1.PixelFormat) / 8);

            byte[] b1bytes = new byte[bytes];
            byte[] b2bytes = new byte[bytes];

            var rect = new Rectangle(0, 0, bmp1.Width, bmp1.Height);

            BitmapData bitmapData1 = bmp1.LockBits(rect, ImageLockMode.ReadOnly, bmp1.PixelFormat);
            BitmapData bitmapData2 = bmp2.LockBits(rect, ImageLockMode.ReadOnly, bmp2.PixelFormat);

            int result = 0;

            try
            {
                Marshal.Copy(bitmapData1.Scan0, b1bytes, 0, bytes);
                Marshal.Copy(bitmapData2.Scan0, b2bytes, 0, bytes);

                for (int n = 0; n <= bytes - 1; n++)
                {
                    if (b1bytes[n] != b2bytes[n])
                    {
                        result = 1;
                        break;
                    }
                }
            }
            finally
            {
                bmp1.UnlockBits(bitmapData1);
                bmp2.UnlockBits(bitmapData2);
            }

            return result;
        }

        /// <summary>
        /// Compares two Bitmap objects iterating all pixels with use of GetPixel - very slow, thread safe.
        /// </summary>
        public static int CompareByPixels(Bitmap bmp1, Bitmap bmp2)
        {
            for (int column = 0; column < bmp1.Width; column++)
            {
                for (int row = 0; row < bmp1.Height; row++)
                {
                    if (!bmp1.GetPixel(column, row).Equals(bmp2.GetPixel(column, row)))
                    {
                        return 1;
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// Compares two Bitmap objects iterating all pixels with use of GetPixel and calculating average error - very slow, thread safe.
        /// </summary>
        public static double CompareByPixelsAverageError(Bitmap bmp1, Bitmap bmp2)
        {
            double totalError = 0;

            for (int column = 0; column < bmp1.Width; column++)
            {
                for (int row = 0; row < bmp1.Height; row++)
                {
                    totalError += ColorHelper.CompareColours(bmp1.GetPixel(column, row), bmp2.GetPixel(column, row)) / 198608D;
                }
            }

            double averageError = totalError / (bmp1.Width * bmp1.Height);

            return averageError;
        }


        /// <summary>
        /// Compares two Bitmap objects converting them to hash codes - slower than lock bits comparsion but thread safe.
        /// </summary>
        //public static CompareResult CompareByHashCode(Bitmap bmp1, Bitmap bmp2, CompareOption options = CompareOption.None)
        //{
        //    if (PreCompare(bmp1, bmp2, out CompareResult result))
        //    {
        //        return result;
        //    }

        //    if ((options & CompareOption.ResizeTo16x16) != 0)
        //    {
        //        var tuple = ResizeBitmapsTo16x16(bmp1, bmp2);
        //        bmp1 = tuple.Item1;
        //        bmp2 = tuple.Item2;
        //    }

        //    var ic = new ImageConverter();

        //    byte[] btImage1 = new byte[1];
        //    byte[] btImage2 = new byte[1];

        //    btImage1 = (byte[])ic.ConvertTo(bmp1, btImage1.GetType());
        //    btImage2 = (byte[])ic.ConvertTo(bmp2, btImage2.GetType());

        //    var shaM = new SHA256Managed();
        //    byte[] hash1 = shaM.ComputeHash(btImage1);
        //    byte[] hash2 = shaM.ComputeHash(btImage2);

        //    for (int i = 0; i < hash1.Length && i < hash2.Length; i++)
        //    {
        //        if (hash1[i] != hash2[i])
        //        {
        //            result = CompareResult.PixelMismatch;
        //            break;
        //        }
        //    }

        //    return result;
        //}
    }
}
