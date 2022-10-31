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
        PNMembershipsResult getMembershipsMetadataResult;
        ChannelMembershipMetadataLocal channelMembershipMetadata;
        ChannelMembershipMetadataLocal removeChannelMembershipMetadata;
        PNMembershipsResult setMembershipsResult;

        internal class ChannelMembershipMetadataLocal
        {
            public ChannelMetadataPersona channel { get; set; }
            public string updated { get; set; }
            public string eTag { get; set; }
            public Dictionary<string, object> custom { get; set; }
        }


        [When(@"I get the memberships")]
        public async Task WhenIGetTheMemberships()
        {
            PNResult<PNMembershipsResult> getMembershipsResponse = await pn.GetMemberships()
                .Uuid(uuidMetadataPersona.id)
                .ExecuteAsync();
            getMembershipsMetadataResult = getMembershipsResponse.Result;
            pnStatus = getMembershipsResponse.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }

        }

        [Then(@"the response contains list with '([^']*)' and '([^']*)' memberships")]
        public void ThenTheResponseContainsListWithAndMemberships(string membership1, string membership2)
        {
            string dirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string membershipFile1 = string.Format("{0}.json", membership1.ToLower());
            string membershipFile2 = string.Format("{0}.json", membership2.ToLower());

            var personaFile1Path = Path.Combine(dirPath, "Data", membershipFile1);
            var personaFile2Path = Path.Combine(dirPath, "Data", membershipFile2);
            List<ChannelMembershipMetadataLocal> personaList = new List<ChannelMembershipMetadataLocal>();
            if (File.Exists(personaFile1Path) && File.Exists(personaFile2Path))
            {
                using (StreamReader r = new StreamReader(personaFile1Path))
                {
                    string json = r.ReadToEnd();
                    channelMembershipMetadata = JsonSerializer.Deserialize<ChannelMembershipMetadataLocal>(json, new JsonSerializerOptions { });
                    personaList.Add(channelMembershipMetadata);
                }
                using (StreamReader r = new StreamReader(personaFile2Path))
                {
                    string json = r.ReadToEnd();
                    channelMembershipMetadata = JsonSerializer.Deserialize<ChannelMembershipMetadataLocal>(json, new JsonSerializerOptions { });
                    personaList.Add(channelMembershipMetadata);
                }
            }
            Assert.AreEqual(personaList[0].channel.id, getMembershipsMetadataResult.Memberships[0].ChannelMetadata.Channel);
            Assert.AreEqual(personaList[1].channel.id, getMembershipsMetadataResult.Memberships[1].ChannelMetadata.Channel);
        }

        [When(@"I get the memberships for current user")]
        public async Task WhenIGetTheMembershipsForCurrentUser()
        {
            PNResult<PNMembershipsResult> getMembershipsResponse = await pn.GetMemberships()
                .ExecuteAsync();
            getMembershipsMetadataResult = getMembershipsResponse.Result;
            pnStatus = getMembershipsResponse.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }

        }

        [When(@"I get the memberships including custom and channel custom information")]
        public async Task WhenIGetTheMembershipsIncludingCustomAndChannelCustomInformation()
        {
            PNResult<PNMembershipsResult> getMembershipsResponse = await pn.GetMemberships()
                .Uuid(uuidMetadataPersona.id)
                .Include(new PNMembershipField[] { PNMembershipField.CUSTOM, PNMembershipField.CHANNEL, PNMembershipField.CHANNEL_CUSTOM })
                .ExecuteAsync();
            getMembershipsMetadataResult = getMembershipsResponse.Result;
            pnStatus = getMembershipsResponse.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }

        //[Then(@"the response contains list with '([^']*)' and '([^']*)' memberships")]
        //public void ThenTheResponseContainsListWithAndMemberships(string vipChatMembership, string dMMembership)
        //{
        //    throw new PendingStepException();
        //}

        [Given(@"the data for '([^']*)' membership")]
        public void GivenTheDataForMembership(string whatMembership)
        {
            string dirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string membershipFile = string.Format("{0}.json", whatMembership.ToLower());

            var personaFile1Path = Path.Combine(dirPath, "Data", membershipFile);
            if (File.Exists(personaFile1Path))
            {
                using (StreamReader r = new StreamReader(personaFile1Path))
                {
                    string json = r.ReadToEnd();
                    if (string.Compare(currentContract, "removeAliceMembership", true) == 0)
                    {
                        removeChannelMembershipMetadata = JsonSerializer.Deserialize<ChannelMembershipMetadataLocal>(json, new JsonSerializerOptions { });
                    }
                    else
                    {
                        channelMembershipMetadata = JsonSerializer.Deserialize<ChannelMembershipMetadataLocal>(json, new JsonSerializerOptions { });
                    }
                }
            }
        }

        [When(@"I set the membership")]
        public async Task WhenISetTheMembership()
        {
            PNMembership membership = new PNMembership()
            {
                Channel = channelMembershipMetadata.channel.id
            };

            PNResult<PNMembershipsResult> setMembershipsResponse = await pn.SetMemberships()
                .Uuid(uuidMetadataPersona.id)
                .Channels(new List<PNMembership>() { membership })
                .ExecuteAsync();
            setMembershipsResult = setMembershipsResponse.Result;
            pnStatus = setMembershipsResponse.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }

        [Then(@"the response contains list with '([^']*)' membership")]
        public void ThenTheResponseContainsListWithMembership(string whatMembership)
        {
            string dirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string membershipFile = string.Format("{0}.json", whatMembership.ToLower());

            var personaFile1Path = Path.Combine(dirPath, "Data", membershipFile);
            List<ChannelMembershipMetadataLocal> membershipsList = new List<ChannelMembershipMetadataLocal>();
            if (File.Exists(personaFile1Path))
            {
                using (StreamReader r = new StreamReader(personaFile1Path))
                {
                    string json = r.ReadToEnd();
                    channelMembershipMetadata = JsonSerializer.Deserialize<ChannelMembershipMetadataLocal>(json, new JsonSerializerOptions { });
                    membershipsList.Add(channelMembershipMetadata);
                }
            }

            Assert.AreEqual(membershipsList[0].channel.id, setMembershipsResult.Memberships[0].ChannelMetadata.Channel);
        }

        [When(@"I set the membership for current user")]
        public async Task WhenISetTheMembershipForCurrentUser()
        {
            PNMembership membership = new PNMembership()
            {
                Channel = channelMembershipMetadata.channel.id
            };

            PNResult<PNMembershipsResult> setMembershipsResponse = await pn.SetMemberships()
                .Channels(new List<PNMembership>() { membership })
                .ExecuteAsync();
            setMembershipsResult = setMembershipsResponse.Result;
            pnStatus = setMembershipsResponse.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }

        [When(@"I remove the membership")]
        public async Task WhenIRemoveTheMembership()
        {
            List<string> membershipList = new List<string>();
            membershipList.Add(removeChannelMembershipMetadata.channel.id);

            PNResult<PNMembershipsResult> removeMembershipsResponse = await pn.RemoveMemberships()
                .Uuid(uuidMetadataPersona.id)
                .Channels(membershipList)
                .ExecuteAsync();
            pnStatus = removeMembershipsResponse.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }

        [When(@"I remove the membership for current user")]
        public async Task WhenIRemoveTheMembershipForCurrentUser()
        {
            List<string> membershipList = new List<string>();
            membershipList.Add(removeChannelMembershipMetadata.channel.id);

            PNResult<PNMembershipsResult> removeMembershipsResponse = await pn.RemoveMemberships()
                .Channels(membershipList)
                .ExecuteAsync();
            pnStatus = removeMembershipsResponse.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }

        [Given(@"the data for '([^']*)' membership that we want to remove")]
        public void GivenTheDataForMembershipThatWeWantToRemove(string whatMembership)
        {
            string dirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string membershipFile = string.Format("{0}.json", whatMembership.ToLower());

            var personaFile1Path = Path.Combine(dirPath, "Data", membershipFile);
            if (File.Exists(personaFile1Path))
            {
                using (StreamReader r = new StreamReader(personaFile1Path))
                {
                    string json = r.ReadToEnd();
                    removeChannelMembershipMetadata = JsonSerializer.Deserialize<ChannelMembershipMetadataLocal>(json, new JsonSerializerOptions { });
                }
            }
        }

        [When(@"I manage memberships")]
        public async Task WhenIManageMemberships()
        {
            PNMembership setMembership = new PNMembership()
            {
                Channel = channelMembershipMetadata.channel.id
            };

            List<string> removeMembershipList = new List<string>();
            removeMembershipList.Add(removeChannelMembershipMetadata.channel.id);

            PNResult<PNMembershipsResult> manageMembershipResponse = await pn.ManageMemberships()
                .Uuid(uuidMetadataPersona.id)
                .Set(new List<PNMembership>() { setMembership })
                .Remove(removeMembershipList)
                .ExecuteAsync();
            setMembershipsResult = manageMembershipResponse.Result;
            pnStatus = manageMembershipResponse.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }

        }

        [Then(@"the response does not contain list with '([^']*)' membership")]
        public void ThenTheResponseDoesNotContainListWithMembership(string whatMembership)
        {
            string dirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string membershipFile = string.Format("{0}.json", whatMembership.ToLower());

            var personaFile1Path = Path.Combine(dirPath, "Data", membershipFile);
            List<ChannelMembershipMetadataLocal> membershipsList = new List<ChannelMembershipMetadataLocal>();
            if (File.Exists(personaFile1Path))
            {
                using (StreamReader r = new StreamReader(personaFile1Path))
                {
                    string json = r.ReadToEnd();
                    channelMembershipMetadata = JsonSerializer.Deserialize<ChannelMembershipMetadataLocal>(json, new JsonSerializerOptions { });
                    membershipsList.Add(channelMembershipMetadata);
                }
            }

            Assert.AreEqual(membershipsList.Count, setMembershipsResult.Memberships.Count);
            Assert.AreNotEqual(membershipsList[0].channel.id, setMembershipsResult.Memberships[0].ChannelMetadata.Channel);
        }

    }
}
