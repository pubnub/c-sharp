using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using PubNubMessaging.Core;

namespace PubNubMessaging.Tests
{
    public class WhenGrantIsRequested: UUnitTestCase
    {

        ManualResetEvent grantManualEvent = new ManualResetEvent(false);
        ManualResetEvent revokeManualEvent = new ManualResetEvent(false);
        bool receivedGrantMessage = false;
        bool receivedRevokeMessage = false;
        int multipleChannelGrantCount = 5;
        int multipleAuthGrantCount = 5;
        string currentUnitTestCase = "";

        [UUnitTest]
        public void ThenSubKeyLevelWithReadWriteShouldReturnSuccess()
        {
			Debug.Log("Running ThenSubKeyLevelWithReadWriteShouldReturnSuccess()");
            currentUnitTestCase = "ThenSubKeyLevelWithReadWriteShouldReturnSuccess";

            receivedGrantMessage = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenSubKeyLevelWithReadWriteShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;
            if (PubnubCommon.PAMEnabled)
            {
                pubnub.GrantAccess<string>("", true, true, 5, AccessToSubKeyLevelCallback, DummyErrorCallback);
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                UUnitAssert.True(receivedGrantMessage, "WhenGrantIsRequested -> ThenSubKeyLevelWithReadWriteShouldReturnSuccess failed.");
            }
            else
            {
                UUnitAssert.False(true,"PAM Not Enabled for WhenGrantIsRequested -> ThenSubKeyLevelWithReadWriteShouldReturnSuccess.");
            }
        }

        [UUnitTest]
        public void ThenSubKeyLevelWithReadShouldReturnSuccess()
        {
			Debug.Log("Running ThenSubKeyLevelWithReadShouldReturnSuccess()");
            currentUnitTestCase = "ThenSubKeyLevelWithReadShouldReturnSuccess";

            receivedGrantMessage = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenSubKeyLevelWithReadShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;
            if (PubnubCommon.PAMEnabled)
            {
                pubnub.GrantAccess<string>("", true, false, 5, AccessToSubKeyLevelCallback, DummyErrorCallback);
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                UUnitAssert.True(receivedGrantMessage, "WhenGrantIsRequested -> ThenSubKeyLevelWithReadShouldReturnSuccess failed.");
            }
            else
            {
                UUnitAssert.False(true,"PAM Not Enabled for WhenGrantIsRequested -> ThenSubKeyLevelWithReadShouldReturnSuccess.");
            }
        }

        [UUnitTest]
        public void ThenSubKeyLevelWithWriteShouldReturnSuccess()
        {
			Debug.Log("Running ThenSubKeyLevelWithWriteShouldReturnSuccess()");
            currentUnitTestCase = "ThenSubKeyLevelWithWriteShouldReturnSuccess";

            receivedGrantMessage = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenSubKeyLevelWithWriteShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;
            if (PubnubCommon.PAMEnabled)
            {
                pubnub.GrantAccess<string>("", false, true, 5, AccessToSubKeyLevelCallback, DummyErrorCallback);
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                UUnitAssert.True(receivedGrantMessage, "WhenGrantIsRequested -> ThenSubKeyLevelWithWriteShouldReturnSuccess failed.");
            }
            else
            {
                UUnitAssert.False(true,"PAM Not Enabled for WhenGrantIsRequested -> ThenSubKeyLevelWithWriteShouldReturnSuccess.");
            }
        }

        [UUnitTest]
        public void ThenChannelLevelWithReadWriteShouldReturnSuccess()
        {
			Debug.Log("Running ThenChannelLevelWithReadWriteShouldReturnSuccess()");
            currentUnitTestCase = "ThenChannelLevelWithReadWriteShouldReturnSuccess";

            receivedGrantMessage = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenChannelLevelWithReadWriteShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            if (PubnubCommon.PAMEnabled)
            {
                pubnub.GrantAccess<string>(channel, true, true, 5, AccessToChannelLevelCallback, DummyErrorCallback);
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                UUnitAssert.True(receivedGrantMessage, "WhenGrantIsRequested -> ThenChannelLevelWithReadWriteShouldReturnSuccess failed.");
            }
            else
            {
                UUnitAssert.False(true,"PAM Not Enabled for WhenGrantIsRequested -> ThenChannelLevelWithReadWriteShouldReturnSuccess.");
            }
        }

