using NUnit.Framework;
using PubnubApi;
using System;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace AcceptanceTests.Steps
{
    public partial class FeatureAccessSteps
    {
        PNAccessManagerRevokeTokenResult revokeResult = null;

        [Given(@"a token")]
        public void GivenAToken()
        {
            //Token created on  09 Nov 2021 with 30 days validity.
            tokenInput = "qEF2AkF0GmGKZRtDdHRsGajAQ3Jlc6VEY2hhbqFhYRj_Q2dycKFhYhj_Q3VzcqBDc3BjoER1dWlkoWFiGP9DcGF0pURjaGFuoENncnCgQ3VzcqBDc3BjoER1dWlkoERtZXRho2VzY29yZRhkZWNvbG9yY3JlZGZhdXRob3JlcGFuZHVEdXVpZGFkQ3NpZ1gg1yPq0K5_N6qKbC-o4QAtOnj2CxCTOaEysrWVBVLhbSs=";
        }

        [Given(@"the token string '(.*)'")]
        public void GivenTheTokenString(string p0)
        {
            tokenInput = p0;
        }

        [When(@"I revoke a token")]
        public async Task WhenIRevokeAToken()
        {
            PNResult<PNAccessManagerRevokeTokenResult> revokeTokenResult = await pn.RevokeToken()
                .Token(tokenInput)
                .ExecuteAsync();
            revokeResult = revokeTokenResult.Result;
            pnStatus = revokeTokenResult.Status;
            if (pnStatus != null && pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }

        [Then(@"I get confirmation that token has been revoked")]
        public void ThenIGetConfirmationThatTokenHasBeenRevoked()
        {
            if (betaVersion && revokeResult == null)
            {
                Assert.Ignore();
            }
            else
            {
                Assert.IsTrue(revokeResult != null);
            }
        }

        [Then(@"the error detail message is not empty")]
        public void ThenTheErrorDetailMessageIsNotEmpty()
        {
            if (pnError != null && pnError.error.details.Count > 0)
            {
                Assert.IsTrue(!string.IsNullOrEmpty(pnError.error.details[0].message));
            }
            else
            {
                Assert.Fail();
            }
        }

        [Then(@"the error service is '(.*)'")]
        public void ThenTheErrorServiceIs(string p0)
        {
            Assert.AreEqual(p0, pnError.service);
        }

    }
}
