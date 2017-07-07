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
        private Dictionary<Guid, byte[][]> TempBagDic = new Dictionary<Guid, byte[][]>();
        private Dictionary<Guid, DateTime> BagTimestamp = new Dictionary<Guid, DateTime>();

        private bool _disposed = false;

        public virtual bool SendMessage(Message msg,EndPoint endpoint)
        {
            throw new NotImplementedException();
        }

        #region 拆包
        protected const int MAX_PACKAGE_LEN = 65507;
        const double MAX_PACKAGE_LEN2 = 65483;
        protected const int MAX_PACKAGE_LEN3 = 65483;

        protected IEnumerable<byte[]> SplitBytes(byte[] bigbytes)
        {
            int byteslen=bigbytes.Length;
            byte[] bytesid = null;
            int packagelen = (int)Math.Ceiling(bigbytes.Length / MAX_PACKAGE_LEN2);
            byte[] packagelenbytes = BitConverter.GetBytes(packagelen);

            if (packagelen > 1)
            {
                bytesid = Guid.NewGuid().ToByteArray();
            }

            var sendbytes=new byte[MAX_PACKAGE_LEN];
            for (int i = 1; i <= packagelen; i++)
            {
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(sendbytes))
                {
                    ms.Write(BitConverter.GetBytes(i), 0, 4);
                    ms.Write(packagelenbytes, 0, 4);

                    if (bytesid != null)
                    {
                        ms.Write(bytesid, 0, bytesid.Length);
                    }

                    int offset = (i - 1) * MAX_PACKAGE_LEN3;
                    ms.Write(bigbytes, offset, Math.Min(bigbytes.Length - offset, MAX_PACKAGE_LEN3));

                    yield return ms.ToArray();
                }
            }
        }

        private int GetBagOffset(int bagno, int baglen)
        {
            if (baglen == 1)
            {
                return 8;
            }

            return 24;
        }

        protected byte[] MargeBag(byte[] bag)
        {
            var packageno = BitConverter.ToInt32(bag, 0);
            var packagelen = BitConverter.ToInt32(bag, 4);
            
            if (packagelen > 1)
            {
                var guid = new Guid(bag.Skip(8).Take(16).ToArray());

                byte[][] bags=null;
                if (!TempBagDic.TryGetValue(guid,out bags))
                {
                    lock (TempBagDic)
                    {
                        if (!TempBagDic.TryGetValue(guid, out bags))
                        {
                            bags = new byte[packagelen][];
                            TempBagDic.Add(guid, bags);
                            BagTimestamp.Add(guid, DateTime.Now);
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
                    TempBagDic.Remove(guid);
                }

                lock (BagTimestamp)
                {
                    BagTimestamp.Remove(guid);
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
                return bag.Skip(8).ToArray();
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

        public ~UDPSocketBase()
        {
            Dispose(false);
        }
        #endregion
    }
}
