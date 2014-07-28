using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebPoll.Models
{

    public class PollQuestion
    {
        public string ID;
        public string Question;
        public bool Active;
        public List<string> AnswerGroup;
    }

    public class PollUserAnswer
    {
        public string QuestionID;
        public string Question;
        public string UserAnswer;
    }

    public class SampleData
    {
        public static string GetActivePollQuestion()
        {
            return "<PollQuestion><ID>PUBNUB-POLL-001</ID><Question>Are you satisfied with your government policies?</Question><Active>true</Active><AnswerGroup><Answer>Yes</Answer><Answer>No</Answer><Answer>Don't Know</Answer></AnswerGroup></PollQuestion>";
        }
    }
}