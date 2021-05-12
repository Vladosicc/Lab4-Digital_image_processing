using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Windows.Media.PixelFormat;

namespace ImgConvert
{
    public static class ImageConvert
    {
        public static BitmapSource ToBitmapSource(this Bitmap bitmap, bool DeleteThisBitmap = false)
        {
            var bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bitmap.PixelFormat);
            var ptr = bmpData.Scan0;
            var Size = bmpData.Stride * bmpData.Height;
            byte[] Bytes = new byte[Size];
            System.Runtime.InteropServices.Marshal.Copy(ptr, Bytes, 0, Size);
            bitmap.UnlockBits(bmpData);
            var res = BitmapSource.Create(bitmap.Width, bitmap.Height, 96, 96, System.Windows.Media.PixelFormats.Bgr24, null, Bytes, bmpData.Stride);
            if (DeleteThisBitmap)
                bitmap.Dispose();
            return res;
        }

        public static Bitmap ToBitmap(this BitmapSource bitmaps)
        {
            int stride = (int)(bitmaps.PixelWidth * (bitmaps.Format.BitsPerPixel / 8.0));
            byte[] pixels = new byte[(int)bitmaps.PixelHeight * stride];
            bitmaps.CopyPixels(pixels, stride, 0);
            var res = new Bitmap(bitmaps.PixelWidth, bitmaps.PixelHeight);
            var bmpData = res.LockBits(new Rectangle(0, 0, res.Width, res.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, GetPixelFormat(bitmaps.Format.BitsPerPixel));
            var ptr = bmpData.Scan0;
            var Size = bmpData.Stride * bmpData.Height;
            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, ptr, Size);
            res.UnlockBits(bmpData);
            res = res.ChangeFormat(System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            return res;
        }

        public static void Save(this BitmapSource bitmaps, string path)
        {
            int stride = (int)bitmaps.PixelWidth * (bitmaps.Format.BitsPerPixel / 8);
            byte[] pixels = new byte[(int)bitmaps.PixelHeight * stride];
            bitmaps.CopyPixels(pixels, stride, 0);
            var res = new Bitmap(bitmaps.PixelWidth, bitmaps.PixelHeight);
            System.Drawing.Imaging.PixelFormat format;
            switch (bitmaps.Format.BitsPerPixel)
            {
                case 4:
                    format = System.Drawing.Imaging.PixelFormat.Format4bppIndexed;
                    break;
                case 8:
                    format = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                    break;
                case 24:
                    format = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
                    break;
                case 32:
                    format = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                    break;
                default:
                    format = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                    break;
            }
            var bmpData = res.LockBits(new Rectangle(0, 0, res.Width, res.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, format);
            var ptr = bmpData.Scan0;
            var Size = bmpData.Stride * bmpData.Height;
            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, ptr, Size);
            res.UnlockBits(bmpData);
            res.Save(path);
            res.Dispose();
        }

        public static void Save(this ImageSource bitmaps, string path)
        {
            ((BitmapSource)bitmaps).Save(path);
        }

        public static Bitmap To24bppRgb(this Bitmap bitmap) //Возвращает битмап в заданном формате
        {
            var newbit24rgb = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            bitmap.Dispose();
            return newbit24rgb;
        }

        public static BitmapSource ToBitmapSource(this byte[] Bytes, int stride, int width, int height, PixelFormat pixelFormat)
        {
            return BitmapSource.Create(width, height, 96, 96, pixelFormat, null, Bytes, stride);
        }

        public static Bitmap ChangeFormat(this Bitmap bitmap, System.Drawing.Imaging.PixelFormat px)
        {
            var newbit = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), px);
            bitmap.Dispose();
            return newbit;
        }

        public static byte[] ToByte(this BitmapSource bitmaps, out int Stride, out int Height, out int Width)
        {
            if (bitmaps.Format.BitsPerPixel != 32 || bitmaps.Format.BitsPerPixel != 24)
            {
                To32argb(ref bitmaps);
            }
            int stride = (int)(bitmaps.PixelWidth * (bitmaps.Format.BitsPerPixel / 8.0));
            Stride = stride;
            Height = bitmaps.PixelHeight;
            Width = bitmaps.PixelWidth;
            byte[] pixels = new byte[(int)bitmaps.PixelHeight * stride];
            bitmaps.CopyPixels(pixels, stride, 0);
            return pixels;
        }

        public static byte[] ToByte(this BitmapSource bitmaps, out int Stride, out int Height, out int Width, out PixelFormat pixelFormat)
        {
            if(bitmaps.Format.BitsPerPixel != 32 && bitmaps.Format.BitsPerPixel != 24)
            {
                To32argb(ref bitmaps);
            }
            int stride = (int)(bitmaps.PixelWidth * (bitmaps.Format.BitsPerPixel / 8.0));
            Stride = stride;
            Height = bitmaps.PixelHeight;
            Width = bitmaps.PixelWidth;
            pixelFormat = bitmaps.Format;
            byte[] pixels = new byte[(int)bitmaps.PixelHeight * stride];
            int max = 0;
            int min = 256;
            bitmaps.CopyPixels(pixels, stride, 0);
            return pixels;
        }

        public static byte[] ToByte(this BitmapSource bitmaps)
        {
            if (bitmaps.Format.BitsPerPixel != 32 || bitmaps.Format.BitsPerPixel != 24)
            {
                To32argb(ref bitmaps);
            }
            int stride = (int)(bitmaps.PixelWidth * (bitmaps.Format.BitsPerPixel / 8.0));
            byte[] pixels = new byte[(int)bitmaps.PixelHeight * stride];
            bitmaps.CopyPixels(pixels, stride, 0);
            return pixels;
        }

        public static void To32argb(ref BitmapSource source)
        {
            var bitmap = source.ToBitmap();
            source = bitmap.ChangeFormat(System.Drawing.Imaging.PixelFormat.Format32bppArgb).ToBitmapSource();
            bitmap.Dispose();
        }

        public static void To24rgb(ref BitmapSource source)
        {
            var bitmap = source.ToBitmap();
            source = bitmap.ChangeFormat(System.Drawing.Imaging.PixelFormat.Format24bppRgb).ToBitmapSource();
            bitmap.Dispose();
        }

        public static System.Drawing.Imaging.PixelFormat GetPixelFormat(int bitsPerPixel)
        {
            switch(bitsPerPixel)
            {
                case 1:
                    return System.Drawing.Imaging.PixelFormat.Format1bppIndexed;
                case 4:
                    return System.Drawing.Imaging.PixelFormat.Format4bppIndexed;
                case 8:
                    return System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                case 16:
                    return System.Drawing.Imaging.PixelFormat.Format16bppRgb555;
                case 24:
                    return System.Drawing.Imaging.PixelFormat.Format24bppRgb;
                case 32:
                    return System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                default:
                    return System.Drawing.Imaging.PixelFormat.Format32bppArgb;
            }
        }

        public static PixelFormat ConvertPixelFormat(this System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return System.Windows.Media.PixelFormats.Bgr24;
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return System.Windows.Media.PixelFormats.Bgra32;
                default:
                    return System.Windows.Media.PixelFormats.Bgra32;
            }
        }

        public static System.Drawing.Imaging.PixelFormat GetPixelFormat(this PixelFormat bitsPerPixel)
        {
            switch (bitsPerPixel.BitsPerPixel)
            {
                case 1:
                    return System.Drawing.Imaging.PixelFormat.Format1bppIndexed;
                case 4:
                    return System.Drawing.Imaging.PixelFormat.Format4bppIndexed;
                case 8:
                    return System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                case 16:
                    return System.Drawing.Imaging.PixelFormat.Format16bppRgb555;
                case 24:
                    return System.Drawing.Imaging.PixelFormat.Format24bppRgb;
                case 32:
                    return System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                default:
                    return System.Drawing.Imaging.PixelFormat.Format32bppArgb;

            }
        }

        public static Bitmap ReClone(this Bitmap bitmap)
        {
            var res = bitmap.Clone() as Bitmap;
            bitmap.Dispose();
            return res;
        }
    }

}
