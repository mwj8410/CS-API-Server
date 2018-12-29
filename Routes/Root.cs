using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using CS_API_Server.TransportModels;

namespace CS_API_Server.Routes
{
    public class Root
    {
        public void GetRoot(WebServer.Request request, WebServer.Response response)
        {
            response.Send(
                200,
                JsonConvert.SerializeObject(new StandardResponse("Get Resource"))
            );
        }

        public void CreateResource(WebServer.Request request, WebServer.Response response)
        {
            // Ideally, you would provide the created resource, but this is just an example
            response.Send(201, "Create Resource");
        }

        public void DeleteResource(WebServer.Request request, WebServer.Response response)
        {
            List<RequestParam> requestParams = request.GetParams();
            Guid id = new Guid(requestParams.Find(x => x.Key == "Id").Value);

            response.Send(200);
        }
    }
}
