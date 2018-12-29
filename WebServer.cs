using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CS_API_Server
{
    public class RequestParam
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public bool Valid { get; set; }

        public RequestParam(string key, string value, bool valid = true)
        {
            Key = key;
            Value = value;
            Valid = valid;
        }
    }

    public class WebServer
    {
        public const string VariableTagExpressionString = ":[a-zA-Z0-9_-]*<(int|string|uuid)>";
        public static (string Key, Regex Expression)[] SupportedValueTypes = {
            ("int", new Regex("^[0-9]*$")),
            ("string", new Regex("^[^\\s/]$")),
            ("uuid", new Regex("^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$"))
        };

        private string Host { get; }
        private bool Running { get; set; }
        private int Port { get; }
        private HttpListener Server { get; }
        private List<(
                string Method,
                Regex Path,
                string RawPath,
                bool HasParams,
                RouteHandlerDelegate Handler
            )> Routes { get; }

        // ToDo: extend Request and response to simplify route logic

        #region constructor

        public WebServer(string host, int port)
        {
            Host = host;
            Port = port;
            Server = new HttpListener();
            Routes = new List<(
                    string Method,
                    Regex Path,
                    string RawPath,
                    bool HasParams,
                    RouteHandlerDelegate Handler
                )>();
        }

        #endregion

        #region Delegates

        public delegate void RouteHandlerDelegate(WebServer.Request request, Response response);

        #endregion

        public void Add(string method, string path, RouteHandlerDelegate handler)
        {
            Server.Prefixes.Add(Host + ":" + Port + path);

            // ToDo: path value needs to be better comprehended into regular expression
            // Such that path parameters are matched
            // A path parameter local url looks like /base/:id<string>/something
            // This indicated that a dynamic string value is to be associated with a localized variable named 'id'

            bool hasParams = false;
            Regex variableTagExpression = new Regex("^" + VariableTagExpressionString + "$");
            Regex variableValueExpression = new Regex("[^\\s/]*");

            string reworkedPath = "/";

            foreach (string part in path.Split('/'))
            {
                if (part.Length == 0)
                {
                    continue;
                }
                if (variableTagExpression.IsMatch(part))
                {
                    hasParams = true;
                    reworkedPath += variableValueExpression + "/";
                }
                else
                {
                    reworkedPath += part + "/";
                }
            }

            Routes.Add((
                    Method: method,
                    Path: new Regex(@"^" + reworkedPath + "$"),
                    RawPath: path,
                    HasParams: hasParams,
                    Handler: handler)
                );
        }

        public void Listen()
        {
            Server.Start();
            Running = true;

            while (Running)
            {
                HttpListenerContext context = Server.GetContext();
                Response response = new Response(context);
                Request request = new Request(context.Request);

                string path = context.Request.Url.LocalPath;
                string method = context.Request.HttpMethod;

                if (path[path.Length - 1] != '/')
                {
                    path += "/";
                }

                var route = Routes.Find(x => x.Path.IsMatch(path) && x.Method == method);
                if (route.HasParams)
                {
                    request.AddDeffinition(true, route.RawPath);
                }

                if (route.Handler != null)
                {
                    route.Handler(request, response);
                    continue;
                }

                #region ToDo

                // ToDo: this commented section demos how to provide static resources.
                // This needs to be encapsulated into operations that allow configuring static paths
                // and then this operation needs to check the path against those configurations.
                // It's fine that response handlers take higher priority when there is a logical overlap

//                string page = Directory.GetCurrentDirectory() + context.Request.Url.LocalPath;
//                if (page == string.Empty)
//                    page = "index.html";
//                TextReader tr = new StreamReader(page);

//                string msg = tr.ReadToEnd();

                // ToDo: change the default response to a 404
                #endregion

                Console.WriteLine(method + " " + path);
                response.Send(404);

                context.Response.Close();
            }
        }

        public void Stop()
        {
            Running = false;
        }

        public class Request
        {
            private bool HasParams { get; set; }
            private string RawPathDef { get; set; }

            public string Body { get; }
            public HttpListenerRequest Raw { get; }

            public Request(HttpListenerRequest request)
            {
                Raw = request;
                HasParams = false;

                if (!request.HasEntityBody)
                {
                    Body = null;
                }

                using (Stream body = Raw.InputStream) // here we have data
                {
                    using (StreamReader reader = new StreamReader(body, Raw.ContentEncoding))
                    {
                        Body = reader.ReadToEnd();
                    }
                }

            }

            public void AddDeffinition(bool hasParams, string rawPathDef)
            {
                HasParams = hasParams;
                RawPathDef = rawPathDef;
            }

            public List<RequestParam> GetParams()
            {
                List <RequestParam> urlParams = new List<RequestParam>();

                Regex variableTagExpression = new Regex("^" + VariableTagExpressionString + "$");

                string[] parts = RawPathDef.Split('/');

                // The last value is always a blank string
                for( int i = 0; i < parts.Length - 1; i++)
                {
                    if (parts[i].Length == 0)
                    {
                        continue;
                    }

                    if (variableTagExpression.IsMatch(parts[i]))
                    {
                        string tagDef = RawPathDef.Split('/')[i];

                        string[] tagParts = tagDef.Split('<');

                        string key = tagParts[0].Substring(1);
                        string valueType = tagParts[1].Substring(0, tagParts[1].Length - 1);

                        var valueTypeDef =new List<(string Key, Regex Expression)>(SupportedValueTypes)
                            .Find(x => x.Key == valueType);

                        bool valid = valueTypeDef.Expression.IsMatch(Raw.Url.Segments[i]);

                        urlParams.Add(new RequestParam(key, Raw.Url.Segments[i], valid));
                    }
                }
                return urlParams;
            }
        }

        public class Response
        {
            private HttpListenerContext Context { get; }

            public Response(HttpListenerContext context)
            {
                Context = context;
            }

            public void Send(int statusCode)
            {
                Context.Response.StatusCode = statusCode;
                Context.Response.Close();
            }

            public void Send(int statusCode, string body)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(body);

                Context.Response.StatusCode = statusCode;
                Context.Response.ContentLength64 = buffer.Length;
                Context.Response.ContentType = "application/json";

                Stream st = Context.Response.OutputStream;
                st.Write(buffer, 0, buffer.Length);

                Context.Response.Close();
            }

        }
    }
}
