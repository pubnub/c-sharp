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
        [When(@"I send a signal")]
        public async Task WhenISendASignal()
        {
            PNResult<PNPublishResult> getPublishResponse = await pn.Signal()
                .Channel("my_channel")
                .Message("test signal message")
                .ExecuteAsync();

            getPublishResult = getPublishResponse.Result;
            pnStatus = getPublishResponse.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }
    }
}
