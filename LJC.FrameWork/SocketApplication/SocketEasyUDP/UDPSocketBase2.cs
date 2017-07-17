using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace LJC.FrameWork.SocketApplication.SocketEasyUDP
{
    public class UDPSocketBase2 : IDisposable
    {
        private static long _bagid = 0;
        private static long _segmentid = 0;

        protected const ushort MTU_MAX = 65507; //65507 1472 548
        protected const ushort MTU_MIN = 548;

        private Dictionary<string, byte[][]> TempBagDic = new Dictionary<string, byte[][]>();
        private Dictionary<string, DateTime> BagRemovedDic = new Dictionary<string, DateTime>();

        protected static int TimeOutTryTimes = 10;
        protected static int TimeOutMillSec = 1000;

        private bool _disposed = false;

        public event Action<Exception> Error = null;

        public virtual bool SendMessage(Message msg, IPEndPoint endpoint)
        {
            throw new NotImplementedException();
        }

        protected virtual void OnError(Exception e)
        {
            if (Error != null)
            {
                Error(e);
            }
        }

        protected string GetBagKey(long bagid, IPEndPoint endpoint)
        {
            string key = string.Empty;
            if (endpoint == null)
            {
                key = bagid.ToString();
            }
            else
            {
                key = string.Format("{0}:{1}:{2}", endpoint.Address.ToString(), endpoint.Port, bagid);
            }

            return key;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bagid"></param>
        /// <param name="endpoint"></param>
        /// <returns>完全未收到null,已被移除[],其它未收到</returns>
        protected int[] GetMissSegment(long bagid,IPEndPoint endpoint,out bool reved)
        {
            byte[][] bagarray = null;
            List<int> list = new List<int>();
            reved = true;
            var bagkey = GetBagKey(bagid, endpoint);

            if (TempBagDic.TryGetValue(bagkey, out bagarray))
            {
                for (int i = 0; i < bagarray.Length; i++)
                {
                    if (bagarray[i] == null)
                    {
                        list.Add(i);
                        reved = false;
                    }
                }

                return list.ToArray();
            }
            else
            {
                if (!BagRemovedDic.ContainsKey(bagkey))
                {
                    reved = false;
                }
                return null;
            }
        }

        #region 拆包

        protected long GetBagId(byte[] bytes)
        {
            var bagid = BitConverter.ToInt64(bytes, 16);
            return bagid;
        }

        protected IEnumerable<byte[]> SplitBytes(byte[] bigbytes,ushort maxPackageLen)
        {
            int byteslen = bigbytes.Length;
            byte[] bytesid = null;
            int packagelen = (int)Math.Ceiling(bigbytes.Length / (double)(maxPackageLen-24));
            byte[] packagelenbytes = BitConverter.GetBytes(packagelen);

            byte[] segmentid = null;

            var newbagid = System.Threading.Interlocked.Increment(ref _bagid);
            //bytesid = Guid.NewGuid().ToByteArray();
            bytesid = BitConverter.GetBytes(newbagid);

            for (int i = 1; i <= packagelen; i++)
            {
                segmentid = BitConverter.GetBytes(System.Threading.Interlocked.Increment(ref _segmentid));

                int offset = (i - 1) * (maxPackageLen - 24);
                var len = Math.Min(bigbytes.Length - offset, maxPackageLen - 24);
                var sendbytes = new byte[len + 24];
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(sendbytes))
                {
                    ms.Write(segmentid, 0, segmentid.Length);

                    ms.Write(BitConverter.GetBytes(i), 0, 4);
                    ms.Write(packagelenbytes, 0, 4);

                    if (bytesid != null)
                    {
                        ms.Write(bytesid, 0, bytesid.Length);
                    }

                    ms.Write(bigbytes, offset, len);

                    yield return ms.ToArray();
                }
            }
        }

        private int GetBagOffset(int bagno, int baglen)
        {
            return 24;
        }

        protected byte[] MargeBag(byte[] bag,IPEndPoint endpoint)
        {
            var packageno = BitConverter.ToInt32(bag, 8);
            var packagelen = BitConverter.ToInt32(bag, 12);

            if (packagelen > 1)
            {
                long bagid = BitConverter.ToInt64(bag, 16);
                string key = GetBagKey(bagid, endpoint);
                byte[][] bags = null;
                if (!TempBagDic.TryGetValue(GetBagKey(bagid,endpoint), out bags))
                {
                    lock (TempBagDic)
                    {
                        if (!TempBagDic.TryGetValue(key, out bags))
                        {
                            bags = new byte[packagelen][];
                            TempBagDic.Add(key, bags);

                        }
                    }
                }

                lock (bags)
                {
                    var index = packageno - 1;
                    if (bags[index] == null)
                    {
                        bags[index] = bag;
                    }
                }

                for (var i = 0; i < bags.Length; i++)
                {
                    if (bags[i] == null)
                    {
                        return null;
                    }
                }

                lock (TempBagDic)
                {
                    TempBagDic.Remove(key);
                }

                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    for (int i = 0; i < bags.Length; i++)
                    {
                        var offset = GetBagOffset(packageno, packagelen);
                        ms.Write(bags[i], offset, bags[i].Length - offset);
                    }

                    return ms.ToArray();
                }
            }
            else
            {
                return bag.Skip(24).ToArray();
            }

        }

        protected void ClearTempBag(long bagid, IPEndPoint endpoint)
        {
            var key = GetBagKey(bagid, endpoint);
            lock (TempBagDic)
            {
                TempBagDic.Remove(key);
            }

            lock (BagRemovedDic)
            {
                BagRemovedDic.Add(key, DateTime.Now);
            }
        }
        #endregion

        #region 资源清理
        protected virtual void DisposeManagedResource()
        {

        }

        protected virtual void DisposeUnManagedResource()
        {

        }

        private void Dispose(bool flag)
        {
            if (_disposed)
            {
                return;
            }

            if (flag)
            {
                //清理托管资源
                DisposeManagedResource();
            }

            //清理非托管资源
            DisposeUnManagedResource();

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~UDPSocketBase2()
        {
            Dispose(false);
        }
        #endregion
    }
}
