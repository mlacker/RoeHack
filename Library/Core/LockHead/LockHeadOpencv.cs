using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RoeHack.Library.Core.LockHead
{
    public class LockHeadOpencv
    {
        public LockHeadRect DetectHead(Image image)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Bitmap bitmap = new Bitmap(image);
            Mat img = OpenCvSharp.Extensions.BitmapConverter.ToMat(bitmap);
            var img2 = img;
            //Cv2.SetDevice(0);
            //Cv2.ImShow("1", img);
            Cv2.CvtColor(img, img, ColorConversionCodes.BGR2HSV_FULL);
            //Cv2.ImShow("2", img);

            Scalar low_value = new Scalar(0, 170, 140);
            Scalar high_value = new Scalar(10, 255, 255);
            Cv2.InRange(img, low_value, high_value, img);
            //Cv2.GaussianBlur(img, img, new OpenCvSharp.Size(5,5), 1.5);
            Cv2.MedianBlur(img, img, 5);

            HierarchyIndex[] hierarchy;
            OpenCvSharp.Point[][] coutours;
            Cv2.FindContours(img, out coutours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxNone);
            //Cv2.DrawContours(img_rgb, coutours, -1, new Scalar(0, 255, 0), 3);
            //Cv2.ImShow("耗时：" + sw.ElapsedMilliseconds, img_rgb);
            List<Rect> coutourRects = new List<Rect>();
            foreach (var coutour in coutours)
            {
                coutourRects.Add(Cv2.BoundingRect(coutour));
            }
            LockHeadRect lockHeadRect = new LockHeadRect();
            lockHeadRect.X = 0;
            lockHeadRect.Y = 0;
            coutourRects = coutourRects.Where(r => r.Height > 2 && r.Height < 60).ToList();
            if (coutourRects.Count > 0)
            {
                var detectCoutour = coutourRects.OrderByDescending(r => r.Height).First();
                lockHeadRect.X = detectCoutour.X + detectCoutour.Width / 2;
                lockHeadRect.Y = detectCoutour.Y- detectCoutour.Width / 4;
                lockHeadRect.TimeSpan = sw.ElapsedMilliseconds.ToString();

                //Cv2.Rectangle(img_rgb, detectCoutour, new Scalar(0, 0, 255), 3);
            }
            return lockHeadRect;
        }
        public struct LockHeadRect
        {
            public int X { get; set; }
            public int Y { get; set; }
            public string TimeSpan { get; set; }
        }
    }
}
