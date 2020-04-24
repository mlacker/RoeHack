using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoeHack.Library.Core.Logging;

namespace RoeHack.Library.Core.LockHead
{
    public class DoLockHead : IDoLockHead
    {
        IDetect detect;
        IMoveMouse moveMouse;
        ILog logger;
        LockHeadOpencv lockHeadOpencv;

        public DoLockHead()
        {
            //detect = new Detect();
            moveMouse = new MoveMouse();
            logger = new ConsoleLogger("DoLockHead", LogLevel.Debug);
            lockHeadOpencv = new LockHeadOpencv();
        }
        public void LockHeadOld(byte[] picStream, Image image, int picWidth, int picHeight)
        {
            //// 识别截图 
            //var detectItems = detect.DetectFromBytes(picStream);

            //// 获取移动的x,y

            //int moveX = 0;
            //int moveY = 0;
            //if (detectItems.Count() > 0)
            //{
            //    var item = detectItems.OrderBy(r => r.Width).First();
            //    if (item.Width < 30)
            //    {
            //        moveX = item.Width / 2 + item.X - (picWidth / 2);
            //        moveY = item.Height / 2 + item.Y - (picHeight / 2);
            //        string con = item.Confidence.ToString().Substring(0, 3);
            //        logger.Debug("识别成功" + con + "mx:" + moveX + " my:" + moveY + " x:" + item.X + " y:" + item.Y + " w:" + item.Width + " h:" + item.Height);
            //        image.Save("d:\\test\\" + con + "mx-" + moveX + "_my-" + moveY + "_x-" + item.X + "_y-" + item.Y + "_w-" + item.Width + "_h-" + item.Height + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            //        // 移动鼠标
            //        IMoveMouse moveMouse = new MoveMouse();
            //        moveMouse.MoveMouse(moveX, moveY);
            //    }

            //}
        }

        public void LockHead(Image image, int picWidth, int picHeight)
        {
            // 识别截图 
            var detectItem = lockHeadOpencv.DetectHead(image);
            
            if (detectItem.X!=0&& detectItem.Y !=0)
            {
                //image.Save("D:\\test\\"+ "x-"+detectItem.X+"_y-"+detectItem.Y+".png");
                //logger.Debug("耗时：" + detectItem.TimeSpan);
                // 获取移动的x,y
                int moveX = 0;
                int moveY = 0;
                moveX = detectItem.X - (picWidth / 2);
                moveY = detectItem.Y - (picHeight / 2);
                IMoveMouse moveMouse = new MoveMouse();
                moveMouse.MoveMouse(moveX, moveY);
                //logger.Debug("yidong x:" + moveX+ " Y" + moveY);
            }
        }
    }
}
