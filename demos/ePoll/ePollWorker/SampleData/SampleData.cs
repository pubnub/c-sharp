using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ePoll.ePollWorker
{
    public class SampleData
    {
        public static string GetActivePollQuestion()
        {
            return "<PollQuestion><ID>PUBNUB-POLL-001</ID><Question>Are you satisfied with your government policies?</Question><Active>true</Active><AnswerGroup><Answer>Yes</Answer><Answer>No</Answer><Answer>Don't Know</Answer></AnswerGroup></PollQuestion>";
        }
    }
}