        [UUnitTest]
        public void ThenChannelLevelWithReadShouldReturnSuccess()
        {
			Debug.Log("Running ThenChannelLevelWithReadShouldReturnSuccess()");
            currentUnitTestCase = "ThenChannelLevelWithReadShouldReturnSuccess";

            receivedGrantMessage = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenChannelLevelWithReadShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            if (PubnubCommon.PAMEnabled)
            {
                pubnub.GrantAccess<string>(channel, true, false, 5, AccessToChannelLevelCallback, DummyErrorCallback);
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                UUnitAssert.True(receivedGrantMessage, "WhenGrantIsRequested -> ThenChannelLevelWithReadShouldReturnSuccess failed.");
            }
            else
            {
                UUnitAssert.False(true,"PAM Not Enabled for WhenGrantIsRequested -> ThenChannelLevelWithReadShouldReturnSuccess.");
            }
        }

        [UUnitTest]
        public void ThenChannelLevelWithWriteShouldReturnSuccess()
        {
			Debug.Log("Running ThenChannelLevelWithWriteShouldReturnSuccess()");
            currentUnitTestCase = "ThenChannelLevelWithWriteShouldReturnSuccess";

            receivedGrantMessage = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenChannelLevelWithWriteShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            if (PubnubCommon.PAMEnabled)
            {
                pubnub.GrantAccess<string>(channel, false, true, 5, AccessToChannelLevelCallback, DummyErrorCallback);
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                UUnitAssert.True(receivedGrantMessage, "WhenGrantIsRequested -> ThenChannelLevelWithWriteShouldReturnSuccess failed.");
            }
            else
            {
                UUnitAssert.False(true,"PAM Not Enabled for WhenGrantIsRequested -> ThenChannelLevelWithWriteShouldReturnSuccess.");
            }
        }

        [UUnitTest]
        public void ThenUserLevelWithReadWriteShouldReturnSuccess()
        {
			Debug.Log("Running ThenUserLevelWithReadWriteShouldReturnSuccess()");
            currentUnitTestCase = "ThenUserLevelWithReadWriteShouldReturnSuccess";

            receivedGrantMessage = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenUserLevelWithReadWriteShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_authchannel";
            string authKey = "hello_my_authkey";
            if (PubnubCommon.PAMEnabled)
            {
                pubnub.AuthenticationKey = authKey;
                pubnub.GrantAccess<string>(channel, true, true, 5, AccessToUserLevelCallback, DummyErrorCallback);
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                UUnitAssert.True(receivedGrantMessage, "WhenGrantIsRequested -> ThenUserLevelWithReadWriteShouldReturnSuccess failed.");
            }
            else
            {
                UUnitAssert.False(true,"PAM Not Enabled for WhenGrantIsRequested -> ThenUserLevelWithReadWriteShouldReturnSuccess.");
            }
        }

        [UUnitTest]
        public void ThenUserLevelWithReadShouldReturnSuccess()
        {
			Debug.Log("Running ThenUserLevelWithReadShouldReturnSuccess()");
            currentUnitTestCase = "ThenUserLevelWithReadShouldReturnSuccess";

            receivedGrantMessage = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenUserLevelWithReadShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_authchannel";
            string authKey = "hello_my_authkey";
            if (PubnubCommon.PAMEnabled)
            {
                pubnub.AuthenticationKey = authKey;
                pubnub.GrantAccess<string>(channel, true, false, 5, AccessToUserLevelCallback, DummyErrorCallback);
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                UUnitAssert.True(receivedGrantMessage, "WhenGrantIsRequested -> ThenUserLevelWithReadShouldReturnSuccess failed.");
            }
            else
            {
                UUnitAssert.False(true,"PAM Not Enabled for WhenGrantIsRequested -> ThenUserLevelWithReadShouldReturnSuccess.");
            }
        }

