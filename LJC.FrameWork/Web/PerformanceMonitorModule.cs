using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace LJC.FrameWork.Web
{
    public class PerformanceMonitorModule : IHttpModule
    {
        DateTime starttime = DateTime.MinValue;

        public void Dispose()
        {
        }

        public static Action<PerformanceMonitor> LogMonitor = null;

        public void Init(HttpApplication context)
        {
            if(LogMonitor==null)
            {
                return;
            }

            context.BeginRequest += Context_BeginRequest;
            context.EndRequest += Context_EndRequest;
        }

        private void Context_BeginRequest(object sender, EventArgs e)
        {
            starttime = DateTime.Now;
        }

        private void Context_EndRequest(object sender, EventArgs e)
        {
            var ticks = DateTime.Now.Subtract(starttime).TotalMilliseconds;
            //var trace = LJC.FrameWork.Comm.ProcessTraceUtil.PrintTrace();
            HttpContext httpContext = ((HttpApplication)sender).Context;

            PerformanceMonitor monitor = new PerformanceMonitor
            {
                Url= httpContext.Request.Url.ToString(),
                Mills=ticks,
                TraceDetail=string.Empty
            };
            
            LogMonitor(monitor);
        }

    }
}
