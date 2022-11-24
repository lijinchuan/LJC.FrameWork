using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    public struct DelayCalResult : IComparable, IFormattable, IConvertible
    {

        public static readonly DelayCalResult Def = 0d;

        internal double m_value;

        // Compares this object to another object, returning an integer that
        // indicates the relationship. 
        // Returns a value less than zero if this  object
        // null is considered to be less than any instance.
        // If object is not of type Int32, this method throws an ArgumentException.
        // 
        public int CompareTo(Object value)
        {
            if (value == null)
            {
                return 1;
            }
            if (value is DelayCalResult)
            {
                // Need to use compare because subtraction will wrap
                // to positive for very large neg numbers, etc.
                var i = (double)value;
                if (m_value < i) return -1;
                if (m_value > i) return 1;
                return 0;
            }
            throw new ArgumentException("Arg_MustBedouble");
        }

        public int CompareTo(DelayCalResult value)
        {
            // Need to use compare because subtraction will wrap
            // to positive for very large neg numbers, etc.
            if (m_value < value) return -1;
            if (m_value > value) return 1;
            return 0;
        }

        public override bool Equals(Object obj)
        {
            if (!(obj is DelayCalResult))
            {
                return false;
            }
            return m_value == ((DelayCalResult)obj).m_value;
        }

        public bool Equals(double obj)
        {
            return m_value == obj;
        }

        // The absolute value of the int contained.
        public override int GetHashCode()
        {
            return (int)m_value;
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [Pure]
        public override String ToString()
        {
            return m_value.ToString();
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [Pure]
        public String ToString(String format)
        {
            return m_value.ToString(format);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [Pure]
        public String ToString(IFormatProvider provider)
        {
            return m_value.ToString(provider);
        }

        [Pure]
        [System.Security.SecuritySafeCritical]  // auto-generated
        public String ToString(String format, IFormatProvider provider)
        {
            return m_value.ToString(format, provider);
        }

        //
        // IConvertible implementation
        // 

        [Pure]
        public TypeCode GetTypeCode()
        {
            return TypeCode.Int32;
        }

        /// <internalonly/>
        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(m_value);
        }

        /// <internalonly/>
        char IConvertible.ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(m_value);
        }

        /// <internalonly/>
        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(m_value);
        }

        /// <internalonly/>
        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(m_value);
        }

        /// <internalonly/>
        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(m_value);
        }

        /// <internalonly/>
        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(m_value);
        }

        /// <internalonly/>
        int IConvertible.ToInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        /// <internalonly/>
        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(m_value);
        }

        /// <internalonly/>
        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(m_value);
        }

        /// <internalonly/>
        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(m_value);
        }

        /// <internalonly/>
        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(m_value);
        }

        /// <internalonly/>
        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(m_value);
        }

        /// <internalonly/>
        Decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(m_value);
        }

        /// <internalonly/>
        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            throw new InvalidCastException("InvalidCast_FromTo DateTime");
        }

        /// <internalonly/>
        Object IConvertible.ToType(Type type, IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public static double operator +(DelayCalResult val1, DelayCalResult val2)
        {
            return val1.m_value + val2.m_value;
        }

        public static bool operator ==(DelayCalResult val1, DelayCalResult val2)
        {
            return val1.m_value == val2.m_value;
        }

        public static bool operator ==(DelayCalResult val1, object val2)
        {
            if (val2 is DelayCalResult)
            {
                return val1.m_value == ((DelayCalResult)val2).m_value;
            }
            return false;
        }

        public static bool operator !=(DelayCalResult val1, object val2)
        {
            if (val2 is DelayCalResult)
            {
                return val1.m_value != ((DelayCalResult)val2).m_value;
            }
            return true;
        }

        public static bool operator ==(object val1, DelayCalResult val2)
        {
            if (val1 is DelayCalResult)
            {
                return val2.m_value == ((DelayCalResult)val1).m_value;
            }
            return false;
        }

        public static bool operator !=(object val1, DelayCalResult val2)
        {
            if (val1 is DelayCalResult)
            {
                return val2.m_value != ((DelayCalResult)val1).m_value;
            }
            return true;
        }

        public static bool operator !=(DelayCalResult val1, DelayCalResult val2)
        {
            return val1.m_value != val2.m_value;
        }

        public static bool operator >=(DelayCalResult val1, DelayCalResult val2)
        {
            return val1.m_value >= val2.m_value;
        }

        public static bool operator <=(DelayCalResult val1, DelayCalResult val2)
        {
            return val1.m_value <= val2.m_value;
        }

        public static bool operator >(DelayCalResult val1, DelayCalResult val2)
        {
            return val1.m_value > val2.m_value;
        }

        public static bool operator <(DelayCalResult val1, DelayCalResult val2)
        {
            return val1.m_value < val2.m_value;
        }

        public static implicit operator DelayCalResult(double val)
        {
            // processing
            return new DelayCalResult() { m_value = val };
        }

        public static implicit operator double(DelayCalResult val)
        {
            return val.m_value;
        }

    }
}
