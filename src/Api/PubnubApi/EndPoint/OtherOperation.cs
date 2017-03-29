using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    internal class OtherOperation : PubnubCoreBase
    {
        private static PNConfiguration config = null;
        private static IJsonPluggableLibrary jsonLibrary = null;
        private IPubnubUnitTest unit = null;

        public OtherOperation(PNConfiguration pubnubConfig) :base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public OtherOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pubnubConfig, jsonPluggableLibrary, null)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public OtherOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
        }


        public void ChangeUUID(string newUUID)
        {
            if (string.IsNullOrEmpty(newUUID) || config.Uuid == newUUID)
            {
                return;
            }

            UuidChanged = true;

            string oldUUID = config.Uuid;

            config.Uuid = newUUID;
            CurrentUuid = newUUID;

            string[] channels = GetCurrentSubscriberChannels();
            string[] channelGroups = GetCurrentSubscriberChannelGroups();

            channels = (channels != null) ? channels : new string[] { };
            channelGroups = (channelGroups != null) ? channelGroups : new string[] { };

            if (channels.Length > 0 || channelGroups.Length > 0)
            {
                string channelsJsonState = BuildJsonUserState(channels.ToArray(), channelGroups.ToArray(), false);
                IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit);
                Uri request = urlBuilder.BuildMultiChannelLeaveRequest(channels, channelGroups, oldUUID, channelsJsonState);

                RequestState<string> requestState = new RequestState<string>();
                requestState.Channels = channels;
                requestState.ChannelGroups = channelGroups;
                requestState.ResponseType = PNOperationType.Leave;
                requestState.Reconnect = false;

                string json = UrlProcessRequest(request, requestState, false); // connectCallback = null
            }

            TerminateCurrentSubscriberRequest();

        }

        public static long TranslateDateTimeToSeconds(DateTime dotNetUTCDateTime)
        {
            TimeSpan timeSpan = dotNetUTCDateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long timeStamp = Convert.ToInt64(timeSpan.TotalSeconds);
            return timeStamp;
        }

        /// <summary>
        /// Convert the UTC/GMT DateTime to Unix Nano Seconds format
        /// </summary>
        /// <param name="dotNetUTCDateTime"></param>
        /// <returns></returns>
        public static long TranslateDateTimeToPubnubUnixNanoSeconds(DateTime dotNetUTCDateTime)
        {
            TimeSpan timeSpan = dotNetUTCDateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long timeStamp = Convert.ToInt64(timeSpan.TotalSeconds) * 10000000;
            return timeStamp;
        }

        /// <summary>
        /// Convert the Unix Nano Seconds format time to UTC/GMT DateTime
        /// </summary>
        /// <param name="unixNanoSecondTime"></param>
        /// <returns></returns>
        public static DateTime TranslatePubnubUnixNanoSecondsToDateTime(long unixNanoSecondTime)
        {
            try
            {
                double timeStamp = unixNanoSecondTime / 10000000;
                DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timeStamp);
                return dateTime;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        public static DateTime TranslatePubnubUnixNanoSecondsToDateTime(string unixNanoSecondTime)
        {
            long numericTime;
            bool tried = Int64.TryParse(unixNanoSecondTime, out numericTime);
            if (tried)
            {
                try
                {
                    double timeStamp = numericTime / 10000000;
                    DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timeStamp);
                    return dateTime;
                }
                catch
                {
                    return DateTime.MinValue;
                }
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        public List<string> GetSubscribedChannels()
        {
            List<string> ret = null;
            string[] currentSubscribedChannels = GetCurrentSubscriberChannels();
            if (currentSubscribedChannels != null)
            {
                ret = currentSubscribedChannels.ToList();
            }

            return ret;
        }

        public List<string> GetSubscribedChannelGroups()
        {
            List<string> ret = null;
            string[] currentSubscribedChannelGroups = GetCurrentSubscriberChannelGroups();
            if (currentSubscribedChannelGroups != null)
            {
                ret = currentSubscribedChannelGroups.ToList();
            }

            return ret;
        }

        public void Destory()
        {
            EndPendingRequests();
        }
    }
}
