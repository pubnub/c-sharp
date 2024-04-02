using System;
using TechTalk.SpecFlow;
using PubnubApi;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Net;
using System.IO;
using System.Threading;
using TechTalk.SpecFlow.Assist;
using System.Net.Http;
using System.Diagnostics;

namespace AcceptanceTests.Steps
{
	[Binding]
    public class EventEngineSteps
    {
        public static bool enableIntenalPubnubLogging = true;
        public static string currentFeature = string.Empty;
        public static string currentContract = string.Empty;
        public static bool betaVersion = false;
        private string acceptance_test_origin = "localhost:8090";
        private bool bypassMockServer = false;
        private readonly ScenarioContext _scenarioContext;
        private Pubnub pn;
        private PNConfiguration config = null;
        private string channel = "my_channel";
        private string channelGroup = "my_channelgroup";
        private string publishMsg = "hello_world";
        PNPublishResult publishResult = null;
        SubscribeCallback subscribeCallback = null;
        private PNMessageResult<object> messageResult = null;
        ManualResetEvent messageReceivedEvent = new ManualResetEvent(false);
        ManualResetEvent statusReceivedEvent = new ManualResetEvent(false);
        ManualResetEvent presenceEvent = new ManualResetEvent(false);
        PNStatus pnStatus = null;
        PubnubError pnError = null;
        IPubnubUnitTest unitTest;

        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine("Unhandled exception occured inside EventEngine. Exiting the test. Please try again.");
            System.Environment.Exit(1);
        }
        public class PubnubUnitTest : IPubnubUnitTest
        {
			long IPubnubUnitTest.Timetoken
            {
                get;
                set;
            }

            string IPubnubUnitTest.RequestId
            {
                get;
                set;
            }

            byte[] IPubnubUnitTest.IV
            {
                get;
                set;
            }

            bool IPubnubUnitTest.InternetAvailable
            {
                get;
                set;
            }

            string IPubnubUnitTest.SdkVersion
            {
                get;
                set;
            }

            bool IPubnubUnitTest.IncludePnsdk
            {
                get;
                set;
            }

            bool IPubnubUnitTest.IncludeUuid
            {
                get;
                set;
            }
            List<KeyValuePair<string,string>> IPubnubUnitTest.EventTypeList 
            { 
                get; 
                set;
            }
            int IPubnubUnitTest.Attempts
            {
                get;
                set;
            }
			public List<KeyValuePair<string, string>> PresenceActivityList
            {
                get;
                set;
            }
        }
        public class PubnubError
        {
            public ErrorMsg error;
            public string service;
            public int status;
        }
        public class MessageDetail
        {
            public string message;
            public string location;
            public string locationType;
        }
        public class ErrorMsg
        {
            public string message;
            public string source;
            public List<MessageDetail> details;
        }