        [UUnitTest]
        public void ThenUserLevelWithWriteShouldReturnSuccess()
        {
			Debug.Log("Running ThenUserLevelWithWriteShouldReturnSuccess()");
            currentUnitTestCase = "ThenUserLevelWithWriteShouldReturnSuccess";

            receivedGrantMessage = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenUserLevelWithWriteShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_authchannel";
            string authKey = "hello_my_authkey";
            if (PubnubCommon.PAMEnabled)
            {
                pubnub.AuthenticationKey = authKey;
                pubnub.GrantAccess<string>(channel, false, true, 5, AccessToUserLevelCallback, DummyErrorCallback);
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                UUnitAssert.True(receivedGrantMessage, "WhenGrantIsRequested -> ThenUserLevelWithWriteShouldReturnSuccess failed.");
            }
            else
            {
                UUnitAssert.False(true,"PAM Not Enabled for WhenGrantIsRequested -> ThenUserLevelWithWriteShouldReturnSuccess.");
            }
        }

        [UUnitTest]
        public void ThenMultipleChannelGrantShouldReturnSuccess()
        {
			Debug.Log("Running ThenMultipleChannelGrantShouldReturnSuccess()");
            currentUnitTestCase = "ThenMultipleChannelGrantShouldReturnSuccess";

            receivedGrantMessage = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenMultipleChannelGrantShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            StringBuilder channelBuilder = new StringBuilder();
            for (int index = 0; index < multipleChannelGrantCount; index++)
            {
                if (index == multipleChannelGrantCount - 1)
                {
                    channelBuilder.AppendFormat("csharp-hello_my_channel-{0}", index);
                }
                else
                {
                    channelBuilder.AppendFormat("csharp-hello_my_channel-{0},", index);
                }
            }
            string channel = "";
            if (!unitTest.EnableStubTest)
            {
                channel = channelBuilder.ToString();
            }
            else
            {
                multipleChannelGrantCount = 5;
                channel = "csharp-hello_my_channel-0,csharp-hello_my_channel-1,csharp-hello_my_channel-2,csharp-hello_my_channel-3,csharp-hello_my_channel-4";
            }
            if (PubnubCommon.PAMEnabled)
            {
                pubnub.GrantAccess<string>(channel, true, true, 5, AccessToMultiChannelGrantCallback, DummyErrorCallback);
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                UUnitAssert.True(receivedGrantMessage, "WhenGrantIsRequested -> ThenMultipleChannelGrantShouldReturnSuccess failed.");
            }
            else
            {
                UUnitAssert.False(true,"PAM Not Enabled for WhenGrantIsRequested -> ThenMultipleChannelGrantShouldReturnSuccess.");
            }
        }

        [UUnitTest]
        public void ThenMultipleAuthGrantShouldReturnSuccess()
        {
			Debug.Log("Running ThenMultipleAuthGrantShouldReturnSuccess()");
            currentUnitTestCase = "ThenMultipleAuthGrantShouldReturnSuccess";

            receivedGrantMessage = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenMultipleAuthGrantShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            StringBuilder authKeyBuilder = new StringBuilder();
            for (int index = 0; index < multipleAuthGrantCount; index++)
            {
                if (index == multipleAuthGrantCount - 1)
                {
                    authKeyBuilder.AppendFormat("csharp-auth_key-{0}", index);
                }
                else
                {
                    authKeyBuilder.AppendFormat("csharp-auth_key-{0},", index);
                }
            }
            string channel = "hello_my_channel";
            string auth = "";
            if (!unitTest.EnableStubTest)
            {
                auth = authKeyBuilder.ToString();
            }
            else
            {
                multipleAuthGrantCount = 5;
                auth = "csharp-auth_key-0,csharp-auth_key-1,csharp-auth_key-2,csharp-auth_key-3,csharp-auth_key-4";
            }
            if (PubnubCommon.PAMEnabled)
            {
                pubnub.AuthenticationKey = auth;
                pubnub.GrantAccess<string>(channel, true, true, 5, AccessToMultiAuthGrantCallback, DummyErrorCallback);
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                UUnitAssert.True(receivedGrantMessage, "WhenGrantIsRequested -> ThenMultipleAuthGrantShouldReturnSuccess failed.");
            }
            else
            {
                UUnitAssert.False(true,"PAM Not Enabled for WhenGrantIsRequested -> ThenMultipleAuthGrantShouldReturnSuccess.");
            }
        }

