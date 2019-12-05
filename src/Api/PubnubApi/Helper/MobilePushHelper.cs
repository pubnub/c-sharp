using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace PubnubApi
{
    public class MobilePushHelper
    {
        private PNPushType[] pushNotificationTypes;
        private string pushTitle;
        private string pushBody;
        private int pushBadge;
        private string pushSound;
        private List<Apns2Data> pushApns2Data;
        private Dictionary<PNPushType, Dictionary<string, object>> pushCustomData { get; set; } = new Dictionary<PNPushType, Dictionary<string, object>>();
        public MobilePushHelper PushTypeSupport(PNPushType[] pushTypeSupport)
        {
            pushNotificationTypes = pushTypeSupport;
            return this;
        }

        public MobilePushHelper Title(string notificationTitle)
        {
            pushTitle = notificationTitle;
            return this;
        }

        public MobilePushHelper Body(string notificationBody)
        {
            pushBody = notificationBody;
            return this;
        }

        public MobilePushHelper Badge(int notificationBadge)
        {
            pushBadge = notificationBadge;
            return this;
        }

        public MobilePushHelper Sound(string notificaitonSound)
        {
            pushSound = notificaitonSound;
            return this;
        }

        /// <summary>
        /// Supports Only APNS2
        /// </summary>
        /// <param name="apns2SupportData"></param>
        /// <returns></returns>
        public MobilePushHelper Apns2Data(List<Apns2Data> apns2SupportData)
        {
            pushApns2Data = apns2SupportData;
            return this;
        }

        public MobilePushHelper Custom(Dictionary<PNPushType, Dictionary<string, object>> customData)
        {
            pushCustomData.Clear();
            foreach (PNPushType key in customData.Keys){
                pushCustomData.Add(key, customData[key]);
            }
            return this;
        }

        public Dictionary<string, object> GetPayload()
        {
            if (pushNotificationTypes == null)
            {
                throw new MissingMemberException("PNPushType is missing");
            }
            if (pushTitle == null)
            {
                throw new MissingMemberException("Title is missing");
            }


            Dictionary<string, object> ret = new Dictionary<string, object>();

            foreach(PNPushType pushType in pushNotificationTypes)
            {
                if (pushType == PNPushType.APNS)
                {
                    Dictionary<string, object> pnApns = BuildApnsPayload(pushType);
                    
                    if (pnApns != null)
                    {
                        ret.Add("pn_apns", pnApns);
                    }
                }
                else if (pushType == PNPushType.APNS2)
                {
                    Dictionary<string, object> pnApns = BuildApnsPayload(pushType);

                    if (pnApns != null && pushApns2Data != null && pushApns2Data.Count > 0)
                    {
                        List<Apns2Data> apns2DataList = BuildApns2Data();
                        pnApns.Add("pn_push", apns2DataList);
                    }
                    
                    ret.Add("pn_apns", pnApns);
                }
                else if (pushType == PNPushType.FCM)
                {
                    Dictionary<string, object> pnFcm = BuildFcmPayload(pushType);
                    if (pnFcm != null)
                    {
                        ret.Add("pn_gcm", pnFcm);
                    }
                }
                else if (pushType == PNPushType.MPNS)
                {
                    Dictionary<string, object> pnMpns = BuildMpnsPayload(pushType);
                    if (pnMpns != null)
                    {
                        ret.Add("pn_mpns", pnMpns);
                    }
                }
            }

            return ret;
        }

        private Dictionary<string, object> BuildApnsPayload(PNPushType pushType)
        {
            Dictionary<string, object> retApsPayload;
            Dictionary<string, object> apnsPayload = new Dictionary<string, object>();

            Dictionary<string, object> apsData = new Dictionary<string, object>();
            if (pushTitle != null && pushBody != null)
            {
                Dictionary<string, object> alertDic = new Dictionary<string, object>();
                alertDic.Add("title", pushTitle);
                alertDic.Add("body", pushBody);

                apsData.Add("alert", alertDic);
            }
            else if (pushTitle != null)
            {
                apsData.Add("alert", pushTitle);
            }
        
            if (pushBadge > 0)
            {
                apsData.Add("badge", pushBadge);
            }
           
            if (pushSound != null)
            {
                apsData.Add("sound", pushSound);
            }


            apnsPayload.Add("aps", apsData);

            Dictionary<string, object> customData = BuildCustomData(pushType);
            if (customData != null)
            {
                retApsPayload = new Dictionary<string, object>(apnsPayload.Concat(customData).GroupBy(item => item.Key).ToDictionary(item => item.Key, item => item.First().Value));
            }
            else
            {
                retApsPayload = apnsPayload;
            }

            return retApsPayload;
        }

        private List<Apns2Data> BuildApns2Data()
        {
            //Placeholder to add any APNS2 specific
            return pushApns2Data;
        }

        private Dictionary<string, object> BuildCustomData(PNPushType pushType)
        {
            Dictionary<string, object> ret = null;

            if (pushCustomData != null && pushCustomData.Count > 0 && pushCustomData.ContainsKey(pushType))
            {
                ret = new Dictionary<string, object>();

                Dictionary<string, object> pushSpecificCustomData = pushCustomData[pushType];
                foreach (KeyValuePair<string, object> kvp in pushSpecificCustomData)
                {
                    ret.Add(kvp.Key, kvp.Value);
                }
            }

            return ret;
        }

        private Dictionary<string, object> BuildFcmPayload(PNPushType pushType)
        {
            Dictionary<string, object> retFcmPayload = new Dictionary<string, object>();

            Dictionary<string, object> fcmData = new Dictionary<string, object>();
            
            if (pushTitle != null)
            {
                fcmData.Add("summary", pushTitle);
            }


            Dictionary<string, object> customData = BuildCustomData(pushType);
            Dictionary<string, object> fcmPayload;
            if (customData != null)
            {
                fcmPayload = new Dictionary<string, object>(fcmData.Concat(customData).GroupBy(item => item.Key).ToDictionary(item => item.Key, item => item.First().Value));
            }
            else
            {
                fcmPayload = fcmData;
            }

            retFcmPayload.Add("data", fcmPayload);

            return retFcmPayload;
        }

        private Dictionary<string, object> BuildMpnsPayload(PNPushType pushType)
        {
            Dictionary<string, object> retMpnsPayload;

            Dictionary<string, object> mpnsData = new Dictionary<string, object>();

            if (pushTitle != null)
            {
                mpnsData.Add("title", pushTitle);
            }

            if (pushBadge > 0)
            {
                mpnsData.Add("count", pushBadge);
            }

            Dictionary<string, object> customData = BuildCustomData(pushType);
            if (customData != null)
            {
                retMpnsPayload = new Dictionary<string, object>(mpnsData.Concat(customData).GroupBy(item => item.Key).ToDictionary(item => item.Key, item => item.First().Value));
            }
            else
            {
                retMpnsPayload = mpnsData;
            }

            return retMpnsPayload;
        }
    }

    public class Apns2Data
    {
        public string collapseId { get; set; }
        public string expiration { get; set; }
        public List<PushTarget> targets { get; set; }

        public string version { get; } = "v2";
    }

    public class PushTarget
    {
        public string topic { get; set; }
        public List<string> exclude_devices { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public Environment environment { get; set; }
    }

    public enum Environment
    {
        [EnumMember(Value = "development")]
        Development,
        [EnumMember(Value = "production")]
        Production
    }
}
