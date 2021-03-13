using LJC.FrameWork.Net.HTTP.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestHttpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //var bts = BitConverter.GetBytes('a');
            //var bts2 = BitConverter.GetBytes(false);
            //BitArray ba = new BitArray(new bool[] { true, true, true, false, true, true, true, true, true,true });
            //byte[] bt=new byte[(int)Math.Ceiling(ba.Length/8.0)];
            //ba.CopyTo(bt, 0);
            //decimal d = 1.21M;
            //Console.WriteLine(d % 1);


            //var total = new DateTime(1899, 12, 31).Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            //var defaultchar = default(char);

            //Console.WriteLine("MaxValue:" + Byte.MaxValue);
            //Console.WriteLine("uint16:" + UInt16.MaxValue);
            //Console.WriteLine("uint32:" + UInt32.MaxValue);
            //Console.WriteLine("uint64:" + UInt64.MaxValue);

            HttpServer http = new HttpServer(new Server(8081));
            http.Handlers.Add(new SubstitutingFileReader());
            http.Handlers.Add(new RESTfulApiHandlerBase(HMethod.GET, "/api/patientinfo", new List<string>() { "Function", "UserJID" }, new PatientGetHander()));
            http.Handlers.Add(new RESTfulApiHandlerBase(HMethod.POST, "/api/patientinfo", new List<string>() { "Function", "UserJID" }, new PatientPostHander()));
            http.Handlers.Add(new RESTfulApiHandlerBase(HMethod.DELETE, "/api/patientinfo", new List<string>() { "Function", "UserJID" }, new PatientDeleteHander()));
            http.Handlers.Add(new RESTfulApiHandlerBase(HMethod.PUT, "/api/patientinfo", new List<string>() { "Function", "UserJID" }, new PatientPutHander()));
            Console.WriteLine("服务已启动，点击任何按键退出");
            Console.ReadKey();
        }
    }

    public class PatientGetHander : IRESTfulHandler
    {

        public bool Process(HttpServer server, HttpRequest request, HttpResponse response, Dictionary<string, string> param)
        {
            Console.WriteLine("PatientGetHander 完成！");
            response.Content = "PatientGetHander 完成！abc";
            return true;
        }
    }

    public class PatientDeleteHander : IRESTfulHandler
    {

        public bool Process(HttpServer server, HttpRequest request, HttpResponse response, Dictionary<string, string> param)
        {
            Console.WriteLine("PatientDeleteHander 完成！");
            response.Content = "PatientDeleteHander 完成！";
            return true;
        }
    }

    public class PatientPostHander : IRESTfulHandler
    {

        public bool Process(HttpServer server, HttpRequest request, HttpResponse response, Dictionary<string, string> param)
        {
            Console.WriteLine("PatientPostHander 完成！");
            response.Content = "PatientPostHander 完成！";
            return true;
        }
    }

    public class PatientPutHander : IRESTfulHandler
    {

        public bool Process(HttpServer server, HttpRequest request, HttpResponse response, Dictionary<string, string> param)
        {
            Console.WriteLine("PatientPutHander 完成！");
            response.Content = "PatientPutHander 完成！";
            return true;
        }
    }
}
