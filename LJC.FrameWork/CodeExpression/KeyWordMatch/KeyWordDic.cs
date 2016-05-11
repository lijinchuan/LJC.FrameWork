using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LJC.FrameWork.CodeExpression.KeyWordMatch
{
    [StructLayout(LayoutKind.Auto)]
    public class KeyWordDic
    {
        private const UInt16 CapLargerCount = 2;

        public static int CallAddCount = 0;


        private bool _hasEndChar = false;
        public bool HasEndChar
        {
            get
            {
                return _hasEndChar;
            }
            set
            {
                _hasEndChar = value;
            }
        }

        public char Key
        {
            get;
            set;
        }

        public UInt16 Count = 0;

        internal KeyWordDic[] kvArray = null;

        //public KeyWordDic()
        //{

        //}

        public KeyWordDic(char key)
        {
            this.Key = key;
        }


        public KeyWordDic this[char key]
        {
            get
            {
                int md = 0;
                int re = this.Find(key, ref md);
                if (re > -1)
                {
                    return kvArray[re];
                }
                return default(KeyWordDic);
            }
        }

        public List<char> Keys()
        {
            return kvArray.Select(p => (char)p.Key).ToList();
        }

        public bool ContainsKey(char key)
        {
            int mid = 0;
            return Find(key, ref mid) > -1;
        }

        public void Remove(char key)
        {
            int mid = 0;
            int fd = Find(key, ref mid);
            if (fd > -1)
            {
                for (int i = fd; i < Count - 1; i++)
                {
                    kvArray[i] = kvArray[i + 1];
                }
                kvArray[Count - 1] = default(KeyWordDic);
                Count--;
            }
        }

        public bool TryGetValue(char key, out KeyWordDic val)
        {
            int fd = 0;
            int re = Find(key, ref fd);
            if (re > -1)
            {
                val = kvArray[re];
                return true;
            }
            else
            {
                val = default(KeyWordDic);
                return false;
            }
        }

        public KeyWordDic Add(char key)
        {
            CallAddCount++;

            KeyWordDic newKv = null;

            if (kvArray == null)
            {
                kvArray = new KeyWordDic[CapLargerCount];
            }

            if (Count == 0)
            {
                newKv = new KeyWordDic(key);
                kvArray[0] = newKv;
            }
            else
            {
                int md = 0;
                if (Find(key, ref md) != -1)
                {
                    throw new Exception("主键已存在:" + key + "!");
                }
                else
                {
                    if (kvArray[md].Key < key)
                    {
                        md = (UInt16)(md + 1);
                    }

                    for (int i = Count - 1; i >= md; i--)
                    {
                        kvArray[i + 1] = kvArray[i];
                    }

                    newKv = new KeyWordDic(key);
                    kvArray[md] = newKv;
                }
            }

            Count++;

            if (Count == kvArray.Length)
            {
                int largerSize = Count * 2;
                if (largerSize > char.MaxValue + 1)
                {
                    largerSize = char.MaxValue + 1;
                }

                var newKvArray = new KeyWordDic[largerSize];
                for (int i = 0; i < Count; i++)
                {
                    newKvArray[i] = kvArray[i];
                }
                kvArray = newKvArray;
            }

            return newKv;
        }

        public int Find(char key, ref int mid)
        {
            mid = 0;

            if (kvArray == null)
                return -1;

            if (Count == 0)
                return -1;

            if (Count == 1)
            {
                return kvArray[0].Key == key ? 0 : -1;
            }

            int lt = 0, rt = Count - 1;

            if (kvArray[lt].Key == key)
            {
                return lt;
            }

            if (kvArray[rt].Key == key)
            {
                return rt;
            }

            if (kvArray[lt].Key > key)
            {
                mid = lt;
                return -1;
            }

            if (kvArray[rt].Key < key)
            {
                mid = rt;
                return -1;
            }

            int md = (lt + (rt - lt) / 2);
            while (lt < md && md < rt)
            {
                if (kvArray[md].Key == key)
                {
                    return md;
                }
                else if (kvArray[md].Key > key)
                {
                    rt = md;
                    md = (lt + (rt - lt) / 2);
                }
                else
                {
                    lt = md;
                    md = (lt + (rt - lt) / 2);
                }
            }

            mid = md;
            return -1;

        }
    }
}
