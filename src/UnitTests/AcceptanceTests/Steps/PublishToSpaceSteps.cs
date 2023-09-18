using System;
using TechTalk.SpecFlow;
using PubnubApi;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Net;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace AcceptanceTests.Steps
{
    public partial class FeaturePublishMessageSteps
    {
        [When(@"I publish message with '([^']*)' space id and '([^']*)' type")]
        public async Task WhenIPublishMessageWithSpaceIdAndType(string p0, string p1)
        {
            PNResult<PNPublishResult> getPublishResponse = await pn.Publish()
                .Channel("my_channel")
                .Message("test message")
                .Type(p1)
                .SpaceId(p0)
                .ExecuteAsync();

            getPublishResult = getPublishResponse.Result;
            pnStatus = getPublishResponse.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }

        [Then(@"I receive a successful response")]
        public void ThenPublishToSpaceReceiveASuccessfulResponse()
        {
            Assert.IsTrue(!pnStatus.Error);
        }

        [Then(@"I receive an error response")]
        public void ThenIReceiveAnErrorResponse()
        {
            Assert.IsTrue(getPublishResult == null && pnStatus.Error);
        }
    }
}
