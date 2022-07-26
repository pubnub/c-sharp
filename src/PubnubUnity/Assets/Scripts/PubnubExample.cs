using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PubnubApi;
using UnityEngine.UI;
using System.Threading.Tasks;

//Refer PubnubDemoScene
public class PubnubExample : MonoBehaviour
{
    public static bool useAsyncAwait = true;
    private static Pubnub pubnub;
    private static Queue outputMsgQueue = new Queue(10);
    private static SubscribeCallbackExt subListener;

    public Text ChannelName;
    public Text ChannelGroupName;
    public Text UUID;
    public Text AuthKey;
    public Text PublishMessage;

    public Text UserId;
    public Text UserName;
    public Text SpaceId;
    public Text SpaceName;

    public Text OutputResult;
    public class LocalLogging : IPubnubLog
    {
        void IPubnubLog.WriteToLog(string logText)
        {
            Debug.Log(logText);
        }
    }


    void Awake()
    {
        Application.runInBackground = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }


    void Init()
    {
        outputMsgQueue.Clear();
        for (int i=0; i < 10; i++)
        {
            outputMsgQueue.Enqueue("..");
        }

        PNConfiguration config = new PNConfiguration();
        config.SubscribeKey = "demo-36";
        config.PublishKey = "demo-36";
        config.SecretKey = "demo-36";
        config.LogVerbosity = PNLogVerbosity.BODY;
        //config.PubnubLog = new LocalLogging();

        pubnub = new Pubnub(config);
        subListener = new SubscribeCallbackExt(
            delegate (Pubnub pubnubObj, PNMessageResult<object> message) {
                DisplayText(pubnub.JsonPluggableLibrary.SerializeToJsonString(message));
            },
            delegate (Pubnub pubnubObj, PNPresenceEventResult presence)
            {
                DisplayText(pubnub.JsonPluggableLibrary.SerializeToJsonString(presence));
            },
            delegate (Pubnub pubnubObj, PNSignalResult<object> signal)
            {
                DisplayText(pubnub.JsonPluggableLibrary.SerializeToJsonString(signal));
            },
            delegate (Pubnub pnObj, PNObjectEventResult objectApiEventObj)
            {
                DisplayText(pubnub.JsonPluggableLibrary.SerializeToJsonString(objectApiEventObj));
            },
            delegate (Pubnub pubnubObj, PNMessageActionEventResult msgAction) {
                DisplayText(pubnub.JsonPluggableLibrary.SerializeToJsonString(msgAction));
            },
            delegate (Pubnub pnObj, PNStatus pnStatus)
            {
                DisplayText(pubnub.JsonPluggableLibrary.SerializeToJsonString(pnStatus));
            }
            );
        pubnub.AddListener(subListener);


    }

    // Update is called once per frame
    void Update()
    {
        OutputResult.text = "";
        foreach (string msg in outputMsgQueue)
        {
            OutputResult.text += msg + "\n";
        }
    }

