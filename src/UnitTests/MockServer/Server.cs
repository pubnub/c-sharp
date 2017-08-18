using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace MockServer
{
    public class Server
    {
        static private Server server = null;
        private X509Certificate2 certificate;
        private TcpListener listener;
        private const int MAXBUFFER = 1024 * 64;
        private string notFoundContent = "<html><head><title>Not Found</title></head><body>Sorry, the object you requested was not found.</body><html>";
        private readonly Uri uri;
        private Thread trf = null;
        private Dictionary<string, Request> requests = new Dictionary<string, Request>();
        private List<string> responses = new List<string>();
        private int read;
        private bool finalizeServer = false;
        private bool secure = false;
        private bool IsRunning = false;

        static public Server Instance()
        {
            if (server == null)
            {
                server = new MockServer.Server(new Uri("https://localhost:9191"));
            }

            return server;
        }

        /// <summary>
        /// Mock Web Server
        /// You must install localhost.cer in Trusted Root Certification Authorities
        /// </summary>
        /// <param name="uri">Uri for server</param>
        public Server(Uri uri)
        {
            this.read = 0;
            this.uri = uri;
            if (this.uri.OriginalString.Contains("https:"))
            {
                secure = true;
            }

            certificate = new X509Certificate2(Resource.localhostPFX, "pubnub");
        }

        /// <summary>
        /// To indicate whether https request or not
        /// </summary>
        /// <param name="secure"></param>
        public void RunOnHttps(bool secure)
        {
            this.secure = secure;
        }

        /// <summary>
        /// Return the cache responses
        /// </summary>
        /// <returns></returns>
        public List<string> GetResponses()
        {
            return responses;
        }

        /// <summary>
        /// Remove all Requests
        /// </summary>
        /// <returns></returns>
        public Server ClearRequests()
        {
            this.requests.Clear();
            this.responses.Clear();
            return this;
        }

        /// <summary>
        /// Add a request to server
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns></returns>
        public Server AddRequest(Request request)
        {
            StringBuilder sb = new StringBuilder();
            string parameters = null;
            foreach (var item in request.Parameters)
            {
                sb.Append(String.Format("&{0}", item));
            }

            if (sb.Length > 0)
            {
                parameters = String.Format("?{0}", sb.ToString().Substring(1));
            }

            string requestUri = String.Format("{0} {1}{2}", request.Method, request.Path, parameters == null ? "" : parameters);
            if (!requests.ContainsKey(requestUri))
            {
                requests.Add(String.Format("{0} {1}{2}", request.Method, request.Path, parameters == null ? "" : parameters), request);
            }
            else
            {
                requests[requestUri] = request;
            }

            return this;
        }

        /// <summary>
        /// Start server
        /// </summary>
        public void Start()
        {
            finalizeServer = false;

            if (trf == null)
            {
                trf = new Thread(new ThreadStart(ServerFunction));
                trf.IsBackground = true;
                trf.Priority = ThreadPriority.Highest;
                trf.SetApartmentState(ApartmentState.MTA);
                trf.Start();
            }
        }

        /// <summary>
        /// Stop server
        /// </summary>
        public void Stop()
        {
            finalizeServer = true;

            LoggingMethod.WriteToLog("Stopping Server...", LoggingMethod.LevelInfo);

            try
            {
                listener.Stop();
            }
            catch
            {
            }

            try
            {
                trf.Abort();
            }
            catch
            {
            }

            trf = null;

            LoggingMethod.WriteToLog("Server was stoped.", LoggingMethod.LevelInfo);

            IsRunning = false;
        }

        /// <summary>
        /// Internal server listener
        /// </summary>
        private void ServerFunction()
        {
            LoggingMethod.WriteToLog("Starting Server...", LoggingMethod.LevelInfo);

        Start:

            try
            {
                var ipEndPoint = new IPEndPoint(IPAddress.Any, uri.Port);
                listener = new TcpListener(ipEndPoint);

                listener.Start();

                while (true)
                {
                    var clientSocket = listener.AcceptSocket();

                    LoggingMethod.WriteToLog(String.Format("Client accepted: {0}", ((IPEndPoint)clientSocket.LocalEndPoint).ToString()), LoggingMethod.LevelInfo);

                    SocketInformation socketInfo = clientSocket.DuplicateAndClose(Process.GetCurrentProcess().Id);

                    Thread trfS = new Thread(new ParameterizedThreadStart((object obj) =>
                    {
                        string strData;
                        byte[] data = new byte[MAXBUFFER];

                        //var sock = (Socket)obj;
                        var sock = new Socket((SocketInformation)obj);
                        SslStream sslStream = null;
                        Stream stream = new NetworkStream(sock);

                        try
                        {
                            if (secure)
                            {
                                /////sslStream = new SslStream(stream, true, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
                                sslStream = new SslStream(stream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
                                sslStream.AuthenticateAsServer(certificate, true, System.Security.Authentication.SslProtocols.Default, false);
                                strData = ReadMessage(sslStream);
                                stream = sslStream;
                            }
                            else
                            {
                                strData = ReadMessage(stream);
                            }

                            ////try
                            ////{
                            ////    ////sslStream = new SslStream(stream, true, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
                            ////    sslStream = new SslStream(stream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
                            ////    sslStream.AuthenticateAsServer(certificate, true, System.Security.Authentication.SslProtocols.Default, false);
                            ////    strData = ReadMessage(sslStream);
                            ////    stream = sslStream;
                            ////}
                            ////catch
                            ////{
                            ////    strData = ReadMessage(stream);
                            ////    strData = "GET /" + strData;
                            ////}

                            LoggingMethod.WriteToLog(String.Format("Request: {0}", strData), LoggingMethod.LevelVerbose);
                        }
                        catch (Exception error)
                        {
                            LoggingMethod.WriteToLog(String.Format("Error: {0}", error.Message), LoggingMethod.LevelError);
                            throw error;
                        }

                        string[] lines = strData.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                        string path = lines[0].Substring(0, lines[0].LastIndexOf(" "));
                        responses.Add(path);
                        System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("        ###  MM/dd/yyyy HH:mm:ss:fff") + " - " + path);

                        try
                        {
                            Request item = null;
                            try
                            {
                                item = requests[path];
                            }
                            catch
                            {
                                try
                                {
                                    item = requests[path.Substring(0, path.IndexOf("?"))];
                                }
                                catch
                                {
                                    item = new MockServer.Request();
                                    item.Method = "GET";

                                    if (path.Contains("GET /v2/presence/") && !path.Contains("/leave?"))
                                    {
                                        item.Response = "{\"t\":{\"t\":\"14844074079055214\",\"r\":7},\"m\":[]}";
                                        item.StatusCode = HttpStatusCode.OK;
                                    }
                                    else if (path.Contains("GET /v2/subscribe/"))
                                    {
                                        item.Response = "{}";
                                        item.StatusCode = HttpStatusCode.OK;
                                    }
                                    else if (path.Contains("GET /time/0"))
                                    {
                                        item.Response = "[14827611897607991]";
                                        item.StatusCode = HttpStatusCode.OK;
                                    }
                                    else if (path.Contains("/leave?"))
                                    {
                                        item.Response = "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}";
                                        item.StatusCode = HttpStatusCode.OK;
                                    }
                                    else if (path.Contains("GET /publish/"))
                                    {
                                        item.Response = "[1,\"Sent\",\"14715322883933786\"]";
                                        item.StatusCode = HttpStatusCode.OK;
                                    }
                                    else if(path.Contains("DELETE /v3/history/sub-key"))
                                    {
                                        item.Response = "{\"status\": 200, \"error\": false, \"error_message\": \"\"}";
                                        item.StatusCode = HttpStatusCode.OK;
                                    }
                                    else
                                    {
                                        item.Response = "";
                                        item.StatusCode = HttpStatusCode.OK;    //// HttpStatusCode.NotFound;
                                    }
                                }
                            }

                            LoggingMethod.WriteToLog(String.Format("Response: {0}", item.Response), LoggingMethod.LevelVerbose);

                            switch (item.StatusCode)
                            {
                                case HttpStatusCode.OK:
                                    {
                                        string statusOK = "HTTP/1.1 200 OK\r\n";
                                        statusOK += "Content-type: text/html\r\n";
                                        statusOK += String.Format("Content-length: {0}\r\n\r\n", item.Response.Length.ToString());
                                        statusOK += item.Response;
                                        stream.Write(System.Text.Encoding.UTF8.GetBytes(statusOK), 0, statusOK.Length);
                                        Thread.Sleep(10);

                                        break;
                                    }

                                case HttpStatusCode.BadRequest:
                                default:
                                    {
                                        string statusBadRequest = "HTTP/1.1 400 Bad Request\r\n";
                                        statusBadRequest += "Content-type: text/html\r\n";
                                        statusBadRequest += String.Format("Content-length: {0}\r\n\r\n", item.Response.Length.ToString());
                                        statusBadRequest += item.Response;
                                        stream.Write(System.Text.Encoding.UTF8.GetBytes(statusBadRequest), 0, statusBadRequest.Length);
                                        Thread.Sleep(10);
                                        break;
                                    }

                                case HttpStatusCode.Unauthorized:
                                    break;
                                case HttpStatusCode.Forbidden:
                                    break;
                                case HttpStatusCode.NotFound:
                                    {
                                        string statusNotFound = "HTTP/1.1 404 Not Found\r\n";
                                        statusNotFound = "HTTP/1.1 404 Not Found\r\n";
                                        statusNotFound += "Content-type: text/html\r\n";
                                        statusNotFound += String.Format("Content-length: {0}\r\n\r\n", item.Response.Length.ToString());
                                        statusNotFound += item.Response;
                                        stream.Write(System.Text.Encoding.UTF8.GetBytes(statusNotFound), 0, statusNotFound.Length);
                                        Thread.Sleep(10);
                                        break;
                                    }
                            }
                        }
                        catch (Exception eHttp)
                        {
                            LoggingMethod.WriteToLog(String.Format("Path not found: {0}", strData), LoggingMethod.LevelError);
                            string statusNotFound = "HTTP/1.1 404 Not Found\r\n";
                            statusNotFound = "HTTP/1.1 404 Not Found\r\n";
                            statusNotFound += "Content-type: text/html\r\n";
                            statusNotFound += String.Format("Content-length: {0}\r\n\r\n", notFoundContent.Length.ToString());
                            statusNotFound += notFoundContent;
                            stream.Write(System.Text.Encoding.UTF8.GetBytes(statusNotFound), 0, statusNotFound.Length);
                            Thread.Sleep(10);
                        }

                        if (sslStream != null)
                        {
                            sslStream.Flush();
                            sslStream.Close();
                        }

                        stream.Flush();
                        stream.Close();
                        clientSocket.Close();
                    }));

                    trfS.IsBackground = true;
                    trfS.SetApartmentState(ApartmentState.MTA);

                    trfS.Start(socketInfo);
                }
            }
            catch (Exception error)
            {
                if (!finalizeServer)
                {
                    LoggingMethod.WriteToLog(String.Format("Error: {0}", error.Message), LoggingMethod.LevelVerbose);
                    listener.Stop();
                    goto Start;
                }
            }
        }

        /// <summary>
        /// for testing purpose only, accept any dodgy certificate... 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// Read a message from Stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Message String</returns>
        private static string ReadMessage(Stream stream)
        {
            byte[] buffer = new byte[2048];
            StringBuilder messageData = new StringBuilder();
            int bytes = -1;
            do
            {
                bytes = stream.Read(buffer, 0, buffer.Length);

                // Use Decoder class to convert from bytes to UTF8
                // in case a character spans two buffers.
                Decoder decoder = Encoding.UTF8.GetDecoder();
                char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                decoder.GetChars(buffer, 0, bytes, chars, 0);
                messageData.Append(chars);
                if (messageData.ToString().Contains("\r\n\r\n"))
                {
                    string msg = messageData.ToString();
                    int index = msg.ToLower().IndexOf("content-length: ");
                    if (index != -1)
                    {
                        int indexlast = msg.IndexOf("\r\n\r\n");
                        int value = Convert.ToInt32(msg.Substring(index + 16, msg.IndexOf("\r\n", index) - index - 16));

                        if (msg.Length == indexlast + 4 + value)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            while (bytes != 0);

            return messageData.ToString();
        }
    }
}
