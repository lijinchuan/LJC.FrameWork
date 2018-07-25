using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.EntityBuf
{
    public interface IEntityBufObject
    {
        byte[] Serialize();
        IEntityBufObject DeSerialize(byte[] bytes);
    }
}
