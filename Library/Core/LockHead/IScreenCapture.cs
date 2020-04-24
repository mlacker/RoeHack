using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoeHack.Library.Core.LockHead
{
   public interface IScreenCapture
    {
        Image CaptureWindow(IntPtr handle,int picWidth);
        bool CheckIsForeground(IntPtr hWnd);
        bool SetForegroundWindow(IntPtr hWnd);
    }
}