        [UUnitTest]
        public void ThenRevokeAtSubKeyLevelReturnSuccess()
        {
			Debug.Log("Running ThenRevokeAtSubKeyLevelReturnSuccess()");
            currentUnitTestCase = "ThenRevokeAtSubKeyLevelReturnSuccess";

            receivedGrantMessage = false;
            receivedRevokeMessage = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenRevokeAtSubKeyLevelReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;
            if (PubnubCommon.PAMEnabled)
            {
                if (!unitTest.EnableStubTest)
                {
                    pubnub.GrantAccess<string>("", true, true, 5, AccessToSubKeyLevelCallback, DummyErrorCallback);
                    Thread.Sleep(1000);
                    grantManualEvent.WaitOne();
                }
                else
                {
                    receivedGrantMessage = true;
                }
                if (receivedGrantMessage)
                {
                    Console.WriteLine("WhenGrantIsRequested -> ThenRevokeAtSubKeyLevelReturnSuccess -> Grant ok..Now trying Revoke");
                    pubnub.GrantAccess<string>("", false, false, 5, RevokeToSubKeyLevelCallback, DummyErrorCallback);
                    Thread.Sleep(1000);
                    revokeManualEvent.WaitOne();
                    UUnitAssert.True(receivedRevokeMessage, "WhenGrantIsRequested -> ThenRevokeAtSubKeyLevelReturnSuccess -> Grant success but revoke failed.");
                }
                else
                {
                    UUnitAssert.True(receivedGrantMessage, "WhenGrantIsRequested -> ThenRevokeAtSubKeyLevelReturnSuccess failed. -> Grant not occured, so is revoke");
                }
            }
            else
            {
                UUnitAssert.False(true,"PAM Not Enabled for WhenGrantIsRequested -> ThenRevokeAtSubKeyLevelReturnSuccess.");
            }
        }

        [UUnitTest]
        public void ThenRevokeAtChannelLevelReturnSuccess()
        {
            Debug.Log("Running ThenRevokeAtChannelLevelReturnSuccess()");
			currentUnitTestCase = "ThenRevokeAtChannelLevelReturnSuccess";

            receivedGrantMessage = false;
            receivedRevokeMessage = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenRevokeAtChannelLevelReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            if (PubnubCommon.PAMEnabled)
            {
                if (!unitTest.EnableStubTest)
                {
                    pubnub.GrantAccess<string>(channel, true, true, 5, AccessToChannelLevelCallback, DummyErrorCallback);
                    Thread.Sleep(1000);
                    grantManualEvent.WaitOne();
                }
                else
                {
                    receivedGrantMessage = true;
                }
                if (receivedGrantMessage)
                {
                    Console.WriteLine("WhenGrantIsRequested -> ThenRevokeAtChannelLevelReturnSuccess -> Grant ok..Now trying Revoke");
                    pubnub.GrantAccess<string>("", false, false, 5, RevokeToChannelLevelCallback, DummyErrorCallback);
                    Thread.Sleep(1000);
                    revokeManualEvent.WaitOne();
                    UUnitAssert.True(receivedRevokeMessage, "WhenGrantIsRequested -> ThenRevokeAtChannelLevelReturnSuccess -> Grant success but revoke failed.");
                }
                else
                {
                    UUnitAssert.True(receivedGrantMessage, "WhenGrantIsRequested -> ThenRevokeAtChannelLevelReturnSuccess failed. -> Grant not occured, so is revoke");
                }
            }
            else
            {
                UUnitAssert.False(true,"PAM Not Enabled for WhenGrantIsRequested -> ThenRevokeAtChannelLevelReturnSuccess.");
            }
        }

