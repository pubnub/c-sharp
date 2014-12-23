using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            Console.WriteLine("Enter Pubnub Origin. Default Origin = pubsub.pubnub.com"); //TODO
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
                subscribeKey = "demo";
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
                publishKey = "demo";
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
                secretKey = "demo";
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
            string channelGroup = "";
            int currentUserChoice = 0;
            string userinput = "";
            Console.WriteLine("");
            while (!exitFlag)
            {
                if (currentUserChoice < 1 || (currentUserChoice > 40 && currentUserChoice != 99))
                {
                    Console.WriteLine("ENTER 1 FOR Subscribe channel/channelgroup");
                    Console.WriteLine("ENTER 2 FOR Publish");
                    Console.WriteLine("ENTER 3 FOR Presence channel/channelgroup");
                    Console.WriteLine("ENTER 4 FOR Detailed History");
                    Console.WriteLine("ENTER 5 FOR Here_Now");
                    Console.WriteLine("ENTER 6 FOR Unsubscribe");
                    Console.WriteLine("ENTER 7 FOR Presence-Unsubscribe");
                    Console.WriteLine("ENTER 8 FOR Time");
                    Console.WriteLine("ENTER 9 FOR Disconnect/Reconnect existing Subscriber(s) (when internet is available)");
                    Console.WriteLine("ENTER 10 TO Disable Network Connection (no internet)");
                    Console.WriteLine("ENTER 11 TO Enable Network Connection (yes internet)");
                    Console.WriteLine("ENTER 12 FOR Grant Access to channel/ChannelGroup");
                    Console.WriteLine("ENTER 13 FOR Audit Access to channel/ChannelGroup");
                    Console.WriteLine("ENTER 14 FOR Revoke Access to channel/ChannelGroup");
                    Console.WriteLine("ENTER 15 FOR Grant Access to Presence Channel/ChannelGroup");
                    Console.WriteLine("ENTER 16 FOR Audit Access to Presence Channel/ChannelGroup");
                    Console.WriteLine("ENTER 17 FOR Revoke Access to Presence Channel/ChannelGroup");
                    Console.WriteLine("ENTER 18 FOR Change/Update Auth Key (Current value = {0})", pubnub.AuthenticationKey);
                    Console.WriteLine("ENTER 19 TO Simulate Machine Sleep Mode");
                    Console.WriteLine("ENTER 20 TO Simulate Machine Awake Mode");
                    Console.WriteLine("ENTER 21 TO Set Presence Heartbeat (Current value = {0} sec)", pubnub.PresenceHeartbeat);
                    Console.WriteLine("ENTER 22 TO Set Presence Heartbeat Interval (Current value = {0} sec)", pubnub.PresenceHeartbeatInterval);
                    Console.WriteLine("Enter 23 TO Set User State by Add/Modify Key-Pair");
                    Console.WriteLine("Enter 24 TO Set User State by Deleting existing Key-Pair");
                    Console.WriteLine("Enter 25 TO Set User State with direct json string");
                    Console.WriteLine("Enter 26 TO Get User State");
                    Console.WriteLine("Enter 27 FOR WhereNow");
                    Console.WriteLine("Enter 28 FOR GlobalHere_Now");
                    Console.WriteLine("Enter 29 TO change UUID. (Current value = {0})", pubnub.SessionUUID);
                    Console.WriteLine("Enter 30 FOR Push - Register Device");
                    Console.WriteLine("Enter 31 FOR Push - Unregister Device");
                    Console.WriteLine("Enter 32 FOR Push - Remove Channel");
                    Console.WriteLine("Enter 33 FOR Push - Get Current Channels");
                    Console.WriteLine("Enter 34 FOR Push - Publish Toast message");
                    Console.WriteLine("Enter 35 FOR Push - Publish Flip Tile message");
                    Console.WriteLine("Enter 36 FOR Push - Publish Cycle Tile message");
                    Console.WriteLine("Enter 37 FOR Push - Publish Iconic Tile message");
                    Console.WriteLine("Enter 38 FOR Channel Group - Add channel(s)");
                    Console.WriteLine("Enter 39 FOR Channel Group - Remove channel/group/namespace");
                    Console.WriteLine("Enter 40 FOR Channel Group - Get channel(s)/namespace(s)");
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
                        Console.WriteLine("NOTE: If you want to consider only Channel Group(s), just hit ENTER");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}",channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Enter CHANNEL GROUP name for subscribe. Use comma to enter multiple channel groups.");
                        Console.WriteLine("To denote a namespaced CHANNEL GROUP, use the colon (:) character with the format namespace:channelgroup.");
                        Console.WriteLine("NOTE: If you want to consider only Channel(s), assuming you already entered , just hit ENTER");
                        channelGroup = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel Group= {0}", channelGroup));
                        Console.ResetColor();
                        Console.WriteLine();

                        if (channel.Length <= 0 && channelGroup.Length <= 0)
                        {
                            Console.WriteLine("To run subscribe(), atleast provide either channel name or channel group name or both");
                        }
                        else
                        {
                            Console.WriteLine("Running subscribe()");
                            pubnub.Subscribe<string>(channel, channelGroup, DisplaySubscribeReturnMessage, DisplaySubscribeConnectStatusMessage, DisplayErrorMessage);
                        }
                        break;
                    case "2":
                        Console.WriteLine("Enter CHANNEL name for publish.");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}",channel));
                        Console.ResetColor();

                        if (channel == "")
                        {
                            Console.WriteLine("Invalid CHANNEL name");
                            break;
                        }
                        Console.WriteLine("Store In History? Enter Y for Yes or N for No. To accept default(Y), just press ENTER");
                        string storeInHistory = Console.ReadLine();
                        bool store = true;
                        if (storeInHistory.ToLower() == "n")
                        {
                            store = false;
                        }

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Store In History = {0}", storeInHistory));
                        Console.ResetColor();




                        Console.WriteLine("Direct JSON String? Enter Y for Yes or N for No. To accept default(N), just press ENTER");
                        string directJson = Console.ReadLine();
                        bool jsonPublish = false;
                        if (directJson.ToLower() == "y")
                        {
                            jsonPublish = true;
                            pubnub.EnableJsonEncodingForPublish = false;
                        }
                        else
                        {
                            pubnub.EnableJsonEncodingForPublish = true;
                        }

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Direct JSON String = {0}", jsonPublish));
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
                            pubnub.Publish<string>(channel, intData, store, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        else if (double.TryParse(publishMsg, out doubleData)) //capture numeric data
                        {
                            pubnub.Publish<string>(channel, doubleData, store, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        else
                        {
                            //check whether any numeric is sent in double quotes
                            if (publishMsg.IndexOf("\"") == 0 && publishMsg.LastIndexOf("\"") == publishMsg.Length - 1)
                            {
                                string strMsg = publishMsg.Substring(1, publishMsg.Length - 2);
                                if (int.TryParse(strMsg, out intData))
                                {
                                    pubnub.Publish<string>(channel, strMsg, store, DisplayReturnMessage, DisplayErrorMessage);
                                }
                                else if (double.TryParse(strMsg, out doubleData))
                                {
                                    pubnub.Publish<string>(channel, strMsg, store, DisplayReturnMessage, DisplayErrorMessage);
                                }
                                else
                                {
                                    pubnub.Publish<string>(channel, publishMsg, store, DisplayReturnMessage, DisplayErrorMessage);
                                }
                            }
                            else
                            {
                                pubnub.Publish<string>(channel, publishMsg, store, DisplayReturnMessage, DisplayErrorMessage);
                            }
                        }
                        break;
                    case "3":
                        Console.WriteLine("Enter CHANNEL name for presence. Use comma to enter multiple channels.");
                        Console.WriteLine("NOTE: If you want to consider only Presence Channel Group(s), just hit ENTER");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Presence Channel = {0}",channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Enter CHANNEL GROUP name for presence. Use comma to enter multiple channel groups.");
                        Console.WriteLine("To denote a namespaced CHANNEL GROUP, use the colon (:) character with the format namespace:channelgroup.");
                        Console.WriteLine("NOTE: If you want to consider only Presence Channel(s), assuming you already entered, just hit ENTER");
                        channelGroup = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Presence Channel Group= {0}", channelGroup));
                        Console.ResetColor();
                        Console.WriteLine();

                        if (channel.Length <= 0 && channelGroup.Length <= 0)
                        {
                            Console.WriteLine("To run presence(), atleast provide either channel name or channel group name or both");
                        }
                        else
                        {
                            Console.WriteLine("Running presence()");
                            pubnub.Presence<string>(channel, channelGroup, DisplaySubscribeReturnMessage, DisplaySubscribeConnectStatusMessage, DisplayErrorMessage);
                        }
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
                        Console.WriteLine("NOTE: If you want to consider only Channel Group, just hit ENTER");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}",channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Enter channel group name");
                        Console.WriteLine("NOTE: If you want to consider only Channel, just hit ENTER");
                        channelGroup = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("ChannelGroup = {0}", channelGroup));
                        Console.ResetColor();

                        if (channel.Length <= 0 && channelGroup.Length <= 0)
                        {
                            Console.WriteLine("To run unsubscribe(), atleast provide either channel name or channel group name or both");
                        }
                        else
                        {
                            Console.WriteLine("Running unsubscribe()");
                            pubnub.Unsubscribe<string>(channel, channelGroup, DisplayReturnMessage, DisplaySubscribeConnectStatusMessage, DisplaySubscribeDisconnectStatusMessage, DisplayErrorMessage);
                        }
                        break;
                    case "7":
                        Console.WriteLine("Enter CHANNEL name for Presence Unsubscribe. Use comma to enter multiple channels.");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}",channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Enter channel group name for Presence Unsubscribe");
                        Console.WriteLine("NOTE: If you want to consider only Channel, just hit ENTER");
                        channelGroup = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("ChannelGroup = {0}", channelGroup));
                        Console.ResetColor();

                        if (channel.Length <= 0 && channelGroup.Length <= 0)
                        {
                            Console.WriteLine("To run presence-unsubscribe(), atleast provide either channel name or channel group name or both");
                        }
                        else
                        {
                            Console.WriteLine("Running presence-unsubscribe()");
                            pubnub.PresenceUnsubscribe<string>(channel, channelGroup, DisplayReturnMessage, DisplayPresenceConnectStatusMessage, DisplayPresenceDisconnectStatusMessage, DisplayErrorMessage);
                        }
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
                        Console.WriteLine("To enter CHANNEL GROUP name, just hit ENTER");
                        channel = Console.ReadLine();

                        if (channel.Trim().Length <= 0)
                        {
                            channel = "";
                            Console.WriteLine("Enter CHANNEL GROUP name for PAM Grant.");
                            channelGroup = Console.ReadLine();
                        }
                        if (channel.Trim().Length <= 0 && channelGroup.Trim().Length <= 0)
                        {
                            Console.WriteLine("Channel or ChannelGroup not provided. Please try again.");
                            break;
                        }
                        Console.WriteLine("Enter the auth_key for PAM Grant (optional)");
                        Console.WriteLine("Press Enter Key if there is no auth_key at this time.");
                        string authGrant = Console.ReadLine();
                        
                        Console.WriteLine("Read Access? Enter Y for Yes (default), N for No.");
                        string readAccess = Console.ReadLine();
                        bool read = (readAccess.ToLower() == "n") ? false : true;

                        bool write = false;
                        if (channel.Trim().Length > 0)
                        {
                            Console.WriteLine("Write Access? Enter Y for Yes (default), N for No.");
                            string writeAccess = Console.ReadLine();
                            write = (writeAccess.ToLower() == "n") ? false : true;
                        }

                        bool manage=false;
                        if (channel.Trim().Length <= 0)
                        {
                            Console.WriteLine("Manage Access? Enter Y for Yes (default), N for No.");
                            string manageAccess = Console.ReadLine();
                            manage = (manageAccess.ToLower() == "n") ? false : true;
                        }
                        Console.WriteLine("How many minutes do you want to allow Grant Access? Enter the number of minutes.");
                        Console.WriteLine("Default = 1440 minutes (24 hours). Press ENTER now to accept default value.");
                        int grantTimeLimitInMinutes;
                        string grantTimeLimit = Console.ReadLine();
                        if (string.IsNullOrEmpty(grantTimeLimit.Trim()))
                        {
                            grantTimeLimitInMinutes = 1440;
                        }
                        else
                        {
                            Int32.TryParse(grantTimeLimit, out grantTimeLimitInMinutes);
                            if (grantTimeLimitInMinutes < 0) grantTimeLimitInMinutes = 1440;
                        }

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}",channel));
                        Console.WriteLine(string.Format("ChannelGroup = {0}", channelGroup));
                        Console.WriteLine(string.Format("auth_key = {0}", authGrant));
                        Console.WriteLine(string.Format("Read Access = {0}", read.ToString()));
                        if (channel.Trim().Length > 0)
                        {
                            Console.WriteLine(string.Format("Write Access = {0}", write.ToString()));
                        }
                        if (channel.Trim().Length <= 0)
                        {
                            Console.WriteLine(string.Format("Manage Access = {0}", manage.ToString()));
                        }
                        Console.WriteLine(string.Format("Grant Access Time Limit = {0}", grantTimeLimitInMinutes.ToString()));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running PamGrant()");
                        if (channel.Trim().Length > 0)
                        {
                            pubnub.GrantAccess<string>(channel, authGrant, read, write, grantTimeLimitInMinutes, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        else
                        {
                            pubnub.ChannelGroupGrantAccess<string>(channelGroup, authGrant, read, manage, grantTimeLimitInMinutes, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        break;
                    case "13":
                        Console.WriteLine("Enter CHANNEL name for PAM Audit");
                        Console.WriteLine("To enter CHANNEL GROUP name, just hit ENTER");
                        channel = Console.ReadLine();

                        if (channel.Trim().Length <= 0)
                        {
                            Console.WriteLine("Enter CHANNEL GROUP name for PAM Audit.");
                            channelGroup = Console.ReadLine();
                        }
                        if (channel.Trim().Length <= 0 && channelGroup.Trim().Length <= 0)
                        {
                            Console.WriteLine("Channel or ChannelGroup not provided. Please try again.");
                            break;
                        }

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.ResetColor();
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("ChannelGroup = {0}", channelGroup));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Enter the auth_key for PAM Audit (optional)");
                        Console.WriteLine("Press Enter Key if there is no auth_key at this time.");
                        string authAudit = Console.ReadLine();
                        
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("auth_key = {0}", authAudit));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running PamAudit()");
                        if (channel.Trim().Length > 0)
                        {
                            pubnub.AuditAccess<string>(channel, authAudit, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        else
                        {
                            pubnub.ChannelGroupAuditAccess<string>(channelGroup, authAudit, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        break;
                    case "14":
                        Console.WriteLine("Enter CHANNEL name for PAM Revoke");
                        Console.WriteLine("To enter CHANNEL GROUP name, just hit ENTER");
                        channel = Console.ReadLine();
                        
                        if (channel.Trim().Length <= 0)
                        {
                            Console.WriteLine("Enter CHANNEL GROUP name for PAM Revoke.");
                            channelGroup = Console.ReadLine();
                        }
                        if (channel.Trim().Length <= 0 && channelGroup.Trim().Length <= 0)
                        {
                            channel = "";
                            Console.WriteLine("Channel or ChannelGroup not provided. Please try again.");
                            break;
                        }
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("ChannelGroup = {0}", channelGroup));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Enter the auth_key for PAM Revoke (optional)");
                        Console.WriteLine("Press Enter Key if there is no auth_key at this time.");
                        string authRevoke = Console.ReadLine();
                        
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("auth_key = {0}", authRevoke));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running PamRevoke()");
                        if (channel.Trim().Length > 0)
                        {
                            pubnub.GrantAccess<string>(channel, authRevoke, false, false, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        else
                        {
                            pubnub.ChannelGroupGrantAccess<string>(channelGroup, authRevoke, false, false, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        break;
                    case "15":
                        Console.WriteLine("Enter CHANNEL name for PAM Grant Presence.");
                        Console.WriteLine("To enter CHANNEL GROUP name, just hit ENTER");
                        channel = Console.ReadLine();

                        if (channel.Trim().Length <= 0)
                        {
                            channel = "";
                            Console.WriteLine("Enter CHANNEL GROUP name for PAM Grant Presence.");
                            channelGroup = Console.ReadLine();
                        }
                        if (channel.Trim().Length <= 0 && channelGroup.Trim().Length <= 0)
                        {
                            Console.WriteLine("Channel or ChannelGroup not provided. Please try again.");
                            break;
                        }
                        
                        Console.WriteLine("Enter the auth_key for PAM Grant Presence (optional)");
                        Console.WriteLine("Press Enter Key if there is no auth_key at this time.");
                        string authGrantPresence = Console.ReadLine();
                        
                        Console.WriteLine("Read Access? Enter Y for Yes (default), N for No.");
                        string readPresenceAccess = Console.ReadLine();
                        bool readPresence = (readPresenceAccess.ToLower() == "n") ? false : true;

                        bool writePresence = false;
                        if (channel.Trim().Length > 0)
                        {
                            Console.WriteLine("Write Access? Enter Y for Yes (default), N for No.");
                            string writePresenceAccess = Console.ReadLine();
                            writePresence = (writePresenceAccess.ToLower() == "n") ? false : true;
                        }

                        bool managePresence=false;
                        if (channel.Trim().Length <= 0)
                        {
                            Console.WriteLine("Manage Access? Enter Y for Yes (default), N for No.");
                            string manageAccess = Console.ReadLine();
                            managePresence = (manageAccess.ToLower() == "n") ? false : true;
                        }

                        Console.WriteLine("How many minutes do you want to allow Grant Presence Access? Enter the number of minutes.");
                        Console.WriteLine("Default = 1440 minutes (24 hours). Press ENTER now to accept default value.");
                        int grantPresenceTimeLimitInMinutes;
                        string grantPresenceTimeLimit = Console.ReadLine();
                        if (string.IsNullOrEmpty(grantPresenceTimeLimit.Trim()))
                        {
                            grantPresenceTimeLimitInMinutes = 1440;
                        }
                        else
                        {
                            Int32.TryParse(grantPresenceTimeLimit, out grantPresenceTimeLimitInMinutes);
                            if (grantPresenceTimeLimitInMinutes < 0) grantPresenceTimeLimitInMinutes = 1440;
                        }

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.WriteLine(string.Format("ChannelGroup = {0}", channelGroup));
                        Console.WriteLine(string.Format("auth_key = {0}", authGrantPresence));
                        Console.WriteLine(string.Format("Read Access = {0}", readPresence.ToString()));
                        if (channel.Trim().Length > 0)
                        {
                            Console.WriteLine(string.Format("Write Access = {0}", writePresence.ToString()));
                        }
                        if (channel.Trim().Length <= 0)
                        {
                            Console.WriteLine(string.Format("Manage Access = {0}", managePresence.ToString()));
                        }
                        Console.WriteLine(string.Format("Grant Access Time Limit = {0}", grantPresenceTimeLimitInMinutes.ToString()));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running PAM GrantPresenceAccess()");
                        if (channel.Trim().Length > 0)
                        {
                            pubnub.GrantPresenceAccess<string>(channel, authGrantPresence, readPresence, writePresence, grantPresenceTimeLimitInMinutes, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        else
                        {
                            pubnub.ChannelGroupGrantPresenceAccess<string>(channelGroup, authGrantPresence, readPresence, managePresence, grantPresenceTimeLimitInMinutes, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        break;
                    case "16":
                        Console.WriteLine("Enter CHANNEL name for PAM Presence Audit");
                        Console.WriteLine("To enter CHANNEL GROUP name, just hit ENTER");
                        channel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        if (channel.Trim().Length <= 0)
                        {
                            Console.WriteLine("Enter CHANNEL GROUP name for PAM Presence Audit.");
                            channelGroup = Console.ReadLine();
                            
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine(string.Format("ChannelGroup = {0}", channelGroup));
                            Console.ResetColor();
                            Console.WriteLine();

                        }
                        if (channel.Trim().Length <= 0 && channelGroup.Trim().Length <= 0)
                        {
                            Console.WriteLine("Channel or ChannelGroup not provided. Please try again.");
                            break;
                        }

                        Console.WriteLine("Enter the auth_key for PAM Presence Audit (optional)");
                        Console.WriteLine("Press Enter Key if there is no auth_key at this time.");
                        string authPresenceAudit = Console.ReadLine();
                        
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("auth_key = {0}", authPresenceAudit));
                        Console.ResetColor();
                        Console.WriteLine();


                        Console.WriteLine("Running PAM Presence Audit()");
                        if (channel.Trim().Length > 0)
                        {
                            pubnub.AuditPresenceAccess<string>(channel, authPresenceAudit, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        else
                        {
                            pubnub.ChannelGroupAuditPresenceAccess<string>(channelGroup, authPresenceAudit, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        break;
                    case "17":
                        Console.WriteLine("Enter CHANNEL name for PAM Presence Revoke");
                        Console.WriteLine("To enter CHANNEL GROUP name, just hit ENTER");
                        channel = Console.ReadLine();

                        if (channel.Trim().Length <= 0)
                        {
                            Console.WriteLine("Enter CHANNEL GROUP name for PAM Revoke.");
                            channelGroup = Console.ReadLine();
                        }
                        if (channel.Trim().Length <= 0 && channelGroup.Trim().Length <= 0)
                        {
                            channel = "";
                            Console.WriteLine("Channel or ChannelGroup not provided. Please try again.");
                            break;
                        }
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("ChannelGroup = {0}", channelGroup));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Enter the auth_key for PAM Presence Revoke (optional)");
                        Console.WriteLine("Press Enter Key if there is no auth_key at this time.");
                        string authPresenceRevoke = Console.ReadLine();
                        
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("auth_key = {0}", authPresenceRevoke));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running PAM Presence Revoke()");
                        if (channel.Trim().Length > 0)
                        {
                            pubnub.GrantPresenceAccess<string>(channel, authPresenceRevoke, false, false, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        else
                        {
                            pubnub.ChannelGroupGrantPresenceAccess<string>(channelGroup, authPresenceRevoke, false, false, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        break;
                    case "18":
                        Console.WriteLine("Enter Auth Key (applies to all subscribed channels).");
                        Console.WriteLine("If you don't want to use Auth Key, Press ENTER Key");
                        authKey = Console.ReadLine();
                        pubnub.AuthenticationKey = authKey;

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Auth Key = {0}", authKey));
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
                        Console.WriteLine("NOTE: If you want to consider only Channel Group, just hit ENTER");
                        string userStateChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", userStateChannel));
                        Console.ResetColor();

                        Console.WriteLine("Enter channel group name");
                        Console.WriteLine("NOTE: If you want to consider only Channel, just hit ENTER");
                        string userStateChannelGroup = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("ChannelGroup = {0}", userStateChannelGroup));
                        Console.ResetColor();

                        Console.WriteLine("User State will be accepted as dictionary key:value pair");

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
                        //string currentUserState = "";
                        if (Int32.TryParse(valueUserState, out valueInt))
                        {
                            pubnub.SetUserState<string>(userStateChannel, userStateChannelGroup, new KeyValuePair<string, object>(keyUserState, valueInt), DisplayReturnMessage, DisplayErrorMessage);
                        }
                        else if (Double.TryParse(valueUserState, out valueDouble))
                        {
                            pubnub.SetUserState<string>(userStateChannel, userStateChannelGroup, new KeyValuePair<string, object>(keyUserState, valueDouble), DisplayReturnMessage, DisplayErrorMessage);
                        }
                        else
                        {
                            pubnub.SetUserState<string>(userStateChannel, userStateChannelGroup, new KeyValuePair<string, object>(keyUserState, valueUserState), DisplayReturnMessage, DisplayErrorMessage);
                        }

                        break;
                    case "24":
                        Console.WriteLine("Enter channel name");
                        Console.WriteLine("NOTE: If you want to consider only Channel Group, just hit ENTER");
                        string deleteChannelUserState = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", deleteChannelUserState));
                        Console.ResetColor();

                        Console.WriteLine("Enter channel group name");
                        Console.WriteLine("NOTE: If you want to consider only Channel, just hit ENTER");
                        string deleteChannelGroupUserState = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("ChannelGroup = {0}", deleteChannelGroupUserState));
                        Console.ResetColor();

                        Console.WriteLine("Enter key of the User State Key-Value pair to be deleted");
                        string deleteKeyUserState = Console.ReadLine();
                        pubnub.SetUserState<string>(deleteChannelUserState, deleteChannelGroupUserState, new KeyValuePair<string, object>(deleteKeyUserState, null), DisplayReturnMessage, DisplayErrorMessage);

                        break;
                    case "25":
                        Console.WriteLine("Enter channel name");
                        Console.WriteLine("NOTE: If you want to consider only Channel Group, just hit ENTER");
                        string setUserStateChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", setUserStateChannel));
                        Console.ResetColor();

                        Console.WriteLine("Enter channel group name");
                        Console.WriteLine("NOTE: If you want to consider only Channel, just hit ENTER");
                        string setUserStateChannelGroup = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("ChannelGroup = {0}", setUserStateChannelGroup));
                        Console.ResetColor();

                        Console.WriteLine("Enter user state in json format (Eg. {\"key1\":\"value1\",\"key2\":\"value2\"}");
                        string jsonUserState = Console.ReadLine();

                        if (jsonUserState.Trim() == "")
                        {
                            Console.WriteLine("Invalid User State");
                            break;
                        }
                        Console.WriteLine("Enter UUID. (Optional. Press ENTER to skip it)");
                        string uuid = Console.ReadLine();
                        if (string.IsNullOrEmpty(uuid))
                        {
                            pubnub.SetUserState<string>(setUserStateChannel, setUserStateChannelGroup, jsonUserState, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        else
                        {
                            pubnub.SetUserState<string>(setUserStateChannel, setUserStateChannelGroup, uuid, jsonUserState, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        break;
                    case "26":
                        Console.WriteLine("Enter channel name");
                        Console.WriteLine("NOTE: If you want to consider only Channel Group, just hit ENTER");
                        string getUserStateChannel2 = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", getUserStateChannel2));
                        Console.ResetColor();

                        Console.WriteLine("Enter channel group name");
                        Console.WriteLine("NOTE: If you want to consider only Channel, just hit ENTER");
                        string setUserStateChannelGroup2 = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("ChannelGroup = {0}", setUserStateChannelGroup2));
                        Console.ResetColor();

                        Console.WriteLine("Enter UUID. (Optional. Press ENTER to skip it)");
                        string uuid2 = Console.ReadLine();
                        if (string.IsNullOrEmpty(uuid2))
                        {
                            pubnub.GetUserState<string>(getUserStateChannel2, setUserStateChannelGroup2, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        else
                        {
                            pubnub.GetUserState<string>(getUserStateChannel2, setUserStateChannelGroup2, uuid2, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        break;
                    case "27":
                        Console.WriteLine("Enter uuid for WhereNow. To consider SessionUUID, just press ENTER");
                        string whereNowUuid = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("uuid = {0}", whereNowUuid));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running Where_Now()");
                        pubnub.WhereNow<string>(whereNowUuid, DisplayReturnMessage, DisplayErrorMessage);
                        break;
                    case "28":
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
                    case "29":
                        Console.WriteLine("ENTER UUID.");
                        string sessionUUID = Console.ReadLine();
                        pubnub.ChangeUUID(sessionUUID);
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("UUID = {0}",pubnub.SessionUUID);
                        Console.ResetColor();
                        break;
                    case "30":
                        Console.WriteLine("Enter channel name");
                        string pushRegisterChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", pushRegisterChannel));
                        Console.ResetColor();

                        Console.WriteLine("Enter Push Token for MPNS");
                        string pushToken = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Push Token = {0}", pushToken));
                        Console.ResetColor();

                        Console.WriteLine("Running RegisterDeviceForPush()");
                        pubnub.RegisterDeviceForPush<string>(pushRegisterChannel, PushTypeService.MPNS, pushToken, DisplayReturnMessage, DisplayErrorMessage);
                        break;
                    case "31":
                        Console.WriteLine("Enter Push Token for MPNS");
                        string unRegisterPushToken = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Push Token = {0}", unRegisterPushToken));
                        Console.ResetColor();

                        Console.WriteLine("Running UnregisterDeviceForPush()");
                        pubnub.UnregisterDeviceForPush<string>(PushTypeService.MPNS, unRegisterPushToken, DisplayReturnMessage, DisplayErrorMessage);
                        break;
                    case "32":
                        Console.WriteLine("Enter channel name");
                        string pushRemoveChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", pushRemoveChannel));
                        Console.ResetColor();

                        Console.WriteLine("Enter Push Token for MPNS");
                        string pushTokenRemove = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Push Token = {0}", pushTokenRemove));
                        Console.ResetColor();

                        Console.WriteLine("Running RegisterDeviceForPush()");
                        pubnub.RemoveChannelForDevicePush<string>(pushRemoveChannel, PushTypeService.MPNS, pushTokenRemove, DisplayReturnMessage, DisplayErrorMessage);
                        break;
                    case "33":
                        Console.WriteLine("Enter Push Token for MPNS");
                        string pushTokenGetChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Push Token = {0}", pushTokenGetChannel));
                        Console.ResetColor();

                        Console.WriteLine("Running RegisterDeviceForPush()");
                        pubnub.GetChannelsForDevicePush<string>(PushTypeService.MPNS, pushTokenGetChannel, DisplayReturnMessage, DisplayErrorMessage);
                        break;
                    case "34": 
                        //Toast message publish
                        Console.WriteLine("Enter channel name");
                        string toastChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", toastChannel));
                        Console.ResetColor();

                        Console.WriteLine("Enter title for Toast");
                        string text1 = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Text1 = {0}", text1));
                        Console.ResetColor();

                        MpnsToastNotification toast = new MpnsToastNotification();
                        toast.text1 = text1;
                        Dictionary<string, object> dicToast = new Dictionary<string, object>();
                        dicToast.Add("pn_mpns", toast);
                        pubnub.EnableDebugForPushPublish = true;

                        Console.WriteLine("Running Publish for Toast");
                        pubnub.Publish<string>(toastChannel, dicToast, DisplayReturnMessage, DisplayErrorMessage);
                        break;
                    case "35":
                        //Flip Tile message publish
                        Console.WriteLine("Enter channel name");
                        string flipTileChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", flipTileChannel));
                        Console.ResetColor();

                        Console.WriteLine("Enter front title for Flip Tile");
                        string flipFrontTitle = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Front Title = {0}", flipFrontTitle));
                        Console.ResetColor();

                        Console.WriteLine("Enter back title for Flip Tile");
                        string flipBackTitle = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Back Title = {0}", flipBackTitle));
                        Console.ResetColor();

                        Console.WriteLine("Enter back content for Flip Tile");
                        string flipBackContent = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Back Content = {0}", flipBackContent));
                        Console.ResetColor();

                        Console.WriteLine("Enter numeric count for Flip Tile. Invalid entry will be set to null");
                        string stringFlipCount = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Count = {0}", stringFlipCount));
                        Console.ResetColor();
                        int? flipTileCount  = null;
                        if (!string.IsNullOrEmpty(stringFlipCount) && stringFlipCount.Trim().Length > 0)
                        {
                            int outValue;
                            flipTileCount = int.TryParse(stringFlipCount, out outValue) ? (int?)outValue : null;
                        }

                        Console.WriteLine("Enter background image path with fully qualified URI or device local relative Path for Flip Tile");
                        string imageBackground = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Small Background Image = {0}", imageBackground));
                        Console.ResetColor();

                        Console.WriteLine("Enter Back background image path with fully qualified URI or device local relative Path for Flip Tile");
                        string imageBackBackground = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Small Background Image = {0}", imageBackBackground));
                        Console.ResetColor();

                        pubnub.PushRemoteImageDomainUri.Add(new Uri("http://cdn.flaticon.com"));

                        MpnsFlipTileNotification flipTile = new MpnsFlipTileNotification();
                        flipTile.title = flipFrontTitle;
                        flipTile.count = flipTileCount;
                        flipTile.back_title = flipBackTitle;
                        flipTile.back_content = flipBackContent;
                        flipTile.background_image = imageBackground;
                        flipTile.back_background_image = imageBackBackground;
                        Dictionary<string, object> dicFlipTile = new Dictionary<string, object>();
                        dicFlipTile.Add("pn_mpns", flipTile);

                        pubnub.EnableDebugForPushPublish = true;
                        pubnub.Publish<string>(flipTileChannel, dicFlipTile, DisplayReturnMessage, DisplayErrorMessage);
                        break;
                    case "36":
                        //Cycle Tile message publish
                        Console.WriteLine("Enter channel name");
                        string cycleTileChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", cycleTileChannel));
                        Console.ResetColor();

                        Console.WriteLine("Enter front title for Cycle Tile");
                        string cycleFrontTitle = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Front Title = {0}", cycleFrontTitle));
                        Console.ResetColor();

                        Console.WriteLine("Enter numeric count for Cycle Tile. Invalid entry will be set to null");
                        string stringCycleCount = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Count = {0}", stringCycleCount));
                        Console.ResetColor();
                        int? cycleTileCount = null;
                        if (!string.IsNullOrEmpty(stringCycleCount) && stringCycleCount.Trim().Length > 0)
                        {
                            int outValue;
                            cycleTileCount = int.TryParse(stringCycleCount, out outValue) ? (int?)outValue : null;
                        }

                        Console.WriteLine("Enter image path (fully qualified URI/device local Path) for Cycle Tile");
                        Console.WriteLine("Multiple image paths can be entered with comma delimiter");
                        string imageCycleTile = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Image Path(s) = {0}", imageCycleTile));
                        Console.ResetColor();

                        MpnsCycleTileNotification cycleTile = new MpnsCycleTileNotification();
                        cycleTile.title = cycleFrontTitle;
                        cycleTile.count = cycleTileCount;
                        cycleTile.images = imageCycleTile.Split(','); // new string[] { imageCycleTile };
                        Dictionary<string, object> dicCycleTile = new Dictionary<string, object>();
                        dicCycleTile.Add("pn_mpns", cycleTile);

                        pubnub.EnableDebugForPushPublish = true;
                        pubnub.Publish<string>(cycleTileChannel, dicCycleTile, DisplayReturnMessage, DisplayErrorMessage);
                        break;
                    case "37":
                        //Iconic Tile message publish
                        Console.WriteLine("Enter channel name");
                        string iconicTileChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", iconicTileChannel));
                        Console.ResetColor();

                        Console.WriteLine("Enter front title for Iconic Tile");
                        string iconicFrontTitle = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Front Title = {0}", iconicFrontTitle));
                        Console.ResetColor();

                        Console.WriteLine("Enter numeric count for Iconic Tile. Invalid entry will be set to null");
                        string stringIconicCount = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Count = {0}", stringIconicCount));
                        Console.ResetColor();
                        int? iconicTileCount = null;
                        if (!string.IsNullOrEmpty(stringIconicCount) && stringIconicCount.Trim().Length > 0)
                        {
                            int outValue;
                            iconicTileCount = int.TryParse(stringIconicCount, out outValue) ? (int?)outValue : null;
                        }

                        Console.WriteLine("Enter Content1 for Iconic Tile.");
                        string iconicTileContent1 = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("iconicTileContent1 = {0}", iconicTileContent1));
                        Console.ResetColor();

                        MpnsIconicTileNotification iconicTile = new MpnsIconicTileNotification();
                        iconicTile.title = iconicFrontTitle;
                        iconicTile.count = iconicTileCount;
                        iconicTile.wide_content_1 = iconicTileContent1;
                        Dictionary<string, object> dicIconicTile = new Dictionary<string, object>();
                        dicIconicTile.Add("pn_mpns", iconicTile);

                        pubnub.EnableDebugForPushPublish = true;
                        pubnub.Publish<string>(iconicTileChannel, dicIconicTile, DisplayReturnMessage, DisplayErrorMessage);
                        break;
                    case "38":
                        Console.WriteLine("Enter namespace");
                        string addChannelGroupNamespace = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("namespace = {0}", addChannelGroupNamespace));
                        Console.ResetColor();
                        
                        Console.WriteLine("Enter channel group name");
                        string addChannelGroupName = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("channel group name = {0}", addChannelGroupName));
                        Console.ResetColor();
                        

                        Console.WriteLine("Enter CHANNEL name. Use comma to enter multiple channels.");
                        channel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}",channel));
                        Console.ResetColor();
                        Console.WriteLine();
                        pubnub.AddChannelsToChannelGroup<string>(channel.Split(','), addChannelGroupNamespace, addChannelGroupName, DisplayReturnMessage, DisplayErrorMessage);
                        break;
                    case "39":
                        Console.WriteLine("Enter namespace");
                        string removeChannelGroupNamespace = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("namespace = {0}", removeChannelGroupNamespace));
                        Console.ResetColor();

                        if (removeChannelGroupNamespace.Trim().Length > 0)
                        {
                            Console.WriteLine("Do you want to remove the namespace and all its group names and all its channels? Default is No. Enter Y for Yes, Else just hit ENTER key");
                            string removeExistingNamespace = Console.ReadLine();
                            if (removeExistingNamespace.ToLower() == "y")
                            {
                                pubnub.RemoveChannelGroupNameSpace<string>(removeChannelGroupNamespace, DisplayReturnMessage, DisplayErrorMessage);
                                break;
                            }
                        }

                        Console.WriteLine("Enter channel group name");
                        string removeChannelGroupName = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("channel group name = {0}", removeChannelGroupName));
                        Console.ResetColor();

                        if (removeChannelGroupName.Trim().Length <= 0)
                        {
                            Console.WriteLine("Channel group not provided. Try again");
                            break;
                        }
                        Console.WriteLine("Do you want to remove the channel group and all its channels? Default is No. Enter Y for Yes, Else just hit ENTER key");
                        string removeExistingGroup = Console.ReadLine();
                        if (removeExistingGroup.ToLower() == "y")
                        {
                            pubnub.RemoveChannelGroup<string>(removeChannelGroupNamespace, removeChannelGroupName, DisplayReturnMessage, DisplayErrorMessage);
                            break;
                        }
                        
                        Console.WriteLine("Enter CHANNEL name. Use comma to enter multiple channels.");
                        channel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}",channel));
                        Console.ResetColor();
                        Console.WriteLine();
                        pubnub.RemoveChannelsFromChannelGroup<string>(channel.Split(','), removeChannelGroupNamespace, removeChannelGroupName, DisplayReturnMessage, DisplayErrorMessage);
                        break;
                    case "40":
                        Console.WriteLine("Do you want to get all existing namespaces? Default is No. Enter Y for Yes, Else just hit ENTER key");
                        string getExistingNamespace = Console.ReadLine();
                        if (getExistingNamespace.ToLower() == "y")
                        {
                            pubnub.GetAllChannelGroupNamespaces<string>(DisplayReturnMessage, DisplayErrorMessage);
                            break;
                        }

                        Console.WriteLine("Enter namespace");
                        string channelGroupNamespace = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("namespace = {0}", channelGroupNamespace));
                        Console.ResetColor();

                        if (channelGroupNamespace.Trim().Length > 0)
                        {
                            Console.WriteLine("Do you want to get all existing channel group names for the provided namespace? Default is No. Enter Y for Yes, Else just hit ENTER key");
                            string getExistingGroupNames = Console.ReadLine();
                            if (getExistingGroupNames.ToLower() == "y")
                            {
                                pubnub.GetAllChannelGroups<string>(channelGroupNamespace, DisplayReturnMessage, DisplayErrorMessage);
                                break;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Do you want to get all existing channel group names? Default is No. Enter Y for Yes, Else just hit ENTER key");
                            string getExistingGroupNames = Console.ReadLine();
                            if (getExistingGroupNames.ToLower() == "y")
                            {
                                pubnub.GetAllChannelGroups<string>(DisplayReturnMessage, DisplayErrorMessage);
                                break;
                            }
                        }
                        
                        Console.WriteLine("Enter channel group name");
                        string channelGroupName = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("channel group name = {0}", channelGroupName));
                        Console.ResetColor();

                        pubnub.GetChannelsForChannelGroup<string>(channelGroupNamespace, channelGroupName, DisplayReturnMessage, DisplayErrorMessage);
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
