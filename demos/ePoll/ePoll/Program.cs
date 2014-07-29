using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ePoll.Types;
using ePoll.ePollManager;

namespace ePoll
{
    class Program
    {
        static PollQuestion pollQuestion;
        static string chosenAnswer = "";
        static string questionID = "";

        static void Main(string[] args)
        {
            System.Diagnostics.Stopwatch elapsedTimer = new System.Diagnostics.Stopwatch();
            elapsedTimer.Start();

            QuestionManager manager = new QuestionManager();
            pollQuestion = manager.GetActiveQuestion();

            questionID = pollQuestion.ID;

            //Console.WriteLine("Poll Question: {0}",pollQuestion.Question);
            //if (pollQuestion.AnswerGroup != null && pollQuestion.AnswerGroup.Count > 0)
            //{
            //    bool validOptionEntered = false;

            //    while (!validOptionEntered)
            //    {
            //        Console.WriteLine("Choose one option below. Enter the choice number:");
            //        for (int index = 0; index < pollQuestion.AnswerGroup.Count; index++)
            //        {
            //            Console.WriteLine("  {0}. {1}", index + 1, pollQuestion.AnswerGroup[index]);
            //        }
            //        string enteredChoice = Console.ReadLine();
            //        Console.WriteLine(enteredChoice);
            //        int answeredChoice;
            //        Int32.TryParse(enteredChoice, out answeredChoice);

            //        if (answeredChoice > 0 && answeredChoice <= pollQuestion.AnswerGroup.Count)
            //        {
            //            validOptionEntered = true;
            //            chosenAnswer = pollQuestion.AnswerGroup[answeredChoice - 1];
                        
            //            Console.WriteLine("Your response is {0}", chosenAnswer);
            //        }
            //    }
            //    if (validOptionEntered)
            //    {
            //        PollUserAnswer userAnswer = new PollUserAnswer();
            //        userAnswer.Question = pollQuestion.Question;
            //        userAnswer.QuestionID = pollQuestion.ID;
            //        userAnswer.UserAnswer = chosenAnswer;

            //        for (int index = 0; index < 100; index++)
            //        {
            //            manager.SaveAnswer(userAnswer);
            //        }
            //    }
            //}

            Console.WriteLine("Poll Results");
            List<string> pollAnswers = manager.GetPollResults(pollQuestion.ID);
            Console.WriteLine("Total Responses = {0}", pollAnswers.Count);

            IEnumerable<string> uniqueAnswers =  pollAnswers.Select(x => x).Distinct();
            Dictionary<string, double> dictionaryAnswer = new Dictionary<string, double>();

            foreach (string answer in uniqueAnswers)
            {
                int answerCount = pollAnswers.Where(s => s == answer).Count();
                double answeredPercent = Math.Round((double)answerCount / pollAnswers.Count,4);
                dictionaryAnswer.Add(answer, answeredPercent);
            }

            //string keyOfMaxValue = dictionaryAnswer.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;

            //double sumAnswer = dictionaryAnswer.Sum(x => x.Value);

            //double maxAnswer = dictionaryAnswer.Max(x => x.Value);
            //int numberOfSameMaxAnswer = dictionaryAnswer.Where(x => x.Value == maxAnswer).Count();
            //if (numberOfSameMaxAnswer > 1)
            //{
            //    var maxAnswerQuestions = dictionaryAnswer.Where(x => x.Value == maxAnswer);
            //    foreach (KeyValuePair<string, double> item in maxAnswerQuestions)
            //    {
            //    }
            //}
            //else
            //{
            //    double sumAnswerMinusMax = sumAnswer - maxAnswer;
            //    dictionaryAnswer[keyOfMaxValue] = 1 - sumAnswerMinusMax; //to avoid minor decimal difference for 100 percent
            //}
            dictionaryAnswer = dictionaryAnswer.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            foreach (KeyValuePair<string, double> item in dictionaryAnswer)
            {
                Console.WriteLine("{0} = {1} - {2}%", item.Key, pollAnswers.Where(s => s == item.Key).Count(), PercentFormat(item.Value));
            }


            elapsedTimer.Stop();
            Console.WriteLine("Elapsed time = {0}", elapsedTimer.Elapsed);
            Console.ReadLine();
        }

        private static double PercentFormat(double value)
        {
            return value * 100;
        }
    }
}