        [UUnitTest]
        public void ThenRevokeAtUserLevelReturnSuccess()
        {
			Debug.Log("Running ThenRevokeAtUserLevelReturnSuccess()");
            currentUnitTestCase = "ThenRevokeAtUserLevelReturnSuccess";

            receivedGrantMessage = false;
            receivedRevokeMessage = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenRevokeAtUserLevelReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_authchannel";
            string authKey = "hello_my_authkey";
            if (PubnubCommon.PAMEnabled)
            {
                if (!unitTest.EnableStubTest)
                {
                    pubnub.AuthenticationKey = authKey;
                    pubnub.GrantAccess<string>(channel, true, true, 5, AccessToUserLevelCallback, DummyErrorCallback);
                    Thread.Sleep(1000);
                    grantManualEvent.WaitOne();
                }
                else
                {
                    receivedGrantMessage = true;
                }
                if (receivedGrantMessage)
                {
                    Console.WriteLine("WhenGrantIsRequested -> ThenRevokeAtUserLevelReturnSuccess -> Grant ok..Now trying Revoke");
                    pubnub.GrantAccess<string>("", false, false, 5, RevokeToUserLevelCallback, DummyErrorCallback);
                    Thread.Sleep(1000);
                    revokeManualEvent.WaitOne();
                    UUnitAssert.True(receivedRevokeMessage, "WhenGrantIsRequested -> ThenRevokeAtUserLevelReturnSuccess -> Grant success but revoke failed.");
                }
                else
                {
                    UUnitAssert.True(receivedGrantMessage, "WhenGrantIsRequested -> ThenRevokeAtUserLevelReturnSuccess failed. -> Grant not occured, so is revoke");
                }
            }
            else
            {
                UUnitAssert.False(true,"PAM Not Enabled for WhenGrantIsRequested -> ThenRevokeAtUserLevelReturnSuccess.");
            }
        }

