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
        static private Pubnub pubnub { get; set; }

        static private string authKey { get; set; } = "";

        public class PlatformPubnubLog : IPubnubLog
        {
            public PlatformPubnubLog()
            {
                // Get folder path may vary based on environment
                string folder = System.IO.Directory.GetCurrentDirectory(); //For console
                //string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); // For iOS
                System.Diagnostics.Debug.WriteLine(folder);
                string logFilePath = System.IO.Path.Combine(folder, "pubnubmessaging.log");
                Trace.Listeners.Add(new TextWriterTraceListener(logFilePath));
            }

            void IPubnubLog.WriteToLog(string log)
            {
                Trace.WriteLine(log);
                Trace.Flush();
            }
        }


        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Unhandled exception occured inside Pubnub C# API. Exiting the application. Please try again.");
            Environment.Exit(1);
        }

        static public void Main()
        {
            PNConfiguration config = new PNConfiguration();
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            StringBuilder hintStringBuilder = new StringBuilder();
            hintStringBuilder.AppendLine();

            hintStringBuilder.AppendLine("HINT: TO TEST RE-CONNECT AND CATCH-UP,");
            hintStringBuilder.AppendLine("      DISCONNECT YOUR MACHINE FROM NETWORK/INTERNET AND ");
            hintStringBuilder.AppendLine("      RE-CONNECT YOUR MACHINE AFTER SOMETIME.");
            hintStringBuilder.AppendLine();
            hintStringBuilder.AppendLine("      IF NO NETWORK BEFORE MAX RE-TRY CONNECT,");
            hintStringBuilder.AppendLine("      NETWORK ERROR MESSAGE WILL BE SENT");
            hintStringBuilder.AppendLine();

            hintStringBuilder.AppendLine("Enter Pubnub Origin. Default Origin = ps.pndsn.com"); //TODO
            hintStringBuilder.AppendLine("If you want to accept default value, press ENTER.");
            Console.WriteLine(hintStringBuilder.ToString());
            string origin = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (origin.Trim() == "")
            {
                origin = "ingress.bronze.aws-pdx-1.ps.pn";// "balancer1b.bronze.aws-pdx-1.ps.pn";// "ingress.bronze.aws-pdx-1.ps.pn";// "ps.pndsn.com";
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
            if (enableSSL.Trim().ToLowerInvariant() == "y")
            {
                Console.WriteLine("SSL Enabled");
            }
            else if (enableSSL.Trim().ToLowerInvariant() == "n")
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

            Console.WriteLine("ENTER cipher key for encryption feature." + Environment.NewLine + "If you don't want to avail at this time, press ENTER.");
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

            Console.WriteLine("ENTER subscribe key." + Environment.NewLine + "If you want to accept default demo subscribe key, press ENTER.");
            string subscribeKey = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (subscribeKey.Trim().Length > 0)
            {
                Console.WriteLine("Subscribe key provided.");
            }
            else
            {
                Console.WriteLine("Default demo subscribe key provided");
                subscribeKey = "sub-c-d7da9e58-c997-11e9-a139-dab2c75acd6f";// "demo-36";
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("ENTER publish key." + Environment.NewLine + "If you want to accept default demo publish key, press ENTER.");
            string publishKey = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (publishKey.Trim().Length > 0)
            {
                Console.WriteLine("Publish key provided.");
            }
            else
            {
                Console.WriteLine("Default demo publish key provided");
                publishKey = "pub-c-03f156ea-a2e3-4c35-a733-9535824be897";// "demo-36";
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("ENTER secret key." + Environment.NewLine + "If you don't want to avail at this time, press ENTER.");
            string secretKey = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (secretKey.Trim().Length > 0)
            {
                Console.WriteLine("Secret key provided.");
            }
            else
            {
                Console.WriteLine("Default demo Secret key provided");
                secretKey = "sec-c-MmUxNTZjMmYtNzFkNS00ODkzLWE2YjctNmQ4YzE5NWNmZDA3";// "sec-c-NjM2YTczYmQtYWY2NS00NmRlLTgxYTktZWMyOTMzZGQ4OWY5";// "demo-36";
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("Use Custom Session UUID? ENTER Y for Yes, else N");
            string enableCustomUUID = Console.ReadLine();
            if (enableCustomUUID.Trim().ToLowerInvariant() == "y")
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

            Console.WriteLine("Enable Internal Logging? Enter Y for Yes, Else N for No." + Environment.NewLine + "Default = Y  ");
            string enableLoggingString = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (enableLoggingString.Trim().ToLowerInvariant() == "n")
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
            //config.EnableTelemetry = false;

            pubnub = new Pubnub(config);
            //Add listener to receive published messages and presence events
            pubnub.AddListener(new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNMessageResult<object> pubMsg) { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(pubMsg)); },
                delegate (Pubnub pnObj, PNPresenceEventResult presenceEvnt) { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(presenceEvnt)); },
                delegate (Pubnub pnObj, PNStatus pnStatus) { Console.WriteLine("{0} {1} {2}", pnStatus.Operation, pnStatus.Category, pnStatus.StatusCode); }
                ));

            //Add listener to receive signal messages
            pubnub.AddListener(new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNSignalResult<object> signalMsg) { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(signalMsg)); },
                delegate (Pubnub pnObj, PNStatus pnStatus) { Console.WriteLine("{0} {1} {2}", pnStatus.Operation, pnStatus.Category, pnStatus.StatusCode); }
                ));

            //Add listener to receive ObjectAPI event messages
            pubnub.AddListener(new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNObjectApiEventResult objectApiEventObj) 
                {
                    if (objectApiEventObj.Type == "user") { /* got user related event. */ }
                    else if (objectApiEventObj.Type == "space") { /* got space related event. */ }
                    else if (objectApiEventObj.Type == "membership") { /* got membership related event. */ }
                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(objectApiEventObj));
                },
                delegate (Pubnub pnObj, PNStatus pnStatus) { Console.WriteLine("{0} {1} {2}", pnStatus.Operation, pnStatus.Category, pnStatus.StatusCode); }
                ));

            bool exitFlag = false;
            string channel = "";
            string channelGroup = "";
            int currentUserChoice = 0;
            string userinput = "";
            Console.WriteLine("");
            while (!exitFlag)
            {
                if (currentUserChoice < 1 || (currentUserChoice > 53 && currentUserChoice != 99))
                {
                    StringBuilder menuOptionsStringBuilder = new StringBuilder();
                    menuOptionsStringBuilder.AppendLine("ENTER 1 FOR Subscribe channel/channelgroup");
                    menuOptionsStringBuilder.AppendLine("ENTER 2 FOR Publish");
                    menuOptionsStringBuilder.AppendLine("ENTER 3 FOR History");
                    menuOptionsStringBuilder.AppendLine("ENTER 4 FOR Here_Now");
                    menuOptionsStringBuilder.AppendLine("ENTER 5 FOR Unsubscribe");
                    menuOptionsStringBuilder.AppendLine("ENTER 6 FOR Time");
                    menuOptionsStringBuilder.AppendLine("ENTER 7 FOR Disconnect/Reconnect existing Subscriber(s) (when internet is available)");
                    menuOptionsStringBuilder.AppendLine("ENTER 8 FOR Grant Access to channel/ChannelGroup");
                    menuOptionsStringBuilder.AppendLine("ENTER 9 FOR Audit Access to channel/ChannelGroup");
                    menuOptionsStringBuilder.AppendLine("ENTER 10 FOR Revoke Access to channel/ChannelGroup");
                    menuOptionsStringBuilder.AppendLine("ENTER 11 TO Simulate Machine Sleep Mode");
                    menuOptionsStringBuilder.AppendLine("ENTER 12 TO Simulate Machine Awake Mode");
                    menuOptionsStringBuilder.AppendLine("Enter 13 TO Set User State by Add/Modify Key-Pair");
                    menuOptionsStringBuilder.AppendLine("Enter 14 TO Set User State by Deleting existing Key-Pair");
                    menuOptionsStringBuilder.AppendLine("Enter 15 TO Get User State");
                    menuOptionsStringBuilder.AppendLine("Enter 16 FOR WhereNow");
                    menuOptionsStringBuilder.AppendLine(string.Format("Enter 17 TO change UUID. (Current value = {0})", config.Uuid));
                    menuOptionsStringBuilder.AppendLine("Enter 18 FOR Disconnect");
                    menuOptionsStringBuilder.AppendLine("Enter 19 FOR Reconnect");
                    menuOptionsStringBuilder.AppendLine("Enter 20 FOR UnsubscribeAll");
                    menuOptionsStringBuilder.AppendLine("Enter 21 FOR GetSubscribeChannels");
                    menuOptionsStringBuilder.AppendLine("Enter 22 FOR GetSubscribeChannelGroups");
                    menuOptionsStringBuilder.AppendLine("Enter 23 FOR DeleteMessages");
                    menuOptionsStringBuilder.AppendLine("Enter 24 FOR MessagesCount");
                    menuOptionsStringBuilder.AppendLine("Enter 31 FOR Push - Register Device");
                    menuOptionsStringBuilder.AppendLine("Enter 32 FOR Push - Remove Channel");
                    menuOptionsStringBuilder.AppendLine("Enter 33 FOR Push - Get Current Channels");
                    menuOptionsStringBuilder.AppendLine("Enter 34 FOR Push - Unregister Device");
                    menuOptionsStringBuilder.AppendLine("Enter 38 FOR Channel Group - Add channel(s)");
                    menuOptionsStringBuilder.AppendLine("Enter 39 FOR Channel Group - Remove channel/group/namespace");
                    menuOptionsStringBuilder.AppendLine("Enter 40 FOR Channel Group - Get channel(s)/namespace(s)");
                    menuOptionsStringBuilder.AppendLine("Enter 41 FOR Signal");
                    menuOptionsStringBuilder.AppendLine("Enter 42 FOR Create User");
                    menuOptionsStringBuilder.AppendLine("Enter 43 FOR Get User(s)");
                    menuOptionsStringBuilder.AppendLine("Enter 44 FOR Delete User");
                    menuOptionsStringBuilder.AppendLine("Enter 45 FOR Update User");
                    menuOptionsStringBuilder.AppendLine("Enter 46 FOR Create Space");
                    menuOptionsStringBuilder.AppendLine("Enter 47 FOR Get Space(s)");
                    menuOptionsStringBuilder.AppendLine("Enter 48 FOR Delete Space");
                    menuOptionsStringBuilder.AppendLine("Enter 49 FOR Update Space");
                    menuOptionsStringBuilder.AppendLine("Enter 50 FOR Add/Update/Remove Memberships");
                    menuOptionsStringBuilder.AppendLine("Enter 51 FOR Add/Update/Remove Members");
                    menuOptionsStringBuilder.AppendLine("Enter 52 FOR Get Memberships");
                    menuOptionsStringBuilder.AppendLine("Enter 53 FOR Get Members");
                    menuOptionsStringBuilder.AppendLine("ENTER 99 FOR EXIT OR QUIT");
                    Console.WriteLine(menuOptionsStringBuilder.ToString());
                    userinput = Console.ReadLine();
                }
                switch (userinput)
                {
                    case "99":
                        exitFlag = true;
                        pubnub.Destroy();
                        break;
                    case "1":
                        Console.WriteLine("Enter CHANNEL name for subscribe. Use comma to enter multiple channels." + Environment.NewLine + "NOTE: If you want to consider only Channel Group(s), just hit ENTER");
                        channel = Console.ReadLine();
                        channel = channel + ",pnuser-pandu-id-test,pandu-space-id,pnuser-pandu-my-id0";

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        StringBuilder cgOptionStringBuilder = new StringBuilder();
                        cgOptionStringBuilder.AppendLine("Enter CHANNEL GROUP name for subscribe. Use comma to enter multiple channel groups.");
                        cgOptionStringBuilder.AppendLine("To denote a namespaced CHANNEL GROUP, use the colon (:) character with the format namespace:channelgroup.");
                        cgOptionStringBuilder.AppendLine("NOTE: If you want to consider only Channel(s), assuming you already entered , just hit ENTER");
                        Console.WriteLine(cgOptionStringBuilder.ToString());
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
                        //    .Execute(new PNPublishResultExt((r, s) => { Console.WriteLine(r.Timetoken); }));


                        double doubleData;
                        int intData;
                        if (int.TryParse(publishMsg, out intData)) //capture numeric data
                        {
                            pubnub.Publish().Channel(channel).Message(intData).Meta(meta).ShouldStore(store).UsePOST(usePost)
                                .Execute(new PNPublishResultExt((r, s) => { if (s.Error) { Console.WriteLine(s.ErrorData.Information); } else { Console.WriteLine(r.Timetoken); } }));
                        }
                        else if (double.TryParse(publishMsg, out doubleData)) //capture numeric data
                        {
                            pubnub.Publish().Channel(channel).Message(doubleData).Meta(meta).ShouldStore(store).UsePOST(usePost)
                                .Execute(new PNPublishResultExt((r, s) => { if (s.Error) { Console.WriteLine(s.ErrorData.Information); } else { Console.WriteLine(r.Timetoken); } }));
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
                                        .Execute(new PNPublishResultExt((r, s) => { if (s.Error) { Console.WriteLine(s.ErrorData.Information); } else { Console.WriteLine(r.Timetoken); } }));
                                }
                                else if (double.TryParse(strMsg, out doubleData))
                                {
                                    pubnub.Publish().Channel(channel).Message(strMsg).Meta(meta).ShouldStore(store).UsePOST(usePost)
                                        .Execute(new PNPublishResultExt((r, s) => { if (s.Error) { Console.WriteLine(s.ErrorData.Information); } else { Console.WriteLine(r.Timetoken); } }));
                                }
                                else
                                {
                                    pubnub.Publish().Channel(channel).Message(publishMsg).Meta(meta).ShouldStore(store).UsePOST(usePost)
                                        .Execute(new PNPublishResultExt((r, s) => { if (s.Error) { Console.WriteLine(s.ErrorData.Information); } else { Console.WriteLine(r.Timetoken); } }));
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
                                        .Execute(new PNPublishResultExt((r, s) => { if (s.Error) { Console.WriteLine(s.ErrorData.Information); } else { Console.WriteLine(r.Timetoken); } }));
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
                            .Execute(new PNHistoryResultExt(
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

                        Console.WriteLine("Enter channel group name" + Environment.NewLine + "NOTE: If you want to consider only Channel, just hit ENTER");
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
                            .Execute(new PNHereNowResultEx(
                                (r, s) => {
                                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                }));
                        break;
                    case "5":
                        Console.WriteLine("Enter CHANNEL name for Unsubscribe. Use comma to enter multiple channels." + Environment.NewLine + "NOTE: If you want to consider only Channel Group, just hit ENTER");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Enter channel group name" + Environment.NewLine + "NOTE: If you want to consider only Channel, just hit ENTER");
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
                                .Execute(
                                    new PNTimeResultExt(
                                        (r, s) =>
                                        {
                                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                    ));
                        break;
                    case "7":
                        Console.WriteLine("Running Disconnect/auto-Reconnect Subscriber Request Connection");
                        pubnub.TerminateCurrentSubscriberRequest();
                        break;

                    case "8":
pubnub.Grant()
    .Channels(new[] { "my_channel" })
    .ChannelGroups(new[] { "my_channelgroup" })
    .Users(new[] { "my_userid" })
    .Spaces(new[] { "my_spaceid" })
    .Read(true)
    .Write(true)
    .TTL(10)
    .Execute(new PNAccessManagerTokenResultExt((result, status) => { }));

                        //pubnub.Grant()
                        //    .Channels(new string[] { "pamv3channel" })
                        //    //.Users(new string[] { "user-1"})
                        //    //.Spaces(new string[] { "space-1"})
                        //    .Read(true)
                        //    .Write(true)
                        //    .Delete(true)
                        //    .Manage(true)
                        //    .TTL(10)
                        //    .Execute(new PNAccessManagerTokenResultExt(
                        //        (r, s) =>
                        //        {
                        //            if (r != null)
                        //            {
                        //                Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                        //            }
                        //            else if (s != null)
                        //            {
                        //                Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(s));
                        //            }
                        //        })
                        //        );
                        //Console.WriteLine("Enter CHANNEL name(s) for PAM Grant.");
                        //channel = Console.ReadLine();

                        //if (channel.Trim().Length <= 0)
                        //{
                        //    channel = "";
                        //}

                        //Console.WriteLine("Enter CHANNEL GROUP name(s) for PAM Grant.");
                        //channelGroup = Console.ReadLine();
                        //if (channelGroup.Trim().Length <= 0)
                        //{
                        //    channelGroup = "";
                        //}

                        //if (channel.Trim().Length <= 0 && channelGroup.Trim().Length <= 0)
                        //{
                        //    Console.WriteLine("Channel or ChannelGroup not provided. Please try again.");
                        //    break;
                        //}
                        //string[] channelList = channel.Split(',');
                        //string[] channelGroupList = channelGroup.Split(',');

                        //Console.WriteLine("Enter the auth_key for PAM Grant (optional)" + Environment.NewLine + "Press Enter Key if there is no auth_key at this time.");
                        //string authGrant = Console.ReadLine();
                        //string[] authKeyList = authGrant.Split(',');

                        //Console.WriteLine("Read Access? Enter Y for Yes (default), N for No.");
                        //string readAccess = Console.ReadLine();
                        //bool read = (readAccess.ToLower() == "n") ? false : true;

                        //bool write = false;
                        //if (channel.Trim().Length > 0)
                        //{
                        //    Console.WriteLine("Write Access? Enter Y for Yes (default), N for No.");
                        //    string writeAccess = Console.ReadLine();
                        //    write = (writeAccess.ToLower() == "n") ? false : true;
                        //}

                        //bool delete = false;
                        //if (channel.Trim().Length > 0)
                        //{
                        //    Console.WriteLine("Delete Access? Enter Y for Yes (default), N for No.");
                        //    string deleteAccess = Console.ReadLine();
                        //    delete = (deleteAccess.ToLower() == "n") ? false : true;
                        //}

                        //bool manage = false;
                        //if (channelGroup.Trim().Length > 0)
                        //{
                        //    Console.WriteLine("Manage Access? Enter Y for Yes (default), N for No.");
                        //    string manageAccess = Console.ReadLine();
                        //    manage = (manageAccess.ToLower() == "n") ? false : true;
                        //}
                        //Console.WriteLine("How many minutes do you want to allow Grant Access? Enter the number of minutes." + Environment.NewLine + "Default = 1440 minutes (24 hours). Press ENTER now to accept default value.");
                        //int grantTimeLimitInMinutes;
                        //string grantTimeLimit = Console.ReadLine();
                        //if (string.IsNullOrEmpty(grantTimeLimit.Trim()))
                        //{
                        //    grantTimeLimitInMinutes = 1440;
                        //}
                        //else
                        //{
                        //    Int32.TryParse(grantTimeLimit, out grantTimeLimitInMinutes);
                        //    if (grantTimeLimitInMinutes < 0) grantTimeLimitInMinutes = 1440;
                        //}

                        //Console.ForegroundColor = ConsoleColor.Blue;
                        //StringBuilder pamGrantStringBuilder = new StringBuilder();
                        //pamGrantStringBuilder.AppendLine(string.Format("Channel = {0}", channel));
                        //pamGrantStringBuilder.AppendLine(string.Format("ChannelGroup = {0}", channelGroup));
                        //pamGrantStringBuilder.AppendLine(string.Format("auth_key = {0}", authGrant));
                        //pamGrantStringBuilder.AppendLine(string.Format("Read Access = {0}", read.ToString()));
                        //if (channel.Trim().Length > 0)
                        //{
                        //    pamGrantStringBuilder.AppendLine(string.Format("Write Access = {0}", write.ToString()));
                        //    pamGrantStringBuilder.AppendLine(string.Format("Delete Access = {0}", delete.ToString()));
                        //}
                        //if (channelGroup.Trim().Length > 0)
                        //{
                        //    pamGrantStringBuilder.AppendLine(string.Format("Manage Access = {0}", manage.ToString()));
                        //}
                        //pamGrantStringBuilder.AppendLine(string.Format("Grant Access Time Limit = {0}", grantTimeLimitInMinutes.ToString()));
                        //Console.WriteLine(pamGrantStringBuilder.ToString());
                        //Console.ResetColor();
                        //Console.WriteLine();

                        //Console.WriteLine("Running PamGrant()");

                        //pubnub.Grant()
                        //    .Channels(channelList)
                        //    .ChannelGroups(channelGroupList)
                        //    .AuthKeys(authKeyList)
                        //    .Read(read)
                        //    .Write(write)
                        //    .Delete(delete)
                        //    .Manage(manage)
                        //    .TTL(grantTimeLimitInMinutes)
                        //    .Execute(new PNAccessManagerGrantResultExt(
                        //        (r, s) =>
                        //        {
                        //            if (r != null)
                        //            {
                        //                Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                        //            }
                        //            else if (s != null)
                        //            {
                        //                Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(s));
                        //            }
                        //        })
                        //        );
                        break;
                    case "9":
                        Console.WriteLine("Enter CHANNEL name for PAM Audit" + Environment.NewLine + "To enter CHANNEL GROUP name, just hit ENTER");
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

                        StringBuilder pamAuditStringBuilder = new StringBuilder();
                        pamAuditStringBuilder.AppendLine();
                        pamAuditStringBuilder.AppendLine(string.Format("Channel = {0}", channel));
                        pamAuditStringBuilder.AppendLine(string.Format("ChannelGroup = {0}", channelGroup));
                        Console.WriteLine(pamAuditStringBuilder.ToString());
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Enter the auth_key for PAM Audit (optional)" + Environment.NewLine + "Press Enter Key if there is no auth_key at this time.");
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
                            .Execute(new PNAccessManagerAuditResultExt(
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

                        StringBuilder pamRevokeStringBuilder = new StringBuilder();
                        pamRevokeStringBuilder.AppendLine(string.Format("Channel = {0}", channel));
                        pamRevokeStringBuilder.AppendLine(string.Format("ChannelGroup = {0}", channelGroup));
                        Console.WriteLine(pamRevokeStringBuilder.ToString());
                        Console.ResetColor();
                        Console.WriteLine();

                        string[] channelList2 = channel.Split(',');
                        string[] channelGroupList2 = channelGroup.Split(',');

                        Console.WriteLine("Enter the auth_key for PAM Revoke (optional)" + Environment.NewLine + "Press Enter Key if there is no auth_key at this time.");
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
                            .Execute(new PNAccessManagerGrantResultExt(
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
                        Console.WriteLine("Enter channel name" + Environment.NewLine + "NOTE: If you want to consider only Channel Group, just hit ENTER");
                        string userStateChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", userStateChannel));
                        Console.ResetColor();

                        Console.WriteLine("Enter channel group name" + Environment.NewLine + "NOTE: If you want to consider only Channel, just hit ENTER");
                        string userStateChannelGroup = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("ChannelGroup = {0}", userStateChannelGroup));
                        Console.ResetColor();

                        Console.WriteLine("User State will be accepted as dictionary key:value pair" + Environment.NewLine + "Enter key. ");
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
                            .Execute(new PNSetStateResultExt(
                                (r, s) => {
                                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                }));

                        break;
                    case "14":
                        Console.WriteLine("Enter channel name" + Environment.NewLine + "NOTE: If you want to consider only Channel Group, just hit ENTER");
                        string deleteChannelUserState = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", deleteChannelUserState));
                        Console.ResetColor();

                        Console.WriteLine("Enter channel group name" + Environment.NewLine + "NOTE: If you want to consider only Channel, just hit ENTER");
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
                            .Execute(new PNSetStateResultExt(
                                (r, s) => {
                                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                }));

                        break;
                    case "15":
                        Console.WriteLine("Enter channel name" + Environment.NewLine + "NOTE: If you want to consider only Channel Group, just hit ENTER");
                        string getUserStateChannel2 = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", getUserStateChannel2));
                        Console.ResetColor();

                        Console.WriteLine("Enter channel group name" + Environment.NewLine + "NOTE: If you want to consider only Channel, just hit ENTER");
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
                            .Execute(new PNGetStateResultExt(
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
                            .Execute(new PNWhereNowResultExt(
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
                        if (pubnub.Disconnect<object>())
                        {
                            Console.WriteLine("Disconnect success");
                        }
                        else
                        {
                            Console.WriteLine("Disconnect failed");
                        }
                        break;
                    case "19":
                        Console.WriteLine("Re-connect");
                        if (pubnub.Reconnect<object>())
                        {
                            Console.WriteLine("Reconnect success");
                        }
                        else
                        {
                            Console.WriteLine("Reconnect failed");
                        }
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
                        string deleteMessageChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", deleteMessageChannel));
                        Console.ResetColor();
                        Console.WriteLine("Enter start timetoken: ");
                        string ttStartDeleteMessage = Console.ReadLine();
                        Console.WriteLine("Enter end timetoken: ");
                        string ttEndDeleteMessage = Console.ReadLine();
                        long deleteStart;
                        long deleteEnd;
                        Int64.TryParse(ttStartDeleteMessage, out deleteStart);
                        Int64.TryParse(ttEndDeleteMessage, out deleteEnd);

                        pubnub.DeleteMessages().Channel(deleteMessageChannel)
                            .Start(deleteStart)
                              .End(deleteEnd)
                            .Execute(new PNDeleteMessageResultExt(
                                (r, s) => {
                                    if (s != null && s.Error)
                                    {
                                        Console.WriteLine(s.ErrorData.Information);
                                    }
                                    else
                                    {
                                        Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    }
                                }));

                        break;
                    case "24":
                        Console.WriteLine("Enter channel name: ");
                        string channelMsgCount = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channelMsgCount));
                        Console.ResetColor();
                        Console.WriteLine("Enter timetoken: ");
                        string ttMessageCount = Console.ReadLine();
                        string[] arrTTMsgCount = ttMessageCount.Split(',');
                        List<long> lstTTMsgCount = new List<long>(arrTTMsgCount.Length);
                        for (int index=0; index < arrTTMsgCount.Length; index++)
                        {
                            long outTT = 0;
                            if (Int64.TryParse(arrTTMsgCount[index], out outTT))
                            {
                                lstTTMsgCount.Add(outTT);
                            }
                        }
                        Console.ForegroundColor = ConsoleColor.Blue;
                        long channelsTT;
                        long.TryParse(ttMessageCount, out channelsTT);
                        Console.WriteLine(string.Format("Timetoken = {0}", ttMessageCount));
                        Console.ResetColor();
                        pubnub.MessageCounts().Channels(channelMsgCount.Split(','))
                            .ChannelsTimetoken(lstTTMsgCount.ToArray())
                            .Execute(new PNMessageCountResultExt(
                                (r, s) => {
                                    if (s != null && s.Error)
                                    {
                                        Console.WriteLine(s.ErrorData.Information);
                                    }
                                    else
                                    {
                                        Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    }
                                }));
                        break;
                    case "31":
                        Console.WriteLine("Enter channel name");
                        string pushRegisterChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", pushRegisterChannel));
                        Console.ResetColor();

                        Console.WriteLine("Enter Push Token for APNS");
                        string pushToken = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Push Token = {0}", pushToken));
                        Console.ResetColor();

                        Console.WriteLine("Running AddPushNotificationsOnChannels()");
                        pubnub.AddPushNotificationsOnChannels().Channels(new string[] { pushRegisterChannel })
                            .PushType(PNPushType.APNS)
                            .DeviceId(pushToken)
                            .Execute(new PNPushAddChannelResultExt(
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

                        Console.WriteLine("Enter Push Token for APNS");
                        string pushTokenRemove = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Push Token = {0}", pushTokenRemove));
                        Console.ResetColor();

                        Console.WriteLine("Running RemovePushNotificationsFromChannels()");
                        pubnub.RemovePushNotificationsFromChannels()
                            .Channels(new string[] { pushRemoveChannel })
                            .PushType(PNPushType.APNS)
                            .DeviceId(pushTokenRemove)
                            .Execute(new PNPushRemoveChannelResultExt(
                                (r, s) => {
                                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                }));
                        break;
                    case "33":
                        Console.WriteLine("Enter Push Token for APNS");
                        string pushTokenGetChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Push Token = {0}", pushTokenGetChannel));
                        Console.ResetColor();

                        Console.WriteLine("Running AuditPushChannelProvisions()");
                        pubnub.AuditPushChannelProvisions()
                            .PushType(PNPushType.APNS)
                            .DeviceId(pushTokenGetChannel)
                            .Execute(new PNPushListProvisionsResultExt(
                                (r, s) => {
                                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                }));
                        break;
                    case "34":
                        Console.WriteLine("Enter Push Token for APNS");
                        string pushTokenUnregisterDevice = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Push Token = {0}", pushTokenUnregisterDevice));
                        Console.ResetColor();

                        Console.WriteLine("Running RemoveAllPushNotificationsFromDeviceWithPushToken()");
                        pubnub.RemoveAllPushNotificationsFromDeviceWithPushToken()
                            .PushType(PNPushType.APNS)
                            .DeviceId(pushTokenUnregisterDevice)
                            .Execute(new PNPushRemoveAllChannelsResultExt(
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
                            .Execute(new PNChannelGroupsAddChannelResultExt(
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
                                .Execute(new PNChannelGroupsDeleteGroupResultExt(
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
                            .Execute(new PNChannelGroupsRemoveChannelResultExt(
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
                                .Execute(new PNChannelGroupsListAllResultExt(
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
                            .Execute(new PNChannelGroupsAllChannelsResultExt(
                                (r, s) => {
                                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                }));
                        break;
                    case "41":
                        Console.WriteLine("Enter CHANNEL name for signal.");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.ResetColor();

                        if (channel == "")
                        {
                            Console.WriteLine("Invalid CHANNEL name");
                            break;
                        }

                        Console.WriteLine("Enter the message for signal and press ENTER key to submit");
                        string signalMsg = Console.ReadLine();

                        Console.WriteLine("Running signal()");

                        double doubleDataSignal;
                        int intDataSignal;
                        if (int.TryParse(signalMsg, out intDataSignal)) //capture numeric data
                        {
                            pubnub.Signal().Channel(channel).Message(intDataSignal)
                                .Execute(new PNPublishResultExt((r, s) => { if (s.Error) { Console.WriteLine(s.ErrorData.Information); } else { Console.WriteLine(r.Timetoken); } }));
                        }
                        else if (double.TryParse(signalMsg, out doubleDataSignal)) //capture numeric data
                        {
                            pubnub.Signal().Channel(channel).Message(doubleDataSignal)
                                .Execute(new PNPublishResultExt((r, s) => { if (s.Error) { Console.WriteLine(s.ErrorData.Information); } else { Console.WriteLine(r.Timetoken); } }));
                        }
                        else
                        {
                            //check whether any numeric is sent in double quotes
                            if (signalMsg.IndexOf("\"") == 0 && signalMsg.LastIndexOf("\"") == signalMsg.Length - 1)
                            {
                                string strMsg = signalMsg.Substring(1, signalMsg.Length - 2);
                                if (int.TryParse(strMsg, out intDataSignal))
                                {
                                    pubnub.Signal().Channel(channel).Message(strMsg)
                                        .Execute(new PNPublishResultExt((r, s) => { if (s.Error) { Console.WriteLine(s.ErrorData.Information); } else { Console.WriteLine(r.Timetoken); } }));
                                }
                                else if (double.TryParse(strMsg, out doubleDataSignal))
                                {
                                    pubnub.Signal().Channel(channel).Message(strMsg)
                                        .Execute(new PNPublishResultExt((r, s) => { if (s.Error) { Console.WriteLine(s.ErrorData.Information); } else { Console.WriteLine(r.Timetoken); } }));
                                }
                                else
                                {
                                    pubnub.Signal().Channel(channel).Message(signalMsg)
                                        .Execute(new PNPublishResultExt((r, s) => { if (s.Error) { Console.WriteLine(s.ErrorData.Information); } else { Console.WriteLine(r.Timetoken); } }));
                                }
                            }
                            else
                            {
                                pubnub.Signal()
                                    .Channel(channel)
                                    .Message(signalMsg)
                                    .Execute(new PNPublishResultExt((r, s) => { if (s.Error) { Console.WriteLine(s.ErrorData.Information); } else { Console.WriteLine(r.Timetoken); } }));
                            }
                        }
                        break;
                    case "42":
                        Console.WriteLine("Enter User Id to create User");
                        string createUserId = Console.ReadLine();
                        Console.WriteLine("Enter User Name to create User");
                        string createUserName = Console.ReadLine();

                        if (string.IsNullOrEmpty(createUserId))
                        {
                            Console.WriteLine("Invalid UserId");
                        }
                        else
                        {
                            pubnub.CreateUser()
                                    .Id(createUserId)
                                    .Name(createUserName)
                                    .Execute(new PNCreateUserResultExt((r, s) => {
                                        if (s.Error) { Console.WriteLine(s.ErrorData.Information); }
                                        else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)); }
                                    }
                                    ));
                        }
                        break;
                    case "43":
                        Console.WriteLine("Enter User Id to retrieve User, else press ENTER.");
                        string singleUserId = Console.ReadLine();

                        if (!string.IsNullOrWhiteSpace(singleUserId))
                        {
                            pubnub.GetUser()
                                .UserId(singleUserId)
                                .IncludeCustom(true)
                                .Execute(new PNGetUserResultExt((r, s) => {
                                    if (s.Error) { Console.WriteLine(s.ErrorData.Information); }
                                    else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)); }
                                }
                                ));
                        }
                        else
                        {
                            pubnub.GetUsers()
                                .IncludeCount(true)
                                .IncludeCustom(true)
                                .Page(new PNPage() { Next = "", Prev = "" })
                                .Execute(new PNGetUsersResultExt((r, s) => {
                                    if (s.Error) { Console.WriteLine(s.ErrorData.Information); }
                                    else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)); }
                                }
                                ));
                        }
                        break;
                    case "44":
                        Console.WriteLine("Enter User Id to delete User.");
                        string deleteUserId = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(deleteUserId))
                        {
                            pubnub.DeleteUser()
                                .Id(deleteUserId)
                                .Execute(new PNDeleteUserResultExt((r, s) => {
                                    if (s.Error) { Console.WriteLine(s.ErrorData.Information); }
                                    else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)); }
                                }
                                ));
                        }
                        else
                        {
                            Console.WriteLine("Invalid User Id. Try again.");
                        }
                        break;
                    case "45":
                        Console.WriteLine("Enter User Id to update User");
                        string updUserId = Console.ReadLine();
                        Console.WriteLine("Enter User Name to update User");
                        string updUserName = Console.ReadLine();

                        if (string.IsNullOrEmpty(updUserId))
                        {
                            Console.WriteLine("Invalid UserId");
                        }
                        else
                        {
                            pubnub.UpdateUser()
                                .Id(updUserId)
                                .Name(updUserName)
                                .Execute(new PNUpdateUserResultExt((r, s) => {
                                    if (s.Error) { Console.WriteLine(s.ErrorData.Information); }
                                    else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)); }
                                }
                                ));
                        }
                        break;
                    case "46":
                        Console.WriteLine("Enter SpaceId to create Space");
                        string createSpaceId = Console.ReadLine();
                        Console.WriteLine("Enter Space Name to create Space");
                        string createSpaceName = Console.ReadLine();

                        if (string.IsNullOrEmpty(createSpaceId))
                        {
                            Console.WriteLine("Invalid SpaceId");
                        }
                        else
                        {
                            pubnub.CreateSpace()
                                .Id(createSpaceId)
                                .Name(createSpaceName)
                                .Execute(new PNCreateSpaceResultExt((r, s) =>
                                {
                                    if (s.Error) { Console.WriteLine(s.ErrorData.Information); }
                                    else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)); }
                                }
                                ));
                        }
                        break;
                    case "47":
                        Console.WriteLine("Enter Space Id to retrieve specific Space, else press ENTER.");
                        string singleSpaceId = Console.ReadLine();

                        if (!string.IsNullOrWhiteSpace(singleSpaceId))
                        {
                            pubnub.GetSpace()
                                .SpaceId(singleSpaceId)
                                .IncludeCustom(true)
                                .Execute(new PNGetSpaceResultExt((r, s) => {
                                    if (s.Error) { Console.WriteLine(s.ErrorData.Information); }
                                    else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)); }
                                }
                                ));
                        }
                        else
                        {
                            pubnub.GetSpaces()
                                .IncludeCount(true)
                                .IncludeCustom(true)
                                //.Limit(2)
                                .Page(new PNPage() { Next = "", Prev = "" })
                                .Execute(new PNGetSpacesResultExt((r, s) => {
                                    if (s.Error) { Console.WriteLine(s.ErrorData.Information); }
                                    else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)); }
                                }
                                ));
                        }
                        break;
                    case "48":
                        Console.WriteLine("Enter Space Id to delete Space.");
                        string deleteSpaceId = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(deleteSpaceId))
                        {
                            pubnub.DeleteSpace()
                                .Id(deleteSpaceId)
                                .Execute(new PNDeleteSpaceResultExt((r, s) => {
                                    if (s.Error) { Console.WriteLine(s.ErrorData.Information); }
                                    else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)); }
                                }
                                ));
                        }
                        else
                        {
                            Console.WriteLine("Invalid Space Id. Try again.");
                        }
                        break;
                    case "49":
                        Console.WriteLine("Enter SpaceId to update Space");
                        string updSpaceId = Console.ReadLine();
                        Console.WriteLine("Enter Space Name to update Space");
                        string updSpaceName = Console.ReadLine();
                        Console.WriteLine("Enter Space Description");
                        string updSpaceDesc = Console.ReadLine();

                        if (string.IsNullOrEmpty(updSpaceId))
                        {
                            Console.WriteLine("Invalid SpaceId");
                        }
                        else
                        {
                            pubnub.UpdateSpace()
                                .Id(updSpaceId)
                                .Name(updSpaceName)
                                .Description(updSpaceDesc)
                                .Execute(new PNUpdateSpaceResultExt((r, s) => {
                                    if (s.Error) { Console.WriteLine(s.ErrorData.Information); }
                                    else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)); }
                                }
                                ));
                        }
                        break;
                    case "50":
                        Console.WriteLine("Enter UserID");
                        string memshipUserId = Console.ReadLine();
                        Console.WriteLine("Enter SpaceId to add");
                        string memshipAddSpaceId = Console.ReadLine();
                        Console.WriteLine("Enter SpaceId to remove");
                        string memshipRemSpaceId = Console.ReadLine();

                        List<PNMembership> addList = new List<PNMembership>();
                        if (!string.IsNullOrEmpty(memshipAddSpaceId))
                        {
                            addList.Add(new PNMembership() { SpaceId = memshipAddSpaceId });
                        }

                        List<PNMembership> updateList = new List<PNMembership>();

                        List<string> removeList = new List<string>();
                        if (!string.IsNullOrEmpty(memshipRemSpaceId))
                        {
                            removeList.Add(memshipRemSpaceId);
                        }

                        pubnub.ManageMemberships()
                            .UserId(memshipUserId)
                            .Add(addList)
                            .Update(updateList)
                            .Remove(removeList)
                            .Include(new PNMembershipField[] { PNMembershipField.CUSTOM, PNMembershipField.SPACE, PNMembershipField.SPACE_CUSTOM })
                            .IncludeCount(true)
                            .Page(new PNPage() { Next = "", Prev = "" })
                            .Execute(new PNManageMembershipsResultExt((r, s) => {
                                if (s.Error) { Console.WriteLine(s.ErrorData.Information); }
                                else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)); }
                            }
                            ));
                        break;
                    case "51":
                        Console.WriteLine("Enter SpaceId");
                        string memberSpaceId = Console.ReadLine();
                        Console.WriteLine("Enter UserId to add");
                        string meberAddUserId = Console.ReadLine();
                        Console.WriteLine("Enter UserId to remove");
                        string memberRemUserId = Console.ReadLine();

                        List<PNMember> addMemberList = new List<PNMember>();
                        if (!string.IsNullOrEmpty(meberAddUserId))
                        {
                            addMemberList.Add(new PNMember() { UserId = meberAddUserId });
                        }

                        List<PNMember> updMemberList = new List<PNMember>();

                        List<string> remMemberList = new List<string>();
                        if (!string.IsNullOrEmpty(memberRemUserId))
                        {
                            remMemberList.Add(memberRemUserId);
                        }

                        if (string.IsNullOrEmpty(memberSpaceId))
                        {
                            Console.WriteLine("Invalid SpaceId");
                        }
                        else
                        {
                            pubnub.ManageMembers()
                                .SpaceId(memberSpaceId)
                                .Add(addMemberList)
                                .Update(updMemberList)
                                .Remove(remMemberList)
                                .Include(new PNMemberField[] { PNMemberField.CUSTOM, PNMemberField.USER, PNMemberField.USER_CUSTOM })
                                .IncludeCount(true)
                                .Page(new PNPage() { Next = "", Prev = "" })
                                .Execute(new PNManageMembersResultExt((r, s) => {
                                    if (s.Error) { Console.WriteLine(s.ErrorData.Information); }
                                    else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)); }
                                }
                                ));
                        }
                        break;
                    case "52":
                        Console.WriteLine("Enter User Id to Get Memberships.");
                        string getMembershipsUserId = Console.ReadLine();
                        if (!string.IsNullOrEmpty(getMembershipsUserId))
                        {
                            pubnub.GetMemberships()
                                .UserId(getMembershipsUserId)
                                .Include(new PNMembershipField[] { PNMembershipField.CUSTOM, PNMembershipField.SPACE, PNMembershipField.SPACE_CUSTOM })
                                .IncludeCount(true)
                                .Page(new PNPage() { Next = "", Prev = "" })
                                .Execute(new PNGetMembershipsResultExt((r, s) => {
                                    if (s.Error) { Console.WriteLine(s.ErrorData.Information); }
                                    else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)); }
                                }
                                ));
                        }
                        else
                        {
                            Console.WriteLine("Invalid UserId!!!");
                        }
                        break;
                    case "53":
                        Console.WriteLine("Enter Space Id to Get Members.");
                        string getMembersSpaceId = Console.ReadLine();
                        if (!string.IsNullOrEmpty(getMembersSpaceId))
                        {
                            pubnub.GetMembers()
                                .SpaceId(getMembersSpaceId)
                                .Include(new PNMemberField[] { PNMemberField.CUSTOM, PNMemberField.USER, PNMemberField.USER_CUSTOM })
                                .IncludeCount(true)
                                .Page(new PNPage() { Next = "", Prev = "" })
                                .Execute(new PNGetMembersResultExt((r, s) => {
                                    if (s.Error) { Console.WriteLine(s.ErrorData.Information); }
                                    else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r)); }
                                }
                                ));
                        }
                        else
                        {
                            Console.WriteLine("Invalid SpaceId!!!");
                        }
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

}
