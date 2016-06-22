using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PubNubMessaging.Core;
using System.IO;
using System.Net;


namespace PubNubMessaging.Tests
{
    public class PubnubUnitTest : IPubnubUnitTest
    {
        private bool enableStubTest = PubnubCommon.EnableStubTest;
        private string _testClassName = "";
        private string _testCaseName = "";

        public bool EnableStubTest
        {
            get
            {
                return enableStubTest;
            }
            set
            {
                enableStubTest = value;
            }
        }

        private Dictionary<string, string> LoadWhenAClientIsPresentedThenPresenceShouldReturnReceivedMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/subscribe/demo-36/hello_my_channel-pnpres/0/0", "[[],\"13596603179264912\"]");
            data.Add("/subscribe/demo-36/hello_my_channel-pnpres/0/13596603179264912", "[[],\"13596603179264912\"]");
            data.Add("/subscribe/demo-36/hello_my_channel,hello_my_channel-pnpres/0/0", "[[],\"13596603179264912\"]");
            data.Add("/subscribe/demo-36/hello_my_channel,hello_my_channel-pnpres/0/13596603179264912", "[[{\"action\": \"join\", \"timestamp\": 1355929955, \"uuid\": \"eb4c1645-1319-4425-865f-008563009d67\", \"occupancy\": 1}],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel,hello_my_channel-pnpres/0/13559006802662768", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel-pnpres/0/13559006802662768", "[[{\"action\": \"join\", \"timestamp\": 1355929955, \"uuid\": \"eb4c1645-1319-4425-865f-008563009d67\", \"occupancy\": 1}],\"13559006802662768\"]");


            data.Add("/subscribe/demo-36/hello_my_channel-pnpres,hello_my_channel/0/0", "[[],\"13596603179264912\"]");
            data.Add("/subscribe/demo-36/hello_my_channel-pnpres,hello_my_channel/0/13596603179264912", "[[{\"action\": \"join\", \"timestamp\": 1355929955, \"uuid\": \"eb4c1645-1319-4425-865f-008563009d67\", \"occupancy\": 1}],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel-pnpres,hello_my_channel/0/13559006802662768", "[[],\"13559006802662768\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel/heartbeat", "{\"status\": 200, \"message\": \"OK\", \"service\": \"Presence\"}");
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel/leave", "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}");
            
            return data;
        }

        private Dictionary<string, string> LoadWhenAClientIsPresentedThenPresenceShouldReturnCustomUUID()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/subscribe/demo-36/hello_my_channel-pnpres/0/0", "[[],\"13596603179264912\"]");
            data.Add("/subscribe/demo-36/hello_my_channel-pnpres/0/13596603179264912", "[[],\"13596603179264912\"]");
            data.Add("/subscribe/demo-36/hello_my_channel,hello_my_channel-pnpres/0/0", "[[],\"13596603179264912\"]");
            data.Add("/subscribe/demo-36/hello_my_channel,hello_my_channel-pnpres/0/13596603179264912", "[[{\"action\": \"join\", \"timestamp\": 1355929955, \"uuid\": \"mylocalmachine.mydomain.com\", \"occupancy\": 1}],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel,hello_my_channel-pnpres/0/13559006802662768", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel-pnpres/0/13559006802662768", "[[],\"13559006802662768\"]");

            data.Add("/subscribe/demo-36/hello_my_channel-pnpres,hello_my_channel/0/0", "[[],\"13596603179264912\"]");
            data.Add("/subscribe/demo-36/hello_my_channel-pnpres,hello_my_channel/0/13596603179264912", "[[{\"action\": \"join\", \"timestamp\": 1355929955, \"uuid\": \"mylocalmachine.mydomain.com\", \"occupancy\": 1}],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel-pnpres,hello_my_channel/0/13559006802662768", "[[],\"13559006802662768\"]");

            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel/leave", "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}");

            return data;
        }

        private Dictionary<string, string> LoadWhenAClientIsPresentedIfHereNowIsCalledThenItShouldReturnInfo()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/subscribe/demo-36/hello_my_channel/0/0", "[[],\"13596603179264912\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13596603179264912", "[[{\"action\": \"join\", \"timestamp\": 1355929955, \"uuid\": \"eb4c1645-1319-4425-865f-008563009d67\", \"occupancy\": 1}],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559006802662768", "[[],\"13559006802662768\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel", "{\"status\": 200, \"message\": \"OK\", \"service\": \"Presence\", \"uuids\": [\"eb4c1645-1319-4425-865f-008563009d67\"], \"occupancy\": 1}");
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel/leave", "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenAClientIsPresentedIfHereNowIsCalledThenItShouldReturnInfoWithUserState()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/subscribe/demo-36/hello_my_channel/0/0", "[[],\"13596603179264912\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13596603179264912", "[[{\"action\": \"join\", \"timestamp\": 1355929955, \"uuid\": \"mylocalmachine.mydomain.com\", \"occupancy\": 1}],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559006802662768", "[[],\"13559006802662768\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel/uuid/mylocalmachine.mydomain.com/data", "{\"status\": 200, \"message\": \"OK\", \"payload\": {\"testkey\": \"testval\"}, \"service\": \"Presence\"}");
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel", "{\"status\": 200, \"message\": \"OK\", \"service\": \"Presence\", \"uuids\": [{\"state\": {\"testkey\": \"testval\"}, \"uuid\": \"mylocalmachine.mydomain.com\"}], \"occupancy\": 1}");
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel/leave", "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenAClientIsPresentedIfGlobalHereNowIsCalledThenItShouldReturnInfo()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v2/presence/sub_key/demo-36", "{\"status\":200,\"message\":\"OK\",\"payload\":{\"channels\":{\"hello_my_channel\":{\"uuids\":[{\"uuid\":\"2417aac2-813f-4f2c-899e-f388033d77fd\"}],\"occupancy\":1}},\"total_channels\":1,\"total_occupancy\":1},\"service\":\"Presence\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenAClientIsPresentedIfGlobalHereNowIsCalledThenItShouldReturnInfoWithUserState()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/subscribe/demo-36/hello_my_channel/0/0", "[[],\"13596603179264912\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13596603179264912", "[[{\"action\": \"join\", \"timestamp\": 1355929955, \"uuid\": \"mylocalmachine.mydomain.com\", \"occupancy\": 1}],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559006802662768", "[[],\"13559006802662768\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel/uuid/mylocalmachine.mydomain.com/data", "{\"status\": 200, \"message\": \"OK\", \"payload\": {\"testkey\": \"testval\"}, \"service\": \"Presence\"}");
            data.Add("/v2/presence/sub_key/demo-36", "{\"status\":200,\"message\":\"OK\",\"payload\":{\"channels\":{\"hello_my_channel\":{\"uuids\":[{\"uuid\":\"mylocalmachine.mydomain.com\"}],\"occupancy\":1}},\"total_channels\":1,\"total_occupancy\":1},\"service\":\"Presence\"}");
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel/leave", "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenAClientIsPresentedIfWhereNowIsCalledThenItShouldReturnInfo()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/subscribe/demo-36/hello_my_channel/0/0", "[[],\"13596603179264912\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13596603179264912", "[[{\"action\": \"join\", \"timestamp\": 1355929955, \"uuid\": \"mylocalmachine.mydomain.com\", \"occupancy\": 1}],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559006802662768", "[[],\"13559006802662768\"]");
            data.Add("/v2/presence/sub_key/demo-36/uuid/mylocalmachine.mydomain.com", "{\"status\":200,\"message\":\"OK\",\"payload\":{\"channels\":[\"hello_my_channel\"]},\"service\":\"Presence\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenAClientIsPresentedIfSetAndGetUserStateThenItShouldReturnInfo()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel/uuid/mylocalmachine.mydomain.com/data", "{\"status\": 200, \"message\": \"OK\", \"payload\": {\"testkey\": \"testval\"}, \"service\": \"Presence\"}");
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel/uuid/mylocalmachine.mydomain.com", "{\"status\": 200, \"uuid\": \"mylocalmachine.mydomain.com\", \"service\": \"Presence\", \"message\": \"OK\", \"payload\": {\"testkey\": \"testval\"}, \"channel\": \"hello_my_channel\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenAClientIsPresentedIfSetAndDeleteUserStateThenItShouldReturnInfo()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel/uuid/mylocalmachine.mydomain.com/data", "{\"status\": 200, \"message\": \"OK\", \"payload\": {\"k2\":\"v2\",\"k\":\"v\"}, \"service\": \"Presence\"}");
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel/uuid/mylocalmachine.mydomain.com", "{\"status\": 200, \"uuid\": \"mylocalmachine.mydomain.com\", \"service\": \"Presence\", \"message\": \"OK\", \"payload\": {\"k\": \"v\"}, \"channel\": \"hello_my_channel\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenAMessageIsPublishedThenUnencryptPublishShouldReturnSuccessCodeAndInfo()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v2/history/sub-key/demo-36/channel/hello_my_channel", "[[\"Pubnub Messaging API 1\"],13557486057035336,13559006802662769]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22Pubnub%20Messaging%20API%201%22", "[1,\"Sent\",\"13559014566792817\"]");
            return data;
        }

        private Dictionary<string, string> LoadWhenAMessageIsPublishedThenUnencryptObjectPublishShouldReturnSuccessCodeAndInfo()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%7B%22foo%22%3A%22hi%21%22%2C%22bar%22%3A%5B1%2C2%2C3%2C4%2C5%5D%7D", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/v2/history/sub-key/demo-36/channel/hello_my_channel", "[[{\"foo\":\"hi!\",\"bar\":[1,2,3,4,5]}],13557486057035336,13559006802662769]");
            return data;
        }

        private Dictionary<string, string> LoadWhenAMessageIsPublishedThenEncryptObjectPublishShouldReturnSuccessCodeAndInfo()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22nQTUCOeyWWgWh5NRLhSlhIingu92WIQ6RFloD9rOZsTUjAhD7AkMaZJVgU7l28e2%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/v2/history/sub-key/demo-36/channel/hello_my_channel", "[[\"nQTUCOeyWWgWh5NRLhSlhIingu92WIQ6RFloD9rOZsTUjAhD7AkMaZJVgU7l28e2\"],13559215464464812,13559215464464812]");
            return data;
        }

        private Dictionary<string, string> LoadWhenAMessageIsPublishedThenEncryptPublishShouldReturnSuccessCodeAndInfo()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
#if (SILVERLIGHT || WINDOWS_PHONE || NETFX_CORE)
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22%2BBY5/miAA8aeuhVl4d13Kg%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22%2BBY5%2FmiAA8aeuhVl4d13Kg%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
#else
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22%2BBY5%2FmiAA8aeuhVl4d13Kg%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
#endif
            data.Add("/v2/history/sub-key/demo-36/channel/hello_my_channel", "[[\"+BY5/miAA8aeuhVl4d13Kg==\"],13557486057035336,13559006802662769]");
            return data;
        }

        private Dictionary<string, string> LoadWhenAMessageIsPublishedThenSecretKeyWithEncryptPublishShouldReturnSuccessCodeAndInfo()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
#if (SILVERLIGHT || WINDOWS_PHONE || NETFX_CORE)
            data.Add("/publish/demo-36/demo-36/f377f886fada25afdf617739af129c2b/hello_my_channel/0/%22f42pIQcWZ9zbTbH8cyLwB/tdvRxjFLOYcBNMVKeHS54%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/e462eda69685ce9ddfd5be20c7e13cab/hello_my_channel/0/%22f42pIQcWZ9zbTbH8cyLwB/tdvRxjFLOYcBNMVKeHS54%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/91db22a2ce85d4d4b2d6c96be732b411/hello_my_channel/0/%22f42pIQcWZ9zbTbH8cyLwB/tdvRxjFLOYcBNMVKeHS54%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/e462eda69685ce9ddfd5be20c7e13cab/hello_my_channel/0/%22f42pIQcWZ9zbTbH8cyLwB%2FtdvRxjFLOYcBNMVKeHS54%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
#else
            data.Add("/publish/demo-36/demo-36/f2df042fa9883d95d1f0ce5c42b69b27/hello_my_channel/0/%22f42pIQcWZ9zbTbH8cyLwB%2FtdvRxjFLOYcBNMVKeHS54%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/e462eda69685ce9ddfd5be20c7e13cab/hello_my_channel/0/%22f42pIQcWZ9zbTbH8cyLwB%2FtdvRxjFLOYcBNMVKeHS54%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
#endif
            data.Add("/v2/history/sub-key/demo-36/channel/hello_my_channel", "[[\"f42pIQcWZ9zbTbH8cyLwB/tdvRxjFLOYcBNMVKeHS54=\"],13559191494674157,13559191494674157]");
            return data;
        }

        private Dictionary<string, string> LoadWhenAMessageIsPublishedThenComplexMessageObjectShouldReturnSuccessCodeAndInfo()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
#if (SILVERLIGHT || WINDOWS_PHONE || NETFX_CORE)
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%7B%22VersionID%22%3A3.4%2C%22Timetoken%22%3A%2213601488652764619%22%2C%22OperationName%22%3A%22Publish%22%2C%22Channels%22%3A%5B%22ch1%22%5D%2C%22DemoMessage%22%3A%7B%22DefaultMessage%22%3A%22%7E%21%40%23%24%25%5E%26%2A%28%29_%2B%20%601234567890-%3D%20qwertyuiop%5B%5D//%20%7B%7D%7C%20asdfghjkl%3B%27%20%3A/%22%20zxcvbnm%2C./%20%3C%3E%3F%20%22%7D%2C%22CustomMessage%22%3A%7B%22DefaultMessage%22%3A%22Welcome%20to%20the%20world%20of%20Pubnub%20for%20Publish%20and%20Subscribe.%20Hah%21%22%7D%2C%22SampleXml%22%3A%5B%7B%22Name%22%3A%7B%22First%22%3A%22John%22%2C%22Middle%22%3A%22P.%22%2C%22Last%22%3A%22Doe%22%7D%2C%22Address%22%3A%7B%22Street%22%3A%22123%20Duck%20Street%22%2C%22City%22%3A%22New%20City%22%2C%22State%22%3A%22New%20York%22%2C%22Country%22%3A%22United%20States%22%7D%2C%22ID%22%3A%22ABCD123%22%7D%2C%7B%22Name%22%3A%7B%22First%22%3A%22Peter%22%2C%22Middle%22%3A%22Z.%22%2C%22Last%22%3A%22Smith%22%7D%2C%22Address%22%3A%7B%22Street%22%3A%2212%20Hollow%20Street%22%2C%22City%22%3A%22Philadelphia%22%2C%22State%22%3A%22Pennsylvania%22%2C%22Country%22%3A%22United%20States%22%7D%2C%22ID%22%3A%22ABCD456%22%7D%5D%7D", "[1,\"Sent\",\"13602210467298480\"]"); //FOR SL
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%7B%22VersionID%22%3A3.4%2C%22Timetoken%22%3A%2213601488652764619%22%2C%22OperationName%22%3A%22Publish%22%2C%22Channels%22%3A%5B%22ch1%22%5D%2C%22DemoMessage%22%3A%7B%22DefaultMessage%22%3A%22%7E%21%40%23%24%25%5E%26%2A%28%29_%2B%20%601234567890-%3D%20qwertyuiop%5B%5D//%20%7B%7D%7C%20asdfghjkl%3B%27%20%3A/%22%20zxcvbnm%2C/%20%3C%3E%3F%20%22%7D%2C%22CustomMessage%22%3A%7B%22DefaultMessage%22%3A%22Welcome%20to%20the%20world%20of%20Pubnub%20for%20Publish%20and%20Subscribe.%20Hah%21%22%7D%2C%22SampleXml%22%3A%5B%7B%22Name%22%3A%7B%22First%22%3A%22John%22%2C%22Middle%22%3A%22P.%22%2C%22Last%22%3A%22Doe%22%7D%2C%22Address%22%3A%7B%22Street%22%3A%22123%20Duck%20Street%22%2C%22City%22%3A%22New%20City%22%2C%22State%22%3A%22New%20York%22%2C%22Country%22%3A%22United%20States%22%7D%2C%22ID%22%3A%22ABCD123%22%7D%2C%7B%22Name%22%3A%7B%22First%22%3A%22Peter%22%2C%22Middle%22%3A%22Z.%22%2C%22Last%22%3A%22Smith%22%7D%2C%22Address%22%3A%7B%22Street%22%3A%2212%20Hollow%20Street%22%2C%22City%22%3A%22Philadelphia%22%2C%22State%22%3A%22Pennsylvania%22%2C%22Country%22%3A%22United%20States%22%7D%2C%22ID%22%3A%22ABCD456%22%7D%5D%7D", "[1,\"Sent\",\"13602210467298480\"]"); //FOR WP7. Difference where there is ./
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%7B%22VersionID%22%3A3.4%2C%22Timetoken%22%3A%2213601488652764619%22%2C%22OperationName%22%3A%22Publish%22%2C%22Channels%22%3A%5B%22ch1%22%5D%2C%22DemoMessage%22%3A%7B%22DefaultMessage%22%3A%22%7E%21%40%23%24%25%5E%26%2A%28%29_%2B%20%601234567890-%3D%20qwertyuiop%5B%5D%5C%5C%20%7B%7D%7C%20asdfghjkl%3B%27%20%3A%5C%22%20zxcvbnm%2C.%2F%20%3C%3E%3F%20%22%7D%2C%22CustomMessage%22%3A%7B%22DefaultMessage%22%3A%22Welcome%20to%20the%20world%20of%20Pubnub%20for%20Publish%20and%20Subscribe.%20Hah%21%22%7D%2C%22SampleXml%22%3A%5B%7B%22Name%22%3A%7B%22First%22%3A%22John%22%2C%22Middle%22%3A%22P.%22%2C%22Last%22%3A%22Doe%22%7D%2C%22Address%22%3A%7B%22Street%22%3A%22123%20Duck%20Street%22%2C%22City%22%3A%22New%20City%22%2C%22State%22%3A%22New%20York%22%2C%22Country%22%3A%22United%20States%22%7D%2C%22ID%22%3A%22ABCD123%22%7D%2C%7B%22Name%22%3A%7B%22First%22%3A%22Peter%22%2C%22Middle%22%3A%22Z.%22%2C%22Last%22%3A%22Smith%22%7D%2C%22Address%22%3A%7B%22Street%22%3A%2212%20Hollow%20Street%22%2C%22City%22%3A%22Philadelphia%22%2C%22State%22%3A%22Pennsylvania%22%2C%22Country%22%3A%22United%20States%22%7D%2C%22ID%22%3A%22ABCD456%22%7D%5D%7D", "[1,\"Sent\",\"13602210467298480\"]");
            data.Add("/v2/history/sub-key/demo-36/channel/hello_my_channel", "[[{\"VersionID\":3.4,\"Timetoken\":\"13601488652764619\",\"OperationName\":\"Publish\",\"Channels\":[\"ch1\"],\"DemoMessage\":{\"DefaultMessage\":\"~!@#$%^&*()_+ `1234567890-= qwertyuiop[]\\\\ {}| asdfghjkl;' :\\\" zxcvbnm,./ <>? \"},\"CustomMessage\":{\"DefaultMessage\":\"Welcome to the world of Pubnub for Publish and Subscribe. Hah!\"},\"SampleXml\":[{\"Name\":{\"First\":\"John\",\"Middle\":\"P.\",\"Last\":\"Doe\"},\"Address\":{\"Street\":\"123 Duck Street\",\"City\":\"New City\",\"State\":\"New York\",\"Country\":\"United States\"},\"ID\":\"ABCD123\"},{\"Name\":{\"First\":\"Peter\",\"Middle\":\"Z.\",\"Last\":\"Smith\"},\"Address\":{\"Street\":\"12 Hollow Street\",\"City\":\"Philadelphia\",\"State\":\"Pennsylvania\",\"Country\":\"United States\"},\"ID\":\"ABCD456\"}]}],13735400879163600,13735400879163600]");
#elif (__MonoCS__ && !UBUNTU)
            //data.Add("/v2/history/sub-key/demo-36/channel/hello_my_channel", "[[{\"VersionID\":3.4,\"Timetoken\":\"13601488652764619\",\"OperationName\":\"Publish\",\"Channels\":[\"ch1\"],\"DemoMessage\":{\"DefaultMessage\":\"~!@#$%^&*()_+ `1234567890-= qwertyuiop[]\\\\ {}| asdfghjkl;' :\\\" zxcvbnm,./ <>? \"},\"CustomMessage\":{\"DefaultMessage\":\"This is a demo message\"}}],13735400879163600,13735400879163600]"); //For mono
            //data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%7B%22VersionID%22%3A3.4%2C%22Timetoken%22%3A%2213601488652764619%22%2C%22OperationName%22%3A%22Publish%22%2C%22Channels%22%3A%5B%22ch1%22%5D%2C%22DemoMessage%22%3A%7B%22DefaultMessage%22%3A%22%7E%21%40%23%24%25%5E%26%2A%28%29_%2B%20%601234567890-%3D%20qwertyuiop%5B%5D%5C%5C%20%7B%7D%7C%20asdfghjkl%3B%27%20%3A%5C%22%20zxcvbnm%2C.%2F%20%3C%3E%3F%20%22%7D%2C%22CustomMessage%22%3A%7B%22DefaultMessage%22%3A%22This%20is%20a%20demo%20message%22%7D%7D", "[1,\"Sent\",\"13602210467298480\"]"); //Mono short publis

			data.Add("/v2/history/sub-key/demo-36/channel/hello_my_channel", "[[{\"VersionID\":3.4,\"Timetoken\":\"13601488652764619\",\"OperationName\":\"Publish\",\"Channels\":[\"ch1\"],\"DemoMessage\":{\"DefaultMessage\":\"~!@#$%^&*()_+ `1234567890-= qwertyuiop[]\\\\ {}| asdfghjkl;' :\\\" zxcvbnm,./ <>? \"},\"CustomMessage\":{\"DefaultMessage\":\"Welcome to the world of Pubnub for Publish and Subscribe. Hah!\"},\"SampleXml\":[{\"Name\":{\"First\":\"John\",\"Middle\":\"P.\",\"Last\":\"Doe\"},\"Address\":{\"Street\":\"123 Duck Street\",\"City\":\"New City\",\"State\":\"New York\",\"Country\":\"United States\"},\"ID\":\"ABCD123\"},{\"Name\":{\"First\":\"Peter\",\"Middle\":\"Z.\",\"Last\":\"Smith\"},\"Address\":{\"Street\":\"12 Hollow Street\",\"City\":\"Philadelphia\",\"State\":\"Pennsylvania\",\"Country\":\"United States\"},\"ID\":\"ABCD456\"}]}],13735400879163600,13735400879163600]"); //For mono
			data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%7B%22VersionID%22%3A3.4%2C%22Timetoken%22%3A%2213601488652764619%22%2C%22OperationName%22%3A%22Publish%22%2C%22Channels%22%3A%5B%22ch1%22%5D%2C%22DemoMessage%22%3A%7B%22DefaultMessage%22%3A%22~%21%40%23%24%25%5E%26%2A%28%29_%2B%20%601234567890-%3D%20qwertyuiop%5B%5D%5C%5C%20%7B%7D%7C%20asdfghjkl%3B%27%20%3A%5C%22%20zxcvbnm%2C.%2F%20%3C%3E%3F%20%22%7D%2C%22CustomMessage%22%3A%7B%22DefaultMessage%22%3A%22Welcome%20to%20the%20world%20of%20Pubnub%20for%20Publish%20and%20Subscribe.%20Hah%21%22%7D%2C%22SampleXml%22%3A%5B%7B%22Name%22%3A%7B%22First%22%3A%22John%22%2C%22Middle%22%3A%22P.%22%2C%22Last%22%3A%22Doe%22%7D%2C%22Address%22%3A%7B%22Street%22%3A%22123%20Duck%20Street%22%2C%22City%22%3A%22New%20City%22%2C%22State%22%3A%22New%20York%22%2C%22Country%22%3A%22United%20States%22%7D%2C%22ID%22%3A%22ABCD123%22%7D%2C%7B%22Name%22%3A%7B%22First%22%3A%22Peter%22%2C%22Middle%22%3A%22Z.%22%2C%22Last%22%3A%22Smith%22%7D%2C%22Address%22%3A%7B%22Street%22%3A%2212%20Hollow%20Street%22%2C%22City%22%3A%22Philadelphia%22%2C%22State%22%3A%22Pennsylvania%22%2C%22Country%22%3A%22United%20States%22%7D%2C%22ID%22%3A%22ABCD456%22%7D%5D%7D", "[1,\"Sent\",\"13602210467298480\"]");
#else
    #if USE_JSONFX
                data.Add("/v2/history/sub-key/demo-36/channel/hello_my_channel", "[[{\"VersionID\":3.4,\"Timetoken\":\"13601488652764619\",\"OperationName\":\"Publish\",\"Channels\":[\"ch1\"],\"DemoMessage\":{\"DefaultMessage\":\"~!@#$%^&*()_+ `1234567890-= qwertyuiop[]\\\\ {}| asdfghjkl;' :\\\" zxcvbnm,./ <>? \"},\"CustomMessage\":{\"DefaultMessage\":\"Welcome to the world of Pubnub for Publish and Subscribe. Hah!\"},\"SampleXml\":[{\"ID\":\"ABCD123\",\"Name\":{\"First\":\"John\",\"Middle\":\"P.\",\"Last\":\"Doe\"},\"Address\":{\"Street\":\"123 Duck Street\",\"City\":\"New City\",\"State\":\"New York\",\"Country\":\"United States\"}},{\"ID\":\"ABCD456\",\"Name\":{\"First\":\"Peter\",\"Middle\":\"Z.\",\"Last\":\"Smith\"},\"Address\":{\"Street\":\"12 Hollow Street\",\"City\":\"Philadelphia\",\"State\":\"Pennsylvania\",\"Country\":\"United States\"}}]}],13735400879163600,13735400879163600]"); //JsonFX
                data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%7B%22VersionID%22%3A3.4%2C%22Timetoken%22%3A%2213601488652764619%22%2C%22OperationName%22%3A%22Publish%22%2C%22Channels%22%3A%5B%22ch1%22%5D%2C%22DemoMessage%22%3A%7B%22DefaultMessage%22%3A%22%7E%21%40%23%24%25%5E%26%2A%28%29_%2B%20%601234567890-%3D%20qwertyuiop%5B%5D%5C%5C%20%7B%7D%7C%20asdfghjkl%3B%27%20%3A%5C%22%20zxcvbnm%2C.%2F%20%3C%3E%3F%20%22%7D%2C%22CustomMessage%22%3A%7B%22DefaultMessage%22%3A%22Welcome%20to%20the%20world%20of%20Pubnub%20for%20Publish%20and%20Subscribe.%20Hah%21%22%7D%2C%22SampleXml%22%3A%5B%7B%22ID%22%3A%22ABCD123%22%2C%22Name%22%3A%7B%22First%22%3A%22John%22%2C%22Middle%22%3A%22P.%22%2C%22Last%22%3A%22Doe%22%7D%2C%22Address%22%3A%7B%22Street%22%3A%22123%20Duck%20Street%22%2C%22City%22%3A%22New%20City%22%2C%22State%22%3A%22New%20York%22%2C%22Country%22%3A%22United%20States%22%7D%7D%2C%7B%22ID%22%3A%22ABCD456%22%2C%22Name%22%3A%7B%22First%22%3A%22Peter%22%2C%22Middle%22%3A%22Z.%22%2C%22Last%22%3A%22Smith%22%7D%2C%22Address%22%3A%7B%22Street%22%3A%2212%20Hollow%20Street%22%2C%22City%22%3A%22Philadelphia%22%2C%22State%22%3A%22Pennsylvania%22%2C%22Country%22%3A%22United%20States%22%7D%7D%5D%7D", "[1,\"Sent\",\"13602210467298480\"]"); //JsonFx
    #else
                data.Add("/v2/history/sub-key/demo-36/channel/hello_my_channel", "[[{\"VersionID\":3.4,\"Timetoken\":\"13601488652764619\",\"OperationName\":\"Publish\",\"Channels\":[\"ch1\"],\"DemoMessage\":{\"DefaultMessage\":\"~!@#$%^&*()_+ `1234567890-= qwertyuiop[]\\\\ {}| asdfghjkl;' :\\\" zxcvbnm,./ <>? \"},\"CustomMessage\":{\"DefaultMessage\":\"Welcome to the world of Pubnub for Publish and Subscribe. Hah!\"},\"SampleXml\":[{\"Name\":{\"First\":\"John\",\"Middle\":\"P.\",\"Last\":\"Doe\"},\"Address\":{\"Street\":\"123 Duck Street\",\"City\":\"New City\",\"State\":\"New York\",\"Country\":\"United States\"},\"ID\":\"ABCD123\"},{\"Name\":{\"First\":\"Peter\",\"Middle\":\"Z.\",\"Last\":\"Smith\"},\"Address\":{\"Street\":\"12 Hollow Street\",\"City\":\"Philadelphia\",\"State\":\"Pennsylvania\",\"Country\":\"United States\"},\"ID\":\"ABCD456\"}]}],13735400879163600,13735400879163600]"); //Newton Json.NET
                data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%7B%22VersionID%22%3A3.4%2C%22Timetoken%22%3A%2213601488652764619%22%2C%22OperationName%22%3A%22Publish%22%2C%22Channels%22%3A%5B%22ch1%22%5D%2C%22DemoMessage%22%3A%7B%22DefaultMessage%22%3A%22%7E%21%40%23%24%25%5E%26%2A%28%29_%2B%20%601234567890-%3D%20qwertyuiop%5B%5D%5C%5C%20%7B%7D%7C%20asdfghjkl%3B%27%20%3A%5C%22%20zxcvbnm%2C.%2F%20%3C%3E%3F%20%22%7D%2C%22CustomMessage%22%3A%7B%22DefaultMessage%22%3A%22Welcome%20to%20the%20world%20of%20Pubnub%20for%20Publish%20and%20Subscribe.%20Hah%21%22%7D%2C%22SampleXml%22%3A%5B%7B%22Name%22%3A%7B%22First%22%3A%22John%22%2C%22Middle%22%3A%22P.%22%2C%22Last%22%3A%22Doe%22%7D%2C%22Address%22%3A%7B%22Street%22%3A%22123%20Duck%20Street%22%2C%22City%22%3A%22New%20City%22%2C%22State%22%3A%22New%20York%22%2C%22Country%22%3A%22United%20States%22%7D%2C%22ID%22%3A%22ABCD123%22%7D%2C%7B%22Name%22%3A%7B%22First%22%3A%22Peter%22%2C%22Middle%22%3A%22Z.%22%2C%22Last%22%3A%22Smith%22%7D%2C%22Address%22%3A%7B%22Street%22%3A%2212%20Hollow%20Street%22%2C%22City%22%3A%22Philadelphia%22%2C%22State%22%3A%22Pennsylvania%22%2C%22Country%22%3A%22United%20States%22%7D%2C%22ID%22%3A%22ABCD456%22%7D%5D%7D", "[1,\"Sent\",\"13602210467298480\"]"); //Newton Json.NET
    #endif
#endif
            return data;
        }

        private Dictionary<string, string> LoadWhenAMessageIsPublishedThenOptionalSecretKeyShouldBeProvidedInConstructor()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/20881fd95e10c440b24f79baf0e4cdb7/hello_my_channel/0/%22Pubnub%20API%20Usage%20Example%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/154de00ed4a7a76b4dc4a83906d05bab/hello_my_channel/0/%22Pubnub%20API%20Usage%20Example%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/cd8eab92c8b4ec5ef50c30d89687fe72/hello_my_channel/0/%22Pubnub%20API%20Usage%20Example%22", "[1,\"Sent\",\"13559014566792817\"]");
            return data;
        }

        private Dictionary<string, string> LoadWhenAMessageIsPublishedIfSSLNotProvidedThenDefaultShouldBeFalse()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22Pubnub%20API%20Usage%20Example%22", "[1,\"Sent\",\"13559014566792817\"]");
            return data;
        }

        private Dictionary<string, string> LoadWhenAMessageIsPublishedThenDisableJsonEncodeShouldSendSerializedObjectMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
