using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using ePoll;
using ePoll.ePollWorker;
using ePoll.Types;

namespace ePoll.UnitTests
{
    [TestFixture]
    public class GetPollQuestion
    {
        [Test]
        public void GetsActivePollQuestion()
        {
            ePoll.Types.PollQuestion activeQuestion;
            ePollWorker.QuestionWorker question = new QuestionWorker();
            activeQuestion = question.GetActiveQuestion();
            if (activeQuestion != null)
            {
                Assert.IsTrue(activeQuestion.Active, "No Active Question");
            }
            else
            {
                Assert.Fail("Poll Question is null");
            }
        }
    }
}
