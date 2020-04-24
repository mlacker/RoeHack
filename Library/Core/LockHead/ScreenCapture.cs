using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RoeHack.Library.Core.LockHead
{
    public class ScreenCapture : IScreenCapture
    {
        public System.Drawing.Image CaptureWindow(IntPtr handle, int picWidth)
        {
            try
            {
                //User32.SetForegroundWindow(handle);
                Rectangle bRect = new Rectangle();
                IntPtr hdcSrc = User32.GetWindowDC(handle);
                User32.RECT wRect = new User32.RECT();
                User32.GetWindowRect(handle, ref wRect);
                bRect.Width = picWidth;
                bRect.Height = picWidth;
                bRect.X = (wRect.right - wRect.left) / 2 - (picWidth / 2);
                bRect.Y = (wRect.bottom - wRect.top) / 2 - (picWidth / 2);

                //bRect.Width = picWidth;
                //bRect.Height = picWidth;
                //bRect.X = 0;// (wRect.right - wRect.left) / 2 - (picWidth / 2);
                //bRect.Y = 0;// (wRect.bottom - wRect.top) / 2 - (picWidth / 2);

                IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
                IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, bRect.Width, bRect.Height);
                IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
                GDI32.BitBlt(hdcDest, 0, 0, bRect.Width, bRect.Height, hdcSrc, bRect.Left, bRect.Top, GDI32.SRCCOPY);
                GDI32.SelectObject(hdcDest, hOld);
                System.Drawing.Image img = System.Drawing.Image.FromHbitmap(hBitmap);
                GDI32.DeleteDC(hdcDest);
                User32.ReleaseDC(handle, hdcSrc);
                GDI32.DeleteObject(hBitmap);
                return img;
            }
            catch (Exception ex)
            {

                return null;
            }

        }

        
        public  bool CheckIsForeground(IntPtr hWnd)
        {

            if (hWnd== User32.GetForegroundWindow())
            {
                return true;
            }
            return false;
        }




        public  bool SetForegroundWindow(IntPtr hWnd)
        {
            return User32.SetForegroundWindow(hWnd);
        }

        //截图函数声明
        private class GDI32
        {
            public const int SRCCOPY = 0x00CC0020;
            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hObjectSource, int nXSrc, int nYSrc, int

  dwRop);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        }
        private class User32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }
            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);

            [DllImport("user32.dll")]
            public static extern bool SetForegroundWindow(IntPtr hWnd);

            public const int WS_EX_TOPMOST = 0x00000008;
            [DllImport("user32.dll", SetLastError = true)]
            public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

            [DllImport("user32.dll")]
            public static extern IntPtr GetForegroundWindow();

        }

    }
}
