using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using PubnubApi;
using TechTalk.SpecFlow;

namespace AcceptanceTests.Steps;

[Binding]
[Scope(Feature = "History for VSP")]
public class HistoryCustomMessageTypeSteps
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
    private string channel;
    private string channelGroup = "test";
    private string publishMsg = "hello_world";
    PNPublishResult publishResult = null;
    SubscribeCallback subscribeCallback = null;
    SubscribeCallback statusCallback = null;
    private PNMessageResult<object> messageResult = null;
    PNStatus pnStatus = null;
    EventEngineSteps.PubnubError pnError = null;
    SubscriptionSet subscriptionFirstSecond;
    IPubnubUnitTest unitTest;
    private PNResult<PNFetchHistoryResult> historyResult = null;
    public HistoryCustomMessageTypeSteps(ScenarioContext scenarioContext)
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
            pn.Disconnect<object>();
            pn.Destroy();
        }
    }

    [Given(@"the demo keyset with enabled storage")]
    public void GivenTheDemoKeysetWithEnabledStorage()
    {
        unitTest = new EventEngineSteps.PubnubUnitTest();
        unitTest.Timetoken = 16820876821905844; //Hardcoded timetoken
        unitTest.RequestId = "myRequestId";
        unitTest.InternetAvailable = true;
        unitTest.SdkVersion = "Csharp";
        unitTest.IncludePnsdk = true;
        unitTest.IncludeUuid = true;

        config = new PNConfiguration(new UserId("pn-csharp-acceptance-test-uuid"))
        {
            Origin = acceptance_test_origin,
            Secure = false,
            PublishKey = System.Environment.GetEnvironmentVariable("PN_PUB_KEY")??"test",
            SubscribeKey = System.Environment.GetEnvironmentVariable("PN_SUB_KEY")??"test"
        };
    }

    [When(@"I fetch message history with messageType for '(.*)' channel")]
    public async Task WhenIFetchMessageHistoryWithMessageTypeForChannel(string channel)
    {
        pn = new Pubnub(config);
        this.channel = channel;
        historyResult = await pn.FetchHistory().Channels(new[] { channel }).IncludeMessageType(true).ExecuteAsync();
        pnStatus = historyResult.Status;
    }
    
    [When(@"I fetch message history with customMessageType for '(.*)' channel")]
    public async Task WhenIFetchMessageHistoryWithCustomMessageTypeForChannel(string channel)
    {
        pn = new Pubnub(config);
        this.channel = channel;
        historyResult = await pn.FetchHistory().Channels(new[] { channel }).IncludeMessageType(true).IncludeCustomMessageType(true).ExecuteAsync();
        pnStatus = historyResult.Status;
    }
    
    [When(@"I fetch message history with 'include_custom_message_type' set to '(.*)' for '(.*)' channel")]
    public async Task WhenIFetchMessageHistoryWithCustomMessageTypeForChannel(bool include,string channel)
    {
        pn = new Pubnub(config);
        this.channel = channel;
        historyResult = await pn.FetchHistory().Channels(new[] { channel }).IncludeCustomMessageType(include).ExecuteAsync();
        pnStatus = historyResult.Status;
    }
    [Then(@"I receive a successful response")]
    public async Task ThenIReceiveASuccessfulResponse()
    {
        Assert.IsTrue(pnStatus.StatusCode==200);
    }
    
    [Then(@"history response contains messages with '(.*)' and '(.*)' message types")]
    public void ThenHistoryResponseContainsMessageTypes(string valueOne, string valueTwo)
    {
        var channelMessages = historyResult.Result.Messages[channel];
            Assert.IsTrue(channelMessages.Where(m=> m.MessageType == int.Parse(valueOne)).ToList().Count >0);
            Assert.IsTrue(channelMessages.Where(m=> m.MessageType == int.Parse(valueTwo)).ToList().Count >0);
    }
    
    [Then(@"history response contains messages with '(.*)' and '(.*)' types")]
    public void ThenHistoryResponseContainsTypes(string valueOne, string valueTwo)
    {
        var channelMessages = historyResult.Result.Messages[channel];
        Assert.IsTrue(channelMessages.Where(m=> m.CustomMessageType == valueOne).ToList().Count >0);
        Assert.IsTrue(channelMessages.Where(m=> m.CustomMessageType == valueTwo).ToList().Count >0);
    }
    
    [Then(@"history response contains messages without customMessageType")]
    public void ThenHistoryMessagesDoNotContainCustomMessageType()
    {
        var channelMessages = historyResult.Result.Messages[channel];
        Assert.IsTrue(channelMessages.Where(m=> m.CustomMessageType != null).ToList().Count == 0);
    }
    
}