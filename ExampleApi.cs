using System;
using System.Collections;
using System.Collections.Generic;

using CS_API_Server.Routes;

namespace CS_API_Server
{
    class ExampleApi
    {
        static void Main(string[] args)
        {
            List<(string Key, string Value)> env = ExtractEnv();
            WebServer server = new WebServer("http://localhost", 1337);

            Root rootHandler = new Root();

            // Declare routes
            server.Add("GET",    "/", rootHandler.GetRoot);
            server.Add("POST", "/resource/", rootHandler.DeleteResource);
            server.Add("DELETE", "/resource/:Id<uuid>/", rootHandler.DeleteResource);

            server.Listen();

            Console.WriteLine("Listening...");
        }

        public static List<(string Key, string Value)> ExtractEnv()
        {
            List<(string Key, string Value)> outVars = new List<(string Key, string Value)>();

            IDictionary environmentVariables = Environment.GetEnvironmentVariables();
            foreach (DictionaryEntry de in environmentVariables)
            {
                outVars.Add((Key: de.Key.ToString(), Value: de.Value.ToString()));
            }

            return outVars;
        }
    }
}
