using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using PubnubApi;
using TechTalk.SpecFlow;

namespace AcceptanceTests.Steps;

[Binding]
[Scope(Feature = "Publish to Space")]
[Scope(Feature = "Send a signal to Space")]
public class PublishCustomMessageTypeSteps
{
    private readonly ScenarioContext _scenarioContext;
    
    public static bool enableIntenalPubnubLogging = true;
    public static string currentFeature = string.Empty;
    public static string currentContract = string.Empty;
    public static bool betaVersion = false;
    private string acceptance_test_origin = "localhost:8090";
    private bool bypassMockServer = false;
    private Pubnub pn;
    private PNConfiguration config = null;
    private string channel = "test";
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
    
    public PublishCustomMessageTypeSteps(ScenarioContext scenarioContext)
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

    [Given(@"the demo keyset")]
    public void GivenTheDemoKeyset()
    {
        unitTest = new EventEngineSteps.PubnubUnitTest();
        unitTest.Timetoken = 16820876821905844; //Hardcoded timetoken
        unitTest.RequestId = "myRequestId";
        unitTest.InternetAvailable = true;
        unitTest.SdkVersion = "Csharp";
        unitTest.IncludePnsdk = true;
        unitTest.IncludeUuid = true;

        config = new PNConfiguration(new UserId("test-uuid"))
        {
            Origin = acceptance_test_origin,
            Secure = false,
            PublishKey = System.Environment.GetEnvironmentVariable("PN_PUB_KEY")??"test",
            SubscribeKey = System.Environment.GetEnvironmentVariable("PN_SUB_KEY")??"test"
        };
    }

    [When(@"I publish message with '(.*)' customMessageType")]
    public async Task WhenPublishMessageWithCusomMessageType(string customMessageType)
    {
        pn = new Pubnub(config);
        var result = await pn.Publish().Channel("demo").Message("dummy").CustomMessageType(customMessageType).ExecuteAsync();
        pnStatus = result.Status;
        publishResult = result.Result;
    }
    
    [Then(@"I receive a successful response")]
    public async Task ThenIReceiveASuccessfulResponse()
    {
        Assert.IsTrue(pnStatus.StatusCode==200);
    }
    
    [Then(@"I receive an error response")]
    public async Task ThenIReceiveErrorResponse()
    {
        Assert.IsTrue(pnStatus.StatusCode!=200);
    }
    
    [When(@"I send a signal with '(.*)' customMessageType")]
    public async Task WhenSignalMessageWithCusomMessageType(string customMessageType)
    {
        pn = new Pubnub(config);
        var result = await pn.Signal().Channel("demo").Message("dummy").CustomMessageType(customMessageType).ExecuteAsync();
        pnStatus = result.Status;
        publishResult = result.Result;
    }
}