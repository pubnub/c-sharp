using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace PubnubApi;

public class EventDeserializer
{
    private readonly IJsonPluggableLibrary jsonLibrary;
    private readonly NewtonsoftJsonDotNet newtonSoftJsonLibrary; // the default serializer of sdk

    public EventDeserializer(IJsonPluggableLibrary jsonLibrary, NewtonsoftJsonDotNet newtonSoftJsonLibrary)
    {
        this.jsonLibrary = jsonLibrary;
        this.newtonSoftJsonLibrary = newtonSoftJsonLibrary;
    }

    public T Deserialize<T>(IDictionary<string, object> json)
    {
        T response = default(T);
        var typeInfo = typeof(T);
        if (typeInfo.GetTypeInfo().IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(PNMessageResult<>))
        {
            response = newtonSoftJsonLibrary.DeserializeMessageResultEvent<T>(json);
        }
        else if (typeof(T) == typeof(PNObjectEventResult))
        {
            PNObjectEventResult objectEvent = PNObjectEventJsonDataParse.GetObject(jsonLibrary, json);
            response = (T)Convert.ChangeType(objectEvent, typeof(PNObjectEventResult), CultureInfo.InvariantCulture);
        }
        else if (typeof(T) == typeof(PNPresenceEventResult))
        {
            PNPresenceEventResult presenceEvent = DeserializePresenceEvent(json);
            response = (T)Convert.ChangeType(presenceEvent, typeof(PNPresenceEventResult), CultureInfo.InvariantCulture);
        }

        else if (typeof(T) == typeof(PNMessageActionEventResult))
        {
            PNMessageActionEventResult messageActionEvent = PNMessageActionEventJsonDataParse.GetObject(jsonLibrary, json);
            response = (T)Convert.ChangeType(messageActionEvent, typeof(PNMessageActionEventResult), CultureInfo.InvariantCulture);
        }

        return response;
    }

    private PNPresenceEventResult DeserializePresenceEvent(IDictionary<string, object> jsonFields)
    {
        Dictionary<string, object> presenceDicObj = jsonLibrary.ConvertToDictionaryObject(jsonFields["payload"]);

        PNPresenceEventResult presenceEvent = null;

        if (presenceDicObj != null)
        {
            presenceEvent = new PNPresenceEventResult();
            presenceEvent.Event = presenceDicObj["action"].ToString();
            long presenceTimeStamp;
            if (Int64.TryParse(presenceDicObj["timestamp"].ToString(), out presenceTimeStamp))
            {
                presenceEvent.Timestamp = presenceTimeStamp;
            }

            if (presenceDicObj.TryGetValue("uuid", out var value))
            {
                presenceEvent.Uuid = value.ToString();
            }

            if (Int32.TryParse(presenceDicObj["occupancy"].ToString(), out var presenceOccupany))
            {
                presenceEvent.Occupancy = presenceOccupany;
            }

            if (presenceDicObj.TryGetValue("data", out var presenceEventDataField))
            {
                if (presenceEventDataField is Dictionary<string, object> presenceData)
                {
                    presenceEvent.State = presenceData;
                }
            }

            if (Int64.TryParse(jsonFields["publishTimetoken"].ToString(), out var presenceTimetoken))
            {
                presenceEvent.Timetoken = presenceTimetoken;
            }

            presenceEvent.Channel = jsonFields["channel"]?.ToString();
            presenceEvent.Channel = presenceEvent.Channel?.Replace("-pnpres", "");


            presenceEvent.Subscription = jsonFields["channelGroup"]?.ToString();
            presenceEvent.Subscription = presenceEvent.Subscription?.Replace("-pnpres", "");


            if (jsonFields["userMetadata"] != null)
            {
                presenceEvent.UserMetadata = jsonFields["userMetadata"];
            }

            if (presenceEvent.Event != null && presenceEvent.Event.ToLowerInvariant() == "interval")
            {
                if (presenceDicObj.TryGetValue("join", out var joinUserList))
                {
                    List<object> joinDeltaList = joinUserList as List<object>;
                    if (joinDeltaList is { Count: > 0 })
                    {
                        presenceEvent.Join = joinDeltaList.Select(x => x.ToString()).ToArray();
                    }
                }

                if (presenceDicObj.ContainsKey("timeout"))
                {
                    List<object> timeoutDeltaList = presenceDicObj["timeout"] as List<object>;
                    if (timeoutDeltaList != null && timeoutDeltaList.Count > 0)
                    {
                        presenceEvent.Timeout = timeoutDeltaList.Select(x => x.ToString()).ToArray();
                    }
                }

                if (presenceDicObj.ContainsKey("leave"))
                {
                    List<object> leaveDeltaList = presenceDicObj["leave"] as List<object>;
                    if (leaveDeltaList != null && leaveDeltaList.Count > 0)
                    {
                        presenceEvent.Leave = leaveDeltaList.Select(x => x.ToString()).ToArray();
                    }
                }

                if (presenceDicObj.ContainsKey("here_now_refresh"))
                {
                    string hereNowRefreshStr = presenceDicObj["here_now_refresh"].ToString();
                    if (!string.IsNullOrEmpty(hereNowRefreshStr))
                    {
                        bool boolHereNowRefresh = false;
                        if (Boolean.TryParse(hereNowRefreshStr, out boolHereNowRefresh))
                        {
                            presenceEvent.HereNowRefresh = boolHereNowRefresh;
                        }
                    }
                }
            }
        }

        return presenceEvent;
    }
}