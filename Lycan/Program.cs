using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Lycan
{
    internal static class Server
    {
        private const int Port = 8888;

        private static readonly HttpListener Listener = new HttpListener { Prefixes = { $"http://localhost:{Port}/" } };

        private static bool _keepGoing = true;

        private static Task _mainLoop;


        public static object DeserializeJson<T>(string Json)
        {
            JavaScriptSerializer JavaScriptSerializer = new JavaScriptSerializer();
            return JavaScriptSerializer.Deserialize<T>(Json);
        }

    public class RESPONCEOBJ
        {
            public string Url { get; set; }
        }
        public static void StartWebServer()
        {
            Console.WriteLine("Listening...");
            if (_mainLoop != null && !_mainLoop.IsCompleted) return; //Already started
            _mainLoop = MainLoopAsync();
        }

        public static async void StopWebServerAsync()
        {
            _keepGoing = false;
            lock (Listener)
            {
                //Use a lock so we don't kill a request that's currently being processed
                Listener.Stop();
            }
            try
            {
                await _mainLoop.ConfigureAwait(false);
            }
            catch { /* je ne care pas */ }
        }

        private static async Task MainLoopAsync()
        {
            Listener.Start();
            while (_keepGoing)
            {
                try
                {
                    //GetContextAsync() returns when a new request come in
                    var context = await Listener.GetContextAsync().ConfigureAwait(true);
                    lock (Listener)
                    {
                        if (_keepGoing) ProcessRequest(context);
                    }
                }
                catch (Exception e)
                {
                    if (e is HttpListenerException) return; //this gets thrown when the listener is stopped
                    Console.WriteLine(e.Message);
                }
            }
        }

        private static void ProcessRequest(HttpListenerContext context)
        {
            using (var response = context.Response)
            {
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "POST, GET");
                try
                {
                    var handled = false;
                    switch (context.Request.Url.AbsolutePath)
                    {
                        //Todo add request urls you need to handle
                        case "/":
                            //Todo deal with request methods
                            switch (context.Request.HttpMethod)
                            {
                                case "POST":
                                    //Update the settings
                                    using (var body = context.Request.InputStream)
                                    using (var reader = new StreamReader(body, context.Request.ContentEncoding))
                                    {
                                        Console.WriteLine("Received request!");

                                        //Get the data that was sent to us
                                        var json = reader.ReadToEnd();


                                        //var URL = JsonConvert.DeserializeObject<UrlObj>(json).Url;


                                        //TODO Deal with the request
                                        RESPONCEOBJ responseObj = (RESPONCEOBJ)DeserializeJson<RESPONCEOBJ>(json);
                                        Console.WriteLine("Request URL: " + responseObj.Url);

                                        //Return 204 No Content to say we did it successfully
                                        response.StatusCode = 204;
                                        handled = true;
                                    }
                                    break;
                            }
                            break;
                    }
                    if (!handled)
                    {
                        response.StatusCode = 404;
                    }
                }
                catch (Exception e)
                {
                    //Return the exception details the client - you may or may not want to do this
                    response.StatusCode = 500;
                    //response.ContentType = "application/json";
                    //var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(e));
                    //response.ContentLength64 = buffer.Length;
                    //response.OutputStream.Write(buffer, 0, buffer.Length);

                    //TODO: Log the exception
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //Start the web server
                Console.WriteLine("Listening on http://127.0.0.1:8888");
                Task.Factory.StartNew(Server.StartWebServer);
            }
            catch
            {
                //Stop the web server
                Server.StopWebServerAsync();
            }

            Console.ReadLine();
        }
    }
}
