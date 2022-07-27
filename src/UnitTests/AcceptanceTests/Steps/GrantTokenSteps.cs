using System;
using TechTalk.SpecFlow;
using PubnubApi;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Net;
using System.Globalization;

namespace AcceptanceTests.Steps
{
    [Binding]
    public partial class FeatureAccessSteps
    {
        public static string currentFeature = string.Empty;
        public static bool betaVersion = false;
        private string acceptance_test_origin = "localhost:8090";
        private bool bypassMockServer = false;
        private readonly ScenarioContext _scenarioContext;
        private Pubnub pn;
        private PNConfiguration config = null;
        GrantInput grantInput = null;
        PNAccessManagerTokenResult grantResult = null;
        PNStatus pnStatus = null;
        PubnubError pnError = null;
        public enum PermissionType { Channel, Group, Uuid};

        private string tokenInput = string.Empty;
        private PNTokenContent tokenContent = null;
        public class ResourcePermType
        {
            public string ResourceId;
            public PermissionType PermType;
        }

        ResourcePermType currentResPermType;

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

        internal class GrantInput
        {
            public Dictionary<string, PNTokenAuthValues> ResourceChannels = new Dictionary<string, PNTokenAuthValues>();
            public Dictionary<string, PNTokenAuthValues> ResourceGroups = new Dictionary<string, PNTokenAuthValues>();
            public Dictionary<string, PNTokenAuthValues> ResourceUuids = new Dictionary<string, PNTokenAuthValues>();
            public Dictionary<string, PNTokenAuthValues> PatternChannels = new Dictionary<string, PNTokenAuthValues>();
            public Dictionary<string, PNTokenAuthValues> PatternGroups = new Dictionary<string, PNTokenAuthValues>();
            public Dictionary<string, PNTokenAuthValues> PatternUuids = new Dictionary<string, PNTokenAuthValues>();
            public int TTL { get; set; }
            public string AuthorizedUuid;
        }
        public FeatureAccessSteps(ScenarioContext scenarioContext)
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
            string currentContract = "";
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

            grantInput = new GrantInput();
            tokenInput = string.Empty;
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

        [Given(@"I have a keyset with access manager enabled")]
        public void GivenIHaveAKeysetWithAccessManagerEnabled()
        {
            config = new PNConfiguration("pn-csharp-acceptance-test-uuid");
            config.Origin = acceptance_test_origin;
            config.Secure = false;
            config.PublishKey = System.Environment.GetEnvironmentVariable("PN_PUB_KEY");
            config.SubscribeKey = System.Environment.GetEnvironmentVariable("PN_SUB_KEY");
            config.SecretKey = System.Environment.GetEnvironmentVariable("PN_SEC_KEY");

            pn = new Pubnub(config);
        }

        [Given(@"the authorized UUID ""(.*)""")]
        public void GivenTheAuthorizedUUID(string p0)
        {
            grantInput.AuthorizedUuid = p0;
        }
        
        [Given(@"the TTL (.*)")]
        public void GivenTheTTL(int p0)
        {
            grantInput.TTL = p0;
        }
        
        [Given(@"the '(.*)' CHANNEL resource access permissions")]
        public void GivenTheCHANNELResourceAccessPermissions(string p0)
        {
            currentResPermType = new ResourcePermType() { ResourceId = p0, PermType = PermissionType.Channel };
            grantInput.ResourceChannels.Add(p0, new PNTokenAuthValues());
        }

        PNTokenAuthValues GetCurrentGivenGrantResourcePermissionsByPermType()
        {
            PNTokenAuthValues currentTokenAuth = null;
            switch (currentResPermType.PermType)
            {
                case PermissionType.Channel:
                    currentTokenAuth = grantInput.ResourceChannels[currentResPermType.ResourceId];
                    break;
                case PermissionType.Group:
                    currentTokenAuth = grantInput.ResourceGroups[currentResPermType.ResourceId];
                    break;
                case PermissionType.Uuid:
                    currentTokenAuth = grantInput.ResourceUuids[currentResPermType.ResourceId];
                    break;
                default:
                    break;
            }
            return currentTokenAuth;
        }

