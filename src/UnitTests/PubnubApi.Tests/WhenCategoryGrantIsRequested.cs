using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using PubnubApi;
using PubnubApi.EndPoint;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenCategoryGrantIsRequested : TestHarness
    {
        private const string AuthKey = "category-auth-key";
        private const string TokenWithoutCategories =
            "p0F2AkF0Gl2BEIJDdHRsGGRDcmVzpERjaGFuoENncnCgQ3VzcqBDc3BjoENwYXSkRGNoYW6gQ2dycKBDdXNyomZeZW1wLSoDZl5tZ3ItKhgbQ3NwY6JpXnB1YmxpYy0qA2pecHJpdmF0ZS0qGBtEbWV0YaBDc2lnWCAsvzGmd2rcgtr9rcs4r2tqC87YSppSYqs9CKfaM5IRZA==";

        private Pubnub pubnub;

        [SetUp]
        public void Init()
        {
            PNConfiguration config = new PNConfiguration(new UserId("category-grant-uuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };
            pubnub = createPubNubInstance(config);
        }

        [TearDown]
        public void Cleanup()
        {
            if (pubnub != null)
            {
                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
            }
        }

        private static RequestParameter CreateRequestParameter(object operation)
        {
            MethodInfo method = operation.GetType().GetMethod(
                "CreateRequestParameter", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(method, Is.Not.Null, "CreateRequestParameter not found via reflection");
            return (RequestParameter)method.Invoke(operation, null);
        }

        private static string CreateToken(Dictionary<object, object> tokenContent)
        {
            Type cbor = typeof(Pubnub).Assembly.GetType("PubnubApi.CBOR");
            Assert.That(cbor, Is.Not.Null, "CBOR not found via reflection");
            MethodInfo encode = cbor.GetMethod("Encode", BindingFlags.Public | BindingFlags.Static);
            object byteStringType = cbor.GetField("TYPE_BYTE_STRING").GetValue(null);
            string hex = (string)encode.Invoke(null, new object[] { tokenContent, byteStringType });
            return Convert.ToBase64String(Convert.FromHexString(hex));
        }

        private PNAccessManagerGrantResult ParseGrantResponse(string response)
        {
            List<object> responseList = new List<object>
            {
                pubnub.JsonPluggableLibrary.DeserializeToDictionaryOfObject(response)
            };
            return pubnub.JsonPluggableLibrary.DeserializeToObject<PNAccessManagerGrantResult>(responseList);
        }

        private static GrantOperation SetPermission(GrantOperation operation, string permission)
        {
            switch (permission)
            {
                case "Read":
                    return operation.Read(true);
                case "Write":
                    return operation.Write(true);
                case "Manage":
                    return operation.Manage(true);
                case "Delete":
                    return operation.Delete(true);
                case "Update":
                    return operation.Update(true);
                case "Join":
                    return operation.Join(true);
                default:
                    throw new ArgumentException($"Unknown permission {permission}");
            }
        }

        [Test]
        public void ThenCategoriesShouldBeSentAsCategoryQueryParameter()
        {
            GrantOperation operation = pubnub.Grant()
                .AuthKeys(new[] { AuthKey })
                .Categories(new[] { PNGrantCategory.Channels, PNGrantCategory.Uuids })
                .Get(true)
                .TTL(10);

            Dictionary<string, string> query = CreateRequestParameter(operation).Query;

            Assert.That(query["auth"], Is.EqualTo(AuthKey));
            Assert.That(query["category"], Is.EqualTo("channels%2Cuuids"));
            Assert.That(query["ttl"], Is.EqualTo("10"));
            Assert.That(query["g"], Is.EqualTo("1"));
            foreach (string permission in new[] { "r", "w", "m", "d", "u", "j" })
            {
                Assert.That(query[permission], Is.EqualTo("0"), $"'{permission}' should not be granted with categories");
            }
        }

        [Test]
        public void ThenCategoryRevokeShouldSendGetPermissionAsZero()
        {
            GrantOperation operation = pubnub.Grant()
                .AuthKeys(new[] { AuthKey })
                .Categories(new[] { PNGrantCategory.Channels })
                .Get(false);

            Dictionary<string, string> query = CreateRequestParameter(operation).Query;

            Assert.That(query["category"], Is.EqualTo("channels"));
            Assert.That(query["g"], Is.EqualTo("0"));
        }

        [Test]
        public void ThenCategoriesCombinedWithChannelShouldSendBoth()
        {
            GrantOperation operation = pubnub.Grant()
                .AuthKeys(new[] { AuthKey })
                .Channels(new[] { "ch-a" })
                .Categories(new[] { PNGrantCategory.Uuids })
                .Get(true);

            Dictionary<string, string> query = CreateRequestParameter(operation).Query;

            Assert.That(query["channel"], Is.EqualTo("ch-a"));
            Assert.That(query["category"], Is.EqualTo("uuids"));
            Assert.That(query["g"], Is.EqualTo("1"));
        }

        [Test]
        public void ThenDuplicateCategoriesShouldBeSentOnceInSortedOrder()
        {
            GrantOperation operation = pubnub.Grant()
                .AuthKeys(new[] { AuthKey })
                .Categories(new[] { PNGrantCategory.Uuids, PNGrantCategory.Channels, PNGrantCategory.Uuids })
                .Get(true);

            Dictionary<string, string> query = CreateRequestParameter(operation).Query;

            Assert.That(query["category"], Is.EqualTo("channels%2Cuuids"));
        }

        [Test]
        public void ThenGrantWithoutCategoriesShouldNotSendCategoryQueryParameter()
        {
            GrantOperation operation = pubnub.Grant()
                .Channels(new[] { "ch-a" })
                .Read(true);

            Dictionary<string, string> query = CreateRequestParameter(operation).Query;

            Assert.That(query.ContainsKey("category"), Is.False);
            Assert.That(query["r"], Is.EqualTo("1"));
        }

        [Test]
        public void ThenCategoryGrantWithoutAuthKeyShouldThrow()
        {
            GrantOperation operation = pubnub.Grant()
                .Categories(new[] { PNGrantCategory.Channels })
                .Get(true);

            Assert.Throws<InvalidOperationException>(() => operation.Execute(new PNAccessManagerGrantResultExt((r, s) => { })));
        }

        [Test]
        public void ThenCategoryGrantWithOnlyEmptyAuthKeysShouldThrow()
        {
            GrantOperation operation = pubnub.Grant()
                .AuthKeys(new[] { "", " " })
                .Categories(new[] { PNGrantCategory.Channels })
                .Get(true);

            Assert.ThrowsAsync<InvalidOperationException>(async () => await operation.ExecuteAsync());
        }

        [TestCase("Read")]
        [TestCase("Write")]
        [TestCase("Manage")]
        [TestCase("Delete")]
        [TestCase("Update")]
        [TestCase("Join")]
        public void ThenCategoryGrantWithNonGetPermissionShouldThrow(string permission)
        {
            GrantOperation operation = SetPermission(pubnub.Grant()
                .AuthKeys(new[] { AuthKey })
                .Categories(new[] { PNGrantCategory.Channels })
                .Get(true), permission);

            Assert.Throws<InvalidOperationException>(() => operation.Execute(new PNAccessManagerGrantResultExt((r, s) => { })));
            Assert.ThrowsAsync<InvalidOperationException>(async () => await operation.ExecuteAsync());
        }

        [Test]
        public void ThenUndefinedCategoryShouldThrow()
        {
            GrantOperation operation = pubnub.Grant()
                .AuthKeys(new[] { AuthKey })
                .Categories(new[] { (PNGrantCategory)99 })
                .Get(true);

            Assert.Throws<InvalidOperationException>(() => operation.Execute(new PNAccessManagerGrantResultExt((r, s) => { })));
        }

        [Test]
        public void ThenCategoryLevelGrantResponseShouldPopulateCategories()
        {
            string response = "{\"message\":\"Success\",\"payload\":{\"level\":\"category\",\"subscribe_key\":\"sub-key\",\"ttl\":1440,\"categories\":{\"channels\":{\"auths\":{\"auth_key\":{\"r\":0,\"w\":0,\"m\":0,\"d\":0,\"g\":1,\"u\":0,\"j\":0}}},\"uuids\":{\"auths\":{\"auth_key\":{\"r\":0,\"w\":0,\"m\":0,\"d\":0,\"g\":1,\"u\":0,\"j\":0}}}}},\"service\":\"Access Manager\",\"status\":200}";

            PNAccessManagerGrantResult result = ParseGrantResponse(response);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Level, Is.EqualTo("category"));
            Assert.That(result.Ttl, Is.EqualTo(1440));
            Assert.That(result.Categories, Is.Not.Null);
            Assert.That(result.Categories.Keys, Is.EquivalentTo(new[] { "channels", "uuids" }));
            foreach (string category in new[] { "channels", "uuids" })
            {
                PNAccessManagerKeyData keyData = result.Categories[category]["auth_key"];
                Assert.That(keyData.GetEnabled, Is.True, $"{category}: Get should be granted");
                Assert.That(keyData.ReadEnabled || keyData.WriteEnabled || keyData.ManageEnabled || keyData.DeleteEnabled
                            || keyData.UpdateEnabled || keyData.JoinEnabled, Is.False, $"{category}: only Get should be granted");
            }
        }

        [Test]
        public void ThenGrantResponseWithoutCategoriesShouldLeaveCategoriesNull()
        {
            string response = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"sub-key\",\"ttl\":5,\"channel\":\"ch-a\",\"auths\":{\"auth_key\":{\"r\":1,\"w\":1,\"m\":0}}},\"service\":\"Access Manager\",\"status\":200}";

            PNAccessManagerGrantResult result = ParseGrantResponse(response);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Channels["ch-a"]["auth_key"].ReadEnabled, Is.True);
            Assert.That(result.Categories, Is.Null);
        }

        [Test]
        public void ThenTokenCategoriesShouldBeSerialized()
        {
            GrantTokenOperation operation = pubnub.GrantToken()
                .TTL(60)
                .Categories(new PNTokenCategories
                {
                    Channels = new PNTokenCategoryPermissions { Get = true },
                    Uuids = new PNTokenCategoryPermissions { Get = true }
                });

            string body = CreateRequestParameter(operation).BodyContentString;

            Assert.That(body, Does.Contain("\"categories\":{\"channels\":32,\"uuids\":32}"));
        }

        [Test]
        public void ThenChannelsCategoryWithChannelResourceShouldBeSerialized()
        {
            GrantTokenOperation operation = pubnub.GrantToken()
                .TTL(60)
                .Resources(new PNTokenResources
                {
                    Channels = new Dictionary<string, PNTokenAuthValues>
                    {
                        { "ch-a", new PNTokenAuthValues { Read = true } }
                    }
                })
                .Categories(new PNTokenCategories
                {
                    Channels = new PNTokenCategoryPermissions { Get = true }
                });

            string body = CreateRequestParameter(operation).BodyContentString;

            Assert.That(body, Does.Contain("\"channels\":{\"ch-a\":1}"));
            Assert.That(body, Does.Contain("\"categories\":{\"channels\":32}"));
        }

        [Test]
        public void ThenTokenCategoriesWithoutGetShouldBeOmitted()
        {
            PNTokenResources resources = new PNTokenResources
            {
                Channels = new Dictionary<string, PNTokenAuthValues>
                {
                    { "ch-a", new PNTokenAuthValues { Read = true } }
                }
            };
            PNTokenCategories[] categoriesWithoutGet =
            {
                null,
                new PNTokenCategories(),
                new PNTokenCategories { Channels = null, Uuids = null },
                new PNTokenCategories { Channels = new PNTokenCategoryPermissions { Get = false } }
            };

            Assert.That(CreateRequestParameter(pubnub.GrantToken().TTL(60).Resources(resources)).BodyContentString,
                Does.Not.Contain("categories"));
            foreach (PNTokenCategories categories in categoriesWithoutGet)
            {
                GrantTokenOperation operation = pubnub.GrantToken()
                    .TTL(60)
                    .Resources(resources)
                    .Categories(categories);

                Assert.That(CreateRequestParameter(operation).BodyContentString, Does.Not.Contain("categories"));
            }
        }

        [Test]
        public void ThenParseTokenShouldDecodeCategories()
        {
            string token = CreateToken(new Dictionary<object, object>
            {
                { "v", 2 },
                { "t", 1234567890 },
                { "ttl", 60 },
                { "cat", new Dictionary<object, object> { { "chan", 32 }, { "uuid", 32 } } }
            });

            PNTokenContent content = pubnub.ParseToken(token);

            Assert.That(content, Is.Not.Null);
            Assert.That(content.TTL, Is.EqualTo(60));
            Assert.That(content.Categories.Channels.Get, Is.True);
            Assert.That(content.Categories.Uuids.Get, Is.True);
        }

        [Test]
        public void ThenParseTokenWithChannelsCategoryOnlyShouldNotGrantUuidsCategory()
        {
            string token = CreateToken(new Dictionary<object, object>
            {
                { "v", 2 },
                { "t", 1234567890 },
                { "ttl", 60 },
                { "res", new Dictionary<object, object> { { "chan", new Dictionary<object, object> { { "ch-a", 1 } } } } },
                { "cat", new Dictionary<object, object> { { "chan", 32 } } }
            });

            PNTokenContent content = pubnub.ParseToken(token);

            Assert.That(content, Is.Not.Null);
            Assert.That(content.Categories.Channels.Get, Is.True);
            Assert.That(content.Categories.Uuids.Get, Is.False);
            Assert.That(content.Resources.Channels["ch-a"].Read, Is.True);
            Assert.That(content.Resources.Channels["ch-a"].Get, Is.False);
        }

        [Test]
        public void ThenParseTokenWithoutCategoriesShouldReturnEmptyCategories()
        {
            PNTokenContent content = pubnub.ParseToken(TokenWithoutCategories);

            Assert.That(content, Is.Not.Null);
            Assert.That(content.Categories, Is.Not.Null);
            Assert.That(content.Categories.Channels.Get, Is.False);
            Assert.That(content.Categories.Uuids.Get, Is.False);
        }
    }
}
