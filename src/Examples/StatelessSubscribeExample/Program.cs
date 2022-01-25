using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PubnubApi;
using PubnubApi.EndPoint;

namespace StatelessSubscribeExample
{
    class Program
    {
        static Pubnub pubnub = null;

        static public void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static public async Task MainAsync()
        {
            pubnub = new Pubnub(null);

            List<string> channelList = new List<string>() { "ch1", "ch2" };
            List<string> channelgroupList = new List<string>() { "cg1", "cg2" };

            Console.WriteLine("DEMO STARTS NOW");
            Console.WriteLine("PRESS H TO CALL Handshake");
            Console.WriteLine("PRESS M TO CALL ReceiveMessages");
            Console.WriteLine("PRESS P TO CALL ReceiveMessages with LongPolling");
            Console.WriteLine("PRESS ESCAPE KEY TO EXIT");
            bool exitFlag = false;
            while (!exitFlag)
            {
                ConsoleKey enteredKey = Console.ReadKey().Key;// != ConsoleKey.Escape
                if (enteredKey == ConsoleKey.Escape)
                {
                    exitFlag = true;
                    break;
                }
                Console.WriteLine();
                //System.Threading.Thread.Sleep(2000);
                PubnubApi.EndPoint.StatelessSubscribeOperation statelessSubscribeOp = new PubnubApi.EndPoint.StatelessSubscribeOperation();
                CancellationTokenSource hankshakeCancellationToken = null;
                CancellationTokenSource receiveMessagesCancellationToken = null;
                long lastTimetoken = 0;
                int lastRegion = 0;
                switch (enteredKey)
                {
                    case ConsoleKey.H:
                        statelessSubscribeOp.HandshakeCompleted += delegate(object sender, HandshakeCompletedEventArgs e) 
                        {
                            Console.WriteLine(string.Format("HandshakeReceived: timetoken={0}; region={1}", e.Timetoken, e.Region));
                            lastTimetoken = e.Timetoken;
                            lastRegion = e.Region;
                        };
                        hankshakeCancellationToken = await statelessSubscribeOp.Handshake(channelList.ToArray(), channelgroupList.ToArray(), statelessSubscribeOp.HandshakeDataCallback).ConfigureAwait(false);
                        break;
                    case ConsoleKey.M:
                        statelessSubscribeOp.MessageReceiveCompleted += delegate(object sender, PubnubApi.EndPoint.MessageReceivedEventArgs e) 
                        {
                            Console.WriteLine(string.Format("MessageReceiveCompleted: timetoken={0}; region={1}", e.Timetoken, e.Region));
                            lastTimetoken = e.Timetoken;
                            lastRegion = e.Region;
                        };
                        statelessSubscribeOp.LongPolling300Seconds = false;
                        receiveMessagesCancellationToken = await statelessSubscribeOp.ReceiveMessages(channelList.ToArray(), channelgroupList.ToArray(), lastTimetoken, lastRegion, statelessSubscribeOp.ReceiveMessagesCallback).ConfigureAwait(false);
                        break;
                    case ConsoleKey.P:
                        statelessSubscribeOp.MessageReceiveCompleted += delegate (object sender, PubnubApi.EndPoint.MessageReceivedEventArgs e)
                        {
                            Console.WriteLine(string.Format("MessageReceiveCompleted: timetoken={0}; region={1}", e.Timetoken, e.Region));
                            lastTimetoken = e.Timetoken;
                            lastRegion = e.Region;
                        };
                        new Thread(async() => {
                            statelessSubscribeOp.LongPolling300Seconds = true;
                            receiveMessagesCancellationToken = await statelessSubscribeOp.ReceiveMessages(channelList.ToArray(), channelgroupList.ToArray(), lastTimetoken, lastRegion, statelessSubscribeOp.ReceiveMessagesCallback).ConfigureAwait(false);
                        })
                        { IsBackground = true }.Start();
                        break;
                    case ConsoleKey.C:
                        if (receiveMessagesCancellationToken != null)
                        {
                            statelessSubscribeOp.CancelReceiveMessages(receiveMessagesCancellationToken);
                        }
                        else
                        {
                            Console.WriteLine("Cancellation token is null");
                        }
                        break;
                    case default(ConsoleKey):
                        break;
                }
                Console.WriteLine();
                Console.WriteLine("PRESS H TO CALL Handshake");
                Console.WriteLine("PRESS M TO CALL ReceiveMessages");
                Console.WriteLine("PRESS P TO CALL ReceiveMessages with LongPolling");
                Console.WriteLine("PRESS C TO CALL CancelReceiveMessages");
                Console.WriteLine("PRESS ESCAPE KEY TO EXIT");
            }
        }

        private static void StatelessSubscribeOp_HandshakeCompleted(object sender, HandshakeCompletedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("HandshakeReceived: timetoken={0}; region={1}", e.Timetoken, e.Region));
        }
    }
}
