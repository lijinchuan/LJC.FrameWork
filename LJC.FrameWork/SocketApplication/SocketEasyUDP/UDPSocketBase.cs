using LJC.FrameWork.SocketApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LJC.FrameWork.SocketEasyUDP
{
    public class UDPSocketBase:IDisposable
    {
        private static long _bagid = 0;
        private static long _segmentid = 0;

        private Dictionary<long, byte[][]> TempBagDic = new Dictionary<long, byte[][]>();
        private Dictionary<long, DateTime> BagTimestamp = new Dictionary<long, DateTime>();

        protected static int TimeOutTryTimes = 10;
        protected static int TimeOutMillSec = 1000;

        private bool _disposed = false;

        public event Action<Exception> Error = null;

        public virtual bool SendMessage(Message msg,EndPoint endpoint)
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

        #region 拆包
        protected const int MAX_PACKAGE_LEN = 548; //65507 1472 548
        protected static double MAX_PACKAGE_LEN2 = MAX_PACKAGE_LEN - 24;
        protected static int MAX_PACKAGE_LEN3 = MAX_PACKAGE_LEN - 24;

        protected IEnumerable<byte[]> SplitBytes(byte[] bigbytes)
        {
            int byteslen=bigbytes.Length;
            byte[] bytesid = null;
            int packagelen = (int)Math.Ceiling(bigbytes.Length / MAX_PACKAGE_LEN2);
            byte[] packagelenbytes = BitConverter.GetBytes(packagelen);

            byte[] segmentid = null;

            if (packagelen > 1)
            {
                var newbagid = System.Threading.Interlocked.Increment(ref _bagid);
                //bytesid = Guid.NewGuid().ToByteArray();
                bytesid = BitConverter.GetBytes(newbagid);
            }

            for (int i = 1; i <= packagelen; i++)
            {
                segmentid =BitConverter.GetBytes(System.Threading.Interlocked.Increment(ref _segmentid));

                int offset = (i - 1) * MAX_PACKAGE_LEN3;
                var len=Math.Min(bigbytes.Length - offset, MAX_PACKAGE_LEN3);
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
            if (baglen == 1)
            {
                return 16;
            }

            return 24;
        }

        protected byte[] MargeBag(byte[] bag)
        {
            var packageno = BitConverter.ToInt32(bag, 8);
            var packagelen = BitConverter.ToInt32(bag, 12);
            
            if (packagelen > 1)
            {
                long bagid = BitConverter.ToInt64(bag,16);

                byte[][] bags=null;
                if (!TempBagDic.TryGetValue(bagid,out bags))
                {
                    lock (TempBagDic)
                    {
                        if (!TempBagDic.TryGetValue(bagid, out bags))
                        {
                            bags = new byte[packagelen][];
                            TempBagDic.Add(bagid, bags);
                            BagTimestamp.Add(bagid, DateTime.Now);
                        }
                    }
                }

                lock (bags)
                {
                    var index=packageno-1;
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
                    TempBagDic.Remove(bagid);
                }

                lock (BagTimestamp)
                {
                    BagTimestamp.Remove(bagid);
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
                return bag.Skip(16).ToArray();
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

        ~UDPSocketBase()
        {
            Dispose(false);
        }
        #endregion
    }
}
