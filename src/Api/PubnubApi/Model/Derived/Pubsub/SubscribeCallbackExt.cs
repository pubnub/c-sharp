using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi
{
    public class SubscribeCallbackExt : SubscribeCallback
    {
        Action<Pubnub, PNMessageResult<object>> subscribeAction = null;
        Action<Pubnub, PNPresenceEventResult> presenceAction = null;
        Action<Pubnub, PNStatus> statusAction = null;

        public SubscribeCallbackExt(Action<Pubnub, PNMessageResult<object>> messageCallback, Action<Pubnub, PNPresenceEventResult> presenceCallback, Action<Pubnub, PNStatus> statusCallback)
        {
            this.subscribeAction = messageCallback;
            this.presenceAction = presenceCallback;
            this.statusAction = statusCallback;
        }

        public override void Message<T>(Pubnub pubnub, PNMessageResult<T> message)
        {
            PNMessageResult<object> message1 = new PNMessageResult<object>();
            message1.Channel = message.Channel;
            message1.Message = (T)(object)message.Message;
            //if (typeof(T) == typeof(string))
            //{
            //    message1.Message = message.Message.ToString();
            //}
            //else if (typeof(T) == typeof(object))
            //{
            //    try
            //    {
            //        message1.Message = Convert.ChangeType(message.Message, typeof(object), CultureInfo.InvariantCulture);
            //    }
            //    catch (Exception ex)
            //    {
            //        message1.Message = (T)(object)message.Message;
            //    }
            //}
            //else
            //{
            //    message1.Message = (T)(object)message.Message;
            //}
            message1.Subscription = message.Subscription;
            message1.Timetoken = message.Timetoken;
            message1.UserMetadata = message.UserMetadata;

            subscribeAction?.Invoke(pubnub, message1);
        }

        public override void Presence(Pubnub pubnub, PNPresenceEventResult presence)
        {
            presenceAction?.Invoke(pubnub, presence);
        }

        public override void Status(Pubnub pubnub, PNStatus status)
        {
            statusAction?.Invoke(pubnub, status);
        }
    }
}
