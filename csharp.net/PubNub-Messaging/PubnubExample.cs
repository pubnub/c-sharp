using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;

namespace PubNubMessaging.Core
{
    public class PubnubExample
    {
        static public Pubnub pubnub;

        static public bool deliveryStatus = false;
        static public string channel = "";
        static public bool showErrorMessageSegments = false;
        static public bool showDebugMessages = false;
        static public string authKey = "";
        static public int presenceHeartbeat = 0;
        static public int presenceHeartbeatInterval = 0;

        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            //Console.WriteLine(e.ExceptionObject.ToString());
            Console.WriteLine("Unhandled exception occured inside Pubnub C# API. Exiting the application. Please try again.");
            Environment.Exit(1);
        }

        static public void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            PubnubProxy proxy = null;
            
            Console.WriteLine("HINT: TO TEST RE-CONNECT AND CATCH-UP,");
            Console.WriteLine("      DISCONNECT YOUR MACHINE FROM NETWORK/INTERNET AND ");
            Console.WriteLine("      RE-CONNECT YOUR MACHINE AFTER SOMETIME.");
            Console.WriteLine();
            Console.WriteLine("      IF NO NETWORK BEFORE MAX RE-TRY CONNECT,");
            Console.WriteLine("      NETWORK ERROR MESSAGE WILL BE SENT");
            Console.WriteLine();

