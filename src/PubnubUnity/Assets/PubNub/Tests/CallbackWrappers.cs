using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using PubnubApi;
using PubnubApi.EndPoint;
using PubnubApi.Unity;

namespace PubnubApi.Unity.Tests {
	public class CallbackWrappers : PNTestBase {
		private readonly string[] channels = new[] { "test" };
		private string channel => channels[0];

		[UnityTest]
		public IEnumerator Fire() {
			pn.Fire().Channel(channel).Message("test").Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator Signal() {
			pn.Signal().Channel(channel).Message("test").Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator ListFiles() {
			// PNStatus status = null;
			// float startTime = Time.time;
			// pn.ListFiles().Channel(channel).Execute(((result, pnStatus) => status = pnStatus));
			// yield return new WaitUntil(() => status == null || Time.time > startTime + 10f);
			// Assert.IsFalse(status.Error);

			pn.ListFiles().Channel(channel).Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator GetFileUrl() {
			pn.GetFileUrl().Channel(channel).FileId("").FileName("")
				.Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsTrue(s.Error);
			Assert.IsTrue(s.ErrorData.Information.Contains("Missing"));
		}

		[UnityTest]
		public IEnumerator SendFile() {
			pn.SendFile().Channel(channel).FileName("test").Message("test").File(new byte[] { 0x00 })
				.Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator DownloadFile() {
			pn.DownloadFile().Channel(channel).FileName("").FileId("")
				.Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsTrue(s.Error);
			Assert.IsTrue(s.ErrorData.Information.Contains("Missing"));
		}

		[UnityTest]
		public IEnumerator PublishFileMessage() {
			pn.PublishFileMessage().Channel(channel).FileName("").FileId("").Message("test")
				.Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsTrue(s.Error);
			Assert.IsTrue(s.ErrorData.Information.Contains("Missing"));
		}

		[UnityTest]
		public IEnumerator FetchHistory() {
			pn.FetchHistory().Channels(channels).Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator WhereNow() {
			pn.WhereNow().Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator GetChannelMembers() {
			pn.GetChannelMembers().Channel(channel).Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator SetChannelMemberships() {
			pn.SetChannelMembers().Channel(channel)
				.Uuids(new List<PNChannelMember>() { new PNChannelMember() { Uuid = "test" } })
				.Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}


		[UnityTest]
		public IEnumerator GetMessageActions() {
			pn.GetMessageActions().Channel(channel).Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator HereNow() {
			pn.HereNow().Channels(channels).Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator AddMessageAction() {
			pn.AddMessageAction().Channel(channel).Action(new PNMessageAction() { Type = "test", Value = "test" })
				.Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;

			Assert.IsTrue(!s.Error || s.ErrorData.Information.Contains("Already Added"));
		}

		[UnityTest]
		public IEnumerator RemoveMessageAction() {
			pn.RemoveMessageAction().Channel(channel).Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsTrue(s.Error);
		}


		[UnityTest]
		public IEnumerator SetMemberships() {
			pn.SetMemberships().Channels(new List<PNMembership>() { new PNMembership() { Channel = "test" } })
				.Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator GetMemberships() {
			pn.GetMemberships().Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator RemoveMemberships() {
			pn.RemoveMemberships().Uuid("test").Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator SetChannelMetadata() {
			pn.SetChannelMetadata().Channel(channel).Name("testname")
				.Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator GetChannelMetadata() {
			pn.GetChannelMetadata().Channel(channel).Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator RemoveChannelMetadata() {
			pn.RemoveChannelMetadata().Channel(channel).Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator AuditPushChannel() {
			pn.AuditPushChannelProvisions().Environment(PushEnvironment.Development).Topic("test")
				.Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator GetAllChannelMetadata() {
			pn.GetAllChannelMetadata().Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator RemoveChannelMembers() {
			pn.RemoveChannelMembers().Channel(channel).Uuids(new List<string>() { "test" })
				.Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator RemoveUuidMetadata() {
			pn.RemoveUuidMetadata().Uuid("test").Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator SetUuidMetadata() {
			pn.SetUuidMetadata().Uuid("test").Name("testname").Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator GetUuidMetadata() {
			pn.GetUuidMetadata().Uuid("test").Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator GetAllUuidMetadata() {
			pn.GetAllUuidMetadata().Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator RemoveAllPushNotificationsFromDeviceWithPushToken() {
			pn.RemoveAllPushNotificationsFromDeviceWithPushToken().Environment(PushEnvironment.Development)
				.Topic("test")
				.Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator RemovePushNotificationsFromChannels() {
			pn.RemovePushNotificationsFromChannels().Channels(channels).Environment(PushEnvironment.Development)
				.Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator AddPushNotificationOnChannels() {
			pn.AddPushNotificationsOnChannels().Channels(channels).Environment(PushEnvironment.Development)
				.Topic("test")
				.Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator DeleteMessages() {
			pn.DeleteMessages().Channel(channel).Start(0).End(99999999)
				.Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator MessageCounts() {
			pn.MessageCounts().Channels(channels).ChannelsTimetoken(new long[] { 0 })
				.Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator SetPresenceState() {
			pn.SetPresenceState().Channels(channels).State(new Dictionary<string, object>()).Uuid("test")
				.Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator DeleteChannelGroup() {
			pn.DeleteChannelGroup().ChannelGroup("testgroup").Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator AddChannelsToGroup() {
			pn.AddChannelsToChannelGroup().Channels(channels).ChannelGroup("testgroup")
				.Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator ListChannelGroups() {
			pn.ListChannelGroups().Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator RemoveChannelsFromChannelGroup() {
			pn.RemoveChannelsFromChannelGroup().Channels(channels).ChannelGroup("testgroup")
				.Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			Assert.IsFalse(s.Error);
		}

		[UnityTest]
		public IEnumerator GrantAndRevokeToken() {
			var tokenResources = new Dictionary<string, PNTokenAuthValues>()
				{ { "test", new PNTokenAuthValues() { Get = true } } };
			pn.GrantToken().AuthorizedUuid("test").TTL(10).Resources(new PNTokenResources() { Uuids = tokenResources })
				.Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;
			var t = assigner().result as PNAccessManagerTokenResult;
			Assert.IsFalse(s.Error);

			pn.RevokeToken().Token(t.Token).Execute(Callback(out awaiter, out assigner));
			yield return awaiter;
			s = assigner().status;
			Assert.IsFalse(s.Error);
		}
	}
}