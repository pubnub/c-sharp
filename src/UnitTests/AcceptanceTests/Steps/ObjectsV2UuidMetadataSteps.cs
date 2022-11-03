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
    [Binding]
    public partial class FeatureObjectsV2MetadataSteps
    {
        public static string currentFeature = string.Empty;
        public static string currentContract = string.Empty;
        public static bool betaVersion = false;
        private string acceptance_test_origin = "localhost:8090";
        private bool bypassMockServer = false;
        private readonly ScenarioContext _scenarioContext;
        private Pubnub pn;
        private PNConfiguration config = null;
        private UuidMetadataPersona uuidMetadataPersona = null;
        private PNGetUuidMetadataResult getUuidMetadataResult = null;
        private PNSetUuidMetadataResult setUuidMetadataResult = null;
        private PNGetAllUuidMetadataResult getAllUuidMetadataResult = null;
        PNStatus pnStatus = null;
        PubnubError pnError = null;

        public class PubnubError
        {
            public ErrorMsg error;
            public string service;
            public int status;
        }
        public class MessageDetail
        {
            public string message;
            public string location;
            public string locationType;
        }
        public class ErrorMsg
        {
            public string message;
            public string source;
            public List<MessageDetail> details;
        }

        public class InternalPubnubLog : IPubnubLog
        {
            void IPubnubLog.WriteToLog(string logText)
            {
                System.Diagnostics.Debug.WriteLine(logText);
            }
        }
        internal class UuidMetadataPersona
        {
            public string name { get; set; }
            public string email { get; set; }
            public string externalId { get; set; }
            public string profileUrl { get; set; }
            public string id { get; set; }
            public string updated { get; set; }
            public string type { get; set; }
            public string status { get; set; }
            public string eTag { get; set; }
            public Dictionary<string, object> custom { get; set; }

        }

        public FeatureObjectsV2MetadataSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [BeforeFeature]
        public static void BeforeFeature(FeatureContext featureContext)
        {
            betaVersion = false;
            if (featureContext.FeatureInfo != null && featureContext.FeatureInfo.Tags.Length > 0)
            {
                List<string> tagList = featureContext.FeatureInfo.Tags.AsEnumerable<string>().ToList();
                foreach (string tag in tagList)
                {
                    if (tag.IndexOf("featureSet=") == 0)
                    {
                        currentFeature = tag.Replace("featureSet=", "");
                    }

                    if (tag.IndexOf("beta") == 0)
                    {
                        betaVersion = true;
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine("Starting " + featureContext.FeatureInfo.Title);
        }

        [AfterFeature]
        public static void AfterFeature(FeatureContext featureContext)
        {
            System.Diagnostics.Debug.WriteLine("Finished " + featureContext.FeatureInfo.Title);
        }

        [BeforeScenario()]
        public void BeforeScenario()
        {
            currentContract = "";
            if (_scenarioContext.ScenarioInfo != null && _scenarioContext.ScenarioInfo.Tags.Length > 0)
            {
                List<string> tagList = _scenarioContext.ScenarioInfo.Tags.AsEnumerable<string>().ToList();
                foreach (string tag in tagList)
                {
                    if (tag.IndexOf("contract=") == 0)
                    {
                        currentContract = tag.Replace("contract=", "");
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(currentContract) && !bypassMockServer)
                {
                    string mockInitContract = string.Format("http://{0}/init?__contract__script__={1}", acceptance_test_origin, currentContract);
                    System.Diagnostics.Debug.WriteLine(mockInitContract);
                    WebClient webClient = new WebClient();
                    string mockInitResponse = webClient.DownloadString(mockInitContract);
                    System.Diagnostics.Debug.WriteLine(mockInitResponse);
                }
            }

        }

        [AfterScenario()]
        public void AfterScenario()
        {
            if (!bypassMockServer)
            {
                string mockExpectContract = string.Format("http://{0}/expect", acceptance_test_origin);
                System.Diagnostics.Debug.WriteLine(mockExpectContract);
                WebClient webClient = new WebClient();
                string mockExpectResponse = webClient.DownloadString(mockExpectContract);
                System.Diagnostics.Debug.WriteLine(mockExpectResponse);
            }
        }

        [Given(@"I have a keyset with Objects V(.*) enabled")]
        public void GivenIHaveAKeysetWithObjectsVEnabled(int p0)
        {
            config = new PNConfiguration(new UserId("pn-csharp-acceptance-test-uuid"));
            config.Origin = acceptance_test_origin;
            config.Secure = false;
            config.PublishKey = System.Environment.GetEnvironmentVariable("PN_PUB_KEY");
            config.SubscribeKey = System.Environment.GetEnvironmentVariable("PN_SUB_KEY");
            config.SecretKey = System.Environment.GetEnvironmentVariable("PN_SEC_KEY");

            pn = new Pubnub(config);

        }

        [Given(@"the id for '([^']*)' persona")]
        public void GivenTheIdForPersona(string personaName)
        {
            string dirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string personaFile = string.Format("{0}.json", personaName.ToLower());
            var personaFilePath = Path.Combine(dirPath, "Data", personaFile);
            if (File.Exists(personaFilePath))
            {
                using (StreamReader r = new StreamReader(personaFilePath))
                {
                    string json = r.ReadToEnd();
                    uuidMetadataPersona = JsonSerializer.Deserialize<UuidMetadataPersona>(json, new JsonSerializerOptions { });
                }
            }
        }

        [When(@"I get the UUID metadata")]
        public async Task WhenIGetTheUUIDMetadata()
        {
            PNResult<PNGetUuidMetadataResult> getUuidMetadataResponse = await pn.GetUuidMetadata()
                .Uuid(uuidMetadataPersona.id)
                .ExecuteAsync();
            getUuidMetadataResult = getUuidMetadataResponse.Result;
            pnStatus = getUuidMetadataResponse.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }

        [Then(@"I receive a successful response")]
        public void ThenIReceiveASuccessfulResponse()
        {
            Assert.IsTrue(!pnStatus.Error);
        }

        [Then(@"the UUID metadata for '([^']*)' persona")]
        public void ThenTheUUIDMetadataForPersona(string personaName)
        {
            if (string.Compare(personaName, "alice", true) == 0)
            {
                Assert.AreEqual(uuidMetadataPersona.name, getUuidMetadataResult.Name);
                Assert.AreEqual(uuidMetadataPersona.id, getUuidMetadataResult.Uuid);
                Assert.AreEqual(uuidMetadataPersona.email, getUuidMetadataResult.Email);
                Assert.AreEqual(uuidMetadataPersona.externalId, getUuidMetadataResult.ExternalId);
                Assert.AreEqual(uuidMetadataPersona.profileUrl, getUuidMetadataResult.ProfileUrl);
                Assert.AreEqual(uuidMetadataPersona.updated, getUuidMetadataResult.Updated);
                Assert.IsNull(getUuidMetadataResult.Custom);
            }
            else if (string.Compare(personaName, "bob", true) == 0)
            {
                Assert.AreEqual(uuidMetadataPersona.name, getUuidMetadataResult.Name);
                Assert.AreEqual(uuidMetadataPersona.id, getUuidMetadataResult.Uuid);
                Assert.AreEqual(uuidMetadataPersona.email, getUuidMetadataResult.Email);
                Assert.AreEqual(uuidMetadataPersona.externalId, getUuidMetadataResult.ExternalId);
                Assert.AreEqual(uuidMetadataPersona.profileUrl, getUuidMetadataResult.ProfileUrl);
                Assert.AreEqual(uuidMetadataPersona.updated, getUuidMetadataResult.Updated);
                Assert.AreEqual(uuidMetadataPersona.custom.Count, getUuidMetadataResult.Custom.Count);
            }
        }

        [Given(@"current user is '([^']*)' persona")]
        public void GivenCurrentUserIsPersona(string personaName)
        {
            if (string.Compare(personaName, "bob", true) == 0)
            {
                string dirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                var personaFilePath = Path.Combine(dirPath, "Data", "bob.json");
                using (StreamReader r = new StreamReader(personaFilePath))
                {
                    string json = r.ReadToEnd();
                    uuidMetadataPersona = JsonSerializer.Deserialize<UuidMetadataPersona>(json, new JsonSerializerOptions { });
                    pn.ChangeUserId(uuidMetadataPersona.id);
                }
            }
            else if (string.Compare(personaName, "alice", true) == 0)
            {
                string dirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                var personaFilePath = Path.Combine(dirPath, "Data", "alice.json");
                using (StreamReader r = new StreamReader(personaFilePath))
                {
                    string json = r.ReadToEnd();
                    uuidMetadataPersona = JsonSerializer.Deserialize<UuidMetadataPersona>(json, new JsonSerializerOptions { });
                    pn.ChangeUserId(uuidMetadataPersona.id);
                }
            }
        }

        [When(@"I get the UUID metadata with custom for current user")]
        public async Task WhenIGetTheUUIDMetadataWithCustomForCurrentUser()
        {
            PNResult<PNGetUuidMetadataResult> getUuidMetadataResponse = await pn.GetUuidMetadata()
                .IncludeCustom(true)
                .ExecuteAsync();
            getUuidMetadataResult = getUuidMetadataResponse.Result;
            pnStatus = getUuidMetadataResponse.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }

        [Given(@"the data for '([^']*)' persona")]
        public void GivenTheDataForPersona(string personaName)
        {
            if (string.Compare(personaName, "alice", true) == 0)
            {
                string dirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                var personaFilePath = Path.Combine(dirPath, "Data", "alice.json");
                using (StreamReader r = new StreamReader(personaFilePath))
                {
                    string json = r.ReadToEnd();
                    uuidMetadataPersona = JsonSerializer.Deserialize<UuidMetadataPersona>(json, new JsonSerializerOptions { });
                }
            }
        }

        [When(@"I set the UUID metadata")]
        public async Task WhenISetTheUUIDMetadata()
        {
            PNResult<PNSetUuidMetadataResult> setUuidMetadataResponse = await pn.SetUuidMetadata()
                .Uuid(uuidMetadataPersona.id)
                .Name(uuidMetadataPersona.name)
                .Email(uuidMetadataPersona.email)
                .ExternalId(uuidMetadataPersona.externalId)
                .ProfileUrl(uuidMetadataPersona.profileUrl)
                .ExecuteAsync();

            setUuidMetadataResult = setUuidMetadataResponse.Result;
            pnStatus = setUuidMetadataResponse.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }

        [Then(@"the UUID metadata for '([^']*)' persona contains updated")]
        public void ThenTheUUIDMetadataForPersonaContainsUpdated(string personaName)
        {
            if (string.Compare(personaName, "alice", true) == 0)
            {
                Assert.AreEqual(uuidMetadataPersona.name, setUuidMetadataResult.Name);
                Assert.AreEqual(uuidMetadataPersona.id, setUuidMetadataResult.Uuid);
                Assert.AreEqual(uuidMetadataPersona.email, setUuidMetadataResult.Email);
                Assert.AreEqual(uuidMetadataPersona.externalId, setUuidMetadataResult.ExternalId);
                Assert.AreEqual(uuidMetadataPersona.profileUrl, setUuidMetadataResult.ProfileUrl);
                Assert.AreEqual(uuidMetadataPersona.updated, setUuidMetadataResult.Updated);
                Assert.IsNull(setUuidMetadataResult.Custom);
            }

        }

        [When(@"I remove the UUID metadata")]
        public async Task WhenIRemoveTheUUIDMetadata()
        {
            PNResult<PNRemoveUuidMetadataResult> removeUuidMetadataResponse = await pn.RemoveUuidMetadata()
                .Uuid(uuidMetadataPersona.id)
                .ExecuteAsync();

            pnStatus = removeUuidMetadataResponse.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }

        [When(@"I remove the UUID metadata for current user")]
        public async Task WhenIRemoveTheUUIDMetadataForCurrentUser()
        {
            PNResult<PNRemoveUuidMetadataResult> removeUuidMetadataResponse = await pn.RemoveUuidMetadata()
                .ExecuteAsync();

            pnStatus = removeUuidMetadataResponse.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }

        [When(@"I get all UUID metadata")]
        public async Task WhenIGetAllUUIDMetadata()
        {
            PNResult<PNGetAllUuidMetadataResult> getAllUuidMetadataResponse = await pn.GetAllUuidMetadata()
                .ExecuteAsync();

            getAllUuidMetadataResult = getAllUuidMetadataResponse.Result;
            pnStatus = getAllUuidMetadataResponse.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }

        [Then(@"the response contains list with '([^']*)' and '([^']*)' UUID metadata")]
        public void ThenTheResponseContainsListWithAndUUIDMetadata(string personaName1, string personaName2)
        {
            string dirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string personaFile1 = string.Format("{0}.json", personaName1.ToLower());
            string personaFile2 = string.Format("{0}.json", personaName2.ToLower());

            var personaFile1Path = Path.Combine(dirPath, "Data", personaFile1);
            var personaFile2Path = Path.Combine(dirPath, "Data", personaFile2);
            List<UuidMetadataPersona> personaList = new List<UuidMetadataPersona>();
            if (File.Exists(personaFile1Path) && File.Exists(personaFile2Path))
            {
                using (StreamReader r = new StreamReader(personaFile1Path))
                {
                    string json = r.ReadToEnd();
                    uuidMetadataPersona = JsonSerializer.Deserialize<UuidMetadataPersona>(json, new JsonSerializerOptions { });
                    personaList.Add(uuidMetadataPersona);
                }
                using (StreamReader r = new StreamReader(personaFile2Path))
                {
                    string json = r.ReadToEnd();
                    uuidMetadataPersona = JsonSerializer.Deserialize<UuidMetadataPersona>(json, new JsonSerializerOptions { });
                    personaList.Add(uuidMetadataPersona);
                }
            }

            Assert.AreEqual(getAllUuidMetadataResult.Uuids[0].Uuid, personaList[0].id);
            Assert.AreEqual(getAllUuidMetadataResult.Uuids[1].Uuid, personaList[1].id);
        }

        [When(@"I get all UUID metadata with custom")]
        public async Task WhenIGetAllUUIDMetadataWithCustom()
        {
            PNResult<PNGetAllUuidMetadataResult> getAllUuidMetadataResponse = await pn.GetAllUuidMetadata()
                .IncludeCustom(true)
                .ExecuteAsync();

            getAllUuidMetadataResult = getAllUuidMetadataResponse.Result;
            pnStatus = getAllUuidMetadataResponse.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }

    }
}
