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
using System.Threading.Tasks;

namespace PubnubApiAsyncAwaitDemo
{
    public class PubnubAsyncAwaitExample
    {
        static private Pubnub pubnub { get; set; }

        static private string authKey { get; set; } = "";

        static private string grantToken { get; set; } = "";

        public class PlatformPubnubLog : IPubnubLog
        {
            public PlatformPubnubLog()
            {
                // Get folder path may vary based on environment
                string folder = System.IO.Directory.GetCurrentDirectory(); //For console
                //string folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments); // For iOS
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
            System.Environment.Exit(1);
        }

        static public void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }
        static public async Task MainAsync()
        {
            string EnvPublishKey = System.Environment.GetEnvironmentVariable("PN_PUB_KEY", EnvironmentVariableTarget.Machine);
            string EnvSubscribeKey = System.Environment.GetEnvironmentVariable("PN_SUB_KEY", EnvironmentVariableTarget.Machine);
            string EnvSecretKey = System.Environment.GetEnvironmentVariable("PN_SEC_KEY", EnvironmentVariableTarget.Machine);


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

            Console.WriteLine("ENTER cipher key for encryption feature." + System.Environment.NewLine + "If you don't want to avail at this time, press ENTER.");
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

            Console.WriteLine("ENTER subscribe key." + System.Environment.NewLine + "If you want to accept default demo subscribe key, press ENTER.");
            string subscribeKey = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (subscribeKey.Trim().Length > 0)
            {
                Console.WriteLine("Subscribe key provided.");
            }
            else
            {
                Console.WriteLine("Default demo subscribe key provided");
                subscribeKey = string.IsNullOrEmpty(EnvSubscribeKey) ? "demo-36" : EnvSubscribeKey;
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("ENTER publish key." + System.Environment.NewLine + "If you want to accept default demo publish key, press ENTER.");
            string publishKey = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (publishKey.Trim().Length > 0)
            {
                Console.WriteLine("Publish key provided.");
            }
            else
            {
                Console.WriteLine("Default demo publish key provided");
                publishKey = string.IsNullOrEmpty(EnvPublishKey) ? "demo-36" : EnvPublishKey;
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("ENTER secret key." + System.Environment.NewLine + "If you don't want to avail at this time, press ENTER.");
            string secretKey = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (secretKey.Trim().Length > 0)
            {
                Console.WriteLine("Secret key provided.");
            }
            else
            {
                Console.WriteLine("Default demo Secret key provided");
                secretKey = string.IsNullOrEmpty(EnvSecretKey) ? "demo-36" : EnvSecretKey;
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

            Console.WriteLine("Enable Internal Logging? Enter Y for Yes, Else N for No." + System.Environment.NewLine + "Default = Y  ");
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

            config.Secure = (enableSSL.Trim().ToLower() == "y");
            config.CipherKey = cipherKey;
            config.SubscribeKey = subscribeKey;
            config.PublishKey = publishKey;
            config.SecretKey = secretKey;
            config.ReconnectionPolicy = PNReconnectionPolicy.LINEAR;
            //config.UseClassicHttpWebRequest = true;
            //config.FilterExpression = "uuid == '" + config.Uuid +  "'";
            config.EnableTelemetry = false;
            config.IncludeRequestIdentifier = false;
            config.IncludeInstanceIdentifier = false;

            pubnub = new Pubnub(config);

            //Add listener to receive published messages and presence events
            pubnub.AddListener(new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNMessageResult<object> pubMsg)
                {
                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(pubMsg));
                    var channelName = pubMsg.Channel;
                    var channelGroupName = pubMsg.Subscription;
                    var pubTT = pubMsg.Timetoken;
                    var msg = pubMsg.Message;
                    var publisher = pubMsg.Publisher;
                },
                delegate (Pubnub pnObj, PNPresenceEventResult presenceEvnt)
                {
                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(presenceEvnt));
                    var action = presenceEvnt.Event; // Can be join, leave, state-change or timeout
                    var channelName = presenceEvnt.Channel; // The channel for which the message belongs
                    var occupancy = presenceEvnt.Occupancy; // No. of users connected with the channel
                    var state = presenceEvnt.State; // User State
                    var channelGroupName = presenceEvnt.Subscription; //  The channel group or wildcard subscription match (if exists)
                    var publishTime = presenceEvnt.Timestamp; // Publish timetoken
                    var timetoken = presenceEvnt.Timetoken;  // Current timetoken
                    var uuid = presenceEvnt.Uuid; // UUIDs of users who are connected with the channel
                },
                delegate (Pubnub pnObj, PNSignalResult<object> signalMsg)
                {
                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(signalMsg));
                    var channelName = signalMsg.Channel; // The channel for which the signal belongs
                    var channelGroupName = signalMsg.Subscription; // The channel group or wildcard subscription match (if exists)
                    var pubTT = signalMsg.Timetoken; // Publish timetoken
                    var msg = signalMsg.Message; // The Payload
                    var publisher = signalMsg.Publisher; //The Publisher
                },
                delegate (Pubnub pnObj, PNObjectEventResult objectEventObj)
                {
                    var channelName = objectEventObj.Channel; // Channel
                    var channelMetadata = objectEventObj.ChannelMetadata; //Channel Metadata
                    var uidMetadata = objectEventObj.UuidMetadata; // UUID metadata
                    var evnt = objectEventObj.Event; // Event
                    var type = objectEventObj.Type; // Event type

                    if (objectEventObj.Type == "uuid") { /* got uuid metadata related event. */ }
                    else if (objectEventObj.Type == "channel") { /* got channel metadata related event. */ }
                    else if (objectEventObj.Type == "membership") { /* got membership related event. */ }
                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(objectEventObj));
                },
                delegate (Pubnub pnObj, PNMessageActionEventResult msgActionEvent)
                {
                    //handle message action
                    var channelName = msgActionEvent.Channel; // The channel for which the message belongs
                    var msgEvent = msgActionEvent.Action; // message action added or removed
                    var msgActionType = msgActionEvent.Event; // message action type
                    var messageTimetoken = msgActionEvent.MessageTimetoken; // The timetoken of the original message
                    var actionTimetoken = msgActionEvent.ActionTimetoken; //The timetoken of the message action
                },
                delegate (Pubnub pnObj, PNFileEventResult fileEvent)
                {
                    //handle file message event
                    var channelName = fileEvent.Channel;
                    var chanelGroupName = fileEvent.Subscription;
                    var fieldId = fileEvent.FileId;
                    var fileName = fileEvent.FileName;
                    var fileMessage = fileEvent.Message;
                    var filePublisher = fileEvent.Publisher;
                    var filePubTT = fileEvent.Timetoken;
                },
                delegate (Pubnub pnObj, PNStatus pnStatus)
                {
                    Console.WriteLine("{0} {1} {2}", pnStatus.Operation, pnStatus.Category, pnStatus.StatusCode);
                    var affectedChannelGroups = pnStatus.AffectedChannelGroups; // The channel groups affected in the operation, of type array.
                    var affectedChannels = pnStatus.AffectedChannels; // The channels affected in the operation, of type array.
                    var category = pnStatus.Category; //Returns PNConnectedCategory
                    var operation = pnStatus.Operation; //Returns PNSubscribeOperation
                }
                ));

            bool exitFlag = false;
            string channel = "";
            string channelGroup = "";
            int currentUserChoice = 0;
            string userinput = "";
            Console.WriteLine("");
            while (!exitFlag)
            {
                if (currentUserChoice < 1 || (currentUserChoice > 64 && currentUserChoice != 99))
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
                    menuOptionsStringBuilder.AppendLine("Enter 25 FOR Fetch Messages");
                    menuOptionsStringBuilder.AppendLine("Enter 31 FOR Push - Register Device");
                    menuOptionsStringBuilder.AppendLine("Enter 32 FOR Push - Remove Channel");
                    menuOptionsStringBuilder.AppendLine("Enter 33 FOR Push - Get Current Channels");
                    menuOptionsStringBuilder.AppendLine("Enter 34 FOR Push - Unregister Device");
                    menuOptionsStringBuilder.AppendLine("Enter 38 FOR Channel Group - Add channel(s)");
                    menuOptionsStringBuilder.AppendLine("Enter 39 FOR Channel Group - Remove channel/group/namespace");
                    menuOptionsStringBuilder.AppendLine("Enter 40 FOR Channel Group - Get channel(s)/namespace(s)");
                    menuOptionsStringBuilder.AppendLine("Enter 41 FOR Signal");
                    menuOptionsStringBuilder.AppendLine("Enter 42 FOR Set UUID Metadata");
                    menuOptionsStringBuilder.AppendLine("Enter 43 FOR Get UUID(s) Metadata");
                    menuOptionsStringBuilder.AppendLine("Enter 44 FOR Remove UUID");
                    menuOptionsStringBuilder.AppendLine("Enter 45 FOR Set Channel Metadata");
                    menuOptionsStringBuilder.AppendLine("Enter 46 FOR Get Channel(s) Metadata");
                    menuOptionsStringBuilder.AppendLine("Enter 47 FOR Remove Channel Metadata");
                    menuOptionsStringBuilder.AppendLine("Enter 48 FOR Manage Memberships");
                    menuOptionsStringBuilder.AppendLine("Enter 49 FOR Manage Members");
                    menuOptionsStringBuilder.AppendLine("Enter 50 FOR Get Memberships");
                    menuOptionsStringBuilder.AppendLine("Enter 51 FOR Get Members");
                    menuOptionsStringBuilder.AppendLine("Enter 52 FOR Set Memberships");
                    menuOptionsStringBuilder.AppendLine("Enter 53 FOR Remove Memberships");
                    menuOptionsStringBuilder.AppendLine("Enter 54 FOR Set Channel Members");
                    menuOptionsStringBuilder.AppendLine("Enter 55 FOR Remove Channel Members");
                    menuOptionsStringBuilder.AppendLine("Enter 56 FOR Add MessageAction");
                    menuOptionsStringBuilder.AppendLine("Enter 57 FOR Remove MessageAction");
                    menuOptionsStringBuilder.AppendLine("Enter 58 FOR Get MessageActions");
                    menuOptionsStringBuilder.AppendLine("Enter 59 FOR SendFile");
                    menuOptionsStringBuilder.AppendLine("Enter 60 FOR DownloadFile");
                    menuOptionsStringBuilder.AppendLine("Enter 61 FOR FileUrl");
                    menuOptionsStringBuilder.AppendLine("Enter 62 FOR DeleteFile");
                    menuOptionsStringBuilder.AppendLine("Enter 63 FOR PublishFileMessage");
                    menuOptionsStringBuilder.AppendLine("Enter 64 FOR ListAllFiles");

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
                        Console.WriteLine("Enter CHANNEL name for subscribe. Use comma to enter multiple channels." + System.Environment.NewLine + "NOTE: If you want to consider only Channel Group(s), just hit ENTER");
                        channel = Console.ReadLine();

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
                        PNResult<PNPublishResult> resp;
                        //resp = await pubnub.Publish()
                        //    .Channel(channel)
                        //    .Message(publishMsg)
                        //    .Meta(meta)
                        //    .ShouldStore(store).UsePOST(usePost)
                        //    .ExecuteAsync();
                        //if (resp.Result != null)
                        //{
                        //    Console.WriteLine(resp.Result.Timetoken);
                        //}


                        double doubleData;
                        int intData;
                        if (int.TryParse(publishMsg, out intData)) //capture numeric data
                        {
                            resp = await pubnub.Publish().Channel(channel).Message(intData).Meta(meta).ShouldStore(store).UsePOST(usePost)
                                .ExecuteAsync();
                            if (resp.Status.Error) { Console.WriteLine(resp.Status.ErrorData.Information); } else { Console.WriteLine(resp.Result.Timetoken); }
                        }
                        else if (double.TryParse(publishMsg, out doubleData)) //capture numeric data
                        {
                            resp = await pubnub.Publish().Channel(channel).Message(doubleData).Meta(meta).ShouldStore(store).UsePOST(usePost)
                                .ExecuteAsync();
                            if (resp.Status.Error) { Console.WriteLine(resp.Status.ErrorData.Information); } else { Console.WriteLine(resp.Result.Timetoken); }
                        }
                        else
                        {
                            //check whether any numeric is sent in double quotes
                            if (publishMsg.IndexOf("\"") == 0 && publishMsg.LastIndexOf("\"") == publishMsg.Length - 1)
                            {
                                string strMsg = publishMsg.Substring(1, publishMsg.Length - 2);
                                if (int.TryParse(strMsg, out intData))
                                {
                                    resp = await pubnub.Publish().Channel(channel).Message(intData).Meta(meta).ShouldStore(store).UsePOST(usePost)
                                        .ExecuteAsync();
                                    if (resp.Status.Error) { Console.WriteLine(resp.Status.ErrorData.Information); } else { Console.WriteLine(resp.Result.Timetoken); }
                                }
                                else if (double.TryParse(strMsg, out doubleData))
                                {
                                    resp = await pubnub.Publish().Channel(channel).Message(doubleData).Meta(meta).ShouldStore(store).UsePOST(usePost)
                                        .ExecuteAsync();
                                    if (resp.Status.Error) { Console.WriteLine(resp.Status.ErrorData.Information); } else { Console.WriteLine(resp.Result.Timetoken); }
                                }
                                else
                                {
                                    resp = await pubnub.Publish().Channel(channel).Message(publishMsg).Meta(meta).ShouldStore(store).UsePOST(usePost)
                                        .ExecuteAsync();
                                    if (resp.Status.Error) { Console.WriteLine(resp.Status.ErrorData.Information); } else { Console.WriteLine(resp.Result.Timetoken); }
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
                                    resp = await pubnub.Publish()
                                        .Channel(channel)
                                        .Message(publishMsg)
                                        .Meta(meta)
                                        .ShouldStore(store)
                                        .UsePOST(usePost)
                                        .ExecuteAsync();
                                    if (resp.Status.Error) { Console.WriteLine(resp.Status.ErrorData.Information); } else { Console.WriteLine(resp.Result.Timetoken); }
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
                        PNResult<PNHistoryResult> respHistory = await pubnub.History()
                            .Channel(channel)
                            .Reverse(false)
                            .Count(100)
                            .IncludeTimetoken(true)
                            .IncludeMeta(true)
                            .ExecuteAsync();
                        if (respHistory.Result != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respHistory.Result));
                        }
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

                        Console.WriteLine("Enter channel group name" + System.Environment.NewLine + "NOTE: If you want to consider only Channel, just hit ENTER");
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
                        PNResult<PNHereNowResult> respHerenow = await pubnub.HereNow()
                            .Channels(channel.Split(','))
                            .ChannelGroups(channelGroup.Split(','))
                            .IncludeUUIDs(showUUID)
                            .IncludeState(includeUserState)
                            .ExecuteAsync();
                        if (respHerenow.Result != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respHerenow.Result));
                        }
                        break;
                    case "5":
                        Console.WriteLine("Enter CHANNEL name for Unsubscribe. Use comma to enter multiple channels." + System.Environment.NewLine + "NOTE: If you want to consider only Channel Group, just hit ENTER");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Enter channel group name" + System.Environment.NewLine + "NOTE: If you want to consider only Channel, just hit ENTER");
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
                        
                        PNResult<PNTimeResult> respTime = await pubnub.Time().ExecuteAsync();
                        if (respTime.Result != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respTime.Result));
                        }
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

                        Console.WriteLine("Enter the auth_key for PAM Grant (optional)" + System.Environment.NewLine + "Press Enter Key if there is no auth_key at this time.");
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

                        bool delete = false;
                        if (channel.Trim().Length > 0)
                        {
                            Console.WriteLine("Delete Access? Enter Y for Yes (default), N for No.");
                            string deleteAccess = Console.ReadLine();
                            delete = (deleteAccess.ToLower() == "n") ? false : true;
                        }

                        bool manage = false;
                        if (channelGroup.Trim().Length > 0)
                        {
                            Console.WriteLine("Manage Access? Enter Y for Yes (default), N for No.");
                            string manageAccess = Console.ReadLine();
                            manage = (manageAccess.ToLower() == "n") ? false : true;
                        }
                        Console.WriteLine("How many minutes do you want to allow Grant Access? Enter the number of minutes." + System.Environment.NewLine + "Default = 1440 minutes (24 hours). Press ENTER now to accept default value.");
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
                        StringBuilder pamGrantStringBuilder = new StringBuilder();
                        pamGrantStringBuilder.AppendLine(string.Format("Channel = {0}", channel));
                        pamGrantStringBuilder.AppendLine(string.Format("ChannelGroup = {0}", channelGroup));
                        pamGrantStringBuilder.AppendLine(string.Format("auth_key = {0}", authGrant));
                        pamGrantStringBuilder.AppendLine(string.Format("Read Access = {0}", read.ToString()));
                        if (channel.Trim().Length > 0)
                        {
                            pamGrantStringBuilder.AppendLine(string.Format("Write Access = {0}", write.ToString()));
                            pamGrantStringBuilder.AppendLine(string.Format("Delete Access = {0}", delete.ToString()));
                        }
                        if (channelGroup.Trim().Length > 0)
                        {
                            pamGrantStringBuilder.AppendLine(string.Format("Manage Access = {0}", manage.ToString()));
                        }
                        pamGrantStringBuilder.AppendLine(string.Format("Grant Access Time Limit = {0}", grantTimeLimitInMinutes.ToString()));
                        Console.WriteLine(pamGrantStringBuilder.ToString());
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running PamGrant()");

                        PNResult<PNAccessManagerGrantResult> respGrant = await pubnub.Grant()
                            .Channels(channelList)
                            .ChannelGroups(channelGroupList)
                            .AuthKeys(authKeyList)
                            .Read(read)
                            .Write(write)
                            .Delete(delete)
                            .Manage(manage)
                            .TTL(grantTimeLimitInMinutes)
                            .ExecuteAsync();
                        if (respGrant.Result != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respGrant.Result));
                        }
                        else if (respGrant.Status != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respGrant.Status));
                        }
                        break;
                    case "9":
                        Console.WriteLine("Enter CHANNEL name for PAM Audit" + System.Environment.NewLine + "To enter CHANNEL GROUP name, just hit ENTER");
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

                        Console.WriteLine("Enter the auth_key for PAM Audit (optional)" + System.Environment.NewLine + "Press Enter Key if there is no auth_key at this time.");
                        string authAudit = Console.ReadLine();
                        string[] authKeyListAudit = authAudit.Split(',');

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("auth_key = {0}", authAudit));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running PamAudit()");

                        PNResult<PNAccessManagerAuditResult> respAudit = await pubnub.Audit()
                            .Channel(channel)
                            .ChannelGroup(channelGroup)
                            .AuthKeys(authKeyListAudit)
                            .ExecuteAsync();
                        if (respAudit.Result != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respAudit.Result));
                        }
                        else if (respAudit.Status != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respAudit.Status));
                        }
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

                        Console.WriteLine("Enter the auth_key for PAM Revoke (optional)" + System.Environment.NewLine + "Press Enter Key if there is no auth_key at this time.");
                        string authRevoke = Console.ReadLine();
                        string[] authKeyList2 = authRevoke.Split(',');

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("auth_key = {0}", authRevoke));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running PamRevoke()");
                        PNResult<PNAccessManagerGrantResult> respGrantRevoke = await pubnub.Grant()
                            .Channels(channelList2)
                            .ChannelGroups(channelGroupList2)
                            .AuthKeys(authKeyList2)
                            .Read(false)
                            .Write(false)
                            .Manage(false)
                            .ExecuteAsync();

                        if (respGrantRevoke.Result != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respGrantRevoke.Result));
                        }
                        else if (respGrantRevoke.Status != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respGrantRevoke.Status));
                        }
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
                        Console.WriteLine("Enter channel name" + System.Environment.NewLine + "NOTE: If you want to consider only Channel Group, just hit ENTER");
                        string userStateChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", userStateChannel));
                        Console.ResetColor();

                        Console.WriteLine("Enter channel group name" + System.Environment.NewLine + "NOTE: If you want to consider only Channel, just hit ENTER");
                        string userStateChannelGroup = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("ChannelGroup = {0}", userStateChannelGroup));
                        Console.ResetColor();

                        Console.WriteLine("User State will be accepted as dictionary key:value pair" + System.Environment.NewLine + "Enter key. ");
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
                        PNResult<PNSetStateResult> respSetState = await pubnub.SetPresenceState()
                            .Channels(userStateChannel.Split(','))
                            .ChannelGroups(userStateChannelGroup.Split(','))
                            .State(addOrModifystate)
                            .ExecuteAsync();
                        if (respSetState.Result != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respSetState.Result));
                        }
                        break;
                    case "14":
                        Console.WriteLine("Enter channel name" + System.Environment.NewLine + "NOTE: If you want to consider only Channel Group, just hit ENTER");
                        string deleteChannelUserState = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", deleteChannelUserState));
                        Console.ResetColor();

                        Console.WriteLine("Enter channel group name" + System.Environment.NewLine + "NOTE: If you want to consider only Channel, just hit ENTER");
                        string deleteChannelGroupUserState = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("ChannelGroup = {0}", deleteChannelGroupUserState));
                        Console.ResetColor();

                        Console.WriteLine("Enter key of the User State Key-Value pair to be deleted");
                        string deleteKeyUserState = Console.ReadLine();
                        Dictionary<string, object> deleteDic = new Dictionary<string, object>();
                        deleteDic.Add(deleteKeyUserState, null);

                        PNResult<PNSetStateResult> respDeleteState = await pubnub.SetPresenceState()
                            .Channels(new string[] { deleteChannelUserState })
                            .ChannelGroups(new string[] { deleteChannelGroupUserState })
                            .State(deleteDic)
                            .ExecuteAsync();
                        if (respDeleteState.Result != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respDeleteState.Result));
                        }

                        break;
                    case "15":
                        Console.WriteLine("Enter channel name" + System.Environment.NewLine + "NOTE: If you want to consider only Channel Group, just hit ENTER");
                        string getUserStateChannel2 = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", getUserStateChannel2));
                        Console.ResetColor();

                        Console.WriteLine("Enter channel group name" + System.Environment.NewLine + "NOTE: If you want to consider only Channel, just hit ENTER");
                        string getUserStateChannelGroup2 = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("ChannelGroup = {0}", getUserStateChannelGroup2));
                        Console.ResetColor();

                        Console.WriteLine("Enter UUID. (Optional. Press ENTER to skip it)");
                        string uuid2 = Console.ReadLine();

                        string[] getUserStateChannel2List = getUserStateChannel2.Split(',');
                        string[] getUserStateChannelGroup2List = getUserStateChannelGroup2.Split(',');

                        PNResult<PNGetStateResult> respGetState = await pubnub.GetPresenceState()
                            .Channels(getUserStateChannel2List)
                            .ChannelGroups(getUserStateChannelGroup2List)
                            .Uuid(uuid2)
                            .ExecuteAsync();
                        if (respGetState.Result != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respGetState.Result));
                        }
                        break;
                    case "16":
                        Console.WriteLine("Enter uuid for WhereNow. To consider SessionUUID, just press ENTER");
                        string whereNowUuid = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("uuid = {0}", whereNowUuid));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running Where_Now()");
                        PNResult<PNWhereNowResult> respWhereNow = await pubnub.WhereNow()
                            .Uuid(whereNowUuid)
                            .ExecuteAsync();
                        if (respWhereNow.Result != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respWhereNow.Result));
                        }
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

                        PNResult<PNDeleteMessageResult> respDelMsg = await pubnub.DeleteMessages().Channel(deleteMessageChannel)
                            .Start(deleteStart)
                              .End(deleteEnd)
                              .ExecuteAsync();
                        if (respDelMsg.Status != null && respDelMsg.Status.Error)
                        {
                            Console.WriteLine(respDelMsg.Status.ErrorData.Information);
                        }
                        else
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respDelMsg.Result));
                        }

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
                        PNResult<PNMessageCountResult> respMsgCount = await pubnub.MessageCounts().Channels(channelMsgCount.Split(','))
                            .ChannelsTimetoken(lstTTMsgCount.ToArray())
                            .ExecuteAsync();
                        if (respMsgCount.Status != null && respMsgCount.Status.Error)
                        {
                            Console.WriteLine(respMsgCount.Status.ErrorData.Information);
                        }
                        else
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respMsgCount.Result));
                        }
                        break;
                    case "25":
                        Console.WriteLine("Enter CHANNEL name(s) for Fetch History");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel(s) = {0}", channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running FetchHistory()");
                        PNResult<PNFetchHistoryResult> respFetchHist = await pubnub.FetchHistory()
                            .Channels(channel.Split(','))
                            .Reverse(false)
                            .MaximumPerChannel(25)
                            .IncludeMeta(true)
                            .ExecuteAsync();
                        if (respFetchHist.Result != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respFetchHist.Result));
                        }
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
                        PNResult<PNPushAddChannelResult> respPusdAddCh = await pubnub.AddPushNotificationsOnChannels().Channels(new string[] { pushRegisterChannel })
                            .PushType(PNPushType.APNS)
                            .DeviceId(pushToken)
                            .ExecuteAsync();
                        if (respPusdAddCh.Result != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respPusdAddCh.Result));
                        }
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
                        PNResult<PNPushRemoveChannelResult> respPushRmCh = await pubnub.RemovePushNotificationsFromChannels()
                            .Channels(new string[] { pushRemoveChannel })
                            .PushType(PNPushType.APNS)
                            .DeviceId(pushTokenRemove)
                            .ExecuteAsync();
                        if (respPushRmCh.Result != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respPushRmCh.Result));
                        }
                        break;
                    case "33":
                        Console.WriteLine("Enter Push Token for APNS");
                        string pushTokenGetChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Push Token = {0}", pushTokenGetChannel));
                        Console.ResetColor();

                        Console.WriteLine("Running AuditPushChannelProvisions()");
                        PNResult<PNPushListProvisionsResult> respAuditPushChProv = await pubnub.AuditPushChannelProvisions()
                            .PushType(PNPushType.APNS)
                            .DeviceId(pushTokenGetChannel)
                            .ExecuteAsync();
                        if (respAuditPushChProv.Result != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respAuditPushChProv.Result));
                        }
                        break;
                    case "34":
                        Console.WriteLine("Enter Push Token for APNS");
                        string pushTokenUnregisterDevice = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Push Token = {0}", pushTokenUnregisterDevice));
                        Console.ResetColor();

                        Console.WriteLine("Running RemoveAllPushNotificationsFromDeviceWithPushToken()");
                        PNResult<PNPushRemoveAllChannelsResult> respRmAllPushNotif = await pubnub.RemoveAllPushNotificationsFromDeviceWithPushToken()
                            .PushType(PNPushType.APNS)
                            .DeviceId(pushTokenUnregisterDevice)
                            .ExecuteAsync();
                        if (respRmAllPushNotif.Result != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respRmAllPushNotif.Result));
                        }
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
                        PNResult<PNChannelGroupsAddChannelResult> respAddChToCg = await pubnub.AddChannelsToChannelGroup()
                            .ChannelGroup(addChannelGroupName)
                            .Channels(channel.Split(','))
                            .ExecuteAsync();
                        if (respAddChToCg.Result != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respAddChToCg.Result));
                        }
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
                            PNResult<PNChannelGroupsDeleteGroupResult> respDelCg = await pubnub.DeleteChannelGroup()
                                .ChannelGroup(removeChannelGroupName)
                                .ExecuteAsync();
                            if (respDelCg.Result != null)
                            {
                                Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respDelCg.Result));
                            }
                            break;
                        }

                        Console.WriteLine("Enter CHANNEL name. Use comma to enter multiple channels.");
                        channel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.ResetColor();
                        Console.WriteLine();
                        PNResult<PNChannelGroupsRemoveChannelResult> respRmChFrmCg = await pubnub.RemoveChannelsFromChannelGroup()
                            .ChannelGroup(removeChannelGroupName)
                            .Channels(channel.Split(','))
                            .ExecuteAsync();
                        if (respRmChFrmCg.Result != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respRmChFrmCg.Result));
                        }
                        break;
                    case "40":
                        Console.WriteLine("Do you want to get all existing channel group names? Default is No. Enter Y for Yes, Else just hit ENTER key");
                        string getExistingGroupNames = Console.ReadLine();
                        if (getExistingGroupNames.ToLower() == "y")
                        {
                            PNResult<PNChannelGroupsListAllResult> respListCg = await pubnub.ListChannelGroups().ExecuteAsync();
                            if (respListCg.Result != null)
                            {
                                Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respListCg.Result));
                            }
                            break;
                        }

                        Console.WriteLine("Enter channel group name");
                        string channelGroupName = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("channel group name = {0}", channelGroupName));
                        Console.ResetColor();

                        PNResult<PNChannelGroupsAllChannelsResult> respListCh4Cg = await pubnub.ListChannelsForChannelGroup()
                            .ChannelGroup(channelGroupName)
                            .ExecuteAsync();
                        if (respListCh4Cg.Result != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respListCh4Cg.Result));
                        }
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

                        PNResult<PNPublishResult> respSignal;
                        double doubleDataSignal;
                        int intDataSignal;
                        if (int.TryParse(signalMsg, out intDataSignal)) //capture numeric data
                        {
                            respSignal = await pubnub.Signal().Channel(channel).Message(intDataSignal)
                                .ExecuteAsync();
                            if (respSignal.Status.Error) { Console.WriteLine(respSignal.Status.ErrorData.Information); } else { Console.WriteLine(respSignal.Result.Timetoken); }
                        }
                        else if (double.TryParse(signalMsg, out doubleDataSignal)) //capture numeric data
                        {
                            respSignal = await pubnub.Signal().Channel(channel).Message(doubleDataSignal)
                                .ExecuteAsync();
                            if (respSignal.Status.Error) { Console.WriteLine(respSignal.Status.ErrorData.Information); } else { Console.WriteLine(respSignal.Result.Timetoken); }
                        }
                        else
                        {
                            //check whether any numeric is sent in double quotes
                            if (signalMsg.IndexOf("\"") == 0 && signalMsg.LastIndexOf("\"") == signalMsg.Length - 1)
                            {
                                string strMsg = signalMsg.Substring(1, signalMsg.Length - 2);
                                if (int.TryParse(strMsg, out intDataSignal))
                                {
                                    respSignal = await pubnub.Signal().Channel(channel).Message(intDataSignal)
                                        .ExecuteAsync();
                                    if (respSignal.Status.Error) { Console.WriteLine(respSignal.Status.ErrorData.Information); } else { Console.WriteLine(respSignal.Result.Timetoken); }
                                }
                                else if (double.TryParse(strMsg, out doubleDataSignal))
                                {
                                    respSignal = await pubnub.Signal().Channel(channel).Message(doubleDataSignal)
                                        .ExecuteAsync();
                                    if (respSignal.Status.Error) { Console.WriteLine(respSignal.Status.ErrorData.Information); } else { Console.WriteLine(respSignal.Result.Timetoken); }
                                }
                                else
                                {
                                    respSignal = await pubnub.Signal().Channel(channel).Message(signalMsg)
                                        .ExecuteAsync();
                                    if (respSignal.Status.Error) { Console.WriteLine(respSignal.Status.ErrorData.Information); } else { Console.WriteLine(respSignal.Result.Timetoken); }
                                }
                            }
                            else
                            {
                                respSignal = await pubnub.Signal()
                                    .Channel(channel)
                                    .Message(signalMsg)
                                    .ExecuteAsync();
                                if (respSignal.Status.Error) { Console.WriteLine(respSignal.Status.ErrorData.Information); } else { Console.WriteLine(respSignal.Result.Timetoken); }
                            }
                        }
                        break;
                    case "42":
                        Console.WriteLine("Enter UUID to SetUuidMetadata");
                        string setUuidMetadataId = Console.ReadLine();
                        Console.WriteLine("Enter UUID Name");
                        string setUuidMetadataName = Console.ReadLine();

                        PNResult<PNSetUuidMetadataResult> setUuidMetadataResponse = await pubnub.SetUuidMetadata()
                            .Uuid(setUuidMetadataId)
                            .Name(setUuidMetadataName)
                            .IncludeCustom(true)
                            .ExecuteAsync();

                        PNSetUuidMetadataResult setUuidMetadataResult = setUuidMetadataResponse.Result;
                        PNStatus status = setUuidMetadataResponse.Status;

                        if (setUuidMetadataResponse.Status.Error) { Console.WriteLine(status.ErrorData.Information); }
                        else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(setUuidMetadataResult)); }

                        break;
                    case "43":
                        Console.WriteLine("Enter UUID to retrieve, else press ENTER.");
                        string getSingleUuidMetadata = Console.ReadLine();

                        if (!string.IsNullOrWhiteSpace(getSingleUuidMetadata))
                        {
                            PNResult<PNGetUuidMetadataResult> getUuidMetadataResponse = await pubnub.GetUuidMetadata()
                                .Uuid(getSingleUuidMetadata)
                                .IncludeCustom(true)
                                .ExecuteAsync();

                           if (getUuidMetadataResponse.Status.Error) { Console.WriteLine(getUuidMetadataResponse.Status.ErrorData.Information); }
                            else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(getUuidMetadataResponse.Result)); }
                        }
                        else
                        {
                            PNResult<PNGetAllUuidMetadataResult> getAllUuidMetadataResponse = await pubnub.GetAllUuidMetadata()
                                .IncludeCustom(true)
                                .IncludeCount(true)
                                .Page(new PNPageObject() { Next = "", Prev = "" })
                                .ExecuteAsync();

                            if (getAllUuidMetadataResponse.Status.Error) { Console.WriteLine(getAllUuidMetadataResponse.Status.ErrorData.Information); }
                            else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(getAllUuidMetadataResponse.Result)); }
                        }
                        break;
                    case "44":
                        Console.WriteLine("Enter UUID to remove.");
                        string removeUuid = Console.ReadLine();
                        PNResult<PNRemoveUuidMetadataResult> removeUuidMetadataResponse = await pubnub.RemoveUuidMetadata()
                            .Uuid(removeUuid)
                            .ExecuteAsync();

                        if (removeUuidMetadataResponse.Status.Error) { Console.WriteLine(removeUuidMetadataResponse.Status.ErrorData.Information); }
                        else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(removeUuidMetadataResponse.Result)); }
                        break;
                    case "45":
                        Console.WriteLine("Enter Channel");
                        string setChannelMetaId = Console.ReadLine();
                        Console.WriteLine("Enter Name for Channel");
                        string setChannelMetaName = Console.ReadLine();

                        if (string.IsNullOrEmpty(setChannelMetaId))
                        {
                            Console.WriteLine("Invalid Channel");
                        }
                        else
                        {
                            PNResult<PNSetChannelMetadataResult> setChannelMetadataResponse = await pubnub.SetChannelMetadata()
                                .Channel(setChannelMetaId)
                                .Name(setChannelMetaName)
                                .IncludeCustom(true)
                                .ExecuteAsync();

                            if (setChannelMetadataResponse.Status.Error) { Console.WriteLine(setChannelMetadataResponse.Status.ErrorData.Information); }
                            else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(setChannelMetadataResponse.Result)); }
                        }
                        break;
                    case "46":
                        Console.WriteLine("Enter Channel to retrieve, else press ENTER.");
                        string singleChannelMetadataId = Console.ReadLine();

                        if (!string.IsNullOrWhiteSpace(singleChannelMetadataId))
                        {
                            PNResult<PNGetChannelMetadataResult> getChannelMetadataResponse = await pubnub.GetChannelMetadata()
                                .Channel(singleChannelMetadataId)
                                .IncludeCustom(true)
                                .ExecuteAsync();

                            if (getChannelMetadataResponse.Status.Error) { Console.WriteLine(getChannelMetadataResponse.Status.ErrorData.Information); }
                            else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(getChannelMetadataResponse.Result)); }
                        }
                        else
                        {
                            PNResult<PNGetAllChannelMetadataResult> getAllChannelMetadataResponse = await pubnub.GetAllChannelMetadata()
                                .IncludeCount(true)
                                .IncludeCustom(true)
                                .Page(new PNPageObject() { Next = "", Prev = "" })
                                .ExecuteAsync();

                            if (getAllChannelMetadataResponse.Status.Error) { Console.WriteLine(getAllChannelMetadataResponse.Status.ErrorData.Information); }
                            else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(getAllChannelMetadataResponse.Result)); }
                        }
                        break;
                    case "47":
                        Console.WriteLine("Enter Channel to remove.");
                        string removeChannelMetadataId = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(removeChannelMetadataId))
                        {
                            PNResult<PNRemoveChannelMetadataResult> removeChannelMetadataResponse = await pubnub.RemoveChannelMetadata()
                                .Channel(removeChannelMetadataId)
                                .ExecuteAsync();

                            if (removeChannelMetadataResponse.Status.Error) { Console.WriteLine(removeChannelMetadataResponse.Status.ErrorData.Information); }
                            else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(removeChannelMetadataResponse.Result)); }
                        }
                        else
                        {
                            Console.WriteLine("Invalid Channel. Try again.");
                        }
                        break;
                    case "48":
                        Console.WriteLine("Enter UUID");
                        string manageMembershipUuidMetaId = Console.ReadLine();
                        Console.WriteLine("Enter Channel to set");
                        string manageMemshipSetChannelMetaId = Console.ReadLine();
                        Console.WriteLine("Enter Chanel to remove");
                        string manageMemshipRemChannelMetaId = Console.ReadLine();

                        List<PNMembership> manageSetMembershipList = new List<PNMembership>();
                        if (!string.IsNullOrEmpty(manageMemshipSetChannelMetaId))
                        {
                            manageSetMembershipList.Add(new PNMembership() { Channel = manageMemshipSetChannelMetaId, Custom = new Dictionary<string, object>() { { "item", "book" } } });
                        }

                        List<string> manageRemoveMembershipList = new List<string>();
                        if (!string.IsNullOrEmpty(manageMemshipRemChannelMetaId))
                        {
                            manageRemoveMembershipList.Add(manageMemshipRemChannelMetaId);
                        }

                        PNResult<PNMembershipsResult> manageMembershipResponse = await pubnub.ManageMemberships()
                            .Uuid(manageMembershipUuidMetaId)
                            .Set(manageSetMembershipList)
                            .Remove(manageRemoveMembershipList)
                            .Include(new PNMembershipField[] { PNMembershipField.CUSTOM, PNMembershipField.CHANNEL, PNMembershipField.CHANNEL_CUSTOM })
                            .IncludeCount(true)
                            .Page(new PNPageObject() { Next = "", Prev = "" })
                            .ExecuteAsync();

                        if (manageMembershipResponse.Status.Error) { Console.WriteLine(manageMembershipResponse.Status.ErrorData.Information); }
                        else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMembershipResponse.Result)); }
                        break;
                    case "49":
                        Console.WriteLine("Enter Channel");
                        string manageMemberChMetadataId = Console.ReadLine();
                        Console.WriteLine("Enter UUID to add");
                        string manageMemberAddUuid = Console.ReadLine();
                        Console.WriteLine("Enter UUID to remove");
                        string manageMemberRemUuid = Console.ReadLine();

                        List<PNChannelMember> manageSetMemberList = new List<PNChannelMember>();
                        if (!string.IsNullOrEmpty(manageMemberAddUuid))
                        {
                            manageSetMemberList.Add(new PNChannelMember() { Uuid = manageMemberAddUuid, Custom = new Dictionary<string, object>() { { "planet", "earth" } } });
                        }

                        List<string> manageRemMemberList = new List<string>();
                        if (!string.IsNullOrEmpty(manageMemberRemUuid))
                        {
                            manageRemMemberList.Add(manageMemberRemUuid);
                        }

                        if (string.IsNullOrEmpty(manageMemberChMetadataId))
                        {
                            Console.WriteLine("Invalid Channel");
                        }
                        else
                        {
                            PNResult<PNChannelMembersResult> manageMemberResponse = await pubnub.ManageChannelMembers()
                                .Channel(manageMemberChMetadataId)
                                .Set(manageSetMemberList)
                                .Remove(manageRemMemberList)
                                .Include(new PNChannelMemberField[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID, PNChannelMemberField.UUID_CUSTOM })
                                .IncludeCount(true)
                                .Page(new PNPageObject() { Next = "", Prev = "" })
                                .ExecuteAsync();

                            if (manageMemberResponse.Status.Error) { Console.WriteLine(manageMemberResponse.Status.ErrorData.Information); }
                            else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMemberResponse.Result)); }
                        }
                        break;
                    case "50":
                        Console.WriteLine("Enter UUID to Get Memberships.");
                        string getMembershipsUuid = Console.ReadLine();

                        PNResult<PNMembershipsResult> getMembershipsResponse = await pubnub.GetMemberships()
                            .Uuid(getMembershipsUuid)
                            .Include(new PNMembershipField[] { PNMembershipField.CUSTOM, PNMembershipField.CHANNEL, PNMembershipField.CHANNEL_CUSTOM })
                            .IncludeCount(true)
                            .Page(new PNPageObject() { Next = "", Prev = "" })
                            .ExecuteAsync();

                        if (getMembershipsResponse.Status.Error) { Console.WriteLine(getMembershipsResponse.Status.ErrorData.Information); }
                        else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(getMembershipsResponse.Result)); }
                        break;
                    case "51":
                        Console.WriteLine("Enter Channel to Get Channel Members.");
                        string getMembersChannelId = Console.ReadLine();
                        if (!string.IsNullOrEmpty(getMembersChannelId))
                        {
                            PNResult<PNChannelMembersResult> getMembersResponse = await pubnub.GetChannelMembers()
                                .Channel(getMembersChannelId)
                                .Include(new PNChannelMemberField[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID, PNChannelMemberField.UUID_CUSTOM })
                                .IncludeCount(true)
                                .Page(new PNPageObject() { Next = "", Prev = "" })
                                .ExecuteAsync();

                            if (getMembersResponse.Status.Error) { Console.WriteLine(getMembersResponse.Status.ErrorData.Information); }
                            else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(getMembersResponse.Result)); }
                        }
                        else
                        {
                            Console.WriteLine("Invalid Channel!!!");
                        }
                        break;
                    case "52":
                        Console.WriteLine("Enter UUID");
                        string setMembershipUuidMetaId = Console.ReadLine();
                        Console.WriteLine("Enter Channel to add");
                        string seMembershipChannelMetaId = Console.ReadLine();

                        List<PNMembership> setMembershipChannelMetadataIdList = new List<PNMembership>();
                        if (!string.IsNullOrEmpty(seMembershipChannelMetaId))
                        {
                            setMembershipChannelMetadataIdList.Add(new PNMembership() { Channel = seMembershipChannelMetaId, Custom = new Dictionary<string, object>() { { "item", "book" } } });
                        }

                        Console.WriteLine("Running SetMemberships()");
                        PNResult<PNMembershipsResult> setMembershipsResponse = await pubnub.SetMemberships()
                            .Uuid(setMembershipUuidMetaId)
                            .Channels(setMembershipChannelMetadataIdList)
                            .Include(new PNMembershipField[] { PNMembershipField.CUSTOM, PNMembershipField.CHANNEL, PNMembershipField.CHANNEL_CUSTOM })
                            .IncludeCount(true)
                            .Page(new PNPageObject() { Next = "", Prev = "" })
                            .ExecuteAsync();

                        if (setMembershipsResponse.Status.Error) { Console.WriteLine(setMembershipsResponse.Status.ErrorData.Information); }
                        else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(setMembershipsResponse.Result)); }
                        break;
                    case "53":
                        Console.WriteLine("Enter UUID");
                        string removeMemshipUuidMetaId = Console.ReadLine();
                        Console.WriteLine("Enter Chanel to remove");
                        string removeMembershipChannelMetaId = Console.ReadLine();

                        List<string> removeMembershipList = new List<string>();
                        if (!string.IsNullOrEmpty(removeMembershipChannelMetaId))
                        {
                            removeMembershipList.Add(removeMembershipChannelMetaId);
                        }

                        Console.WriteLine("Running RemoveMemberships()");
                        PNResult<PNMembershipsResult> removeMembershipsResponse = await pubnub.RemoveMemberships()
                            .Uuid(removeMemshipUuidMetaId)
                            .Channels(removeMembershipList)
                            .Include(new PNMembershipField[] { PNMembershipField.CUSTOM, PNMembershipField.CHANNEL, PNMembershipField.CHANNEL_CUSTOM })
                            .IncludeCount(true)
                            .Page(new PNPageObject() { Next = "", Prev = "" })
                            .ExecuteAsync();

                        if (removeMembershipsResponse.Status.Error) { Console.WriteLine(removeMembershipsResponse.Status.ErrorData.Information); }
                        else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(removeMembershipsResponse.Result)); }
                        break;
                    case "54":
                        Console.WriteLine("Enter Channel");
                        string setmemberChMetadataId = Console.ReadLine();
                        Console.WriteLine("Enter UUID to add");
                        string setMemberChUuid = Console.ReadLine();

                        List<PNChannelMember> setMemberChannelList = new List<PNChannelMember>();
                        if (!string.IsNullOrEmpty(setMemberChUuid))
                        {
                            setMemberChannelList.Add(new PNChannelMember() { Uuid = setMemberChUuid, Custom = new Dictionary<string, object>() { { "planet", "earth" } } });
                        }

                        if (string.IsNullOrEmpty(setmemberChMetadataId))
                        {
                            Console.WriteLine("Invalid Channel");
                        }
                        else
                        {
                            Console.WriteLine("Running SetChannelMembers()");
                            PNResult<PNChannelMembersResult> setChannelMembersResponse = await pubnub.SetChannelMembers()
                                .Channel(setmemberChMetadataId)
                                .Uuids(setMemberChannelList)
                                .Include(new PNChannelMemberField[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID, PNChannelMemberField.UUID_CUSTOM })
                                .IncludeCount(true)
                                .Page(new PNPageObject() { Next = "", Prev = "" })
                                .ExecuteAsync();

                            if (setChannelMembersResponse.Status.Error) { Console.WriteLine(setChannelMembersResponse.Status.ErrorData.Information); }
                            else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(setChannelMembersResponse.Result)); }
                        }
                        break;
                    case "55":
                        Console.WriteLine("Enter Channel");
                        string removeChMemberMetadataId = Console.ReadLine();
                        Console.WriteLine("Enter UUID to remove");
                        string removeMemberChUuid = Console.ReadLine();

                        List<string> removeChannelMemberList = new List<string>();
                        if (!string.IsNullOrEmpty(removeMemberChUuid))
                        {
                            removeChannelMemberList.Add(removeMemberChUuid);
                        }

                        if (string.IsNullOrEmpty(removeChMemberMetadataId))
                        {
                            Console.WriteLine("Invalid Channel");
                        }
                        else
                        {
                            Console.WriteLine("Running RemoveChannelMembers()");
                            PNResult<PNChannelMembersResult> manageMemberResponse = await pubnub.RemoveChannelMembers()
                                .Channel(removeChMemberMetadataId)
                                .Uuids(removeChannelMemberList)
                                .Include(new PNChannelMemberField[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID, PNChannelMemberField.UUID_CUSTOM })
                                .IncludeCount(true)
                                .Page(new PNPageObject() { Next = "", Prev = "" })
                                .ExecuteAsync();

                            if (manageMemberResponse.Status.Error) { Console.WriteLine(manageMemberResponse.Status.ErrorData.Information); }
                            else { Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMemberResponse.Result)); }
                        }
                        break;
                    case "56":
                        Console.WriteLine("Enter Channel Name to Add Message Actions");
                        string addMsgActionChannelName = Console.ReadLine();
                        Console.WriteLine("Enter MessageTimetoken");
                        string addMsgActionMsgTT = Console.ReadLine();
                        Console.WriteLine("Enter Message Action Type");
                        string addMsgActionType = Console.ReadLine();
                        Console.WriteLine("Enter Message Action Value");
                        string addMsgActionValue = Console.ReadLine();
                        long addMsgActionMsgTimetoken;
                        Int64.TryParse(addMsgActionMsgTT.ToString(), out addMsgActionMsgTimetoken);

                        PNResult<PNAddMessageActionResult> respAddMsgAction = await pubnub.AddMessageAction()
                            .Channel(addMsgActionChannelName)
                            .MessageTimetoken(addMsgActionMsgTimetoken)
                            .Action(new PNMessageAction { Type = addMsgActionType, Value = addMsgActionValue })
                            .ExecuteAsync();

                        if (respAddMsgAction.Result != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respAddMsgAction.Result));
                        }
                        else
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respAddMsgAction.Status));
                        }
                        break;
                    case "57":
                        Console.WriteLine("Enter Channel Name to Remove Message Actions");
                        string rmMsgActionChannelName = Console.ReadLine();
                        Console.WriteLine("Enter MessageTimetoken");
                        string rmMsgActionMsgTT = Console.ReadLine();
                        Console.WriteLine("Enter ActionTimetoken");
                        string rmMsgActionActTT = Console.ReadLine();
                        Console.WriteLine("Enter UUID");
                        string rmMsgActionUuid = Console.ReadLine();
                        long rmMsgActionMsgTimetoken;
                        Int64.TryParse(rmMsgActionMsgTT.ToString(), out rmMsgActionMsgTimetoken);
                        long rmMsgActionMsgActionTimetoken;
                        Int64.TryParse(rmMsgActionActTT.ToString(), out rmMsgActionMsgActionTimetoken);

                        PNResult<PNRemoveMessageActionResult> respRmMsgAction = await pubnub.RemoveMessageAction()
                            .Channel(rmMsgActionChannelName)
                            .MessageTimetoken(rmMsgActionMsgTimetoken)
                            .ActionTimetoken(rmMsgActionMsgActionTimetoken)
                            .Uuid(rmMsgActionUuid)
                            .ExecuteAsync();

                        if (respRmMsgAction.Result != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respRmMsgAction.Result));
                        }
                        else
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respRmMsgAction.Status));
                        }
                        break;
                    case "58":
                        Console.WriteLine("Enter Channel Name to Get Message Actions");
                        string getMsgActionChannelName = Console.ReadLine();

                        PNResult<PNGetMessageActionsResult> respGetMsgAction = await pubnub.GetMessageActions()
                            .Channel(getMsgActionChannelName)
                            .ExecuteAsync();

                        if (respGetMsgAction.Result != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respGetMsgAction.Result));
                        }
                        else
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respGetMsgAction.Status));
                        }
                        break;
                    case "59":
                        Console.WriteLine("Enter Channel Name for SendFile");
                        string sendFileChannelName = Console.ReadLine();
                        Console.WriteLine("Enter full file path which needs to be sent");
                        string sendFileFullPath = Console.ReadLine();
                        Console.WriteLine("Enter file message");
                        string sendFileMessage = Console.ReadLine();

                        PNResult<PNFileUploadResult> fileUploadResponse = await pubnub.SendFile()
                            .Channel(sendFileChannelName)
                            .File(sendFileFullPath)
                            .Message(sendFileMessage)
                            .ExecuteAsync();
                        PNFileUploadResult fileUploadResult = fileUploadResponse.Result;
                        PNStatus fileUploadStatus = fileUploadResponse.Status;
                        if (fileUploadResult != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(fileUploadResult));
                        }
                        else
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(fileUploadStatus));
                        }
                        break;
                    case "60": //DownloadFile
                        Console.WriteLine("Enter Channel Name");
                        string downloadFileUrlChannelName = Console.ReadLine();
                        Console.WriteLine("Enter file id");
                        string downloadUrlFileId = Console.ReadLine();
                        Console.WriteLine("Enter file name");
                        string downloadUrlFileName = Console.ReadLine();
                        PNResult<PNDownloadFileResult> fileDownloadResponse = await pubnub.DownloadFile()
                            .Channel(downloadFileUrlChannelName)
                            .FileId(downloadUrlFileId)
                            .FileName(downloadUrlFileName)
                            .ExecuteAsync();
                        PNDownloadFileResult fileDownloadResult = fileDownloadResponse.Result;
                        PNStatus fileDownloadStatus = fileDownloadResponse.Status;
                        if (fileDownloadResult != null)
                        {
                            fileDownloadResult.SaveFileToLocal(downloadUrlFileName);
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(fileDownloadResult.FileName));
                        }
                        else
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(fileDownloadStatus));
                        }
                        break;
                    case "61"://GetFileUrl
                        Console.WriteLine("Enter Channel Name");
                        string getFileUrlChannelName = Console.ReadLine();
                        Console.WriteLine("Enter file id");
                        string getUrlFileId = Console.ReadLine();
                        Console.WriteLine("Enter file name");
                        string getUrlFileName = Console.ReadLine();
                        PNResult<PNFileUrlResult> getFileUrlResponse = await pubnub.GetFileUrl()
                            .Channel(getFileUrlChannelName)
                            .FileId(getUrlFileId)
                            .FileName(getUrlFileName)
                            .ExecuteAsync();
                        PNFileUrlResult getFileUrlResult = getFileUrlResponse.Result;
                        PNStatus getFileUrlStatus = getFileUrlResponse.Status;
                        if (getFileUrlResult != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(getFileUrlResult));
                        }
                        else
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(getFileUrlStatus));
                        }
                        break;
                    case "62":
                        Console.WriteLine("Enter Channel Name");
                        string deleteFileUrlChannelName = Console.ReadLine();
                        Console.WriteLine("Enter file id");
                        string deleteFileId = Console.ReadLine();
                        Console.WriteLine("Enter file name");
                        string deleteFileName = Console.ReadLine();
                        PNResult<PNDeleteFileResult> deleteFileResponse = await pubnub.DeleteFile()
                            .Channel(deleteFileUrlChannelName)
                            .FileId(deleteFileId)
                            .FileName(deleteFileName)
                            .ExecuteAsync();
                        PNDeleteFileResult deleteFileResult = deleteFileResponse.Result;
                        PNStatus deleteFileStatus = deleteFileResponse.Status;
                        if (deleteFileResult != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(deleteFileResult));
                        }
                        else
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(deleteFileStatus));
                        }
                        break;
                    case "63":
                        Console.WriteLine("Enter Channel Name for PublishFileMessage");
                        string pubFileMessageChannelName = Console.ReadLine();
                        Console.WriteLine("Enter file id");
                        string pubFileId = Console.ReadLine();
                        Console.WriteLine("Enter file name");
                        string pubFileName = Console.ReadLine();
                        Console.WriteLine("Enter file message");
                        string pubFileMessage = Console.ReadLine();
                        PNResult<PNPublishFileMessageResult> publishFileMsgResponse = await pubnub.PublishFileMessage()
                            .Channel(pubFileMessageChannelName)
                            .FileId(pubFileId)
                            .FileName(pubFileName)
                            .Message(pubFileMessage)
                            .ExecuteAsync();
                        PNPublishFileMessageResult publishFileMsgResult = publishFileMsgResponse.Result;
                        PNStatus publishFileMsgStatus = publishFileMsgResponse.Status;
                        if (publishFileMsgResult != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(publishFileMsgResult));
                        }
                        else
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(publishFileMsgStatus));
                        }
                        break;
                    case "64":
                        Console.WriteLine("Enter Channel Name");
                        string listFilesChannelName = Console.ReadLine();
                        PNResult<PNListFilesResult> listFilesResponse = await pubnub.ListFiles()
                            .Channel(listFilesChannelName)
                            .ExecuteAsync();
                        PNListFilesResult listFilesResult = listFilesResponse.Result;
                        PNStatus listFilesStatus = listFilesResponse.Status;
                        if (listFilesResult != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(listFilesResult));
                        }
                        else
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(listFilesStatus));
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

    public class CustomMessage
    {
        public string Text { get; set; }
        public int Id { get; set; }
    }

    public enum PhoneType
    {
        Home,
        Mobile,
        Work
    }

}
