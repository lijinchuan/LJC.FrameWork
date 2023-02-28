using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;

namespace LJC.FrameWork.Net.HTTP.Server
{
    public class HttpServer
    {
        Server s;
        Hashtable hostmap = Hashtable.Synchronized(new Hashtable());    // Map<string, string>: Host => Home folder
        ArrayList handlers = new ArrayList();        // List<IHttpHandler>
        Hashtable sessions = Hashtable.Synchronized(new Hashtable());        // Map<string,Session>

        int sessionTimeout = 600;

        public Hashtable Hostmap { get { return hostmap; } }
        public Server Server { get { return s; } }
        public ArrayList Handlers { get { return handlers; } }
        public int SessionTimeout
        {
            get { return sessionTimeout; }
            set { sessionTimeout = value; CleanUpSessions(); }
        }

        public HttpServer(Server s)
        {
            this.s = s;
            s.Connect += new ClientEvent(ClientConnect);
            handlers.Add(new FallbackHandler());
        }

        bool ClientConnect(Server s, ClientInfo ci)
        {
            ci.Delimiter = "\r\n\r\n";
            ci.Data = new ClientData(ci);
            ci.OnRead += new ConnectionRead(ClientRead);
            ci.OnReadBytes += new ConnectionReadBytes(ClientReadBytes);
            return true;
        }

        void ClientRead(ClientInfo ci, string text)
        {
            // Read header, if in right state
            ClientData data = (ClientData)ci.Data;
            if (data.state != ClientState.Header) return; // already done; must be some text in content, which will be handled elsewhere
            text = text.Substring(data.headerskip);
            Console.WriteLine("Read header: " + text + " (skipping first " + data.headerskip + ")");
            data.headerskip = 0;
            string[] lines = text.Replace("\r\n", "\n").Split('\n');
            data.req.HeaderText = text;
            // First line: METHOD /path/url HTTP/version
            string[] firstline = lines[0].Split(' ');
            if (firstline.Length != 3) { SendResponse(ci, data.req, new HttpResponse(400, "Incorrect first header line " + lines[0]), false); return; }
            if (firstline[2].Substring(0, 4) != "HTTP") { SendResponse(ci, data.req, new HttpResponse(400, "Unknown protocol " + firstline[2]), false); return; }
            data.req.Method = firstline[0];
            data.req.Url = firstline[1];
            data.req.HttpVersion = firstline[2].Substring(5);
            int p;
            for (int i = 1; i < lines.Length; i++)
            {
                p = lines[i].IndexOf(':');
                if (p > 0) data.req.Header[lines[i].Substring(0, p)] = lines[i].Substring(p + 2);
                else Console.WriteLine("Warning, incorrect header line " + lines[i]);
            }
            // If ? in URL, split out query information
            p = firstline[1].IndexOf('?');
            if (p > 0)
            {
                data.req.Page = data.req.Url.Substring(0, p);
                data.req.QueryString = data.req.Url.Substring(p + 1);
            }
            else
            {
                data.req.Page = data.req.Url;
                data.req.QueryString = "";
            }

            if (data.req.Page.IndexOf("..") >= 0) { SendResponse(ci, data.req, new HttpResponse(400, "Invalid path"), false); return; }

            if (!data.req.Header.TryGetValue("Host", out data.req.Host)) { SendResponse(ci, data.req, new HttpResponse(400, "No Host specified"), false); return; }

            string cookieHeader;
            if (data.req.Header.TryGetValue("Cookie", out cookieHeader))
            {
                string[] cookies = cookieHeader.Split(';');
                foreach (string cookie in cookies)
                {
                    p = cookie.IndexOf('=');
                    if (p > 0)
                    {
                        data.req.Cookies[cookie.Substring(0, p).Trim()] = cookie.Substring(p + 1);
                    }
                    else
                    {
                        data.req.Cookies[cookie.Trim()] = "";
                    }
                }
            }

            string contentLengthString;
            if (data.req.Header.TryGetValue("Content-Length", out contentLengthString))
                data.req.ContentLength = Int32.Parse(contentLengthString);
            else data.req.ContentLength = 0;

            //if(data.req.ContentLength > 0){
            data.state = ClientState.PreContent;
            data.skip = text.Length + 4;
            //} else DoProcess(ci);

            //ClientReadBytes(ci, new byte[0], 0); // For content length 0 body
        }

