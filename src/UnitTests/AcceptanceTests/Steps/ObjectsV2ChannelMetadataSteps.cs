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
    public partial class FeatureObjectsV2MetadataSteps
    {
        private ChannelMetadataPersona channelMetadataPersona = null;
        private PNGetChannelMetadataResult getChannelMetadataResult = null;
        private PNSetChannelMetadataResult setChannelMetadataResult = null;
        private PNGetAllChannelMetadataResult getAllChannelMetadataResult = null;

        internal class ChannelMetadataPersona
        {
            public string name { get; set; }
            public string id { get; set; }
            public string description { get; set; }
            public string updated { get; set; }
            public string eTag { get; set; }
            public Dictionary<string, object> custom {get; set;}
        }

        [Given(@"the id for '([^']*)' channel")]
        public void GivenTheIdForChannel(string personaName)
        {
            if (personaName == null) return;
            channelMetadataPersona = null;
            string dirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string personaFile = string.Format("{0}.json", personaName.Trim().ToLower());
            var personaFilePath = Path.Combine(dirPath ?? "", "Data", personaFile);
            if (File.Exists(personaFilePath))
            {
                using (StreamReader r = new StreamReader(personaFilePath))
                {
                    string json = r.ReadToEnd();
                    channelMetadataPersona = JsonSerializer.Deserialize<ChannelMetadataPersona>(json, new JsonSerializerOptions { });
                }
            }
        }

        [When(@"I get the channel metadata")]
        public async Task WhenIGetTheChannelMetadata()
        {
            PNResult<PNGetChannelMetadataResult> getChannelMetadataResponse = await pn.GetChannelMetadata()
                .Channel(channelMetadataPersona.id)
                .ExecuteAsync();
            getChannelMetadataResult = getChannelMetadataResponse.Result;
            pnStatus = getChannelMetadataResponse.Status;
            if (pnStatus != null && pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }

        [Then(@"the channel metadata for '([^']*)' channel")]
        public void ThenTheChannelMetadataForChannel(string personaName)
        {
            Assert.AreEqual(channelMetadataPersona.name, getChannelMetadataResult.Name);
            Assert.AreEqual(channelMetadataPersona.id, getChannelMetadataResult.Channel);
            Assert.AreEqual(channelMetadataPersona.description, getChannelMetadataResult.Description);
            Assert.AreEqual(channelMetadataPersona.updated, getChannelMetadataResult.Updated);
            
            if (string.Compare(personaName, "chat", true) == 0)
            {
                Assert.AreEqual(channelMetadataPersona.custom, getChannelMetadataResult.Custom);
            }
            else if (string.Compare(personaName, "dm", true) == 0)
            {
                Assert.AreEqual(channelMetadataPersona.custom.Count, getChannelMetadataResult.Custom.Count);
            }
            else
            {
                Assert.Fail("TEST MISMATCH", personaName);
            }
        }

        [When(@"I get the channel metadata with custom")]
        public async Task WhenIGetTheChannelMetadataWithCustom()
        {
            PNResult<PNGetChannelMetadataResult> getChannelMetadataResponse = await pn.GetChannelMetadata()
                .Channel(channelMetadataPersona.id)
                .IncludeCustom(true)
                .ExecuteAsync();
            getChannelMetadataResult = getChannelMetadataResponse.Result;
            pnStatus = getChannelMetadataResponse.Status;
            if (pnStatus != null && pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }

        [Given(@"the data for '([^']*)' channel")]
        public void GivenTheDataForChannel(string personaName)
        {
            if (personaName == null) return;
            channelMetadataPersona = null;
            string dirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string personaFile = string.Format("{0}.json", personaName.Trim().ToLower());
            var personaFilePath = Path.Combine(dirPath, "Data", personaFile);
            if (File.Exists(personaFilePath))
            {
                using (StreamReader r = new StreamReader(personaFilePath))
                {
                    string json = r.ReadToEnd();
                    channelMetadataPersona = JsonSerializer.Deserialize<ChannelMetadataPersona>(json, new JsonSerializerOptions { });
                }
            }
        }

        [When(@"I set the channel metadata")]
        public async Task WhenISetTheChannelMetadata()
        {
            PNResult<PNSetChannelMetadataResult> setChannelMetadataResponse = await pn.SetChannelMetadata()
                .Channel(channelMetadataPersona.id)
                .Name(channelMetadataPersona.name)
                .Description(channelMetadataPersona.description)
                .ExecuteAsync();
            setChannelMetadataResult = setChannelMetadataResponse.Result;
            pnStatus = setChannelMetadataResponse.Status;
            if (pnStatus != null && pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }

        }

        [Then(@"the channel metadata for '([^']*)' channel contains updated")]
        public void ThenTheChannelMetadataForChannelContainsUpdated(string personaName)
        {
            Assert.AreEqual(channelMetadataPersona.name, setChannelMetadataResult.Name);
            Assert.AreEqual(channelMetadataPersona.id, setChannelMetadataResult.Channel);
            Assert.AreEqual(channelMetadataPersona.description, setChannelMetadataResult.Description);
            Assert.AreEqual(channelMetadataPersona.updated, setChannelMetadataResult.Updated);

            if (string.Compare(personaName, "chat", true) == 0)
            {
                Assert.AreEqual(channelMetadataPersona.custom, setChannelMetadataResult.Custom);
            }
            else
            {
                Assert.Fail("TEST MISMATCH", personaName);
            }
        }

        [When(@"I remove the channel metadata")]
        public async Task WhenIRemoveTheChannelMetadata()
        {
            PNResult<PNRemoveChannelMetadataResult> removeChannelMetadataResponse = await pn.RemoveChannelMetadata()
                .Channel(channelMetadataPersona.id)
                .ExecuteAsync();
            pnStatus = removeChannelMetadataResponse.Status;
            if (pnStatus != null && pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }

        }

        [When(@"I get all channel metadata")]
        public async Task WhenIGetAllChannelMetadata()
        {
            getAllChannelMetadataResult = null;
            PNResult<PNGetAllChannelMetadataResult> getAllChannelMetadataResponse = await pn.GetAllChannelMetadata()
                .ExecuteAsync();
            getAllChannelMetadataResult = getAllChannelMetadataResponse.Result;
            pnStatus = getAllChannelMetadataResponse.Status;
            if (pnStatus != null && pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }

        [Then(@"the response contains list with '([^']*)' and '([^']*)' channel metadata")]
        public void ThenTheResponseContainsListWithAndChannelMetadata(string personaName1, string personaName2)
        {
            if (personaName1 == null || personaName2 == null) return;
            string dirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string personaFile1 = string.Format("{0}.json", personaName1.Trim().ToLower());
            string personaFile2 = string.Format("{0}.json", personaName2.Trim().ToLower());

            var personaFile1Path = Path.Combine(dirPath ?? "", "Data", personaFile1);
            var personaFile2Path = Path.Combine(dirPath ?? "", "Data", personaFile2);
            List<ChannelMetadataPersona> personaList = new List<ChannelMetadataPersona>();
            if (File.Exists(personaFile1Path) && File.Exists(personaFile2Path))
            {
                using (StreamReader r = new StreamReader(personaFile1Path))
                {
                    string json = r.ReadToEnd();
                    channelMetadataPersona = JsonSerializer.Deserialize<ChannelMetadataPersona>(json, new JsonSerializerOptions { });
                    personaList.Add(channelMetadataPersona);
                }
                using (StreamReader r = new StreamReader(personaFile2Path))
                {
                    string json = r.ReadToEnd();
                    channelMetadataPersona = JsonSerializer.Deserialize<ChannelMetadataPersona>(json, new JsonSerializerOptions { });
                    personaList.Add(channelMetadataPersona);
                }
            }

            if (
                (string.Compare(getAllChannelMetadataResult.Channels[0].Channel, personaList[0].id) == 0
                    && string.Compare(getAllChannelMetadataResult.Channels[1].Channel, personaList[1].id) == 0) 
                    ||
                (string.Compare(getAllChannelMetadataResult.Channels[0].Channel, personaList[1].id) == 0
                    && string.Compare(getAllChannelMetadataResult.Channels[1].Channel, personaList[0].id) == 0)
                )
            {
                Assert.True(true);
            }
            else
            {
                if (betaVersion) { Assert.True(true); } else { Assert.Fail(); }
            }
        }

        [When(@"I get all channel metadata with custom")]
        public async Task WhenIGetAllChannelMetadataWithCustom()
        {
            getAllChannelMetadataResult = null;
            PNResult<PNGetAllChannelMetadataResult> getAllChannelMetadataResponse = await pn.GetAllChannelMetadata()
                .IncludeCustom(true)
                .ExecuteAsync();
            getAllChannelMetadataResult = getAllChannelMetadataResponse.Result;
            pnStatus = getAllChannelMetadataResponse.Status;
            if (pnStatus != null && pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }

    }
}
