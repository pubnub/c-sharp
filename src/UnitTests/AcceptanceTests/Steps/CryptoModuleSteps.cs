using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Threading;
using TechTalk.SpecFlow;
using System.Diagnostics;
using PubnubApi.Security.Crypto;
using PubnubApi.Security.Crypto.Cryptors;
using NUnit.Framework;
using System.IO;

namespace AcceptanceTests.Steps
{
    [Binding]
    public class CryptoModuleSteps
    {
        public static bool enableIntenalPubnubLogging = true;
        public static string currentFeature = string.Empty;
        public static string currentContract = string.Empty;
        public static bool betaVersion = false;
        private string acceptance_test_origin = "localhost:8090";
        private bool bypassMockServer = false;
        private readonly ScenarioContext _scenarioContext;
        string defaultCryptoId = string.Empty;
        string addlCryptoId = string.Empty;
        CryptoModule cryptoModule;
        string cryptoOutcome = string.Empty;
        string cipherKey = string.Empty;
        bool useDynamicRandIV = false;
        string sourceFile = string.Empty;
        string encryptedFile = string.Empty;
        string decryptedToOriginalFile = string.Empty;
        long sourceFileSize = 0;

        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine("Unhandled exception occured inside EventEngine. Exiting the test. Please try again.");
            System.Environment.Exit(1);
        }
        public CryptoModuleSteps(ScenarioContext scenarioContext)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
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
                    HttpClient httpclient = new HttpClient();
                    string mockInitResponse = httpclient.GetStringAsync(new Uri(mockInitContract)).Result;
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

        [Given(@"Crypto module with '([^']*)' cryptor")]
        public void GivenCryptoModuleWithCryptor(string crytorId)
        {
            defaultCryptoId = crytorId;
        }

        [Given(@"with '([^']*)' cipher key")]
        public void GivenWithCipherKey(string pubnubenigma)
        {
            cipherKey = pubnubenigma;
        }

        private void SetCryptoModule()
        {
            if (defaultCryptoId == "acrh" && addlCryptoId == "")
            {
                cryptoModule = new CryptoModule(new AesCbcCryptor(cipherKey), null);
            }
            else if (defaultCryptoId == "legacy" && addlCryptoId == "")
            {
                cryptoModule = new CryptoModule(new LegacyCryptor(cipherKey, useDynamicRandIV), null);
            }
            else if ((defaultCryptoId == "acrh" && addlCryptoId == "legacy") || (defaultCryptoId == "legacy" && addlCryptoId == "acrh"))
            {
                cryptoModule = new CryptoModule(new AesCbcCryptor(cipherKey), new LegacyCryptor(cipherKey, useDynamicRandIV));
            }
        }

        [When(@"I decrypt '([^']*)' file")]
        public void WhenIDecryptFile(string p0)
        {
            var dirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var sourceFile = Path.Combine(dirPath ?? "", "Features\\Encryption\\assets", p0);
            string fileExt = Path.GetExtension(sourceFile);
            var destFile = Path.Combine(dirPath ?? "", string.Format($"decrypted_to_original{fileExt}"));
            if (System.IO.File.Exists(destFile))
            {
                System.IO.File.Delete(destFile);
            }
            try
            {
                SetCryptoModule();
                cryptoModule.DecryptFile(sourceFile, destFile);
                if (new System.IO.FileInfo(destFile).Length >= 0)
                {
                    cryptoOutcome = "success";
                }
            }
            catch (System.Exception e)
            {
                cryptoOutcome = e.Message;
            }
        }

        [Then(@"I receive '([^']*)'")]
        public void ThenIReceive(string p0)
        {
            Assert.AreEqual(p0, cryptoOutcome);
        }

        [Given(@"Legacy code with '([^']*)' cipher key and '([^']*)' vector")]
        public void GivenLegacyCodeWithCipherKeyAndVector(string pubnubenigma, string random)
        {
            cipherKey = pubnubenigma;
            useDynamicRandIV = random == "random";
        }

        [Given(@"with '([^']*)' vector")]
        public void GivenWithVector(string vectorValue)
        {
            useDynamicRandIV = vectorValue == "random";
        }

        [When(@"I encrypt '([^']*)' file as '([^']*)'")]
        public void WhenIEncryptFileAs(string p0, string fileType)
        {
            var dirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            sourceFile = Path.Combine(dirPath ?? "", "Features\\Encryption\\assets", p0);
            string fileExt = Path.GetExtension(sourceFile);
            encryptedFile = Path.Combine(dirPath ?? "", string.Format($"original_to_encrypted{fileExt}"));
            if (System.IO.File.Exists(encryptedFile))
            {
                System.IO.File.Delete(encryptedFile);
            }
            sourceFileSize = new FileInfo(sourceFile).Length;
            SetCryptoModule();
            cryptoModule.EncryptFile(sourceFile, encryptedFile);
        }

        [Then(@"Successfully decrypt an encrypted file with legacy code")]
        public void ThenSuccessfullyDecryptAnEncryptedFileWithLegacyCode()
        {
            var dirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string fileExt = Path.GetExtension(sourceFile);
            decryptedToOriginalFile = Path.Combine(dirPath ?? "", string.Format($"decrypt_to_original{fileExt}"));
            SetCryptoModule();
            cryptoModule.DecryptFile(encryptedFile, decryptedToOriginalFile);
            long decryptedFileSize = new FileInfo(sourceFile).Length;
            Assert.IsTrue(sourceFileSize == decryptedFileSize);            
        }

        [When(@"I decrypt '([^']*)' file as '([^']*)'")]
        public void WhenIDecryptFileAs(string p0, string fileType)
        {
            var dirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            encryptedFile = Path.Combine(dirPath ?? "", "Features\\Encryption\\assets", p0);
            string fileExt = Path.GetExtension(encryptedFile);
            decryptedToOriginalFile = Path.Combine(dirPath ?? "", string.Format($"decrypt_to_original{fileExt}"));
            SetCryptoModule();
            cryptoModule.DecryptFile(encryptedFile, decryptedToOriginalFile);
        }

        [Then(@"Decrypted file content equal to the '([^']*)' file content")]
        public void ThenDecryptedFileContentEqualToTheFileContent(string p0)
        {
            var dirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string expectdFileContent = File.ReadAllText(Path.Combine(dirPath ?? "", "Features\\Encryption\\assets", p0));
            string decryptedFileContent = File.ReadAllText(decryptedToOriginalFile);
            Assert.AreEqual(expectdFileContent, decryptedFileContent);            
        }

        [Given(@"Crypto module with default '([^']*)' and additional '([^']*)' cryptors")]
        public void GivenCryptoModuleWithDefaultAndAdditionalCryptors(string legacy, string acrh)
        {
            defaultCryptoId = legacy;
            addlCryptoId = acrh;
        }
    }
}