        public string GetFilename(HttpRequest req)
        {
            string folder = (string)hostmap[req.Host];
            if (folder == null) folder = "webhome";
            if (req.Page == "/") return folder + "/index.html";
            else return folder + req.Page;
        }

        void DoProcess(ClientInfo ci)
        {
            ClientData data = (ClientData)ci.Data;
            string sessid;
            if (data.req.Cookies.TryGetValue("_sessid", out sessid))
                data.req.Session = (Session)sessions[sessid];
            bool closed = Process(ci, data.req);
            data.state = closed ? ClientState.Closed : ClientState.Header;
            data.read = 0;
            HttpRequest oldreq = data.req;
            data.req = new HttpRequest(); // Once processed, the connection will be used for a new request
            data.req.Session = oldreq.Session; // ... but session is persisted
            data.req.From = ((IPEndPoint)ci.Socket.RemoteEndPoint).Address;
        }

        void ClientReadBytes(ClientInfo ci, byte[] bytes, int len)
        {
            CleanUpSessions();
            int ofs = 0;
            ClientData data = (ClientData)ci.Data;
            Console.WriteLine("Reading " + len + " bytes of content, in state " + data.state + ", skipping " + data.skip + ", read " + data.read);
            switch (data.state)
            {
                case ClientState.Content: break;
                case ClientState.PreContent:
                    data.state = ClientState.Content;
                    if ((data.skip - data.read) > len) { data.skip -= len; return; }
                    ofs = data.skip - data.read; data.skip = 0;
                    break;
                //case ClientState.Header: data.read += len - data.headerskip; return;
                default: data.read += len; return;
            }
            try
            {
                if (data.req.RawStream == null)
                {
                    data.req.RawStream = new MemoryStream();
                }
                data.req.RawStream.Write(bytes, ofs, len - ofs);
                //data.req.Content += Encoding.UTF8.GetString(bytes, ofs, len - ofs);
                data.req.BytesRead += len - ofs;
                data.headerskip += len - ofs;
#if DEBUG
                Console.WriteLine("Reading " + (len - ofs) + " bytes of content. Got " + data.req.BytesRead + " of " + data.req.ContentLength);
#endif
                if (data.req.BytesRead >= data.req.ContentLength)
                {
                    //销毁
                    _ = data.req.RawData;

                    if (data.req.Method == "POST")
                    {
                        if (data.req.QueryString == "") data.req.QueryString = data.req.GetContent();
                        else data.req.QueryString += "&" + data.req.GetContent();
                    }
                    ParseQuery(data.req);
                    DoProcess(ci);
                }
            }
            catch (Exception ex)
            {
                HttpResponse resp = new HttpResponse();
                new ServerErrorHandler(ex).Process(this, data.req, resp);
                SendResponse(ci, data.req, resp, true);
            }
        }

        void ParseQuery(HttpRequest req)
        {
            if (req.QueryString == "") return;
            string[] sections = req.QueryString.Split('&');
            for (int i = 0; i < sections.Length; i++)
            {
                int p = sections[i].IndexOf('=');
                if (p < 0) req.Query[sections[i]] = "";
                else req.Query[sections[i].Substring(0, p)] = URLDecode(sections[i].Substring(p + 1));
            }
        }

        public static string URLDecode(string input)
        {
            return System.Web.HttpUtility.UrlDecode(input);

            //StringBuilder output = new StringBuilder();
            //int p;
            //while ((p = input.IndexOf('%')) >= 0)
            //{
            //    output.Append(input.Substring(0, p));
            //    string hexNumber = input.Substring(p + 1, 2);
            //    input = input.Substring(p + 3);
            //    output.Append((char)int.Parse(hexNumber, System.Globalization.NumberStyles.HexNumber));
            //}
            //return output.Append(input).ToString();
        }

        protected virtual bool Process(ClientInfo ci, HttpRequest req)
        {
            HttpResponse resp = new HttpResponse();
            resp.Url = req.Url;
            resp.ContentType = "Content-Type: text/html; charset=utf-8";
            //注意，此处从最后添加的Handler开始遍历，如果找到合适的则退出循环
            for (int i = handlers.Count - 1; i >= 0; i--)
            {
                IHttpHandler handler = (IHttpHandler)handlers[i];
                //如果handler.Process返回成功，则直接退出循环
                if (handler.Process(this, req, resp))
                {
                    //SendResponse(ci, req, resp, resp.ReturnCode != 200);
                    SendResponse(ci, req, resp, false);
                    return resp.ReturnCode != 200;
                }
            }
            return true;
        }

