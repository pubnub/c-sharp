using System;
using PubNubMessaging.Core;
using NUnit.Framework;
using System.ComponentModel;
using System.Collections.Generic;


namespace PubNubMessaging.Tests
{
  [TestFixture]
  public class WhenUnsubscribedToAChannel
  {
    [Test]
    public void ThenNonExistentChannelShouldReturnNotSubscribed()
    {
      Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);

      Common common = new Common();
      common.DeliveryStatus = false;
      common.Response = null;
      
      pubnub.PubnubUnitTest = common.CreateUnitTestInstance("WhenUnsubscribedToAChannel", "ThenNonExistentChannelShouldReturnNotSubscribed");
      
      string channel = "hello_world";
      
      pubnub.Unsubscribe<string>(channel, common.DisplayReturnMessageDummy, common.DisplayReturnMessageDummy, common.DisplayReturnMessageDummy, common.DisplayReturnMessage);
      common.WaitForResponse();

      Console.WriteLine("Response:" + common.Response);
      if (common.Response.ToString().ToLower().Contains ("not subscribed")) {
        Assert.Pass();
      }
      else
      {
        Assert.Fail();
      }
    }

    [Test]
    public void ThenShouldReturnUnsubscribedMessage()
    {
      Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
      
      Common common = new Common();
      common.DeliveryStatus = false;
      common.Response = null;
      
      pubnub.PubnubUnitTest = common.CreateUnitTestInstance("WhenUnsubscribedToAChannel", "ThenShouldReturnUnsubscribedMessage");
      
      string channel = "hello_world";

      pubnub.Subscribe<string>(channel, common.DisplayReturnMessageDummy, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

      common.WaitForResponse();
      common.DeliveryStatus = false;
      common.Response = null;

      pubnub.Unsubscribe<string>(channel, common.DisplayReturnMessageDummy, common.DisplayReturnMessageDummy, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);
      common.WaitForResponse();

      if (common.Response.ToString().Contains ("Unsubscribed from")) {
        Console.WriteLine("Response:" + common.Response);
        Assert.Pass();
      }
      else
      {
        Assert.Fail();
      }    
    }
  }
}

