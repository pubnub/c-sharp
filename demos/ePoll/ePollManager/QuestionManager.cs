using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ePoll.Types;
using ePoll.ePollWorker;

namespace ePoll.ePollManager
{
    public class QuestionManager
    {
        public PollQuestion GetActiveQuestion()
        {
            QuestionWorker questionWorker = new QuestionWorker();
            return questionWorker.GetActiveQuestion();
        }

        public bool ConfigurePollMonitor()
        {
            bool ret = false;

            QuestionWorker questionWorker = new QuestionWorker();

            return ret;
        }

        public bool SaveAnswer(PollUserAnswer answer)
        {
            bool ret = false;

            QuestionWorker worker = new QuestionWorker();
            ret = worker.SaveAnswer(answer);

            return ret;
        }

        public List<string> GetPollResults(string questionID)
        {
            List<string> ret = null;

            QuestionWorker worker = new QuestionWorker();
            ret = worker.GetPollResults(questionID);

            return ret;
        }
    }
}