        internal enum ClientState { Closed, Header, PreContent, Content };
        internal class ClientData
        {
            internal HttpRequest req = new HttpRequest();
            internal ClientState state = ClientState.Header;
            internal int skip, read, headerskip;

            internal ClientData(ClientInfo ci)
            {
                req.From = ((IPEndPoint)ci.Socket.RemoteEndPoint).Address;
            }

            public void Clear()
            {
                req.Clear();
                state = ClientState.Header;
                skip = read = headerskip = default;
            }
        }

        public Session RequestSession(HttpRequest req)
        {
            if (req.Session != null)
            {
                if (sessions[req.Session.ID] == req.Session) return req.Session;
            }
            req.Session = new Session(req.From);
            sessions[req.Session.ID] = req.Session;
            return req.Session;
        }

        void CleanUpSessions()
        {
            ArrayList toRemove = new ArrayList();
            lock (sessions.SyncRoot)
            {
                ICollection keys = sessions.Keys;
                foreach (string k in keys)
                {
                    Session s = (Session)sessions[k];
                    int time = (int)((DateTime.Now - s.LastTouched).TotalSeconds);
                    if (time > sessionTimeout)
                    {
                        toRemove.Add(k);
                        Console.WriteLine("Removed session " + k);
                    }
                }
            }
            foreach (object k in toRemove) sessions.Remove(k);
        }

        // Response stuff
        static Hashtable Responses = new Hashtable();
        static HttpServer()
        {
            Responses[200] = "OK";
            Responses[302] = "Found";
            Responses[303] = "See Other";
            Responses[400] = "Bad Request";
            Responses[404] = "Not Found";
            Responses[500] = "Misc Server Error";
            Responses[502] = "Server Busy";
        }

        void SendResponse(ClientInfo ci, HttpRequest req, HttpResponse resp, bool close)
        {
            close = true;

            var en = Encoding.UTF8;
#if DEBUG
            Console.WriteLine("Response: " + resp.ReturnCode + Responses[resp.ReturnCode]);
#endif
            ByteBuilder bb = new ByteBuilder();
            bb.Add(en.GetBytes("HTTP/1.1 " + resp.ReturnCode + " " + Responses[resp.ReturnCode] +
                    "\r\nDate: " + DateTime.Now.ToString("R") +
                    "\r\nServer: RedCoronaEmbedded/1.0" +
                    "\r\nConnection: " + (close ? "close" : "Keep-Alive")));
            if (resp.RawContent == null)
            {
                //bb.Add(Encoding.UTF8.GetBytes("\r\nContent-Encoding: utf-8" +
                //    "\r\nContent-Length: " + resp.Content.Length));

                bb.Add(en.GetBytes("\r\nContent-Encoding: "+en.BodyName+"" +
                    "\r\nContent-Length: " + (en.GetByteCount(resp.Content))));
            }
            else
            {
                bb.Add(en.GetBytes("\r\nContent-Length: " + resp.RawContent.Length));
            }
            if (resp.ContentType != null)
            {
                bb.Add(en.GetBytes("\r\nContent-Type: " + resp.ContentType));
            }
            if (req.Session != null)
            {
                bb.Add(en.GetBytes("\r\nSet-Cookie: _sessid=" + req.Session.ID + "; path=/"));
            }
            var cookieHeader = "Set-Cookie";
            foreach (KeyValuePair<string, string> de in resp.Header)
            {
                var name = de.Key;
                var value = de.Value;
                if (name.Equals(cookieHeader, StringComparison.OrdinalIgnoreCase))
                {
                    StringBuilder sb = new StringBuilder();
                    for (var j = 0; j < value.Length - 1; j++)
                    {
                        if (value[j] == ',' && value[j + 1] != ' ')
                        {
                            bb.Add(en.GetBytes("\r\n" + de.Key + ": " + sb.ToString()));
                            sb.Clear();
                            continue;
                        }

                        sb.Append(value[j]);
                    }
                    sb.Append(value[value.Length - 1]);
                    bb.Add(en.GetBytes("\r\n" + de.Key + ": " + sb.ToString()));
                }
                else
                {
                    //bb.Add(Encoding.UTF8.GetBytes("\r\n" + de.Key + ": " + de.Value));
                    bb.Add(en.GetBytes("\r\n" + de.Key + ": " + de.Value));
                }
            }
            bb.Add(en.GetBytes("\r\n\r\n")); // End of header
            if (resp.RawContent != null)
            {
                bb.Add(resp.RawContent);
            }
            else
            {
                //bb.Add(Encoding.UTF8.GetBytes(resp.Content));
                bb.Add(en.GetBytes(resp.Content));
            }
            ci.Send(bb.Read(0, bb.Length));
#if DEBUG
            Console.WriteLine("** SENDING\n" + resp.Content);
#endif
            ci.Clear();
            if (close)
            {
                ci.Close();
            }
            else
            {
                ci.BeginReceive();
            }
        }

