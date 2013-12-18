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
    public class WhenUnsubscribedToAChannel: UUnitTestCase
    {
        ManualResetEvent meNotSubscribed = new ManualResetEvent(false);
        ManualResetEvent meChannelSubscribed = new ManualResetEvent(false);
        ManualResetEvent meChannelUnsubscribed = new ManualResetEvent(false);

        bool receivedNotSubscribedMessage = false;
        bool receivedUnsubscribedMessage = false;
        bool receivedChannelConnectedMessage = false;

        [UUnitTest]
        public void ThenNoExistChannelShouldReturnNotSubscribed()
        {
            Debug.Log("Running ThenNoExistChannelShouldReturnNotSubscribed()");
            receivedNotSubscribedMessage = false;
            Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);

            string channel = "hello_my_channel";

            pubnub.Unsubscribe<string>(channel, DummyMethodNoExistChannelUnsubscribeChannelUserCallback, DummyMethodNoExistChannelUnsubscribeChannelConnectCallback, DummyMethodNoExistChannelUnsubscribeChannelDisconnectCallback1, DummyErrorCallback);

            meNotSubscribed.WaitOne();

            pubnub.EndPendingRequests();

            UUnitAssert.True(receivedNotSubscribedMessage, "WhenUnsubscribedToAChannel --> ThenNoExistChannelShouldReturnNotSubscribed Failed");
        }

        [UUnitTest]
        public void ThenShouldReturnUnsubscribedMessage()
        {
            Debug.Log("Running ThenShouldReturnUnsubscribedMessage()");
            receivedChannelConnectedMessage = false;
            receivedUnsubscribedMessage = false;

            Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenUnsubscribedToAChannel";
            unitTest.TestCaseName = "ThenShouldReturnUnsubscribedMessage";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            pubnub.Subscribe<string>(channel, DummyMethodChannelSubscribeUserCallback, DummyMethodChannelSubscribeConnectCallback, DummyErrorCallback);
            meChannelSubscribed.WaitOne();

            if (receivedChannelConnectedMessage)
            {
                pubnub.Unsubscribe<string>(channel, DummyMethodUnsubscribeChannelUserCallback, DummyMethodUnsubscribeChannelConnectCallback, DummyMethodUnsubscribeChannelDisconnectCallback, DummyErrorCallback);
                meChannelUnsubscribed.WaitOne();
            }

            pubnub.EndPendingRequests();

            UUnitAssert.True(receivedUnsubscribedMessage, "WhenUnsubscribedToAChannel --> ThenShouldReturnUnsubscribedMessage Failed");
        }

        private void DummyMethodChannelSubscribeUserCallback(string result)
        {
        }

        private void DummyMethodChannelSubscribeConnectCallback(string result)
        {
            if (result.Contains("Connected"))
            {
                receivedChannelConnectedMessage = true;
            }
            meChannelSubscribed.Set();
        }

        private void DummyMethodUnsubscribeChannelUserCallback(string result)
        {
        }

        private void DummyMethodUnsubscribeChannelConnectCallback(string result)
        {
        }

        private void DummyMethodUnsubscribeChannelDisconnectCallback(string result)
        {
            if (result.Contains("Unsubscribed from"))
            {
                receivedUnsubscribedMessage = true;
            }
            meChannelUnsubscribed.Set();
        }

        private void DummyMethodNoExistChannelUnsubscribeChannelUserCallback(string result)
        {
            if (result.Contains("not subscribed"))
            {
                receivedNotSubscribedMessage = true;
            }
            meNotSubscribed.Set();
        }

        private void DummyMethodNoExistChannelUnsubscribeChannelConnectCallback(string result)
        {
        }

        private void DummyMethodNoExistChannelUnsubscribeChannelDisconnectCallback1(string result)
        {
        }

        void DummyErrorCallback(string result)
        {
            Debug.Log("WhenUnsubscribedToAChannel ErrorCallback = " + result);
        }
    }
}