#if (SILVERLIGHT || WINDOWS_PHONE)
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%7B%22operation%22%3A%22ReturnData%22%2C%22channel%22%3A%22Mobile1%22%2C%22sequenceNumber%22%3A0%2C%22data%22%3A%5B%22ping%201.0.0.1%22%5D%7D", "[1,\"Sent\",\"13602210467298480\"]");
            data.Add("/v2/history/sub-key/demo-36/channel/hello_my_channel", "[[{\"operation\":\"ReturnData\",\"channel\":\"Mobile1\",\"sequenceNumber\":0,\"data\":[\"ping 1.0.0.1\"]}],13651583681093356,13651583681093356]");
#else
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%7B%22operation%22%3A%22ReturnData%22%2C%22channel%22%3A%22Mobile1%22%2C%22sequenceNumber%22%3A0%2C%22data%22%3A%5B%22ping%201.0.0.1%22%5D%7D", "[1,\"Sent\",\"13602210467298480\"]");
            data.Add("/v2/history/sub-key/demo-36/channel/hello_my_channel", "[[{\"operation\":\"ReturnData\",\"channel\":\"Mobile1\",\"sequenceNumber\":0,\"data\":[\"ping 1.0.0.1\"]}],13651583681093356,13651583681093356]");
#endif
            return data;
        }

        private Dictionary<string, string> LoadWhenAMessageIsPublishedThenLargeMessageShoudFailWithMessageTooLargeInfo()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
