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
        PNChannelMembersResult getChannelMembersMetadataResult;
        ChannelMemberMetadataLocal channelMemberMetadata;
        ChannelMemberMetadataLocal removeChannelMemberMetadata;
        PNChannelMembersResult setChannelMembersResult;

        internal class ChannelMemberMetadataLocal
        {
            public UuidMetadataPersona uuid { get; set; }
            public string updated { get; set; }
            public string eTag { get; set; }
            public Dictionary<string, object> custom { get; set; }
        }


        [When(@"I get the channel members")]
        public async Task WhenIGetTheChannelMembers()
        {
            PNResult<PNChannelMembersResult> getMembersResponse = await pn.GetChannelMembers()
                .Channel(channelMetadataPersona.id)
                .ExecuteAsync();
            getChannelMembersMetadataResult = getMembersResponse.Result;
            pnStatus = getMembersResponse.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }

        }

        [Then(@"the response contains list with '([^']*)' and '([^']*)' members")]
        public void ThenTheResponseContainsListWithAndMembers(string channelMember1, string channelMember2)
        {
            string dirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string channelMemberFile1 = string.Format("{0}.json", channelMember1.ToLower());
            string channelMemberFile2 = string.Format("{0}.json", channelMember2.ToLower());

            var memberFile1Path = Path.Combine(dirPath, "Data", channelMemberFile1);
            var memberFile2Path = Path.Combine(dirPath, "Data", channelMemberFile2);
            List<ChannelMemberMetadataLocal> personaList = new List<ChannelMemberMetadataLocal>();
            if (File.Exists(memberFile1Path) && File.Exists(memberFile2Path))
            {
                using (StreamReader r = new StreamReader(memberFile1Path))
                {
                    string json = r.ReadToEnd();
                    channelMemberMetadata = JsonSerializer.Deserialize<ChannelMemberMetadataLocal>(json, new JsonSerializerOptions { });
                    personaList.Add(channelMemberMetadata);
                }
                using (StreamReader r = new StreamReader(memberFile2Path))
                {
                    string json = r.ReadToEnd();
                    channelMemberMetadata = JsonSerializer.Deserialize<ChannelMemberMetadataLocal>(json, new JsonSerializerOptions { });
                    personaList.Add(channelMemberMetadata);
                }
            }
            Assert.AreEqual(personaList[0].uuid.id, getChannelMembersMetadataResult.ChannelMembers[0].UuidMetadata.Uuid);
            Assert.AreEqual(personaList[1].uuid.id, getChannelMembersMetadataResult.ChannelMembers[1].UuidMetadata.Uuid);
            if (string.Compare(currentContract, "getMembersOfVipChatChannelWithCustomAndUuidWithCustom", true) == 0)
            {
                Assert.AreEqual(personaList[1].custom.Count, getChannelMembersMetadataResult.ChannelMembers[1].Custom.Count);
            }
        }

        [When(@"I get the channel members including custom and UUID custom information")]
        public async Task WhenIGetTheChannelMembersIncludingCustomAndUUIDCustomInformation()
        {
            PNResult<PNChannelMembersResult> getMembersResponse = await pn.GetChannelMembers()
                .Channel(channelMetadataPersona.id)
                .Include(new PNChannelMemberField[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID, PNChannelMemberField.UUID_CUSTOM })
                .ExecuteAsync();
            getChannelMembersMetadataResult = getMembersResponse.Result;
            pnStatus = getMembersResponse.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }

        [Given(@"the data for '([^']*)' member")]
        public void GivenTheDataForMember(string whatChannelMember)
        {
            string dirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string memberFile = string.Format("{0}.json", whatChannelMember.ToLower());

            var memberFile1Path = Path.Combine(dirPath, "Data", memberFile);
            if (File.Exists(memberFile1Path))
            {
                using (StreamReader r = new StreamReader(memberFile1Path))
                {
                    string json = r.ReadToEnd();
                    channelMemberMetadata = JsonSerializer.Deserialize<ChannelMemberMetadataLocal>(json, new JsonSerializerOptions { });
                }
            }
        }

        [When(@"I set a channel member")]
        public async Task WhenISetAChannelMember()
        {
            setChannelMembersResult = null;
            PNChannelMember channelMember = new PNChannelMember()
            {
                Uuid = channelMemberMetadata.uuid.id
            };

            PNResult<PNChannelMembersResult> setChannelMembersResponse = await pn.SetChannelMembers()
                .Channel(channelMetadataPersona.id)
                .Uuids(new List<PNChannelMember>() { channelMember })
                .ExecuteAsync();
            setChannelMembersResult = setChannelMembersResponse.Result;
            pnStatus = setChannelMembersResponse.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }

        [Then(@"the response contains list with '([^']*)' member")]
        public void ThenTheResponseContainsListWithMember(string whatChannelMember)
        {
            string dirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string memberFile = string.Format("{0}.json", whatChannelMember.ToLower());

            var memberFile1Path = Path.Combine(dirPath, "Data", memberFile);
            List<ChannelMemberMetadataLocal> membersList = new List<ChannelMemberMetadataLocal>();
            if (File.Exists(memberFile1Path))
            {
                using (StreamReader r = new StreamReader(memberFile1Path))
                {
                    string json = r.ReadToEnd();
                    channelMemberMetadata = JsonSerializer.Deserialize<ChannelMemberMetadataLocal>(json, new JsonSerializerOptions { });
                    membersList.Add(channelMemberMetadata);
                }
            }

            Assert.AreEqual(membersList[0].uuid.id, setChannelMembersResult.ChannelMembers[0].UuidMetadata.Uuid);
            if (string.Compare(currentContract, "setMembersForChatChannelWithCustomAndUuidWithCustom", true) == 0)
            {
                Assert.AreEqual(membersList[0].custom.Count, setChannelMembersResult.ChannelMembers[0].Custom.Count);
            }
        }

        [When(@"I set a channel member including custom and UUID with custom")]
        public async Task WhenISetAChannelMemberIncludingCustomAndUUIDWithCustom()
        {
            setChannelMembersResult = null;
            PNChannelMember channelMember = new PNChannelMember()
            {
                Uuid = channelMemberMetadata.uuid.id
            };

            PNResult<PNChannelMembersResult> setChannelMembersResponse = await pn.SetChannelMembers()
                .Channel(channelMetadataPersona.id)
                .Uuids(new List<PNChannelMember>() { channelMember })
                .Include(new PNChannelMemberField[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID, PNChannelMemberField.UUID_CUSTOM })
                .ExecuteAsync();
            setChannelMembersResult = setChannelMembersResponse.Result;
            pnStatus = setChannelMembersResponse.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }

        [Given(@"the data for '([^']*)' member that we want to remove")]
        public void GivenTheDataForMemberThatWeWantToRemove(string whatChannelMember)
        {
            string dirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string memberFile = string.Format("{0}.json", whatChannelMember.ToLower());

            var memberFile1Path = Path.Combine(dirPath, "Data", memberFile);
            if (File.Exists(memberFile1Path))
            {
                using (StreamReader r = new StreamReader(memberFile1Path))
                {
                    string json = r.ReadToEnd();
                    removeChannelMemberMetadata = JsonSerializer.Deserialize<ChannelMemberMetadataLocal>(json, new JsonSerializerOptions { });
                }
            }
        }

        [When(@"I remove a channel member")]
        public async Task WhenIRemoveAChannelMember()
        {
            List<string> channelMemberList = new List<string>();
            channelMemberList.Add(removeChannelMemberMetadata.uuid.id);

            PNResult<PNChannelMembersResult> removeMemberResponse = await pn.RemoveChannelMembers()
                .Channel(channelMetadataPersona.id)
                .Uuids(channelMemberList)
                .ExecuteAsync();
            pnStatus = removeMemberResponse.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }

        [When(@"I manage channel members")]
        public async Task WhenIManageChannelMembers()
        {
            PNChannelMember setChannelMemberList = new PNChannelMember()
            {
                Uuid = channelMemberMetadata.uuid.id
            };

            List<string> removeChannelMemberList = new List<string>();
            removeChannelMemberList.Add(removeChannelMemberMetadata.uuid.id);

            PNResult<PNChannelMembersResult> manageMemberResponse = await pn.ManageChannelMembers()
                .Channel(channelMetadataPersona.id)
                .Set(new List<PNChannelMember>() { setChannelMemberList })
                .Remove(removeChannelMemberList)
                .ExecuteAsync();
            setChannelMembersResult = manageMemberResponse.Result;
            pnStatus = manageMemberResponse.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }

        [Then(@"the response does not contain list with '([^']*)' member")]
        public void ThenTheResponseDoesNotContainListWithMember(string whatChannelMember)
        {
            string dirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string memberFile = string.Format("{0}.json", whatChannelMember.ToLower());

            var memberFile1Path = Path.Combine(dirPath, "Data", memberFile);
            List<ChannelMemberMetadataLocal> membersList = new List<ChannelMemberMetadataLocal>();
            if (File.Exists(memberFile1Path))
            {
                using (StreamReader r = new StreamReader(memberFile1Path))
                {
                    string json = r.ReadToEnd();
                    channelMemberMetadata = JsonSerializer.Deserialize<ChannelMemberMetadataLocal>(json, new JsonSerializerOptions { });
                    membersList.Add(channelMemberMetadata);
                }
            }

            Assert.AreEqual(membersList.Count, setChannelMembersResult.ChannelMembers.Count);
            Assert.AreNotEqual(membersList[0].uuid.id, setChannelMembersResult.ChannelMembers[0].UuidMetadata.Uuid);

        }
    }
}
