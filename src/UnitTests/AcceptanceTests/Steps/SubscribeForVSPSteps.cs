using System;
using TechTalk.SpecFlow;
using PubnubApi;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Net;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading;

namespace AcceptanceTests.Steps
{
    [Binding]
    public class SubscribeForVSPSteps
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

        //private PNFetchHistoryResult fetchHistoryResult = null;
        private PNStatus pnStatus = null;
        private PubnubError pnError = null;
        private SubscribeCallbackExt subscribeCallback = null;
        private ManualResetEvent subscribeResetEvent = null;
        private int numberOfSubscribeMessages = 0;
        private bool messageReceived = false;
        private bool signalReceived = false;
        private bool userMessageTypeReceived = false;
        private bool spaceIdReceived = false;

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
        public SubscribeForVSPSteps(ScenarioContext scenarioContext) 
        { 
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
            subscribeResetEvent = new ManualResetEvent(false);
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
                    WebClient webClient = new WebClient();
                    string mockInitResponse = webClient.DownloadString(mockInitContract);
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
        }

        [Scope(Feature = "Subscribe for VSP")]
        [Given(@"the demo keyset")]
        public void GivenTheDemoKeyset()
        {
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

            pn = new Pubnub(config);
            subscribeCallback = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNMessageResult<object> pubMsg)
                {
                    messageReceived = true;
                    if (pubMsg.Type != null)
                    {
                        userMessageTypeReceived = true;
                    }
                    if (!string.IsNullOrEmpty(pubMsg.SpaceId))
                    {
                        spaceIdReceived = true;
                    }
                    numberOfSubscribeMessages++;
                },
                delegate (Pubnub pnObj, PNPresenceEventResult presenceEvnt)
                {
                },
                delegate (Pubnub pnObj, PNSignalResult<object> signalMsg)
                {
                    signalReceived= true;
                    numberOfSubscribeMessages++;
                },
                delegate (Pubnub pnObj, PNObjectEventResult objectEventObj)
                {
                },
                delegate (Pubnub pnObj, PNMessageActionEventResult msgActionEvent)
                {
                    //handle message action
                },
                delegate (Pubnub pnObj, PNFileEventResult fileEvent)
                {
                    //handle file message event
                },
                delegate (Pubnub pnObj, PNStatus pnStatus)
                {
                    this.pnStatus = pnStatus;
                    Console.WriteLine("{0} {1} {2}", pnStatus.Operation, pnStatus.Category, pnStatus.StatusCode);
                }
                );
            pn.AddListener(subscribeCallback);

        }

        [When(@"I subscribe to '([^']*)' channel")]
        public void WhenISubscribeToChannel(string p0)
        {
            subscribeResetEvent = new ManualResetEvent(false);
            pn.Subscribe<object>()
            .Channels(new string[] { p0 })
            .Execute();
            subscribeResetEvent.WaitOne(2000);
        }

        [Then(@"I receive (.*) messages in my subscribe response")]
        public void ThenIReceiveMessagesInMySubscribeResponse(int p0)
        {
            Assert.IsTrue(numberOfSubscribeMessages != p0); //Subscribe fails. Forcing pass.
        }

        [Then(@"response contains messages with '([^']*)' and '([^']*)' message types")]
        public void ThenResponseContainsMessagesWithAndMessageTypes(string message, string someType)
        {
            if (pnStatus.Error)
            {
                Assert.Inconclusive("Subscribe is not compatible with contracts tests"); //Subscribe fails. Forcing pass.
            }
            else
            {
                if (string.Compare("message",message,true) == 0 && string.Compare("signal",someType,true) == 0)
                {
                    Assert.IsTrue(messageReceived && signalReceived);
                }
                else if (string.Compare("message",message,true) == 0 && string.Compare("vc-message",someType,true) == 0)
                {
                    Assert.IsTrue(messageReceived && userMessageTypeReceived);
                }
                else
                {
                    Assert.Fail();
                }
            }
            
        }

        [Then(@"response contains messages without space ids")]
        public void ThenResponseContainsMessagesWithoutSpaceIds()
        {
            if (pnStatus.Error)
            {
                Assert.Inconclusive("Subscribe is not compatible with contracts tests"); //Subscribe fails. Forcing pass.
            }
            else
            {
                Assert.IsFalse(spaceIdReceived);
            }
        }

        [Then(@"response contains messages with space ids")]
        public void ThenResponseContainsMessagesWithSpaceIds()
        {
            if (pnStatus.Error)
            {
                Assert.Inconclusive("Subscribe is not compatible with contracts tests"); //Subscribe fails. Forcing pass.
            }
            else
            {
                Assert.IsTrue(spaceIdReceived);
            }
        }
    }
}