#if (SILVERLIGHT || WINDOWS_PHONE)
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22Numerous%20questions%20remain%20about%20the%20origins%20of%20the%20chemical%20and%20what%20impact%20its%20apparent%20use%20could%20have%20on%20the%20ongoing%20Syrian%20civil%20war%20and%20international%20involvement%20in%20it.When%20asked%20if%20the%20intelligence%20community%27s%20conclusion%20pushed%20the%20situation%20across%20President%20Barack%20Obama%27s%20/%22red%20line/%22%20that%20could%20potentially%20trigger%20more%20U.S.%20involvement%20in%20the%20Syrian%20civil%20war%2C%20Hagel%20said%20it%27s%20too%20soon%20to%20say./%22We%20need%20all%20the%20facts.%20We%20need%20all%20the%20information%2C/%22%20he%20said.%20/%22What%20I%27ve%20just%20given%20you%20is%20what%20our%20intelligence%20community%20has%20said%20they%20know.%20As%20I%20also%20said%2C%20they%20are%20still%20assessing%20and%20they%20are%20still%20looking%20at%20what%20happened%2C%20who%20was%20responsible%20and%20the%20other%20specifics%20that%20we%27ll%20need./%22%20In%20a%20letter%20sent%20to%20lawmakers%20before%20Hagel%27s%20announcement%2C%20the%20White%20House%20said%20that%20intelligence%20analysts%20have%20concluded%20/%22with%20varying%20degrees%20of%20confidence%20that%20the%20Syrian%20regime%20has%20used%20chemical%20weapons%20on%20a%20small%20scale%20in%20Syria%2C%20specifically%20the%20chemical%20agent%20sarin./%22%20In%20the%20letter%2C%20signed%20by%20White%20House%20legislative%20affairs%20office%20Director%20Miguel%20Rodriguez%2C%20the%20White%20House%20said%20the%20/%22chain%20of%20custody/%22%20of%20the%20chemicals%20was%20not%20clear%20and%20that%20intelligence%20analysts%20could%20not%20confirm%20the%20circumstances%20under%20which%20the%20sarin%20was%20used%2C%20including%20the%20role%20of%20Syrian%20President%20Bashar%20al-Assad%27s%20regime.%20Read%20Rodriguez%27s%20letter%20%22", "[0,\"Message Too Large\",\"13559014566792817\"]"); //FOR SL
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22Numerous%20questions%20remain%20about%20the%20origins%20of%20the%20chemical%20and%20what%20impact%20its%20apparent%20use%20could%20have%20on%20the%20ongoing%20Syrian%20civil%20war%20and%20international%20involvement%20in%20it.When%20asked%20if%20the%20intelligence%20community%27s%20conclusion%20pushed%20the%20situation%20across%20President%20Barack%20Obama%27s%20/%22red%20line/%22%20that%20could%20potentially%20trigger%20more%20U.S.%20involvement%20in%20the%20Syrian%20civil%20war%2C%20Hagel%20said%20it%27s%20too%20soon%20to%20say/%22We%20need%20all%20the%20facts.%20We%20need%20all%20the%20information%2C/%22%20he%20said.%20/%22What%20I%27ve%20just%20given%20you%20is%20what%20our%20intelligence%20community%20has%20said%20they%20know.%20As%20I%20also%20said%2C%20they%20are%20still%20assessing%20and%20they%20are%20still%20looking%20at%20what%20happened%2C%20who%20was%20responsible%20and%20the%20other%20specifics%20that%20we%27ll%20need/%22%20In%20a%20letter%20sent%20to%20lawmakers%20before%20Hagel%27s%20announcement%2C%20the%20White%20House%20said%20that%20intelligence%20analysts%20have%20concluded%20/%22with%20varying%20degrees%20of%20confidence%20that%20the%20Syrian%20regime%20has%20used%20chemical%20weapons%20on%20a%20small%20scale%20in%20Syria%2C%20specifically%20the%20chemical%20agent%20sarin/%22%20In%20the%20letter%2C%20signed%20by%20White%20House%20legislative%20affairs%20office%20Director%20Miguel%20Rodriguez%2C%20the%20White%20House%20said%20the%20/%22chain%20of%20custody/%22%20of%20the%20chemicals%20was%20not%20clear%20and%20that%20intelligence%20analysts%20could%20not%20confirm%20the%20circumstances%20under%20which%20the%20sarin%20was%20used%2C%20including%20the%20role%20of%20Syrian%20President%20Bashar%20al-Assad%27s%20regime.%20Read%20Rodriguez%27s%20letter%20%22", "[0,\"Message Too Large\",\"13559014566792817\"]"); // FOR WP7. Difference where there is ./
#elif NETFX_CORE
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22Numerous%20questions%20remain%20about%20the%20origins%20of%20the%20chemical%20and%20what%20impact%20its%20apparent%20use%20could%20have%20on%20the%20ongoing%20Syrian%20civil%20war%20and%20international%20involvement%20in%20it.When%20asked%20if%20the%20intelligence%20community%27s%20conclusion%20pushed%20the%20situation%20across%20President%20Barack%20Obama%27s%20red%20line%20that%20could%20potentially%20trigger%20more%20U.S.%20involvement%20in%20the%20Syrian%20civil%20war%2C%20Hagel%20said%20it%27s%20too%20soon%20to%20say.%20We%20need%20all%20the%20facts.%20We%20need%20all%20the%20information%2C%20he%20said.%20What%20I%27ve%20just%20given%20you%20is%20what%20our%20intelligence%20community%20has%20said%20they%20know.%20As%20I%20also%20said%2C%20they%20are%20still%20assessing%20and%20they%20are%20still%20looking%20at%20what%20happened%2C%20who%20was%20responsible%20and%20the%20other%20specifics%20that%20we%27ll%20need.%20In%20a%20letter%20sent%20to%20lawmakers%20before%20Hagel%27s%20announcement%2C%20the%20White%20House%20said%20that%20intelligence%20analysts%20have%20concluded%20with%20varying%20degrees%20of%20confidence%20that%20the%20Syrian%20regime%20has%20used%20chemical%20weapons%20on%20a%20small%20scale%20in%20Syria%2C%20specifically%20the%20chemical%20agent%20sarin.%20In%20the%20letter%2C%20signed%20by%20White%20House%20legislative%20affairs%20office%20Director%20Miguel%20Rodriguez%2C%20the%20White%20House%20said%20the%20chain%20of%20custody%20of%20the%20chemicals%20was%20not%20clear%20and%20that%20intelligence%20analysts%20could%20not%20confirm%20the%20circumstances%20under%20which%20the%20sarin%20was%20used%2C%20including%20the%20role%20of%20Syrian%20President%20Bashar%20al-Assad%27s%20regime.%20Read%20Rodriguez%27s%20letter%20to%20Levin%20%28PDF%29%20But%2C%20the%20letter%20said%2C%20we%20do%20believe%20that%20any%20use%20of%20chemical%20weapons%20in%20Syria%20would%20very%20likely%20have%20originated%20with%20the%20Assad%20regime.%20The%20Syrian%20government%20has%20been%20battling%20a%20rebellion%20for%20more%20than%20two%20years%2C%20bringing%20international%20condemnation%20of%20the%20regime%20and%20pleas%20for%20greater%20international%20assistance.%20The%20United%20Nations%20estimated%20in%20February%20that%20more%20than%2070%2C000%20people%20had%20died%20since%20the%20conflict%20began.%20The%20administration%20is%20pressing%20for%20a%20comprehensive%20United%20Nations%20investigation%20that%20can%20credibly%20evaluate%20the%20evidence%20and%20establish%20what%20took%20place%2C%20the%20White%20House%20letter%20said.%20Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20Numerous%20questions%20remain%20about%20the%20origins%20of%20the%20chemical%20and%20what%20impact%20its%20apparent%20use%20could%20have%20on%20the%20ongoing%20Syrian%20civil%20war%20and%20international%20involvement%20in%20it.When%20asked%20if%20the%20intelligence%20community%27s%20conclusion%20pushed%20the%20situation%20across%20President%20Barack%20Obama%27s%20red%20line%20that%20could%20potentially%20trigger%20more%20U.S.%20involvement%20in%20the%20Syrian%20civil%20war%2C%20Hagel%20said%20it%27s%20too%20soon%20to%20say.We%20need%20all%20the%20facts.%20We%20need%20all%20the%20information%2C%20he%20said.%20What%20I%27ve%20just%20given%20you%20is%20what%20our%20intelligence%20community%20has%20said%20they%20know.%20As%20I%20also%20said%2C%20they%20are%20still%20assessing%20and%20they%20are%20still%20looking%20at%20what%20happened%2C%20who%20was%20responsible%20and%20the%20other%20specifics%20that%20we%27ll%20need.%20In%20a%20letter%20sent%20to%20lawmakers%20before%20Hagel%27s%20announcement%2C%20the%20White%20House%20said%20that%20intelligence%20analysts%20have%20concluded%20with%20varying%20degrees%20of%20confidence%20that%20the%20Syrian%20regime%20has%20used%20chemical%20weapons%20on%20a%20small%20scale%20in%20Syria%2C%20specifically%20the%20chemical%20agent%20sarin.%20In%20the%20letter%2C%20signed%20by%20White%20House%20legislative%20affairs%20office%20Director%20Miguel%20Rodriguez%2C%20the%20White%20House%20said%20the%20chain%20of%20custody%20of%20the%20chemicals%20was%20not%20clear%20and%20that%20intelligence%20analysts%20could%20not%20confirm%20the%20circumstances%20under%20which%20the%20sarin%20was%20used%2C%20including%20the%20role%20of%20Syrian%20President%20Bashar%20al-Assad%27s%20regime.%20Read%20Rodriguez%27s%20letter%20to%20Levin%20%28PDF%29%20But%2C%20the%20letter%20said%2C%20we%20do%20believe%20that%20any%20use%20of%20chemical%20weapons%20in%20Syria%20would%20very%20likely%20have%20originated%20with%20the%20Assad%20regime.%20The%20Syrian%20government%20has%20been%20battling%20a%20rebellion%20for%20more%20than%20two%20years%2C%20bringing%20international%20condemnation%20of%20the%20regime%20and%20pleas%20for%20greater%20international%20assistance.%20The%20United%20Nations%20estimated%20in%20February%20that%20more%20than%2070%2C000%20people%20had%20died%20since%20the%20conflict%20began.%20The%20administration%20is%20pressing%20for%20a%20comprehensive%20United%20Nations%20investigation%20that%20can%20credibly%20evaluate%20the%20evidence%20and%20establish%20what%20took%20place%2C%20the%20White%20House%20letter%20said.%20Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20Numerous%20questions%20remain%20about%20the%20origins%20of%20the%20chemical%20and%20what%20impact%20its%20apparent%20use%20could%20have%20on%20the%20ongoing%20Syrian%20civil%20war%20and%20international%20involvement%20in%20it.When%20asked%20if%20the%20intelligence%20community%27s%20conclusion%20pushed%20the%20situation%20across%20President%20Barack%20Obama%27s%20red%20line%20that%20could%20potentially%20trigger%20more%20U.S.%20involvement%20in%20the%20Syrian%20civil%20war%2C%20Hagel%20said%20it%27s%20too%20soon%20to%20say.%20We%20need%20all%20the%20facts.%20We%20need%20all%20the%20information%2C%20he%20said.%20What%20I%27ve%20just%20given%20you%20is%20what%20our%20intelligence%20community%20has%20said%20they%20know.%20As%20I%20also%20said%2C%20they%20are%20still%20assessing%20and%20they%20are%20still%20looking%20at%20what%20happened%2C%20who%20was%20responsible%20and%20the%20other%20specifics%20that%20we%27ll%20need.%20In%20a%20letter%20sent%20to%20lawmakers%20before%20Hagel%27s%20announcement%2C%20the%20White%20House%20said%20that%20intelligence%20analysts%20have%20concluded%20with%20varying%20degrees%20of%20confidence%20that%20the%20Syrian%20regime%20has%20used%20chemical%20weapons%20on%20a%20small%20scale%20in%20Syria%2C%20specifically%20the%20chemical%20agent%20sarin.%20In%20the%20letter%2C%20signed%20by%20White%20House%20legislative%20affairs%20office%20Director%20Miguel%20Rodriguez%2C%20the%20White%20House%20said%20the%20chain%20of%20custody%20of%20the%20chemicals%20was%20not%20clear%20and%20that%20intelligence%20analysts%20could%20not%20confirm%20the%20circumstances%20under%20which%20the%20sarin%20was%20used%2C%20including%20the%20role%20of%20Syrian%20President%20Bashar%20al-Assad%27s%20regime.%20Read%20Rodriguez%27s%20letter%20to%20Levin%20%28PDF%29%20But%2C%20the%20letter%20said%2C%20we%20do%20believe%20that%20any%20use%20of%20chemical%20weapons%20in%20Syria%20would%20very%20likely%20have%20originated%20with%20the%20Assad%20regime.%20The%20Syrian%20government%20has%20been%20battling%20a%20rebellion%20for%20more%20than%20two%20years%2C%20bringing%20international%20condemnation%20of%20the%20regime%20and%20pleas%20for%20greater%20international%20assistance.%20The%20United%20Nations%20estimated%20in%20February%20that%20more%20than%2070%2C000%20people%20had%20died%20since%20the%20conflict%20began.%20The%20administration%20is%20pressing%20for%20a%20comprehensive%20United%20Nations%20investigation%20that%20can%20credibly%20evaluate%20the%20evidence%20and%20establish%20what%20took%20place%2C%20the%20White%20House%20letter%20said.%20Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20Numerous%20questions%20remain%20about%20the%20origins%20of%20the%20chemical%20and%20what%20impact%20its%20apparent%20use%20could%20have%20on%20the%20ongoing%20Syrian%20civil%20war%20and%20international%20involvement%20in%20it.When%20asked%20if%20the%20intelligence%20community%27s%20conclusion%20pushed%20the%20situation%20across%20President%20Barack%20Obama%27s%20red%20line%20that%20could%20potentially%20trigger%20more%20U.S.%20involvement%20in%20the%20Syrian%20civil%20war%2C%20Hagel%20said%20it%27s%20too%20soon%20to%20say.We%20need%20all%20the%20facts.%20We%20need%20all%20the%20information%2C%20he%20said.%20What%20I%27ve%20just%20given%20you%20is%20what%20our%20intelligence%20community%20has%20said%20they%20know.%20As%20I%20also%20said%2C%20they%20are%20still%20assessing%20and%20they%20are%20still%20looking%20at%20what%20happened%2C%20who%20was%20responsible%20and%20the%20other%20specifics%20that%20we%27ll%20need.%20In%20a%20letter%20sent%20to%20lawmakers%20before%20Hagel%27s%20announcement%2C%20the%20White%20House%20said%20that%20intelligence%20analysts%20have%20concluded%20with%20varying%20degrees%20of%20confidence%20that%20the%20Syrian%20regime%20has%20used%20chemical%20weapons%20on%20a%20small%20scale%20in%20Syria%2C%20specifically%20the%20chemical%20agent%20sarin.%20In%20the%20letter%2C%20signed%20by%20White%20House%20legislative%20affairs%20office%20Director%20Miguel%20Rodriguez%2C%20the%20White%20House%20said%20the%20chain%20of%20custody%20of%20the%20chemicals%20was%20not%20clear%20and%20that%20intelligence%20analysts%20could%20not%20confirm%20the%20circumstances%20under%20which%20the%20sarin%20was%20used%2C%20including%20the%20role%20of%20Syrian%20President%20Bashar%20al-Assad%27s%20regime.%20Read%20Rodriguez%27s%20letter%20to%20Levin%20%28PDF%29%20But%2C%20the%20letter%20said%2C%20we%20do%20believe%20that%20any%20use%20of%20chemical%20weapons%20in%20Syria%20would%20very%20likely%20have%20originated%20with%20the%20Assad%20regime.%20The%20Syrian%20government%20has%20been%20battling%20a%20rebellion%20for%20more%20than%20two%20years%2C%20bringing%20international%20condemnation%20of%20the%20regime%20and%20pleas%20for%20greater%20international%20assistance.%20The%20United%20Nations%20estimated%20in%20February%20that%20more%20than%2070%2C000%20people%20had%20died%20since%20the%20conflict%20began.%20The%20administration%20is%20pressing%20for%20a%20comprehensive%20United%20Nations%20investigation%20that%20can%20credibly%20evaluate%20the%20evidence%20and%20establish%20what%20took%20place%2C%20the%20White%20House%20letter%20said.%20Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20Numerous%20questions%20remain%20about%20the%20origins%20of%20the%20chemical%20and%20what%20impact%20its%20apparent%20use%20could%20have%20on%20the%20ongoing%20Syrian%20civil%20war%20and%20international%20involvement%20in%20it.When%20asked%20if%20the%20intelligence%20community%27s%20conclusion%20pushed%20the%20situation%20across%20President%20Barack%20Obama%27s%20red%20line%20that%20could%20potentially%20trigger%20more%20U.S.%20involvement%20in%20the%20Syrian%20civil%20war%2C%20Hagel%20said%20it%27s%20too%20soon%20to%20say.We%20need%20all%20the%20facts.%20We%20need%20all%20the%20information%2C%20he%20said.%20What%20I%27ve%20just%20given%20you%20is%20what%20our%20intelligence%20community%20has%20said%20they%20know.%20As%20I%20also%20said%2C%20they%20are%20still%20assessing%20and%20they%20are%20still%20looking%20at%20what%20happened%2C%20who%20was%20responsible%20and%20the%20other%20specifics%20that%20we%27ll%20need.%20In%20a%20letter%20sent%20to%20lawmakers%20before%20Hagel%27s%20announcement%2C%20the%20White%20House%20said%20that%20intelligence%20analysts%20have%20concluded%20with%20varying%20degrees%20of%20confidence%20that%20the%20Syrian%20regime%20has%20used%20chemical%20weapons%20on%20a%20small%20scale%20in%20Syria%2C%20specifically%20the%20chemical%20agent%20sarin.%20In%20the%20letter%2C%20signed%20by%20White%20House%20legislative%20affairs%20office%20Director%20Miguel%20Rodriguez%2C%20the%20White%20House%20said%20the%20chain%20of%20custody%20of%20the%20chemicals%20was%20not%20clear%20and%20that%20intelligence%20analysts%20could%20not%20confirm%20the%20circumstances%20under%20which%20the%20sarin%20was%20used%2C%20including%20the%20role%20of%20Syrian%20President%20Bashar%20al-Assad%27s%20regime.%20Read%20Rodriguez%27s%20letter%20to%20Levin%20%28PDF%29%20But%2C%20the%20letter%20said%2C%20we%20do%20believe%20that%20any%20use%20of%20chemical%20weapons%20in%20Syria%20would%20very%20likely%20have%20originated%20with%20the%20Assad%20regime.%20The%20Syrian%20government%20has%20been%20battling%20a%20rebellion%20for%20more%20than%20two%20years%2C%20bringing%20international%20condemnation%20of%20the%20regime%20and%20pleas%20for%20greater%20international%20assistance.%20The%20United%20Nations%20estimated%20in%20February%20that%20more%20than%2070%2C000%20people%20had%20died%20since%20the%20conflict%20began.%20The%20administration%20is%20pressing%20for%20a%20comprehensive%20United%20Nations%20investigation%20that%20can%20credibly%20evaluate%20the%20evidence%20and%20establish%20what%20took%20place%2C%20the%20White%20House%20letter%20said.%20Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20Numerous%20questions%20remain%20about%20the%20origins%20of%20the%20chemical%20and%20what%20impact%20its%20apparent%20use%20could%20have%20on%20the%20ongoing%20Syrian%20civil%20war%20and%20international%20involvement%20in%20it.When%20asked%20if%20the%20intelligence%20community%27s%20conclusion%20pushed%20the%20situation%20across%20President%20Barack%20Obama%27s%20red%20line%20that%20could%20potentially%20trigger%20more%20U.S.%20involvement%20in%20the%20Syrian%20civil%20war%2C%20Hagel%20said%20it%27s%20too%20soon%20to%20say.We%20need%20all%20the%20facts.%20We%20need%20all%20the%20information%2C%20he%20said.%20What%20I%27ve%20just%20given%20you%20is%20what%20our%20intelligence%20community%20has%20said%20they%20know.%20As%20I%20also%20said%2C%20they%20are%20still%20assessing%20and%20they%20are%20still%20looking%20at%20what%20happened%2C%20who%20was%20responsible%20and%20the%20other%20specifics%20that%20we%27ll%20need.%20In%20a%20letter%20sent%20to%20lawmakers%20before%20Hagel%27s%20announcement%2C%20the%20White%20House%20said%20that%20intelligence%20analysts%20have%20concluded%20with%20varying%20degrees%20of%20confidence%20that%20the%20Syrian%20regime%20has%20used%20chemical%20weapons%20on%20a%20small%20scale%20in%20Syria%2C%20specifically%20the%20chemical%20agent%20sarin.%20In%20the%20letter%2C%20signed%20by%20White%20House%20legislative%20affairs%20office%20Director%20Miguel%20Rodriguez%2C%20the%20White%20House%20said%20the%20chain%20of%20custody%20of%20the%20chemicals%20was%20not%20clear%20and%20that%20intelligence%20analysts%20could%20not%20confirm%20the%20circumstances%20under%20which%20the%20sarin%20was%20used%2C%20including%20the%20role%20of%20Syrian%20President%20Bashar%20al-Assad%27s%20regime.%20Read%20Rodriguez%27s%20letter%20to%20Levin%20%28PDF%29%20But%2C%20the%20letter%20said%2C%20we%20do%20believe%20that%20any%20use%20of%20chemical%20weapons%20in%20Syria%20would%20very%20likely%20have%20originated%20with%20the%20Assad%20regime.%20The%20Syrian%20government%20has%20been%20battling%20a%20rebellion%20for%20more%20than%20two%20years%2C%20bringing%20international%20condemnation%20of%20the%20regime%20and%20pleas%20for%20greater%20international%20assistance.%20The%20United%20Nations%20estimated%20in%20February%20that%20more%20than%2070%2C000%20people%20had%20died%20since%20the%20conflict%20began.%20The%20administration%20is%20pressing%20for%20a%20comprehensive%20United%20Nations%20investigation%20that%20can%20credibly%20evaluate%20the%20evidence%20and%20establish%20what%20took%20place%2C%20the%20White%20House%20letter%20said.%20Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20Numerous%20questions%20remain%20about%20the%20origins%20of%20the%20chemical%20and%20what%20impact%20its%20apparent%20use%20could%20have%20on%20the%20ongoing%20Syrian%20civil%20war%20and%20international%20involvement%20in%20it.When%20asked%20if%20the%20intelligence%20community%27s%20conclusion%20pushed%20the%20situation%20across%20President%20Barack%20Obama%27s%20red%20line%20that%20could%20potentially%20trigger%20more%20U.S.%20involvement%20in%20the%20Syrian%20civil%20war%2C%20Hagel%20said%20it%27s%20too%20soon%20to%20say.We%20need%20all%20the%20facts.%20We%20need%20all%20the%20information%2C%20he%20said.%20What%20I%27ve%20just%20given%20you%20is%20what%20our%20intelligence%20community%20has%20said%20they%20know.%20As%20I%20also%20said%2C%20they%20are%20still%20assessing%20and%20they%20are%20still%20looking%20at%20what%20happened%2C%20who%20was%20responsible%20and%20the%20other%20specifics%20that%20we%27ll%20need.%20In%20a%20letter%20sent%20to%20lawmakers%20before%20Hagel%27s%20announcement%2C%20the%20White%20House%20said%20that%20intelligence%20analysts%20have%20concluded%20with%20varying%20degrees%20of%20confidence%20that%20the%20Syrian%20regime%20has%20used%20chemical%20weapons%20on%20a%20small%20scale%20in%20Syria%2C%20specifically%20the%20chemical%20agent%20sarin.%20In%20the%20letter%2C%20signed%20by%20White%20House%20legislative%20affairs%20office%20Director%20Miguel%20Rodriguez%2C%20the%20White%20House%20said%20the%20chain%20of%20custody%20of%20the%20chemicals%20was%20not%20clear%20and%20that%20intelligence%20analysts%20could%20not%20confirm%20the%20circumstances%20under%20which%20the%20sarin%20was%20used%2C%20including%20the%20role%20of%20Syrian%20President%20Bashar%20al-Assad%27s%20regime.%20Read%20Rodriguez%27s%20letter%20to%20Levin%20%28PDF%29%20But%2C%20the%20letter%20said%2C%20we%20do%20believe%20that%20any%20use%20of%20chemical%20weapons%20in%20Syria%20would%20very%20likely%20have%20originated%20with%20the%20Assad%20regime.%20The%20Syrian%20government%20has%20been%20battling%20a%20rebellion%20for%20more%20than%20two%20years%2C%20bringing%20international%20condemnation%20of%20the%20regime%20and%20pleas%20for%20greater%20international%20assistance.%20The%20United%20Nations%20estimated%20in%20February%20that%20more%20than%2070%2C000%20people%20had%20died%20since%20the%20conflict%20began.%20The%20administration%20is%20pressing%20for%20a%20comprehensive%20United%20Nations%20investigation%20that%20can%20credibly%20evaluate%20the%20evidence%20and%20establish%20what%20took%20place%2C%20the%20White%20House%20letter%20said.%20Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20Numerous%20questions%20remain%20about%20the%20origins%20of%20the%20chemical%20and%20what%20impact%20its%20apparent%20use%20could%20have%20on%20the%20ongoing%20Syrian%20civil%20war%20and%20international%20involvement%20in%20it.When%20asked%20if%20the%20intelligence%20community%27s%20conclusion%20pushed%20the%20situation%20across%20President%20Barack%20Obama%27s%20red%20line%20that%20could%20potentially%20trigger%20more%20U.S.%20involvement%20in%20the%20Syrian%20civil%20war%2C%20Hagel%20said%20it%27s%20too%20soon%20to%20say.We%20need%20all%20the%20facts.%20We%20need%20all%20the%20information%2C%20he%20said.%20What%20I%27ve%20just%20given%20you%20is%20what%20our%20intelligence%20community%20has%20said%20they%20know.%20As%20I%20also%20said%2C%20they%20are%20still%20assessing%20and%20they%20are%20still%20looking%20at%20what%20happened%2C%20who%20was%20responsible%20and%20the%20other%20specifics%20that%20we%27ll%20need.%20In%20a%20letter%20sent%20to%20lawmakers%20before%20Hagel%27s%20announcement%2C%20the%20White%20House%20said%20that%20intelligence%20analysts%20have%20concluded%20with%20varying%20degrees%20of%20confidence%20that%20the%20Syrian%20regime%20has%20used%20chemical%20weapons%20on%20a%20small%20scale%20in%20Syria%2C%20specifically%20the%20chemical%20agent%20sarin.%20In%20the%20letter%2C%20signed%20by%20White%20House%20legislative%20affairs%20office%20Director%20Miguel%20Rodriguez%2C%20the%20White%20House%20said%20the%20chain%20of%20custody%20of%20the%20chemicals%20was%20not%20clear%20and%20that%20intelligence%20analysts%20could%20not%20confirm%20the%20circumstances%20under%20which%20the%20sarin%20was%20used%2C%20including%20the%20role%20of%20Syrian%20President%20Bashar%20al-Assad%27s%20regime.%20Read%20Rodriguez%27s%20letter%20to%20Levin%20%28PDF%29%20But%2C%20the%20letter%20said%2C%20we%20do%20believe%20that%20any%20use%20of%20chemical%20weapons%20in%20Syria%20would%20very%20likely%20have%20originated%20with%20the%20Assad%20regime.%20The%20Syrian%20government%20has%20been%20battling%20a%20rebellion%20for%20more%20than%20two%20years%2C%20bringing%20international%20condemnation%20of%20the%20regime%20and%20pleas%20for%20greater%20international%20assistance.%20The%20United%20Nations%20estimated%20in%20February%20that%20more%20than%2070%2C000%20people%20had%20died%20since%20the%20conflict%20began.%20The%20administration%20is%20pressing%20for%20a%20comprehensive%20United%20Nations%20investigation%20that%20can%20credibly%20evaluate%20the%20evidence%20and%20establish%20what%20took%20place%2C%20the%20White%20House%20letter%20said.%20Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20Numerous%20questions%20remain%20about%20the%20origins%20of%20the%20chemical%20and%20what%20impact%20its%20apparent%20use%20could%20have%20on%20the%20ongoing%20Syrian%20civil%20war%20and%20international%20involvement%20in%20it.When%20asked%20if%20the%20intelligence%20community%27s%20conclusion%20pushed%20the%20situation%20across%20President%20Barack%20Obama%27s%20red%20line%20that%20could%20potentially%20trigger%20more%20U.S.%20involvement%20in%20the%20Syrian%20civil%20war%2C%20Hagel%20said%20it%27s%20too%20soon%20to%20say.We%20need%20all%20the%20facts.%20We%20need%20all%20the%20information%2C%20he%20said.%20What%20I%27ve%20just%20given%20you%20is%20what%20our%20intelligence%20community%20has%20said%20they%20know.%20As%20I%20also%20said%2C%20they%20are%20still%20assessing%20and%20they%20are%20still%20looking%20at%20what%20happened%2C%20who%20was%20responsible%20and%20the%20other%20specifics%20that%20we%27ll%20need.%20In%20a%20letter%20sent%20to%20lawmakers%20before%20Hagel%27s%20announcement%2C%20the%20White%20House%20said%20that%20intelligence%20analysts%20have%20concluded%20with%20varying%20degrees%20of%20confidence%20that%20the%20Syrian%20regime%20has%20used%20chemical%20weapons%20on%20a%20small%20scale%20in%20Syria%2C%20specifically%20the%20chemical%20agent%20sarin.%20In%20the%20letter%2C%20signed%20by%20White%20House%20legislative%20affairs%20office%20Director%20Miguel%20Rodriguez%2C%20the%20White%20House%20said%20the%20chain%20of%20custody%20of%20the%20chemicals%20was%20not%20clear%20and%20that%20intelligence%20analysts%20could%20not%20confirm%20the%20circumstances%20under%20which%20the%20sarin%20was%20used%2C%20including%20the%20role%20of%20Syrian%20President%20Bashar%20al-Assad%27s%20regime.%20Read%20Rodriguez%27s%20letter%20to%20Levin%20%28PDF%29%20But%2C%20the%20letter%20said%2C%20we%20do%20believe%20that%20any%20use%20of%20chemical%20weapons%20in%20Syria%20would%20very%20likely%20have%20originated%20with%20the%20Assad%20regime.%20The%20Syrian%20government%20has%20been%20battling%20a%20rebellion%20for%20more%20than%20two%20years%2C%20bringing%20international%20condemnation%20of%20the%20regime%20and%20pleas%20for%20greater%20international%20assistance.%20The%20United%20Nations%20estimated%20in%20February%20that%20more%20than%2070%2C000%20people%20had%20died%20since%20the%20conflict%20began.%20The%20administration%20is%20pressing%20for%20a%20comprehensive%20United%20Nations%20investigation%20that%20can%20credibly%20evaluate%20the%20evidence%20and%20establish%20what%20took%20place%2C%20the%20White%20House%20letter%20said.%20Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20Numerous%20questions%20remain%20about%20the%20origins%20of%20the%20chemical%20and%20what%20impact%20its%20apparent%20use%20could%20have%20on%20the%20ongoing%20Syrian%20civil%20war%20and%20international%20involvement%20in%20it.When%20asked%20if%20the%20intelligence%20community%27s%20conclusion%20pushed%20the%20situation%20across%20President%20Barack%20Obama%27s%20red%20line%20that%20could%20potentially%20trigger%20more%20U.S.%20involvement%20in%20the%20Syrian%20civil%20war%2C%20Hagel%20said%20it%27s%20too%20soon%20to%20say.We%20need%20all%20the%20facts.%20We%20need%20all%20the%20information%2C%20he%20said.%20What%20I%27ve%20just%20given%20you%20is%20what%20our%20intelligence%20community%20has%20said%20they%20know.%20As%20I%20also%20said%2C%20they%20are%20still%20assessing%20and%20they%20are%20still%20looking%20at%20what%20happened%2C%20who%20was%20responsible%20and%20the%20other%20specifics%20that%20we%27ll%20need.%20In%20a%20letter%20sent%20to%20lawmakers%20before%20Hagel%27s%20announcement%2C%20the%20White%20House%20said%20that%20intelligence%20analysts%20have%20concluded%20with%20varying%20degrees%20of%20confidence%20that%20the%20Syrian%20regime%20has%20used%20chemical%20weapons%20on%20a%20small%20scale%20in%20Syria%2C%20specifically%20the%20chemical%20agent%20sarin.%20In%20the%20letter%2C%20signed%20by%20White%20House%20legislative%20affairs%20office%20Director%20Miguel%20Rodriguez%2C%20the%20White%20House%20said%20the%20chain%20of%20custody%20of%20the%20chemicals%20was%20not%20clear%20and%20that%20intelligence%20analysts%20could%20not%20confirm%20the%20circumstances%20under%20which%20the%20sarin%20was%20used%2C%20including%20the%20role%20of%20Syrian%20President%20Bashar%20al-Assad%27s%20regime.%20Read%20Rodriguez%27s%20letter%20to%20Levin%20%28PDF%29%20But%2C%20the%20letter%20said%2C%20we%20do%20believe%20that%20any%20use%20of%20chemical%20weapons%20in%20Syria%20would%20very%20likely%20have%20originated%20with%20the%20Assad%20regime.%20The%20Syrian%20government%20has%20been%20battling%20a%20rebellion%20for%20more%20than%20two%20years%2C%20bringing%20international%20condemnation%20of%20the%20regime%20and%20pleas%20for%20greater%20international%20assistance.%20The%20United%20Nations%20estimated%20in%20February%20that%20more%20than%2070%2C000%20people%20had%20died%20since%20the%20conflict%20began.%20The%20administration%20is%20pressing%20for%20a%20comprehensive%20United%20Nations%20investigation%20that%20can%20credibly%20evaluate%20the%20evidence%20and%20establish%20what%20took%20place%2C%20the%20White%20House%20letter%20said.%20Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20ONE..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20TWO..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20THREE..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20FOUR..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20FIVE..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20SIX..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20SEVEN..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20EIGHT..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20NINE..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20TEN..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20ELEVEN..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20THIRTEEN..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20FOURTEEN..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20FIFTEEN..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20SIXTEEN..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20SEVENTEEN..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20EIGHTEEN..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20NINETEEN..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20TWENTY..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20TWENTY%20ONE..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20alpha%20beta%2012%20%20TWENTY%20ONE..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20alpha%20beta%2012%20TWENTY%20ONE..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20alpha%20beta%2012%20TWENTY%20ONE..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20alpha%20beta%2012%20TWENTY%20ONE..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20alpha%20beta%2012%22", "[0,\"Message Too Large\",\"13559014566792817\"]");
#else
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22Numerous%20questions%20remain%20about%20the%20origins%20of%20the%20chemical%20and%20what%20impact%20its%20apparent%20use%20could%20have%20on%20the%20ongoing%20Syrian%20civil%20war%20and%20international%20involvement%20in%20it.When%20asked%20if%20the%20intelligence%20community%27s%20conclusion%20pushed%20the%20situation%20across%20President%20Barack%20Obama%27s%20%5C%22red%20line%5C%22%20that%20could%20potentially%20trigger%20more%20U.S.%20involvement%20in%20the%20Syrian%20civil%20war%2C%20Hagel%20said%20it%27s%20too%20soon%20to%20say.%5C%22We%20need%20all%20the%20facts.%20We%20need%20all%20the%20information%2C%5C%22%20he%20said.%20%5C%22What%20I%27ve%20just%20given%20you%20is%20what%20our%20intelligence%20community%20has%20said%20they%20know.%20As%20I%20also%20said%2C%20they%20are%20still%20assessing%20and%20they%20are%20still%20looking%20at%20what%20happened%2C%20who%20was%20responsible%20and%20the%20other%20specifics%20that%20we%27ll%20need.%5C%22%20In%20a%20letter%20sent%20to%20lawmakers%20before%20Hagel%27s%20announcement%2C%20the%20White%20House%20said%20that%20intelligence%20analysts%20have%20concluded%20%5C%22with%20varying%20degrees%20of%20confidence%20that%20the%20Syrian%20regime%20has%20used%20chemical%20weapons%20on%20a%20small%20scale%20in%20Syria%2C%20specifically%20the%20chemical%20agent%20sarin.%5C%22%20In%20the%20letter%2C%20signed%20by%20White%20House%20legislative%20affairs%20office%20Director%20Miguel%20Rodriguez%2C%20the%20White%20House%20said%20the%20%5C%22chain%20of%20custody%5C%22%20of%20the%20chemicals%20was%20not%20clear%20and%20that%20intelligence%20analysts%20could%20not%20confirm%20the%20circumstances%20under%20which%20the%20sarin%20was%20used%2C%20including%20the%20role%20of%20Syrian%20President%20Bashar%20al-Assad%27s%20regime.%20Read%20Rodriguez%27s%20letter%20to%20Levin%20%28PDF%29%20But%2C%20the%20letter%20said%2C%20%5C%22we%20do%20believe%20that%20any%20use%20of%20chemical%20weapons%20in%20Syria%20would%20very%20likely%20have%20originated%20with%20the%20Assad%20regime.%5C%22%20The%20Syrian%20government%20has%20been%20battling%20a%20rebellion%20for%20more%20than%20two%20years%2C%20bringing%20international%20condemnation%20of%20the%20regime%20and%20pleas%20for%20greater%20international%20assistance.%20The%20United%20Nations%20estimated%20in%20February%20that%20more%20than%2070%2C000%20people%20had%20died%20since%20the%20conflict%20began.%20The%20administration%20is%20%5C%22pressing%20for%20a%20comprehensive%20United%20Nations%20investigation%20that%20can%20credibly%20evaluate%20the%20evidence%20and%20establish%20what%20took%20place%2C%5C%22%20the%20White%20House%20letter%20said.%20Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20Numerous%20questions%20remain%20about%20the%20origins%20of%20the%20chemical%20and%20what%20impact%20its%20apparent%20use%20could%20have%20on%20the%20ongoing%20Syrian%20civil%20war%20and%20international%20involvement%20in%20it.When%20asked%20if%20the%20intelligence%20community%27s%20conclusion%20pushed%20the%20situation%20across%20President%20Barack%20Obama%27s%20%5C%22red%20line%5C%22%20that%20could%20potentially%20trigger%20more%20U.S.%20involvement%20in%20the%20Syrian%20civil%20war%2C%20Hagel%20said%20it%27s%20too%20soon%20to%20say.%5C%22We%20need%20all%20the%20facts.%20We%20need%20all%20the%20information%2C%5C%22%20he%20said.%20%5C%22What%20I%27ve%20just%20given%20you%20is%20what%20our%20intelligence%20community%20has%20said%20they%20know.%20As%20I%20also%20said%2C%20they%20are%20still%20assessing%20and%20they%20are%20still%20looking%20at%20what%20happened%2C%20who%20was%20responsible%20and%20the%20other%20specifics%20that%20we%27ll%20need.%5C%22%20In%20a%20letter%20sent%20to%20lawmakers%20before%20Hagel%27s%20announcement%2C%20the%20White%20House%20said%20that%20intelligence%20analysts%20have%20concluded%20%5C%22with%20varying%20degrees%20of%20confidence%20that%20the%20Syrian%20regime%20has%20used%20chemical%20weapons%20on%20a%20small%20scale%20in%20Syria%2C%20specifically%20the%20chemical%20agent%20sarin.%5C%22%20In%20the%20letter%2C%20signed%20by%20White%20House%20legislative%20affairs%20office%20Director%20Miguel%20Rodriguez%2C%20the%20White%20House%20said%20the%20%5C%22chain%20of%20custody%5C%22%20of%20the%20chemicals%20was%20not%20clear%20and%20that%20intelligence%20analysts%20could%20not%20confirm%20the%20circumstances%20under%20which%20the%20sarin%20was%20used%2C%20including%20the%20role%20of%20Syrian%20President%20Bashar%20al-Assad%27s%20regime.%20Read%20Rodriguez%27s%20letter%20to%20Levin%20%28PDF%29%20But%2C%20the%20letter%20said%2C%20%5C%22we%20do%20believe%20that%20any%20use%20of%20chemical%20weapons%20in%20Syria%20would%20very%20likely%20have%20originated%20with%20the%20Assad%20regime.%5C%22%20The%20Syrian%20government%20has%20been%20battling%20a%20rebellion%20for%20more%20than%20two%20years%2C%20bringing%20international%20condemnation%20of%20the%20regime%20and%20pleas%20for%20greater%20international%20assistance.%20The%20United%20Nations%20estimated%20in%20February%20that%20more%20than%2070%2C000%20people%20had%20died%20since%20the%20conflict%20began.%20The%20administration%20is%20%5C%22pressing%20for%20a%20comprehensive%20United%20Nations%20investigation%20that%20can%20credibly%20evaluate%20the%20evidence%20and%20establish%20what%20took%20place%2C%5C%22%20the%20White%20House%20letter%20said.%20Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20Numerous%20questions%20remain%20about%20the%20origins%20of%20the%20chemical%20and%20what%20impact%20its%20apparent%20use%20could%20have%20on%20the%20ongoing%20Syrian%20civil%20war%20and%20international%20involvement%20in%20it.When%20asked%20if%20the%20intelligence%20community%27s%20conclusion%20pushed%20the%20situation%20across%20President%20Barack%20Obama%27s%20%5C%22red%20line%5C%22%20that%20could%20potentially%20trigger%20more%20U.S.%20involvement%20in%20the%20Syrian%20civil%20war%2C%20Hagel%20said%20it%27s%20too%20soon%20to%20say.%5C%22We%20need%20all%20the%20facts.%20We%20need%20all%20the%20information%2C%5C%22%20he%20said.%20%5C%22What%20I%27ve%20just%20given%20you%20is%20what%20our%20intelligence%20community%20has%20said%20they%20know.%20As%20I%20also%20said%2C%20they%20are%20still%20assessing%20and%20they%20are%20still%20looking%20at%20what%20happened%2C%20who%20was%20responsible%20and%20the%20other%20specifics%20that%20we%27ll%20need.%5C%22%20In%20a%20letter%20sent%20to%20lawmakers%20before%20Hagel%27s%20announcement%2C%20the%20White%20House%20said%20that%20intelligence%20analysts%20have%20concluded%20%5C%22with%20varying%20degrees%20of%20confidence%20that%20the%20Syrian%20regime%20has%20used%20chemical%20weapons%20on%20a%20small%20scale%20in%20Syria%2C%20specifically%20the%20chemical%20agent%20sarin.%5C%22%20In%20the%20letter%2C%20signed%20by%20White%20House%20legislative%20affairs%20office%20Director%20Miguel%20Rodriguez%2C%20the%20White%20House%20said%20the%20%5C%22chain%20of%20custody%5C%22%20of%20the%20chemicals%20was%20not%20clear%20and%20that%20intelligence%20analysts%20could%20not%20confirm%20the%20circumstances%20under%20which%20the%20sarin%20was%20used%2C%20including%20the%20role%20of%20Syrian%20President%20Bashar%20al-Assad%27s%20regime.%20Read%20Rodriguez%27s%20letter%20to%20Levin%20%28PDF%29%20But%2C%20the%20letter%20said%2C%20%5C%22we%20do%20believe%20that%20any%20use%20of%20chemical%20weapons%20in%20Syria%20would%20very%20likely%20have%20originated%20with%20the%20Assad%20regime.%5C%22%20The%20Syrian%20government%20has%20been%20battling%20a%20rebellion%20for%20more%20than%20two%20years%2C%20bringing%20international%20condemnation%20of%20the%20regime%20and%20pleas%20for%20greater%20international%20assistance.%20The%20United%20Nations%20estimated%20in%20February%20that%20more%20than%2070%2C000%20people%20had%20died%20since%20the%20conflict%20began.%20The%20administration%20is%20%5C%22pressing%20for%20a%20comprehensive%20United%20Nations%20investigation%20that%20can%20credibly%20evaluate%20the%20evidence%20and%20establish%20what%20took%20place%2C%5C%22%20the%20White%20House%20letter%20said.%20Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20Numerous%20questions%20remain%20about%20the%20origins%20of%20the%20chemical%20and%20what%20impact%20its%20apparent%20use%20could%20have%20on%20the%20ongoing%20Syrian%20civil%20war%20and%20international%20involvement%20in%20it.When%20asked%20if%20the%20intelligence%20community%27s%20conclusion%20pushed%20the%20situation%20across%20President%20Barack%20Obama%27s%20%5C%22red%20line%5C%22%20that%20could%20potentially%20trigger%20more%20U.S.%20involvement%20in%20the%20Syrian%20civil%20war%2C%20Hagel%20said%20it%27s%20too%20soon%20to%20say.%5C%22We%20need%20all%20the%20facts.%20We%20need%20all%20the%20information%2C%5C%22%20he%20said.%20%5C%22What%20I%27ve%20just%20given%20you%20is%20what%20our%20intelligence%20community%20has%20said%20they%20know.%20As%20I%20also%20said%2C%20they%20are%20still%20assessing%20and%20they%20are%20still%20looking%20at%20what%20happened%2C%20who%20was%20responsible%20and%20the%20other%20specifics%20that%20we%27ll%20need.%5C%22%20In%20a%20letter%20sent%20to%20lawmakers%20before%20Hagel%27s%20announcement%2C%20the%20White%20House%20said%20that%20intelligence%20analysts%20have%20concluded%20%5C%22with%20varying%20degrees%20of%20confidence%20that%20the%20Syrian%20regime%20has%20used%20chemical%20weapons%20on%20a%20small%20scale%20in%20Syria%2C%20specifically%20the%20chemical%20agent%20sarin.%5C%22%20In%20the%20letter%2C%20signed%20by%20White%20House%20legislative%20affairs%20office%20Director%20Miguel%20Rodriguez%2C%20the%20White%20House%20said%20the%20%5C%22chain%20of%20custody%5C%22%20of%20the%20chemicals%20was%20not%20clear%20and%20that%20intelligence%20analysts%20could%20not%20confirm%20the%20circumstances%20under%20which%20the%20sarin%20was%20used%2C%20including%20the%20role%20of%20Syrian%20President%20Bashar%20al-Assad%27s%20regime.%20Read%20Rodriguez%27s%20letter%20to%20Levin%20%28PDF%29%20But%2C%20the%20letter%20said%2C%20%5C%22we%20do%20believe%20that%20any%20use%20of%20chemical%20weapons%20in%20Syria%20would%20very%20likely%20have%20originated%20with%20the%20Assad%20regime.%5C%22%20The%20Syrian%20government%20has%20been%20battling%20a%20rebellion%20for%20more%20than%20two%20years%2C%20bringing%20international%20condemnation%20of%20the%20regime%20and%20pleas%20for%20greater%20international%20assistance.%20The%20United%20Nations%20estimated%20in%20February%20that%20more%20than%2070%2C000%20people%20had%20died%20since%20the%20conflict%20began.%20The%20administration%20is%20%5C%22pressing%20for%20a%20comprehensive%20United%20Nations%20investigation%20that%20can%20credibly%20evaluate%20the%20evidence%20and%20establish%20what%20took%20place%2C%5C%22%20the%20White%20House%20letter%20said.%20Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20Numerous%20questions%20remain%20about%20the%20origins%20of%20the%20chemical%20and%20what%20impact%20its%20apparent%20use%20could%20have%20on%20the%20ongoing%20Syrian%20civil%20war%20and%20international%20involvement%20in%20it.When%20asked%20if%20the%20intelligence%20community%27s%20conclusion%20pushed%20the%20situation%20across%20President%20Barack%20Obama%27s%20%5C%22red%20line%5C%22%20that%20could%20potentially%20trigger%20more%20U.S.%20involvement%20in%20the%20Syrian%20civil%20war%2C%20Hagel%20said%20it%27s%20too%20soon%20to%20say.%5C%22We%20need%20all%20the%20facts.%20We%20need%20all%20the%20information%2C%5C%22%20he%20said.%20%5C%22What%20I%27ve%20just%20given%20you%20is%20what%20our%20intelligence%20community%20has%20said%20they%20know.%20As%20I%20also%20said%2C%20they%20are%20still%20assessing%20and%20they%20are%20still%20looking%20at%20what%20happened%2C%20who%20was%20responsible%20and%20the%20other%20specifics%20that%20we%27ll%20need.%5C%22%20In%20a%20letter%20sent%20to%20lawmakers%20before%20Hagel%27s%20announcement%2C%20the%20White%20House%20said%20that%20intelligence%20analysts%20have%20concluded%20%5C%22with%20varying%20degrees%20of%20confidence%20that%20the%20Syrian%20regime%20has%20used%20chemical%20weapons%20on%20a%20small%20scale%20in%20Syria%2C%20specifically%20the%20chemical%20agent%20sarin.%5C%22%20In%20the%20letter%2C%20signed%20by%20White%20House%20legislative%20affairs%20office%20Director%20Miguel%20Rodriguez%2C%20the%20White%20House%20said%20the%20%5C%22chain%20of%20custody%5C%22%20of%20the%20chemicals%20was%20not%20clear%20and%20that%20intelligence%20analysts%20could%20not%20confirm%20the%20circumstances%20under%20which%20the%20sarin%20was%20used%2C%20including%20the%20role%20of%20Syrian%20President%20Bashar%20al-Assad%27s%20regime.%20Read%20Rodriguez%27s%20letter%20to%20Levin%20%28PDF%29%20But%2C%20the%20letter%20said%2C%20%5C%22we%20do%20believe%20that%20any%20use%20of%20chemical%20weapons%20in%20Syria%20would%20very%20likely%20have%20originated%20with%20the%20Assad%20regime.%5C%22%20The%20Syrian%20government%20has%20been%20battling%20a%20rebellion%20for%20more%20than%20two%20years%2C%20bringing%20international%20condemnation%20of%20the%20regime%20and%20pleas%20for%20greater%20international%20assistance.%20The%20United%20Nations%20estimated%20in%20February%20that%20more%20than%2070%2C000%20people%20had%20died%20since%20the%20conflict%20began.%20The%20administration%20is%20%5C%22pressing%20for%20a%20comprehensive%20United%20Nations%20investigation%20that%20can%20credibly%20evaluate%20the%20evidence%20and%20establish%20what%20took%20place%2C%5C%22%20the%20White%20House%20letter%20said.%20Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20Numerous%20questions%20remain%20about%20the%20origins%20of%20the%20chemical%20and%20what%20impact%20its%20apparent%20use%20could%20have%20on%20the%20ongoing%20Syrian%20civil%20war%20and%20international%20involvement%20in%20it.When%20asked%20if%20the%20intelligence%20community%27s%20conclusion%20pushed%20the%20situation%20across%20President%20Barack%20Obama%27s%20%5C%22red%20line%5C%22%20that%20could%20potentially%20trigger%20more%20U.S.%20involvement%20in%20the%20Syrian%20civil%20war%2C%20Hagel%20said%20it%27s%20too%20soon%20to%20say.%5C%22We%20need%20all%20the%20facts.%20We%20need%20all%20the%20information%2C%5C%22%20he%20said.%20%5C%22What%20I%27ve%20just%20given%20you%20is%20what%20our%20intelligence%20community%20has%20said%20they%20know.%20As%20I%20also%20said%2C%20they%20are%20still%20assessing%20and%20they%20are%20still%20looking%20at%20what%20happened%2C%20who%20was%20responsible%20and%20the%20other%20specifics%20that%20we%27ll%20need.%5C%22%20In%20a%20letter%20sent%20to%20lawmakers%20before%20Hagel%27s%20announcement%2C%20the%20White%20House%20said%20that%20intelligence%20analysts%20have%20concluded%20%5C%22with%20varying%20degrees%20of%20confidence%20that%20the%20Syrian%20regime%20has%20used%20chemical%20weapons%20on%20a%20small%20scale%20in%20Syria%2C%20specifically%20the%20chemical%20agent%20sarin.%5C%22%20In%20the%20letter%2C%20signed%20by%20White%20House%20legislative%20affairs%20office%20Director%20Miguel%20Rodriguez%2C%20the%20White%20House%20said%20the%20%5C%22chain%20of%20custody%5C%22%20of%20the%20chemicals%20was%20not%20clear%20and%20that%20intelligence%20analysts%20could%20not%20confirm%20the%20circumstances%20under%20which%20the%20sarin%20was%20used%2C%20including%20the%20role%20of%20Syrian%20President%20Bashar%20al-Assad%27s%20regime.%20Read%20Rodriguez%27s%20letter%20to%20Levin%20%28PDF%29%20But%2C%20the%20letter%20said%2C%20%5C%22we%20do%20believe%20that%20any%20use%20of%20chemical%20weapons%20in%20Syria%20would%20very%20likely%20have%20originated%20with%20the%20Assad%20regime.%5C%22%20The%20Syrian%20government%20has%20been%20battling%20a%20rebellion%20for%20more%20than%20two%20years%2C%20bringing%20international%20condemnation%20of%20the%20regime%20and%20pleas%20for%20greater%20international%20assistance.%20The%20United%20Nations%20estimated%20in%20February%20that%20more%20than%2070%2C000%20people%20had%20died%20since%20the%20conflict%20began.%20The%20administration%20is%20%5C%22pressing%20for%20a%20comprehensive%20United%20Nations%20investigation%20that%20can%20credibly%20evaluate%20the%20evidence%20and%20establish%20what%20took%20place%2C%5C%22%20the%20White%20House%20letter%20said.%20Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20Numerous%20questions%20remain%20about%20the%20origins%20of%20the%20chemical%20and%20what%20impact%20its%20apparent%20use%20could%20have%20on%20the%20ongoing%20Syrian%20civil%20war%20and%20international%20involvement%20in%20it.When%20asked%20if%20the%20intelligence%20community%27s%20conclusion%20pushed%20the%20situation%20across%20President%20Barack%20Obama%27s%20%5C%22red%20line%5C%22%20that%20could%20potentially%20trigger%20more%20U.S.%20involvement%20in%20the%20Syrian%20civil%20war%2C%20Hagel%20said%20it%27s%20too%20soon%20to%20say.%5C%22We%20need%20all%20the%20facts.%20We%20need%20all%20the%20information%2C%5C%22%20he%20said.%20%5C%22What%20I%27ve%20just%20given%20you%20is%20what%20our%20intelligence%20community%20has%20said%20they%20know.%20As%20I%20also%20said%2C%20they%20are%20still%20assessing%20and%20they%20are%20still%20looking%20at%20what%20happened%2C%20who%20was%20responsible%20and%20the%20other%20specifics%20that%20we%27ll%20need.%5C%22%20In%20a%20letter%20sent%20to%20lawmakers%20before%20Hagel%27s%20announcement%2C%20the%20White%20House%20said%20that%20intelligence%20analysts%20have%20concluded%20%5C%22with%20varying%20degrees%20of%20confidence%20that%20the%20Syrian%20regime%20has%20used%20chemical%20weapons%20on%20a%20small%20scale%20in%20Syria%2C%20specifically%20the%20chemical%20agent%20sarin.%5C%22%20In%20the%20letter%2C%20signed%20by%20White%20House%20legislative%20affairs%20office%20Director%20Miguel%20Rodriguez%2C%20the%20White%20House%20said%20the%20%5C%22chain%20of%20custody%5C%22%20of%20the%20chemicals%20was%20not%20clear%20and%20that%20intelligence%20analysts%20could%20not%20confirm%20the%20circumstances%20under%20which%20the%20sarin%20was%20used%2C%20including%20the%20role%20of%20Syrian%20President%20Bashar%20al-Assad%27s%20regime.%20Read%20Rodriguez%27s%20letter%20to%20Levin%20%28PDF%29%20But%2C%20the%20letter%20said%2C%20%5C%22we%20do%20believe%20that%20any%20use%20of%20chemical%20weapons%20in%20Syria%20would%20very%20likely%20have%20originated%20with%20the%20Assad%20regime.%5C%22%20The%20Syrian%20government%20has%20been%20battling%20a%20rebellion%20for%20more%20than%20two%20years%2C%20bringing%20international%20condemnation%20of%20the%20regime%20and%20pleas%20for%20greater%20international%20assistance.%20The%20United%20Nations%20estimated%20in%20February%20that%20more%20than%2070%2C000%20people%20had%20died%20since%20the%20conflict%20began.%20The%20administration%20is%20%5C%22pressing%20for%20a%20comprehensive%20United%20Nations%20investigation%20that%20can%20credibly%20evaluate%20the%20evidence%20and%20establish%20what%20took%20place%2C%5C%22%20the%20White%20House%20letter%20said.%20Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20Numerous%20questions%20remain%20about%20the%20origins%20of%20the%20chemical%20and%20what%20impact%20its%20apparent%20use%20could%20have%20on%20the%20ongoing%20Syrian%20civil%20war%20and%20international%20involvement%20in%20it.When%20asked%20if%20the%20intelligence%20community%27s%20conclusion%20pushed%20the%20situation%20across%20President%20Barack%20Obama%27s%20%5C%22red%20line%5C%22%20that%20could%20potentially%20trigger%20more%20U.S.%20involvement%20in%20the%20Syrian%20civil%20war%2C%20Hagel%20said%20it%27s%20too%20soon%20to%20say.%5C%22We%20need%20all%20the%20facts.%20We%20need%20all%20the%20information%2C%5C%22%20he%20said.%20%5C%22What%20I%27ve%20just%20given%20you%20is%20what%20our%20intelligence%20community%20has%20said%20they%20know.%20As%20I%20also%20said%2C%20they%20are%20still%20assessing%20and%20they%20are%20still%20looking%20at%20what%20happened%2C%20who%20was%20responsible%20and%20the%20other%20specifics%20that%20we%27ll%20need.%5C%22%20In%20a%20letter%20sent%20to%20lawmakers%20before%20Hagel%27s%20announcement%2C%20the%20White%20House%20said%20that%20intelligence%20analysts%20have%20concluded%20%5C%22with%20varying%20degrees%20of%20confidence%20that%20the%20Syrian%20regime%20has%20used%20chemical%20weapons%20on%20a%20small%20scale%20in%20Syria%2C%20specifically%20the%20chemical%20agent%20sarin.%5C%22%20In%20the%20letter%2C%20signed%20by%20White%20House%20legislative%20affairs%20office%20Director%20Miguel%20Rodriguez%2C%20the%20White%20House%20said%20the%20%5C%22chain%20of%20custody%5C%22%20of%20the%20chemicals%20was%20not%20clear%20and%20that%20intelligence%20analysts%20could%20not%20confirm%20the%20circumstances%20under%20which%20the%20sarin%20was%20used%2C%20including%20the%20role%20of%20Syrian%20President%20Bashar%20al-Assad%27s%20regime.%20Read%20Rodriguez%27s%20letter%20to%20Levin%20%28PDF%29%20But%2C%20the%20letter%20said%2C%20%5C%22we%20do%20believe%20that%20any%20use%20of%20chemical%20weapons%20in%20Syria%20would%20very%20likely%20have%20originated%20with%20the%20Assad%20regime.%5C%22%20The%20Syrian%20government%20has%20been%20battling%20a%20rebellion%20for%20more%20than%20two%20years%2C%20bringing%20international%20condemnation%20of%20the%20regime%20and%20pleas%20for%20greater%20international%20assistance.%20The%20United%20Nations%20estimated%20in%20February%20that%20more%20than%2070%2C000%20people%20had%20died%20since%20the%20conflict%20began.%20The%20administration%20is%20%5C%22pressing%20for%20a%20comprehensive%20United%20Nations%20investigation%20that%20can%20credibly%20evaluate%20the%20evidence%20and%20establish%20what%20took%20place%2C%5C%22%20the%20White%20House%20letter%20said.%20Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20Numerous%20questions%20remain%20about%20the%20origins%20of%20the%20chemical%20and%20what%20impact%20its%20apparent%20use%20could%20have%20on%20the%20ongoing%20Syrian%20civil%20war%20and%20international%20involvement%20in%20it.When%20asked%20if%20the%20intelligence%20community%27s%20conclusion%20pushed%20the%20situation%20across%20President%20Barack%20Obama%27s%20%5C%22red%20line%5C%22%20that%20could%20potentially%20trigger%20more%20U.S.%20involvement%20in%20the%20Syrian%20civil%20war%2C%20Hagel%20said%20it%27s%20too%20soon%20to%20say.%5C%22We%20need%20all%20the%20facts.%20We%20need%20all%20the%20information%2C%5C%22%20he%20said.%20%5C%22What%20I%27ve%20just%20given%20you%20is%20what%20our%20intelligence%20community%20has%20said%20they%20know.%20As%20I%20also%20said%2C%20they%20are%20still%20assessing%20and%20they%20are%20still%20looking%20at%20what%20happened%2C%20who%20was%20responsible%20and%20the%20other%20specifics%20that%20we%27ll%20need.%5C%22%20In%20a%20letter%20sent%20to%20lawmakers%20before%20Hagel%27s%20announcement%2C%20the%20White%20House%20said%20that%20intelligence%20analysts%20have%20concluded%20%5C%22with%20varying%20degrees%20of%20confidence%20that%20the%20Syrian%20regime%20has%20used%20chemical%20weapons%20on%20a%20small%20scale%20in%20Syria%2C%20specifically%20the%20chemical%20agent%20sarin.%5C%22%20In%20the%20letter%2C%20signed%20by%20White%20House%20legislative%20affairs%20office%20Director%20Miguel%20Rodriguez%2C%20the%20White%20House%20said%20the%20%5C%22chain%20of%20custody%5C%22%20of%20the%20chemicals%20was%20not%20clear%20and%20that%20intelligence%20analysts%20could%20not%20confirm%20the%20circumstances%20under%20which%20the%20sarin%20was%20used%2C%20including%20the%20role%20of%20Syrian%20President%20Bashar%20al-Assad%27s%20regime.%20Read%20Rodriguez%27s%20letter%20to%20Levin%20%28PDF%29%20But%2C%20the%20letter%20said%2C%20%5C%22we%20do%20believe%20that%20any%20use%20of%20chemical%20weapons%20in%20Syria%20would%20very%20likely%20have%20originated%20with%20the%20Assad%20regime.%5C%22%20The%20Syrian%20government%20has%20been%20battling%20a%20rebellion%20for%20more%20than%20two%20years%2C%20bringing%20international%20condemnation%20of%20the%20regime%20and%20pleas%20for%20greater%20international%20assistance.%20The%20United%20Nations%20estimated%20in%20February%20that%20more%20than%2070%2C000%20people%20had%20died%20since%20the%20conflict%20began.%20The%20administration%20is%20%5C%22pressing%20for%20a%20comprehensive%20United%20Nations%20investigation%20that%20can%20credibly%20evaluate%20the%20evidence%20and%20establish%20what%20took%20place%2C%5C%22%20the%20White%20House%20letter%20said.%20Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20Numerous%20questions%20remain%20about%20the%20origins%20of%20the%20chemical%20and%20what%20impact%20its%20apparent%20use%20could%20have%20on%20the%20ongoing%20Syrian%20civil%20war%20and%20international%20involvement%20in%20it.When%20asked%20if%20the%20intelligence%20community%27s%20conclusion%20pushed%20the%20situation%20across%20President%20Barack%20Obama%27s%20%5C%22red%20line%5C%22%20that%20could%20potentially%20trigger%20more%20U.S.%20involvement%20in%20the%20Syrian%20civil%20war%2C%20Hagel%20said%20it%27s%20too%20soon%20to%20say.%5C%22We%20need%20all%20the%20facts.%20We%20need%20all%20the%20information%2C%5C%22%20he%20said.%20%5C%22What%20I%27ve%20just%20given%20you%20is%20what%20our%20intelligence%20community%20has%20said%20they%20know.%20As%20I%20also%20said%2C%20they%20are%20still%20assessing%20and%20they%20are%20still%20looking%20at%20what%20happened%2C%20who%20was%20responsible%20and%20the%20other%20specifics%20that%20we%27ll%20need.%5C%22%20In%20a%20letter%20sent%20to%20lawmakers%20before%20Hagel%27s%20announcement%2C%20the%20White%20House%20said%20that%20intelligence%20analysts%20have%20concluded%20%5C%22with%20varying%20degrees%20of%20confidence%20that%20the%20Syrian%20regime%20has%20used%20chemical%20weapons%20on%20a%20small%20scale%20in%20Syria%2C%20specifically%20the%20chemical%20agent%20sarin.%5C%22%20In%20the%20letter%2C%20signed%20by%20White%20House%20legislative%20affairs%20office%20Director%20Miguel%20Rodriguez%2C%20the%20White%20House%20said%20the%20%5C%22chain%20of%20custody%5C%22%20of%20the%20chemicals%20was%20not%20clear%20and%20that%20intelligence%20analysts%20could%20not%20confirm%20the%20circumstances%20under%20which%20the%20sarin%20was%20used%2C%20including%20the%20role%20of%20Syrian%20President%20Bashar%20al-Assad%27s%20regime.%20Read%20Rodriguez%27s%20letter%20to%20Levin%20%28PDF%29%20But%2C%20the%20letter%20said%2C%20%5C%22we%20do%20believe%20that%20any%20use%20of%20chemical%20weapons%20in%20Syria%20would%20very%20likely%20have%20originated%20with%20the%20Assad%20regime.%5C%22%20The%20Syrian%20government%20has%20been%20battling%20a%20rebellion%20for%20more%20than%20two%20years%2C%20bringing%20international%20condemnation%20of%20the%20regime%20and%20pleas%20for%20greater%20international%20assistance.%20The%20United%20Nations%20estimated%20in%20February%20that%20more%20than%2070%2C000%20people%20had%20died%20since%20the%20conflict%20began.%20The%20administration%20is%20%5C%22pressing%20for%20a%20comprehensive%20United%20Nations%20investigation%20that%20can%20credibly%20evaluate%20the%20evidence%20and%20establish%20what%20took%20place%2C%5C%22%20the%20White%20House%20letter%20said.%20Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20ONE..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20TWO..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20THREE..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20FOUR..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20FIVE..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20SIX..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20SEVEN..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20EIGHT..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20NINE..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20TEN..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20ELEVEN..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20THIRTEEN..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20FOURTEEN..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20FIFTEEN..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20SIXTEEN..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20SEVENTEEN..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20EIGHTEEN..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20NINETEEN..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20TWENTY..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20TWENTY%20ONE..Sen.%20John%20McCain%2C%20one%20of%20the%20lawmakers%20who%20received%20the%20letter%2C%20said%20the%20use%20of%20chemical%20weapons%20was%20only%20a%20matter%20of%20time.%20alpha%20beta%2012%22", "[0,\"Message Too Large\",\"13559014566792817\"]");
#endif
            return data;
        }

        private Dictionary<string, string> LoadWhenDetailedHistoryIsRequestedDetailHistoryCount10ReturnsRecords()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v2/history/sub-key/demo-36/channel/hello_my_channel", "[[\"f42pIQcWZ9zbTbH8cyLwB/tdvRxjFLOYcBNMVKeHS54=\",\"Pubnub API Usage Example\",\"nQTUCOeyWWgWh5NRLhSlhIingu92WIQ6RFloD9rOZsTUjAhD7AkMaZJVgU7l28e2\",\"Pubnub API Usage Example\",\"nQTUCOeyWWgWh5NRLhSlhIingu92WIQ6RFloD9rOZsTUjAhD7AkMaZJVgU7l28e2\",\"+BY5/miAA8aeuhVl4d13Kg==\",\"Pubnub API Usage Example\",\"f42pIQcWZ9zbTbH8cyLwB/tdvRxjFLOYcBNMVKeHS54=\",{\"foo\":\"hi!\",\"bar\":[1,2,3,4,5]},\"Pubnub Messaging API 1\"],13559191494674157,13559319777162196]");
            return data;
        }

        private Dictionary<string, string> LoadWhenDetailedHistoryIsRequestedDetailHistoryCount10ReverseTrueReturnsRecords()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v2/history/sub-key/demo-36/channel/hello_my_channel", "[[\"Pubnub API Usage Example\",\"nQTUCOeyWWgWh5NRLhSlhIingu92WIQ6RFloD9rOZsTUjAhD7AkMaZJVgU7l28e2\",\"+BY5/miAA8aeuhVl4d13Kg==\",\"Pubnub API Usage Example\",\"f42pIQcWZ9zbTbH8cyLwB/tdvRxjFLOYcBNMVKeHS54=\",{\"foo\":\"hi!\",\"bar\":[1,2,3,4,5]},\"Pubnub Messaging API 1\",\"DetailedHistoryStartTimeWithReverseTrue 13557486100000000 0\",\"DetailedHistoryStartTimeWithReverseTrue 13557486100000000 1\",\"DetailedHistoryStartTimeWithReverseTrue 13557486100000000 3\"],13557486057035336,13557486128690220]");
            return data;
        }

        private Dictionary<string, string> LoadWhenDetailedHistoryIsRequestedDetailedHistoryStartWithReverseTrue()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v2/history/sub-key/demo-36/channel/hello_my_channel", "[[\"DetailedHistoryStartTimeWithReverseTrue 0\",\"DetailedHistoryStartTimeWithReverseTrue 1\",\"DetailedHistoryStartTimeWithReverseTrue 2\",\"DetailedHistoryStartTimeWithReverseTrue 3\",\"DetailedHistoryStartTimeWithReverseTrue 4\",\"DetailedHistoryStartTimeWithReverseTrue 5\",\"DetailedHistoryStartTimeWithReverseTrue 6\",\"DetailedHistoryStartTimeWithReverseTrue 7\",\"DetailedHistoryStartTimeWithReverseTrue 8\",\"DetailedHistoryStartTimeWithReverseTrue 9\"],13559326456056557,13559327017296315]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22DetailedHistoryStartTimeWithReverseTrue%200%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22DetailedHistoryStartTimeWithReverseTrue%201%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22DetailedHistoryStartTimeWithReverseTrue%202%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22DetailedHistoryStartTimeWithReverseTrue%203%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22DetailedHistoryStartTimeWithReverseTrue%204%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22DetailedHistoryStartTimeWithReverseTrue%205%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22DetailedHistoryStartTimeWithReverseTrue%206%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22DetailedHistoryStartTimeWithReverseTrue%207%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22DetailedHistoryStartTimeWithReverseTrue%208%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22DetailedHistoryStartTimeWithReverseTrue%209%22", "[1,\"Sent\",\"13559014566792817\"]");
            return data;
        }

        private Dictionary<string, string> LoadWhenDetailedHistoryIsRequestedDetailHistoryWithNullKeysReturnsError()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v2/history/sub-key//channel/hello_my_channel", "{\"status\":400,\"message\":\"Could Not Parse Request\",\"service\":\"Access Manager\",\"error\":true}");
            return data;
        }

        private Dictionary<string, string> LoadWhenDetailedHistoryIsRequestedDetailHistoryNoStoreShouldNotGetMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22Pubnub%20Messaging%20With%20No%20Storage%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/pam/pam/0/hello_my_channel/0/%22Pubnub%20Messaging%20With%20No%20Storage%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/v2/history/sub-key/demo-36/channel/hello_my_channel", "[[],0,0]");
            data.Add("/v2/history/sub-key/pam/channel/hello_my_channel", "[[],0,0]");
            return data;
        }

        private Dictionary<string, string> LoadWhenDetailedHistoryIsRequestedDetailHistoryShouldReturnDecryptMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22f42pIQcWZ9zbTbH8cyLwByD%2FGsviOE0vcREIEVPARR0%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/v2/history/sub-key/demo-36/channel/hello_my_channel", "[[\"f42pIQcWZ9zbTbH8cyLwByD/GsviOE0vcREIEVPARR0=\"],14271207051615569,14271207051615569]");
            return data;
        }

        private Dictionary<string, string> LoadWhenDetailedHistoryIsRequestedDetailHistoryShouldReturnServerTime1()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/time/0", "[14271224264234400]");
            return data;
        }

        private Dictionary<string, string> LoadWhenDetailedHistoryIsRequestedDetailHistoryShouldReturnServerTime2()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/time/0", "[14271224327449617]");
            return data;
        }

        private Dictionary<string, string> LoadWhenDetailedHistoryIsRequestedDetailHistoryShouldReturnServerTime3()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/time/0", "[14271224393563309]");
            return data;
        }

        private Dictionary<string, string> LoadWhenDetailedHistoryIsRequestedDetailedHistoryShouldReturnEncryptedMessageBasedOnParams()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
                       
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22kvIeHmojsLyV1KMBo82DYQ%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22Ld0rZfbe4yN0Qj4V7o2BuQ%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22zNlnhYco9o6a646%2BOx6ksg%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22mR8EEMx154BBHU3OOa%2BYjQ%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22v%2BviLoq0Gj2docUMAYyoYg%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22F2ZPfJnzuU34VKe24ds81A%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%222K%2FTO5WADvJRhvX7Zk0IpQ%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22oWOYyGxkWFJ1gpJxhcyzjA%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22LwEzvPCHdM8Yagg6oKknvg%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22%2FjjH%2FPT4NrK5HHjDT2KAlQ%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/publish/demo-36/demo-36/155b7bd8269bacde295010fd1a10899f/hello_my_channel/0/%22kvIeHmojsLyV1KMBo82DYQ%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/d717121f8d51b777e0a2dbe5dc8f818f/hello_my_channel/0/%22Ld0rZfbe4yN0Qj4V7o2BuQ%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/4b79cac17fc4c52cde119c8b09d6a70c/hello_my_channel/0/%22zNlnhYco9o6a646%2BOx6ksg%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/57c79052b0ccc0f63d6bd5de5e7ffe05/hello_my_channel/0/%22mR8EEMx154BBHU3OOa%2BYjQ%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/d90d19ce93013ffa3ae76dd40b9bbad5/hello_my_channel/0/%22v%2BviLoq0Gj2docUMAYyoYg%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/publish/demo-36/demo-36/292b383b26ba55fc38671eb9749ca27d/hello_my_channel/0/%22F2ZPfJnzuU34VKe24ds81A%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/ee1c53de3bd8ce7c63703881fb30dad0/hello_my_channel/0/%222K%2FTO5WADvJRhvX7Zk0IpQ%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/da9219cd36be50e44af1f13c30e3a0fc/hello_my_channel/0/%22oWOYyGxkWFJ1gpJxhcyzjA%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/06d3a3c66138d3e1e98c16cb4b28b447/hello_my_channel/0/%22LwEzvPCHdM8Yagg6oKknvg%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/ba5ec8b3290acff6505e156e91838906/hello_my_channel/0/%22%2FjjH%2FPT4NrK5HHjDT2KAlQ%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/v2/history/sub-key/demo-36/channel/hello_my_channel?count=5&reverse=true&start=14271224264234400&end=14271224327449617&uuid=myuuid", "[[\"kvIeHmojsLyV1KMBo82DYQ==\",\"Ld0rZfbe4yN0Qj4V7o2BuQ==\",\"zNlnhYco9o6a646+Ox6ksg==\",\"mR8EEMx154BBHU3OOa+YjQ==\",\"v+viLoq0Gj2docUMAYyoYg==\"],14271224271199908,14271224324230142]");
            data.Add("/v2/history/sub-key/demo-36/channel/hello_my_channel?count=5&reverse=true&start=14271224327449616&uuid=myuuid", "[[\"F2ZPfJnzuU34VKe24ds81A==\",\"2K/TO5WADvJRhvX7Zk0IpQ==\",\"oWOYyGxkWFJ1gpJxhcyzjA==\",\"LwEzvPCHdM8Yagg6oKknvg==\",\"/jjH/PT4NrK5HHjDT2KAlQ==\"],14271224337423013,14271224390377690]");
            data.Add("/v2/history/sub-key/demo-36/channel/hello_my_channel?count=5&start=14271224327449616&uuid=myuuid", "[[\"kvIeHmojsLyV1KMBo82DYQ==\",\"Ld0rZfbe4yN0Qj4V7o2BuQ==\",\"zNlnhYco9o6a646+Ox6ksg==\",\"mR8EEMx154BBHU3OOa+YjQ==\",\"v+viLoq0Gj2docUMAYyoYg==\"],14271224271199908,14271224324230142]");

            return data;
        }

        private Dictionary<string, string> LoadWhenDetailedHistoryIsRequestedDetailedHistoryShouldReturnUnencryptedMessageBasedOnParams()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/0", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/1", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/2", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/3", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/4", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/5.1", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/6.1", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/7.1", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/8.1", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/9.1", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/publish/demo-36/demo-36/6c45a5d05e409fbac6b50d10027b276f/hello_my_channel/0/0", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/bf08bd19cea9915a5e557529c4a29028/hello_my_channel/0/1", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/cc125f5ded91bf364369346d39b09adb/hello_my_channel/0/2", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/dc69f4352ddbe3252e5a0f1d389bf186/hello_my_channel/0/3", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/ab26456775f28be0232efe7a92545ffa/hello_my_channel/0/4", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/publish/demo-36/demo-36/638c03dfeb41e9b61e8b774001559e1d/hello_my_channel/0/5.1", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/7dc6b36e781f7c426c97efbdb30d87a9/hello_my_channel/0/6.1", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/56ec2f91fb9d78c3c0f5a04e9b138576/hello_my_channel/0/7.1", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/9034b22230cb5e1eab4f8d549422771e/hello_my_channel/0/8.1", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/1118a02dc5d490ee212f175775452329/hello_my_channel/0/9.1", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/v2/history/sub-key/demo-36/channel/hello_my_channel?count=5&reverse=true&start=14271224264234400&end=14271224327449617&uuid=myuuid", "[[0,1,2,3,4],14271224271199908,14271224324230142]");
            data.Add("/v2/history/sub-key/demo-36/channel/hello_my_channel?count=5&reverse=true&start=14271224327449616&uuid=myuuid", "[[5.1,6.1,7.1,8.1],14271224337423013,14271224390377690]");
            data.Add("/v2/history/sub-key/demo-36/channel/hello_my_channel?count=5&start=14271224327449616&uuid=myuuid", "[[0,1,2,3,4],14271224271199908,14271224324230142]");

            return data;
        }

        private Dictionary<string, string> LoadWhenGetRequestServerTimeThenItShouldReturnTimeStamp()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/time/0", "[13559011090230537]");
            return data;
        }

        private Dictionary<string, string> LoadWhenGetRequestServerTimeThenWithProxyItShouldReturnTimeStamp()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/time/0", "[13559011090230537]");
            return data;
        }

        private Dictionary<string, string> LoadWhenSubscribedToAChannelThenSubscribeShouldReturnReceivedMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22Test%20for%20WhenSubscribedToAChannel%20ThenItShouldReturnReceivedMessage%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22Zbr7pEF%2FGFGKj1rOstp0tWzA4nwJXEfj%2BezLtAr8qqE%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/d4a031148880d7c3d96251f02586726c/hello_my_channel/0/%22Zbr7pEF%2FGFGKj1rOstp0tWzA4nwJXEfj%2BezLtAr8qqE%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/47770aa0273635b0694b07336afeb435/hello_my_channel/0/%22Test%20for%20WhenSubscribedToAChannel%20ThenItShouldReturnReceivedMessage%22", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/subscribe/demo-36/hello_my_channel/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559006802662768", "[[\"Test for WhenSubscribedToAChannel ThenItShouldReturnReceivedMessage\"],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559014566792816", "[[],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel/leave", "{\"action\": \"leave\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenSubscribedToAChannelThenSubscribeShouldReturnReceivedCipherMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22QoHwTga0QtOCtJRQ6sqtyateB%2FVotNt%2F50y23yXW7rpCbZdJLUAVKKbf01SpN6zghA6MqQaaHRXoYqAf84RF56C7Ky6Oi6jLqN2I5%2FlXSCw%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/d4a031148880d7c3d96251f02586726c/hello_my_channel/0/%22Zbr7pEF%2FGFGKj1rOstp0tWzA4nwJXEfj%2BezLtAr8qqE%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/cc7da4d07401634f5cad21ce254d7787/hello_my_channel/0/%22QoHwTga0QtOCtJRQ6sqtyateB%2FVotNt%2F50y23yXW7rpCbZdJLUAVKKbf01SpN6zghA6MqQaaHRXoYqAf84RF56C7Ky6Oi6jLqN2I5%2FlXSCw%3D%22", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/subscribe/demo-36/hello_my_channel/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559006802662768", "[[\"QoHwTga0QtOCtJRQ6sqtyateB/VotNt/50y23yXW7rpCbZdJLUAVKKbf01SpN6zghA6MqQaaHRXoYqAf84RF56C7Ky6Oi6jLqN2I5/lXSCw=\"],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559014566792816", "[[],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel/leave", "{\"action\": \"leave\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenSubscribedToAChannelThenSubscribeShouldReturnReceivedComplexMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%7B%22foo%22%3A%22hi%21%22%2C%22bar%22%3A%5B1%2C2%2C3%2C4%2C5%5D%7D", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/a50a618029da59507f80118062de229a/hello_my_channel/0/%7B%22foo%22%3A%22hi%21%22%2C%22bar%22%3A%5B1%2C2%2C3%2C4%2C5%5D%7D", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/d4a031148880d7c3d96251f02586726c/hello_my_channel/0/%22Zbr7pEF%2FGFGKj1rOstp0tWzA4nwJXEfj%2BezLtAr8qqE%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22Zbr7pEF%2FGFGKj1rOstp0tWzA4nwJXEfj%2BezLtAr8qqE%3D%22", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/subscribe/demo-36/hello_my_channel/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559006802662768", "[[{\"foo\":\"hi!\",\"bar\":[1,2,3,4,5]}],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559014566792816", "[[],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel/leave", "{\"action\": \"leave\"}");
            return data;
        }
        private Dictionary<string, string> LoadWhenSubscribedToAChannelThenSubscribeShouldReturnReceivedCipherComplexMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%7B%22foo%22%3A%22hi%21%22%2C%22bar%22%3A%5B1%2C2%2C3%2C4%2C5%5D%7D", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/a50a618029da59507f80118062de229a/hello_my_channel/0/%7B%22foo%22%3A%22hi%21%22%2C%22bar%22%3A%5B1%2C2%2C3%2C4%2C5%5D%7D", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/d4a031148880d7c3d96251f02586726c/hello_my_channel/0/%22Zbr7pEF%2FGFGKj1rOstp0tWzA4nwJXEfj%2BezLtAr8qqE%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22Zbr7pEF%2FGFGKj1rOstp0tWzA4nwJXEfj%2BezLtAr8qqE%3D%22", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/subscribe/demo-36/hello_my_channel/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559006802662768", "[[\"Zbr7pEF/GFGKj1rOstp0tWzA4nwJXEfj+ezLtAr8qqE=\"],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559014566792816", "[[],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel/leave", "{\"action\": \"leave\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenSubscribedToAChannelThenSubscribeShouldReturnReceivedEmojiMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22Text%20with%20%5Cud83d%5Cude1c%20emoji%20%5Cud83c%5Cudf89.%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/8003ac87ee0d9c9ba205c9697972891c/hello_my_channel/0/%22Text%20with%20%5Cud83d%5Cude1c%20emoji%20%5Cud83c%5Cudf89.%22", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/subscribe/demo-36/hello_my_channel/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559006802662768", "[[\"Text with \ud83d\ude1c emoji \ud83c\udf89.\"],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559014566792816", "[[\"Text with \ud83d\ude1c emoji \ud83c\udf89.\"],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel/leave", "{\"action\": \"leave\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenSubscribedToAChannelThenSubscribeShouldReturnReceivedCipherEmojiMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/fc80436ca486018b9f97dd7fc93cd9ef/hello_my_channel/0/%22vaD98V5XDtEvByw6RrxT9Ya76GKQLhyrEZw9Otrsu1KBVDIqGgWkrAD8X6TM%2FXC6%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22Zbr7pEF%2FGFGKj1rOstp0tWzA4nwJXEfj%2BezLtAr8qqE%3D%22", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/subscribe/demo-36/hello_my_channel/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559006802662768", "[[\"vaD98V5XDtEvByw6RrxT9Ya76GKQLhyrEZw9Otrsu1KBVDIqGgWkrAD8X6TM/XC6\"],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559014566792816", "[[\"vaD98V5XDtEvByw6RrxT9Ya76GKQLhyrEZw9Otrsu1KBVDIqGgWkrAD8X6TM/XC6\"],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel/leave", "{\"action\": \"leave\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenSubscribedToAChannelThenSubscribeShouldReturnUnicodeMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22Text%20with%20%5Cu00DC%5Cu00D6%5Cu6F22%5Cu8A9E%22", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/subscribe/demo-36/hello_my_channel/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559006802662768", "[[\"Text with \u00DC\u00D6\u6F22\u8A9E\"],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559014566792816", "[[\"Text with \u00DC\u00D6\u6F22\u8A9E\"],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel/leave", "{\"action\": \"leave\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenSubscribedToAChannelThenSubscribeShouldReturnUnicodeCipherMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            return data;
        }

        private Dictionary<string, string> LoadWhenSubscribedToAChannelThenSubscribeShouldReturnReceivedForwardSlashMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22Text%20with%20%2F%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/20eb1f490fd3682a417640ec5fdd0d1b/hello_my_channel/0/%22Text%20with%20%2F%22", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/subscribe/demo-36/hello_my_channel/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559006802662768", "[[\"Text with \\/\"],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559014566792816", "[[],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel/leave", "{\"action\": \"leave\"}");
            return data;
        }
        
        private Dictionary<string, string> LoadWhenSubscribedToAChannelThenSubscribeShouldReturnReceivedForwardSlashCipherMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22s98XlGoA68ypX1Z7A7mOwQ%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/487a1708fcddc1ff3ed45b2a43ee256b/hello_my_channel/0/%22s98XlGoA68ypX1Z7A7mOwQ%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/subscribe/demo-36/hello_my_channel/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559006802662768", "[[\"s98XlGoA68ypX1Z7A7mOwQ==\"],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559014566792816", "[[\"s98XlGoA68ypX1Z7A7mOwQ==\"],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel/leave", "{\"action\": \"leave\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenSubscribedToAChannelThenSubscribeShouldReturnSpecialCharMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22Text%20with%20%27%5C%22%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/45f266dc617d0e42619b8886f05015aa/hello_my_channel/0/%22Text%20with%20%27%5C%22%22", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/subscribe/demo-36/hello_my_channel/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559006802662768", "[[\"Text with '\\\"\"],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559014566792816", "[[\"Text with '\\\"\"],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel/leave", "{\"action\": \"leave\"}");
            return data;
        }
        
        private Dictionary<string, string> LoadWhenSubscribedToAChannelThenSubscribeShouldReturnSpecialCharCipherMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22kl7vmPUMMz6UdliN7t6XYw%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/d99eb7e70213acb4f839ea7a9653c29d/hello_my_channel/0/%22kl7vmPUMMz6UdliN7t6XYw%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/subscribe/demo-36/hello_my_channel/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559006802662768", "[[\"kl7vmPUMMz6UdliN7t6XYw==\"],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559014566792816", "[[\"kl7vmPUMMz6UdliN7t6XYw==\"],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel/leave", "{\"action\": \"leave\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenSubscribedToAChannelGroupThenSubscribeShouldReturnReceivedMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/channel-registration/sub-key/demo-36/channel-group/hello_my_group?add=hello_my_channel", "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22Test%20for%20WhenSubscribedToAChannelGroup%20ThenItShouldReturnReceivedMessage%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/subscribe/demo-36/,/0/0?uuid=myuuid&channel-group=hello_my_group", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/,/0/13559006802662768?uuid=myuuid&channel-group=hello_my_group", "[[\"Test for WhenSubscribedToAChannel ThenItShouldReturnReceivedMessage\"],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/,/0/13559014566792816?uuid=myuuid&channel-group=hello_my_group", "[[],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/,/leave?uuid=myuuid&channel-group=hello_my_group", "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenSubscribedToAChannelThenSubscribeShouldReturnConnectStatus()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22Test%20for%20WhenSubscribedToAChannel%20ThenItShouldReturnReceivedMessage%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559006802662768", "[[\"Test for WhenSubscribedToAChannel ThenItShouldReturnReceivedMessage\"],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559014566792816", "[[],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel/leave", "{\"action\": \"leave\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenSubscribedToAChannelGroupThenSubscribeShouldReturnConnectStatus()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/channel-registration/sub-key/demo-36/channel-group/hello_my_group?add=hello_my_channel", "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}");
            //data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22Test%20for%20WhenSubscribedToAChannelGroup%20ThenItShouldReturnReceivedMessage%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/subscribe/demo-36/,/0/0?uuid=myuuid&channel-group=hello_my_group", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/,/0/13559006802662768?uuid=myuuid&channel-group=hello_my_group", "[[\"Test for WhenSubscribedToAChannel ThenItShouldReturnReceivedMessage\"],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/,/0/13559014566792816?uuid=myuuid&channel-group=hello_my_group", "[[],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/,/leave?uuid=myuuid&channel-group=hello_my_group", "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenSubscribedToAChannelThenMultiSubscribeShouldReturnConnectStatus()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/subscribe/demo-36/hello_my_channel1/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel1/0/13559006802662768", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel1,hello_my_channel2/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel1,hello_my_channel2/0/13559006802662768", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel2,hello_my_channel1/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel2,hello_my_channel1/0/13559006802662768", "[[],\"13559006802662768\"]");
            return data;
        }

        private Dictionary<string, string> LoadWhenSubscribedToAChannelGroupThenMultiSubscribeShouldReturnConnectStatus()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/channel-registration/sub-key/demo-36/channel-group/hello_my_group1?add=hello_my_channel1", "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}");
            data.Add("/v1/channel-registration/sub-key/demo-36/channel-group/hello_my_group2?add=hello_my_channel2", "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}");
            //data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22Test%20for%20WhenSubscribedToAChannelGroup%20ThenItShouldReturnReceivedMessage%22", "[1,\"Sent\",\"13559014566792817\"]");
            
            data.Add("/subscribe/demo-36/,/0/0?uuid=myuuid&channel-group=hello_my_group1,hello_my_group2", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/,/0/13559006802662768?uuid=myuuid&channel-group=hello_my_group1,hello_my_group2", "[[\"Test for WhenSubscribedToAChannel ThenItShouldReturnReceivedMessage\"],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/,/0/13559014566792816?uuid=myuuid&channel-group=hello_my_group1,hello_my_group2", "[[],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/,/leave?uuid=myuuid&channel-group=hello_my_group1,hello_my_group2", "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}");

            data.Add("/subscribe/demo-36/,/0/0?uuid=myuuid&channel-group=hello_my_group2,hello_my_group1", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/,/0/13559006802662768?uuid=myuuid&channel-group=hello_my_group2,hello_my_group1", "[[\"Test for WhenSubscribedToAChannel ThenItShouldReturnReceivedMessage\"],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/,/0/13559014566792816?uuid=myuuid&channel-group=hello_my_group2,hello_my_group1", "[[],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/,/leave?uuid=myuuid&channel-group=hello_my_group2,hello_my_group1", "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenSubscribedToAChannelThenDuplicateChannelShouldReturnAlreadySubscribed()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/subscribe/demo-36/hello_my_channel/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559006802662768", "[[],\"13559006802662768\"]");
            return data;
        }

        private Dictionary<string, string> LoadWhenSubscribedToAChannelThenSubscriberShouldBeAbleToReceiveManyMessages()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/subscribe/demo-36/hello_my_channel/0/0", "[[],\"13602645380839594\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13602645380839594", "[[\"742730406\",\"1853970548\",\"1899616327\",\"1043229779\",\"1270838952\",\"788288787\",\"627599385\",\"1517373321\",\"1202317119\",\"184893837\"],\"13602645382888692\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13602645382888692", "[[],\"13602645382888692\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel/leave", "{\"action\": \"leave\"}");
            return data;
        }

        private Dictionary<string, string> WhenSubscribedToWildcardChannel_ChannelAndChannelGroupAndWildcardChannelSubscribeShouldReturnReceivedMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/channel-registration/sub-key/demo-36/channel-group/hello_my_group?add=hello_my_channel1", "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}");
            
            data.Add("/subscribe/demo-36/foo.%2A,hello_my_channel/0/0?uuid=myuuid&channel-group=hello_my_group", "[[],\"14339584751454774\",\"\",\"\"]");
            data.Add("/subscribe/demo-36/foo.%2A,hello_my_channel/0/14339584751454774?uuid=myuuid&channel-group=hello_my_group", "[[\"Test for cg\"],\"14339594143230275\",\"hello_my_group\",\"hello_my_channel1\"]");
            data.Add("/subscribe/demo-36/foo.%2A,hello_my_channel/0/14339594143230275?uuid=myuuid&channel-group=hello_my_group", "[[{\"action\": \"join\", \"timestamp\": 1433959414, \"uuid\": \"myuuid\", \"occupancy\": 1}],\"14339594149517538\",\"foo.*\",\"foo.*-pnpres\"]");
            data.Add("/subscribe/demo-36/foo.%2A,hello_my_channel/0/14339594149517538?uuid=myuuid&channel-group=hello_my_group", "[[\"Test for wc\"],\"14339594154592482\",\"foo.*\",\"foo.a\"]");
            data.Add("/subscribe/demo-36/foo.%2A,hello_my_channel/0/14339594154592482?uuid=myuuid&channel-group=hello_my_group", "[[\"Test for normal ch\"],\"14339594165903036\",\"hello_my_group\",\"hello_my_channel\"]");
            data.Add("/subscribe/demo-36/foo.%2A,hello_my_channel/0/14339594165903036?uuid=myuuid&channel-group=hello_my_group", "[[],\"14339594165903037\",\"\",\"\"]");
            data.Add("/subscribe/demo-36/foo.%2A,hello_my_channel/0/14339594165903037?uuid=myuuid&channel-group=hello_my_group", "[[],\"14339594165903037\",\"\",\"\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel,foo.%2A/leave?uuid=myuuid&channel-group=hello_my_group", "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}");

            data.Add("/subscribe/demo-36/hello_my_channel,foo.%2A/0/0?uuid=myuuid&channel-group=hello_my_group", "[[],\"14339584751454774\",\"\",\"\"]");
            data.Add("/subscribe/demo-36/hello_my_channel,foo.%2A/0/14339584751454774?uuid=myuuid&channel-group=hello_my_group", "[[\"Test for cg\"],\"14339594143230275\",\"hello_my_group\",\"hello_my_channel1\"]");
            data.Add("/subscribe/demo-36/hello_my_channel,foo.%2A/0/14339594143230275?uuid=myuuid&channel-group=hello_my_group", "[[{\"action\": \"join\", \"timestamp\": 1433959414, \"uuid\": \"myuuid\", \"occupancy\": 1}],\"14339594149517538\",\"foo.*\",\"foo.*-pnpres\"]");
            data.Add("/subscribe/demo-36/hello_my_channel,foo.%2A/0/14339594149517538?uuid=myuuid&channel-group=hello_my_group", "[[\"Test for wc\"],\"14339594154592482\",\"foo.*\",\"foo.a\"]");
            data.Add("/subscribe/demo-36/hello_my_channel,foo.%2A/0/14339594154592482?uuid=myuuid&channel-group=hello_my_group", "[[\"Test for normal ch\"],\"14339594165903036\",\"hello_my_group\",\"hello_my_channel\"]");
            data.Add("/subscribe/demo-36/hello_my_channel,foo.%2A/0/14339594165903036?uuid=myuuid&channel-group=hello_my_group", "[[],\"14339594165903037\",\"\",\"\"]");
            data.Add("/subscribe/demo-36/hello_my_channel,foo.%2A/0/14339594165903037?uuid=myuuid&channel-group=hello_my_group", "[[],\"14339594165903037\",\"\",\"\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/foo.%2A,hello_my_channel/leave?uuid=myuuid&channel-group=hello_my_group", "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}");

            data.Add("/publish/demo-36/demo-36/0/hello_my_channel1/0/%22Test%20for%20cg%22?uuid=myuuid", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/foo.a/0/%22Test%20for%20wc%22?uuid=myuuid", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22Test%20for%20normal%20ch%22?uuid=myuuid", "[1,\"Sent\",\"13559014566792817\"]");
            return data;
        }

        private Dictionary<string, string> WhenSubscribedToWildcardChannelThenSubscribeShouldReturnWildCardPresenceEventInWildcardPresenceCallback()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/subscribe/demo-36/foo.%2A/0/0", "[[{\"action\": \"join\", \"timestamp\": 1434009722, \"uuid\": \"myuuid\", \"occupancy\": 1}],\"14340097229800922\",\"foo.*\",\"foo.*-pnpres\"]");
            data.Add("/subscribe/demo-36/foo.%2A/0/14340097229800922", "[[],\"14340097229800922\"]");
            return data;
        }

        private Dictionary<string, string> LoadWhenSubscribedToWildcardChannelThenSubscribeShouldReturnReceivedMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/foo.bar/0/%22Test%20for%20WhenSubscribedToAChannel%20ThenItShouldReturnReceivedMessage%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/foo.bar/0/%22Zbr7pEF%2FGFGKj1rOstp0tWzA4nwJXEfj%2BezLtAr8qqE%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/d259936bf6578bfe1fbdce0108aa1d27/foo.bar/0/%22QoHwTga0QtOCtJRQ6sqtyateB%2FVotNt%2F50y23yXW7rpCbZdJLUAVKKbf01SpN6zghA6MqQaaHRXoYqAf84RF56C7Ky6Oi6jLqN2I5%2FlXSCw%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/191f2302ebeebdcbe473be21dd33e3f0/foo.bar/0/%22Test%20for%20WhenSubscribedToAChannel%20ThenItShouldReturnReceivedMessage%22", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/subscribe/demo-36/foo.%2A/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/foo.%2A/0/13559006802662768", "[[\"Test for WhenSubscribedToAChannel ThenItShouldReturnReceivedMessage\"],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/foo.%2A/0/13559014566792816", "[[],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/foo.%2A/leave", "{\"action\": \"leave\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenSubscribedToWildcardChannelThenSubscribeShouldReturnReceivedCipherMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/foo.bar/0/%22QoHwTga0QtOCtJRQ6sqtyateB%2FVotNt%2F50y23yXW7rpCbZdJLUAVKKbf01SpN6zghA6MqQaaHRXoYqAf84RF56C7Ky6Oi6jLqN2I5%2FlXSCw%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/d259936bf6578bfe1fbdce0108aa1d27/foo.bar/0/%22Zbr7pEF%2FGFGKj1rOstp0tWzA4nwJXEfj%2BezLtAr8qqE%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/d259936bf6578bfe1fbdce0108aa1d27/foo.bar/0/%22QoHwTga0QtOCtJRQ6sqtyateB%2FVotNt%2F50y23yXW7rpCbZdJLUAVKKbf01SpN6zghA6MqQaaHRXoYqAf84RF56C7Ky6Oi6jLqN2I5%2FlXSCw%3D%22", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/subscribe/demo-36/foo.%2A/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/foo.%2A/0/13559006802662768", "[[\"QoHwTga0QtOCtJRQ6sqtyateB/VotNt/50y23yXW7rpCbZdJLUAVKKbf01SpN6zghA6MqQaaHRXoYqAf84RF56C7Ky6Oi6jLqN2I5/lXSCw=\"],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/foo.%2A/0/13559014566792816", "[[],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/foo.%2A/leave", "{\"action\": \"leave\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenSubscribedToWildcardChannelThenSubscribeShouldReturnReceivedComplexMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/foo.bar/0/%7B%22foo%22%3A%22hi%21%22%2C%22bar%22%3A%5B1%2C2%2C3%2C4%2C5%5D%7D", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/a50a618029da59507f80118062de229a/foo.bar/0/%7B%22foo%22%3A%22hi%21%22%2C%22bar%22%3A%5B1%2C2%2C3%2C4%2C5%5D%7D", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/d4a031148880d7c3d96251f02586726c/foo.bar/0/%22Zbr7pEF%2FGFGKj1rOstp0tWzA4nwJXEfj%2BezLtAr8qqE%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/foo.bar/0/%22Zbr7pEF%2FGFGKj1rOstp0tWzA4nwJXEfj%2BezLtAr8qqE%3D%22", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/subscribe/demo-36/foo.%2A/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/foo.%2A/0/13559006802662768", "[[{\"foo\":\"hi!\",\"bar\":[1,2,3,4,5]}],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/foo.%2A/0/13559014566792816", "[[],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/foo.%2A/leave", "{\"action\": \"leave\"}");
            return data;
        }
        private Dictionary<string, string> LoadWhenSubscribedToWildcardChannelThenSubscribeShouldReturnReceivedCipherComplexMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/foo.bar/0/%7B%22foo%22%3A%22hi%21%22%2C%22bar%22%3A%5B1%2C2%2C3%2C4%2C5%5D%7D", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/a50a618029da59507f80118062de229a/foo.bar/0/%7B%22foo%22%3A%22hi%21%22%2C%22bar%22%3A%5B1%2C2%2C3%2C4%2C5%5D%7D", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/d4a031148880d7c3d96251f02586726c/foo.bar/0/%22Zbr7pEF%2FGFGKj1rOstp0tWzA4nwJXEfj%2BezLtAr8qqE%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/foo.bar/0/%22Zbr7pEF%2FGFGKj1rOstp0tWzA4nwJXEfj%2BezLtAr8qqE%3D%22", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/subscribe/demo-36/foo.%2A/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/foo.%2A/0/13559006802662768", "[[\"Zbr7pEF/GFGKj1rOstp0tWzA4nwJXEfj+ezLtAr8qqE=\"],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/foo.%2A/0/13559014566792816", "[[],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/foo.%2A/leave", "{\"action\": \"leave\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenSubscribedToWildcardChannelThenSubscribeShouldReturnReceivedEmojiMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/foo.bar/0/%22Text%20with%20%F0%9F%98%9C%20emoji%20%F0%9F%8E%89.%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/a39655eba4d795924da2a3192b5b3e3f/foo.bar/0/%22Text%20with%20%F0%9F%98%9C%20emoji%20%F0%9F%8E%89.%22", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/subscribe/demo-36/foo.%2A/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/foo.%2A/0/13559006802662768", "[[\"Text with \ud83d\ude1c emoji \ud83c\udf89.\"],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/foo.%2A/0/13559014566792816", "[[\"Text with \ud83d\ude1c emoji \ud83c\udf89.\"],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/foo.%2A/leave", "{\"action\": \"leave\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenSubscribedToWildcardChannelThenSubscribeShouldReturnReceivedCipherEmojiMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/1ed2b4d04cb379f5b9930ec87f211f3f/foo.bar/0/%22vaD98V5XDtEvByw6RrxT9Ya76GKQLhyrEZw9Otrsu1KBVDIqGgWkrAD8X6TM%2FXC6%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/0/foo.bar/0/%22Zbr7pEF%2FGFGKj1rOstp0tWzA4nwJXEfj%2BezLtAr8qqE%3D%22", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/subscribe/demo-36/foo.%2A/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/foo.%2A/0/13559006802662768", "[[\"vaD98V5XDtEvByw6RrxT9Ya76GKQLhyrEZw9Otrsu1KBVDIqGgWkrAD8X6TM/XC6\"],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/foo.%2A/0/13559014566792816", "[[\"vaD98V5XDtEvByw6RrxT9Ya76GKQLhyrEZw9Otrsu1KBVDIqGgWkrAD8X6TM/XC6\"],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/foo.%2A/leave", "{\"action\": \"leave\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenSubscribedToWildcardChannelThenSubscribeShouldReturnUnicodeMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/foo.bar/0/%22Text%20with%20%5Cu00DC%5Cu00D6%5Cu6F22%5Cu8A9E%22", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/subscribe/demo-36/foo.%2A/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/foo.%2A/0/13559006802662768", "[[\"Text with \u00DC\u00D6\u6F22\u8A9E\"],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/foo.%2A/0/13559014566792816", "[[\"Text with \u00DC\u00D6\u6F22\u8A9E\"],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/foo.%2A/leave", "{\"action\": \"leave\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenSubscribedToWildcardChannelThenSubscribeShouldReturnUnicodeCipherMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            return data;
        }

        private Dictionary<string, string> LoadWhenSubscribedToWildcardChannelThenSubscribeShouldReturnReceivedForwardSlashMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/foo.bar/0/%22Text%20with%20%2F%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/20eb1f490fd3682a417640ec5fdd0d1b/foo.bar/0/%22Text%20with%20%2F%22", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/subscribe/demo-36/foo.%2A/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/foo.%2A/0/13559006802662768", "[[\"Text with \\/\"],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/foo.%2A/0/13559014566792816", "[[],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/foo.%2A/leave", "{\"action\": \"leave\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenSubscribedToWildcardChannelThenSubscribeShouldReturnReceivedForwardSlashCipherMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/foo.bar/0/%22s98XlGoA68ypX1Z7A7mOwQ%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/487a1708fcddc1ff3ed45b2a43ee256b/foo.bar/0/%22s98XlGoA68ypX1Z7A7mOwQ%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/subscribe/demo-36/foo.%2A/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/foo.%2A/0/13559006802662768", "[[\"s98XlGoA68ypX1Z7A7mOwQ==\"],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/foo.%2A/0/13559014566792816", "[[\"s98XlGoA68ypX1Z7A7mOwQ==\"],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/foo.%2A/leave", "{\"action\": \"leave\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenSubscribedToWildcardChannelThenSubscribeShouldReturnSpecialCharMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/foo.bar/0/%22Text%20with%20%27%5C%22%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/45f266dc617d0e42619b8886f05015aa/foo.bar/0/%22Text%20with%20%27%5C%22%22", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/subscribe/demo-36/foo.%2A/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/foo.%2A/0/13559006802662768", "[[\"Text with '\\\"\"],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/foo.%2A/0/13559014566792816", "[[\"Text with '\\\"\"],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/foo.%2A/leave", "{\"action\": \"leave\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenSubscribedToWildcardChannelThenSubscribeShouldReturnSpecialCharCipherMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/foo.bar/0/%22kl7vmPUMMz6UdliN7t6XYw%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/publish/demo-36/demo-36/d99eb7e70213acb4f839ea7a9653c29d/foo.bar/0/%22kl7vmPUMMz6UdliN7t6XYw%3D%3D%22", "[1,\"Sent\",\"13559014566792817\"]");

            data.Add("/subscribe/demo-36/foo.%2A/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/foo.%2A/0/13559006802662768", "[[\"kl7vmPUMMz6UdliN7t6XYw==\"],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/foo.%2A/0/13559014566792816", "[[\"kl7vmPUMMz6UdliN7t6XYw==\"],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/foo.%2A/leave", "{\"action\": \"leave\"}");
            return data;
        }


        private Dictionary<string, string> LoadWhenUnsubscribedToAChannelThenShouldReturnUnsubscribedMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/subscribe/demo-36/hello_my_channel/0/0", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/hello_my_channel/0/13559006802662768", "[[],\"13559006802662768\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/hello_my_channel/leave", "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenUnsubscribedToAChannelGroupThenShouldReturnUnsubscribedMessage()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/channel-registration/sub-key/demo-36/channel-group/hello_my_group?add=hello_my_channel", "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}");
            //data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%22Test%20for%20WhenSubscribedToAChannelGroup%20ThenItShouldReturnReceivedMessage%22", "[1,\"Sent\",\"13559014566792817\"]");
            data.Add("/subscribe/demo-36/,/0/0?uuid=myuuid&channel-group=hello_my_group", "[[],\"13559006802662768\"]");
            data.Add("/subscribe/demo-36/,/0/13559006802662768?uuid=myuuid&channel-group=hello_my_group", "[[\"Test for WhenSubscribedToAChannel ThenItShouldReturnReceivedMessage\"],\"13559014566792816\"]");
            data.Add("/subscribe/demo-36/,/0/13559014566792816?uuid=myuuid&channel-group=hello_my_group", "[[],\"13559014566792816\"]");
            data.Add("/v2/presence/sub_key/demo-36/channel/,/leave?uuid=myuuid&channel-group=hello_my_group", "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenAuditIsRequestedThenSubKeyLevelShouldReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/auth/audit/sub-key/sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"channels\":{},\"subscribe_key\":\"sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe\",\"level\":\"subkey\"},\"service\":\"Access Manager\"}");
            data.Add("/v1/auth/audit/sub-key/demo-36", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"channels\":{},\"subscribe_key\":\"demo-36\",\"level\":\"subkey\"},\"service\":\"Access Manager\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenAuditIsRequestedThenChannelLevelShouldReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/auth/audit/sub-key/sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"channels\":{},\"subscribe_key\":\"sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe\",\"level\":\"channel\"},\"service\":\"Access Manager\"}");
            data.Add("/v1/auth/audit/sub-key/demo-36", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"channels\":{},\"subscribe_key\":\"demo-36\",\"level\":\"channel\"},\"service\":\"Access Manager\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenAuditIsRequestedThenChannelGroupLevelShouldReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/auth/audit/sub-key/sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe", "{\"status\":200,\"service\":\"Access Manager\",\"message\":\"Success\",\"payload\":{\"channel-groups\":{},\"subscribe_key\":\"sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe\",\"level\":\"channel-group\"}}");
            data.Add("/v1/auth/audit/sub-key/demo-36", "{\"status\":200,\"service\":\"Access Manager\",\"message\":\"Success\",\"payload\":{\"channel-groups\":{},\"subscribe_key\":\"demo-36\",\"level\":\"channel-group\"}}");
            return data;
        }

        private Dictionary<string, string> LoadWhenGrantIsRequestedThenSubKeyLevelWithReadWriteShouldReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/auth/grant/sub-key/sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"subscribe_key\":\"sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe\",\"r\":1,\"ttl\":5,\"w\":1,\"level\":\"subkey\"},\"service\":\"Access Manager\"}");
            data.Add("/v1/auth/grant/sub-key/demo-36", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"subscribe_key\":\"demo-36\",\"r\":1,\"ttl\":5,\"w\":1,\"level\":\"subkey\"},\"service\":\"Access Manager\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenGrantIsRequestedThenSubKeyLevelWithReadShouldReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/auth/grant/sub-key/sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"subscribe_key\":\"sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe\",\"r\":1,\"ttl\":5,\"w\":0,\"level\":\"subkey\"},\"service\":\"Access Manager\"}");
            data.Add("/v1/auth/grant/sub-key/demo-36", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"subscribe_key\":\"demo-36\",\"r\":1,\"ttl\":5,\"w\":0,\"level\":\"subkey\"},\"service\":\"Access Manager\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenGrantIsRequestedThenSubKeyLevelWithWriteShouldReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/auth/grant/sub-key/sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"subscribe_key\":\"sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe\",\"r\":0,\"ttl\":5,\"w\":1,\"level\":\"subkey\"},\"service\":\"Access Manager\"}");
            data.Add("/v1/auth/grant/sub-key/demo-36", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"subscribe_key\":\"demo-36\",\"r\":0,\"ttl\":5,\"w\":1,\"level\":\"subkey\"},\"service\":\"Access Manager\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenGrantIsRequestedThenChannelLevelWithReadWriteShouldReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/auth/grant/sub-key/sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"channels\":{\"hello_my_channel\":{\"r\":1,\"w\":1}},\"subscribe_key\":\"sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe\",\"level\":\"channel\"},\"service\":\"Access Manager\"}");
            data.Add("/v1/auth/grant/sub-key/demo-36", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"channels\":{\"hello_my_channel\":{\"r\":1,\"w\":1}},\"subscribe_key\":\"demo-36\",\"level\":\"channel\"},\"service\":\"Access Manager\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenGrantIsRequestedThenChannelLevelWithReadShouldReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/auth/grant/sub-key/sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"channels\":{\"hello_my_channel\":{\"r\":1,\"w\":0}},\"subscribe_key\":\"sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe\",\"level\":\"channel\"},\"service\":\"Access Manager\"}");
            data.Add("/v1/auth/grant/sub-key/demo-36", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"channels\":{\"hello_my_channel\":{\"r\":1,\"w\":0}},\"subscribe_key\":\"demo-36\",\"level\":\"channel\"},\"service\":\"Access Manager\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenGrantIsRequestedThenChannelLevelWithWriteShouldReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/auth/grant/sub-key/sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"channels\":{\"hello_my_channel\":{\"r\":0,\"w\":1}},\"subscribe_key\":\"sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe\",\"level\":\"channel\"},\"service\":\"Access Manager\"}");
            data.Add("/v1/auth/grant/sub-key/demo-36", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"channels\":{\"hello_my_channel\":{\"r\":0,\"w\":1}},\"subscribe_key\":\"demo-36\",\"level\":\"channel\"},\"service\":\"Access Manager\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenGrantIsRequestedThenUserLevelWithReadWriteShouldReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/auth/grant/sub-key/sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"ttl\":5,\"auths\":{\"hello_my_authkey\":{\"r\":1,\"w\":1}},\"subscribe_key\":\"sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe\",\"level\":\"user\",\"channel\":\"hello_my_authchannel\"},\"service\":\"Access Manager\"}");
            data.Add("/v1/auth/grant/sub-key/demo-36", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"ttl\":5,\"auths\":{\"hello_my_authkey\":{\"r\":1,\"w\":1}},\"subscribe_key\":\"demo-36\",\"level\":\"user\",\"channel\":\"hello_my_authchannel\"},\"service\":\"Access Manager\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenGrantIsRequestedThenUserLevelWithReadShouldReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/auth/grant/sub-key/sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"ttl\":5,\"auths\":{\"hello_my_authkey\":{\"r\":1,\"w\":0}},\"subscribe_key\":\"sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe\",\"level\":\"user\",\"channel\":\"hello_my_authchannel\"},\"service\":\"Access Manager\"}");
            data.Add("/v1/auth/grant/sub-key/demo-36", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"ttl\":5,\"auths\":{\"hello_my_authkey\":{\"r\":1,\"w\":0}},\"subscribe_key\":\"demo-36\",\"level\":\"user\",\"channel\":\"hello_my_authchannel\"},\"service\":\"Access Manager\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenGrantIsRequestedThenUserLevelWithWriteShouldReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/auth/grant/sub-key/sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"ttl\":5,\"auths\":{\"hello_my_authkey\":{\"r\":0,\"w\":1}},\"subscribe_key\":\"sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe\",\"level\":\"user\",\"channel\":\"hello_my_authchannel\"},\"service\":\"Access Manager\"}");
            data.Add("/v1/auth/grant/sub-key/demo-36", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"ttl\":5,\"auths\":{\"hello_my_authkey\":{\"r\":0,\"w\":1}},\"subscribe_key\":\"demo-36\",\"level\":\"user\",\"channel\":\"hello_my_authchannel\"},\"service\":\"Access Manager\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenGrantIsRequestedThenMultipleChannelGrantShouldReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/auth/grant/sub-key/sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"channels\":{\"csharp-hello_my_channel-4\":{\"r\":1,\"w\":1},\"csharp-hello_my_channel-1\":{\"r\":1,\"w\":1},\"csharp-hello_my_channel-0\":{\"r\":1,\"w\":1},\"csharp-hello_my_channel-3\":{\"r\":1,\"w\":1},\"csharp-hello_my_channel-2\":{\"r\":1,\"w\":1}},\"subscribe_key\":\"sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe\",\"ttl\":5,\"level\":\"channel\"},\"service\":\"Access Manager\"}");
            data.Add("/v1/auth/grant/sub-key/demo-36", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"channels\":{\"csharp-hello_my_channel-4\":{\"r\":1,\"w\":1},\"csharp-hello_my_channel-1\":{\"r\":1,\"w\":1},\"csharp-hello_my_channel-0\":{\"r\":1,\"w\":1},\"csharp-hello_my_channel-3\":{\"r\":1,\"w\":1},\"csharp-hello_my_channel-2\":{\"r\":1,\"w\":1}},\"subscribe_key\":\"demo-36\",\"ttl\":5,\"level\":\"channel\"},\"service\":\"Access Manager\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenGrantIsRequestedThenMultipleAuthGrantShouldReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/auth/grant/sub-key/sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"ttl\":5,\"auths\":{\"csharp-auth_key-2\":{\"r\":1,\"w\":1},\"csharp-auth_key-3\":{\"r\":1,\"w\":1},\"csharp-auth_key-0\":{\"r\":1,\"w\":1},\"csharp-auth_key-1\":{\"r\":1,\"w\":1},\"csharp-auth_key-4\":{\"r\":1,\"w\":1}},\"subscribe_key\":\"sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe\",\"level\":\"user\",\"channel\":\"hello_my_channel\"},\"service\":\"Access Manager\"}");
            data.Add("/v1/auth/grant/sub-key/demo-36", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"ttl\":5,\"auths\":{\"csharp-auth_key-2\":{\"r\":1,\"w\":1},\"csharp-auth_key-3\":{\"r\":1,\"w\":1},\"csharp-auth_key-0\":{\"r\":1,\"w\":1},\"csharp-auth_key-1\":{\"r\":1,\"w\":1},\"csharp-auth_key-4\":{\"r\":1,\"w\":1}},\"subscribe_key\":\"demo-36\",\"level\":\"user\",\"channel\":\"hello_my_channel\"},\"service\":\"Access Manager\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenGrantIsRequestedThenRevokeAtSubKeyLevelReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/auth/grant/sub-key/sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"subscribe_key\":\"sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe\",\"r\":0,\"ttl\":5,\"w\":0,\"level\":\"subkey\"},\"service\":\"Access Manager\"}");
            data.Add("/v1/auth/grant/sub-key/demo-36", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"subscribe_key\":\"demo-36\",\"r\":0,\"ttl\":5,\"w\":0,\"level\":\"subkey\"},\"service\":\"Access Manager\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenGrantIsRequestedThenRevokeAtChannelLevelReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/auth/grant/sub-key/sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"channels\":{},\"subscribe_key\":\"sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe\",\"level\":\"channel\"},\"service\":\"Access Manager\"}");
            data.Add("/v1/auth/grant/sub-key/demo-36", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"channels\":{},\"subscribe_key\":\"demo-36\",\"level\":\"channel\"},\"service\":\"Access Manager\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenGrantIsRequestedThenRevokeAtUserLevelReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/auth/grant/sub-key/sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"ttl\":5,\"auths\":{},\"subscribe_key\":\"sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe\",\"level\":\"user\",\"channel\":\"hello_my_authchannel\"},\"service\":\"Access Manager\"}");
            data.Add("/v1/auth/grant/sub-key/demo-36", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"ttl\":5,\"auths\":{},\"subscribe_key\":\"demo-36\",\"level\":\"user\",\"channel\":\"hello_my_authchannel\"},\"service\":\"Access Manager\"}");
            return data;
        }

        private Dictionary<string, string> LoadWhenGrantIsRequestedThenChannelGroupLevelWithReadManageShouldReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/auth/grant/sub-key/sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe", "{\"status\":200,\"service\":\"Access Manager\",\"message\":\"Success\",\"payload\":{\"channel-groups\":{\"hello_my_group\":{\"r\":1,\"m\":1,\"w\":0}},\"subscribe_key\":\"sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe\",\"ttl\":5,\"level\":\"channel-group\"}}");
            data.Add("/v1/auth/grant/sub-key/demo-36", "{\"status\":200,\"service\":\"Access Manager\",\"message\":\"Success\",\"payload\":{\"channel-groups\":{\"hello_my_group\":{\"r\":1,\"m\":1,\"w\":0}},\"subscribe_key\":\"demo-36\",\"ttl\":5,\"level\":\"channel-group\"}}");
            return data;
        }

        private Dictionary<string, string> LoadWhenGrantIsRequestedThenChannelGroupLevelWithReadShouldReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/auth/grant/sub-key/sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe", "{\"status\":200,\"service\":\"Access Manager\",\"message\":\"Success\",\"payload\":{\"channel-groups\":{\"hello_my_group\":{\"r\":1,\"m\":0,\"w\":0}},\"subscribe_key\":\"sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe\",\"ttl\":5,\"level\":\"channel-group\"}}");
            data.Add("/v1/auth/grant/sub-key/demo-36", "{\"status\":200,\"service\":\"Access Manager\",\"message\":\"Success\",\"payload\":{\"channel-groups\":{\"hello_my_group\":{\"r\":1,\"m\":0,\"w\":0}},\"subscribe_key\":\"demo-36\",\"ttl\":5,\"level\":\"channel-group\"}}");
            return data;
        }

        private Dictionary<string, string> LoadGrantRequestUnitTestInit()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/auth/grant/sub-key/sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"channels\":{\"hello_my_channel\":{\"r\":1,\"w\":1}},\"subscribe_key\":\"sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe\",\"level\":\"channel\"},\"service\":\"Access Manager\"}");
            data.Add("/v1/auth/grant/sub-key/demo-36", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"channels\":{\"hello_my_channel\":{\"r\":1,\"w\":1}},\"subscribe_key\":\"demo-36\",\"level\":\"channel\"},\"service\":\"Access Manager\"}");
            return data;
        }

        private Dictionary<string, string> LoadGrantRequestUnitTestInit2()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/auth/grant/sub-key/sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"channels\":{\"hello_my_channel\":{\"r\":1,\"w\":1},\"hello_my_channel-pnpres\":{\"r\":1,\"w\":1}},\"subscribe_key\":\"sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe\",\"level\":\"channel\"},\"service\":\"Access Manager\"}");
            data.Add("/v1/auth/grant/sub-key/demo-36", "{\"status\":200,\"message\":\"Success\",\"payload\":{\"channels\":{\"hello_my_channel\":{\"r\":1,\"w\":1},\"hello_my_channel-pnpres\":{\"r\":1,\"w\":1}},\"subscribe_key\":\"demo-36\",\"level\":\"channel\"},\"service\":\"Access Manager\"}");
            return data;
        }

        private Dictionary<string, string> LoadGrantRequestUnitTestInit3()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/auth/grant/sub-key/sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe", "{\"status\":200,\"service\":\"Access Manager\",\"message\":\"Success\",\"payload\":{\"channel-groups\":{\"hello_my_group\":{\"r\":1,\"m\":1,\"w\":0}},\"subscribe_key\":\"sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe\",\"ttl\":20,\"level\":\"channel-group\"}}");
            data.Add("/v1/auth/grant/sub-key/demo-36", "{\"status\":200,\"service\":\"Access Manager\",\"message\":\"Success\",\"payload\":{\"channel-groups\":{\"hello_my_group\":{\"r\":1,\"m\":1,\"w\":0}},\"subscribe_key\":\"demo-36\",\"ttl\":20,\"level\":\"channel-group\"}}");
            return data;
        }

        private Dictionary<string, string> LoadWhenPushIsRequestedThenRegisterDeviceShouldReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/push/sub-key/demo-36/devices/http%3A%252F%252Fsn1.notify.live.net%252Fthrottledthirdparty%252F01.00%252FAQG2MdvoLlZFT7-VJ2TJ5LnbAgAAAAADAQAAAAQUZm52OkRFNzg2NTMxMzlFMEZFNkMFBlVTU0MwMQ", "[1,\"Modified Channels\",\"hello_my_channel\"]");
            return data;
        }

        private Dictionary<string, string> LoadWhenPushIsRequestedThenUnregisterDeviceShouldReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/push/sub-key/demo-36/devices/http%3A%252F%252Fsn1.notify.live.net%252Fthrottledthirdparty%252F01.00%252FAQG2MdvoLlZFT7-VJ2TJ5LnbAgAAAAADAQAAAAQUZm52OkRFNzg2NTMxMzlFMEZFNkMFBlVTU0MwMQ/remove", "[1,\"Removed Device\"]");
            return data;
        }

        private Dictionary<string, string> LoadWhenPushIsRequestedThenRemoveChannelForDeviceShouldReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/push/sub-key/demo-36/devices/http%3A%252F%252Fsn1.notify.live.net%252Fthrottledthirdparty%252F01.00%252FAQG2MdvoLlZFT7-VJ2TJ5LnbAgAAAAADAQAAAAQUZm52OkRFNzg2NTMxMzlFMEZFNkMFBlVTU0MwMQ", "[1,\"Modified Channels\"]");
            return data;
        }

        private Dictionary<string, string> LoadWhenPushIsRequestedThenGetAllChannelsForDeviceShouldReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/push/sub-key/demo-36/devices/http%3A%252F%252Fsn1.notify.live.net%252Fthrottledthirdparty%252F01.00%252FAQG2MdvoLlZFT7-VJ2TJ5LnbAgAAAAADAQAAAAQUZm52OkRFNzg2NTMxMzlFMEZFNkMFBlVTU0MwMQ", "[\"hello_my_channel\",\"\"]");
            return data;
        }

        private Dictionary<string, string> LoadWhenPushIsRequestedThenPublishMpnsToastShouldReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%7B%22pn_mpns%22%3A%7B%22type%22%3A%22toast%22%2C%22text1%22%3A%22hardcode%20message%22%2C%22text2%22%3A%22%22%2C%22param%22%3A%22%22%7D%2C%22pn_debug%22%3Atrue%7D", "[1,\"Sent\",\"13559014566792817\"]");
            return data;
        }

        private Dictionary<string, string> LoadWhenPushIsRequestedThenPublishMpnsFlipTileShouldReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
#if (SILVERLIGHT || WINDOWS_PHONE)
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%7B%22pn_mpns%22%3A%7B%22type%22%3A%22flip%22%2C%22delay%22%3A0%2C%22title%22%3A%22front%20title%22%2C%22count%22%3A6%2C%22small_background_image%22%3A%22%22%2C%22background_image%22%3A%22http%3A//cdn.flaticon.com/png/256/37985.png%22%2C%22back_background_image%22%3A%22Assets/Tiles/pubnub3.png%22%2C%22back_content%22%3A%22back%20message%22%2C%22back_title%22%3A%22back%20title%22%2C%22wide_background_image%22%3A%22%22%2C%22wide_back_background_image%22%3A%22%22%2C%22wide_back_content%22%3A%22%22%7D%2C%22pn_debug%22%3Atrue%7D", "[1,\"Sent\",\"13559014566792817\"]");
#else
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%7B%22pn_mpns%22%3A%7B%22type%22%3A%22flip%22%2C%22delay%22%3A0%2C%22title%22%3A%22front%20title%22%2C%22count%22%3A6%2C%22small_background_image%22%3A%22%22%2C%22background_image%22%3A%22http%3A%2F%2Fcdn.flaticon.com%2Fpng%2F256%2F37985.png%22%2C%22back_background_image%22%3A%22Assets%2FTiles%2Fpubnub3.png%22%2C%22back_content%22%3A%22back%20message%22%2C%22back_title%22%3A%22back%20title%22%2C%22wide_background_image%22%3A%22%22%2C%22wide_back_background_image%22%3A%22%22%2C%22wide_back_content%22%3A%22%22%7D%2C%22pn_debug%22%3Atrue%7D", "[1,\"Sent\",\"13559014566792817\"]");
#endif
            return data;
        }

        private Dictionary<string, string> LoadWhenPushIsRequestedThenPublishMpnsCycleTileShouldReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
#if (SILVERLIGHT || WINDOWS_PHONE)
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%7B%22pn_mpns%22%3A%7B%22type%22%3A%22cycle%22%2C%22delay%22%3A0%2C%22title%22%3A%22front%20title%22%2C%22count%22%3A2%2C%22small_background_image%22%3A%22%22%2C%22images%22%3A%5B%22Assets/Tiles/pubnub1.png%22%2C%22Assets/Tiles/pubnub2.png%22%2C%22Assets/Tiles/pubnub3.png%22%2C%22Assets/Tiles/pubnub4.png%22%5D%7D%2C%22pn_debug%22%3Atrue%7D", "[1,\"Sent\",\"13559014566792817\"]");
#else
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%7B%22pn_mpns%22%3A%7B%22type%22%3A%22cycle%22%2C%22delay%22%3A0%2C%22title%22%3A%22front%20title%22%2C%22count%22%3A2%2C%22small_background_image%22%3A%22%22%2C%22images%22%3A%5B%22Assets%2FTiles%2Fpubnub1.png%22%2C%22Assets%2FTiles%2Fpubnub2.png%22%2C%22Assets%2FTiles%2Fpubnub3.png%22%2C%22Assets%2FTiles%2Fpubnub4.png%22%5D%7D%2C%22pn_debug%22%3Atrue%7D", "[1,\"Sent\",\"13559014566792817\"]");
#endif
            return data;
        }

        private Dictionary<string, string> LoadWhenPushIsRequestedThenPublishMpnsIconicTileShouldReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
