using UnityEngine;
using PubnubApi;
using System.Xml;
using System.Collections.Generic;
using System;
//using System.Reflection;

namespace PubNubAPI.Tests
{

    class PubnubDemoObject
    {
        public double VersionID {get; set;} 
        public long Timetoken {get; set;} 
        public string OperationName {get; set;}
        public string[] Channels {get; set;}
        public PubnubDemoMessage DemoMessage {get; set;}
        public PubnubDemoMessage CustomMessage {get; set;}
        public XmlDocument SampleXml {get; set;}

        public PubnubDemoObject(){
            VersionID = 3.4;
            Timetoken = 13601488652764619;
            OperationName = "Publish";
            Channels = new string[]{ "ch1" };
            DemoMessage = new PubnubDemoMessage ();
            CustomMessage = new PubnubDemoMessage ("This is a demo message");
            SampleXml = new PubnubDemoMessage ().TryXmlDemo ();
        }
        
    }

    class PubnubDemoMessage
    {
        public string DefaultMessage = "~!@#$%^&*()_+ `1234567890-= qwertyuiop[]\\ {}| asdfghjkl;' :\" zxcvbnm,./ <>? ";
        public PubnubDemoMessage ()
        {
        }

        public PubnubDemoMessage (string message)
        {
            DefaultMessage = message;
        }

        public XmlDocument TryXmlDemo ()
        {
            XmlDocument xmlDocument = new XmlDocument ();
            xmlDocument.LoadXml ("<DemoRoot><Person ID='ABCD123'><Name><First>John</First><Middle>P.</Middle><Last>Doe</Last></Name><Address><Street>123 Duck Street</Street><City>New City</City><State>New York</State><Country>United States</Country></Address></Person><Person ID='ABCD456'><Name><First>Peter</First><Middle>Z.</Middle><Last>Smith</Last></Name><Address><Street>12 Hollow Street</Street><City>Philadelphia</City><State>Pennsylvania</State><Country>United States</Country></Address></Person></DemoRoot>");

            return xmlDocument;
        }
    }

    public class PlayModeCommon {
		public static bool SslOn = true;
		public static bool CipherOn = false;
		public static string Origin = "ps.pndsn.com";
        public static string PublishKey = "pub-c-38994634-9e05-4967-bc66-2ac2cef65ed9"; //"pub-c-cdea0ef1-c571-4b72-b43f-ff1dc8aa4c5d";
        public static string SubscribeKey = "sub-c-c9710928-1b7a-11e3-a0c8-02ee2ddab7fe"; //"sub-c-4757f09c-c3f2-11e9-9d00-8a58a5558306";
        public static string SecretKey = "sec-c-ZDkzZTBkOTEtNTQxZS00MmQ3LTljMWUtMTNiNGZjNWUwMTVk";
        public static string cg1 = "channelGroup1";
        public static string cg2 = "channelGroup2";
        public static string ch1 = "channel1";
        public static string ch2 = "channel2";

        public static int WaitTimeForAsyncResponse = 2;
        public static int WaitTimeBetweenCalls1 = 1;
        public static int WaitTimeBetweenCalls = 2;
        public static int WaitTimeBetweenCalls2 = 3;
        public static int WaitTimeBetweenCalls3 = 3;
        public static int WaitTimeBetweenCalls4 = 4;
        public static int WaitTimeBetweenCalls5 = 4;


        public static PNConfiguration SetPNConfig(bool useCipher){
            PNConfiguration pnConfiguration = new PNConfiguration ();
            pnConfiguration.Origin = Origin;
            pnConfiguration.SubscribeKey = SubscribeKey;
            pnConfiguration.PublishKey = PublishKey;
            pnConfiguration.SecretKey = SecretKey;
            if(useCipher){
                pnConfiguration.CipherKey = "enigma";
            }
            pnConfiguration.LogVerbosity = PNLogVerbosity.BODY; 
            //pnConfiguration.PresenceTimeout = 60;
            //pnConfiguration.SetPresenceTimeoutWithCustomInterval(60, 30);
            pnConfiguration.Secure = SslOn;
            pnConfiguration.ReconnectionPolicy = PNReconnectionPolicy.LINEAR;
            return pnConfiguration;
        }
    }
}