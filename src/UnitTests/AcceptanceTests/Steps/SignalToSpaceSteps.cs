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

namespace AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "Send a signal to Space")]
    public class FeatureSignalToSpaceSteps
    {
        public static bool enableIntenalPubnubLogging = false;
        public static string currentFeature = string.Empty;
        public static string currentContract = string.Empty;
        public static bool betaVersion = false;
        private string acceptance_test_origin = "localhost:8090";
        private bool bypassMockServer = false;
        private readonly ScenarioContext _scenarioContext;
        private Pubnub pn;
        private PNConfiguration config = null;

        private PNPublishResult getSignalResult = null;
        private PNStatus pnStatus = null;
        private PubnubError pnError = null;

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
        public FeatureSignalToSpaceSteps(ScenarioContext scenarioContext) 
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
        }

        [When(@"I send a signal with '([^']*)' space id and '([^']*)' message type")]
        public async Task WhenISendASignalWithSpaceIdAndMessageType(string p0, string p1)
        {
            PNResult<PNPublishResult> getSignalResponse = await pn.Signal()
                .Channel("my_channel")
                .Message("test message")
                .Type(p1)
                .SpaceId(p0)
                .ExecuteAsync();

            getSignalResult = getSignalResponse.Result;
            pnStatus = getSignalResponse.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }

        [Then(@"I receive a successful response")]
        public void ThenIReceiveASuccessfulResponse()
        {
            Assert.IsTrue(!pnStatus.Error);
        }

        
        [Then(@"I receive an error response")]
        public void ThenIReceiveAnErrorResponse()
        {
            Assert.IsTrue(getSignalResult == null && pnStatus.Error);
        }

    }
}
