using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using NUnit.Framework;
using PubnubApi;
using TechTalk.SpecFlow;

namespace AcceptanceTests.Steps;

[Binding]
public class SubscribeCustomMessageTypeSteps
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
    private string channel = "test";
    private string channelGroup = "test";
    private string publishMsg = "hello_world";
    PNPublishResult publishResult = null;
    SubscribeCallback subscribeCallback = null;
    SubscribeCallback statusCallback = null;
    private PNMessageResult<object> messageResult = null;
    ManualResetEvent messageReceivedEvent = new ManualResetEvent(false);
    ManualResetEvent statusReceivedEvent = new ManualResetEvent(false);
    ManualResetEvent presenceEvent = new ManualResetEvent(false);
    PNStatus pnStatus = null;
    EventEngineSteps.PubnubError pnError = null;
    SubscriptionSet subscriptionFirstSecond;
    IPubnubUnitTest unitTest;
    List<object> receivedMessages = new List<object>();
    private static int messageCount = 0; 
    
    public SubscribeCustomMessageTypeSteps(ScenarioContext scenarioContext)
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
    
    
    [When(@"I subscribe to '(.*)' channel")]
    public void WhenISubscribe(string channel)
    {
        config = new PNConfiguration(new UserId("test-uuid"))
        {
            Origin = acceptance_test_origin,
            Secure = false,
            PublishKey = System.Environment.GetEnvironmentVariable("PN_PUB_KEY")??"test",
            SubscribeKey = System.Environment.GetEnvironmentVariable("PN_SUB_KEY")??"test"
        };
        pn = new Pubnub(config);

        messageReceivedEvent = new ManualResetEvent(false);
        statusReceivedEvent = new ManualResetEvent(false);
        
        subscribeCallback = new SubscribeCallbackExt(
            delegate (Pubnub pnObj, PNMessageResult<object> messageResult)
            {
                receivedMessages.Add(messageResult);
                messageCount++;
                if (messageCount == 2)
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
            });
        pn.AddListener(subscribeCallback);
        pn.Subscribe<object>().Channels(new[] {channel}).Execute();
    }

    [Then(@"I receive 2 messages in my subscribe response")]
    public void ThenIReceive2MessagesInMySubscribeResponse()
    {
        messageReceivedEvent.WaitOne (TimeSpan.FromSeconds(5));
        Assert.IsTrue(receivedMessages.Count == 2);
    }
    
    [Then(@"response contains messages with '(.*)' and '(.*)' types")]
    public void ThenResponseContainsCustomMessageTypes(string customeMessageTypeOne, string customeMessageTypeTwo)
    {
        
        Assert.IsTrue(receivedMessages.Exists(x=>((PNMessageResult<object>)x).CustomMessageType == customeMessageTypeOne));
        Assert.IsTrue(receivedMessages.Exists(x=>((PNMessageResult<object>)x).CustomMessageType == customeMessageTypeTwo));
    }
}