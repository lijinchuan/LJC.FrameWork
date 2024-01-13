using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LJC.FrameWork.SocketApplication
{
    public class SendMessageResult
    {
        public int SendCount
        {
            get;
            set;
        }

        public DateTime Start
        {
            get;
            set;
        } = DateTime.Now;

        public DateTime StartSend
        {
            get;
            set;
        } = DateTime.Now;

        public DateTime EndSend
        {
            get;
            set;
        } = DateTime.Now;
    }
}
