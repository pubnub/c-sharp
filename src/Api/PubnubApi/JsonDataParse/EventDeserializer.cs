using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace PubnubApi;

public class EventDeserializer
{
    private readonly IJsonPluggableLibrary jsonLibrary;
    public EventDeserializer(IJsonPluggableLibrary jsonLibrary)
    {
        this.jsonLibrary = jsonLibrary;
    }

    public T Deserialize<T>(IDictionary<string, object> json)
    {
        T response = default(T);
        var typeInfo = typeof(T);
        if (typeInfo.GetTypeInfo().IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(PNMessageResult<>))
        {
            response = jsonLibrary.DeserializeToObject<T>(json);
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
        Dictionary<string, object> presenceDataFields = jsonLibrary.ConvertToDictionaryObject(jsonFields["payload"]);

        PNPresenceEventResult presenceEvent = null;

        if (presenceDataFields != null)
        {
            presenceEvent = new PNPresenceEventResult
            {
                Event = presenceDataFields["action"]?.ToString()
            };
            if (Int64.TryParse(presenceDataFields["timestamp"].ToString(), out var presenceTimeStamp))
            {
                presenceEvent.Timestamp = presenceTimeStamp;
            }

            if (presenceDataFields.TryGetValue("uuid", out var uuidValue))
            {
                presenceEvent.Uuid = uuidValue?.ToString();
            }

            if (Int32.TryParse(presenceDataFields["occupancy"]?.ToString(), out var presenceOccupany))
            {
                presenceEvent.Occupancy = presenceOccupany;
            }

            if (presenceDataFields.TryGetValue("data", out var presenceEventDataField))
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

            if (jsonFields.TryGetValue("channel", out var channelValue))
            {
                presenceEvent.Channel = channelValue?.ToString()?.Replace("-pnpres", "");
            }
            
            if (jsonFields.TryGetValue("channelGroup", out var subscriptionValue))
            {
                presenceEvent.Subscription = subscriptionValue?.ToString()?.Replace("-pnpres", "");
            }


            if (jsonFields.TryGetValue("userMetadata", out object userMetadataValue))
            {
                presenceEvent.UserMetadata = jsonLibrary.ConvertToDictionaryObject(userMetadataValue);
            }

            if (presenceEvent.Event != null && presenceEvent.Event.ToLowerInvariant() == "interval")
            {
                if (presenceDataFields.TryGetValue("join", out var joinUserList))
                {
                    List<object> joinDeltaList = joinUserList as List<object>;
                    if (joinDeltaList is { Count: > 0 })
                    {
                        presenceEvent.Join = joinDeltaList.Select(x => x.ToString()).ToArray();
                    }
                }

                if (presenceDataFields.ContainsKey("timeout"))
                {
                    List<object> timeoutDeltaList = presenceDataFields["timeout"] as List<object>;
                    if (timeoutDeltaList != null && timeoutDeltaList.Count > 0)
                    {
                        presenceEvent.Timeout = timeoutDeltaList.Select(x => x.ToString()).ToArray();
                    }
                }

                if (presenceDataFields.ContainsKey("leave"))
                {
                    List<object> leaveDeltaList = presenceDataFields["leave"] as List<object>;
                    if (leaveDeltaList != null && leaveDeltaList.Count > 0)
                    {
                        presenceEvent.Leave = leaveDeltaList.Select(x => x.ToString()).ToArray();
                    }
                }

                if (presenceDataFields.ContainsKey("here_now_refresh"))
                {
                    string hereNowRefreshStr = presenceDataFields["here_now_refresh"].ToString();
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