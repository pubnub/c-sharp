using System;
using TechTalk.SpecFlow;

namespace AcceptanceTests.Steps
{
    public partial class FeaturePublishMessageSteps
    {
        [When(@"I publish message with '([^']*)' space id and '([^']*)' message type")]
        public void WhenIPublishMessageWithSpaceIdAndMessageType(string p0, string p1)
        {
            throw new PendingStepException();
        }

        //[When(@"I publish message with '([^']*)' space id and '([^']*)' message type")]
        //public void WhenIPublishMessageWithSpaceIdAndMessageType(string p0, string ts)
        //{
        //    throw new PendingStepException();
        //}

        //[When(@"I publish message with '([^']*)' space id and '([^']*)' message type")]
        //public void WhenIPublishMessageWithSpaceIdAndMessageType(string ts, string p1)
        //{
        //    throw new PendingStepException();
        //}
    }
}