        void AccessToSubKeyLevelCallback(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    //object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
					object[] deserializedMessage = new JsonFXDotNet().DeserializeToListOfObject(receivedMessage).ToArray();
					Dictionary<string,object> dictionary = null;
					if (deserializedMessage is object[])
					{
						dictionary = deserializedMessage[0] as Dictionary<string, object>;
					}
                    //JContainer dictionary = serializedMessage[0] as JContainer;
                    if (dictionary != null)
                    {
                        int statusCode = (int)dictionary["status"];
                        string statusMessage = dictionary["message"].ToString();
                        if (statusCode == 200 && statusMessage.ToLower() == "success")
                        {
                            Dictionary<string, object> payload = dictionary["payload"] as Dictionary<string, object>;
                            if (payload != null)
                            {
                                bool read = (payload["r"].ToString() == "1") ? true : false;
                                bool write = (payload["w"].ToString() == "1") ? true : false;
                                string level = payload["level"].ToString();
                                if (level == "subkey")
                                {
                                    switch (currentUnitTestCase)
                                    {
                                        case "ThenSubKeyLevelWithReadWriteShouldReturnSuccess":
                                        case "ThenRevokeAtSubKeyLevelReturnSuccess":
                                            if (read && write) receivedGrantMessage = true;
                                            break;
                                        case "ThenSubKeyLevelWithReadShouldReturnSuccess":
                                            if (read && !write) receivedGrantMessage = true;
                                            break;
                                        case "ThenSubKeyLevelWithWriteShouldReturnSuccess":
                                            if (!read && write) receivedGrantMessage = true;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                grantManualEvent.Set();
            }
        }

        void AccessToChannelLevelCallback(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    //object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
					object[] deserializedMessage = new JsonFXDotNet().DeserializeToListOfObject(receivedMessage).ToArray();
					Dictionary<string,object> dictionary = null;
					string currentChannel = "";
					if (deserializedMessage is object[])
					{
						dictionary = deserializedMessage[0] as Dictionary<string, object>;
                    	currentChannel = deserializedMessage[1].ToString();
					}
                    //JContainer dictionary = serializedMessage[0] as JContainer;
                    if (dictionary != null)
                    {
                        int statusCode = (int)dictionary["status"];
                        string statusMessage = dictionary["message"].ToString();
                        if (statusCode == 200 && statusMessage.ToLower() == "success")
                        {
                            Dictionary<string, object> payload = dictionary["payload"] as Dictionary<string, object>;
                            if (payload != null)
                            {
                                string level = payload["level"].ToString();
                                Dictionary<string, object> channels = payload["channels"] as Dictionary<string, object>;
                                if (channels != null)
                                {
                                    Dictionary<string, object> channelContainer = channels[currentChannel] as Dictionary<string, object>;
                                    if (channelContainer != null)
                                    {
		                                bool read = (channelContainer["r"].ToString() == "1") ? true : false;
		                                bool write = (channelContainer["w"].ToString() == "1") ? true : false;
                                        if (level == "channel")
                                        {
                                            switch (currentUnitTestCase)
                                            {
                                                case "ThenChannelLevelWithReadWriteShouldReturnSuccess":
                                                case "ThenRevokeAtChannelLevelReturnSuccess":
                                                    if (read && write) receivedGrantMessage = true;
                                                    break;
                                                case "ThenChannelLevelWithReadShouldReturnSuccess":
                                                    if (read && !write) receivedGrantMessage = true;
                                                    break;
                                                case "ThenChannelLevelWithWriteShouldReturnSuccess":
                                                    if (!read && write) receivedGrantMessage = true;
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                grantManualEvent.Set();
            }
        }

        void AccessToUserLevelCallback(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    //object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
					object[] deserializedMessage = new JsonFXDotNet().DeserializeToListOfObject(receivedMessage).ToArray();
					Dictionary<string,object> dictionary = null;
					string currentChannel = "";
					if (deserializedMessage is object[])
					{
						dictionary = deserializedMessage[0] as Dictionary<string, object>;
                    	currentChannel = deserializedMessage[1].ToString();
					}
                    //JContainer dictionary = serializedMessage[0] as JContainer;
                    //string currentChannel = serializedMessage[1].ToString();
                    if (dictionary != null)
                    {
                        int statusCode = (int)dictionary["status"];
                        string statusMessage = dictionary["message"].ToString();
                        if (statusCode == 200 && statusMessage.ToLower() == "success")
                        {
                            Dictionary<string, object> payload = dictionary["payload"] as Dictionary<string, object>;
                            if (payload != null)
                            {
                                string level = payload["level"].ToString();
                                string channel = payload["channel"].ToString();
                                Dictionary<string, object> auths = payload["auths"] as Dictionary<string, object>;
                                if (auths != null && auths.Count > 0)
                                {
                                    foreach(string authKey in auths.Keys)
                                    {
                                        //if (auth is JProperty)
                                        //{
                                            //var authProperty = auth as JProperty;
                                            //if (authProperty != null)
                                            //{
                                                //string authKey = authProperty.Name;
                                                Dictionary<string, object> authKeyContainer = auths[authKey] as Dictionary<string, object>;
                                                if (authKeyContainer != null && authKeyContainer.Count > 0)
                                                {
					                                bool read = (authKeyContainer["r"].ToString() == "1") ? true : false;
					                                bool write = (authKeyContainer["w"].ToString() == "1") ? true : false;
                                                    if (level == "user")
                                                    {
                                                        switch (currentUnitTestCase)
                                                        {
                                                            case "ThenUserLevelWithReadWriteShouldReturnSuccess":
                                                            case "ThenRevokeAtUserLevelReturnSuccess":
                                                                if (read && write) receivedGrantMessage = true;
                                                                break;
                                                            case "ThenUserLevelWithReadShouldReturnSuccess":
                                                                if (read && !write) receivedGrantMessage = true;
                                                                break;
                                                            case "ThenUserLevelWithWriteShouldReturnSuccess":
                                                                if (!read && write) receivedGrantMessage = true;
                                                                break;
                                                            default:
                                                                break;
                                                        }
                                                    }
                                                }

                                            //}
                                        //}
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                grantManualEvent.Set();
            }
        }

        void AccessToMultiChannelGrantCallback(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    //object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
					object[] deserializedMessage = new JsonFXDotNet().DeserializeToListOfObject(receivedMessage).ToArray();
					Dictionary<string,object> dictionary = null;
					string currentChannel = "";
					if (deserializedMessage is object[])
					{
						dictionary = deserializedMessage[0] as Dictionary<string, object>;
                    	currentChannel = deserializedMessage[1].ToString();
					}
                    //JContainer dictionary = serializedMessage[0] as JContainer;
                    //string currentChannel = serializedMessage[1].ToString();
                    if (dictionary != null)
                    {
                        int statusCode = (int)dictionary["status"];
                        string statusMessage = dictionary["message"].ToString();
                        if (statusCode == 200 && statusMessage.ToLower() == "success")
                        {
                            Dictionary<string, object> payload = dictionary["payload"] as Dictionary<string, object>;
                            if (payload != null)
                            {
                                string level = payload["level"].ToString();
                                Dictionary<string, object> channels = payload["channels"] as Dictionary<string, object>;
                                if (channels != null)
                                {
                                    //Console.WriteLine("{0} - AccessToMultiChannelGrantCallback - Grant MultiChannel Count (Received/Sent) = {1}/{2}", currentUnitTestCase, channels.Count, multipleChannelGrantCount);
                                    if (channels.Count == multipleChannelGrantCount)
                                    {
                                        receivedGrantMessage = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                grantManualEvent.Set();
            }
        }

        void AccessToMultiAuthGrantCallback(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    //object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
					object[] deserializedMessage = new JsonFXDotNet().DeserializeToListOfObject(receivedMessage).ToArray();
					Dictionary<string,object> dictionary = null;
					string currentChannel = "";
					if (deserializedMessage is object[])
					{
						dictionary = deserializedMessage[0] as Dictionary<string, object>;
                    	currentChannel = deserializedMessage[1].ToString();
					}
                    //JContainer dictionary = serializedMessage[0] as JContainer;
                    //string currentChannel = serializedMessage[1].ToString();
                    if (dictionary != null)
                    {
                        int statusCode = (int)dictionary["status"];
                        string statusMessage = dictionary["message"].ToString();
                        if (statusCode == 200 && statusMessage.ToLower() == "success")
                        {
                            Dictionary<string, object> payload = dictionary["payload"] as Dictionary<string, object>;
                            if (payload != null)
                            {
                                string level = payload["level"].ToString();
                                string channel = payload["channel"].ToString();
                                Dictionary<string, object> auths = payload["auths"] as Dictionary<string, object>;
                                if (auths != null)
                                {
                                    //Console.WriteLine("{0} - AccessToMultiAuthGrantCallback - Grant Auth Count (Received/Sent) = {1}/{2}", currentUnitTestCase, auths.Count, multipleAuthGrantCount);
                                    if (auths.Count == multipleAuthGrantCount)
                                    {
                                        receivedGrantMessage = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                grantManualEvent.Set();
            }
        }

        void RevokeToSubKeyLevelCallback(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    //object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
					object[] deserializedMessage = new JsonFXDotNet().DeserializeToListOfObject(receivedMessage).ToArray();
					Dictionary<string,object> dictionary = null;
					if (deserializedMessage is object[])
					{
						dictionary = deserializedMessage[0] as Dictionary<string, object>;
					}
                    //JContainer dictionary = serializedMessage[0] as JContainer;
                    if (dictionary != null)
                    {
                        int statusCode = (int)dictionary["status"];
                        string statusMessage = dictionary["message"].ToString();
                        if (statusCode == 200 && statusMessage.ToLower() == "success")
                        {
                            Dictionary<string, object> payload = dictionary["payload"] as Dictionary<string, object>;
                            if (payload != null)
                            {
                                bool read = (payload["r"].ToString() == "1") ? true : false;
                                bool write = (payload["w"].ToString() == "1") ? true : false;
                                string level = payload["level"].ToString();
                                if (level == "subkey")
                                {
                                    switch (currentUnitTestCase)
                                    {
                                        case "ThenRevokeAtSubKeyLevelReturnSuccess":
                                            if (!read && !write) receivedRevokeMessage = true;
                                            break;
                                        case "ThenSubKeyLevelWithReadShouldReturnSuccess":
                                            //if (read && !write) receivedGrantMessage = true;
                                            break;
                                        case "ThenSubKeyLevelWithWriteShouldReturnSuccess":
                                            //if (!read && write) receivedGrantMessage = true;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                        }

                        //if (dictionary.
                        //if (status == "200")
                        //{
                        //    receivedGrantMessage = true;
                        //}
                    }
                    //var level = dictionary["level"].ToString();
                }
            }
            catch { }
            finally
            {
                revokeManualEvent.Set();
            }
        }

        void RevokeToChannelLevelCallback(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    ///object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
					object[] deserializedMessage = new JsonFXDotNet().DeserializeToListOfObject(receivedMessage).ToArray();
					Dictionary<string,object> dictionary = null;
					string currentChannel = "";
					if (deserializedMessage is object[])
					{
						dictionary = deserializedMessage[0] as Dictionary<string, object>;
                    	currentChannel = deserializedMessage[1].ToString();
					}
                    //JContainer dictionary = serializedMessage[0] as JContainer;
                    //string currentChannel = serializedMessage[1].ToString();
                    if (dictionary != null)
                    {
                        int statusCode = (int)dictionary["status"];
                        string statusMessage = dictionary["message"].ToString();
                        if (statusCode == 200 && statusMessage.ToLower() == "success")
                        {
                            Dictionary<string, object> payload = dictionary["payload"] as Dictionary<string, object>;
                            if (payload != null)
                            {
                                string level = payload["level"].ToString();
                                Dictionary<string, object> channels = payload["channels"] as Dictionary<string, object>;
                                if (channels != null)
                                {
                                    var channelContainer = (channels.ContainsKey(currentChannel)) ? channels[currentChannel] : null;
                                    if (channelContainer == null)
                                    {
                                        receivedRevokeMessage = true;
                                    }
                                }
                                else
                                {
                                    receivedRevokeMessage = true;
                                }
                            }
                        }
                    }
                }
            }
            catch {}
            finally
            {
                revokeManualEvent.Set();
            }
        }

        void RevokeToUserLevelCallback(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    //object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
					object[] deserializedMessage = new JsonFXDotNet().DeserializeToListOfObject(receivedMessage).ToArray();
					Dictionary<string,object> dictionary = null;
					string currentChannel = "";
					if (deserializedMessage is object[])
					{
						dictionary = deserializedMessage[0] as Dictionary<string, object>;
                    	currentChannel = deserializedMessage[1].ToString();
					}
                    //JContainer dictionary = serializedMessage[0] as JContainer;
                    //string currentChannel = serializedMessage[1].ToString();
                    if (dictionary != null)
                    {
                        int statusCode = (int)dictionary["status"];
                        string statusMessage = dictionary["message"].ToString();
                        if (statusCode == 200 && statusMessage.ToLower() == "success")
                        {
                            Dictionary<string, object> payload = dictionary["payload"] as Dictionary<string, object>;
                            if (payload != null)
                            {
                                string level = payload["level"].ToString();
                                string channel = payload["channel"].ToString();
                                Dictionary<string, object> auths = payload["auths"] as Dictionary<string, object>;
                                if (auths != null && auths.Count > 0)
                                {
                                    receivedRevokeMessage = true;
                                    foreach (string authKey in auths.Keys)
                                    {
                                        //if (auth is JProperty)
                                        //{
                                            //var authProperty = auth as JProperty;
                                            //if (authProperty != null)
                                            //{
                                                //string authKey = authProperty.Name;
                                                Dictionary<string, object> authKeyContainer = auths[authKey] as Dictionary<string, object>;
                                                if (authKeyContainer != null)
                                                {
                                                    receivedRevokeMessage = false;
                                                    break;
                                                }

                                            //}
                                        //}
                                    }
                                }
                                else
                                {
                                    receivedRevokeMessage = true;
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                revokeManualEvent.Set();
            }
        }

        private void DummyErrorCallback(PubnubClientError result)
        {
			Debug.Log(result.Description);
        }

    }
}
