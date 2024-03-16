using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Kaye.Live
{
    public class ImageTools
    {
        [DllImport("kernel32.dll", EntryPoint = "GetConsoleWindow", SetLastError = true)]
        private static extern IntPtr GetConsoleHandle();
        static IntPtr handler = GetConsoleHandle();

        public static void DisplayImage(Bitmap img, Int32 x, Int32 y)
        {

            using (Graphics graphics = Graphics.FromHwnd(handler))
                graphics.DrawImage(img, x, y, img.Width, img.Height);
        }

        public static void DisplayImageReset(Bitmap img, Int32 x, Int32 y)
        {

            while (true) DisplayImage(img, x, y);
        }

        public static Bitmap ResizeBitmap(Bitmap img, Int32 maxwidth, Int32 maxheight)
        {
            Double ratio = (Double)img.Width / (Double)img.Height;
            Int32 nw = img.Width, nh = img.Height;
            if (nw > maxwidth)
            {
                nw = maxwidth;
                nh = (Int32)Math.Ceiling((Decimal)nw / (Decimal)ratio);
            }

            if (nh > maxheight)
            {
                nh = maxheight;
                nw = (Int32)Math.Ceiling((Decimal)nh * (Decimal)ratio);
            }
            Bitmap image = new Bitmap(img, nw, nh);
            return image;
        }

        public static Bitmap EmptyBitmap(Color Background, Size Size)
        {
            Bitmap Output = new Bitmap(Size.Width, Size.Height);
            using (Graphics g = Graphics.FromImage(Output)) g.Clear(Background);
            return Output;
        }
    }
}