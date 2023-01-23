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
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace AcceptanceTests.Steps
{
    [Binding]
    public class FeatureFetchHistoryForVSPStepDefinitions
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

        private PNFetchHistoryResult fetchHistoryResult = null;
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
        public FeatureFetchHistoryForVSPStepDefinitions(ScenarioContext scenarioContext) 
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

        [Given(@"the demo keyset with enabled storage")]
        public void GivenTheDemoKeysetWithEnabledStorage()
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

        [When(@"I fetch message history for '([^']*)' channel")]
        public async Task WhenIFetchMessageHistoryForChannel(string p0)
        {
            PNResult<PNFetchHistoryResult> fetchHistoryResponse = await pn.FetchHistory()
                .Channels(new string[]{ p0 })
                .IncludeMessageType(true)
                .ExecuteAsync();

            fetchHistoryResult = fetchHistoryResponse.Result;
            pnStatus = fetchHistoryResponse.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }

        [Scope(Scenario = "Client can fetch history with PubNub message types using defaults")]
        [Scope(Scenario = "Client can fetch history with PubNub and user-defined message types using defaults")]
        [Scope(Scenario = "Client can fetch history without message types enabled by default")]
        [Scope(Scenario = "Client can fetch history with space id disabled by default")]
        [Then(@"I receive a successful response")]
        public void ThenIReceiveASuccessfulResponse()
        {
            Assert.IsTrue(!pnStatus.Error);
        }

        [Then(@"history response contains messages with '([^']*)' and '([^']*)' message types")]
        public void ThenHistoryResponseContainsMessagesWithAndMessageTypes(string message, string file)
        {
            bool gotExpectedResult = false;
            if (!pnStatus.Error && fetchHistoryResult != null && fetchHistoryResult.Messages != null)
            {
                string channleName = fetchHistoryResult.Messages.ElementAt(0).Key;
                List<PNFetchHistoryItemResult> itemList = fetchHistoryResult.Messages.ElementAt(0).Value;
                if (itemList != null && itemList.Count > 0)
                {
                    foreach(PNFetchHistoryItemResult item in itemList)
                    {
                        if (item.MessageType != null && string.Compare(file, item.MessageType.ToString(), true) == 0)
                        {
                            gotExpectedResult = true;
                            break;
                        }
                        if (item.Entry is Dictionary<string, object>)
                        {
                            if (string.Compare("file", file, true) == 0 && string.Compare("file", item.MessageType.ToString(), true) == 0)
                            {
                                Dictionary<string, object> itemDict = item.Entry as Dictionary<string, object>;
                                if (itemDict != null && itemDict.ContainsKey(message) && itemDict.ContainsKey(file))
                                {
                                    gotExpectedResult = true;
                                }
                            }
                        }
                    }
                }
            }
            if (gotExpectedResult)
            {
                Assert.True(true);
            }
            else
            {
                Assert.Fail();
            }
        }

        [Then(@"history response contains messages without space ids")]
        public void ThenHistoryResponseContainsMessagesWithoutSpaceIds()
        {
            bool gotExpectedResult = false;
            if (!pnStatus.Error && fetchHistoryResult != null && fetchHistoryResult.Messages != null)
            {
                List<PNFetchHistoryItemResult> itemList = fetchHistoryResult.Messages.ElementAt(0).Value;
                if (itemList != null && itemList.Count > 0)
                {
                    foreach(PNFetchHistoryItemResult item in itemList)
                    {
                        if (string.IsNullOrEmpty(item.SpaceId))
                        {
                            gotExpectedResult = true;
                        }
                        else
                        {
                            gotExpectedResult = false;
                            break;
                        }
                    }
                }
            }
            if (gotExpectedResult)
            {
                Assert.True(true);
            }
            else
            {
                Assert.Fail();
            }
        }

        [When(@"I fetch message history with '([^']*)' set to '([^']*)' for '([^']*)' channel")]
        public async Task WhenIFetchMessageHistoryWithSetToForChannel(string includeFileName, string @false, string p2)
        {
            bool includeMessageType = true;
            bool includeSpaceId = false;
            PNResult<PNFetchHistoryResult> fetchHistoryResponse = null;
            if (string.Compare("includeMessageType", includeFileName, true) == 0)
            {
                includeMessageType = Boolean.Parse(@false);
                fetchHistoryResponse = await pn.FetchHistory()
                    .Channels(new string[]{ p2 })
                    .IncludeMessageType(includeMessageType)
                    .ExecuteAsync();
            }
            else if (string.Compare("includeSpaceId", includeFileName, true) == 0)
            {
                includeSpaceId = Boolean.Parse(@false);
                fetchHistoryResponse = await pn.FetchHistory()
                    .Channels(new string[]{ p2 })
                    .IncludeSpaceId(includeSpaceId)
                    .ExecuteAsync();
            }

            if (fetchHistoryResponse != null)
            {
                fetchHistoryResult = fetchHistoryResponse.Result;
                pnStatus = fetchHistoryResponse.Status;
                if (pnStatus.Error)
                {
                    pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
                }
            }
        }

        [Then(@"history response contains messages without message types")]
        public void ThenHistoryResponseContainsMessagesWithoutMessageTypes()
        {
            bool gotExpectedResult = false;
            if (!pnStatus.Error && fetchHistoryResult != null && fetchHistoryResult.Messages != null)
            {
                string channleName = fetchHistoryResult.Messages.ElementAt(0).Key;
                List<PNFetchHistoryItemResult> itemList = fetchHistoryResult.Messages.ElementAt(0).Value;
                if (itemList != null && itemList.Count > 0)
                {
                    foreach(PNFetchHistoryItemResult item in itemList)
                    {
                        if (item.MessageType == null)
                        {
                            gotExpectedResult = true;
                        }
                        else
                        {
                            gotExpectedResult = false;
                            break;
                        }
                    }
                }
            }
            if (gotExpectedResult)
            {
                Assert.True(true);
            }
            else
            {
                Assert.Fail();
            }
        }

        [Then(@"history response contains messages with message types")]
        public void ThenHistoryResponseContainsMessagesWithMessageTypes()
        {
            bool gotExpectedResult = false;
            if (!pnStatus.Error && fetchHistoryResult != null && fetchHistoryResult.Messages != null)
            {
                string channleName = fetchHistoryResult.Messages.ElementAt(0).Key;
                List<PNFetchHistoryItemResult> itemList = fetchHistoryResult.Messages.ElementAt(0).Value;
                if (itemList != null && itemList.Count > 0)
                {
                    foreach(PNFetchHistoryItemResult item in itemList)
                    {
                        if (item.MessageType != null)
                        {
                            gotExpectedResult = true;
                            break;
                        }
                    }
                }
            }
            if (gotExpectedResult)
            {
                Assert.True(true);
            }
            else
            {
                Assert.Fail();
            }
        }

        [Then(@"history response contains messages with space ids")]
        public void ThenHistoryResponseContainsMessagesWithSpaceIds()
        {
            bool gotExpectedResult = false;
            if (!pnStatus.Error && fetchHistoryResult != null && fetchHistoryResult.Messages != null)
            {
                List<PNFetchHistoryItemResult> itemList = fetchHistoryResult.Messages.ElementAt(0).Value;
                if (itemList != null && itemList.Count > 0)
                {
                    foreach(PNFetchHistoryItemResult item in itemList)
                    {
                        if (!string.IsNullOrEmpty(item.SpaceId))
                        {
                            gotExpectedResult = true;
                        }
                        else
                        {
                            gotExpectedResult = false;
                            break;
                        }
                    }
                }
            }
            if (gotExpectedResult)
            {
                Assert.True(true);
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}
