using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace LJC.FrameWork.Comm
{

    public class ImageHelper
    {
        [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
        private static extern bool BitBlt(
            IntPtr hdcDest, //目标设备的句柄
            int nXDest, // 目标对象的左上角的X坐标
            int nYDest, // 目标对象的左上角的X坐标
            int nWidth, // 目标对象的矩形的宽度
            int nHeight, // 目标对象的矩形的长度
            IntPtr hdcSrc, // 源设备的句柄
            int nXSrc, // 源对象的左上角的X坐标
            int nYSrc, // 源对象的左上角的X坐标
            System.Int32 dwRop // 光栅的操作值
            );

        [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
        private static extern IntPtr CreateDC(
            string lpszDriver, // 驱动名称
            string lpszDevice, // 设备名称
            string lpszOutput, // 无用，可以设定位"NULL"
            IntPtr lpInitData // 任意的打印机数据
            );


        public static Bitmap ZoomImage(Bitmap oldMap, int zoomWidth, int zoomHeight)
        {
            return oldMap;
        }

        public static Bitmap CutScreen()
        {
            //Bitmap bit = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            //Graphics g = Graphics.FromImage(bit);
            //g.CopyFromScreen(new Point(0, 0), new Point(0, 0), bit.Size);
            //g.Dispose();
            //return bit;
            IntPtr dc1 = CreateDC("DISPLAY", null,null, (IntPtr)null);
            //创建显示器的DC
            Graphics g1 = Graphics.FromHdc(dc1);
            //由一个指定设备的句柄创建一个新的Graphics对象
            Bitmap MyImage =new Bitmap(Screen.PrimaryScreen.Bounds.Width,Screen.PrimaryScreen.Bounds.Height, g1);
            //根据屏幕大小创建一个与之相同大小的Bitmap对象
            Graphics g2 = Graphics.FromImage(MyImage);
            //获得屏幕的句柄
            IntPtr dc3 = g1.GetHdc();
            //获得位图的句柄
            IntPtr dc2 = g2.GetHdc();
            //把当前屏幕捕获到位图对象中
            BitBlt(dc2, 0, 0, Screen.PrimaryScreen.Bounds.Width,Screen.PrimaryScreen.Bounds.Height,dc3, 0, 0, 13369376);
            //把当前屏幕拷贝到位图中
            g1.ReleaseHdc(dc3);
            //释放屏幕句柄
            g2.ReleaseHdc(dc2);
            Bitmap img = new Bitmap(MyImage, 800, 600);
            return img;
        }

        public static bool BMPToJPEG(string bmpPath, string savePath)
        {
            try
            {
                Image bmp = Image.FromFile(bmpPath);
                bmp.Save(savePath, System.Drawing.Imaging.ImageFormat.Jpeg);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static bool SaveImage(Stream imgStream, ref string filename, bool rewrite = true)
        {
            try
            {
                ImageFormat imgFormat = ImageFormat.Bmp;
                FileInfo imgFile = new FileInfo(filename);
                switch (imgFile.Extension.ToLower())
                {
                    case ".jpg":
                    case ".jpeg":
                        imgFormat = ImageFormat.Jpeg;
                        break;
                    case ".png":
                        imgFormat = ImageFormat.Png;
                        break;
                    case ".gif":
                        imgFormat = ImageFormat.Gif;
                        break;
                    case ".icon":
                        imgFormat = ImageFormat.Icon;
                        break;
                    case ".bmp":
                        imgFormat = ImageFormat.Bmp;
                        break;
                    default:
                        throw new Exception("不支持图像保存为" + imgFile.Extension.ToLower() + "格式。");
                }
                if (!rewrite && imgFile.Exists)
                {
                    filename = string.Concat(imgFile.Directory.FullName, "\\", Guid.NewGuid().ToString(), imgFile.Extension);
                }

                Bitmap bitmap = new Bitmap(imgStream);

                bitmap.Save(filename, imgFormat);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>  
        ///  Resize图片   
        /// </summary>  
        /// <param name="bmp">原始Bitmap </param>  
        /// <param name="newW">新的宽度</param>  
        /// <param name="newH">新的高度</param>  
        /// <param name="Mode">保留着，暂时未用</param>  
        /// <returns>处理以后的图片</returns>  

        public static Bitmap ResizeImage(Bitmap bmp, int newW, int newH, int Mode)
        {
            Bitmap b = new Bitmap(newW, newH);
            Graphics g = Graphics.FromImage(b);
            // 插值算法的质量   
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(bmp, new Rectangle(0, 0, newW, newH), new Rectangle(0, 0, bmp.Width, bmp.Height), GraphicsUnit.Pixel);
            g.Dispose();
            return b;
        }
        /// <summary>  
        /// 剪裁 -- 用GDI+   
        /// </summary>  
        /// <param name="b">原始Bitmap</param>  
        /// <param name="StartX">开始坐标X</param>  
        /// <param name="StartY">开始坐标Y</param>  
        /// <param name="iWidth">宽度</param>  
        /// <param name="iHeight">高度</param>  
        /// <returns>剪裁后的Bitmap</returns>  
        public static Bitmap Cut(Bitmap b, int StartX, int StartY, int iWidth, int iHeight)
        {
            if (b == null)
            {
                return null;
            }
            int w = b.Width;
            int h = b.Height;
            if (StartX >= w || StartY >= h)
            {
                return null;
            }
            if (StartX + iWidth > w)
            {
                iWidth = w - StartX;
            }
            if (StartY + iHeight > h)
            {
                iHeight = h - StartY;
            }
            
            //Bitmap bmpOut = new Bitmap(iWidth, iHeight, PixelFormat.Format24bppRgb);
            Bitmap bmpOut = new Bitmap(iWidth, iHeight, b.PixelFormat);
            Graphics g = Graphics.FromImage(bmpOut);
            g.DrawImage(b, new Rectangle(0, 0, iWidth, iHeight), new Rectangle(StartX, StartY, iWidth, iHeight), GraphicsUnit.Pixel);
            g.Dispose();
            return bmpOut;

        }

        public static Bitmap ResizeImageX(Bitmap bmp, int newW)
        {
            int newH = (int)(newW * 1.0 / bmp.Width * bmp.Height);
            return ResizeImage(bmp, newW, newH, 0);
        }

        public static Bitmap ResizeImageY(Bitmap bmp, int newH)
        {
            int newW = (int)(newH * 1.0 / bmp.Height * bmp.Width);
            return ResizeImage(bmp, newW, newH, 0);
        }

    }
}
