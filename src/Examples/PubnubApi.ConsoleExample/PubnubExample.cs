using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Globalization;
using System.Diagnostics;

using PubnubApi;
namespace PubnubApiDemo
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

        public class PlatformPubnubLog : IPubnubLog
        {
            private string logFilePath = "";

            public PlatformPubnubLog()
            {
                // Get folder path may vary based on environment
                string folder = System.IO.Directory.GetCurrentDirectory(); //For console
                //string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); // For iOS
                System.Diagnostics.Debug.WriteLine(folder);
                logFilePath = System.IO.Path.Combine(folder, "pubnubmessaging.log");
                Trace.Listeners.Add(new TextWriterTraceListener(logFilePath));
            }

            public void WriteToLog(string log)
            {
                Trace.WriteLine(log);
                Trace.Flush();
            }
        }


        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            //Console.WriteLine(e.ExceptionObject.ToString());
            Console.WriteLine("Unhandled exception occured inside Pubnub C# API. Exiting the application. Please try again.");
            Environment.Exit(1);
        }

        static public void Main()
        {
            PNConfiguration config = new PNConfiguration();
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            Console.WriteLine("HINT: TO TEST RE-CONNECT AND CATCH-UP,");
            Console.WriteLine("      DISCONNECT YOUR MACHINE FROM NETWORK/INTERNET AND ");
            Console.WriteLine("      RE-CONNECT YOUR MACHINE AFTER SOMETIME.");
            Console.WriteLine();
            Console.WriteLine("      IF NO NETWORK BEFORE MAX RE-TRY CONNECT,");
            Console.WriteLine("      NETWORK ERROR MESSAGE WILL BE SENT");
            Console.WriteLine();

            Console.WriteLine("Enter Pubnub Origin. Default Origin = ps.pndsn.com"); //TODO
            Console.WriteLine("If you want to accept default value, press ENTER.");
            string origin = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (origin.Trim() == "")
            {
                origin = "ps.pndsn.com";
                Console.WriteLine("Default Origin selected");
            }
            else
            {
                Console.WriteLine("Pubnub Origin Provided");
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("Enable SSL? ENTER Y for Yes, else N. (Default N)");
            string enableSSL = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (enableSSL.Trim().ToLower() == "y")
            {
                Console.WriteLine("SSL Enabled");
            }
            else if (enableSSL.Trim().ToLower() == "n")
            {
                Console.WriteLine("SSL NOT Enabled");
            }
            else
            {
                enableSSL = "N";
                Console.WriteLine("SSL Disabled (default)");
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

            Console.WriteLine("Use Custom Session UUID? ENTER Y for Yes, else N");
            string enableCustomUUID = Console.ReadLine();
            if (enableCustomUUID.Trim().ToLower() == "y")
            {
                Console.WriteLine("ENTER Session UUID.");
                string sessionUUID = Console.ReadLine();
                if (string.IsNullOrEmpty(sessionUUID) || sessionUUID.Trim().Length == 0)
                {
                    Console.WriteLine("Invalid UUID. Default value will be set.");
                }
                else
                {
                    config.Uuid = sessionUUID;
                }
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

            Console.WriteLine("Enter Auth Key. If you don't want to use Auth Key, Press ENTER Key");
            authKey = Console.ReadLine();
            config.AuthKey = authKey;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(string.Format("Auth Key = {0}", authKey));
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("Enable Internal Logging? Enter Y for Yes, Else N for No.");
            Console.WriteLine("Default = Y  ");
            string enableLoggingString = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (enableLoggingString.Trim().ToLower() == "n")
            {
                config.LogVerbosity = PNLogVerbosity.NONE;
                config.PubnubLog = null;
                Console.WriteLine("Disabled internal logging");
            }
            else
            {
                config.LogVerbosity = PNLogVerbosity.BODY;
                config.PubnubLog = new PlatformPubnubLog();
                Console.WriteLine("Enabled internal logging");
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
                Console.WriteLine("Subscribe Timeout = {0}", subscribeTimeout);
                config.SubscribeTimeout = subscribeTimeout;
            }
            else
            {
                Console.WriteLine("Subscribe Timeout = {0} (default)", config.SubscribeTimeout);
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
                config.NonSubscribeRequestTimeout = nonSubscribeTimeout;
            }
            else
            {
                Console.WriteLine("Non Subscribe Timeout = {0} (default)", config.NonSubscribeRequestTimeout);
            }
            Console.ResetColor();
            Console.WriteLine();

            /*  */
            Console.WriteLine("Presence Heartbeat Timeout disabled by default. (default). Enter the value to enable, else press ENTER");
            string presenceHeartbeatTimeoutEntry = Console.ReadLine();
            int presenceHeartbeatTimeout;
            Int32.TryParse(presenceHeartbeatTimeoutEntry, out presenceHeartbeatTimeout);
            Console.ForegroundColor = ConsoleColor.Blue;
            if (presenceHeartbeatTimeout > 0)
            {
                Console.WriteLine("Presence Timeout = {0}", presenceHeartbeatTimeout);
                config.PresenceTimeout = presenceHeartbeatTimeout;
            }
            config.HeartbeatNotificationOption = PNHeartbeatNotificationOption.Failures;

            Console.ResetColor();
            Console.WriteLine();

            config.Origin = origin;

            config.Secure = (enableSSL.Trim().ToLower() == "y") ? true : false;
            config.CipherKey = cipherKey;
            config.SubscribeKey = subscribeKey;
            config.PublishKey = publishKey;
            config.SecretKey = secretKey;
            config.ReconnectionPolicy = PNReconnectionPolicy.LINEAR;
            //config.UseClassicHttpWebRequest = true;
            //config.FilterExpression = "uuid == '" + config.Uuid +  "'";
            config.EnableTelemetry = false;

            pubnub = new Pubnub(config);
            pubnub.AddListener(new SubscribeCallbackExt(
                (o, m) => { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m)); },
                (o, p) => { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(p)); },
                (o, s) => {
                    Console.WriteLine("{0} {1} {2}", s.Operation, s.Category, s.StatusCode);
                }));

            bool exitFlag = false;
            string channel = "";
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
                    Console.WriteLine("ENTER 3 FOR History");
                    Console.WriteLine("ENTER 4 FOR Here_Now");
                    Console.WriteLine("ENTER 5 FOR Unsubscribe");
                    Console.WriteLine("ENTER 6 FOR Time");
                    Console.WriteLine("ENTER 7 FOR Disconnect/Reconnect existing Subscriber(s) (when internet is available)");
                    Console.WriteLine("ENTER 8 FOR Grant Access to channel/ChannelGroup");
                    Console.WriteLine("ENTER 9 FOR Audit Access to channel/ChannelGroup");
                    Console.WriteLine("ENTER 10 FOR Revoke Access to channel/ChannelGroup");
                    Console.WriteLine("ENTER 11 TO Simulate Machine Sleep Mode");
                    Console.WriteLine("ENTER 12 TO Simulate Machine Awake Mode");
                    Console.WriteLine("Enter 13 TO Set User State by Add/Modify Key-Pair");
                    Console.WriteLine("Enter 14 TO Set User State by Deleting existing Key-Pair");
                    Console.WriteLine("Enter 15 TO Get User State");
                    Console.WriteLine("Enter 16 FOR WhereNow");
                    Console.WriteLine("Enter 17 TO change UUID. (Current value = {0})", config.Uuid);
                    Console.WriteLine("Enter 18 FOR Disconnect");
                    Console.WriteLine("Enter 19 FOR Reconnect");
                    Console.WriteLine("Enter 20 FOR UnsubscribeAll");
                    Console.WriteLine("Enter 21 FOR GetSubscribeChannels");
                    Console.WriteLine("Enter 22 FOR GetSubscribeChannelGroups");
                    Console.WriteLine("Enter 23 FOR DeleteMessages");
                    Console.WriteLine("Enter 31 FOR Push - Register Device");
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
                        pubnub.Destroy();
                        break;
                    case "1":
                        Console.WriteLine("Enter CHANNEL name for subscribe. Use comma to enter multiple channels.");
                        Console.WriteLine("NOTE: If you want to consider only Channel Group(s), just hit ENTER");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
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

                            pubnub.Subscribe<object>()
                                .WithPresence()
                                .Channels(channel.Split(','))
                                .ChannelGroups(channelGroup.Split(','))
                                .Execute();
                        }
                        break;
                    case "2":
                        Console.WriteLine("Enter CHANNEL name for publish.");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.ResetColor();

                        if (channel == "")
                        {
                            Console.WriteLine("Invalid CHANNEL name");
                            break;
                        }

                        bool usePost = false;
                        Console.WriteLine("UsePOST? Enter Y for Yes or N for NO. To accept default(N), just press ENTER");
                        string userPostString = Console.ReadLine();
                        if (userPostString.ToLower() == "y")
                        {
                            usePost = true;
                        }
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("UsePOST = {0}", usePost.ToString()));
                        Console.ResetColor();

                        bool useSync = false;
                        Console.WriteLine("Use Sync? Enter Y for Yes or N for NO. To accept default(N), just press ENTER");
                        string useSyncString = Console.ReadLine();
                        if (useSyncString.ToLower() == "y")
                        {
                            useSync = true;
                        }
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Sync = {0}", usePost.ToString()));
                        Console.ResetColor();

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

                        Console.WriteLine("Enter User Meta Data in JSON dictionary format. If you don't want to enter for now, just press ENTER");
                        string jsonUserMetaData = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Entered User Meta Data = {0}", jsonUserMetaData));
                        Console.ResetColor();

                        Dictionary<string, object> meta = null;
                        if (!string.IsNullOrEmpty(jsonUserMetaData))
                        {
                            meta = pubnub.JsonPluggableLibrary.DeserializeToObject<Dictionary<string, object>>(jsonUserMetaData);
                            if (meta == null)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("INVALID META DATA");
                                Console.ResetColor();
                            }
                        }


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
                        //UserCreated userCreated = new UserCreated();
                        //userCreated.TimeStamp = DateTime.Now;
                        //List<Phone> phoneList = new List<Phone>();
                        //phoneList.Add(new Phone() { Number = "111-222-2222", PhoneType = PhoneType.Mobile, Extenion = "11" });
                        //userCreated.User = new User { Id = 11, Name = "Doe", Addressee = new Addressee { Id = Guid.NewGuid(), Street = "My Street" }, Phones = phoneList };

                        //pubnub.Publish()
                        //    .Channel(channel)
                        //    .Message(userCreated)
                        //    .Meta(meta)
                        //    .ShouldStore(store).UsePOST(usePost)
                        //    .Async(new PNPublishResultExt((r, s) => { Console.WriteLine(r.Timetoken); }));


                        double doubleData;
                        int intData;
                        if (int.TryParse(publishMsg, out intData)) //capture numeric data
                        {
                            pubnub.Publish().Channel(channel).Message(intData).Meta(meta).ShouldStore(store).UsePOST(usePost)
                                .Async(new PNPublishResultExt((r, s) => { Console.WriteLine(r.Timetoken); }));
                        }
                        else if (double.TryParse(publishMsg, out doubleData)) //capture numeric data
                        {
                            pubnub.Publish().Channel(channel).Message(doubleData).Meta(meta).ShouldStore(store).UsePOST(usePost)
                                .Async(new PNPublishResultExt((r, s) => { Console.WriteLine(r.Timetoken); }));
                        }
                        else
                        {
                            //check whether any numeric is sent in double quotes
                            if (publishMsg.IndexOf("\"") == 0 && publishMsg.LastIndexOf("\"") == publishMsg.Length - 1)
                            {
                                string strMsg = publishMsg.Substring(1, publishMsg.Length - 2);
                                if (int.TryParse(strMsg, out intData))
                                {
                                    pubnub.Publish().Channel(channel).Message(strMsg).Meta(meta).ShouldStore(store).UsePOST(usePost)
                                        .Async(new PNPublishResultExt((r, s) => { Console.WriteLine(r.Timetoken); }));
                                }
                                else if (double.TryParse(strMsg, out doubleData))
                                {
                                    pubnub.Publish().Channel(channel).Message(strMsg).Meta(meta).ShouldStore(store).UsePOST(usePost)
                                        .Async(new PNPublishResultExt((r, s) => { Console.WriteLine(r.Timetoken); }));
                                }
                                else
                                {
                                    pubnub.Publish().Channel(channel).Message(publishMsg).Meta(meta).ShouldStore(store).UsePOST(usePost)
                                        .Async(new PNPublishResultExt((r, s) => { Console.WriteLine(r.Timetoken); }));
                                }
                            }
                            else
                            {
                                if (useSync)
                                {
                                    PNPublishResult pubRes = pubnub.Publish()
                                        .Channel(channel)
                                        .Message(publishMsg)
                                        .Meta(meta)
                                        .ShouldStore(store)
                                        .UsePOST(usePost).Sync();
                                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(pubRes));
                                }
                                else
                                {
                                    pubnub.Publish()
                                        .Channel(channel)
                                        .Message(publishMsg)
                                        .Meta(meta)
                                        .ShouldStore(store)
                                        .UsePOST(usePost)
                                        .Async(new PNPublishResultExt((r, s) => { Console.WriteLine(r.Timetoken); }));
                                }
                            }
                        }
                        break;
                    case "3":
                        Console.WriteLine("Enter CHANNEL name for History");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running history()");
                        pubnub.History()
                            .Channel(channel)
                            .Reverse(false)
                            .Count(100)
                            .IncludeTimetoken(true)
                            .Async(new PNHistoryResultExt(
                                (r, s) =>
                                {
                                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                }));
                        break;
                    case "4":
                        bool showUUID = true;
                        bool includeUserState = false;

                        Console.WriteLine("Enter CHANNEL name for HereNow");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Enter channel group name");
                        Console.WriteLine("NOTE: If you want to consider only Channel, just hit ENTER");
                        channelGroup = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("ChannelGroup = {0}", channelGroup));
                        Console.ResetColor();

                        Console.WriteLine("Show UUID List? Y or N? Default is Y. Press N for No Else press ENTER");
                        string userChoiceShowUUID = Console.ReadLine();
                        if (userChoiceShowUUID.ToLower() == "n")
                        {
                            showUUID = false;
                        }
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Show UUID = {0}", showUUID));
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
                        pubnub.HereNow()
                            .Channels(channel.Split(','))
                            .ChannelGroups(channelGroup.Split(','))
                            .IncludeUUIDs(showUUID)
                            .IncludeState(includeUserState)
                            .Async(new PNHereNowResultEx(
                                (r, s) => {
                                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                }));
                        break;
                    case "5":
                        Console.WriteLine("Enter CHANNEL name for Unsubscribe. Use comma to enter multiple channels.");
                        Console.WriteLine("NOTE: If you want to consider only Channel Group, just hit ENTER");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
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
                            pubnub.Unsubscribe<object>()
                                .Channels(new string[] { channel })
                                .ChannelGroups(new string[] { channelGroup })
                                .Execute();

                        }
                        break;
                    case "6":
                        Console.WriteLine("Running time()");
                        pubnub.Time()
                                .Async(
                                    new PNTimeResultExt(
                                        (r, s) => {
                                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                    ));
                        break;
                    case "7":
                        Console.WriteLine("Running Disconnect/auto-Reconnect Subscriber Request Connection");
                        pubnub.TerminateCurrentSubscriberRequest();
                        break;

                    case "8":
                        Console.WriteLine("Enter CHANNEL name(s) for PAM Grant.");
                        channel = Console.ReadLine();

                        if (channel.Trim().Length <= 0)
                        {
                            channel = "";
                        }

                        Console.WriteLine("Enter CHANNEL GROUP name(s) for PAM Grant.");
                        channelGroup = Console.ReadLine();
                        if (channelGroup.Trim().Length <= 0)
                        {
                            channelGroup = "";
                        }

                        if (channel.Trim().Length <= 0 && channelGroup.Trim().Length <= 0)
                        {
                            Console.WriteLine("Channel or ChannelGroup not provided. Please try again.");
                            break;
                        }
                        string[] channelList = channel.Split(',');
                        string[] channelGroupList = channelGroup.Split(',');

                        Console.WriteLine("Enter the auth_key for PAM Grant (optional)");
                        Console.WriteLine("Press Enter Key if there is no auth_key at this time.");
                        string authGrant = Console.ReadLine();
                        string[] authKeyList = authGrant.Split(',');

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

                        bool manage = false;
                        if (channelGroup.Trim().Length > 0)
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
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.WriteLine(string.Format("ChannelGroup = {0}", channelGroup));
                        Console.WriteLine(string.Format("auth_key = {0}", authGrant));
                        Console.WriteLine(string.Format("Read Access = {0}", read.ToString()));
                        if (channel.Trim().Length > 0)
                        {
                            Console.WriteLine(string.Format("Write Access = {0}", write.ToString()));
                        }
                        if (channelGroup.Trim().Length > 0)
                        {
                            Console.WriteLine(string.Format("Manage Access = {0}", manage.ToString()));
                        }
                        Console.WriteLine(string.Format("Grant Access Time Limit = {0}", grantTimeLimitInMinutes.ToString()));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running PamGrant()");

                        pubnub.Grant()
                            .Channels(channelList)
                            .ChannelGroups(channelGroupList)
                            .AuthKeys(authKeyList)
                            .Read(read)
                            .Write(write)
                            .Manage(manage)
                            .TTL(grantTimeLimitInMinutes)
                            .Async(new PNAccessManagerGrantResultExt(
                                (r, s) =>
                                {
                                    if (r != null)
                                    {
                                        Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    }
                                    else if (s != null)
                                    {
                                        Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(s));
                                    }
                                })
                                );
                        break;
                    case "9":
                        Console.WriteLine("Enter CHANNEL name for PAM Audit");
                        Console.WriteLine("To enter CHANNEL GROUP name, just hit ENTER");
                        channel = Console.ReadLine();

                        if (channel.Trim().Length <= 0)
                        {
                            Console.WriteLine("Enter CHANNEL GROUP name for PAM Audit.");
                            channelGroup = Console.ReadLine();
                            channel = "";
                        }
                        else
                        {
                            channelGroup = "";
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
                        string[] authKeyListAudit = authAudit.Split(',');

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("auth_key = {0}", authAudit));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running PamAudit()");

                        pubnub.Audit()
                            .Channel(channel)
                            .ChannelGroup(channelGroup)
                            .AuthKeys(authKeyListAudit)
                            .Async(new PNAccessManagerAuditResultExt(
                                (r, s) => {
                                    if (r != null)
                                    {
                                        Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    }
                                    else if (s != null)
                                    {
                                        Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(s));
                                    }
                                })
                             );
                        break;
                    case "10":
                        Console.WriteLine("Enter CHANNEL name(s) for PAM Revoke");
                        channel = Console.ReadLine();
                        if (channel.Trim().Length <= 0)
                        {
                            channel = "";
                        }

                        Console.WriteLine("Enter CHANNEL GROUP name(s) for PAM Revoke.");
                        channelGroup = Console.ReadLine();
                        if (channelGroup.Trim().Length <= 0)
                        {
                            channelGroup = "";
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

                        string[] channelList2 = channel.Split(',');
                        string[] channelGroupList2 = channelGroup.Split(',');

                        Console.WriteLine("Enter the auth_key for PAM Revoke (optional)");
                        Console.WriteLine("Press Enter Key if there is no auth_key at this time.");
                        string authRevoke = Console.ReadLine();
                        string[] authKeyList2 = authRevoke.Split(',');

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("auth_key = {0}", authRevoke));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running PamRevoke()");
                        pubnub.Grant()
                            .Channels(channelList2)
                            .ChannelGroups(channelGroupList2)
                            .AuthKeys(authKeyList2)
                            .Read(false)
                            .Write(false)
                            .Manage(false)
                            .Async(new PNAccessManagerGrantResultExt(
                                (r, s) =>
                                {
                                    if (r != null)
                                    {
                                        Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    }
                                    else if (s != null)
                                    {
                                        Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(s));
                                    }
                                })
                                );
                        break;
                    case "11":
                        Console.WriteLine("Enabling simulation of Sleep/Suspend Mode");
                        pubnub.EnableMachineSleepModeForTestingOnly();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Machine Sleep Mode simulation activated");
                        Console.ResetColor();
                        break;
                    case "12":
                        Console.WriteLine("Disabling simulation of Sleep/Suspend Mode");
                        pubnub.DisableMachineSleepModeForTestingOnly();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Simulation going to awake mode");
                        Console.ResetColor();
                        break;
                    case "13":
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

                        Dictionary<string, object> addOrModifystate = new Dictionary<string, object>();
                        if (Int32.TryParse(valueUserState, out valueInt))
                        {
                            addOrModifystate.Add(keyUserState, valueInt);
                        }
                        else if (Double.TryParse(valueUserState, out valueDouble))
                        {
                            addOrModifystate.Add(keyUserState, valueDouble);
                        }
                        else
                        {
                            addOrModifystate.Add(keyUserState, valueUserState);
                        }
                        pubnub.SetPresenceState()
                            .Channels(userStateChannel.Split(','))
                            .ChannelGroups(userStateChannelGroup.Split(','))
                            .State(addOrModifystate)
                            .Async(new PNSetStateResultExt(
                                (r, s) => {
                                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                }));

                        break;
                    case "14":
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
                        Dictionary<string, object> deleteDic = new Dictionary<string, object>();
                        deleteDic.Add(deleteKeyUserState, null);
                        pubnub.SetPresenceState()
                            .Channels(new string[] { deleteChannelUserState })
                            .ChannelGroups(new string[] { deleteChannelGroupUserState })
                            .State(deleteDic)
                            .Async(new PNSetStateResultExt(
                                (r, s) => {
                                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                }));

                        break;
                    case "15":
                        Console.WriteLine("Enter channel name");
                        Console.WriteLine("NOTE: If you want to consider only Channel Group, just hit ENTER");
                        string getUserStateChannel2 = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", getUserStateChannel2));
                        Console.ResetColor();

                        Console.WriteLine("Enter channel group name");
                        Console.WriteLine("NOTE: If you want to consider only Channel, just hit ENTER");
                        string getUserStateChannelGroup2 = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("ChannelGroup = {0}", getUserStateChannelGroup2));
                        Console.ResetColor();

                        Console.WriteLine("Enter UUID. (Optional. Press ENTER to skip it)");
                        string uuid2 = Console.ReadLine();

                        string[] getUserStateChannel2List = getUserStateChannel2.Split(',');
                        string[] getUserStateChannelGroup2List = getUserStateChannelGroup2.Split(',');

                        pubnub.GetPresenceState()
                            .Channels(getUserStateChannel2List)
                            .ChannelGroups(getUserStateChannelGroup2List)
                            .Uuid(uuid2)
                            .Async(new PNGetStateResultExt(
                                (r, s) => {
                                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                }));

                        break;
                    case "16":
                        Console.WriteLine("Enter uuid for WhereNow. To consider SessionUUID, just press ENTER");
                        string whereNowUuid = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("uuid = {0}", whereNowUuid));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running Where_Now()");
                        pubnub.WhereNow()
                            .Uuid(whereNowUuid)
                            .Async(new PNWhereNowResultExt(
                                (r, s) => {
                                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                }));
                        break;
                    case "17":
                        Console.WriteLine("ENTER UUID.");
                        string sessionUUID = Console.ReadLine();
                        pubnub.ChangeUUID(sessionUUID);
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("UUID = {0}", config.Uuid);
                        Console.ResetColor();
                        break;
                    case "18":
                        Console.WriteLine("Disconnect");
                        pubnub.Disconnect<object>();
                        break;
                    case "19":
                        Console.WriteLine("Re-connect");
                        pubnub.Reconnect<object>();
                        break;
                    case "20":
                        Console.WriteLine("UnsubscribeAll");
                        pubnub.UnsubscribeAll<object>();
                        break;
                    case "21":
                        Console.WriteLine("GetSubscribedChannels");
                        List<string> chList = pubnub.GetSubscribedChannels();
                        if (chList != null && chList.Count > 0)
                        {
                            Console.WriteLine(chList.Aggregate((x,y)=> x + "," + y));
                        }
                        else
                        {
                            Console.WriteLine("No channels");
                        }
                        break;
                    case "22":
                        Console.WriteLine("GetSubscribedChannelGroups");
                        List<string> cgList = pubnub.GetSubscribedChannelGroups();
                        if (cgList != null && cgList.Count > 0)
                        {
                            Console.WriteLine(cgList.Aggregate((x, y) => x + "," + y));
                        }
                        else
                        {
                            Console.WriteLine("No channelgroups");
                        }
                        break;
                    case "23":
                        Console.WriteLine("Enter channel name: ");
                        //Console.WriteLine("NOTE: If you want to consider only Channel Group, just hit ENTER");
                        string deleteMessageChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", deleteMessageChannel));
                        Console.ResetColor();
                        pubnub.DeleteMessages().Channel(deleteMessageChannel) //.Start(15021998840118350).End(15026815715586992)
                            .Async(new PNDeleteMessageResultExt(
                                (r, s) => {
                                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                }));

                        break;
                    case "31":
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

                        Console.WriteLine("Running AddPushNotificationsOnChannels()");
                        pubnub.AddPushNotificationsOnChannels().Channels(new string[] { pushRegisterChannel })
                            .PushType(PNPushType.MPNS)
                            .DeviceId(pushToken)
                            .Async(new PNPushAddChannelResultExt(
                                (r, s) => {
                                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                }));
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

                        Console.WriteLine("Running RemovePushNotificationsFromChannels()");
                        pubnub.RemovePushNotificationsFromChannels()
                            .Channels(new string[] { pushRemoveChannel })
                            .PushType(PNPushType.MPNS)
                            .DeviceId(pushTokenRemove)
                            .Async(new PNPushRemoveChannelResultExt(
                                (r, s) => {
                                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                }));
                        break;
                    case "33":
                        Console.WriteLine("Enter Push Token for MPNS");
                        string pushTokenGetChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Push Token = {0}", pushTokenGetChannel));
                        Console.ResetColor();

                        Console.WriteLine("Running AuditPushChannelProvisions()");
                        pubnub.AuditPushChannelProvisions()
                            .PushType(PNPushType.APNS)
                            .DeviceId(pushTokenGetChannel)
                            .Async(new PNPushListProvisionsResultExt(
                                (r, s) => {
                                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                }));
                        break;

                    case "38":
                        Console.WriteLine("Enter channel group name");
                        string addChannelGroupName = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("channel group name = {0}", addChannelGroupName));
                        Console.ResetColor();


                        Console.WriteLine("Enter CHANNEL name. Use comma to enter multiple channels.");
                        channel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.ResetColor();
                        Console.WriteLine();
                        pubnub.AddChannelsToChannelGroup()
                            .ChannelGroup(addChannelGroupName)
                            .Channels(channel.Split(','))
                            .Async(new PNChannelGroupsAddChannelResultExt(
                                (r, s) => {
                                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                }));
                        break;
                    case "39":
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
                        Console.WriteLine("Do you want to delete the channel group and all its channels? Default is No. Enter Y for Yes, Else just hit ENTER key");
                        string removeExistingGroup = Console.ReadLine();
                        if (removeExistingGroup.ToLower() == "y")
                        {
                            pubnub.DeleteChannelGroup()
                                .ChannelGroup(removeChannelGroupName)
                                .Async(new PNChannelGroupsDeleteGroupResultExt(
                                    (r, s) => {
                                        Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    }));
                            break;
                        }

                        Console.WriteLine("Enter CHANNEL name. Use comma to enter multiple channels.");
                        channel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.ResetColor();
                        Console.WriteLine();
                        pubnub.RemoveChannelsFromChannelGroup()
                            .ChannelGroup(removeChannelGroupName)
                            .Channels(channel.Split(','))
                            .Async(new PNChannelGroupsRemoveChannelResultExt(
                                (r, s) => {
                                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                }));
                        break;
                    case "40":
                        Console.WriteLine("Do you want to get all existing channel group names? Default is No. Enter Y for Yes, Else just hit ENTER key");
                        string getExistingGroupNames = Console.ReadLine();
                        if (getExistingGroupNames.ToLower() == "y")
                        {
                            pubnub.ListChannelGroups()
                                .Async(new PNChannelGroupsListAllResultExt(
                                    (r, s) => {
                                        Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    }));
                            break;
                        }

                        Console.WriteLine("Enter channel group name");
                        string channelGroupName = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("channel group name = {0}", channelGroupName));
                        Console.ResetColor();

                        pubnub.ListChannelsForChannelGroup()
                            .ChannelGroup(channelGroupName)
                            .Async(new PNChannelGroupsAllChannelsResultExt(
                                (r, s) => {
                                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                }));
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

        static void Display(string message)
        {
            Console.WriteLine(message);
        }

    }


    public class PubnubDemoObject
    {
        public double VersionID = 3.4;
        public string Timetoken = "13601488652764619";
        public string OperationName = "Publish";
        public string[] Channels = { "ch1" };
        public PubnubDemoMessage DemoMessage = new PubnubDemoMessage();
        public PubnubDemoMessage CustomMessage = new PubnubDemoMessage("Welcome to the world of Pubnub for Publish and Subscribe. Hah!");
        public Person[] SampleXml = new DemoRoot().Person.ToArray();
    }

    public class PubnubDemoMessage
    {
        public string DefaultMessage = "~!@#$%^&*()_+ `1234567890-= qwertyuiop[]\\ {}| asdfghjkl;' :\" zxcvbnm,./ <>? ";

        public PubnubDemoMessage()
        {
        }

        public PubnubDemoMessage(string message)
        {
            DefaultMessage = message;
        }

    }

    public class DemoRoot
    {
        public List<Person> Person
        {
            get
            {
                List<Person> ret = new List<Person>();
                Person p1 = new Person();
                p1.ID = "ABCD123";
                //PersonID id1 = new PersonID(); id1.ID = "ABCD123" ;
                //p1.ID = id1;
                Name n1 = new Name();
                n1.First = "John";
                n1.Middle = "P.";
                n1.Last = "Doe";
                p1.Name = n1;

                Address a1 = new Address();
                a1.Street = "123 Duck Street";
                a1.City = "New City";
                a1.State = "New York";
                a1.Country = "United States";
                p1.Address = a1;

                ret.Add(p1);

                Person p2 = new Person();
                p2.ID = "ABCD456";
                //PersonID id2 = new PersonID(); id2.ID = "ABCD123" ;
                //p2.ID = id2;
                Name n2 = new Name();
                n2.First = "Peter";
                n2.Middle = "Z.";
                n2.Last = "Smith";
                p2.Name = n2;

                Address a2 = new Address();
                a2.Street = "12 Hollow Street";
                a2.City = "Philadelphia";
                a2.State = "Pennsylvania";
                a2.Country = "United States";
                p2.Address = a2;

                ret.Add(p2);

                return ret;

            }
        }
    }

    public class Person
    {
        public string ID { get; set; }

        public Name Name;

        public Address Address;
    }

    public class Name
    {
        public string First { get; set; }
        public string Middle { get; set; }
        public string Last { get; set; }
    }

    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }


    public class UserCreated
    {
        public DateTime TimeStamp { get; set; }
        public User User { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Addressee Addressee { get; set; }
        public List<Phone> Phones { get; set; }
    }

    public class Addressee
    {
        public Guid Id { get; set; }
        public string Street { get; set; }
    }

    public class Phone
    {
        public string Number { get; set; }
        public string Extenion { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PhoneType PhoneType { get; set; }
    }

    public enum PhoneType
    {
        Home,
        Mobile,
        Work
    }

    public class DemoTimeResult : PNCallback<PNTimeResult>
    {
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub(null);
        public DemoTimeResult(Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }

        public override void OnResponse(PNTimeResult result, PNStatus status)
        {
            if (result != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            }
            else if (status != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }
    };

    public class DemoPublishResult : PNCallback<PNPublishResult>
    {
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub(null);
        public DemoPublishResult(Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }

        public override void OnResponse(PNPublishResult result, PNStatus status)
        {
            if (result != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            }
            else if (status != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }
    };

    public class DemoHistoryResult : PNCallback<PNHistoryResult>
    {
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub(null);
        public DemoHistoryResult(Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse(PNHistoryResult result, PNStatus status)
        {
            if (result != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            }
            else if (status != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }
    };

    public class DemoHereNowResult : PNCallback<PNHereNowResult>
    {
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub(null);
        public DemoHereNowResult(Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse(PNHereNowResult result, PNStatus status)
        {
            if (result != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            }
            else if (status != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }
    };

    public class DemoWhereNowResult : PNCallback<PNWhereNowResult>
    {
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub(null);
        public DemoWhereNowResult(Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse(PNWhereNowResult result, PNStatus status)
        {
            if (result != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            }
            else if (status != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }
    };

    public class DemoPNGetStateResult : PNCallback<PNGetStateResult>
    {
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub(null);
        public DemoPNGetStateResult(Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse(PNGetStateResult result, PNStatus status)
        {
            if (result != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            }
            else if (status != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }
    };

    public class DemoPNSetStateResult : PNCallback<PNSetStateResult>
    {
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub(null);
        public DemoPNSetStateResult(Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse(PNSetStateResult result, PNStatus status)
        {
            if (result != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            }
            else if (status != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }
    };

    public class DemoSubscribeCallback : SubscribeCallback
    {
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub(null);
        public DemoSubscribeCallback(Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void Message<T>(Pubnub pubnub, PNMessageResult<T> message)
        {
            if (message != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(message));
            }
        }

        public override void Presence(Pubnub pubnub, PNPresenceEventResult presence)
        {
            if (presence != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(presence));
            }
        }

        public override void Status(Pubnub pubnub, PNStatus status)
        {
            string msg = string.Format("Operation: {0}; Category: {1};  StatusCode: {2}", status.Operation, status.Category, status.StatusCode);
            this.callback(msg);

            //if (status.StatusCode != 200 || status.Error)
            //{
            //    Console.ForegroundColor = ConsoleColor.Red;
            //    if (status.ErrorData != null)
            //    {
            //        Console.WriteLine(status.ErrorData.Information);
            //    }
            //    Console.ForegroundColor = ConsoleColor.White;
            //}

            if (status.Category == PNStatusCategory.PNUnexpectedDisconnectCategory)
            {
                // This event happens when radio / connectivity is lost
            }
            else if (status.Category == PNStatusCategory.PNConnectedCategory)
            {
                //Console.WriteLine("CONNECTED {0} Channels = {1}, ChannelGroups = {2}", status.StatusCode, string.Join(",", status.AffectedChannels), string.Join(",", status.AffectedChannelGroups));
                // Connect event. You can do stuff like publish, and know you'll get it.
                // Or just use the connected event to confirm you are subscribed for
                // UI / internal notifications, etc

            }
            else if (status.Category == PNStatusCategory.PNReconnectedCategory)
            {
                //Console.WriteLine("RE-CONNECTED {0} Channels = {1}, ChannelGroups = {2}", status.StatusCode, string.Join(",", status.AffectedChannels), string.Join(",", status.AffectedChannelGroups));
                // Happens as part of our regular operation. This event happens when
                // radio / connectivity is lost, then regained.
            }
            else if (status.Category == PNStatusCategory.PNDecryptionErrorCategory)
            {
                // Handle messsage decryption error. Probably client configured to
                // encrypt messages and on live data feed it received plain text.
            }
        }
    }

    public class DemoGrantResult : PNCallback<PNAccessManagerGrantResult>
    {
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub(null);
        public DemoGrantResult(Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse(PNAccessManagerGrantResult result, PNStatus status)
        {
            if (result != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            }
            else if (status != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }
    };

    public class DemoAuditResult : PNCallback<PNAccessManagerAuditResult>
    {
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub(null);
        public DemoAuditResult(Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse(PNAccessManagerAuditResult result, PNStatus status)
        {
            if (result != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            }
            else if (status != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }
    };

    public class DemoPushAddChannel : PNCallback<PNPushAddChannelResult>
    {
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub(null);
        public DemoPushAddChannel(Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse(PNPushAddChannelResult result, PNStatus status)
        {
            if (result != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            }
            else if (status != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }
    }

    public class DemoPushRemoveChannel : PNCallback<PNPushRemoveChannelResult>
    {
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub(null);
        public DemoPushRemoveChannel(Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse(PNPushRemoveChannelResult result, PNStatus status)
        {
            if (result != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            }
            else if (status != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }
    }

    public class DemoPushListProvisionChannel : PNCallback<PNPushListProvisionsResult>
    {
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub(null);
        public DemoPushListProvisionChannel(Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse(PNPushListProvisionsResult result, PNStatus status)
        {
            if (result != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            }
            else if (status != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }
    }

    public class DemoChannelGroupAddChannel : PNCallback<PNChannelGroupsAddChannelResult>
    {
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub(null);
        public DemoChannelGroupAddChannel(Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse(PNChannelGroupsAddChannelResult result, PNStatus status)
        {
            if (result != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            }
            else if (status != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }
    }

    public class DemoChannelGroupRemoveChannel : PNCallback<PNChannelGroupsRemoveChannelResult>
    {
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub(null);
        public DemoChannelGroupRemoveChannel(Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse(PNChannelGroupsRemoveChannelResult result, PNStatus status)
        {
            if (result != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            }
            else if (status != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }
    }

    public class DemoChannelGroupDeleteGroup : PNCallback<PNChannelGroupsDeleteGroupResult>
    {
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub(null);
        public DemoChannelGroupDeleteGroup(Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse(PNChannelGroupsDeleteGroupResult result, PNStatus status)
        {
            if (result != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            }
            else if (status != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }
    }

    public class DemoChannelGroupAll : PNCallback<PNChannelGroupsListAllResult>
    {
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub(null);
        public DemoChannelGroupAll(Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse(PNChannelGroupsListAllResult result, PNStatus status)
        {
            if (result != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            }
            else if (status != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }
    }

    public class DemoChannelGroupAllChannels : PNCallback<PNChannelGroupsAllChannelsResult>
    {
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub(null);
        public DemoChannelGroupAllChannels(Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse(PNChannelGroupsAllChannelsResult result, PNStatus status)
        {
            if (result != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            }
            else if (status != null)
            {
                this.callback(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }
    }

}
