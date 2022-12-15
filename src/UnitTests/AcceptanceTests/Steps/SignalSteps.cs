using System;
using TechTalk.SpecFlow;

namespace AcceptanceTests.Steps
{
    [Binding]
    public class SignalStepDefinitions
    {
        [When(@"I send a signal")]
        public void WhenISendASignal()
        {
            throw new PendingStepException();
        }
    }
}
