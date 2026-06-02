using LJC.FrameWork.Data.EntityDataBase;
using LJC.FrameWork.LogManager;
using LJC.FrameWork.Net.HTTP.Server;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.SOA
{
    internal class ApiSimulateHandler : IHttpHandler, IStreamingHttpHandler
    {
        private const int StreamingThreshold = 1024 * 1000;

        private static IEnumerable<byte[]> SplitBytes(byte[] data, int chunkSize)
        {
            if (data == null || data.Length == 0)
            {
                yield break;
            }

            for (var offset = 0; offset < data.Length; offset += chunkSize)
            {
                var size = Math.Min(chunkSize, data.Length - offset);
                var chunk = new byte[size];
                Buffer.BlockCopy(data, offset, chunk, 0, size);
                yield return chunk;
            }
        }

        private static Contract.WebRequest BuildWebRequest(HttpRequest request, string url, byte[] inputData = null, int? inputDataLength = null)
        {
            return new Contract.WebRequest
            {
                Host = request.Host,
                VirUrl = url,
                Cookies = request.Cookies,
                Headers = request.Header,
                Method = request.Method,
                InputData = inputData,
                InputDataLength = inputDataLength ?? (inputData == null ? 0 : inputData.Length)
            };
        }

        public bool Begin(HttpServer server, HttpRequest request, HttpResponse response)
        {
            server.RequestSession(request).Touch();

            if (SimulateServerManager.TransferRequestChunkSessionFactory == null)
            {
                return false;
            }

            response.StreamWriter = (sendWebResponse) =>
            {
                try
                {
                    request.Tag = SimulateServerManager.TransferRequestChunkSessionFactory(BuildWebRequest(request, NormalizeUrl(request), null, request.ContentLength), (data, isLast, code, contentType, headers) =>
                    {
                        try
                        {
                            response.ReturnCode = code;
                            if (headers != null)
                            {
                                foreach (var kv in headers)
                                {
                                    response.Header[kv.Key] = kv.Value;
                                }
                            }
                            if (!string.IsNullOrWhiteSpace(contentType)) response.ContentType = contentType;
                            sendWebResponse(data, isLast, code, contentType, headers);
                        }
                        catch { }
                    });
                }
                catch (Exception)
                {
                    try { sendWebResponse(new byte[0], true, 500, null, null); } catch { }
                }
            };

            return true;
        }

        public void Append(HttpServer server, HttpRequest request, byte[] bytes, int offset, int count, bool isLast)
        {
            var session = request.Tag as SimulateServerManager.IWebRequestChunkSession;
            if (session == null)
            {
                return;
            }

            if (count <= 0)
            {
                if (isLast)
                {
                    session.Append(new byte[0], true);
                }
                return;
            }

            var chunk = new byte[count];
            Buffer.BlockCopy(bytes, offset, chunk, 0, count);
            session.Append(chunk, isLast);
        }

        private string NormalizeUrl(HttpRequest request)
        {
            var url = request.Url;
            if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                var sqlArray = url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (sqlArray.Length > 2)
                {
                    url = string.Join("/", sqlArray.Skip(2).ToArray());
                }
            }
            else
            {
                url = string.Join("/", url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries));
            }

            if (request.Url.EndsWith("/"))
            {
                url += "/";
            }

            return url;
        }

        public bool Process(HttpServer server, HttpRequest request, HttpResponse response)
        {
            server.RequestSession(request).Touch();

            var url = NormalizeUrl(request);
            var isLargeRequest = request.ContentLength > StreamingThreshold;

            var ipHeader = "X-Forwarded-For";
            if (request.Header.ContainsKey(ipHeader))
            {
                request.Header[ipHeader] = request.From.ToString() + "," + request.Header[ipHeader];
            }
            else
            {
                request.Header.Add(ipHeader, request.From.ToString());
            }
            if (isLargeRequest)
            {
                LogHelper.Instance.Debug(string.Format("大数据请求：{0},{1}b", url, request.ContentLength));
            }

            if (isLargeRequest && SimulateServerManager.TransferRequestChunkSessionFactory != null)
            {
                return Begin(server, request, response);
            }

            var rawData = request.RawData;

            if (SimulateServerManager.TransferRequestStream != null)
            {
                // Assign stream writer (HttpServer.HttpResponse.StreamWriter exists and accepted by runtime)
                response.StreamWriter = (sendWebResponse) =>
                {
                    try
                    {
                        SimulateServerManager.TransferRequestStream(new Contract.WebRequest
                        {
                            Host = request.Host,
                            VirUrl = url,
                            Cookies = request.Cookies,
                            Headers = request.Header,
                            Method = request.Method,
                            InputData = rawData,
                            InputDataLength = rawData == null ? 0 : rawData.Length
                        }, (data, isLast, code, contentType, headers) =>
                        {
                            try
                            {
                                response.ReturnCode = code;
                                if (headers != null)
                                {
                                    foreach (var kv in headers)
                                    {
                                        response.Header[kv.Key] = kv.Value;
                                    }
                                }
                                if (!string.IsNullOrWhiteSpace(contentType)) response.ContentType = contentType;
                                sendWebResponse(data, isLast, code, contentType, headers);
                            }
                            catch { }
                        });
                    }
                    catch (Exception)
                    {
                        try { sendWebResponse(new byte[0], true, 500, null, null); } catch { }
                    }
                };

                return true;
            }

            var simulateResponse = SimulateServerManager.TransferRequest(new Contract.WebRequest
            {
                Host = request.Host,
                VirUrl = url,
                Cookies = request.Cookies,
                Headers = request.Header,
                Method = request.Method,
                InputData = rawData
            });

            if (isLargeRequest)
            {
                LogHelper.Instance.Debug(string.Format("大数据请求完成：{0},{1}b", url, request.ContentLength));
            }
            if (simulateResponse != null)
            {
                response.Header = simulateResponse.Headers ?? new Dictionary<string, string>();
                response.RawContent = simulateResponse.ResponseData;
                if (!string.IsNullOrWhiteSpace(simulateResponse.ContentType))
                {
                    response.ContentType = simulateResponse.ContentType;
                }
                response.ReturnCode = simulateResponse.ResponseCode;
                response.Url = simulateResponse.Url;

                return true;
            }
            else if (url.Split('?')[0].EndsWith("_sitelist", StringComparison.OrdinalIgnoreCase))
            {
                response.ContentType = string.Format("{0}; charset={1}", "text/html", "utf-8");
                var html = "";
                foreach (var web in SimulateServerManager.GetWebMapperList())
                {
                    html += string.Format("{0}:{1}<br/>", web.MappingRoot, web.MappingPort);
                }
                response.Content = html;
                return true;
            }
            else
            {
                response.ContentType = string.Format("{0}; charset={1}", "text/html", "utf-8");
                response.ReturnCode = 404;
                response.Content = request.Url + " 不存在！";

                return true;
            }
        }
    }
}