#if (SILVERLIGHT || WINDOWS_PHONE)
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%7B%22pn_mpns%22%3A%7B%22type%22%3A%22iconic%22%2C%22delay%22%3A0%2C%22title%22%3A%22front%20title%22%2C%22count%22%3A2%2C%22icon_image%22%3A%22%22%2C%22small_icon_image%22%3A%22%22%2C%22background_color%22%3A%22%22%2C%22wide_content_1%22%3A%22my%20wide%20content%22%2C%22wide_content_2%22%3A%22%22%2C%22wide_content_3%22%3A%22%22%7D%2C%22pn_debug%22%3Atrue%7D", "[1,\"Sent\",\"13559014566792817\"]");
#else
            data.Add("/publish/demo-36/demo-36/0/hello_my_channel/0/%7B%22pn_mpns%22%3A%7B%22type%22%3A%22iconic%22%2C%22delay%22%3A0%2C%22title%22%3A%22front%20title%22%2C%22count%22%3A2%2C%22icon_image%22%3A%22%22%2C%22small_icon_image%22%3A%22%22%2C%22background_color%22%3A%22%22%2C%22wide_content_1%22%3A%22my%20wide%20content%22%2C%22wide_content_2%22%3A%22%22%2C%22wide_content_3%22%3A%22%22%7D%2C%22pn_debug%22%3Atrue%7D", "[1,\"Sent\",\"13559014566792817\"]");
