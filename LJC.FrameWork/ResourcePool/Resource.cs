using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.ResourcePool
{
    public class Resource<T> : IDisposable where T : class
    {
        private string __id;
        // Methods
        public Resource()
        {
            this.__id = Guid.NewGuid().ToString();
        }

        public void Dispose()
        {
            Status status = new Status();
            status.IsUsing = false;
            this.Status = status;
        }

        public void ExHandler()
        {
            Status status = new Status();
            status.IsUsing = false;
            status.IsActive = false;
            status.HasException = true;
            this.Status = status;
        }

        // Properties
        public T Current
        {
            get;
            set;
        }

        public string ID
        {
            get
            {
                return __id;
            }
        }

        public Status Status
        {
            get;
            set;
        }
    }
}
