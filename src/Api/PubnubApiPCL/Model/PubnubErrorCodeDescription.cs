
using System.Collections.Generic;

namespace PubnubApi
{
    internal static class PubnubErrorCodeDescription
    {
        private static Dictionary<int, string> dictionaryCodes = new Dictionary<int, string>();

        static PubnubErrorCodeDescription()
        {
            //HTTP ERROR CODES and PubNub Context description
            dictionaryCodes.Add(4000, "If you must publish a message greater than the default of max message size of 1.8K (post-URLEncoded) please enable the elastic message size feature from your admin portal at admin.pubnub.com.");
            dictionaryCodes.Add(4001, "Bad Request. Please check the entered inputs or web request URL");
            dictionaryCodes.Add(4002, "Invalid Key. Please verify your pub and sub keys");
            dictionaryCodes.Add(4003, "No UUID specified. Please ensure that UUID is being passed to server for heartbeat");
            dictionaryCodes.Add(4004, "Invalid Timestamp. Please try again. If the issue continues, please contact PubNub support");
            dictionaryCodes.Add(4005, "Invalid Key. Please verify your pub and sub keys");
            dictionaryCodes.Add(4006, "Channel group or groups result in empty subscription set. Please ensure that channels are added to the channel group before subscribe.");
            dictionaryCodes.Add(4007, "Invalid Key. Please verify your pub/sub/secret/cipher keys");
            dictionaryCodes.Add(4010, "Please provide a valid subscribe key");
            dictionaryCodes.Add(4020, "PAM is not enabled for this keyset. Please contact PubNub support for instructions on enabling PAM.");
            dictionaryCodes.Add(4030, "Not authorized. Please ensure that the channel has the correct PAM permission, your authentication key is set correctly, then try again via unsub and re-sub. For further assistance, contact PubNub support.");
            dictionaryCodes.Add(4031, "Please verify pub, sub, and secret keys. For assistance, contact PubNub support");
            dictionaryCodes.Add(4040, "HTTP 404 - Not Found Occured. Please try again. If the issue continues, please contact PubNub support");
            dictionaryCodes.Add(4140, "The URL request too long. Reduce the length by reducing subscription/presence channels or grant/revoke/audit channels/auth key list. Hint: You may spread the load across multiple PubNub instances to prevent this message.");
            dictionaryCodes.Add(5000, "Internal Server Error. Please try again. If the issue continues, please contact PubNub support");
            dictionaryCodes.Add(5020, "Bad Gateway. Please try again. If the issue continues, please contact PubNub support");
            dictionaryCodes.Add(5030, "Service Unavailable. Please try again. If the issue continues, please contact PubNub support");
            dictionaryCodes.Add(5040, "Gateway Timeout. Please try again. If the issue continues, please contact PubNub support");

            //PubNub API ERROR CODES and PubNub Context description
            dictionaryCodes.Add(103, "Please verify origin, host name, and internet connectivity");
            dictionaryCodes.Add(104, "Please verify your cipher key");
            dictionaryCodes.Add(105, "Web Request was cancelled due to change in subsciber/presence channel list or cancelled for object cleaning at the end of Pubnub object session");
            dictionaryCodes.Add(106, "Please check network/internet connection");
            dictionaryCodes.Add(107, "Internal exception. Please ignore"); //This won't go to callback. It will be suppressed.
            dictionaryCodes.Add(108, "Please check network/internet connection");
            dictionaryCodes.Add(109, "No network/internet connection. Please check network/internet connection");
            dictionaryCodes.Add(110, "Network/internet connection is back. Active subscriber/presence channels will be restored.");
            dictionaryCodes.Add(111, "Duplicate channel subscription is not allowed. Internally Pubnub API removes the duplicates before processing");
            dictionaryCodes.Add(112, "Channel Already Subscribed. Duplicate channel subscription not allowed");
            dictionaryCodes.Add(113, "Channel Already Presence-Subscribed. Duplicate channel presence-subscription not allowed");
            dictionaryCodes.Add(114, "Please verify your cipher key");
            dictionaryCodes.Add(115, "Protocol Error. Please contact PubNub with log, use-case, and error details.");
            dictionaryCodes.Add(116, "ServerProtocolViolation. Please contact PubNub with error details.");
            dictionaryCodes.Add(117, "Input contains invalid channel name");
            dictionaryCodes.Add(118, "Channel or ChannelGroup not subscribed yet");
            dictionaryCodes.Add(119, "Channel or ChannelGroup not subscribed for presence yet");
            dictionaryCodes.Add(120, "Incomplete unsubscribe. Try again for unsubscribe.");
            dictionaryCodes.Add(121, "Incomplete presence-unsubscribe. Try again for presence-unsubscribe.");
            dictionaryCodes.Add(122, "Network/Internet connection not available. C# client retrying again to verify connection. No action is needed from your side.");
            dictionaryCodes.Add(123, "During non-availability of network/internet, max retries for connection were attempted. So unsubscribed the channel.");
            dictionaryCodes.Add(124, "During non-availability of network/internet, max retries for connection were attempted. So presence-unsubscribed the channel.");
            dictionaryCodes.Add(125, "Publish operation timeout occured.");
            dictionaryCodes.Add(126, "HereNow operation timeout occured.");
            dictionaryCodes.Add(127, "Detailed History operation timeout occured.");
            dictionaryCodes.Add(128, "Time operation timeout occured.");
            dictionaryCodes.Add(129, "Error occured in external component. Please contact PubNub support with full error object details for further investigation");
            dictionaryCodes.Add(130, "Client machine is sleeping. Please check your machine.");
            dictionaryCodes.Add(131, "Timeout occured while setting user state. Please try again. If it continues, please contact PubNub support");
            dictionaryCodes.Add(132, "Timeout occured while getting user state. Please try again. If it continues, please contact PubNub support");
            dictionaryCodes.Add(133, "Timeout occured while running WhereNow. Please try again. If it continues, please contact PubNub support");
            dictionaryCodes.Add(134, "Timeout occured while running GlobalHereNow. Please try again. If it continues, please contact PubNub support");
            dictionaryCodes.Add(135, "Timeout occured while running PAM operations. Please try again. If it continues, please contact PubNub support");
            dictionaryCodes.Add(136, "User State Unchanged");
            dictionaryCodes.Add(137, "Timeout occured while registering device for push notifications. Please try again. If it continues, please contact PubNub support");
            dictionaryCodes.Add(138, "Timeout occured while performing operation related to channel group. Please try again. If it continues, please contact PubNub support");
            dictionaryCodes.Add(139, "Duplicate channel group subscription is not allowed. Internally Pubnub API removes the duplicates before processing");
            dictionaryCodes.Add(140, "WebExcepton. The underlying connection was closed: An unexpected error occurred on a receive. If it continues, please contact PubNub support");
            dictionaryCodes.Add(0, "Undocumented error. Please contact PubNub support with full error object details for further investigation");
        }

        public static string GetStatusCodeDescription(PubnubErrorCode pubnubErrorCode)
        {
            string defaultDescription = "Please contact PubNub support with your error object details";
            int key = (int)pubnubErrorCode;
            string description = dictionaryCodes.ContainsKey(key) ? dictionaryCodes[key] : defaultDescription;
            return description;
        }
    }
}
