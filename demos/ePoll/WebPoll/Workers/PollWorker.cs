using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebPoll.Workers
{
    using WebPoll.Models;
    
    using System.Threading;
    using System.Collections.Concurrent;
    using System.Xml;

    using PubNubMessaging.Core;

    public class PollWorker
    {
        private static Pubnub pubnub;

        static ConcurrentDictionary<string, ManualResetEvent> mrePresenceConnect = new ConcurrentDictionary<string, ManualResetEvent>();
        static ConcurrentDictionary<string, bool> presenceChannelConnected = new ConcurrentDictionary<string, bool>();


        static ConcurrentDictionary<string, ManualResetEvent> mrePublish = new ConcurrentDictionary<string, ManualResetEvent>();
        static ConcurrentDictionary<string, bool> messagePublished = new ConcurrentDictionary<string, bool>();

        static ConcurrentDictionary<string, ManualResetEvent> mreDetailedHistory = new ConcurrentDictionary<string, ManualResetEvent>();
        static ConcurrentDictionary<string, bool> detailedHistoryReceived = new ConcurrentDictionary<string, bool>();
        static ConcurrentDictionary<string, long> detailedHistoryStartTime = new ConcurrentDictionary<string, long>();
        static ConcurrentDictionary<string, List<string>> channelDetailedHistory = new ConcurrentDictionary<string, List<string>>();

        static PollWorker()
        {
            if (pubnub == null)
            {
                pubnub = new Pubnub("demo", "demo");
            }
        }

        public PollQuestion GetActiveQuestion()
        {
            PollQuestion ret = null;

            //For real-time applications, you can get data from databases like SQL Server
            string sampleXmlString = SampleData.GetActivePollQuestion();

            XmlDocument pollQuestionXmlDoc = new XmlDocument();
            pollQuestionXmlDoc.LoadXml(sampleXmlString);

            XmlElement root = pollQuestionXmlDoc.DocumentElement;

            XmlNode activeNode = pollQuestionXmlDoc.SelectSingleNode("//PollQuestion/Active");
            if (root.Name == "PollQuestion")
            {
                ret = new PollQuestion();
                ret.ID = pollQuestionXmlDoc.SelectSingleNode("//PollQuestion/ID").InnerText;
                ret.Question = pollQuestionXmlDoc.SelectSingleNode("//PollQuestion/Question").InnerText;
                string activeStatus = pollQuestionXmlDoc.SelectSingleNode("//PollQuestion/Active").InnerText;
                ret.Active = Boolean.Parse(activeStatus);

                XmlNodeList answerChoices = pollQuestionXmlDoc.SelectNodes("//PollQuestion/AnswerGroup/Answer");
                if (answerChoices != null && answerChoices.Count > 0)
                {
                    ret.AnswerGroup = new List<string>();
                    foreach (XmlNode answer in answerChoices)
                    {
                        ret.AnswerGroup.Add(answer.InnerText);
                    }
                }
            }


            return ret;
        }

        public bool SaveAnswer(PollUserAnswer answer)
        {
            bool ret = false;

            string pubnubChannel = answer.QuestionID;
            mrePublish.AddOrUpdate(pubnubChannel, new ManualResetEvent(false), (key, oldState) => new ManualResetEvent(false));
            messagePublished[pubnubChannel] = false;

            pubnub.Publish<string>(pubnubChannel, answer.UserAnswer, PollUserAnswerPublishRegularCallback, PollUserAnswerPublishErrorCallback);
            mrePublish[pubnubChannel].WaitOne(TimeSpan.FromSeconds(20));

            if (messagePublished[pubnubChannel])
            {
                ret = true;
            }

            return ret;
        }

        public List<string> GetPollResults(string questionID)
        {
            List<string> ret = null;

            string pubnubChannel = questionID;

            mreDetailedHistory.AddOrUpdate(pubnubChannel, new ManualResetEvent(false), (key, oldState) => new ManualResetEvent(false));
            detailedHistoryReceived.AddOrUpdate(pubnubChannel, false, (key, oldState) => false);
            detailedHistoryStartTime.AddOrUpdate(pubnubChannel, 0, (key, oldState) => 0);
            channelDetailedHistory.AddOrUpdate(pubnubChannel, new List<string>(), (key, oldState) => new List<string>());

            detailedHistoryReceived[pubnubChannel] = false;

            long currentTimetoken = Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds(DateTime.UtcNow);
            detailedHistoryStartTime[pubnubChannel] = 0; // currentTimetoken;

            while (!detailedHistoryReceived[pubnubChannel])
            {
                pubnub.DetailedHistory<string>(pubnubChannel, detailedHistoryStartTime[pubnubChannel], PollResultsRegularCallback, PollResultsErrorCallback, true);
                //mreDetailedHistory[pubnubChannel].WaitOne(TimeSpan.FromSeconds(10));
                mreDetailedHistory[pubnubChannel].WaitOne();
                if (!detailedHistoryReceived[pubnubChannel])
                {
                    mreDetailedHistory[pubnubChannel].Reset();
                }
            }

            ret = channelDetailedHistory[pubnubChannel];

            return ret;
        }

        public static bool ConfigurePollQuestionMonitor(PollQuestion pollQuestion)
        {
            bool ret = false;

            //Based on the design of the application, you can have unique id or name
            string pubnubChannel = pollQuestion.ID;

            mrePresenceConnect.AddOrUpdate(pubnubChannel, new ManualResetEvent(false), (key, oldState) => new ManualResetEvent(false));
            presenceChannelConnected[pubnubChannel] = false;

            pubnub.Presence<string>(pubnubChannel, PollQuestionMonitorRegularCallback, PollQuestionMonitorConfiguredCallback, PollQuestionMonitorErrorCallback);
            mrePresenceConnect[pubnubChannel].WaitOne(TimeSpan.FromSeconds(10));

            if (presenceChannelConnected[pubnubChannel])
            {
                ret = true;
            }

            return ret;
        }

        private static void PollQuestionMonitorConfiguredCallback(string connectStatus)
        {
            if (!string.IsNullOrEmpty(connectStatus) && !string.IsNullOrEmpty(connectStatus.Trim()))
            {
                object[] deserializedResult = pubnub.JsonPluggableLibrary.DeserializeToObject(connectStatus) as object[];
                if (deserializedResult is object[])
                {
                    int statusCode = Int32.Parse(deserializedResult[0].ToString());
                    string statusMessage = (string)deserializedResult[1];
                    string channel = (string)deserializedResult[2];
                    if (presenceChannelConnected.ContainsKey(channel))
                    {
                        presenceChannelConnected[channel] = true;
                        mrePresenceConnect[channel].Set();
                    }
                }
            }
        }

        private static void PollQuestionMonitorRegularCallback(string monitorData)
        {
        }

        private static void PollQuestionMonitorErrorCallback(PubnubClientError monitorError)
        {
        }

        private static void PollUserAnswerPublishRegularCallback(string publishResult)
        {
            if (!string.IsNullOrEmpty(publishResult) && !string.IsNullOrEmpty(publishResult.Trim()))
            {
                List<object> deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(publishResult) as List<object>;
                if (deserializedMessage != null && deserializedMessage.Count >= 3)
                {
                    long statusCode = Int64.Parse(deserializedMessage[0].ToString());
                    string statusMessage = (string)deserializedMessage[1];
                    string channelName = (string)deserializedMessage[3];
                    if (statusCode == 1 && statusMessage.ToLower() == "sent")
                    {
                        if (messagePublished.ContainsKey(channelName))
                        {
                            messagePublished[channelName] = true;
                        }
                    }
                    if (mrePublish.ContainsKey(channelName))
                    {
                        mrePublish[channelName].Set();
                    }
                }
            }
        }

        private static void PollUserAnswerPublishErrorCallback(PubnubClientError publishError)
        {
        }

        private static void PollResultsRegularCallback(string pollResults)
        {
            if (!string.IsNullOrEmpty(pollResults) && !string.IsNullOrEmpty(pollResults.Trim()))
            {
                List<object> deserializedContainer = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(pollResults) as List<object>;
                if (deserializedContainer != null && deserializedContainer.Count > 0)
                {
                    string channelName = deserializedContainer[3].ToString();

                    object deserializedMessage = deserializedContainer[0] as object;
                    List<object> messageList = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(deserializedMessage.ToString());
                    if (messageList != null && messageList.Count > 0)
                    {
                        List<string> listMessages = messageList.Cast<string>().ToList();
                        channelDetailedHistory[channelName].AddRange(listMessages);
                    }

                    if (detailedHistoryStartTime.ContainsKey(channelName))
                    {
                        long startTimetoken = Int64.Parse(deserializedContainer[1].ToString());
                        long endTimetoken = Int64.Parse(deserializedContainer[2].ToString());


                        if (detailedHistoryStartTime[channelName] == endTimetoken)
                        {
                            if (detailedHistoryReceived.ContainsKey(channelName))
                            {
                                detailedHistoryReceived[channelName] = true;
                            }
                        }
                        //else
                        //{
                        //    detailedHistoryStartTime[channelName] = startTimetoken;
                        //}

                        detailedHistoryStartTime[channelName] = endTimetoken;

                        if (detailedHistoryStartTime[channelName] == 0)
                        {
                            if (detailedHistoryReceived.ContainsKey(channelName))
                            {
                                detailedHistoryReceived[channelName] = true;
                            }
                        }
                    }


                    if (mreDetailedHistory.ContainsKey(channelName))
                    {
                        mreDetailedHistory[channelName].Set();
                    }
                }
            }
        }

        private static void PollResultsErrorCallback(PubnubClientError pollResultError)
        {
        }
    }
}