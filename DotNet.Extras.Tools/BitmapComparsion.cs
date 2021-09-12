// -----------------------------------------------------------------------
// <copyright file="BitmapComparsion.cs" company="Laura Kolcavova">
// Copyright (c) Laura Kolcavova. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace DotNet.Extras.Tools
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    /// <summary>
    /// Helper class for Bitmap comparsion.
    /// </summary>
    public static class BitmapComparsion
    {
        /// <summary>
        /// Enum for comaparing result.
        /// </summary>
        public enum CompareResult
        {
            /// <summary>
            /// None.
            /// </summary>
            None,

            /// <summary>
            /// Ok.
            /// </summary>
            Ok,

            /// <summary>
            /// Pixel mismatch.
            /// </summary>
            PixelMismatch,

            /// <summary>
            /// Size mismatch.
            /// </summary>
            SizeMismatch,
        }

        /// <summary>
        /// Enum for compare options.
        /// </summary>
        [Flags]
        public enum CompareOption
        {
            /// <summary>
            /// None.
            /// </summary>
            None,

            /// <summary>
            /// Images will be resized to 16x16 pixels.
            /// </summary>
            ResizeTo16x16,
        }

        /// <summary>
        /// Compares two Bitmap objects with use of memory comparsion using BitmapData class - very fast but thread unsafe.
        /// </summary>
        /// <param name="bmp1">First Bitmap object.</param>
        /// <param name="bmp2">Second Bitmap object.</param>
        /// <param name="options">Compare options.</param>
        /// <returns><see cref="CompareResult"/>.</returns>
        public static CompareResult CompareByLockBits(Bitmap bmp1, Bitmap bmp2, CompareOption options = CompareOption.None)
        {
            if (PreCompare(bmp1, bmp2, out CompareResult result))
            {
                return result;
            }

            if ((options & CompareOption.ResizeTo16x16) != 0)
            {
                var tuple = ResizeBitmapsTo16x16(bmp1, bmp2);
                bmp1 = tuple.Item1;
                bmp2 = tuple.Item2;
            }

            int bytes = bmp1.Width * bmp1.Height * (Image.GetPixelFormatSize(bmp1.PixelFormat) / 8);

            byte[] b1bytes = new byte[bytes];
            byte[] b2bytes = new byte[bytes];

            var rect = new Rectangle(0, 0, bmp1.Width, bmp1.Height);

            BitmapData bitmapData1 = bmp1.LockBits(rect, ImageLockMode.ReadOnly, bmp1.PixelFormat);
            BitmapData bitmapData2 = bmp2.LockBits(rect, ImageLockMode.ReadOnly, bmp2.PixelFormat);

            try
            {
                Marshal.Copy(bitmapData1.Scan0, b1bytes, 0, bytes);
                Marshal.Copy(bitmapData2.Scan0, b2bytes, 0, bytes);

                for (int n = 0; n <= bytes - 1; n++)
                {
                    if (b1bytes[n] != b2bytes[n])
                    {
                        result = CompareResult.PixelMismatch;
                        break;
                    }
                }
            }
            catch
            {
                result = CompareResult.None;
            }
            finally
            {
                bmp1.UnlockBits(bitmapData1);
                bmp2.UnlockBits(bitmapData2);
            }

            return result;
        }

        /// <summary>
        /// Compares two Bitmap objects converting them to hash codes - slower than lock bits comparsion but thread safe.
        /// </summary>
        /// <param name="bmp1">First Bitmap object.</param>
        /// <param name="bmp2">Second Bitmap object.</param>
        /// <param name="options">Compare options.</param>
        /// <returns><see cref="CompareResult"/>.</returns>
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

        /// <summary>
        /// Compares two Bitmap objects iterating all pixels with use of GetPixel - very slow, thread safe.
        /// </summary>
        /// <param name="bmp1">First Bitmap object.</param>
        /// <param name="bmp2">Second Bitmap object.</param>
        /// <param name="options">Compare options.</param>
        /// <returns><see cref="CompareResult"/>.</returns>
        public static CompareResult CompareByPixels(Bitmap bmp1, Bitmap bmp2, CompareOption options = CompareOption.None)
        {
            if (PreCompare(bmp1, bmp2, out CompareResult result))
            {
                return result;
            }

            if ((options & CompareOption.ResizeTo16x16) != 0)
            {
                var tuple = ResizeBitmapsTo16x16(bmp1, bmp2);
                bmp1 = tuple.Item1;
                bmp2 = tuple.Item2;
            }

            for (int column = 0; column < bmp1.Width && result == CompareResult.Ok; column++)
            {
                for (int row = 0; row < bmp1.Height; row++)
                {
                    if (!bmp1.GetPixel(column, row).Equals(bmp2.GetPixel(column, row)))
                    {
                        result = CompareResult.PixelMismatch;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Compares two Bitmap objects iterating all pixels with use of GetPixel and calculating average error which is compared with given error threshold - very slow, thread safe.
        /// </summary>
        /// <param name="bmp1">First Bitmap object.</param>
        /// <param name="bmp2">Second Bitmap object.</param>
        /// <param name="errorThreshold">Error threshold.</param>
        /// <param name="options">Compare options.</param>
        /// <returns><see cref="CompareResult"/>.</returns>
        public static CompareResult CompareByPixelWithTolerance(Bitmap bmp1, Bitmap bmp2, double errorThreshold = 0.0D, CompareOption options = CompareOption.None)
        {
            if (PreCompare(bmp1, bmp2, out CompareResult result))
            {
                return result;
            }

            if ((options & CompareOption.ResizeTo16x16) != 0)
            {
                var tuple = ResizeBitmapsTo16x16(bmp1, bmp2);
                bmp1 = tuple.Item1;
                bmp2 = tuple.Item2;
            }

            double totalError = 0;

            for (int column = 0; column < bmp1.Width; column++)
            {
                for (int row = 0; row < bmp1.Height; row++)
                {
                    totalError += CompareColours(bmp1.GetPixel(column, row), bmp2.GetPixel(column, row)) / 198608D;
                }
            }

            double averageError = totalError / (bmp1.Width * bmp1.Height);

            if (averageError > errorThreshold)
            {
                result = CompareResult.PixelMismatch;
            }

            return result;
        }

        /// <summary>
        /// Compares two given <see cref="Color"/>s using Pythagorean distance between the Red, Green and Blue components.
        /// </summary>
        /// <param name="c1">Color 1.</param>
        /// <param name="c2">Color 2.</param>
        /// <returns>Value of how different two given <see cref="Color"/>s are.</returns>
        public static int CompareColours(Color c1, Color c2)
        {
            return (int)(Math.Pow((int)c1.R - c2.R, 2) + Math.Pow((int)c1.B - c2.B, 2) + Math.Pow((int)c1.G - c2.G, 2));
        }

        private static Tuple<Bitmap, Bitmap> ResizeBitmapsTo16x16(Bitmap bmp1, Bitmap bmp2)
        {
            var size16x16 = new Size(16, 16);

            return new Tuple<Bitmap, Bitmap>(
                new Bitmap(bmp1, size16x16),
                new Bitmap(bmp2, size16x16)
            );
        }

        private static bool PreCompare(Bitmap bmp1, Bitmap bmp2, out CompareResult result)
        {
            bool compared = false;
            result = CompareResult.Ok;

            if (bmp1 == null || bmp2 == null)
            {
                result = CompareResult.None;
                compared = true;
            }

            if (Bitmap.Equals(bmp1, bmp2))
            {
                result = CompareResult.Ok;
                compared = true;
            }

            if (!Size.Equals(bmp1.Size, bmp2.Size) || !PixelFormat.Equals(bmp1.PixelFormat, bmp2.PixelFormat))
            {
                result = CompareResult.SizeMismatch;
                compared = true;
            }

            return compared;
        }
    }
}