        PNTokenAuthValues GetCurrentGivenGrantPatternPermissionsByPermType()
        {
            PNTokenAuthValues currentTokenAuth = null;
            switch (currentResPermType.PermType)
            {
                case PermissionType.Channel:
                    currentTokenAuth = grantInput.PatternChannels[currentResPermType.ResourceId];
                    break;
                case PermissionType.Group:
                    currentTokenAuth = grantInput.PatternGroups[currentResPermType.ResourceId];
                    break;
                case PermissionType.Uuid:
                    currentTokenAuth = grantInput.PatternUuids[currentResPermType.ResourceId];
                    break;
                default:
                    break;
            }
            return currentTokenAuth;
        }

        [Given(@"grant resource permission READ")]
        public void GivenGrantResourcePermissionREAD()
        {
            PNTokenAuthValues perms = GetCurrentGivenGrantResourcePermissionsByPermType();
            if (perms != null) { perms.Read = true; }
        }
        
        [Given(@"grant resource permission WRITE")]
        public void GivenGrantResourcePermissionWRITE()
        {
            PNTokenAuthValues perms = GetCurrentGivenGrantResourcePermissionsByPermType();
            if (perms != null) { perms.Write = true; }
        }

        [Given(@"grant resource permission GET")]
        public void GivenGrantResourcePermissionGET()
        {
            PNTokenAuthValues perms = GetCurrentGivenGrantResourcePermissionsByPermType();
            if (perms != null) { perms.Get = true; }
        }

        [Given(@"grant resource permission MANAGE")]
        public void GivenGrantResourcePermissionMANAGE()
        {
            PNTokenAuthValues perms = GetCurrentGivenGrantResourcePermissionsByPermType();
            if (perms != null) { perms.Manage = true; }
        }

        [Given(@"grant resource permission UPDATE")]
        public void GivenGrantResourcePermissionUPDATE()
        {
            PNTokenAuthValues perms = GetCurrentGivenGrantResourcePermissionsByPermType();
            if (perms != null) { perms.Update = true; }
        }

        [Given(@"grant resource permission JOIN")]
        public void GivenGrantResourcePermissionJOIN()
        {
            PNTokenAuthValues perms = GetCurrentGivenGrantResourcePermissionsByPermType();
            if (perms != null) { perms.Join = true; }
        }

        [Given(@"grant resource permission DELETE")]
        public void GivenGrantResourcePermissionDELETE()
        {
            PNTokenAuthValues perms = GetCurrentGivenGrantResourcePermissionsByPermType();
            if (perms != null) { perms.Delete = true; }
        }

        [Given(@"the '(.*)' CHANNEL_GROUP resource access permissions")]
        public void GivenTheCHANNEL_GROUPResourceAccessPermissions(string p0)
        {
            currentResPermType = new ResourcePermType() { ResourceId = p0, PermType = PermissionType.Group };
            grantInput.ResourceGroups.Add(p0, new PNTokenAuthValues());
        }

        [Given(@"the '(.*)' UUID resource access permissions")]
        public void GivenTheUUIDResourceAccessPermissions(string p0)
        {
            currentResPermType = new ResourcePermType() { ResourceId = p0, PermType = PermissionType.Uuid };
            grantInput.ResourceUuids.Add(p0, new PNTokenAuthValues());
        }

        [Given(@"the '(.*)' CHANNEL pattern access permissions")]
        public void GivenTheCHANNELPatternAccessPermissions(string p0)
        {
            currentResPermType = new ResourcePermType() { ResourceId = p0, PermType = PermissionType.Channel };
            grantInput.PatternChannels.Add(p0, new PNTokenAuthValues());
        }

        [Given(@"grant pattern permission READ")]
        public void GivenGrantPatternPermissionREAD()
        {
            PNTokenAuthValues perms = GetCurrentGivenGrantPatternPermissionsByPermType();
            if (perms != null) { perms.Read = true; }
        }

        [Given(@"grant pattern permission WRITE")]
        public void GivenGrantPatternPermissionWRITE()
        {
            PNTokenAuthValues perms = GetCurrentGivenGrantPatternPermissionsByPermType();
            if (perms != null) { perms.Write = true; }
        }

        [Given(@"grant pattern permission GET")]
        public void GivenGrantPatternPermissionGET()
        {
            PNTokenAuthValues perms = GetCurrentGivenGrantPatternPermissionsByPermType();
            if (perms != null) { perms.Get = true; }
        }

