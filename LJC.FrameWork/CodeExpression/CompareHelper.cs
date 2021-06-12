using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    static class CompareHelper
    {
        public static bool Eq(this object obj1,object obj2)
        {
            if (obj1 == null && obj2 == null)
            {
                return true;
            }

            if (obj1 == null || obj2 == null)
            {
                return false;
            }

            if(obj1 is double
                ||obj1 is int)
            {
                return Convert.ToDouble(obj1) == Convert.ToDouble(obj2);
            }

            if(obj1 is DateTime)
            {
                return (DateTime)obj1 == Convert.ToDateTime(obj2);
            }

            if(obj1 is string)
            {
                return (string)obj1 == Convert.ToString(obj2);
            }

            return false;
        }

        public static bool BigerEq(this object obj1,object obj2)
        {
            if (obj1 == null && obj2 == null)
            {
                return true;
            }

            if (obj1 == null)
            {
                return false;
            }

            if (obj2 == null)
            {
                return true;
            }

            if (obj1 is double
                || obj1 is int)
            {
                return Convert.ToDouble(obj1) >= Convert.ToDouble(obj2);
            }

            if (obj1 is DateTime)
            {
                return (DateTime)obj1 >= Convert.ToDateTime(obj2);
            }

            if (obj1 is string)
            {
                return Convert.ToString(obj1).CompareTo(Convert.ToString(obj2))>=0;
            }

            return false;
        }

        public static bool Biger(this object obj1, object obj2)
        {
            if (obj1 == null && obj2 == null)
            {
                return false;
            }

            if (obj1 == null)
            {
                return false;
            }

            if (obj2 == null)
            {
                return true;
            }

            if (obj1 is double
                || obj1 is int)
            {
                return Convert.ToDouble(obj1) > Convert.ToDouble(obj2);
            }

            if (obj1 is DateTime)
            {
                return (DateTime)obj1 > Convert.ToDateTime(obj2);
            }

            if (obj1 is string)
            {
                return Convert.ToString(obj1).CompareTo(Convert.ToString(obj2)) > 0;
            }

            return false;
        }

        public static bool SmallerEq(this object obj1, object obj2)
        {
            if (obj1 == null && obj2 == null)
            {
                return true;
            }

            if (obj1 == null)
            {
                return true;
            }

            if (obj2 == null)
            {
                return false;
            }

            if (obj1 is double
                || obj1 is int)
            {
                return Convert.ToDouble(obj1) <= Convert.ToDouble(obj2);
            }

            if (obj1 is DateTime)
            {
                return (DateTime)obj1 <= Convert.ToDateTime(obj2);
            }

            if (obj1 is string)
            {
                return Convert.ToString(obj1).CompareTo(Convert.ToString(obj2)) <= 0;
            }

            return false;
        }

        public static bool Smaller(this object obj1, object obj2)
        {
            if (obj1 == null && obj2 == null)
            {
                return false;
            }

            if (obj1 == null)
            {
                return true;
            }

            if (obj2 == null)
            {
                return false;
            }

            if (obj1 is double
                || obj1 is int)
            {
                return Convert.ToDouble(obj1) < Convert.ToDouble(obj2);
            }

            if (obj1 is DateTime)
            {
                return (DateTime)obj1 < Convert.ToDateTime(obj2);
            }

            if (obj1 is string)
            {
                return Convert.ToString(obj1).CompareTo(Convert.ToString(obj2)) < 0;
            }

            return false;
        }
    }
}
