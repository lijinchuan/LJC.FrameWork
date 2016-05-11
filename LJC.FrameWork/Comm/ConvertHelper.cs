using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.Comm
{
    public static class ConvertHelper
    {
        public static decimal ConvertToDecimal(string str)
        {
            if (string.IsNullOrEmpty(str))
                return default(decimal);

            var result=default(decimal);
            decimal.TryParse(str, out result);

            return result;
        }

        public static float ConvertToFloat(string str)
        {
            if (string.IsNullOrEmpty(str))
                return default(float);

            var result = default(float);
            float.TryParse(str, out result);

            return result;
        }

        public static DateTime ConvertToDateTime(string str)
        {
            if (string.IsNullOrEmpty(str))
                return default(DateTime);

            var result = default(DateTime);
            DateTime.TryParse(str, out result);

            return result;
        }

        public static int ConvertToInt(string str)
        {
            if (string.IsNullOrEmpty(str))
                return default(int);

            var result = default(int);
            int.TryParse(str, out result);

            return result;
        }

        public static long ConvertToLong(string str)
        {
            if (string.IsNullOrEmpty(str))
                return default(long);

            var result = default(long);
            long.TryParse(str, out result);

            return result;
        }

        public static double ConvertToDouble(string str)
        {
            if (string.IsNullOrEmpty(str))
                return default(double);

            var result = default(double);
            double.TryParse(str, out result);

            return result;
        }
    }
}
