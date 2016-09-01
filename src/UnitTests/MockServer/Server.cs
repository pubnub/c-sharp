using System;
using System.Collections.Generic;
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
        private X509Certificate2 certificate;
        private TcpListener listener;
        private const int MAXBUFFER = 1024 * 64;
        private byte[] data = new byte[MAXBUFFER];
        private string strData;
        private string notFoundContent = "<html><head><title>Not Found</title></head><body>Sorry, the object you requested was not found.</body><html>";
        private readonly Uri uri;
        private Thread trf = null;
        private SortedList<string, Request> requests = new SortedList<string, Request>();
        private int read = 0;
        private bool secure = false;

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
            certificate = new X509Certificate2(Resource.localhostPFX, "pubnub");
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
            if (sb.Length>0)
            {
                parameters = String.Format("?{0}", sb.ToString().Substring(1));
            }
            requests.Add(String.Format("{0} {1}{2}", request.Method, request.Path, parameters == null ? "" : parameters), request);
            return this;
        }

        /// <summary>
        /// Start server
        /// </summary>
        public void Start()
        {
            if (trf == null)
            {
                trf = new Thread(new ThreadStart(ServerFunction));
                trf.IsBackground = true;
                trf.SetApartmentState(ApartmentState.MTA);
                trf.Start();
            }
        }

        /// <summary>
        /// Stop server
        /// </summary>
        public void Stop()
        {
            LoggingMethod.WriteToLog("Stopping Server...", LoggingMethod.LevelInfo);

            try
            {
                listener.Stop();
            }
            catch { }

            try
            {
                trf.Abort();
            }
            catch { }
            trf = null;

            LoggingMethod.WriteToLog("Server was stoped.", LoggingMethod.LevelInfo);
        }

        /// <summary>
        /// Internal server listener
        /// </summary>
        private void ServerFunction()
        {
            LoggingMethod.WriteToLog("Starting Server...", LoggingMethod.LevelInfo);

            SslStream sslStream = null;
            Stream stream = null;

        Start:

            try
            {

                var ipEndPoint = new IPEndPoint(IPAddress.Any, uri.Port);
                listener = new TcpListener(ipEndPoint);

                listener.Start(255);

                while (true)
                {

                    data = new byte[MAXBUFFER];

                    var clientSocket = listener.AcceptTcpClient();

                    LoggingMethod.WriteToLog(String.Format("Client accepted: {0}", clientSocket.Client.LocalEndPoint.ToString()), LoggingMethod.LevelInfo);

                    try
                    {
                        if (secure)
                        {
                            sslStream = new SslStream(clientSocket.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
                            sslStream.AuthenticateAsServer(certificate, true, System.Security.Authentication.SslProtocols.Default, false);
                            strData = ReadMessage(sslStream);
                            stream = sslStream;
                        }
                        else
                        {
                            stream = clientSocket.GetStream();
                            strData = ReadMessage(stream);
                        }

                        LoggingMethod.WriteToLog(String.Format("Request: {0}", strData), LoggingMethod.LevelVerbose);
                    }
                    catch (Exception error)
                    {
                        LoggingMethod.WriteToLog(String.Format("Error: {0}", error.Message), LoggingMethod.LevelError);
                        throw error;
                    }


                    string[] lines = strData.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    string path = lines[0].Substring(0, lines[0].LastIndexOf(" "));

                    try
                    {
                        Request item = null;
                        try
                        {
                            item = requests[path];
                        }
                        catch
                        {
                            item = requests[path.Substring(0, path.IndexOf("?"))];
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
                    catch
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

                    stream.Close();
                    clientSocket.Close();
                }
            }
            catch (Exception error)
            {
                LoggingMethod.WriteToLog(String.Format("Error: {0}", error.Message), LoggingMethod.LevelVerbose);
                listener.Stop();
                goto Start;
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
        static string ReadMessage(Stream stream)
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
                    break;
                }
            } while (bytes != 0);

            return messageData.ToString();
        }
    }
}