        public class InternalPubnubLog : IPubnubLog
        {
            void IPubnubLog.WriteToLog(string logText)
            {
                System.Diagnostics.Debug.WriteLine(logText);
                string dirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string logFilePath = System.IO.Path.Combine(dirPath, "pubnubmessaging.log");
                System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(logFilePath));

            }
        }
        public class SubscribeResponseRow
        {
            public string type { get; set; }
            public string name { get; set; }
        }
        public EventEngineSteps(ScenarioContext scenarioContext)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
            _scenarioContext = scenarioContext;
        }

        [BeforeFeature]
        public static void BeforeFeature(FeatureContext featureContext)
        {
            betaVersion = false;
            if (featureContext.FeatureInfo != null && featureContext.FeatureInfo.Tags.Length > 0)
            {
                List<string> tagList = featureContext.FeatureInfo.Tags.AsEnumerable<string>().ToList();
                foreach (string tag in tagList)
                {
                    if (tag.IndexOf("featureSet=") == 0)
                    {
                        currentFeature = tag.Replace("featureSet=", "");
                    }

                    if (tag.IndexOf("beta") == 0)
                    {
                        betaVersion = true;
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine("Starting " + featureContext.FeatureInfo.Title);
        }

        [AfterFeature]
        public static void AfterFeature(FeatureContext featureContext)
        {
            System.Diagnostics.Debug.WriteLine("Finished " + featureContext.FeatureInfo.Title);
        }

        [BeforeScenario()]
        public void BeforeScenario()
        {
            currentContract = "";
            if (_scenarioContext.ScenarioInfo != null && _scenarioContext.ScenarioInfo.Tags.Length > 0)
            {
                List<string> tagList = _scenarioContext.ScenarioInfo.Tags.AsEnumerable<string>().ToList();
                foreach (string tag in tagList)
                {
                    if (tag.IndexOf("contract=") == 0)
                    {
                        currentContract = tag.Replace("contract=", "");
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(currentContract) && !bypassMockServer)
                {
                    string mockInitContract = string.Format("http://{0}/init?__contract__script__={1}", acceptance_test_origin, currentContract);
                    System.Diagnostics.Debug.WriteLine(mockInitContract);
                    HttpClient httpclient = new HttpClient();
                    string mockInitResponse = httpclient.GetStringAsync(new Uri(mockInitContract)).Result;
                    System.Diagnostics.Debug.WriteLine(mockInitResponse);
                }
            }

        }

        [AfterScenario()]
        public void AfterScenario()
        {
            if (!bypassMockServer)
            {
                string mockExpectContract = string.Format("http://{0}/expect", acceptance_test_origin);
                System.Diagnostics.Debug.WriteLine(mockExpectContract);
                WebClient webClient = new WebClient();
                string mockExpectResponse = webClient.DownloadString(mockExpectContract);
                System.Diagnostics.Debug.WriteLine(mockExpectResponse);
            }
            if (pn != null) {
                pn.UnsubscribeAll<object>();
            }
        }

        [Given(@"the demo keyset with event engine enabled")]
        [Given(@"the demo keyset with Presence EE enabled")]
        public void GivenTheDemoKeysetWithEventEngineEnabled()
        {
            unitTest = new PubnubUnitTest();
            unitTest.Timetoken = 16820876821905844; //Hardcoded timetoken
            unitTest.RequestId = "myRequestId";
            unitTest.InternetAvailable = true;
            unitTest.SdkVersion = "Csharp";
            unitTest.IncludePnsdk = true;
            unitTest.IncludeUuid = true;

            config = new PNConfiguration(new UserId("pn-csharp-acceptance-test-uuid"));
            config.Origin = acceptance_test_origin;
            config.Secure = false;
            config.PublishKey = System.Environment.GetEnvironmentVariable("PN_PUB_KEY");
            config.SubscribeKey = System.Environment.GetEnvironmentVariable("PN_SUB_KEY");
            config.SecretKey = System.Environment.GetEnvironmentVariable("PN_SEC_KEY");
            if (enableIntenalPubnubLogging)
            {
                config.LogVerbosity = PNLogVerbosity.BODY;
                config.PubnubLog = new InternalPubnubLog();
            }
            else
            {
                config.LogVerbosity = PNLogVerbosity.NONE;
            }
            config.EnableEventEngine = true;

            messageReceivedEvent = new ManualResetEvent(false);
            statusReceivedEvent = new ManualResetEvent(false);

            subscribeCallback = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNMessageResult<object> pubMsg)
                {
                    Console.WriteLine($"Message received in listener. {pn.JsonPluggableLibrary.SerializeToJsonString(pubMsg)}");
                    messageResult = pubMsg;
                    messageReceivedEvent.Set();
                },
                delegate (Pubnub pnObj, PNPresenceEventResult presenceEvnt)
                {
                    Console.WriteLine(pn.JsonPluggableLibrary.SerializeToJsonString(presenceEvnt));
                    presenceEvent.Set();
                },
                delegate (Pubnub pnObj, PNSignalResult<object> signalMsg)
                {
                    Console.WriteLine(pn.JsonPluggableLibrary.SerializeToJsonString(signalMsg));
                },
                delegate (Pubnub pnObj, PNObjectEventResult objectEventObj)
                {
                    Console.WriteLine(pn.JsonPluggableLibrary.SerializeToJsonString(objectEventObj));
                },
                delegate (Pubnub pnObj, PNMessageActionEventResult msgActionEvent)
                {
                    System.Diagnostics.Debug.WriteLine(pn.JsonPluggableLibrary.SerializeToJsonString(msgActionEvent));
                },
                delegate (Pubnub pnObj, PNFileEventResult fileEvent)
                {
                    System.Diagnostics.Debug.WriteLine(pn.JsonPluggableLibrary.SerializeToJsonString(fileEvent));
                },
                delegate (Pubnub pnObj, PNStatus status)
                {
                    pnStatus = status;
                    Console.WriteLine("{0} {1} {2}", pnStatus.Operation, pnStatus.Category, pnStatus.StatusCode);
                    if (currentContract == "subscribeHandshakeFailure" && pn.PubnubUnitTest.Attempts == 3)
                    {
                        statusReceivedEvent.Set();
                    }
                    if (pnStatus.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        statusReceivedEvent.Set();
                    }
                }
                );

        }

        [Given(@"heartbeatInterval set to '(.*)', timeout set to '(.*)' and suppressLeaveEvents set to '(.*)'")]
        public void GivenPresenceConfiguration(string heartbeatInterval, string timeout, string suppressLeaveEvents)
        {
            config.SetPresenceTimeoutWithCustomInterval(Convert.ToInt32(timeout), Convert.ToInt32(heartbeatInterval));
            config.SuppressLeaveEvents = Convert.ToBoolean(suppressLeaveEvents);
        }

        [When(@"I join '(.*)', '(.*)', '(.*)' channels")]
        public void WhenIJoinChannels(string first, string second, string third)
        {
            pn = new Pubnub(config);
            pn.PubnubUnitTest = unitTest;
            pn.PubnubUnitTest.PresenceActivityList?.Clear();

            messageReceivedEvent = new ManualResetEvent(false);
            statusReceivedEvent = new ManualResetEvent(false);

            pn.AddListener(subscribeCallback);
            pn.Subscribe<object>()
                .Channels(new string[] { first, second, third })
                .Execute();
        }

        [When(@"I join '(.*)', '(.*)', '(.*)' channels with presence")]
        public void WhenIJoinChannelsWithPresence(string first, string second, string third)
        {
            pn = new Pubnub(config);
            pn.PubnubUnitTest = unitTest;
            pn.PubnubUnitTest.PresenceActivityList?.Clear();

            messageReceivedEvent = new ManualResetEvent(false);
            statusReceivedEvent = new ManualResetEvent(false);
            presenceEvent = new ManualResetEvent(false);

            pn.AddListener(subscribeCallback);
            pn.Subscribe<object>()
                .Channels(new string[] { first, second, third })
                .WithPresence()
                .Execute();
        }

        [Then(@"I wait '(.*)' seconds")]
        public void ThenIWait(string waitSeconds)
        {
            Thread.Sleep(Convert.ToInt32(waitSeconds) * 1000);
        }

        [Then(@"I wait for getting Presence joined events")]
        public void ThenIWaitForPresenceJoinEvents()
        {
            messageReceivedEvent.WaitOne(TimeSpan.FromSeconds(5));
        }

        [Then(@"I observe the following Events and Invocations of the Presence EE:")]
        public void ThenIObserverPresenceEventEngine(Table table)
        {
            if (pn.PubnubUnitTest == null)
            {
                Assert.Fail();
            }
            System.Diagnostics.Debug.WriteLine($"COUNT = {pn.PubnubUnitTest.PresenceActivityList?.Count} ");
            for (int i = 0; i < pn.PubnubUnitTest.PresenceActivityList?.Count(); i++)
            {
                System.Diagnostics.Debug.WriteLine($"{pn.PubnubUnitTest.PresenceActivityList[i].Key} - {pn.PubnubUnitTest.PresenceActivityList[i].Value} ");
			}
            IEnumerable<SubscribeResponseRow> expectedRowSet =  table.CreateSet<SubscribeResponseRow>();
                
            Assert.True(pn.PubnubUnitTest.PresenceActivityList.Count() >= expectedRowSet?.Count());
            bool match = false;
            for (int rowIndex = 0; rowIndex < expectedRowSet.Count(); rowIndex++) {
                SubscribeResponseRow row = expectedRowSet.ElementAt(rowIndex);
                System.Diagnostics.Debug.WriteLine($"{row.type} - {row.name} ");
                if (row.type == pn.PubnubUnitTest.PresenceActivityList[rowIndex].Key
                    && row.name == pn.PubnubUnitTest.PresenceActivityList[rowIndex].Value) {
                    match = true;
                } else {
                    match = false;
                    break;
                }
            }
            Assert.True(match == true);
        }

        [When(@"I subscribe")]
        public void WhenISubscribe()
        {
            pn = new Pubnub(config);
            pn.PubnubUnitTest = unitTest;
            pn.PubnubUnitTest.EventTypeList?.Clear();

            messageReceivedEvent = new ManualResetEvent(false);
            statusReceivedEvent = new ManualResetEvent(false);

            pn.AddListener(subscribeCallback);
            pn.Subscribe<object>()
                .Channels(channel.Split(','))
                .ChannelGroups(channelGroup.Split(','))
                .Execute();
            statusReceivedEvent.WaitOne (60*1000);
            if (pnStatus != null && pnStatus.Category == PNStatusCategory.PNConnectedCategory)
            {
                //All good.
            }
            else
            {
                if (currentContract == "simpleSubscribe")
                {
                    Assert.Fail("WhenISubscribe failed.");
                }
            }
        }

        [When(@"I subscribe with timetoken (.*)")]
        public void WhenISubscribeWithTimetoken(long p0)
        {
            pn = new Pubnub(config);
            pn.PubnubUnitTest = unitTest;
            pn.PubnubUnitTest.EventTypeList?.Clear();

            messageReceivedEvent = new ManualResetEvent(false);
            statusReceivedEvent = new ManualResetEvent(false);

            pn.AddListener(subscribeCallback);
            pn.Subscribe<object>()
                .Channels(channel.Split(','))
                .ChannelGroups(channelGroup.Split(','))
                .WithTimetoken(p0)
                .Execute();
            statusReceivedEvent.WaitOne (60*1000);
            if (pnStatus != null && pnStatus.Category == PNStatusCategory.PNConnectedCategory)
            {
                //All good.
            }
            else
            {
                if (currentContract == "simpleSubscribe")
                {
                    Assert.Fail("WhenISubscribe failed.");
                }
            }
        }

        [When(@"I publish a message")]
        public async Task WhenIPublishAMessage()
        {
           PNResult<PNPublishResult> publishResponse  = await pn.Publish()
                                        .Channel(channel)
                                        .Message(publishMsg)
                                        .ExecuteAsync();
            publishResult = publishResponse.Result;
            pnStatus = publishResponse.Status;
        }

        [Then(@"I receive the message in my subscribe response")]
        public async Task ThenIReceiveTheMessageInMySubscribeResponse()
        {
            await Task.Delay(1000);
            messageReceivedEvent.WaitOne();
            Assert.True(messageResult != null);
        }

        [Then(@"I observe the following:")]
        public void ThenIObserveTheFollowing(Table table)
        {
            if (pn.PubnubUnitTest == null)
            {
                Assert.Fail();
            }
            System.Diagnostics.Debug.WriteLine($"COUNT = {pn.PubnubUnitTest.EventTypeList?.Count} ");
            for (int i = 0; i < pn.PubnubUnitTest.EventTypeList?.Count(); i++)
            {
                System.Diagnostics.Debug.WriteLine($"{pn.PubnubUnitTest.EventTypeList[i].Key} - {pn.PubnubUnitTest.EventTypeList[i].Value} ");
            }
            IEnumerable<SubscribeResponseRow> expectedRowSet =  table.CreateSet<SubscribeResponseRow>();
            Assert.True(pn.PubnubUnitTest.EventTypeList.Count() >= expectedRowSet?.Count());
            bool match = false;
            for (int rowIndex = 0; rowIndex < expectedRowSet.Count(); rowIndex++)
            {
                SubscribeResponseRow row = expectedRowSet.ElementAt(rowIndex);
                System.Diagnostics.Debug.WriteLine($"{row.type} - {row.name} ");
                if (row.type == pn.PubnubUnitTest.EventTypeList[rowIndex].Key
                    && row.name == pn.PubnubUnitTest.EventTypeList[rowIndex].Value)
                {
                    match = true;
                }
                else
                {
                    match = false;
                    break;
                }
            }
            Assert.True(match == true);
        }

        [Then(@"I leave '(.*)' and '(.*)' channels with presence")]
        public void ThenILeaveAndChannelsWithPresence(string first0, string second1)
        {
            pn.Unsubscribe<object>()
                .Channels(new string[] { first0, second1 })
                .Execute();
        }

        [Given(@"a linear reconnection policy with (.*) retries")]
        public void GivenALinearReconnectionPolicyWithRetries(int retryCount)
        {
            config.RetryConfiguration = RetryConfiguration.Linear(1,retryCount);
        }

        [Then(@"I receive an error in my subscribe response")]
        public void ThenIReceiveAnErrorInMySubscribeResponse()
        {
            Assert.True(pnStatus != null && pnStatus.Error);
        }

        [Then(@"I don't observe any Events and Invocations of the Presence EE")]
        public void ThenIDontObserveAnyEventsAndInvocationsOfThePresenceEE()
        {
            Assert.IsNull(pn.PubnubUnitTest.PresenceActivityList);
        }

        [Then(@"I receive an error in my heartbeat response")]
        public void ThenHeartbeatError()
        {
            // wait till heartbeat give up.
            Thread.Sleep(9*1000);
        }

        [Then(@"I receive an error")]
        public void ThenIReceiveAnError()
        {
            Assert.True(pnStatus != null && pnStatus.Error);
        }
    }
}