#endif
            return data;
        }

        private Dictionary<string, string> LoadWhenChannelGroupIsRequestedThenAddChannelShouldReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/channel-registration/sub-key/demo-36/channel-group/hello_my_group?add=hello_my_channel", "{\"status\":200,\"message\":\"OK\",\"service\":\"channel-registry\",\"error\":false}");
            return data;
        }

        private Dictionary<string, string> LoadWhenChannelGroupIsRequestedThenRemoveChannelShouldReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/channel-registration/sub-key/demo-36/channel-group/hello_my_group?remove=hello_my_channel", "{\"status\":200,\"message\":\"OK\",\"service\":\"channel-registry\",\"error\":false}");
            return data;
        }

        private Dictionary<string, string> LoadWhenChannelGroupIsRequestedThenGetChannelListShouldReturnSuccess()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("/v1/channel-registration/sub-key/demo-36/channel-group/hello_my_group", "{\"status\": 200, \"payload\": {\"channels\": [\"hello_my_channel\"], \"group\": \"hello_my_group\"}, \"service\": \"channel-registry\", \"error\": false}");
            return data;
        }

        public string GetStubResponse(HttpWebRequest request)
        {
            Uri requestUri = request.RequestUri;

            Dictionary<string, string> responseDictionary = null;
            string stubResponse = "!! Stub Response Not Assigned !!";
            System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}", _testClassName, _testCaseName));
            switch (_testClassName)
            {
                case "GrantRequestUnitTest":
                    switch (_testCaseName)
                    {
                        case "Init":
                            responseDictionary = LoadGrantRequestUnitTestInit();
                            break;
                        case "Init2":
                            responseDictionary = LoadGrantRequestUnitTestInit2();
                            break;
                        case "Init3":
                            responseDictionary = LoadGrantRequestUnitTestInit3();
                            break;
                        default:
                            break;
                    }
                    break;
                case "WhenAClientIsPresented":
                    switch (_testCaseName)
                    {
                        case "ThenPresenceShouldReturnReceivedMessage":
                            responseDictionary = LoadWhenAClientIsPresentedThenPresenceShouldReturnReceivedMessage();
                            break;
                        case "ThenPresenceShouldReturnCustomUUID":
                            responseDictionary = LoadWhenAClientIsPresentedThenPresenceShouldReturnCustomUUID();
                            break;
                        case "IfHereNowIsCalledThenItShouldReturnInfo":
                            responseDictionary = LoadWhenAClientIsPresentedIfHereNowIsCalledThenItShouldReturnInfo();
                            break;
                        case "IfGlobalHereNowIsCalledThenItShouldReturnInfo":
                            responseDictionary = LoadWhenAClientIsPresentedIfGlobalHereNowIsCalledThenItShouldReturnInfo();
                            break;
                        case "IfWhereNowIsCalledThenItShouldReturnInfo":
                            responseDictionary = LoadWhenAClientIsPresentedIfWhereNowIsCalledThenItShouldReturnInfo();
                            break;
                        case "IfHereNowIsCalledThenItShouldReturnInfoWithUserState":
                            responseDictionary = LoadWhenAClientIsPresentedIfHereNowIsCalledThenItShouldReturnInfoWithUserState();
                            break;
                        case "IfGlobalHereNowIsCalledThenItShouldReturnInfoWithUserState":
                            responseDictionary = LoadWhenAClientIsPresentedIfGlobalHereNowIsCalledThenItShouldReturnInfoWithUserState();
                            break;
                        case "IfSetAndGetUserStateThenItShouldReturnInfo":
                            responseDictionary = LoadWhenAClientIsPresentedIfSetAndGetUserStateThenItShouldReturnInfo();
                            break;
                        case "IfSetAndDeleteUserStateThenItShouldReturnInfo":
                            responseDictionary = LoadWhenAClientIsPresentedIfSetAndDeleteUserStateThenItShouldReturnInfo();
                            break;
                        default:
                            break;
                    }
                    break;
                case "WhenAMessageIsPublished":
                    switch (_testCaseName)
                    {
                        case "ThenUnencryptPublishShouldReturnSuccessCodeAndInfo":
                            responseDictionary = LoadWhenAMessageIsPublishedThenUnencryptPublishShouldReturnSuccessCodeAndInfo();
                            break;
                        case "ThenUnencryptObjectPublishShouldReturnSuccessCodeAndInfo":
                            responseDictionary = LoadWhenAMessageIsPublishedThenUnencryptObjectPublishShouldReturnSuccessCodeAndInfo();
                            break;
                        case "ThenEncryptObjectPublishShouldReturnSuccessCodeAndInfo":
                            responseDictionary = LoadWhenAMessageIsPublishedThenEncryptObjectPublishShouldReturnSuccessCodeAndInfo();
                            break;
                        case "ThenEncryptPublishShouldReturnSuccessCodeAndInfo":
                            responseDictionary = LoadWhenAMessageIsPublishedThenEncryptPublishShouldReturnSuccessCodeAndInfo();
                            break;
                        case "ThenSecretKeyWithEncryptPublishShouldReturnSuccessCodeAndInfo":
                            responseDictionary = LoadWhenAMessageIsPublishedThenSecretKeyWithEncryptPublishShouldReturnSuccessCodeAndInfo();
                            break;
                        case "ThenComplexMessageObjectShouldReturnSuccessCodeAndInfo":
                            responseDictionary = LoadWhenAMessageIsPublishedThenComplexMessageObjectShouldReturnSuccessCodeAndInfo();
                            break;
                        case "ThenOptionalSecretKeyShouldBeProvidedInConstructor":
                            responseDictionary = LoadWhenAMessageIsPublishedThenOptionalSecretKeyShouldBeProvidedInConstructor();
                            break;
                        case "IfSSLNotProvidedThenDefaultShouldBeFalse":
                            responseDictionary = LoadWhenAMessageIsPublishedIfSSLNotProvidedThenDefaultShouldBeFalse();
                            break;
                        case "ThenDisableJsonEncodeShouldSendSerializedObjectMessage":
                            responseDictionary = LoadWhenAMessageIsPublishedThenDisableJsonEncodeShouldSendSerializedObjectMessage();
                            break;
                        case "ThenLargeMessageShoudFailWithMessageTooLargeInfo":
                            responseDictionary = LoadWhenAMessageIsPublishedThenLargeMessageShoudFailWithMessageTooLargeInfo();
                            break;
                        default:
                            break;
                    }
                    break;
                case "WhenDetailedHistoryIsRequested":
                    switch (_testCaseName)
                    {
                        case "DetailHistoryCount10ReturnsRecords":
                            responseDictionary = LoadWhenDetailedHistoryIsRequestedDetailHistoryCount10ReturnsRecords();
                            break;
                        case "DetailHistoryCount10ReverseTrueReturnsRecords":
                            responseDictionary = LoadWhenDetailedHistoryIsRequestedDetailHistoryCount10ReverseTrueReturnsRecords();
                            break;
                        case "DetailedHistoryStartWithReverseTrue":
                            responseDictionary = LoadWhenDetailedHistoryIsRequestedDetailedHistoryStartWithReverseTrue();
                            break;
                        case "DetailHistoryWithNullKeysReturnsError":
                            responseDictionary = LoadWhenDetailedHistoryIsRequestedDetailHistoryWithNullKeysReturnsError();
                            break;
                        case "DetailHistoryNoStoreShouldNotGetMessage":
                            responseDictionary = LoadWhenDetailedHistoryIsRequestedDetailHistoryNoStoreShouldNotGetMessage();
                            break;
                        case "DetailHistoryShouldReturnDecryptMessage":
                            responseDictionary = LoadWhenDetailedHistoryIsRequestedDetailHistoryShouldReturnDecryptMessage();
                            break;
                        case "DetailHistoryShouldReturnServerTime1":
                            responseDictionary = LoadWhenDetailedHistoryIsRequestedDetailHistoryShouldReturnServerTime1();
                            break;
                        case "DetailHistoryShouldReturnServerTime2":
                            responseDictionary = LoadWhenDetailedHistoryIsRequestedDetailHistoryShouldReturnServerTime2();
                            break;
                        case "DetailHistoryShouldReturnServerTime3":
                            responseDictionary = LoadWhenDetailedHistoryIsRequestedDetailHistoryShouldReturnServerTime3();
                            break;
                        case "DetailedHistoryShouldReturnEncryptedMessageBasedOnParams":
                            responseDictionary = LoadWhenDetailedHistoryIsRequestedDetailedHistoryShouldReturnEncryptedMessageBasedOnParams();
                            break;
                        case "DetailedHistoryShouldReturnUnencryptedMessageBasedOnParams":
                            responseDictionary = LoadWhenDetailedHistoryIsRequestedDetailedHistoryShouldReturnUnencryptedMessageBasedOnParams();
                            break;
                        default:
                            break;
                    }
                    break;
                case "WhenGetRequestServerTime":
                    switch (_testCaseName)
                    {
                        case "ThenItShouldReturnTimeStamp":
                            responseDictionary = LoadWhenGetRequestServerTimeThenItShouldReturnTimeStamp();
                            break;
                        case "ThenWithProxyItShouldReturnTimeStamp":
                            responseDictionary = LoadWhenGetRequestServerTimeThenWithProxyItShouldReturnTimeStamp();
                            break;
                        default:
                            break;
                    }
                    break;
                case "WhenSubscribedToAChannel":
                    switch (_testCaseName)
                    {
                        case "ThenSubscribeShouldReturnReceivedMessage":
                            responseDictionary = LoadWhenSubscribedToAChannelThenSubscribeShouldReturnReceivedMessage();
                            break;
                        case "ThenSubscribeShouldReturnReceivedCipherMessage":
                            responseDictionary = LoadWhenSubscribedToAChannelThenSubscribeShouldReturnReceivedCipherMessage();
                            break;
                        case "ThenSubscribeShouldReturnConnectStatus":
                            responseDictionary = LoadWhenSubscribedToAChannelThenSubscribeShouldReturnConnectStatus();
                            break;
                        case "ThenMultiSubscribeShouldReturnConnectStatus":
                            responseDictionary = LoadWhenSubscribedToAChannelThenMultiSubscribeShouldReturnConnectStatus();
                            break;
                        case "ThenDuplicateChannelShouldReturnAlreadySubscribed":
                            responseDictionary = LoadWhenSubscribedToAChannelThenDuplicateChannelShouldReturnAlreadySubscribed();
                            break;
                        case "ThenSubscriberShouldBeAbleToReceiveManyMessages":
                            responseDictionary = LoadWhenSubscribedToAChannelThenSubscriberShouldBeAbleToReceiveManyMessages();
                            break;
                        case "ThenSubscribeShouldReturnReceivedComplexMessage":
                            responseDictionary = LoadWhenSubscribedToAChannelThenSubscribeShouldReturnReceivedComplexMessage();
                            break;
                        case "ThenSubscribeShouldReturnReceivedCipherComplexMessage":
                            responseDictionary = LoadWhenSubscribedToAChannelThenSubscribeShouldReturnReceivedCipherComplexMessage();
                            break;
                        case "ThenSubscribeShouldReturnReceivedEmojiMessage":
                            responseDictionary = LoadWhenSubscribedToAChannelThenSubscribeShouldReturnReceivedEmojiMessage();
                            break;
                        case "ThenSubscribeShouldReturnReceivedCipherEmojiMessage":
                            responseDictionary = LoadWhenSubscribedToAChannelThenSubscribeShouldReturnReceivedCipherEmojiMessage();
                            break;
                        case "ThenSubscribeShouldReturnUnicodeMessage":
                            responseDictionary = LoadWhenSubscribedToAChannelThenSubscribeShouldReturnUnicodeMessage();
                            break;
                        case "ThenSubscribeShouldReturnUnicodeCipherMessage":
                            responseDictionary = LoadWhenSubscribedToAChannelThenSubscribeShouldReturnUnicodeCipherMessage();
                            break;
                        case "ThenSubscribeShouldReturnReceivedForwardSlashMessage":
                            responseDictionary = LoadWhenSubscribedToAChannelThenSubscribeShouldReturnReceivedForwardSlashMessage();
                            break;
                        case "ThenSubscribeShouldReturnReceivedForwardSlashCipherMessage":
                            responseDictionary = LoadWhenSubscribedToAChannelThenSubscribeShouldReturnReceivedForwardSlashCipherMessage();
                            break;
                        case "ThenSubscribeShouldReturnSpecialCharMessage":
                            responseDictionary = LoadWhenSubscribedToAChannelThenSubscribeShouldReturnSpecialCharMessage();
                            break;
                        case "ThenSubscribeShouldReturnSpecialCharCipherMessage":
                            responseDictionary = LoadWhenSubscribedToAChannelThenSubscribeShouldReturnSpecialCharCipherMessage();
                            break;
                        default:
                            break;
                    }
                    break;
                case "WhenSubscribedToWildcardChannel":
                    switch (_testCaseName)
                    {
                        case "ChannelAndChannelGroupAndWildcardChannelSubscribeShouldReturnReceivedMessage":
                            responseDictionary = WhenSubscribedToWildcardChannel_ChannelAndChannelGroupAndWildcardChannelSubscribeShouldReturnReceivedMessage();
                            break;
                        case "ThenSubscribeShouldReturnWildCardPresenceEventInWildcardPresenceCallback":
                            responseDictionary = WhenSubscribedToWildcardChannelThenSubscribeShouldReturnWildCardPresenceEventInWildcardPresenceCallback();
                            break;
                        case "ThenSubscribeShouldReturnReceivedMessage":
                            responseDictionary = LoadWhenSubscribedToWildcardChannelThenSubscribeShouldReturnReceivedMessage();
                            break;
                        case "ThenSubscribeShouldReturnReceivedCipherMessage":
                            responseDictionary = LoadWhenSubscribedToWildcardChannelThenSubscribeShouldReturnReceivedCipherMessage();
                            break;
                        case "ThenSubscriberShouldBeAbleToReceiveManyMessages":
                            responseDictionary = LoadWhenSubscribedToAChannelThenSubscriberShouldBeAbleToReceiveManyMessages();
                            break;
                        case "ThenSubscribeShouldReturnReceivedComplexMessage":
                            responseDictionary = LoadWhenSubscribedToAChannelThenSubscribeShouldReturnReceivedComplexMessage();
                            break;
                        case "ThenSubscribeShouldReturnReceivedCipherComplexMessage":
                            responseDictionary = LoadWhenSubscribedToWildcardChannelThenSubscribeShouldReturnReceivedCipherComplexMessage();
                            break;
                        case "ThenSubscribeShouldReturnReceivedEmojiMessage":
                            responseDictionary = LoadWhenSubscribedToWildcardChannelThenSubscribeShouldReturnReceivedEmojiMessage();
                            break;
                        case "ThenSubscribeShouldReturnReceivedCipherEmojiMessage":
                            responseDictionary = LoadWhenSubscribedToWildcardChannelThenSubscribeShouldReturnReceivedCipherEmojiMessage();
                            break;
                        case "ThenSubscribeShouldReturnUnicodeMessage":
                            responseDictionary = LoadWhenSubscribedToWildcardChannelThenSubscribeShouldReturnUnicodeMessage();
                            break;
                        case "ThenSubscribeShouldReturnUnicodeCipherMessage":
                            responseDictionary = LoadWhenSubscribedToWildcardChannelThenSubscribeShouldReturnUnicodeCipherMessage();
                            break;
                        case "ThenSubscribeShouldReturnReceivedForwardSlashMessage":
                            responseDictionary = LoadWhenSubscribedToWildcardChannelThenSubscribeShouldReturnReceivedForwardSlashMessage();
                            break;
                        case "ThenSubscribeShouldReturnReceivedForwardSlashCipherMessage":
                            responseDictionary = LoadWhenSubscribedToWildcardChannelThenSubscribeShouldReturnReceivedForwardSlashCipherMessage();
                            break;
                        case "ThenSubscribeShouldReturnSpecialCharMessage":
                            responseDictionary = LoadWhenSubscribedToWildcardChannelThenSubscribeShouldReturnSpecialCharMessage();
                            break;
                        case "ThenSubscribeShouldReturnSpecialCharCipherMessage":
                            responseDictionary = LoadWhenSubscribedToWildcardChannelThenSubscribeShouldReturnSpecialCharCipherMessage();
                            break;
                        default:
                            break;
                    }
                    break;
                case "WhenSubscribedToAChannelGroup":
                    switch (_testCaseName)
                    {
                        case "ThenSubscribeShouldReturnReceivedMessage":
                            responseDictionary = LoadWhenSubscribedToAChannelGroupThenSubscribeShouldReturnReceivedMessage();
                            break;
                        case "ThenSubscribeShouldReturnConnectStatus":
                            responseDictionary = LoadWhenSubscribedToAChannelGroupThenSubscribeShouldReturnConnectStatus();
                            break;
                        case "ThenMultiSubscribeShouldReturnConnectStatus":
                            responseDictionary = LoadWhenSubscribedToAChannelGroupThenMultiSubscribeShouldReturnConnectStatus();
                            break;
                        default:
                            break;
                    }
                    break;
                case "WhenUnsubscribedToAChannel":
                    switch (_testCaseName)
                    {
                        case "ThenShouldReturnUnsubscribedMessage":
                            responseDictionary = LoadWhenUnsubscribedToAChannelThenShouldReturnUnsubscribedMessage();
                            break;
                        default:
                            break;
                    }
                    break;
                case "WhenUnsubscribedToAChannelGroup":
                    switch (_testCaseName)
                    {
                        case "ThenShouldReturnUnsubscribedMessage":
                            responseDictionary = LoadWhenUnsubscribedToAChannelGroupThenShouldReturnUnsubscribedMessage();
                            break;
                        default:
                            break;
                    }
                    break;
                case "WhenAuditIsRequested":
                    switch (_testCaseName)
                    {
                        case "ThenSubKeyLevelShouldReturnSuccess":
                            responseDictionary = LoadWhenAuditIsRequestedThenSubKeyLevelShouldReturnSuccess();
                            break;
                        case "ThenChannelLevelShouldReturnSuccess":
                            responseDictionary = LoadWhenAuditIsRequestedThenChannelLevelShouldReturnSuccess();
                            break;
                        case "ThenChannelGroupLevelShouldReturnSuccess":
                            responseDictionary = LoadWhenAuditIsRequestedThenChannelGroupLevelShouldReturnSuccess();
                            break;
                        default:
                            break;
                    }
                    break;
                case "WhenGrantIsRequested":
                    switch (_testCaseName)
                    {
                        case "ThenSubKeyLevelWithReadWriteShouldReturnSuccess":
                            responseDictionary = LoadWhenGrantIsRequestedThenSubKeyLevelWithReadWriteShouldReturnSuccess();
                            break;
                        case "ThenSubKeyLevelWithReadShouldReturnSuccess":
                            responseDictionary = LoadWhenGrantIsRequestedThenSubKeyLevelWithReadShouldReturnSuccess();
                            break;
                        case "ThenSubKeyLevelWithWriteShouldReturnSuccess":
                            responseDictionary = LoadWhenGrantIsRequestedThenSubKeyLevelWithWriteShouldReturnSuccess();
                            break;
                        case "ThenChannelLevelWithReadWriteShouldReturnSuccess":
                            responseDictionary = LoadWhenGrantIsRequestedThenChannelLevelWithReadWriteShouldReturnSuccess();
                            break;
                        case "ThenChannelLevelWithReadShouldReturnSuccess":
                            responseDictionary = LoadWhenGrantIsRequestedThenChannelLevelWithReadShouldReturnSuccess();
                            break;
                        case "ThenChannelLevelWithWriteShouldReturnSuccess":
                            responseDictionary = LoadWhenGrantIsRequestedThenChannelLevelWithWriteShouldReturnSuccess();
                            break;
                        case "ThenUserLevelWithReadWriteShouldReturnSuccess":
                            responseDictionary = LoadWhenGrantIsRequestedThenUserLevelWithReadWriteShouldReturnSuccess();
                            break;
                        case "ThenUserLevelWithReadShouldReturnSuccess":
                            responseDictionary = LoadWhenGrantIsRequestedThenUserLevelWithReadShouldReturnSuccess();
                            break;
                        case "ThenUserLevelWithWriteShouldReturnSuccess":
                            responseDictionary = LoadWhenGrantIsRequestedThenUserLevelWithWriteShouldReturnSuccess();
                            break;
                        case "ThenMultipleChannelGrantShouldReturnSuccess":
                            responseDictionary = LoadWhenGrantIsRequestedThenMultipleChannelGrantShouldReturnSuccess();
                            break;
                        case "ThenMultipleAuthGrantShouldReturnSuccess":
                            responseDictionary = LoadWhenGrantIsRequestedThenMultipleAuthGrantShouldReturnSuccess();
                            break;
                        case "ThenRevokeAtSubKeyLevelReturnSuccess":
                            responseDictionary = LoadWhenGrantIsRequestedThenRevokeAtSubKeyLevelReturnSuccess();
                            break;
                        case "ThenRevokeAtChannelLevelReturnSuccess":
                            responseDictionary = LoadWhenGrantIsRequestedThenRevokeAtChannelLevelReturnSuccess();
                            break;
                        case "ThenRevokeAtUserLevelReturnSuccess":
                            responseDictionary = LoadWhenGrantIsRequestedThenRevokeAtUserLevelReturnSuccess();
                            break;
                        case "ThenChannelGroupLevelWithReadManageShouldReturnSuccess":
                            responseDictionary = LoadWhenGrantIsRequestedThenChannelGroupLevelWithReadManageShouldReturnSuccess();
                            break;
                        case "ThenChannelGroupLevelWithReadShouldReturnSuccess":
                            responseDictionary = LoadWhenGrantIsRequestedThenChannelGroupLevelWithReadShouldReturnSuccess();
                            break;
                        default:
                            break;
                    }
                    break;
                case "WhenPushIsRequested":
                    switch (_testCaseName)
                    {
                        case "ThenRegisterDeviceShouldReturnSuccess":
                            responseDictionary = LoadWhenPushIsRequestedThenRegisterDeviceShouldReturnSuccess();
                            break;
                        case "ThenUnregisterDeviceShouldReturnSuccess":
                            responseDictionary = LoadWhenPushIsRequestedThenUnregisterDeviceShouldReturnSuccess();
                            break;
                        case "ThenRemoveChannelForDeviceShouldReturnSuccess":
                            responseDictionary = LoadWhenPushIsRequestedThenRemoveChannelForDeviceShouldReturnSuccess();
                            break;
                        case "ThenGetAllChannelsForDeviceShouldReturnSuccess":
                            responseDictionary = LoadWhenPushIsRequestedThenGetAllChannelsForDeviceShouldReturnSuccess();
                            break;
                        case "ThenPublishMpnsToastShouldReturnSuccess":
                            responseDictionary = LoadWhenPushIsRequestedThenPublishMpnsToastShouldReturnSuccess();
                            break;
                        case "ThenPublishMpnsFlipTileShouldReturnSuccess":
                            responseDictionary = LoadWhenPushIsRequestedThenPublishMpnsFlipTileShouldReturnSuccess();
                            break;
                        case "ThenPublishMpnsCycleTileShouldReturnSuccess":
                            responseDictionary = LoadWhenPushIsRequestedThenPublishMpnsCycleTileShouldReturnSuccess();
                            break;
                        case "ThenPublishMpnsIconicTileShouldReturnSuccess":
                            responseDictionary = LoadWhenPushIsRequestedThenPublishMpnsIconicTileShouldReturnSuccess();
                            break;
                        default:
                            break;
                    }
                    break;
                case "WhenChannelGroupIsRequested":
                    switch (_testCaseName)
                    {
                        case "ThenAddChannelShouldReturnSuccess":
                            responseDictionary = LoadWhenChannelGroupIsRequestedThenAddChannelShouldReturnSuccess();
                            break;
                        case "ThenRemoveChannelShouldReturnSuccess":
                            responseDictionary = LoadWhenChannelGroupIsRequestedThenRemoveChannelShouldReturnSuccess();
                            break;
                        case "ThenGetChannelListShouldReturnSuccess":
                            responseDictionary = LoadWhenChannelGroupIsRequestedThenGetChannelListShouldReturnSuccess();
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            if (responseDictionary != null && responseDictionary.ContainsKey(requestUri.AbsolutePath))
            {
                stubResponse = responseDictionary[requestUri.AbsolutePath];
                if (_testClassName == "WhenAMessageIsPublished" && _testCaseName == "ThenLargeMessageShoudFailWithMessageTooLargeInfo")
                {
                    PubnubWebResponse stubWebResponse = new PubnubWebResponse(new MemoryStream(Encoding.UTF8.GetBytes(stubResponse)), HttpStatusCode.BadRequest);
#if (SILVERLIGHT || WINDOWS_PHONE || NETFX_CORE)
                    WebException largeMessageException = new WebException("The remote server returned an error: (400) Bad Request", null, WebExceptionStatus.Pending, stubWebResponse);
#else
                    WebException largeMessageException = new WebException("The remote server returned an error: (400) Bad Request", null, WebExceptionStatus.ProtocolError, stubWebResponse);
#endif
                    throw largeMessageException;
                }
            }
            else if (responseDictionary != null)
            {
#if (SILVERLIGHT || WINDOWS_PHONE || NETFX_CORE)
                string uriKey = string.Format("{0}{1}",requestUri.AbsolutePath, requestUri.Query);
                if (responseDictionary.ContainsKey(uriKey))
                {
                    stubResponse = responseDictionary[uriKey];
                }
                else if (requestUri.Query.Contains("&pnsdk="))
                {
                    int indexOfPnsdk = requestUri.Query.IndexOf("&pnsdk=");
                    string modifiedQuery = requestUri.Query.Substring(0, indexOfPnsdk);
                    string uriModifiedKey = string.Format("{0}{1}",requestUri.AbsolutePath, modifiedQuery);
                    if (responseDictionary.ContainsKey(uriModifiedKey))
                    {
                        stubResponse = responseDictionary[uriModifiedKey];
                    }
                    else
                    {
                         stubResponse = "[]";
                    }
                }
#else
                if (responseDictionary.ContainsKey(requestUri.PathAndQuery))
                {
                    stubResponse = responseDictionary[requestUri.PathAndQuery];
                }
                else if (requestUri.PathAndQuery.Contains("&pnsdk="))
                {
                    int indexOfPnsdk = requestUri.PathAndQuery.IndexOf("&pnsdk=");
                    string modifiedRequestUri = requestUri.PathAndQuery.Substring(0, indexOfPnsdk);
                    if (responseDictionary.ContainsKey(modifiedRequestUri))
                    {
                        stubResponse = responseDictionary[modifiedRequestUri];
                    }
                    else
                    {
                        stubResponse = "[]";
                    }
                }
#endif
            }
            else
            {
                stubResponse = "[]";
            }
            if (stubResponse == "!! Stub Response Not Assigned !!" || stubResponse == "[]")
            {
                Console.WriteLine("requestUri.AbsolutePath = " + requestUri.AbsolutePath);
                Console.WriteLine(string.Format("stubResponse = {0} for testcase = {1}", stubResponse, _testCaseName));
                System.Diagnostics.Debug.WriteLine("requestUri.AbsolutePath = " + requestUri.AbsolutePath);
                System.Diagnostics.Debug.WriteLine("stubResponse = " + stubResponse);
            }
            return stubResponse;
        }

        public string TestCaseName
        {
            get
            {
                return _testCaseName;
            }
            set
            {
                _testCaseName = value;
            }
        }


        public string TestClassName
        {
            get
            {
                return _testClassName;
            }
            set
            {
                _testClassName = value;
            }
        }
    }
}