        class FallbackHandler : IHttpHandler
        {
            public bool Process(HttpServer server, HttpRequest req, HttpResponse resp)
            {
#if DEBUG
                Console.WriteLine("Processing " + req);
#endif
                server.RequestSession(req);
                StringBuilder sb = new StringBuilder();
                sb.Append("<h3>Session</h3>");
                sb.Append("<p>ID: " + req.Session.ID + "<br>User: " + req.Session.User);
                sb.Append("<h3>Header</h3>");
                sb.Append("Method: " + req.Method + "; URL: '" + req.Url + "'; HTTP version " + req.HttpVersion + "<p>");
                foreach (KeyValuePair<string, string> ide in req.Header) sb.Append(" " + ide.Key + ": " + ide.Value + "<br>");
                sb.Append("<h3>Cookies</h3>");
                foreach (KeyValuePair<string, string> ide in req.Cookies) sb.Append(" " + ide.Key + ": " + ide.Value + "<br>");
                sb.Append("<h3>Query</h3>");
                foreach (KeyValuePair<string, string> ide in req.Query) sb.Append(" " + ide.Key + ": " + ide.Value + "<br>");
                sb.Append("<h3>Content</h3>");
                sb.Append(req.GetContent());
                resp.Content = sb.ToString();
                return true;
            }
        }

        class ServerErrorHandler : IHttpHandler
        {
            private Exception ex
            {
                get;
                set;
            }

            public ServerErrorHandler(Exception e)
            {
                this.ex = e;
            }

            public bool Process(HttpServer server, HttpRequest req, HttpResponse resp)
            {
                server.RequestSession(req);
                resp.Url = req.Url;
                resp.ReturnCode = (int)HttpStatusCode.InternalServerError;
                resp.ContentType = "Content-Type: text/html; charset=utf-8";
                StringBuilder sb = new StringBuilder();
                sb.Append("<h3>Session</h3>");
                sb.Append("<p>ID: " + req.Session.ID + "<br>User: " + req.Session.User);
                sb.Append("<h3>Header</h3>");
                sb.Append("Method: " + req.Method + "; URL: '" + req.Url + "'; HTTP version " + req.HttpVersion + "<p>");
                foreach (KeyValuePair<string, string> ide in req.Header) sb.Append(" " + ide.Key + ": " + ide.Value + "<br>");
                sb.Append("<h3>Cookies</h3>");
                foreach (KeyValuePair<string, string> ide in req.Cookies) sb.Append(" " + ide.Key + ": " + ide.Value + "<br>");
                sb.Append("<h3>Query</h3>");
                foreach (KeyValuePair<string, string> ide in req.Query) sb.Append(" " + ide.Key + ": " + ide.Value + "<br>");
                sb.Append("<h3>Content</h3>");
                sb.Append(req.GetContent());
                sb.Append("<h3>Error:</h3>");
                sb.Append(ex.ToString());
                resp.Content = sb.ToString();
                return true;
            }
        }
    }

    public class HttpRequest
    {
        public bool GotHeader = false;
        public string Method, Url, Page, HttpVersion, Host, HeaderText, QueryString;
        public IPAddress From;
        internal MemoryStream RawStream;
        public Dictionary<string, string> Query = new Dictionary<string, string>(), Header = new Dictionary<string, string>(), Cookies = new Dictionary<string, string>();

        public int ContentLength, BytesRead;
        public Session Session;

        private string _content = null;
        public string GetContent(Encoding encoding = null)
        {
            if (_content != null|| RawData==null)
            {
                return _content;
            }
            _content = (encoding ?? Encoding.UTF8).GetString(RawData);
            return _content;
        }

        private byte[] _rawData = null;
        public byte[] RawData
        {
            get
            {
                if (RawStream != null && _rawData == null)
                {
                    _rawData = RawStream.ToArray();
                    RawStream.Dispose();
                    RawStream = default;
                }
                return _rawData;
            }
        }