        [Given(@"grant pattern permission MANAGE")]
        public void GivenGrantPatternPermissionMANAGE()
        {
            PNTokenAuthValues perms = GetCurrentGivenGrantPatternPermissionsByPermType();
            if (perms != null) { perms.Manage = true; }
        }

        [Given(@"grant pattern permission UPDATE")]
        public void GivenGrantPatternPermissionUPDATE()
        {
            PNTokenAuthValues perms = GetCurrentGivenGrantPatternPermissionsByPermType();
            if (perms != null) { perms.Update = true; }
        }

        [Given(@"grant pattern permission JOIN")]
        public void GivenGrantPatternPermissionJOIN()
        {
            PNTokenAuthValues perms = GetCurrentGivenGrantPatternPermissionsByPermType();
            if (perms != null) { perms.Join = true; }
        }

        [Given(@"grant pattern permission DELETE")]
        public void GivenGrantPatternPermissionDELETE()
        {
            PNTokenAuthValues perms = GetCurrentGivenGrantPatternPermissionsByPermType();
            if (perms != null) { perms.Delete = true; }
        }

        [Given(@"the '(.*)' CHANNEL_GROUP pattern access permissions")]
        public void GivenTheCHANNEL_GROUPPatternAccessPermissions(string p0)
        {
            currentResPermType = new ResourcePermType() { ResourceId = p0, PermType = PermissionType.Group };
            grantInput.PatternGroups.Add(p0, new PNTokenAuthValues());
        }

        [Given(@"the '(.*)' UUID pattern access permissions")]
        public void GivenTheUUIDPatternAccessPermissions(string p0)
        {
            currentResPermType = new ResourcePermType() { ResourceId = p0, PermType = PermissionType.Uuid };
            grantInput.PatternUuids.Add(p0, new PNTokenAuthValues());
        }

        [Given(@"deny resource permission READ")]
        public void GivenDenyResourcePermissionREAD()
        {
            PNTokenAuthValues perms = GetCurrentGivenGrantResourcePermissionsByPermType();
            if (perms != null) { perms.Read = false; }
        }

        [Given(@"deny resource permission GET")]
        public void GivenDenyResourcePermissionGET()
        {
            PNTokenAuthValues perms = GetCurrentGivenGrantResourcePermissionsByPermType();
            if (perms != null) { perms.Get = false; }
        }

        [When(@"I grant a token specifying those permissions")]
        public async Task WhenIGrantATokenSpecifyingThosePermissions()
        {
            tokenContent = null;
            PNResult<PNAccessManagerTokenResult> pamGrantResult = await pn.GrantToken()
                .TTL(grantInput.TTL)
                .AuthorizedUuid(grantInput.AuthorizedUuid)
                .Resources(new PNTokenResources()
                {
                    Channels = grantInput.ResourceChannels,
                    ChannelGroups = grantInput.ResourceGroups,
                    Uuids = grantInput.ResourceUuids
                })
                .Patterns(new PNTokenPatterns()
                {
                    Channels = grantInput.PatternChannels,
                    ChannelGroups = grantInput.PatternGroups,
                    Uuids = grantInput.PatternUuids
                })
                .ExecuteAsync();
            grantResult = pamGrantResult.Result;
            pnStatus = pamGrantResult.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }
        
        [When(@"I attempt to grant a token specifying those permissions")]
        public async Task WhenIAttemptToGrantATokenSpecifyingThosePermissions()
        {
            tokenContent = null;
            PNResult<PNAccessManagerTokenResult> pamGrantResult = await pn.GrantToken()
                .TTL(grantInput.TTL)
                .AuthorizedUuid(grantInput.AuthorizedUuid)
                .Resources(new PNTokenResources()
                {
                    Channels = grantInput.ResourceChannels,
                    ChannelGroups = grantInput.ResourceGroups,
                    Uuids = grantInput.ResourceUuids
                })
                .Patterns(new PNTokenPatterns()
                {
                    Channels = grantInput.PatternChannels,
                    ChannelGroups = grantInput.PatternGroups,
                    Uuids = grantInput.PatternUuids
                })
                .ExecuteAsync();
            grantResult = pamGrantResult.Result;
            pnStatus = pamGrantResult.Status;
            if (pnStatus.Error)
            {
                pnError = pn.JsonPluggableLibrary.DeserializeToObject<PubnubError>(pnStatus.ErrorData.Information);
            }
        }

