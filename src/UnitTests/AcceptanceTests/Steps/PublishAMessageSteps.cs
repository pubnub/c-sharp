using System;
using TechTalk.SpecFlow;

namespace AcceptanceTests.Steps
{
    [Binding]
    public class FeaturePublishAMessageSteps
    {
        public FeaturePublishAMessageSteps() 
        { 
        
        }

        [Given(@"the demo keyset")]
        public void GivenTheDemoKeyset()
        {
            throw new PendingStepException();
        }

        [When(@"I publish a message")]
        public void WhenIPublishAMessage()
        {
            throw new PendingStepException();
        }

        [Then(@"I receive successful response")]
        public void ThenIReceiveSuccessfulResponse()
        {
            throw new PendingStepException();
        }

        [When(@"I publish a message with JSON metadata")]
        public void WhenIPublishAMessageWithJSONMetadata()
        {
            throw new PendingStepException();
        }

        [When(@"I publish a message with string metadata")]
        public void WhenIPublishAMessageWithStringMetadata()
        {
            throw new PendingStepException();
        }

        [Given(@"the invalid keyset")]
        public void GivenTheInvalidKeyset()
        {
            throw new PendingStepException();
        }

        [Then(@"I receive error response")]
        public void ThenIReceiveErrorResponse()
        {
            throw new PendingStepException();
        }
    }
}
