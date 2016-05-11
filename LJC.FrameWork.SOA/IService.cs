using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LJC.FrameWork.SocketApplication;

namespace LJC.FrameWork.SOA
{
    internal interface IService
    {
        bool RegisterService();

        object DoResponse(int funcId, byte[] request);

    }
}