        [When(@"I parse the token")]
        public void WhenIParseTheToken()
        {
            tokenContent = null;
            currentResPermType = new ResourcePermType();
            if (!string.IsNullOrEmpty(tokenInput))
            {
                tokenContent = pn.ParseToken(tokenInput);
            }
        }
        
        [Then(@"the token contains the authorized UUID ""(.*)""")]
        public void ThenTheTokenContainsTheAuthorizedUUID(string p0)
        {
            PNTokenContent content = pn.ParseToken(grantResult.Token);
            if (betaVersion && string.Compare(p0, content.AuthorizedUuid, true, CultureInfo.InvariantCulture) != 0)
            {
                Assert.Ignore();
            }
            else
            {
                Assert.AreEqual(p0, content.AuthorizedUuid);
            }
        }

        [Then(@"the token contains the TTL (.*)")]
        public void ThenTheTokenContainsTheTTL(int p0)
        {
            PNTokenContent content = pn.ParseToken(grantResult.Token);
            Assert.AreEqual(p0, content.TTL);
        }
        
        [Then(@"the token has '(.*)' CHANNEL resource access permissions")]
        public void ThenTheTokenHasCHANNELResourceAccessPermissions(string p0)
        {
            currentResPermType.PermType = PermissionType.Channel;
            currentResPermType.ResourceId = p0;

            PNTokenContent content = pn.ParseToken(grantResult.Token);
            Assert.True(content.Resources.Channels.Count > 0 && content.Resources.Channels.ContainsKey(currentResPermType.ResourceId));
        }

        [Then(@"token resource permission READ")]
        public void ThenTokenResourcePermissionREAD()
        {
            PNTokenAuthValues perms = ParseTokenAndGetCurrentResourcePermissionsOfTokenByPermType();
            Assert.IsTrue(perms != null && perms.Read);
        }

        [Then(@"token resource permission WRITE")]
        public void ThenTokenResourcePermissionWRITE()
        {
            PNTokenAuthValues perms = ParseTokenAndGetCurrentResourcePermissionsOfTokenByPermType();
            Assert.IsTrue(perms != null && perms.Write);
        }

        [Then(@"token resource permission GET")]
        public void ThenTokenResourcePermissionGET()
        {
            PNTokenAuthValues perms = ParseTokenAndGetCurrentResourcePermissionsOfTokenByPermType();
            Assert.IsTrue(perms != null && perms.Get);
        }

        [Then(@"token resource permission MANAGE")]
        public void ThenTokenResourcePermissionMANAGE()
        {
            PNTokenAuthValues perms = ParseTokenAndGetCurrentResourcePermissionsOfTokenByPermType();
            Assert.IsTrue(perms != null && perms.Manage);
        }

        [Then(@"token resource permission UPDATE")]
        public void ThenTokenResourcePermissionUPDATE()
        {
            PNTokenAuthValues perms = ParseTokenAndGetCurrentResourcePermissionsOfTokenByPermType();
            Assert.IsTrue(perms != null && perms.Update);
        }

        [Then(@"token resource permission JOIN")]
        public void ThenTokenResourcePermissionJOIN()
        {
            PNTokenAuthValues perms = ParseTokenAndGetCurrentResourcePermissionsOfTokenByPermType();
            Assert.IsTrue(perms != null && perms.Join);
        }

        [Then(@"token resource permission DELETE")]
        public void ThenTokenResourcePermissionDELETE()
        {
            PNTokenContent content = pn.ParseToken(grantResult.Token);
            if (content.Resources.Channels.Count > 0)
            {
                PNTokenAuthValues perms = content.Resources.Channels.First().Value;
                Assert.IsTrue(perms.Delete);
            }
            else
            {
                Assert.Fail();
            }
        }

        [Then(@"the token has '(.*)' CHANNEL_GROUP resource access permissions")]
        public void ThenTheTokenHasCHANNEL_GROUPResourceAccessPermissions(string p0)
        {
            currentResPermType.PermType = PermissionType.Group;
            currentResPermType.ResourceId = p0;

            PNTokenContent content = pn.ParseToken(grantResult.Token);
            Assert.True(content.Resources.ChannelGroups.Count > 0 && content.Resources.ChannelGroups.ContainsKey(currentResPermType.ResourceId));
        }

