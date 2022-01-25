using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PubnubApi.EndPoint
{
    public class HandshakeCompletedEventArgs : EventArgs
    {
        public long Timetoken { get; set; }
        public int Region { get; set; }
        public Dictionary<string, object> HandshakeInputs { get; set; }
    }

    public class MessageReceivedEventArgs: EventArgs
    {
        public long Timetoken { get; set; }
        public int Region { get; set; }
        public object Message { get; set; }
        public Dictionary<string, object> ReceiveMessageInputs { get; set; }
    }
    public class StatelessSubscribeOperation
    {
        List<string> channelList = null;
        List<string> channelgroupList = null;
        //public delegate void HandshakeCompletedEventHandler(object sender, HandshakeCompletedEventArgs e);
        public bool LongPolling300Seconds = false;
        public StatelessSubscribeOperation Channels(List<string> channels)
        {
            channelList = channels;
            return this;
        }

        public StatelessSubscribeOperation ChannelGroups(List<string> channelGroups)
        {
            channelgroupList = channelGroups;
            return this;
        }

        //public object HandshakeCallbackDataState { get; set; }
        public void HandshakeDataCallback(object handshakeCallbackData, object handshakeInputs)
        {
            //HandshakeCallbackDataState
            System.Diagnostics.Debug.WriteLine("called HandshakeDataCallback. I got timetoken from handshakeCallbackData.");

            HandshakeCompletedEventArgs handshakeEventArgs = new HandshakeCompletedEventArgs();
            handshakeEventArgs.Timetoken = 16357771919061273;
            handshakeEventArgs.Region = 2;
            handshakeEventArgs.HandshakeInputs = handshakeInputs as Dictionary<string, object>;

            OnHandshakeCompleted(handshakeEventArgs);
        }

        /// <summary>
        /// Initial subscribe request to request server timtoken for the set of channels and channel-groups
        /// </summary>
        /// <param name="rawChannels">Array of channels</param>
        /// <param name="rawChannelGroups">Array of channel-groups</param>
        /// <param name="handshakeData">Callback data containing Handshake response/error</param>
        /// <returns>Cancellable Handshake token</returns>
        public async Task<CancellationTokenSource> Handshake(string[] rawChannels, string[] rawChannelGroups, Action<object, object> handshakeData)
        {
            CancellationTokenSource handShakeTokenSource = new CancellationTokenSource();

            System.Diagnostics.Debug.WriteLine("called Handshake");
            //Call REST API and get JSON response
            //Pass the json response to Utility method to parse and return model data.
            //Assume some json data = {"t":{"t":"16357771919061273","r":2},"m":[]} 
            string sampleJson = "{\"t\":{\"t\":\"16357771919061273\",\"r\":2},\"m\":[]}";

            //Return the response in the determined format
            object tbdDataFormatForHandshake  = await ParseAndReturnHandshakeData(sampleJson);

            Dictionary<string, object> dictCurrentInputs = new Dictionary<string, object>();
            dictCurrentInputs.Add("channels", rawChannels);
            dictCurrentInputs.Add("channel-groups", rawChannelGroups);

            //send response/error to callback
            handshakeData(tbdDataFormatForHandshake, dictCurrentInputs);
            //We can data either through callback or raise event handler

            //should we call receive messages from here?

            return handShakeTokenSource;
        }

        protected virtual void OnHandshakeCompleted(HandshakeCompletedEventArgs e)
        {
            EventHandler<HandshakeCompletedEventArgs> handler = HandshakeCompleted;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        public event EventHandler<HandshakeCompletedEventArgs> HandshakeCompleted;

        protected virtual void OnMessageReceived(MessageReceivedEventArgs e)
        {
            EventHandler<MessageReceivedEventArgs> handler = MessageReceiveCompleted;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        public event EventHandler<MessageReceivedEventArgs> MessageReceiveCompleted;

        public void ReceiveMessagesCallback(object receiveMessagesCallbackData, object receiveMessagesInputs)
        {
            //HandshakeCallbackDataState
            System.Diagnostics.Debug.WriteLine("called ReceiveMessagesCallback. I got message from receiveMessagesCallbackData.");

            MessageReceivedEventArgs messageReceivedEventArgs = new MessageReceivedEventArgs();
            messageReceivedEventArgs.Timetoken = 16357772585951145;
            messageReceivedEventArgs.Region = 2;
            messageReceivedEventArgs.ReceiveMessageInputs = receiveMessagesInputs as Dictionary<string, object>;

            OnMessageReceived(messageReceivedEventArgs);
        }
        public async Task<CancellationTokenSource> ReceiveMessages(string[] rawChannels, string[] rawChannelGroups, long timetoken, int region, Action<object, object> receiveMessagesData)
        {
            CancellationTokenSource receiveMessagesTokenSource = new CancellationTokenSource();

            System.Diagnostics.Debug.WriteLine("called ReceiveMessages");
            if (LongPolling300Seconds)
            {
#if NET35 || NET40
            await Task.Factory.StartNew(() => { });
#else
                await Task.Delay(30 * 1000); //Added this until this method gets await stmt.
#endif

            }
            //Call REST API and get JSON response
            //{"t":{"t":"16357772585951145","r":2},"m":[{"a":"3","f":512,"i":"pn-c4816def-7c54-4694-98d4-d11f6470f334","p":{"t":"16357772585899327","r":2},"k":"demo-36","c":"aaa","d":"hello test"}]}
            string sampleJson = "{\"t\":{\"t\":\"16357772585951145\",\"r\":2},\"m\":[{\"a\":\"3\",\"f\":512,\"i\":\"pn-c4816def-7c54-4694-98d4-d11f6470f334\",\"p\":{\"t\":\"16357772585899327\",\"r\":2},\"k\":\"demo-36\",\"c\":\"aaa\",\"d\":\"hello test\"}]}";
            //Pass the json response to Utility method to parse and return model data.

            //Return the response in the determined format
            object tbdDataFormatForReceiveMessages = await ParseAndReturnReceiveMessagesData(sampleJson);

            Dictionary<string, object> dictCurrentInputs = new Dictionary<string, object>();
            dictCurrentInputs.Add("channels", rawChannels);
            dictCurrentInputs.Add("channel-groups", rawChannelGroups);

            //send response/error to callback
            receiveMessagesData(tbdDataFormatForReceiveMessages, dictCurrentInputs);

            return receiveMessagesTokenSource;
        }

        public async Task<CancellationTokenSource> Execute()
        {
            //Hard coded data
            List<string> channelList = new List<string>() { "ch1", "ch2" };
            List<string> channelgroupList = new List<string>() { "cg1", "cg2" };

            return await Handshake(channelList.ToArray(), channelgroupList.ToArray(), HandshakeDataCallback);
        }


        //Utility methods
        public void CancelHandshake(CancellationTokenSource handshakeTokenSource)
        {
            System.Diagnostics.Debug.WriteLine("called CancelHandshake");
            handshakeTokenSource.Cancel();
        }

        public void CancelReceiveMessages(CancellationTokenSource receiveMessagesTokenSource)
        {
            System.Diagnostics.Debug.WriteLine("called CancelReceiveMessages");
            receiveMessagesTokenSource.Cancel();
        }

        public async Task<object> ParseAndReturnHandshakeData(string jsonData)
        {
#if NET35 || NET40
            await Task.Factory.StartNew(() => { });
#else
            await Task.Delay(1); //Added this until this method gets await stmt.
#endif

            System.Diagnostics.Debug.WriteLine("called ParseAndReturnHandshakeData");
            //Return the response in the determined format
            Dictionary<string, object> timeAndRegionDict = new Dictionary<string, object>();
            timeAndRegionDict.Add("t", 16357771919061273);
            timeAndRegionDict.Add("r", 2);

            Dictionary<string, object> tbdDataFormatForHandshake = new Dictionary<string, object>();
            tbdDataFormatForHandshake.Add("success", timeAndRegionDict);
            tbdDataFormatForHandshake.Add("error", false); //"Error Object Format to determinted"

            return tbdDataFormatForHandshake;
        }

        public async Task<object> ParseAndReturnReceiveMessagesData(string jsonData)
        {
#if NET35 || NET40
            await Task.Factory.StartNew(() => { });
#else
            await Task.Delay(1); //Added this until this method gets await stmt.
#endif
            System.Diagnostics.Debug.WriteLine("called ParseAndReturnReceiveMessagesData");
            //Return the response in the determined format

            return null;
        }
    }
}
