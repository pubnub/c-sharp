using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebPoll.Controllers
{
    using WebPoll.Models;
    using WebPoll.Workers;

    public class PollController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("Welcome");
        }

        public ActionResult Welcome()
        {
            PollWorker worker = new PollWorker();
            PollQuestion question = worker.GetActiveQuestion();
            if (question == null)
            {
                //return HttpNotFound;
            }
            return View(question);
        }

        [HttpPost]
        public ActionResult SaveAnswer()
        {
            if (Request.Form != null && Request.Form.Count > 0 && Request.Form["PollAnswer"] != null)
            {
                PollUserAnswer answer = new PollUserAnswer();
                answer.Question = Request.Form["Question"];
                answer.QuestionID = Request.Form["ID"];
                answer.UserAnswer = Request.Form["PollAnswer"];

                PollWorker worker = new PollWorker();
                bool saveStatus = worker.SaveAnswer(answer);

                ViewData["PollAnswerSaveStatus"] = saveStatus;
                ViewData["ID"] = answer.QuestionID;
            }

            return View();
        }

        [HttpPost]
        public ActionResult PollResult()
        {
            if (Request.Form != null && Request.Form.Count > 0 && Request.Form["ID"] != null)
            {
                PollWorker worker = new PollWorker();
                string questionID = Request.Form["ID"].ToString();
                List<string> pollAnswers = worker.GetPollResults(questionID);

                if (pollAnswers != null && pollAnswers.Count > 0)
                {
                    ViewData["PollAnswer"] = pollAnswers;

                    IEnumerable<string> uniqueAnswers = pollAnswers.Select(x => x).Distinct();
                    Dictionary<string, double> dictionaryAnswerCount = new Dictionary<string, double>();

                    foreach (string answer in uniqueAnswers)
                    {
                        int answerCount = pollAnswers.Where(s => s == answer).Count();
                        double answeredPercent = Math.Round((double)answerCount / pollAnswers.Count, 4);
                        dictionaryAnswerCount.Add(answer, answeredPercent);
                    }
                    dictionaryAnswerCount = dictionaryAnswerCount.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

                    ViewData["ID"] = questionID;
                    ViewData["AnswerCount"] = dictionaryAnswerCount;
                }
            }
            return View();
        }
    }
}
