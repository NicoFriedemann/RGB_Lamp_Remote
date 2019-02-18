using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace rgb_remote
{
    class ColorUtils
    {
        //public static Bitmap screenPixel = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        public enum Direction
        {
            X,
            Y
        };

        private enum DeviceCap
        {
            DesktopVertical = 117,
            DesktopHorizontal = 118
        }

        public static Size GetPhysicalDisplaySize()
        {
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop = g.GetHdc();

            int physicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DesktopVertical);
            int physicalScreenWidth = GetDeviceCaps(desktop, (int)DeviceCap.DesktopHorizontal);

            return new Size(physicalScreenWidth, physicalScreenHeight);
        }

        public const int C_MAX_RGB_VAL = 255;
        public struct CurrentCursorDetails
        {
            public byte Red;
            public byte Green;
            public byte Blue;
            public float Hue;
            public float Saturation;
            public float Brightness;
            public int PosX;
            public int PosY;
        };

        public System.Windows.Media.Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            byte v = Convert.ToByte(value);
            byte p = Convert.ToByte(value * (1 - saturation));
            byte q = Convert.ToByte(value * (1 - f * saturation));
            byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return System.Windows.Media.Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return System.Windows.Media.Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return System.Windows.Media.Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return System.Windows.Media.Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return System.Windows.Media.Color.FromArgb(255, t, p, v);
            else
                return System.Windows.Media.Color.FromArgb(255, v, p, q);
        }

        private Color GetColorAt(Point location)
        {
            Bitmap screenPixel = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
            using (Graphics gdest = Graphics.FromImage(screenPixel))
            {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, location.X, location.Y, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }
            return screenPixel.GetPixel(0, 0);
        }

        private System.Drawing.Color CalculateAverageColor(Bitmap bm)
        {
            int width = bm.Width;
            int height = bm.Height;
            int red = 0;
            int green = 0;
            int blue = 0;
            int count = 0;
            int avgR = 0;
            int avgG = 0;
            int avgB = 0;

            int minDiversion = 0; // drop pixels that do not differ by at least minDiversion between color values (white, gray or black)
            int dropped = 0; // keep track of dropped pixels
            long[] totals = new long[] { 0, 0, 0 };
            int bppModifier = bm.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb ? 3 : 4; // cutting corners, will fail on anything else but 32 and 24 bit images

            BitmapData srcData = bm.LockBits(new System.Drawing.Rectangle(0, 0, bm.Width, bm.Height), ImageLockMode.ReadOnly, bm.PixelFormat);
            int stride = srcData.Stride;
            IntPtr Scan0 = srcData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int idx = (y * stride) + x * bppModifier;
                        red = p[idx + 2];
                        green = p[idx + 1];
                        blue = p[idx];
                        if (Math.Abs(red - green) > minDiversion || Math.Abs(red - blue) > minDiversion || Math.Abs(green - blue) > minDiversion)
                        {
                            totals[2] += red;
                            totals[1] += green;
                            totals[0] += blue;
                        }
                        else
                        {
                            dropped++;
                        }
                    }
                }
            }

            try
            {
                count = width * height - dropped;
                avgR = (int)(totals[2] / count);
                avgG = (int)(totals[1] / count);
                avgB = (int)(totals[0] / count);
            }
            catch (Exception e)
            {
                 
            }
            return System.Drawing.Color.FromArgb(avgR, avgG, avgB);
        }

        public CurrentCursorDetails GetPixelInfoAroundMousePosition(int OffsetX, int OffsetY)
        {
            Point cursor = new Point();
            GetCursorPos(ref cursor);
            var bitmap = new Bitmap(OffsetX * 2, OffsetY * 2);
            var gfxScreenshot = Graphics.FromImage(bitmap);
            gfxScreenshot.CopyFromScreen(cursor.X-OffsetX, cursor.Y-OffsetY, cursor.X+OffsetX, cursor.Y-OffsetY, bitmap.Size);     
            System.Drawing.Color col = CalculateAverageColor(bitmap);
            CurrentCursorDetails res = new CurrentCursorDetails
            {
                PosX = cursor.X,
                PosY = cursor.Y,
                Red = col.R,
                Green = col.G,
                Blue = col.B,
                Hue = col.GetHue(),
                Saturation = col.GetSaturation(),
                Brightness = col.GetBrightness()
            };
            return res;
        }


        public CurrentCursorDetails GetPixelInfoAtMousePosition()
        {
            Point cursor = new Point();
            GetCursorPos(ref cursor);
            var c = GetColorAt(cursor);
            CurrentCursorDetails res = new CurrentCursorDetails
            {
                PosX = cursor.X,
                PosY = cursor.Y,
                Red = c.R,
                Green = c.G,
                Blue = c.B,
                Hue = c.GetHue(),
                Saturation = c.GetSaturation(),
                Brightness = c.GetBrightness()
            };
            return res;
        }
    }
}
