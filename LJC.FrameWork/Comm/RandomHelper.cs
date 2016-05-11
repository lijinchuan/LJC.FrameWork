using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm
{
    /// <summary>
    /// 随机数算法
    /// </summary>
    public class RandomHelper
    {
        private static int seek = 0;
        private static int maxSeek = 666666;

        static RandomHelper()
        {
            seek = new Random().Next(seek, maxSeek);
        }

        /// <summary>
        /// 计算可能性
        /// </summary>
        /// <param name="rate">0-1之前的数</param>
        /// <returns></returns>
        public static bool IsProbable(double rate)
        {
            rate = Math.Round(rate, 7);
            if (rate >= 1)
                return true;
            if (rate <= 0)
                return false;

            int len = rate.ToString().Length - rate.ToString().IndexOf(".") - 1;
            int rank = Math.Max((int)Math.Pow(10, len), 100);

            return GetRandomNumer(1, rank) <= rate * rank;
        }

        /// <summary>
        /// 取一个随机数
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GetRandomNumer(int min, int max)
        {
            if (seek >= maxSeek)
            {
                seek = 0;
            }

            return new Random((int)(Math.Pow(seek++, 1.6)) % int.MaxValue).Next(min, max);
        }
    }
}