            Console.WriteLine("Enter Pubnub Origin. Default Origin = pubsub.pubnub.com");
            Console.WriteLine("If you want to accept default value, press ENTER.");
            string origin = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (origin.Trim() == "")
            {
                origin = "pubsub.pubnub.com";
                Console.WriteLine("Default Origin selected");
            }
            else
            {
                Console.WriteLine("Pubnub Origin Provided");
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("Enable SSL? ENTER Y for Yes, else N");
            string enableSSL = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (enableSSL.Trim().ToLower() == "y")
            {
                Console.WriteLine("SSL Enabled");
            }
            else
            {
                Console.WriteLine("SSL NOT Enabled");
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("ENTER cipher key for encryption feature.");
            Console.WriteLine("If you don't want to avail at this time, press ENTER.");
            string cipherKey = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (cipherKey.Trim().Length > 0)
            {
                Console.WriteLine("Cipher key provided.");
            }
            else
            {
                Console.WriteLine("No Cipher key provided");
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("ENTER subscribe key.");
            Console.WriteLine("If you want to accept default demo subscribe key, press ENTER.");
            string subscribeKey = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (subscribeKey.Trim().Length > 0)
            {
                Console.WriteLine("Subscribe key provided.");
            }
            else
            {
                Console.WriteLine("Default demo subscribe key provided");
                subscribeKey = "demo-36";
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("ENTER publish key.");
            Console.WriteLine("If you want to accept default demo publish key, press ENTER.");
            string publishKey = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (publishKey.Trim().Length > 0)
            {
                Console.WriteLine("Publish key provided.");
            }
            else
            {
                Console.WriteLine("Default demo publish key provided");
                publishKey = "demo-36";
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("ENTER secret key.");
            Console.WriteLine("If you don't want to avail at this time, press ENTER.");
            string secretKey = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (secretKey.Trim().Length > 0)
            {
                Console.WriteLine("Secret key provided.");
            }
            else
            {
                Console.WriteLine("Default demo Secret key provided");
                secretKey = "demo-36";
            }
            Console.ResetColor();
            Console.WriteLine();

            pubnub = new Pubnub(publishKey, subscribeKey, secretKey, cipherKey,
                (enableSSL.Trim().ToLower() == "y") ? true : false);

            pubnub.Origin = origin;

            Console.WriteLine("Use Custom Session UUID? ENTER Y for Yes, else N");
            string enableCustomUUID = Console.ReadLine();
            if (enableCustomUUID.Trim().ToLower() == "y")
            {
                Console.WriteLine("ENTER Session UUID.");
                string sessionUUID = Console.ReadLine();
                pubnub.SessionUUID = sessionUUID;
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Accepted Custom Session UUID.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Default Session UUID opted.");
                Console.ResetColor();
            }
            Console.WriteLine();

            Console.WriteLine("By default Resume On Reconnect is enabled. Do you want to disable it? ENTER Y for Yes, else N");
            string disableResumeOnReconnect = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (disableResumeOnReconnect.Trim().ToLower() == "y")
            {
                Console.WriteLine("Resume On Reconnect Disabled");
                pubnub.EnableResumeOnReconnect = false;
            }
            else
            {
                Console.WriteLine("Resume On Reconnect Enabled by default");
                pubnub.EnableResumeOnReconnect = true;
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("Subscribe Timeout = 310 seconds (default). Enter the value to change, else press ENTER");
            string subscribeTimeoutEntry = Console.ReadLine();
            int subscribeTimeout;
            Int32.TryParse(subscribeTimeoutEntry, out subscribeTimeout);
            Console.ForegroundColor = ConsoleColor.Blue;
            if (subscribeTimeout > 0)
            {
                Console.WriteLine("Subscribe Timeout = {0}",subscribeTimeout);
                pubnub.SubscribeTimeout = subscribeTimeout;
            }
            else
            {
                Console.WriteLine("Subscribe Timeout = {0} (default)", pubnub.SubscribeTimeout);
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("Non Subscribe Timeout = 15 seconds (default). Enter the value to change, else press ENTER");
            string nonSubscribeTimeoutEntry = Console.ReadLine();
            int nonSubscribeTimeout;
            Int32.TryParse(nonSubscribeTimeoutEntry, out nonSubscribeTimeout);
            Console.ForegroundColor = ConsoleColor.Blue;
            if (nonSubscribeTimeout > 0)
            {
                Console.WriteLine("Non Subscribe Timeout = {0}", nonSubscribeTimeout);
                pubnub.NonSubscribeTimeout = nonSubscribeTimeout;
            }
            else
            {
                Console.WriteLine("Non Subscribe Timeout = {0} (default)", pubnub.NonSubscribeTimeout);
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("Network Check MAX retries = 50 (default). Enter the value to change, else press ENTER");
            string networkCheckMaxRetriesEntry = Console.ReadLine();
            int networkCheckMaxRetries;
            Int32.TryParse(networkCheckMaxRetriesEntry, out networkCheckMaxRetries);
            Console.ForegroundColor = ConsoleColor.Blue;
            if (networkCheckMaxRetries > 0)
            {
                Console.WriteLine("Network Check MAX retries = {0}", networkCheckMaxRetries);
                pubnub.NetworkCheckMaxRetries = networkCheckMaxRetries;
            }
            else
            {
                Console.WriteLine("Network Check MAX retries = {0} (default)", pubnub.NetworkCheckMaxRetries);
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("Network Check Retry Interval = 10 seconds (default). Enter the value to change, else press ENTER");
            string networkCheckRetryIntervalEntry = Console.ReadLine();
            int networkCheckRetryInterval;
            Int32.TryParse(networkCheckRetryIntervalEntry, out networkCheckRetryInterval);
            Console.ForegroundColor = ConsoleColor.Blue;
            if (networkCheckRetryInterval > 0)
            {
                Console.WriteLine("Network Check Retry Interval = {0} seconds", networkCheckRetryInterval);
                pubnub.NetworkCheckRetryInterval = networkCheckRetryInterval;
            }
            else
            {
                Console.WriteLine("Network Check Retry Interval = {0} seconds (default)", pubnub.NetworkCheckRetryInterval);
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("Local Client Heartbeat Interval = 15 seconds (default). Enter the value to change, else press ENTER");
            string heartbeatIntervalEntry = Console.ReadLine();
            int localClientHeartbeatInterval;
            Int32.TryParse(heartbeatIntervalEntry, out localClientHeartbeatInterval);
            Console.ForegroundColor = ConsoleColor.Blue;
            if (localClientHeartbeatInterval > 0)
            {
                Console.WriteLine("Heartbeat Interval = {0} seconds", localClientHeartbeatInterval);
                pubnub.LocalClientHeartbeatInterval = localClientHeartbeatInterval;
            }
            else
            {
                Console.WriteLine("Heartbeat Interval = {0} seconds (default)", pubnub.LocalClientHeartbeatInterval);
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("HTTP Proxy Server with NTLM authentication(IP + username/pwd) exists? ENTER Y for Yes, else N");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("NOTE: Pubnub example is being tested with CCProxy 7.3 Demo version");
            Console.ResetColor();
            string enableProxy = Console.ReadLine();
            if (enableProxy.Trim().ToLower() == "y")
            {
                bool proxyAccepted = false;
                while (!proxyAccepted)
                {
                    Console.WriteLine("ENTER proxy server name or IP.");
                    string proxyServer = Console.ReadLine();
                    Console.WriteLine("ENTER port number of proxy server.");
                    string proxyPort = Console.ReadLine();
                    int port;
                    Int32.TryParse(proxyPort, out port);
                    Console.WriteLine("ENTER user name for proxy server authentication.");
                    string proxyUsername = Console.ReadLine();
                    Console.WriteLine("ENTER password for proxy server authentication.");
                    string proxyPassword = Console.ReadLine();
                    
                    proxy = new PubnubProxy();
                    proxy.ProxyServer = proxyServer;
                    proxy.ProxyPort = port;
                    proxy.ProxyUserName = proxyUsername;
                    proxy.ProxyPassword = proxyPassword;
                    Console.ForegroundColor = ConsoleColor.Blue;
                    try
                    {
                        pubnub.Proxy = proxy;
                        proxyAccepted = true;
                        Console.WriteLine("Proxy details accepted");
                        Console.ResetColor();
                    }
                    catch (MissingFieldException mse)
                    {
                        Console.WriteLine(mse.Message);
                        Console.WriteLine("Please RE-ENTER Proxy Server details.");
                    }
                    Console.ResetColor();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("No Proxy");
                Console.ResetColor();
            }
            Console.WriteLine();

            Console.WriteLine("Enter Auth Key. If you don't want to use Auth Key, Press ENTER Key");
            authKey = Console.ReadLine();
            pubnub.AuthenticationKey = authKey;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(string.Format("Auth Key = {0}", authKey));
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("Display ErrorCallback messages? Enter Y for Yes, Else N for No.");
            Console.WriteLine("Default = N  ");
            string displayErrMessage = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (displayErrMessage.Trim().ToLower() == "y" )
            {
                showErrorMessageSegments = true;
                Console.WriteLine("ErrorCallback messages will  be displayed");
            }
            else
            {
                showErrorMessageSegments = false;
                Console.WriteLine("ErrorCallback messages will NOT be displayed.");
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("Display Debug Info in ErrorCallback messages? Enter Y for Yes, Else N for No.");
            Console.WriteLine("Default = Y  ");
            string debugMessage = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (debugMessage.Trim().ToLower() == "n")
            {
                showDebugMessages = false;
                Console.WriteLine("ErrorCallback messages will NOT  be displayed");
            }
            else
            {
                showDebugMessages = true;
                Console.WriteLine("Debug messages will be displayed.");
            }
            Console.ResetColor();
            Console.WriteLine();


            bool exitFlag = false;
            string channel="";
            int currentUserChoice = 0;
            string userinput = "";
            Console.WriteLine("");
            while (!exitFlag)
            {
                if (currentUserChoice < 1 || (currentUserChoice > 30 && currentUserChoice != 99))
                {
                    Console.WriteLine("ENTER 1 FOR Subscribe");
                    Console.WriteLine("ENTER 2 FOR Publish");
                    Console.WriteLine("ENTER 3 FOR Presence");
                    Console.WriteLine("ENTER 4 FOR Detailed History");
                    Console.WriteLine("ENTER 5 FOR Here_Now");
                    Console.WriteLine("ENTER 6 FOR Unsubscribe");
                    Console.WriteLine("ENTER 7 FOR Presence-Unsubscribe");
                    Console.WriteLine("ENTER 8 FOR Time");
                    Console.WriteLine("ENTER 9 FOR Disconnect/Reconnect existing Subscriber(s) (when internet is available)");
                    Console.WriteLine("ENTER 10 TO Disable Network Connection (no internet)");
                    Console.WriteLine("ENTER 11 TO Enable Network Connection (yes internet)");
                    Console.WriteLine("ENTER 12 FOR Grant Access");
                    Console.WriteLine("ENTER 13 FOR Audit Access");
                    Console.WriteLine("ENTER 14 FOR Revoke Access");
                    Console.WriteLine("ENTER 15 FOR Grant Access for Presence Channel");
                    Console.WriteLine("ENTER 16 FOR Audit Access for Presence Channel");
                    Console.WriteLine("ENTER 17 FOR Revoke Access for Presence Channel");
                    Console.WriteLine("ENTER 18 FOR Change/Update Auth Key (Current value = {0})", pubnub.AuthenticationKey);
                    Console.WriteLine("ENTER 19 TO Simulate Machine Sleep Mode");
                    Console.WriteLine("ENTER 20 TO Simulate Machine Awake Mode");
                    Console.WriteLine("ENTER 21 TO Set Presence Heartbeat (Current value = {0} sec)", pubnub.PresenceHeartbeat);
                    Console.WriteLine("ENTER 22 TO Set Presence Heartbeat Interval (Current value = {0} sec)", pubnub.PresenceHeartbeatInterval);
                    Console.WriteLine("Enter 23 TO Add/Modify Local User State");
                    Console.WriteLine("Enter 24 TO Delete Local User State");
                    Console.WriteLine("Enter 25 TO View/Get Local User State");
                    Console.WriteLine("Enter 26 TO Set User State");
                    Console.WriteLine("Enter 27 TO Get User State");
                    Console.WriteLine("Enter 28 FOR WhereNow");
                    Console.WriteLine("Enter 29 FOR GlobalHere_Now");
                    Console.WriteLine("Enter 30 TO change UUID. (Current value = {0})", pubnub.SessionUUID);
                    Console.WriteLine("ENTER 99 FOR EXIT OR QUIT");

                    userinput = Console.ReadLine();
                }
                switch (userinput)
                {
                    case "99":
                        exitFlag = true;
                        pubnub.EndPendingRequests();
                        break;
                    case "1":
                        Console.WriteLine("Enter CHANNEL name for subscribe. Use comma to enter multiple channels.");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}",channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running subscribe()");
                        pubnub.Subscribe<string>(channel, DisplaySubscribeReturnMessage, DisplaySubscribeConnectStatusMessage, DisplayErrorMessage);

                        break;
                    case "2":
                        Console.WriteLine("Enter CHANNEL name for publish.");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}",channel));
                        Console.ResetColor();

                        /* TO TEST SMALL TEXT PUBLISH ONLY */
                        Console.WriteLine("Enter the message for publish and press ENTER key to submit");
                        //string publishMsg = Console.ReadLine();

                        /* UNCOMMENT THE FOLLOWING CODE BLOCK TO TEST LARGE TEXT PUBLISH ONLY */
                        #region Code To Test Large Text Publish
                        ConsoleKeyInfo enteredKey;
                        StringBuilder publishBuilder = new StringBuilder();
                        do
                        {
                            enteredKey = Console.ReadKey(); //This logic is being used to capture > 2K input in console window
                            if (enteredKey.Key != ConsoleKey.Enter)
                            {
                                publishBuilder.Append(enteredKey.KeyChar);
                            }
                        } while (enteredKey.Key != ConsoleKey.Enter);
                        string publishMsg = publishBuilder.ToString();
                        #endregion
                        
                        Console.WriteLine("Running publish()");

                        double doubleData;
                        int intData;
                        if (int.TryParse(publishMsg, out intData)) //capture numeric data
                        {
                            pubnub.Publish<string>(channel, intData, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        else if (double.TryParse(publishMsg, out doubleData)) //capture numeric data
                        {
                            pubnub.Publish<string>(channel, doubleData, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        else
                        {
                            //check whether any numeric is sent in double quotes
                            if (publishMsg.IndexOf("\"") == 0 && publishMsg.LastIndexOf("\"") == publishMsg.Length - 1)
                            {
                                string strMsg = publishMsg.Substring(1, publishMsg.Length - 2);
                                if (int.TryParse(strMsg, out intData))
                                {
                                    pubnub.Publish<string>(channel, strMsg, DisplayReturnMessage, DisplayErrorMessage);
                                }
                                else if (double.TryParse(strMsg, out doubleData))
                                {
                                    pubnub.Publish<string>(channel, strMsg, DisplayReturnMessage, DisplayErrorMessage);
                                }
                                else
                                {
                                    pubnub.Publish<string>(channel, publishMsg, DisplayReturnMessage, DisplayErrorMessage);
                                }
                            }
                            else
                            {
                                pubnub.Publish<string>(channel, publishMsg, DisplayReturnMessage, DisplayErrorMessage);
                            }
                        }
                        break;
                    case "3":
                        Console.WriteLine("Enter CHANNEL name for presence. Use comma to enter multiple channels.");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Presence Channel = {0}",channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running presence()");
                        pubnub.Presence<string>(channel, DisplayPresenceReturnMessage, DisplayPresenceConnectStatusMessage, DisplayErrorMessage);

                        break;
                    case "4":
                        Console.WriteLine("Enter CHANNEL name for Detailed History");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}",channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running detailed history()");
                        pubnub.DetailedHistory<string>(channel, 100, DisplayReturnMessage, DisplayErrorMessage);
                        break;
                    case "5":
                        bool showUUID = true;
                        bool includeUserState = false;

                        Console.WriteLine("Enter CHANNEL name for HereNow");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}",channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Show UUID List? Y or N? Default is Y. Press N for No Else press ENTER");
                        string userChoiceShowUUID = Console.ReadLine();
                        if (userChoiceShowUUID.ToLower() == "n")
                        {
                            showUUID = false;
                        }
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Show UUID = {0}",showUUID));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Include User State? Y or N? Default is N. Press Y for Yes Else press ENTER");
                        string userChoiceIncludeUserState = Console.ReadLine();
                        if (userChoiceIncludeUserState.ToLower() == "y")
                        {
                            includeUserState = true;
                        }
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Include User State = {0}", includeUserState));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running Here_Now()");
                        pubnub.HereNow<string>(channel, showUUID, includeUserState, DisplayReturnMessage, DisplayErrorMessage);
                        break;
                    case "6":
                        Console.WriteLine("Enter CHANNEL name for Unsubscribe. Use comma to enter multiple channels.");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}",channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running unsubscribe()");
                        pubnub.Unsubscribe<string>(channel, DisplayReturnMessage, DisplaySubscribeConnectStatusMessage, DisplaySubscribeDisconnectStatusMessage, DisplayErrorMessage);
                        break;
                    case "7":
                        Console.WriteLine("Enter CHANNEL name for Presence Unsubscribe. Use comma to enter multiple channels.");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}",channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running presence-unsubscribe()");
                        pubnub.PresenceUnsubscribe<string>(channel, DisplayReturnMessage, DisplayPresenceConnectStatusMessage, DisplayPresenceDisconnectStatusMessage, DisplayErrorMessage);
                        break;
                    case "8":
                        Console.WriteLine("Running time()");
                        pubnub.Time<string>(DisplayReturnMessage, DisplayErrorMessage);
                        break;
                    case "9":
                        Console.WriteLine("Running Disconnect/auto-Reconnect Subscriber Request Connection");
                        pubnub.TerminateCurrentSubscriberRequest();
                        break;
                    case "10":
                        Console.WriteLine("Disabling Network Connection (no internet)");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Initiating Simulation of Internet non-availability");
                        Console.WriteLine("Until Choice=11 is entered, no operations will occur");
                        Console.WriteLine("NOTE: Publish from other pubnub clients can occur and those will be ");
                        Console.WriteLine("      captured upon choice=11 is entered provided resume on reconnect is enabled.");
                        Console.ResetColor();
                        pubnub.EnableSimulateNetworkFailForTestingOnly();
                        break;
                    case "11":
                        Console.WriteLine("Enabling Network Connection (yes internet)");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Stopping Simulation of Internet non-availability");  
                        Console.ResetColor();
                        pubnub.DisableSimulateNetworkFailForTestingOnly();
                        break;
                    case "12":
                        Console.WriteLine("Enter CHANNEL name for PAM Grant. For Presence, Select Option 15.");
                        channel = Console.ReadLine();
                        Console.WriteLine("Read Access? Enter Y for Yes (default), N for No.");
                        string readAccess = Console.ReadLine();
                        bool read = (readAccess.ToLower() == "n") ? false : true;
                        Console.WriteLine("Write Access? Enter Y for Yes (default), N for No.");
                        string writeAccess = Console.ReadLine();
                        bool write = (writeAccess.ToLower() == "n") ? false : true;
                        Console.WriteLine("How many minutes do you want to allow Grant Access? Enter the number of minutes.");
                        Console.WriteLine("Default = 1440 minutes (24 hours). Press ENTER now to accept default value.");
                        string grantTimeLimit = Console.ReadLine();
                        int grantTimeLimitInSeconds;
                        Int32.TryParse(grantTimeLimit, out grantTimeLimitInSeconds);
                        if (grantTimeLimitInSeconds == 0) grantTimeLimitInSeconds = 1440;

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}",channel));
                        Console.WriteLine(string.Format("Read Access = {0}", read.ToString()));
                        Console.WriteLine(string.Format("Write Access = {0}", write.ToString()));
                        Console.WriteLine(string.Format("Grant Access Time Limit = {0}", grantTimeLimitInSeconds.ToString()));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running PamGrant()");
                        pubnub.GrantAccess<string>(channel, read, write, grantTimeLimitInSeconds, DisplayReturnMessage, DisplayErrorMessage);
                        break;
                    case "13":
                        Console.WriteLine("Enter CHANNEL name for PAM Audit");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running PamAudit()");
                        pubnub.AuditAccess<string>(channel,DisplayReturnMessage, DisplayErrorMessage);
                        break;
                    case "14":
                        Console.WriteLine("Enter CHANNEL name for PAM Revoke");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running PamRevoke()");
                        pubnub.GrantAccess<string>(channel, false,false, DisplayReturnMessage, DisplayErrorMessage);
                        break;
                    case "15":
                        Console.WriteLine("Enter CHANNEL name for PAM Grant Presence.");
                        channel = Console.ReadLine();
                        Console.WriteLine("Read Access? Enter Y for Yes (default), N for No.");
                        string readPresenceAccess = Console.ReadLine();
                        bool readPresence = (readPresenceAccess.ToLower() == "n") ? false : true;
                        Console.WriteLine("Write Access? Enter Y for Yes (default), N for No.");
                        string writePresenceAccess = Console.ReadLine();
                        bool writePresence = (writePresenceAccess.ToLower() == "n") ? false : true;
                        Console.WriteLine("How many minutes do you want to allow Grant Presence Access? Enter the number of minutes.");
                        Console.WriteLine("Default = 1440 minutes (24 hours). Press ENTER now to accept default value.");
                        string grantPresenceTimeLimit = Console.ReadLine();
                        int grantPresenceTimeLimitInSeconds;
                        Int32.TryParse(grantPresenceTimeLimit, out grantPresenceTimeLimitInSeconds);
                        if (grantPresenceTimeLimitInSeconds == 0) grantPresenceTimeLimitInSeconds = 1440;

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.WriteLine(string.Format("Read Access = {0}", readPresence.ToString()));
                        Console.WriteLine(string.Format("Write Access = {0}", writePresence.ToString()));
                        Console.WriteLine(string.Format("Grant Access Time Limit = {0}", grantPresenceTimeLimitInSeconds.ToString()));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running PAM GrantPresenceAccess()");
                        pubnub.GrantPresenceAccess<string>(channel, readPresence, writePresence, grantPresenceTimeLimitInSeconds, DisplayReturnMessage, DisplayErrorMessage);
                        break;
                    case "16":
                        Console.WriteLine("Enter CHANNEL name for PAM Presence Audit");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running PAM Presence Audit()");
                        pubnub.AuditPresenceAccess<string>(channel, DisplayReturnMessage, DisplayErrorMessage);
                        break;
                    case "17":
                        Console.WriteLine("Enter CHANNEL name for PAM Presence Revoke");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running PAM Presence Revoke()");
                        pubnub.GrantPresenceAccess<string>(channel, false, false, DisplayReturnMessage, DisplayErrorMessage);
                        break;
                    case "18":
                        Console.WriteLine("Enter Auth Key. Use comma to enter multiple Auth Keys.");
                        Console.WriteLine("If you don't want to use Auth Key, Press ENTER Key");
                        authKey = Console.ReadLine();
                        pubnub.AuthenticationKey = authKey;

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Auth Key(s) = {0}", authKey));
                        Console.ResetColor();
                        Console.WriteLine();

                        break;
                    case "19":
                        Console.WriteLine("Enabling simulation of Sleep/Suspend Mode");  
                        pubnub.EnableMachineSleepModeForTestingOnly();
                        Console.ForegroundColor = ConsoleColor.Red;  
                        Console.WriteLine("Machine Sleep Mode simulation activated");
                        Console.ResetColor();  
                        break;
                    case "20":
                        Console.WriteLine("Disabling simulation of Sleep/Suspend Mode");  
                        pubnub.DisableMachineSleepModeForTestingOnly();
                        Console.ForegroundColor = ConsoleColor.Red;  
                        Console.WriteLine("Simulation going to awake mode");  
                        Console.ResetColor();  
                        break;
                    case "21":
                        Console.WriteLine("Enter Presence Heartbeat in seconds");
                        string pnHeartbeatInput = Console.ReadLine();
                        Int32.TryParse(pnHeartbeatInput, out presenceHeartbeat);
                        pubnub.PresenceHeartbeat = presenceHeartbeat;
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Presence Heartbeat = {0}", presenceHeartbeat));
                        Console.ResetColor();
                        break;
                    case "22":
                        Console.WriteLine("Enter Presence Heartbeat Interval in seconds");
                        Console.WriteLine("NOTE: Ensure that it is less than Presence Heartbeat-3 seconds");
                        string pnHeartbeatIntervalInput = Console.ReadLine();
                        Int32.TryParse(pnHeartbeatIntervalInput, out presenceHeartbeatInterval);
                        pubnub.PresenceHeartbeatInterval = presenceHeartbeatInterval;
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Presence Heartbeat Interval = {0}", presenceHeartbeatInterval));
                        Console.ResetColor();
                        break;
                    case "23":
                        Console.WriteLine("Enter channel name");
                        string userStateChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", userStateChannel));
                        Console.ResetColor();

                        Console.WriteLine("User State will be accepted as dictionary key:value pair");
                        while (true)
                        {
                            Console.WriteLine("Enter key. ");
                            string keyUserState = Console.ReadLine();
                            if (string.IsNullOrEmpty(keyUserState.Trim()))
                            {
                                Console.WriteLine("dictionary key:value pair entry completed.");
                                break;
                            }
                            Console.WriteLine("Enter value");
                            string valueUserState = Console.ReadLine();
                            int valueInt;
                            double valueDouble;
                            string currentUserState = "";
                            if (Int32.TryParse(valueUserState, out valueInt))
                            {
                                currentUserState = pubnub.SetLocalUserState(userStateChannel, keyUserState, valueInt);
                            }
                            else if (Double.TryParse(valueUserState, out valueDouble))
                            {
                                currentUserState = pubnub.SetLocalUserState(userStateChannel, keyUserState, valueDouble);
                            }
                            else
                            {
                                currentUserState = pubnub.SetLocalUserState(userStateChannel, keyUserState, valueUserState);
                            }

                            Console.ForegroundColor = ConsoleColor.Blue;
                            if (!string.IsNullOrEmpty(currentUserState))
                            {
                                Console.WriteLine("Current User State = {0}", currentUserState);
                            }
                            else
                            {
                                Console.Write("No User State Exists");
                            }
                            Console.ResetColor();
                        }
                        break;

                    case "24":
                        Console.WriteLine("Enter channel name");
                        string deleteUserStateChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", deleteUserStateChannel));
                        Console.ResetColor();

                        Console.WriteLine("Enter key of the local user state to be deleted");
                        string deleteKeyUserState = Console.ReadLine();
                        string currentUserStateAfterDelete = pubnub.SetLocalUserState(deleteUserStateChannel, deleteKeyUserState, null);

                        Console.ForegroundColor = ConsoleColor.Blue;
                        if (!string.IsNullOrEmpty(currentUserStateAfterDelete))
                        {
                            Console.WriteLine("Current User State = {0}", currentUserStateAfterDelete);
                        }
                        else
                        {
                            Console.Write("No User State Exists");
                        }
                        Console.ResetColor();
                        break;
                    case "25":
                        Console.WriteLine("Enter channel name");
                        string getUserStateChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", getUserStateChannel));
                        Console.ResetColor();

                        string currentUserStateView = pubnub.GetLocalUserState(getUserStateChannel);

                        Console.ForegroundColor = ConsoleColor.Blue;
                        if (!string.IsNullOrEmpty(currentUserStateView))
                        {
                            Console.WriteLine("Current User State = {0}", currentUserStateView);
                        }
                        else
                        {
                            Console.Write("No User State Exists");
                        }
                        Console.ResetColor();
                        break;
                    case "26":
                        Console.WriteLine("Enter channel name");
                        string setUserStateChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", setUserStateChannel));
                        Console.ResetColor();

                        Console.WriteLine("NOTE: Hopefully you added local user state.");
                        Console.WriteLine("If you want to consider already created local user state, press ENTER");
                        Console.WriteLine("Else enter user state in json format (Eg. {\"key1\":\"value1\",\"key2\":\"value2\"}");
                        string manualJsonUserState = Console.ReadLine();

                        string jsonUserState = "";
                        if (string.IsNullOrEmpty(manualJsonUserState))
                        {
                            jsonUserState = pubnub.GetLocalUserState(setUserStateChannel);
                        }
                        else
                        {
                            jsonUserState = manualJsonUserState;
                        }
                        if (jsonUserState == "" || jsonUserState == "{}")
                        {
                            Console.WriteLine("Invalid User State");
                            break;
                        }
                        Console.WriteLine("Enter UUID. (Optional. Press ENTER to skip it)");
                        string uuid = Console.ReadLine();
                        if (string.IsNullOrEmpty(uuid))
                        {
                            pubnub.SetUserState<string>(setUserStateChannel, jsonUserState, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        else
                        {
                            pubnub.SetUserState<string>(setUserStateChannel, uuid, jsonUserState, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        break;
                    case "27":
                        Console.WriteLine("Enter channel name");
                        string getUserStateChannel2 = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", getUserStateChannel2));
                        Console.ResetColor();

                        Console.WriteLine("Enter UUID. (Optional. Press ENTER to skip it)");
                        string uuid2 = Console.ReadLine();
                        if (string.IsNullOrEmpty(uuid2))
                        {
                            pubnub.GetUserState<string>(getUserStateChannel2, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        else
                        {
                            pubnub.GetUserState<string>(getUserStateChannel2, uuid2, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        break;
                    case "28":
                        Console.WriteLine("Enter uuid for WhereNow. To consider SessionUUID, just press ENTER");
                        string whereNowUuid = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("uuid = {0}", whereNowUuid));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running Where_Now()");
                        pubnub.WhereNow<string>(whereNowUuid, DisplayReturnMessage, DisplayErrorMessage);
                        break;
                    case "29":
                        bool globalHereNowShowUUID = true;
                        bool globalHereNowIncludeUserState = false;

                        Console.WriteLine("Show UUID List? Y or N? Default is Y. Press N for No Else press ENTER");
                        string userChoiceGlobalHereNowShowUUID = Console.ReadLine();
                        if (userChoiceGlobalHereNowShowUUID.ToLower() == "n")
                        {
                            globalHereNowShowUUID = false;
                        }
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Show UUID = {0}", globalHereNowShowUUID));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Include User State? Y or N? Default is N. Press Y for Yes Else press ENTER");
                        string userChoiceGlobalHereNowIncludeUserState = Console.ReadLine();
                        if (userChoiceGlobalHereNowIncludeUserState.ToLower() == "y")
                        {
                            globalHereNowIncludeUserState = true;
                        }
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Include User State = {0}", globalHereNowIncludeUserState));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running Global HereNow()");
                        pubnub.GlobalHereNow<string>(globalHereNowShowUUID, globalHereNowIncludeUserState,DisplayReturnMessage, DisplayErrorMessage);
                        break;
                    case "30":
                        Console.WriteLine("ENTER UUID.");
                        string sessionUUID = Console.ReadLine();
                        pubnub.ChangeUUID(sessionUUID);
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("UUID = {0}",pubnub.SessionUUID);
                        Console.ResetColor();
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("INVALID CHOICE. ENTER 99 FOR EXIT OR QUIT");
                        Console.ResetColor();
                        break;
                }
                if (!exitFlag)
                {
                    userinput = Console.ReadLine();
                    Int32.TryParse(userinput, out currentUserChoice);
                }
            }

            Console.WriteLine("\nPress any key to exit.\n\n");
            Console.ReadLine();

        }

        /// <summary>
        /// Callback method captures the response in JSON string format for all operations
        /// </summary>
        /// <param name="result"></param>
        static void DisplayReturnMessage(string result)
        {
            Console.WriteLine("REGULAR CALLBACK:");
            Console.WriteLine(result);
            Console.WriteLine();
        }

        /// <summary>
        /// Callback method captures the response in JSON string format for Subscribe
        /// </summary>
        /// <param name="result"></param>
        static void DisplaySubscribeReturnMessage(string result)
        {
            Console.WriteLine("SUBSCRIBE REGULAR CALLBACK:");
            Console.WriteLine(result);
            Console.WriteLine();
        }

        /// <summary>
        /// Callback method captures the response in JSON string format for Presence
        /// </summary>
        /// <param name="result"></param>
        static void DisplayPresenceReturnMessage(string result)
        {
            Console.WriteLine("PRESENCE REGULAR CALLBACK:");
            Console.WriteLine(result);
            Console.WriteLine();
        }

        /// <summary>
        /// Callback method to provide the connect status of Subscribe call
        /// </summary>
        /// <param name="result"></param>
        static void DisplaySubscribeConnectStatusMessage(string result)
        {
            Console.WriteLine("SUBSCRIBE CONNECT CALLBACK:");
            Console.WriteLine(result);
            Console.WriteLine();
        }

        /// <summary>
        /// Callback method to provide the connect status of Presence call
        /// </summary>
        /// <param name="result"></param>
        static void DisplayPresenceConnectStatusMessage(string result)
        {
            Console.WriteLine("PRESENCE CONNECT CALLBACK:");
            Console.WriteLine(result);
            Console.WriteLine();
        }

        static void DisplaySubscribeDisconnectStatusMessage(string result)
        {
            Console.WriteLine("SUBSCRIBE DISCONNECT CALLBACK:");
            Console.WriteLine(result);
            Console.WriteLine();
        }

        static void DisplayPresenceDisconnectStatusMessage(string result)
        {
            Console.WriteLine("PRESENCE DISCONNECT CALLBACK:");
            Console.WriteLine(result);
            Console.WriteLine();
        }

        /// <summary>
        /// Callback method for error messages
        /// </summary>
        /// <param name="result"></param>
        static void DisplayErrorMessage(PubnubClientError result)
        {
            Console.WriteLine();
            Console.WriteLine(result.Description);
            Console.WriteLine();

            switch (result.StatusCode)
            {
                case 103:
                    //Warning: Verify origin host name and internet connectivity
                    break;
                case 104:
                    //Critical: Verify your cipher key
                    break;
                case 106:
                    //Warning: Check network/internet connection
                    break;
                case 108:
                    //Warning: Check network/internet connection
                    break;
                case 109:
                    //Warning: No network/internet connection. Please check network/internet connection
                    break;
                case 110:
                    //Informational: Network/internet connection is back. Active subscriber/presence channels will be restored.
                    break;
                case 111:
                    //Informational: Duplicate channel subscription is not allowed. Internally Pubnub API removes the duplicates before processing.
                    break;
                case 112:
                    //Informational: Channel Already Subscribed/Presence Subscribed. Duplicate channel subscription not allowed
                    break;
                case 113:
                    //Informational: Channel Already Presence-Subscribed. Duplicate channel presence-subscription not allowed
                    break;
                case 114:
                    //Warning: Please verify your cipher key
                    break;
                case 115:
                    //Warning: Protocol Error. Please contact PubNub with error details.
                    break;
                case 116:
                    //Warning: ServerProtocolViolation. Please contact PubNub with error details.
                    break;
                case 117:
                    //Informational: Input contains invalid channel name
                    break;
                case 118:
                    //Informational: Channel not subscribed yet
                    break;
                case 119:
                    //Informational: Channel not subscribed for presence yet
                    break;
                case 120:
                    //Informational: Incomplete unsubscribe. Try again for unsubscribe.
                    break;
                case 121:
                    //Informational: Incomplete presence-unsubscribe. Try again for presence-unsubscribe.
                    break;
                case 122:
                    //Informational: Network/Internet connection not available. C# client retrying again to verify connection. No action is needed from your side.
                    break;
                case 123:
                    //Informational: During non-availability of network/internet, max retries for connection were attempted. So unsubscribed the channel.
                    break;
                case 124:
                    //Informational: During non-availability of network/internet, max retries for connection were attempted. So presence-unsubscribed the channel.
                    break;
                case 125:
                    //Informational: Publish operation timeout occured.
                    break;
                case 126:
                    //Informational: HereNow operation timeout occured
                    break;
                case 127:
                    //Informational: Detailed History operation timeout occured
                    break;
                case 128:
                    //Informational: Time operation timeout occured
                    break;
                case 4000:
                    //Warning: Message too large. Your message was not sent. Try to send this again smaller sized
                    break;
                case 4001:
                    //Warning: Bad Request. Please check the entered inputs or web request URL
                    break;
                case 4002:
                    //Warning: Invalid Key. Please verify the publish key
                    break;
                case 4010:
                    //Critical: Please provide correct subscribe key. This corresponds to a 401 on the server due to a bad sub key
                    break;
                case 4020:
                    // PAM is not enabled. Please contact PubNub support
                    break;
                case 4030:
                    //Warning: Not authorized. Check the permimissions on the channel. Also verify authentication key, to check access.
                    break;
                case 4031:
                    //Warning: Incorrect public key or secret key.
                    break;
                case 4140:
                    //Warning: Length of the URL is too long. Reduce the length by reducing subscription/presence channels or grant/revoke/audit channels/auth key list
                    break;
                case 5000:
                    //Critical: Internal Server Error. Unexpected error occured at PubNub Server. Please try again. If same problem persists, please contact PubNub support
                    break;
                case 5020:
                    //Critical: Bad Gateway. Unexpected error occured at PubNub Server. Please try again. If same problem persists, please contact PubNub support
                    break;
                case 5040:
                    //Critical: Gateway Timeout. No response from server due to PubNub server timeout. Please try again. If same problem persists, please contact PubNub support
                    break;
                case 0:
                    //Undocumented error. Please contact PubNub support with full error object details for further investigation
                    break;
                default:
                    break;
            }
            if (showErrorMessageSegments)
            {
                DisplayErrorMessageSegments(result);
                Console.WriteLine();
            }
        }


        static void DisplayErrorMessageSegments(PubnubClientError pubnubError)
        {
            // These are all the attributes you may be interested in logging, switchiing on etc:

            Console.WriteLine("<STATUS CODE>: {0}", pubnubError.StatusCode); // Unique ID of Error

            Console.WriteLine("<MESSAGE>: {0}", pubnubError.Message); // Message received from server/clent or from .NET exception

            Console.WriteLine("<SEVERITY>: {0}", pubnubError.Severity); // Info can be ignored, Warning and Error should be handled

            if (pubnubError.DetailedDotNetException != null)
            {
                Console.WriteLine(pubnubError.IsDotNetException); // Boolean flag to check .NET exception
                Console.WriteLine("<DETAILED DOT.NET EXCEPTION>: {0}", pubnubError.DetailedDotNetException.ToString()); // Full Details of .NET exception
            }
            Console.WriteLine("<MESSAGE SOURCE>: {0}", pubnubError.MessageSource); // Did this originate from Server or Client-side logic
            if (pubnubError.PubnubWebRequest != null)
            {
                //Captured Web Request details
                Console.WriteLine("<HTTP WEB REQUEST>: {0}", pubnubError.PubnubWebRequest.RequestUri.ToString()); 
                Console.WriteLine("<HTTP WEB REQUEST - HEADERS>: {0}", pubnubError.PubnubWebRequest.Headers.ToString()); 
            }
            if (pubnubError.PubnubWebResponse != null)
            {
                //Captured Web Response details
                Console.WriteLine("<HTTP WEB RESPONSE - HEADERS>: {0}", pubnubError.PubnubWebResponse.Headers.ToString());
            }
            Console.WriteLine("<DESCRIPTION>: {0}", pubnubError.Description); // Useful for logging and troubleshooting and support
            Console.WriteLine("<CHANNEL>: {0}", pubnubError.Channel); //Channel name(s) at the time of error
            Console.WriteLine("<DATETIME>: {0}", pubnubError.ErrorDateTimeGMT); //GMT time of error

        }
    }
}