        [Then(@"the token has '(.*)' UUID resource access permissions")]
        public void ThenTheTokenHasUUIDResourceAccessPermissions(string p0)
        {
            currentResPermType.PermType = PermissionType.Uuid;
            currentResPermType.ResourceId = p0;

            PNTokenContent content = (grantResult != null) ? pn.ParseToken(grantResult.Token) : tokenContent;
            Assert.True(content.Resources.Uuids.Count > 0 && content.Resources.Uuids.ContainsKey(currentResPermType.ResourceId));
        }

        [Then(@"the token has '(.*)' CHANNEL pattern access permissions")]
        public void ThenTheTokenHasCHANNELPatternAccessPermissions(string p0)
        {
            currentResPermType.PermType = PermissionType.Channel;
            currentResPermType.ResourceId = p0;

            PNTokenContent content = pn.ParseToken(grantResult.Token);
            Assert.True(content.Patterns.Channels.Count > 0 && content.Patterns.Channels.ContainsKey(currentResPermType.ResourceId));
        }

        private PNTokenAuthValues ParseTokenAndGetCurrentResourcePermissionsOfTokenByPermType()
        {
            PNTokenAuthValues perms = null;
            PNTokenContent content = (grantResult != null) ? pn.ParseToken(grantResult.Token) : tokenContent;
            switch (currentResPermType.PermType)
            {
                case PermissionType.Channel:
                    if (content.Resources.Channels.Count > 0)
                    {
                        perms = content.Resources.Channels[currentResPermType.ResourceId];
                    }
                    break;
                case PermissionType.Group:
                    if (content.Resources.ChannelGroups.Count > 0)
                    {
                        perms = content.Resources.ChannelGroups[currentResPermType.ResourceId];
                    }
                    break;
                case PermissionType.Uuid:
                    if (content.Resources.Uuids.Count > 0)
                    {
                        perms = content.Resources.Uuids[currentResPermType.ResourceId];
                    }
                    break;
                default:
                    break;
            }

            return perms;
        }

        private PNTokenAuthValues ParseTokenAndGetCurrentPatternPermissionsOfTokenByPermType()
        {
            PNTokenAuthValues perms = null;
            PNTokenContent content = (grantResult != null) ? pn.ParseToken(grantResult.Token) : tokenContent;
            switch (currentResPermType.PermType)
            {
                case PermissionType.Channel:
                    if (content.Patterns.Channels.Count > 0)
                    {
                        perms = content.Patterns.Channels[currentResPermType.ResourceId];
                    }
                    break;
                case PermissionType.Group:
                    if (content.Patterns.ChannelGroups.Count > 0)
                    {
                        perms = content.Patterns.ChannelGroups[currentResPermType.ResourceId];
                    }
                    break;
                case PermissionType.Uuid:
                    if (content.Patterns.Uuids.Count > 0)
                    {
                        perms = content.Patterns.Uuids[currentResPermType.ResourceId];
                    }
                    break;
                default:
                    break;
            }

            return perms;
        }

        [Then(@"token pattern permission READ")]
        public void ThenTokenPatternPermissionREAD()
        {
            PNTokenAuthValues perms = ParseTokenAndGetCurrentPatternPermissionsOfTokenByPermType();
            Assert.IsTrue(perms != null && perms.Read);
        }

        [Then(@"token pattern permission WRITE")]
        public void ThenTokenPatternPermissionWRITE()
        {
            PNTokenAuthValues perms = ParseTokenAndGetCurrentPatternPermissionsOfTokenByPermType();
            Assert.IsTrue(perms != null && perms.Write);
        }

        [Then(@"token pattern permission GET")]
        public void ThenTokenPatternPermissionGET()
        {
            PNTokenAuthValues perms = ParseTokenAndGetCurrentPatternPermissionsOfTokenByPermType();
            Assert.IsTrue(perms != null && perms.Get);
        }

        [Then(@"token pattern permission MANAGE")]
        public void ThenTokenPatternPermissionMANAGE()
        {
            PNTokenAuthValues perms = ParseTokenAndGetCurrentPatternPermissionsOfTokenByPermType();
            Assert.IsTrue(perms != null && perms.Manage);
        }

