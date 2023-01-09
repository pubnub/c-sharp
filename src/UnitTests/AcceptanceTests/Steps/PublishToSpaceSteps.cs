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
        [When(@"I publish message with '([^']*)' space id and '([^']*)' message type")]
        public async Task WhenIPublishMessageWithSpaceIdAndMessageType(string p0, string p1)
        {
            PNResult<PNPublishResult> getPublishResponse = await pn.Publish()
                .Channel("my_channel")
                .Message("test message")
                .MessageType(new MessageType(p1))
                .SpaceId(p0)
                .ExecuteAsync();

            getPublishResult = getPublishResponse.Result;
            pnStatus = getPublishResponse.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }

        [Scope(Scenario = "Publish message to space success")]
        [Then(@"I receive a successful response")]
        public void ThenPublishToSpaceReceiveASuccessfulResponse()
        {
            Assert.IsTrue(!pnStatus.Error);
        }
    }
}