        public void Clear()
        {
            GotHeader = false;
            Method = Url = Page = HttpVersion = Host = HeaderText = QueryString = default;
            if (RawStream != null)
            {
                RawStream.Dispose();
                RawStream = default;
            }
            Query.Clear();
            Header.Clear();
            Cookies.Clear();
            ContentLength = BytesRead = default;
        }
    }

    public class HttpResponse
    {
        public int ReturnCode = 200;
        public Dictionary<string, string> Header = new Dictionary<string, string>();
        public string Url, Content, ContentType = "text/html";
        public byte[] RawContent = null;

        public HttpResponse() { }
        public HttpResponse(int code, string content) { ReturnCode = code; Content = content; }

        public void MakeRedirect(string newurl)
        {
            ReturnCode = 303;
            Header["Location"] = newurl;
            Content = "This document is requesting a redirection to <a href=" + newurl + ">" + newurl + "</a>";
        }
    }

    public interface IHttpHandler
    {
        bool Process(HttpServer server, HttpRequest request, HttpResponse response);
    }

    public class Session
    {
        string id;
        IPAddress user;
        DateTime lasttouched;

        Hashtable data = new Hashtable();

        public string ID { get { return id; } }
        public DateTime LastTouched { get { return lasttouched; } }
        public IPAddress User { get { return user; } }

        public object this[object key]
        {
            get { return data[key]; }
            set { data[key] = value; Touch(); }
        }

        public Session(IPAddress user)
        {
            this.user = user;
            this.id = Guid.NewGuid().ToString();
            Touch();
        }

        public void Touch() { lasttouched = DateTime.Now; }
    }

    public class SubstitutingFileReader : IHttpHandler
    {
        // Reads a file, and substitutes <%x>
        HttpRequest req;
        bool substitute = true;

        public bool Substitute { get { return substitute; } set { substitute = value; } }

        public static Hashtable MimeTypes;

        static SubstitutingFileReader()
        {
            MimeTypes = new Hashtable();
            MimeTypes[".html"] = "text/html";
            MimeTypes[".htm"] = "text/html";
            MimeTypes[".css"] = "text/css";
            MimeTypes[".js"] = "application/x-javascript";

            MimeTypes[".png"] = "image/png";
            MimeTypes[".gif"] = "image/gif";
            MimeTypes[".jpg"] = "image/jpeg";
            MimeTypes[".jpeg"] = "image/jpeg";
        }

        public virtual bool Process(HttpServer server, HttpRequest request, HttpResponse response)
        {
            string fn = server.GetFilename(request);
            if (!File.Exists(fn))
            {
                response.ReturnCode = 404;
                response.Content = "File not found.";
                return true;
            }
            string ext = Path.GetExtension(fn);
            string mime = (string)MimeTypes[ext];
            if (mime == null) mime = "application/octet-stream";
            response.ContentType = mime;
            try
            {
                if (mime.Substring(0, 5) == "text/")
                {
                    // Mime type 'text' is substituted
                    StreamReader sr = new StreamReader(fn);
                    response.Content = sr.ReadToEnd();
                    sr.Close();
                    if (substitute)
                    {
                        // Do substitutions
                        Regex regex = new Regex(@"\<\%(?<tag>[^>]+)\>");
                        lock (this)
                        {
                            req = request;
                            response.Content = regex.Replace(response.Content, new MatchEvaluator(RegexMatch));
                        }
                    }
                }
                else
                {
                    FileStream fs = File.Open(fn, FileMode.Open);
                    byte[] buf = new byte[fs.Length];
                    fs.Read(buf, 0, buf.Length);
                    fs.Close();
                    response.RawContent = buf;
                }
            }
            catch (Exception e)
            {
                response.ReturnCode = 500;
                response.Content = "Error reading file: " + e;
                return true;
            }
            return true;
        }

        public virtual string GetValue(HttpRequest req, string tag)
        {
            return "<span class=error>Unknown substitution: " + tag + "</span>";
        }

        string RegexMatch(Match m)
        {
            try
            {
                return GetValue(req, m.Groups["tag"].Value);
            }
            catch (Exception e)
            {
                return "<span class=error>Error substituting " + m.Groups["tag"].Value + "</span>";
            }
        }
    }

    public enum HMethod
    {
        GET,
        POST,
        DELETE,
        PUT
    }
    
}
