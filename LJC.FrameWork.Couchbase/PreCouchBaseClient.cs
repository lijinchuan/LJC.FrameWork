using Couchbase;
using Enyim.Caching.Memcached;
using Enyim.Caching.Memcached.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enyim.Caching.Memcached.Results.Extensions;
using Couchbase.Configuration;

namespace LJC.FrameWork.MemCached
{
    public class PreCouchBaseClient :Couchbase.CouchbaseClient
    {
        enum OperationState
        {
            Unspecified,
            Success,
            Failure,
            InvalidVBucket
        }

        interface IOperationWithState
        {
            // Properties
            OperationState State { get; }
        }

        public PreCouchBaseClient(string section)
            : base(section)
        {

        }

        public PreCouchBaseClient(CouchbaseClientConfiguration config)
            : base(config)
        {

        }

        private IOperationResult ExecuteWithRedirect(IMemcachedNode startNode, ISingleItemOperation op)
        {
            IOperationResult result = startNode.Execute(op);
            if (!result.Success)
            {
                IOperationWithState state = op as IOperationWithState;
                if (state == null)
                {
                    return result;
                }
                if (state.State != OperationState.InvalidVBucket)
                {
                    return result;
                }
                foreach (IMemcachedNode node in base.Pool.GetWorkingNodes())
                {
                    result = node.Execute(op);
                    if (result.Success)
                    {
                        return result;
                    }
                    if (state.State != OperationState.InvalidVBucket)
                    {
                        return result;
                    }
                }
            }
            return result;
        }

        protected override IGetOperationResult PerformTryGet(string key, out ulong cas, out object value)
        {
            string str = base.KeyTransformer.Transform(key);
            IMemcachedNode startNode = base.Pool.Locate(str);
            IGetOperationResult source = base.GetOperationResultFactory.Create();
            if (startNode != null)
            {
                IGetOperation op = base.Pool.OperationFactory.Get(str);
                IOperationResult result2 = this.ExecuteWithRedirect(startNode, op);
                if (result2.Success)
                {
                    source.Value = value = op.Result.Data; // base.Transcoder.Deserialize(op.Result);
                    source.Cas = cas = op.CasValue;
                    if (base.PerformanceMonitor != null)
                    {
                        base.PerformanceMonitor.Get(1, true);
                    }
                    source.Pass(null);
                    return source;
                }
                value = null;
                cas = 0L;
                result2.Combine(source);
                return source;
            }
            value = null;
            cas = 0L;
            if (base.PerformanceMonitor != null)
            {
                base.PerformanceMonitor.Get(1, false);
            }
            source.Fail("Unable to locate node", null);
            return source;
        }
    }
}
