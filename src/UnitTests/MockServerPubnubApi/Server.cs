﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        static private Server server;
        private readonly X509Certificate2 certificate;
        private TcpListener listener;
        private const int MAXBUFFER = 1024 * 64;
        private string notFoundContent = "<html><head><title>Not Found</title></head><body>Sorry, the object you requested was not found.</body><html>";
        private readonly Uri uri;
        private Thread trf;
        private Dictionary<string, Request> requests = new Dictionary<string, Request>();
        private List<string> responses = new List<string>();
        private bool finalizeServer;
        private bool secure;

        public static Server Instance()
        {
            return server ??= new MockServer.Server(new Uri("http://localhost:9191"));
        }

        /// <summary>
        /// Mock Web Server
        /// You must install localhost.cer in Trusted Root Certification Authorities
        /// </summary>
        /// <param name="uri">Uri for server</param>
        public Server(Uri uri)
        {
            this.uri = uri;
            if (this.uri.OriginalString.Contains("https:"))
            {
                secure = true;
            }
            if (secure)
            {
                certificate = new X509Certificate2(Resource.localhostPFX, "pubnub");
            }
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
            var allParameters = string.Empty;
            foreach (var parameter in request.Parameters)
            {
                allParameters += parameter;
            }
            var requestUriOutset = $"{request.Method} {request.Path} {allParameters}";
            requests[requestUriOutset] = request;
            return this;
        }
        
        /// <summary>
        /// Turns a request URL into a pseudo-hash used for the requests dictionary
        /// </summary>
        private string UrlToRequestKey(string requestUri)
        {
            var spaceIndex = requestUri.IndexOf(" ", StringComparison.Ordinal);
            var questionIndex = requestUri.IndexOf("?", StringComparison.Ordinal);
            var method = requestUri.Substring(0, spaceIndex);
            var path = requestUri.Substring(spaceIndex+1,questionIndex - (spaceIndex + 1));
            var joinedParams = requestUri.Substring(questionIndex+1);
            var splitParams = joinedParams.Split("&").ToList();
            splitParams.Sort();
            var allParameters = string.Empty;
            foreach (var parameter in splitParams)
            {
                allParameters += parameter;
            }
            var key = $"{method} {path} {allParameters}";
            return key;
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
                /* ignore */
            }

            try
            {
                trf.Abort();
            }
            catch
            {
                /* ignore */
            }

            trf = null;

            LoggingMethod.WriteToLog("Server was stoped.", LoggingMethod.LevelInfo);
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

                        var sock = new Socket((SocketInformation)obj);
                        SslStream sslStream = null;
                        Stream stream = new NetworkStream(sock);

                        try
                        {
                            if (secure)
                            {
                                sslStream = new SslStream(stream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
                                sslStream.AuthenticateAsServer(certificate, true, System.Security.Authentication.SslProtocols.Default, false);
                                strData = ReadMessage(sslStream);
                                stream = sslStream;
                            }
                            else
                            {
                                strData = ReadMessage(stream);
                            }

                            LoggingMethod.WriteToLog(String.Format("Request: {0}", strData), LoggingMethod.LevelVerbose);
                        }
                        catch (Exception error)
                        {
                            LoggingMethod.WriteToLog(String.Format("Error: {0}", error), LoggingMethod.LevelError);
                            throw;
                        }

                        string[] lines = strData.Split(new [] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                        string url = lines[0].Substring(0, lines[0].LastIndexOf(" ", StringComparison.InvariantCultureIgnoreCase));
                        Debug.WriteLine(DateTime.Now.ToString("        ###  MM/dd/yyyy HH:mm:ss:fff") + " - " + url);
                        string path = url.Substring(0, url.LastIndexOf("?", StringComparison.InvariantCultureIgnoreCase));
                        responses.Add(path);

                        try
                        {
                            if (!requests.TryGetValue(UrlToRequestKey(url), out var item))
                            {
                                LoggingMethod.WriteToLog("Request not found in Mock Server!", LoggingMethod.LevelVerbose);
                                item = new Request()
                                {
                                    Response = this.notFoundContent,
                                    StatusCode = HttpStatusCode.NotFound
                                };
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
                            System.Diagnostics.Debug.WriteLine(eHttp.ToString());
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
                        sock.Close(1000);
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
                    int index = msg.ToLowerInvariant().IndexOf("content-length: ");
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