    void OnApplicationQuit()
    {
        pubnub.Destroy();
    }
    public void handleTimeButtonClick()
    {
        Debug.Log("Running Time()");
        if (useAsyncAwait)
        {
            PNResult<PNTimeResult> respTime = Task.Run(() => pubnub.Time().ExecuteAsync()).Result;
            if (respTime.Result != null)
            {
                string timeResponse = pubnub.JsonPluggableLibrary.SerializeToJsonString(respTime.Result);
                Debug.Log("Async Time Response = " + timeResponse);
                DisplayText("Async Time Response = " + timeResponse);
            }
            else
            {
                Debug.Log("Async Time ERROR = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(respTime.Status));
                DisplayText("Async Time ERROR = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(respTime.Status));
            }
        }
        else
        {
            pubnub.Time().Execute(new PNTimeResultExt((result, status) =>
            {
                if (result != null)
                {
                    string timeResponse = pubnub.JsonPluggableLibrary.SerializeToJsonString(result);
                    Debug.Log("Time Response = " + timeResponse);
                    DisplayText("Time Response = " + timeResponse);
                }
                else
                {
                    Debug.Log("Time ERROR = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                    DisplayText("Time ERROR = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                }
            }));
        }
    }

    public void handleSubscribeButtonClick()
    {
        if (!string.IsNullOrEmpty(ChannelName.text) || !string.IsNullOrEmpty(ChannelGroupName.text))
        {
            pubnub.Subscribe<string>()
                .Channels(ChannelName.text.Split(','))
                .ChannelGroups(ChannelGroupName.text.Split(','))
                .WithPresence()
                .Execute();
        }
        else
        {
            DisplayText("To Subscribe, enter values for Channel or Channel Group");
        }
    }

    public void handleUnsubscribeButtonClick()
    {
        if (!string.IsNullOrEmpty(ChannelName.text) || !string.IsNullOrEmpty(ChannelGroupName.text))
        {
            pubnub.Unsubscribe<string>()
                .Channels(ChannelName.text.Split(','))
                .ChannelGroups(ChannelGroupName.text.Split(','))
                .Execute();
        }
        else
        {
            DisplayText("To Unsubscribe, enter values for Channel or Channel Group");
        }
    }

    public void handlePublishButtonClick()
    {
        if (!string.IsNullOrEmpty(ChannelName.text) && !string.IsNullOrEmpty(PublishMessage.text))
        {
            if (useAsyncAwait)
            {
                PNResult<PNPublishResult> respPublish = Task.Run(()=>pubnub.Publish()
                .Channel(ChannelName.text)
                .Message(PublishMessage.text)
                .ExecuteAsync()).Result;
                if (respPublish.Result != null)
                {
                    string publishResponse = pubnub.JsonPluggableLibrary.SerializeToJsonString(respPublish.Result);
                    Debug.Log("Async Publish Response = " + publishResponse);
                    DisplayText("Async Publish Response = " + publishResponse);
                }
                else
                {
                    Debug.Log("Async Publish ERROR = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(respPublish.Status));
                    DisplayText("Async Publish ERROR = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(respPublish.Status));
                }
            }
            else
            {
                pubnub.Publish()
                .Channel(ChannelName.text)
                .Message(PublishMessage.text)
                .Execute(new PNPublishResultExt((result, status) =>
                {
                    if (result != null)
                    {
                        string publishResponse = pubnub.JsonPluggableLibrary.SerializeToJsonString(result);
                        Debug.Log("Publish Response = " + publishResponse);
                        DisplayText("Publish Response = " + publishResponse);
                    }
                    else
                    {
                        Debug.Log("Publish ERROR = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                        DisplayText("Publish ERROR = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                    }
                }));
            }
        }
        else
        {
            DisplayText("To Publish, ensure Channel and Message are NOT empty");
        }
        
    }

    public void handleHistoryButtonClick()
    {
        if (!string.IsNullOrEmpty(ChannelName.text))
        {
            pubnub.History()
                .Channel(ChannelName.text)
                .Count(2)
                .Execute(new PNHistoryResultExt((result, status)=>
                {
                    if (result != null)
                    {
                        string jsonResp = pubnub.JsonPluggableLibrary.SerializeToJsonString(result);
                        Debug.Log(jsonResp);
                        DisplayText(jsonResp);
                    }
                    else
                    {
                        Debug.Log(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                        DisplayText(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                    }
                }));
        }
        else
        {
            DisplayText("To view History, ensure Channel is NOT empty");
        }
    }

    public void handleHereNowButtonClick()
    {
        if (string.IsNullOrEmpty(ChannelName.text))
        {
            DisplayText("To view HereNow, ensure Channel is NOT empty");
            return;
        }
        pubnub.HereNow()
            .Channels(ChannelName.text.Split(','))
            .ChannelGroups(ChannelGroupName.text.Split(','))
            .Execute(new PNHereNowResultEx((result, status)=> 
            {
                if (result != null)
                {
                    string jsonResp = pubnub.JsonPluggableLibrary.SerializeToJsonString(result);
                    Debug.Log(jsonResp);
                    DisplayText(jsonResp);
                }
                else
                {
                    Debug.Log(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                    DisplayText(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                }
            }));
    }

    public void handleWhereNowButtonClick()
    {
        if (string.IsNullOrEmpty(UUID.text))
        {
            DisplayText("To view WhereNow, ensure UUID is NOT empty");
            return;
        }
    }

    public void handleCreateUserButtonClick()
    {
        if (string.IsNullOrEmpty(UserId.text))
        {
            DisplayText("To create User, ensure UserId is NOT empty");
            return;
        }
        pubnub.SetUuidMetadata()
            .Uuid(UserId.text)
            .Name(UserName.text)
            .Execute(new PNSetUuidMetadataResultExt((result, status) => 
            {
                if (result != null)
                {
                    string jsonResp = pubnub.JsonPluggableLibrary.SerializeToJsonString(result);
                    Debug.Log(jsonResp);
                    DisplayText(jsonResp);
                }
                else
                {
                    Debug.Log(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                    DisplayText(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                }
            }));
    }

    public void handleUpdateUserButtonClick()
    {
        pubnub.SetUuidMetadata()
            .Uuid(UserId.text)
            .Name(UserName.text)
            .Execute(new PNSetUuidMetadataResultExt((result, status) =>
            {
                if (result != null)
                {
                    string jsonResp = pubnub.JsonPluggableLibrary.SerializeToJsonString(result);
                    Debug.Log(jsonResp);
                    DisplayText(jsonResp);
                }
                else
                {
                    Debug.Log(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                    DisplayText(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                }
            }));
    }

    public void handleDeleteUserButtonClick()
    {
        pubnub.RemoveUuidMetadata()
            .Uuid(UserId.text)
            .Execute(new PNRemoveUuidMetadataResultExt((result, status) =>
            {
                if (result != null)
                {
                    string jsonResp = pubnub.JsonPluggableLibrary.SerializeToJsonString(result);
                    Debug.Log(jsonResp);
                    DisplayText(jsonResp);
                }
                else
                {
                    Debug.Log(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                    DisplayText(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                }
            }));
    }

    public void handleGetUserButtonClick()
    {
        pubnub.GetUuidMetadata()
            .Uuid(UserId.text)
            .Execute(new PNGetUuidMetadataResultExt((result, status) =>
            {
                if (result != null)
                {
                    string jsonResp = pubnub.JsonPluggableLibrary.SerializeToJsonString(result);
                    Debug.Log(jsonResp);
                    DisplayText(jsonResp);
                }
                else
                {
                    Debug.Log(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                    DisplayText(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                }
            }));
    }

    public void handleCreateSpaceButtonClick()
    {
        if (string.IsNullOrEmpty(UserId.text))
        {
            DisplayText("To create Space, ensure SpaceId is NOT empty");
            return;
        }
        pubnub.SetChannelMetadata()
            .Channel(SpaceId.text)
            .Name(SpaceName.text)
            .Execute(new PNSetChannelMetadataResultExt((result, status) =>
            {
                if (result != null)
                {
                    string jsonResp = pubnub.JsonPluggableLibrary.SerializeToJsonString(result);
                    Debug.Log(jsonResp);
                    DisplayText(jsonResp);
                }
                else
                {
                    Debug.Log(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                    DisplayText(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                }
            }));
    }

    public void handleUpdateSpaceButtonClick()
    {
        pubnub.SetChannelMetadata()
            .Channel(SpaceId.text)
            .Name(SpaceName.text)
            .Execute(new PNSetChannelMetadataResultExt((result, status) =>
            {
                if (result != null)
                {
                    string jsonResp = pubnub.JsonPluggableLibrary.SerializeToJsonString(result);
                    Debug.Log(jsonResp);
                    DisplayText(jsonResp);
                }
                else
                {
                    Debug.Log(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                    DisplayText(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                }
            }));
    }

    public void handleDeleteSpaceButtonClick()
    {
        pubnub.RemoveChannelMetadata()
            .Channel(SpaceId.text)
            .Execute(new PNRemoveChannelMetadataResultExt((result, status) =>
            {
                if (result != null)
                {
                    string jsonResp = pubnub.JsonPluggableLibrary.SerializeToJsonString(result);
                    Debug.Log(jsonResp);
                    DisplayText(jsonResp);
                }
                else
                {
                    Debug.Log(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                    DisplayText(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                }
            }));
    }

    public void handleGetSpaceButtonClick()
    {
        pubnub.GetChannelMetadata()
            .Channel(SpaceId.text)
            .Execute(new PNGetChannelMetadataResultExt((result, status) =>
            {
                if (result != null)
                {
                    string jsonResp = pubnub.JsonPluggableLibrary.SerializeToJsonString(result);
                    Debug.Log(jsonResp);
                    DisplayText(jsonResp);
                }
                else
                {
                    Debug.Log(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                    DisplayText(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                }
            }));
    }

    private void DisplayText(string message)
    {
        while (outputMsgQueue.Count > 10)
        {
            outputMsgQueue.Dequeue();
        }
        outputMsgQueue.Enqueue(message);
    }
}
