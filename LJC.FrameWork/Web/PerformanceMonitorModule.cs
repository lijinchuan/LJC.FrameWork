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

        public static Action<HttpContext> PreAuthenticateRequest;

        public void Init(HttpApplication context)
        {
            if(LogMonitor==null)
            {
                return;
            }

            context.BeginRequest += Context_BeginRequest;
            context.EndRequest += Context_EndRequest;

            context.AcquireRequestState += context_AcquireRequestState;
            context.AuthenticateRequest += context_AuthenticateRequest;
            context.AuthorizeRequest += context_AuthorizeRequest;
            context.PreRequestHandlerExecute += context_PreRequestHandlerExecute;
            context.PostRequestHandlerExecute += context_PostRequestHandlerExecute;
            context.ReleaseRequestState += context_ReleaseRequestState;
            context.ResolveRequestCache += context_ResolveRequestCache;
            context.PreSendRequestHeaders += context_PreSendRequestHeaders;
            context.PreSendRequestContent += context_PreSendRequestContent;
        }

        void context_PreSendRequestContent(object sender, EventArgs e)
        {
            PageTraceUtil.Trace("PreSendRequestContent");
        }

        void context_PreSendRequestHeaders(object sender, EventArgs e)
        {
            PageTraceUtil.Trace("PreSendRequestHeaders");
        }

        void context_ResolveRequestCache(object sender, EventArgs e)
        {
            PageTraceUtil.Trace("ResolveRequestCache");
        }

        void context_ReleaseRequestState(object sender, EventArgs e)
        {
            PageTraceUtil.Trace("ReleaseRequestState");
        }

        void context_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            PageTraceUtil.Trace("PostRequestHandlerExecute");
        }

        void context_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            PageTraceUtil.Trace("PreRequestHandlerExecute");
        }

        void context_AuthorizeRequest(object sender, EventArgs e)
        {
            PageTraceUtil.Trace("AuthorizeRequest");
        }

        void context_AuthenticateRequest(object sender, EventArgs e)
        {
            if (PreAuthenticateRequest != null)
            {
                HttpContext httpContext = ((HttpApplication)sender).Context;
                if (httpContext != null)
                {
                    PreAuthenticateRequest(httpContext);
                }
            }

            PageTraceUtil.Trace("AuthenticateRequest");
        }

        void context_AcquireRequestState(object sender, EventArgs e)
        {
            PageTraceUtil.Trace("AcquireRequestState");
        }

        private void Context_BeginRequest(object sender, EventArgs e)
        {
            starttime = DateTime.Now;
            HttpContext httpContext = ((HttpApplication)sender).Context;
            httpContext.StartTrace();
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
                TraceDetail= httpContext.PrintTrace()
            };
            
            LogMonitor(monitor);
        }

    }
}