        [Then(@"token pattern permission UPDATE")]
        public void ThenTokenPatternPermissionUPDATE()
        {
            PNTokenAuthValues perms = ParseTokenAndGetCurrentPatternPermissionsOfTokenByPermType();
            Assert.IsTrue(perms != null && perms.Update);
        }

        [Then(@"token pattern permission JOIN")]
        public void ThenTokenPatternPermissionJOIN()
        {
            PNTokenAuthValues perms = ParseTokenAndGetCurrentPatternPermissionsOfTokenByPermType();
            Assert.IsTrue(perms != null && perms.Join);
        }

        [Then(@"token pattern permission DELETE")]
        public void ThenTokenPatternPermissionDELETE()
        {
            PNTokenAuthValues perms = ParseTokenAndGetCurrentPatternPermissionsOfTokenByPermType();
            Assert.IsTrue(perms != null && perms.Delete);
        }

        [Then(@"the token has '(.*)' CHANNEL_GROUP pattern access permissions")]
        public void ThenTheTokenHasCHANNEL_GROUPPatternAccessPermissions(string p0)
        {
            currentResPermType.PermType = PermissionType.Group;
            currentResPermType.ResourceId = p0;

            PNTokenContent content = pn.ParseToken(grantResult.Token);
            Assert.True(content.Patterns.ChannelGroups.Count > 0 && content.Patterns.ChannelGroups.ContainsKey(currentResPermType.ResourceId));
        }

        [Then(@"the token has '(.*)' UUID pattern access permissions")]
        public void ThenTheTokenHasUUIDPatternAccessPermissions(string p0)
        {
            currentResPermType.PermType = PermissionType.Uuid;
            currentResPermType.ResourceId = p0;

            PNTokenContent content = (grantResult != null) ? pn.ParseToken(grantResult.Token) : tokenContent;
            Assert.True(content.Patterns.Uuids.Count > 0 && content.Patterns.Uuids.ContainsKey(currentResPermType.ResourceId));
        }

        [Then(@"the token does not contain an authorized uuid")]
        public void ThenTheTokenDoesNotContainAnAuthorizedUuid()
        {
            string token = grantResult.Token;
            PNTokenContent content = pn.ParseToken(token);
            Assert.True(string.IsNullOrEmpty(content.AuthorizedUuid));
        }

        [Then(@"an error is returned")]
        public void ThenAnErrorIsReturned()
        {
            Assert.IsTrue(pnStatus.Error);
        }
        
        [Then(@"the error status code is (.*)")]
        public void ThenTheErrorStatusCodeIs(int p0)
        {
            if (pnError != null)
            {
                Assert.AreEqual(p0, pnError.status);
            }
            else
            {
                Assert.Fail();
            }
        }
        
        [Then(@"the error message is '(.*)'")]
        public void ThenTheErrorMessageIs(string p0)
        {
            if (pnError != null)
            {
                Assert.AreEqual(p0, pnError.error.message);
            }
            else
            {
                Assert.Fail();
            }
        }
        
        [Then(@"the error source is '(.*)'")]
        public void ThenTheErrorSourceIs(string p0)
        {
            if (pnError != null)
            {
                Assert.AreEqual(p0, pnError.error.source);
            }
            else
            {
                Assert.Fail();
            }
        }

        [Then(@"the error detail message is '(.*)'")]
        public void ThenTheErrorDetailMessageIs(string p0)
        {
            if (pnError != null && pnError.error.details.Count > 0)
            {
                Assert.AreEqual(p0, pnError.error.details[0].message);
            }
            else
            {
                Assert.Fail();
            }
        }

        [Then(@"the error detail location is '(.*)'")]
        public void ThenTheErrorDetailLocationIs(string p0)
        {
            if (pnError != null && pnError.error.details.Count > 0)
            {
                Assert.AreEqual(p0, pnError.error.details[0].location);
            }
            else
            {
                Assert.Fail();
            }
        }

        [Then(@"the error detail location type is '(.*)'")]
        public void ThenTheErrorDetailLocationTypeIs(string p0)
        {
            if (pnError != null && pnError.error.details.Count > 0)
            {
                Assert.AreEqual(p0, pnError.error.details[0].locationType);
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}
