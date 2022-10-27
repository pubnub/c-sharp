using System;
using TechTalk.SpecFlow;

namespace AcceptanceTests.Steps
{
    public partial class FeatureObjectsV2MetadataSteps
    {
        [When(@"I get the memberships")]
        public void WhenIGetTheMemberships()
        {
            throw new PendingStepException();
        }

        [Then(@"the response contains list with '([^']*)' and '([^']*)' memberships")]
        public void ThenTheResponseContainsListWithAndMemberships(string chatMembership, string patientMembership)
        {
            throw new PendingStepException();
        }

        [When(@"I get the memberships for current user")]
        public void WhenIGetTheMembershipsForCurrentUser()
        {
            throw new PendingStepException();
        }

        [When(@"I get the memberships including custom and channel custom information")]
        public void WhenIGetTheMembershipsIncludingCustomAndChannelCustomInformation()
        {
            throw new PendingStepException();
        }

        //[Then(@"the response contains list with '([^']*)' and '([^']*)' memberships")]
        //public void ThenTheResponseContainsListWithAndMemberships(string vipChatMembership, string dMMembership)
        //{
        //    throw new PendingStepException();
        //}

        [Given(@"the data for '([^']*)' membership")]
        public void GivenTheDataForMembership(string chatMembership)
        {
            throw new PendingStepException();
        }

        [When(@"I set the membership")]
        public void WhenISetTheMembership()
        {
            throw new PendingStepException();
        }

        [Then(@"the response contains list with '([^']*)' membership")]
        public void ThenTheResponseContainsListWithMembership(string chatMembership)
        {
            throw new PendingStepException();
        }

        [When(@"I set the membership for current user")]
        public void WhenISetTheMembershipForCurrentUser()
        {
            throw new PendingStepException();
        }

        [When(@"I remove the membership")]
        public void WhenIRemoveTheMembership()
        {
            throw new PendingStepException();
        }

        [When(@"I remove the membership for current user")]
        public void WhenIRemoveTheMembershipForCurrentUser()
        {
            throw new PendingStepException();
        }
    }
}
