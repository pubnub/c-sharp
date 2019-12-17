using UnityEngine;
using System.Threading;
using UnityEditor;
#if !UNITY_WSA_10_0
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using PubnubApi;
using System.Collections.Generic;
using System;
#endif

namespace PubNubAPI.Tests
{
	public class PlayModeTests {

        public PlayModeTests()
        {
            EditorApplication.playModeStateChanged += LogPlayModeState;
        }
        private static void LogPlayModeState(PlayModeStateChange state)
        {
            Debug.Log(state);
        }

#if !UNITY_WSA_10_0
        #region "Time"
        [UnityTest]
		public IEnumerator TestTime() {
			PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);

			Pubnub pubnub = new Pubnub(pnConfiguration);
			bool testReturn = false;
			pubnub.Time ().Execute (new PNTimeResultExt((result, status) => {
				bool statusError = status.Error;
				Debug.Log(statusError);
				bool resultTimeToken = result.Timetoken.Equals(0);
				Debug.Log(resultTimeToken);
				testReturn =  !statusError && !resultTimeToken;
            }));
			yield return new WaitForSeconds (PlayModeCommon.WaitTimeForAsyncResponse);
			Assert.True(testReturn, "test didn't return");
			pubnub.Destroy();
		}
		#endregion

		#region "WhereNow"
		[UnityTest]
		public IEnumerator TestWhereNow() {
			PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
			Pubnub pubnub = new Pubnub(pnConfiguration);
			System.Random r = new System.Random ();

			string whereNowChannel = "UnityTestWhereNowChannel"+ r.Next (100);

			pubnub.Subscribe<string>().Channels(new string[]{whereNowChannel}).WithPresence().Execute();
			yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);
			bool testReturn = false;
			pubnub.WhereNow().Uuid(pnConfiguration.Uuid).Execute (new PNWhereNowResultExt((result, status) => {
				bool statusError = status.Error;
				Debug.Log("statusError:" + statusError);

				if(result.Channels!=null){
					Debug.Log(result.Channels.Contains(whereNowChannel));
					testReturn = !statusError && result.Channels.Contains(whereNowChannel);
				} else {
					Assert.Fail("result.Channels null");
				}
             }));
			yield return new WaitForSeconds (PlayModeCommon.WaitTimeForAsyncResponse);
			Assert.True(testReturn, "test didn't return");
			pubnub.Destroy();
		}
		#endregion

		#region "HereNow"
		[UnityTest]
		public IEnumerator TestHereNowChannel() {
			PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
			pnConfiguration.Uuid = "UnityTestHereNowUUID";
			Pubnub pubnub = new Pubnub(pnConfiguration);
			string hereNowChannel = "UnityTestHereNowChannel";
			List<string> channelList = new List<string>();
			channelList.Add(hereNowChannel);
			foreach(string ch in channelList){
				Debug.Log("ch0:" + ch);
			}

			pubnub.Subscribe<string>().Channels(channelList.ToArray()).Execute();
			yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);
			bool testReturn = false;
			foreach(string ch in channelList){
				Debug.Log("ch:" + ch);
			}

			pubnub.HereNow().Channels(channelList.ToArray()).IncludeState(true).IncludeUUIDs(true)
                .Execute(new PNHereNowResultEx((result, status) => {
					Debug.Log("status.Error:" + status.Error);
					bool matchResult = MatchHereNowresult(pubnub, result, channelList, pnConfiguration.Uuid, false, false, true, 0, false, null);
					testReturn = !status.Error && matchResult;
                }));

			yield return new WaitForSeconds (PlayModeCommon.WaitTimeForAsyncResponse);
			Assert.True(testReturn, "test didn't return");
			pubnub.Destroy();
		}

		[UnityTest]
		public IEnumerator TestHereNowChannels() {
			PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
			pnConfiguration.Uuid = "UnityTestHereNowUUID";
			Pubnub pubnub = new Pubnub(pnConfiguration);
			string hereNowChannel = "UnityTestHereNowChannel1";
			string hereNowChannel2 = "UnityTestHereNowChannel2";
			List<string> channelList = new List<string>();
			channelList.Add(hereNowChannel);
			channelList.Add(hereNowChannel2);

			pubnub.Subscribe<string>().Channels(channelList.ToArray()).Execute();
			yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);
			bool testReturn = false;
			pubnub.HereNow().Channels(channelList.ToArray()).IncludeState(true).IncludeUUIDs(true).Execute(new PNHereNowResultEx((result, status) => {
					Debug.Log("status.Error:" + status.Error);
                    Assert.True(!status.Error);
					bool matchResult = MatchHereNowresult(pubnub, result, channelList, pnConfiguration.Uuid, false, false, true, 0, false, null);
                    testReturn = !status.Error && matchResult;
                }));

			yield return new WaitForSeconds (PlayModeCommon.WaitTimeForAsyncResponse);
			Assert.True(testReturn, "test didn't return");
			pubnub.Destroy();
		}

		[UnityTest]
		public IEnumerator TestHereNowChannelGroup() {
			PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
			pnConfiguration.Uuid = "UnityTestHereNowUUID";
			Pubnub pubnub = new Pubnub(pnConfiguration);
			string hereNowChannel = "UnityTestHereNowChannel";
			string channelGroup = "channelGroup1";
			List<string> channelList = new List<string>();
			channelList.Add(hereNowChannel);
			List<string> channelGroupList = new List<string>();
			channelGroupList.Add(channelGroup);

			pubnub.AddChannelsToChannelGroup().ChannelGroup(channelGroup).Channels(channelList.ToArray()).Execute(new PNChannelGroupsAddChannelResultExt((result, status) => {
                Debug.Log ("in AddChannelsToChannelGroup");
            }));
			yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);

			foreach(string ch in channelList){
				Debug.Log("ch0:" + ch);
			}

			pubnub.Subscribe<string>().ChannelGroups(channelGroupList.ToArray()).Execute();
			yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);
			bool testReturn = false;
			foreach(string ch in channelList){
				Debug.Log("ch:" + ch);
			}

			pubnub.HereNow().ChannelGroups(channelGroupList.ToArray()).IncludeState(true).IncludeUUIDs(true).Execute(new PNHereNowResultEx((result, status) => {
					Debug.Log("status.Error:" + status.Error);
                    Assert.True(!status.Error);
					//Assert.True(result.TotalOccupancy.Equals(1));
					bool matchResult = MatchHereNowresult(pubnub, result, channelList, pnConfiguration.Uuid, false, false, true, 0, false, null);
					testReturn = !status.Error && matchResult;
                }));

			yield return new WaitForSeconds (PlayModeCommon.WaitTimeForAsyncResponse);
			Assert.True(testReturn, "test didn't return");
			pubnub.Destroy();
		}

		[UnityTest]
		public IEnumerator TestHereNowChannelGroups() {
			PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
			pnConfiguration.Uuid = "UnityTestHereNowUUID";
			Pubnub pubnub = new Pubnub(pnConfiguration);
			string hereNowChannel = "UnityTestHereNowChannel";
			string hereNowChannel2 = "UnityTestHereNowChannel2";
			string channelGroup = "channelGroup2";
			List<string> channelList = new List<string>();
			channelList.Add(hereNowChannel);
			channelList.Add(hereNowChannel2);
			List<string> channelGroupList = new List<string>();
			channelGroupList.Add(channelGroup);
			pubnub.AddChannelsToChannelGroup().ChannelGroup(channelGroup).Channels(channelList.ToArray()).Execute(new PNChannelGroupsAddChannelResultExt((result, status) => {
                Debug.Log ("in AddChannelsToChannelGroup");
            }));
			yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);

			pubnub.Subscribe<string>().ChannelGroups(channelGroupList.ToArray()).Execute();
			yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);
			bool testReturn = false;
			pubnub.HereNow().ChannelGroups(channelGroupList.ToArray()).IncludeState(true).IncludeUUIDs(true).Execute(new PNHereNowResultEx((result, status) => {
					Debug.Log("status.Error:" + status.Error);
                    Assert.True(!status.Error);
					//Assert.True(result.TotalOccupancy.Equals(1));
					bool matchResult = MatchHereNowresult(pubnub, result, channelList, pnConfiguration.Uuid, false, false, true, 0, false, null);
                    testReturn = !status.Error && matchResult;
                }));

			yield return new WaitForSeconds (PlayModeCommon.WaitTimeForAsyncResponse);
			Assert.True(testReturn, "test didn't return");
			pubnub.Destroy();
		}

		[UnityTest]
		public IEnumerator TestHereNowChannelsAndChannelGroups() {
			PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
			pnConfiguration.Uuid = "UnityTestHereNowUUID";
            Pubnub pubnub = new Pubnub(pnConfiguration);
			string hereNowChannel = "UnityTestHereNowChannel3";
			string hereNowChannel2 = "UnityTestHereNowChannel4";
			string hereNowChannel3 = "UnityTestHereNowChannel5";
			string channelGroup = "channelGroup3";
			List<string> channelList = new List<string>();
			channelList.Add(hereNowChannel);
			channelList.Add(hereNowChannel2);
			List<string> channelList2 = new List<string>();
			channelList2.Add(hereNowChannel3);
			List<string> channelGroupList = new List<string>();
			channelGroupList.Add(channelGroup);
			pubnub.AddChannelsToChannelGroup().ChannelGroup(channelGroup).Channels(channelList.ToArray()).Execute(new PNChannelGroupsAddChannelResultExt((result, status) => {
                Debug.Log ("in AddChannelsToChannelGroup");
            }));
			yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);

			pubnub.Subscribe<string>().Channels(channelList2.ToArray()).ChannelGroups(channelGroupList.ToArray()).Execute();
			yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);
			bool testReturn = false;
			pubnub.HereNow().Channels(channelList2.ToArray()).ChannelGroups(channelGroupList.ToArray()).IncludeState(true).IncludeUUIDs(true).Execute(new PNHereNowResultEx((result, status) => {
					Debug.Log("status.Error:" + status.Error);
                    Assert.True(!status.Error);
					//Assert.True(result.TotalOccupancy.Equals(1));
					channelList.AddRange(channelList2);
					bool matchResult = MatchHereNowresult(pubnub, result, channelList, pnConfiguration.Uuid, false, false, true, 0, false, null);
                    testReturn = !status.Error && matchResult;
                }));

			yield return new WaitForSeconds (PlayModeCommon.WaitTimeForAsyncResponse);
			Assert.True(testReturn, "test didn't return");
			pubnub.Destroy();
		}

        [UnityTest]
        public IEnumerator TestHereNowWithUUIDWithState()
        {
            bool testReturn = false;

            PNConfiguration pnConfiguration = new PNConfiguration();
            pnConfiguration.Origin = PlayModeCommon.Origin;
            pnConfiguration.SubscribeKey = PlayModeCommon.SubscribeKey;
            pnConfiguration.PublishKey = PlayModeCommon.PublishKey;
            pnConfiguration.SecretKey = PlayModeCommon.SecretKey;
            pnConfiguration.LogVerbosity = PNLogVerbosity.BODY;
            pnConfiguration.Secure = true;
            pnConfiguration.ReconnectionPolicy = PNReconnectionPolicy.LINEAR;
            //pnConfiguration.PubnubLog = new InternalLog();
            pnConfiguration.Uuid = "TestHereNowWithUUIDWithState";

            Pubnub pubnub = new Pubnub(pnConfiguration);

            System.Random r = new System.Random();

            string hereNowChannel = "aaa";// "UnityTestHereNowChannel6" + r.Next (100);
            string hereNowChannel2 = "bbb";// "UnityTestHereNowChannel7" + r.Next (100);
            string hereNowChannel3 = "ccc";// "UnityTestHereNowChannel8" + r.Next (100);

            string channelGroup = "cgunity";// "channelGroup6" + r.Next (100);

            List<string> channelListForCg = new List<string>();
            channelListForCg.Add(hereNowChannel);
            channelListForCg.Add(hereNowChannel2);

            List<string> channelList = new List<string>();
            channelList.Add(hereNowChannel3);

            List<string> channelGroupList = new List<string>();
            channelGroupList.Add(channelGroup);
            pubnub.AddChannelsToChannelGroup().ChannelGroup(channelGroup).Channels(channelListForCg.ToArray()).Execute(new PNChannelGroupsAddChannelResultExt((result, status) =>
            {
                Debug.Log("in AddChannelsToChannelGroup = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls5);

            pubnub.ListChannelsForChannelGroup().ChannelGroup(channelGroup).Execute(new PNChannelGroupsAllChannelsResultExt((result, status) => 
            {
                Debug.Log("in ListChannelsForChannelGroup = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls);

            SubscribeCallbackExt listener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNMessageResult<object> pubMsg) { },
                delegate (Pubnub pnObj, PNPresenceEventResult presenceEvnt) { /* Debug.Log("presenceEvnt = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(presenceEvnt)); */ },
                delegate (Pubnub pnObj, PNStatus pnStatus) { /* Debug.Log("pnStatus = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(pnStatus)); */ }
                );
            pubnub.AddListener(listener);
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls);

            pubnub.Subscribe<string>().Channels(channelList.ToArray()).ChannelGroups(channelGroupList.ToArray()).WithPresence().Execute();
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls5);

            Dictionary<string, object> state = new Dictionary<string, object>();
            state.Add("k", "v");
            pubnub.SetPresenceState().Channels(channelList.ToArray()).ChannelGroups(channelGroupList.ToArray()).State(state).Execute(new PNSetStateResultExt((result, status) =>
            {
                Debug.Log("in SetPresenceState = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls5);
            pubnub.GetPresenceState().Channels(channelList.ToArray()).ChannelGroups(channelGroupList.ToArray()).Execute(new PNGetStateResultExt((result, status) =>
            {
                Debug.Log("in GetPresenceState = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls);
            //pubnub.SetPresenceState().Channels(channelList.ToArray()).State(state).Execute(new PNSetStateResultExt((result, status) => {
            //    Debug.Log("in SetPresenceState = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            //}));
            //yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls);
            pubnub.HereNow().Channels(channelList.ToArray()).ChannelGroups(channelGroupList.ToArray()).IncludeState(true).IncludeUUIDs(true).Execute(new PNHereNowResultEx((result, status) =>
            {
                Debug.Log("status.Error:" + status.Error);
                Debug.Log("HereNow = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                Debug.Log("status = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                Assert.True(!status.Error);
                channelListForCg.AddRange(channelList);
                bool matchResult = MatchHereNowresult(pubnub, result, channelListForCg, pnConfiguration.Uuid, false, true, false, 1, true, state);
                testReturn = !status.Error && matchResult;
            }));

            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls5);

            pubnub.Unsubscribe<string>().Channels(channelList.ToArray()).ChannelGroups(channelGroupList.ToArray()).Execute();
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls1);

            Assert.True(testReturn, "test didn't return");
            pubnub.Destroy();
        }


        //[UnityTest]
        public IEnumerator TestGlobalHereNow() {
			PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
			pnConfiguration.Uuid = "UnityTestHereNowUUID";
			Pubnub pubnub = new Pubnub(pnConfiguration);
			System.Random r = new System.Random ();

			string hereNowChannel = "UnityTestHereNowChannel6"+ r.Next (100);
			string hereNowChannel2 = "UnityTestHereNowChannel7"+ r.Next (100);
			string hereNowChannel3 = "UnityTestHereNowChannel8"+ r.Next (100);
			string channelGroup = "channelGroup4"+ r.Next (100);
			List<string> channelList = new List<string>();
			channelList.Add(hereNowChannel);
			channelList.Add(hereNowChannel2);
			List<string> channelList2 = new List<string>();
			channelList2.Add(hereNowChannel3);
			List<string> channelGroupList = new List<string>();
			channelGroupList.Add(channelGroup);
			pubnub.AddChannelsToChannelGroup().ChannelGroup(channelGroup).Channels(channelList.ToArray()).Execute(new PNChannelGroupsAddChannelResultExt((result, status) => {
                Debug.Log ("in AddChannelsToChannelGroup");
            }));
			yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);

			pubnub.Subscribe<string>().Channels(channelList2.ToArray()).ChannelGroups(channelGroupList.ToArray()).Execute();
			yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);
			bool testReturn = false;
			pubnub.HereNow().IncludeState(true).IncludeUUIDs(true).Execute(new PNHereNowResultEx((result, status) => {
					Debug.Log("status.Error:" + status.Error);
                    Assert.True(!status.Error);
					//Assert.True(result.TotalOccupancy.Equals(1));
					channelList.AddRange(channelList2);
					bool matchResult = MatchHereNowresult(pubnub, result, channelList, pnConfiguration.Uuid, false, false, true, 0, false, null);
					
                    testReturn = !status.Error && matchResult;
                }));

			yield return new WaitForSeconds (PlayModeCommon.WaitTimeForAsyncResponse);
			Assert.True(testReturn, "test didn't return");
			pubnub.Destroy();
		}

		[UnityTest]
		public IEnumerator TestGlobalHereNowWithoutUUID() {
			PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
			pnConfiguration.Uuid = "UnityTestHereNowUUID";
			Pubnub pubnub = new Pubnub(pnConfiguration);
			System.Random r = new System.Random ();

			string hereNowChannel = "UnityTestHereNowChannel6"+ r.Next (100);
			string hereNowChannel2 = "UnityTestHereNowChannel7"+ r.Next (100);
			string hereNowChannel3 = "UnityTestHereNowChannel8"+ r.Next (100);

			string channelGroup = "channelGroup5"+ r.Next (100);
			List<string> channelList = new List<string>();
			channelList.Add(hereNowChannel);
			channelList.Add(hereNowChannel2);
			List<string> channelList2 = new List<string>();
			channelList2.Add(hereNowChannel3);
			List<string> channelGroupList = new List<string>();
			channelGroupList.Add(channelGroup);
			pubnub.AddChannelsToChannelGroup().ChannelGroup(channelGroup).Channels(channelList.ToArray()).Execute(new PNChannelGroupsAddChannelResultExt((result, status) => {
                Debug.Log ("in AddChannelsToChannelGroup");
            }));
			yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);

			pubnub.Subscribe<string>().Channels(channelList2.ToArray()).ChannelGroups(channelGroupList.ToArray()).Execute();
			yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);
			bool testReturn = false;
			pubnub.HereNow().IncludeState(true).IncludeUUIDs(false).Execute(new PNHereNowResultEx((result, status) => {
					Debug.Log("status.Error:" + status.Error);
                    Assert.True(!status.Error);
					//Assert.True(resultTotalOccupancy.Equals(1));
					channelList.AddRange(channelList2);
					bool matchResult = MatchHereNowresult(pubnub, result, channelList, pnConfiguration.Uuid, false, true, false, 1, false, null);
                    testReturn = !status.Error && matchResult;
                }));

			yield return new WaitForSeconds (PlayModeCommon.WaitTimeForAsyncResponse);
			Assert.True(testReturn, "test didn't return");
			pubnub.Destroy();
		}

		//[UnityTest]
		public IEnumerator TestGlobalHereNowWithoutUUIDWithState() {
			PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
			System.Random r = new System.Random ();
			pnConfiguration.Uuid = "UnityTestHereNowUUID"+ r.Next (100);
			Pubnub pubnub = new Pubnub(pnConfiguration);			

			string hereNowChannel = "UnityTestHereNowChannel6"+ r.Next (100);
			string hereNowChannel2 = "UnityTestHereNowChannel7"+ r.Next (100);
			string hereNowChannel3 = "UnityTestHereNowChannel8"+ r.Next (100);
			string channelGroup = "channelGroup6"+ r.Next (100);
			List<string> channelList = new List<string>();
			channelList.Add(hereNowChannel);
			channelList.Add(hereNowChannel2);
			List<string> channelList2 = new List<string>();
			channelList2.Add(hereNowChannel3);
			List<string> channelGroupList = new List<string>();
			channelGroupList.Add(channelGroup);
			pubnub.AddChannelsToChannelGroup().ChannelGroup(channelGroup).Channels(channelList.ToArray()).Execute(new PNChannelGroupsAddChannelResultExt((result, status) => {
                Debug.Log ("in AddChannelsToChannelGroup");
            }));
			yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);

			pubnub.Subscribe<string>().Channels(channelList2.ToArray()).ChannelGroups(channelGroupList.ToArray()).Execute();
			yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);
			Dictionary<string, object> state = new Dictionary<string, object>();
			state.Add("k", "v");
			pubnub.SetPresenceState().Channels(channelList.ToArray()).ChannelGroups(channelGroupList.ToArray()).State(state).Execute(new PNSetStateResultExt((result, status) => {
                
            }));
			yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);
			bool testReturn = false;
			pubnub.HereNow().IncludeState(true).IncludeUUIDs(false).Execute(new PNHereNowResultEx((result, status) => {
					Debug.Log("status.Error:" + status.Error);
                    Assert.True(!status.Error);
					//Assert.True(resultTotalOccupancy.Equals(1));
					channelList.AddRange(channelList2);
					bool matchResult = MatchHereNowresult(pubnub, result, channelList, pnConfiguration.Uuid, false, true, false, 1, true, state);
					testReturn = !status.Error && matchResult;
                }));

			yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls3);
			Assert.True(testReturn, "test didn't return");
			pubnub.Destroy();
		}


        public bool MatchHereNowresult(Pubnub pubnub, PNHereNowResult result, List<string> channelList, string uuid, bool checkOccupancy, bool checkOccupancyOnly, bool checkOccupantData, int occupancy, bool checkState, Dictionary<string, object> state){
			bool matchResult = false;
			if(result.Channels!=null){
				Dictionary<string, PNHereNowChannelData> dict = result.Channels;
				PNHereNowChannelData pnHereNowChannelData;
				
				foreach(string hereNowChannel in channelList){
					if(dict.TryGetValue(hereNowChannel, out pnHereNowChannelData)){
						if(checkOccupancy || checkOccupancyOnly){
							matchResult = pnHereNowChannelData.Occupancy.Equals(occupancy);
							Debug.Log("Occupancy.Equals:" + matchResult);
						}

						if (checkState || checkOccupantData){
							bool found = false;
							bool checkStateResult = false;
							foreach(PNHereNowOccupantData pnHereNowOccupantData in pnHereNowChannelData.Occupants){
								Debug.Log("finding:" + pnHereNowOccupantData.Uuid);
								
								if(checkState){
									Debug.Log(state.ToString());
									
									checkStateResult = pnHereNowOccupantData.State.Equals(pubnub.JsonPluggableLibrary.SerializeToJsonString(state));
									Debug.Log("checkStateResult:" + checkStateResult);
								}
								
								if(checkOccupantData){
									if(pnHereNowOccupantData.Uuid.Equals(uuid)){
										found = true;
										Debug.Log("found:" + pnHereNowOccupantData.Uuid);
										break;
									} 
								}
							}
							if(checkState && checkOccupantData){
								matchResult = checkStateResult && found;
							} else if(checkOccupantData){
								matchResult = found;
							} else if (checkState){
								matchResult = checkState;
							}
							
						}
					}else {
						Assert.Fail("channel not found" + hereNowChannel);
					}
				}
				
			} else {
				Assert.Fail("Channels null");
			}
			Debug.Log("matchResult:" + matchResult.ToString());
			return matchResult;
		}
		#endregion

		#region "Publish"
		[UnityTest]
		public IEnumerator TestPublishString() {
			string publishChannel = "UnityTestPublishChannel";
			string payload = string.Format("test message {0}", DateTime.Now.Ticks.ToString());
			yield return DoPublishTestProcsssing(payload, publishChannel);
		}

		[UnityTest]
		public IEnumerator TestPublishInt() {
			string publishChannel = "UnityTestPublishChannel";
			object payload = 1;
			yield return DoPublishTestProcsssing(payload, publishChannel);
		}

		[UnityTest]
		public IEnumerator TestPublishDouble() {
			string publishChannel = "UnityTestPublishChannel";
			double payload = 1.1;
			yield return DoPublishTestProcsssing(payload, publishChannel);
		}

		[UnityTest]
		public IEnumerator TestPublishDoubleArr() {
			string publishChannel = "UnityTestPublishChannel";
			double[] payload = {1.1};
			yield return DoPublishTestProcsssing(payload, publishChannel);
		}

		[UnityTest]
		public IEnumerator TestPublishEmptyArr() {
			string publishChannel = "UnityTestPublishChannel";
			object[] payload = {};
			yield return DoPublishTestProcsssing(payload, publishChannel);
		}

		[UnityTest]
		public IEnumerator TestPublishEmptyDict() {
			string publishChannel = "UnityTestPublishChannel";
			Dictionary<string, int> payload = new Dictionary<string, int>();
			yield return DoPublishTestProcsssing(payload, publishChannel);
		}

		[UnityTest]
		public IEnumerator TestPublishDict() {
			string publishChannel = "UnityTestPublishChannel";
			Dictionary<string, string> payload = new Dictionary<string, string>();
			payload.Add("cat", "test");
			yield return DoPublishTestProcsssing(payload, publishChannel);
		}

		[UnityTest]
		public IEnumerator TestPublishLong() {
			string publishChannel = "UnityTestPublishChannel";
			long payload = 14255515120803306;
			yield return DoPublishTestProcsssing(payload, publishChannel);
		}

		[UnityTest]
		public IEnumerator TestPublishLongArr() {
			string publishChannel = "UnityTestPublishChannel";
			long[] payload = {14255515120803306};
			yield return DoPublishTestProcsssing(payload, publishChannel);
		}

		[UnityTest]
		public IEnumerator TestPublishIntArr() {
			string publishChannel = "UnityTestPublishChannel";
			int[] payload = {13, 14};
			yield return DoPublishTestProcsssing(payload, publishChannel);
		}

		[UnityTest]
		public IEnumerator TestPublishStringArr() {
			string publishChannel = "UnityTestPublishChannel";
			string[] payload = {"testarr"};
			yield return DoPublishTestProcsssing(payload, publishChannel);
		}

		[UnityTest]
		public IEnumerator TestPublishComplexMessage() {
			string publishChannel = "UnityTestPublishChannel";
			object payload = new PubnubDemoObject ();
			yield return DoPublishTestProcsssing(payload, publishChannel);
		}

        [UnityTest]
        public IEnumerator TestJoinLeave() {
			string channel = "UnityTestJoinChannel";
			yield return DoJoinLeaveTestProcsssing(channel);
		}

		[UnityTest]
		public IEnumerator TestConnected() {
			PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
			System.Random r = new System.Random ();
			pnConfiguration.Uuid = "UnityTestConnectedUUID_" + r.Next (100);
			string channel = "UnityTestConnectedChannel";

			Pubnub pubnub = new Pubnub(pnConfiguration);
			List<string> channelList2 = new List<string>();
			channelList2.Add(channel);
			bool tresult = false;
            ManualResetEvent mre = new ManualResetEvent(false);
            SubscribeCallbackExt listener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNMessageResult<object> pubMsg) { },
                delegate (Pubnub pnObj, PNPresenceEventResult presenceEvnt) { },
                delegate (Pubnub pnObj, PNStatus pnStatus) 
                {
                    if (pnStatus.Category == PNStatusCategory.PNConnectedCategory && pnStatus.AffectedChannels.Contains(channel))
                    {
                        tresult = true;
                        Assert.True(tresult);
                        mre.Set();
                    }
                }
                );
            pubnub.AddListener(listener);
			pubnub.Subscribe<string>().Channels(channelList2.ToArray()).Execute();
            mre.WaitOne(6*1000);
            pubnub.Unsubscribe<string>().Channels(channelList2.ToArray()).Execute();
            pubnub.Destroy();
            yield return new WaitForSeconds(7);
            Assert.True(tresult, "test didn't return");

        }

        private class InternalLog : IPubnubLog
        {
            void IPubnubLog.WriteToLog(string logText)
            {
                Debug.Log("InternalLog : " + logText);
            }
        }

        SubscribeCallbackExt joinLeaveListener = null;
        public IEnumerator DoJoinLeaveTestProcsssing(string channel) {
			PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
            pnConfiguration.PresenceTimeout = 300;
            pnConfiguration.SetPresenceTimeoutWithCustomInterval(300, 0);
            //pnConfiguration.LogVerbosity = PNLogVerbosity.BODY;
            //pnConfiguration.PubnubLog = new InternalLog();

            System.Random r = new System.Random ();
			channel = channel+ r.Next (100);
			pnConfiguration.Uuid = "UnityTestJoinUUID_" + r.Next (100);
			Pubnub pubnub = new Pubnub(pnConfiguration);
            string secondInstanceUuid = "UnityTestJoinUUID_" + r.Next(100);
            pnConfiguration.Uuid = secondInstanceUuid;
            Pubnub pubnub2 = new Pubnub(pnConfiguration);
			
			List<string> channelList2 = new List<string>();
			channelList2.Add(channel);
			bool tJoinResult = false;
			bool tLeaveResult = false;

            //ManualResetEvent mre = new ManualResetEvent(false);
            joinLeaveListener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNMessageResult<object> pubMsg) { },
                delegate (Pubnub pnObj, PNPresenceEventResult presenceEvnt) 
                {
                    Debug.Log(pubnub.JsonPluggableLibrary.SerializeToJsonString(presenceEvnt));
                    if (presenceEvnt.Event.Equals("join"))
                    {
                        Debug.Log(presenceEvnt.Uuid);
                        Debug.Log(presenceEvnt.Timestamp);
                        Debug.Log(presenceEvnt.Occupancy);
                        //Debug.Log(string.Join(",", presenceEvnt.Join));
                        bool containsUUID = presenceEvnt.Uuid.Contains(pnConfiguration.Uuid);
                        Assert.True(containsUUID);
                        bool containsOccupancy = presenceEvnt.Occupancy > 0;
                        Assert.True(containsOccupancy);
                        bool containsTimestamp = presenceEvnt.Timestamp > 0;
                        Assert.True(containsTimestamp);

                        tJoinResult = containsTimestamp && containsOccupancy && containsUUID;
                        Debug.Log("containsUUID " + containsUUID + " " + tJoinResult);

                    }
                    else if (presenceEvnt.Event.Equals("leave"))
                    {
                        bool containsUUID = presenceEvnt.Uuid.Contains(pnConfiguration.Uuid);
                        Assert.True(containsUUID);
                        bool containsTimestamp = presenceEvnt.Timestamp > 0;
                        Assert.True(containsTimestamp);
                        tLeaveResult = containsTimestamp && containsUUID;

                        Debug.Log("containsUUID " + containsUUID + " " + tLeaveResult);
                    }
                    //mre.Set();
                },
                delegate (Pubnub pnObj, PNStatus pnStatus) { }
                );
            pubnub.AddListener(joinLeaveListener);
            pubnub2.AddListener(joinLeaveListener);

            pubnub.Subscribe<string>().Channels(channelList2.ToArray()).WithPresence().Execute();
            //mre.WaitOne(5 * 1000);
            //yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls3);
            yield return new WaitForSeconds(7);

            Assert.True(tJoinResult, "join test didn't return");

            //mre = new ManualResetEvent(false);
            pubnub2.Subscribe<string>().Channels(channelList2.ToArray()).WithPresence().Execute();
            //mre.WaitOne(5 * 1000);
            //yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls3);
            yield return new WaitForSeconds (7);			

            //mre = new ManualResetEvent(false);
            pubnub2.Unsubscribe<string>().Channels(channelList2.ToArray()).Execute();
            //mre.WaitOne(15 * 1000);

            yield return new WaitForSeconds (7);
			Assert.True(tLeaveResult, "leave test didn't return");
			pubnub.Destroy();
			pubnub2.Destroy();
		}

		[UnityTest]
		public IEnumerator TestPublishLoadTest() {
			string publishChannel = "UnityTestPublishChannel";
			Dictionary<string, bool> payload = new Dictionary<string, bool>();
			for(int i=0; i<50; i++){
				payload.Add(string.Format("payload {0}", i), false);
			}
			
			PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
			pnConfiguration.Uuid = "UnityTestPublishLoadTestUUID";
			Pubnub pubnub = new Pubnub(pnConfiguration);
			List<string> channelList2 = new List<string>();
			channelList2.Add(publishChannel);
            //bool testReturn = false;

            //ManualResetEvent mre = new ManualResetEvent(false);
            SubscribeCallbackExt listener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNMessageResult<object> pubMsg) 
                {
                    if (payload.ContainsKey(pubMsg.Message.ToString()))
                    {
                        payload[pubMsg.Message.ToString()] = true;
                    }
                },
                delegate (Pubnub pnObj, PNPresenceEventResult presenceEvnt) { },
                delegate (Pubnub pnObj, PNStatus pnStatus)
                {
                }
                );
            pubnub.AddListener(listener);


            pubnub.Subscribe<string>().Channels(channelList2.ToArray()).Execute();
			yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);

			foreach(KeyValuePair<string, bool> kvp in payload){
				pubnub.Publish().Channel(publishChannel).Message(kvp.Key).Execute(new PNPublishResultExt((result, status) => {
					Assert.True(!result.Timetoken.Equals(0));
					Assert.True(status.Error.Equals(false));
					Assert.True(status.StatusCode.Equals(0), status.StatusCode.ToString());
				}));
			}
			yield return new WaitForSeconds (20);

			bool tresult = false;
			foreach(KeyValuePair<string, bool> kvp in payload){
				if(!kvp.Value){
					Debug.Log("=======>>>>>>>>" + kvp.Key);
					tresult = true;
				}
			}

			Assert.True(!tresult);
			pubnub.Destroy();
		}

		public IEnumerator DoPublishTestProcsssing(object payload, string publishChannel){
			Debug.Log("PAYLOAD:"+payload.ToString());
			PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
			pnConfiguration.Uuid = "UnityTestPublishUUID";
			Pubnub pubnub = new Pubnub(pnConfiguration);
			List<string> channelList2 = new List<string>();
			channelList2.Add(publishChannel);
			
			bool testReturn = false;
			

            SubscribeCallbackExt listener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNMessageResult<object> pubMsg)
                {
                    Debug.Log("PAYLOAD20:" + payload.ToString() + " " + payload.GetType());

                    Assert.True(pubMsg.Channel.Equals(publishChannel));
                    if (payload.GetType().Equals(typeof(Int32)))
                    {
                        long expected;
                        if (Int64.TryParse(payload.ToString(), out expected))
                        {
                            int response;
                            if (Int32.TryParse(pubMsg.Message.ToString(), out response))
                            {
                                bool expectedAndResponseMatch = expected.Equals(response);
                                Assert.IsTrue(expectedAndResponseMatch);
                                testReturn = expectedAndResponseMatch;
                            }
                            else
                            {
                                Assert.Fail("response int conversion failed");
                            }
                        }
                        else
                        {
                            Assert.Fail("expected int conversion failed");
                        }
                    }
                    else if (payload.GetType().Equals(typeof(Int64)))
                    {
                        long expected;
                        if (Int64.TryParse(payload.ToString(), out expected))
                        {
                            long response;
                            if (Int64.TryParse(pubMsg.Message.ToString(), out response))
                            {
                                bool expectedAndResponseMatch = expected.Equals(response);
                                Assert.IsTrue(expectedAndResponseMatch);
                                testReturn = expectedAndResponseMatch;
                            }
                            else
                            {
                                Assert.Fail("response long conversion failed");
                            }
                        }
                        else
                        {
                            Assert.Fail("expected long conversion failed");
                        }
                    }
                    else if (payload.GetType().Equals(typeof(double)))
                    {
                        double expected;
                        if (double.TryParse(payload.ToString(), out expected))
                        {
                            double response;
                            if (double.TryParse(pubMsg.Message.ToString(), out response))
                            {
                                bool expectedAndResponseMatch = expected.Equals(response);
                                Assert.IsTrue(expectedAndResponseMatch);
                                testReturn = expectedAndResponseMatch;
                            }
                            else
                            {
                                Assert.Fail("response double conversion failed");
                            }
                        }
                        else
                        {
                            Assert.Fail("expected long conversion failed");
                        }
                    }
                    else if (payload.GetType().Equals(typeof(Int64[])))
                    {
                        string serializedMsg = pubnub.JsonPluggableLibrary.SerializeToJsonString(pubMsg.Message);
                        Int64[] response = pubnub.JsonPluggableLibrary.DeserializeToObject<Int64[]>(serializedMsg);
                        Debug.Log(pubMsg.Message.GetType());
                        Debug.Log(response.GetType().Equals(typeof(Int64[])));
                        Int64[] expected = (Int64[])payload;
                        //string[] response = (string[])pubMsg.Message;
                        foreach (Int64 iExp in expected)
                        {
                            bool found = false;
                            foreach (Int64 iResp in response)
                            {
                                if (iExp.Equals(iResp))
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                            {
                                Assert.Fail("response not found");
                            }
                            else
                            {
                                testReturn = found;
                            }
                        }
                    }
                    else if (payload.GetType().Equals(typeof(double[])))
                    {
                        string serializedMsg = pubnub.JsonPluggableLibrary.SerializeToJsonString(pubMsg.Message);
                        double[] response = pubnub.JsonPluggableLibrary.DeserializeToObject<double[]>(serializedMsg);
                        Debug.Log(pubMsg.Message.GetType());
                        Debug.Log(response.GetType().Equals(typeof(double[])));
                        double[] expected = (double[])payload;
                        foreach (double iExp in expected)
                        {
                            Debug.Log("iExp = " + iExp.ToString());
                            bool found = false;
                            foreach (double iResp in response)
                            {
                                Debug.Log("iResp = " + iResp.ToString());
                                if (iExp.Equals(iResp))
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                            {
                                Assert.Fail("response not found");
                            }
                            else
                            {
                                testReturn = found;
                            }
                        }
                    }
                    else if (payload.GetType().Equals(typeof(string[])))
                    {
                        string serializedMsg = pubnub.JsonPluggableLibrary.SerializeToJsonString(pubMsg.Message);
                        string[] response = pubnub.JsonPluggableLibrary.DeserializeToObject<string[]>(serializedMsg);
                        string[] expected = (string[])payload;
                        //string[] response = (string[])pubMsg.Message;
                        foreach (string strExp in expected)
                        {
                            bool found = false;
                            foreach (string strResp in response)
                            {
                                if (strExp.Equals(strResp))
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                            {
                                Assert.Fail("response not found");
                            }
                            else
                            {
                                testReturn = found;
                            }
                        }
                    }
                    else if (payload.GetType().Equals(typeof(System.Object[])))
                    {
                        string serializedMsg = pubnub.JsonPluggableLibrary.SerializeToJsonString(pubMsg.Message);
                        object[] response = pubnub.JsonPluggableLibrary.DeserializeToObject<object[]>(serializedMsg);
                        System.Object[] expected = (System.Object[])payload;
                        //System.Object[] response = (System.Object[])pubMsg.Message;
                        // + payload.GetType().Equals(typeof(System.Object[])) + expected[0].Equals(response[0]));
                        bool expectedAndResponseMatch = expected.Length.Equals(response.Length) && expected.Length.Equals(0);
                        Assert.IsTrue(expectedAndResponseMatch);
                        testReturn = expectedAndResponseMatch;
                    }
                    else if (payload.GetType().Equals(typeof(Dictionary<string, string>)))
                    {
                        Dictionary<string,object> response = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(pubMsg.Message);
                        Dictionary<string, string> expected = (Dictionary<string, string>)payload;
                        //IDictionary response = pubMsg.Message as IDictionary;
                        Debug.Log("PAYLOAD21:" + payload.ToString() + payload.GetType());
                        //Assert.True(response["cat"].Equals("test"));
                        bool expectedAndResponseMatch = response["cat"].Equals("test");
                        testReturn = expectedAndResponseMatch;
                    }
                    else if (payload.GetType().Equals(typeof(Dictionary<string, int>)))
                    {
                        Dictionary<string, int> expected = (Dictionary<string, int>)payload;
                        IDictionary response = pubMsg.Message as IDictionary;
                        Debug.Log("PAYLOAD22:" + payload.ToString() + payload.GetType());
                        bool expectedAndResponseMatch = (response == null || response.Count < 1);
                        Assert.IsTrue(expectedAndResponseMatch);
                        testReturn = expectedAndResponseMatch;

                        //Assert.True(expected.Count.Equals(response.Count) && expected.Count.Equals(0));

                    }
                    else if (payload.GetType().Equals(typeof(Int32[])))
                    {
                        string serializedMsg = pubnub.JsonPluggableLibrary.SerializeToJsonString(pubMsg.Message);
                        Int32[] response = pubnub.JsonPluggableLibrary.DeserializeToObject<Int32[]>(serializedMsg); 
                        Int32[] expected = (Int32[])payload;
                        //Int32[] response = (Int32[])pubMsg.Message;
                        foreach (int iExp in expected)
                        {
                            bool found = false;
                            foreach (int iResp in response)
                            {
                                if (iExp.Equals(iResp))
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                            {
                                Assert.Fail("response not found");
                            }
                            else
                            {
                                testReturn = found;
                            }
                        }
                    }
                    else if (payload.GetType().Name == "PubnubDemoObject")
                    {
                        string serializedMsg = pubnub.JsonPluggableLibrary.SerializeToJsonString(pubMsg.Message);
                        PubnubDemoObject response = new PubnubDemoObject();
                        pubnub.JsonPluggableLibrary.PopulateObject(serializedMsg, response);

                        Debug.Log("PAYLOAD2 PubnubDemoObject:" + payload.ToString() + payload.GetType());
                        PubnubDemoObject expected = payload as PubnubDemoObject;

                        //Debug.Log("PopulateObject = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(response));
                        //Debug.Log("payload = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(payload));
                        //Debug.Log("expected = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(expected));
                        Debug.Log(pubMsg.Message == null);
                        Debug.Log(response.GetType());
                        
                        Debug.Log("expected.VersionID:" + expected.VersionID);
                        Debug.Log("response.VersionID:" + response.VersionID);

                        bool versionIdMatch = response.VersionID.Equals(expected.VersionID);
                        bool timetokenMatch = response.Timetoken.Equals(expected.Timetoken);
                        bool operationNameMatch = response.OperationName.Equals(expected.OperationName);
                        bool demoMessageMatch = response.DemoMessage.DefaultMessage.Equals(expected.DemoMessage.DefaultMessage);
                        bool customMessageMatch = response.CustomMessage.DefaultMessage.Equals(expected.CustomMessage.DefaultMessage);

                        testReturn = versionIdMatch && timetokenMatch && operationNameMatch && demoMessageMatch && customMessageMatch;
                    }
                    else
                    {
                        Debug.Log("PAYLOAD24:" + payload.ToString() + payload.GetType());
                        testReturn = pubMsg.Message.Equals(payload);
                    }
                    //testReturn = true;
                },
                delegate (Pubnub pnObj, PNPresenceEventResult presenceEvnt) { },
                delegate (Pubnub pnObj, PNStatus pnStatus)
                {
                    if (!pnStatus.Category.Equals(PNStatusCategory.PNConnectedCategory))
                    {
                        
                    }
                }
                );
            pubnub.AddListener(listener);
            pubnub.Subscribe<object>().Channels(channelList2.ToArray()).Execute();
			yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);
			
			pubnub.Publish().Channel(publishChannel).Message(payload).Execute(new PNPublishResultExt((result, status) => {
				Assert.True(!result.Timetoken.Equals(0));
				Assert.True(status.Error.Equals(false));
				Assert.True(status.StatusCode.Equals(0), status.StatusCode.ToString());
			}));
			yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls3);
			Assert.True(testReturn, "test didn't return");
			pubnub.Destroy();
		}

		[UnityTest]
		public IEnumerator TestSubscribeWithTT() {
			PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
			System.Random r = new System.Random ();
			pnConfiguration.Uuid = "UnityTestConnectedUUID_" + r.Next (100);
			string channel = "UnityTestWithTTLChannel";
			string payload = string.Format("payload {0}", pnConfiguration.Uuid);

			Pubnub pubnub = new Pubnub(pnConfiguration);
			long timetoken = 0;
			pubnub.Time().Execute(new PNTimeResultExt((result, status) => {
				timetoken = result.Timetoken;
			}));
			yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);
			Assert.True(!timetoken.Equals(0));

			pubnub.Publish().Channel(channel).Message(payload).Execute(new PNPublishResultExt((result, status) => {
				Assert.True(!result.Timetoken.Equals(0));
				Assert.True(status.Error.Equals(false));
				Assert.True(status.StatusCode.Equals(0), status.StatusCode.ToString());
			}));

			List<string> channelList2 = new List<string>();
			channelList2.Add(channel);
			bool tresult = false;

			SubscribeCallbackExt listener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNMessageResult<object> pubMsg)
                {
                    tresult = pubMsg.Channel.Equals(channel) && pubMsg.Message.ToString().Equals(payload);
                },
                delegate (Pubnub pnObj, PNPresenceEventResult presenceEvnt) { },
                delegate (Pubnub pnObj, PNStatus pnStatus)
                {
                    
                }
                );
            pubnub.AddListener(listener);
            pubnub.Subscribe<string>().Channels(channelList2.ToArray()).WithTimetoken(timetoken).Execute();
			yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls3);
			Assert.True(tresult, "test didn't return");
			pubnub.Destroy();

		}

		[UnityTest]
		public IEnumerator TestSignalsAndSubscribe() {
			PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
			System.Random r = new System.Random ();
			pnConfiguration.Uuid = "UnityTestConnectedUUID_" + r.Next (100);
			string channel = "UnityTestSignalChannel_"  + r.Next (100);
			string payload = string.Format("Signal {0}", r.Next (100));

			Pubnub pubnub = new Pubnub(pnConfiguration);
			List<string> channelList2 = new List<string>();
			channelList2.Add(channel);
			bool tresult = false;

			
            SubscribeCallbackExt listener = new SubscribeCallbackExt(
                 delegate (Pubnub pnObj, PNMessageResult<object> pubMsg)
                 {
                 },
                 delegate (Pubnub pnObj, PNPresenceEventResult presenceEvnt) { },
                 delegate (Pubnub pnObj, PNSignalResult<object> signalMsg) 
                 {
                     if (signalMsg != null)
                     {
                         tresult = signalMsg.Channel.Equals(channel) && signalMsg.Message.ToString().Equals(payload);
                         Debug.Log("Signal tresult:" + tresult + channel + payload);
                     }
                 },
                 delegate (Pubnub pnObj, PNStatus pnStatus)
                 {

                 }
                 );
            pubnub.AddListener(listener);
            pubnub.Subscribe<string>().Channels(channelList2.ToArray()).Execute();
			yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);

			pubnub.Signal().Channel(channel).Message(payload).Execute(new PNPublishResultExt((result, status) => {
				Assert.True(!result.Timetoken.Equals(0));
				Assert.True(status.Error.Equals(false));
				Assert.True(status.StatusCode.Equals(0), status.StatusCode.ToString());
			}));
			yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);
			Assert.True(tresult, "test didn't return");
			pubnub.Destroy();

		}

        [UnityTest]
        public IEnumerator TestMembersAndMemberships()
        {
            //Create user 1
            PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
            //pnConfiguration.PubnubLog = new InternalLog();
            System.Random r = new System.Random();
            pnConfiguration.Uuid = "UnityTestConnectedUUID_" + r.Next(1000);
            int ran = r.Next(1000);
            string id = "id" + ran;
            string name = string.Format("name {0}", ran);
            string email = string.Format("email {0}", ran);
            string externalID = string.Format("externalID {0}", ran);
            string profileURL = string.Format("profileURL {0}", ran);

            //PNUserSpaceInclude[] include = new PNUserSpaceInclude[] { PNUserSpaceInclude.PNUserSpaceCustom };

            Pubnub pubnub = new Pubnub(pnConfiguration);
            bool tresult = false;

            Dictionary<string, object> userCustom = new Dictionary<string, object>();
            userCustom.Add("uck1", "ucv1");
            userCustom.Add("uck2", "ucv2");

            pubnub.CreateUser().Email(email).ExternalId(externalID).Name(name).Id(id).CustomObject(userCustom).ProfileUrl(profileURL).Execute(new PNCreateUserResultExt((result, status) =>
            {
                Debug.Log(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                Assert.True(status.Error.Equals(false));
                Assert.True(status.StatusCode.Equals(200));
                Assert.AreEqual(name, result.Name);
                Assert.AreEqual(email, result.Email);
                Assert.AreEqual(externalID, result.ExternalId);
                Assert.AreEqual(profileURL, result.ProfileUrl);
                Assert.AreEqual(id, result.Id);
                Assert.AreEqual(result.Updated, result.Created);
                //Assert.True(!string.IsNullOrEmpty(result.ETag), result.ETag);
                Assert.True("ucv1" == result.Custom["uck1"].ToString());
                Assert.True("ucv2" == result.Custom["uck2"].ToString());
                tresult = true;

            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls);
            Assert.True(tresult, "CreateUser didn't return");

            tresult = false;

            pubnub.GetUser().UserId(id).IncludeCustom(true).Execute(new PNGetUserResultExt((result, status) =>
            {
                Assert.True(status.Error.Equals(false));
                Assert.True(status.StatusCode.Equals(200));
                Assert.AreEqual(name, result.Name);
                Assert.AreEqual(email, result.Email);
                Assert.AreEqual(externalID, result.ExternalId);
                Assert.AreEqual(profileURL, result.ProfileUrl);
                Assert.AreEqual(id, result.Id);
                Assert.AreEqual(result.Updated, result.Created);
                //Assert.True(!string.IsNullOrEmpty(result.ETag), result.ETag);
                Assert.True("ucv1" == result.Custom["uck1"].ToString());
                Assert.True("ucv2" == result.Custom["uck2"].ToString());
                tresult = true;
            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls);
            Assert.True(tresult, "GetUser didn't return");
            //Create user 2
            //Create space 1
            //Create space 2
            //Add Space Memberships
            //Update Space Memberships
            //Get Space Memberships
            //Remove Space Memberships
            //Add user memberships
            //Update user memberships
            //Get members
            //Remove user memberships
            //delete user 1
            //delete space 1
            //delete user 1
            //delete space 1
        }

        [UnityTest]
        public IEnumerator TestUserCRUD()
        {
            PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
            System.Random r = new System.Random();
            pnConfiguration.Uuid = "UnityTestConnectedUUID_" + r.Next(1000);
            int ran = r.Next(1000);
            string id = "id" + ran;
            string name = string.Format("name {0}", ran);
            string email = string.Format("email {0}", ran);
            string externalID = string.Format("externalID {0}", ran);
            string profileURL = string.Format("profileURL {0}", ran);

            //PNUserSpaceInclude[] include = new PNUserSpaceInclude[] { PNUserSpaceInclude.PNUserSpaceCustom };

            Pubnub pubnub = new Pubnub(pnConfiguration);
            bool tresult = false;

            pubnub.CreateUser().Email(email).ExternalId(externalID).Name(name).Id(id).ProfileUrl(profileURL).Execute(new PNCreateUserResultExt((result, status) =>
            {
                Assert.True(status.Error.Equals(false));
                Assert.True(status.StatusCode.Equals(200));
                Assert.AreEqual(name, result.Name);
                Assert.AreEqual(email, result.Email);
                Assert.AreEqual(externalID, result.ExternalId);
                Assert.AreEqual(profileURL, result.ProfileUrl);
                Assert.AreEqual(id, result.Id);
                Assert.AreEqual(result.Updated, result.Created);
                //Assert.True(!string.IsNullOrEmpty(result.ETag), result.ETag);
                Assert.True(result.Custom == null);
                tresult = true;

            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls);
            Assert.True(tresult, "CreateUser didn't return");

            tresult = false;

            pubnub.GetUser().UserId(id).Execute(new PNGetUserResultExt((result, status) =>
            {
                Assert.True(status.Error.Equals(false));
                Assert.True(status.StatusCode.Equals(200));
                Assert.AreEqual(name, result.Name);
                Assert.AreEqual(email, result.Email);
                Assert.AreEqual(externalID, result.ExternalId);
                Assert.AreEqual(profileURL, result.ProfileUrl);
                Assert.AreEqual(id, result.Id);
                Assert.AreEqual(result.Updated, result.Created);
                //Assert.True(!string.IsNullOrEmpty(result.ETag), result.ETag);
                Assert.True(result.Custom == null);
                tresult = true;
            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls);
            Assert.True(tresult, "GetUser didn't return");

            tresult = false;

            int ran2 = r.Next(1000);
            string name2 = string.Format("name {0}", ran2);
            string email2 = string.Format("email {0}", ran2);
            string externalID2 = string.Format("externalID {0}", ran2);
            string profileURL2 = string.Format("profileURL {0}", ran2);
            tresult = false;

            pubnub.UpdateUser().Email(email2).ExternalId(externalID2).Name(name2).Id(id).ProfileUrl(profileURL2).Execute(new PNUpdateUserResultExt((result, status) =>
            {
                Assert.True(status.Error.Equals(false));
                Assert.True(status.StatusCode.Equals(200));
                Assert.AreEqual(name2, result.Name);
                Assert.AreEqual(email2, result.Email);
                Assert.AreEqual(externalID2, result.ExternalId);
                Assert.AreEqual(profileURL2, result.ProfileUrl);
                Assert.AreEqual(id, result.Id);
                Assert.AreNotEqual(result.Updated, result.Created);
                //Assert.True(!string.IsNullOrEmpty(result.ETag), result.ETag);
                Assert.True(result.Custom == null);
                tresult = true;

            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls);
            Assert.True(tresult, "UpdateUser didn't return");


            pubnub.GetUsers().Execute(new PNGetUsersResultExt((result, status) =>
            {
                Assert.True(status.Error.Equals(false));
                Assert.True(status.StatusCode.Equals(200));
                if (result.Users != null)
                {
                    foreach (PNUserResult pnUserResult in result.Users)
                    {
                        if (pnUserResult.Id.Equals(id))
                        {
                            Assert.AreEqual(name2, pnUserResult.Name);
                            Assert.AreEqual(email2, pnUserResult.Email);
                            Assert.AreEqual(externalID2, pnUserResult.ExternalId);
                            Assert.AreEqual(profileURL2, pnUserResult.ProfileUrl);
                            Assert.AreNotEqual(pnUserResult.Updated, pnUserResult.Created);
                            //Assert.True(!string.IsNullOrEmpty(pnUserResult.ETag), pnUserResult.ETag);
                            Assert.True(pnUserResult.Custom == null);
                            tresult = true;
                        }
                    }
                }
            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls);
            Assert.True(tresult, "GetUsers didn't return");

            tresult = false;

            pubnub.DeleteUser().Id(id).Execute(new PNDeleteUserResultExt((result, status) =>
            {
                Assert.True(status.Error.Equals(false));
                Assert.True(status.StatusCode.Equals(200));
                tresult = true;

            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls);
            Assert.True(tresult, "DeleteUser didn't return");

            pubnub.Destroy();
        }

        [UnityTest]
        public IEnumerator TestSpaceCRUD()
        {
            PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
            System.Random r = new System.Random();
            pnConfiguration.Uuid = "UnityTestConnectedUUID_" + r.Next(1000);
            int ran = r.Next(1000);
            string id = "id" + ran;
            string name = string.Format("name {0}", ran);
            string description = string.Format("description {0}", ran);

            //PNUserSpaceInclude[] include = new PNUserSpaceInclude[] { PNUserSpaceInclude.PNUserSpaceCustom };

            Pubnub pubnub = new Pubnub(pnConfiguration);
            bool tresult = false;

            pubnub.CreateSpace().Description(description).Name(name).Id(id).Execute(new PNCreateSpaceResultExt((result, status) =>
            {
                Assert.True(status.Error.Equals(false));
                Assert.True(status.StatusCode.Equals(200));
                Assert.AreEqual(name, result.Name);
                Assert.AreEqual(description, result.Description);
                Assert.AreEqual(id, result.Id);
                Assert.AreEqual(result.Updated, result.Created);
                //Assert.True(!string.IsNullOrEmpty(result.ETag), result.ETag);
                Assert.True(result.Custom == null);
                tresult = true;

            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls);
            Assert.True(tresult, "CreateSpace didn't return");
            tresult = false;

            pubnub.GetSpace().SpaceId(id).Execute(new PNGetSpaceResultExt((result, status) =>
            {
                Assert.True(status.Error.Equals(false));
                Assert.True(status.StatusCode.Equals(200));
                Assert.AreEqual(name, result.Name);
                Assert.AreEqual(description, result.Description);
                Assert.AreEqual(id, result.Id);
                Assert.AreEqual(result.Updated, result.Created);
                //Assert.True(!string.IsNullOrEmpty(result.ETag), result.ETag);
                Assert.True(result.Custom == null);
                tresult = true;
            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls);
            Assert.True(tresult, "GetSpace didn't return");

            tresult = false;

            int ran2 = r.Next(1000);
            string name2 = string.Format("name {0}", ran2);
            string description2 = string.Format("description {0}", ran2);
            tresult = false;

            pubnub.UpdateSpace().Description(description2).Name(name2).Id(id).Execute(new PNUpdateSpaceResultExt((result, status) =>
            {
                Assert.True(status.Error.Equals(false));
                Assert.True(status.StatusCode.Equals(200));
                Assert.AreEqual(name2, result.Name);
                Assert.AreEqual(description2, result.Description);
                Assert.AreEqual(id, result.Id);
                Assert.AreNotEqual(result.Updated, result.Created);
                //Assert.True(!string.IsNullOrEmpty(result.ETag), result.ETag);
                Assert.True(result.Custom == null);
                tresult = true;

            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls);
            Assert.True(tresult, "UpdateSpace didn't return");


            pubnub.GetSpaces().Execute(new PNGetSpacesResultExt((result, status) =>
            {
                Assert.True(status.Error.Equals(false));
                Assert.True(status.StatusCode.Equals(200));
                if (result.Spaces != null)
                {
                    foreach (PNSpaceResult pnSpaceResult in result.Spaces)
                    {
                        if (pnSpaceResult.Id.Equals(id))
                        {
                            Assert.AreEqual(name2, pnSpaceResult.Name);
                            Assert.AreEqual(description2, pnSpaceResult.Description);
                            Assert.AreNotEqual(pnSpaceResult.Updated, pnSpaceResult.Created);
                            //Assert.True(!string.IsNullOrEmpty(pnSpaceResult.ETag), pnSpaceResult.ETag);
                            Assert.True(pnSpaceResult.Custom == null);
                            tresult = true;
                        }
                    }
                }
            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls);
            Assert.True(tresult, "GetSpaces didn't return");

            tresult = false;

            pubnub.DeleteSpace().Id(id).Execute(new PNDeleteSpaceResultExt((result, status) =>
            {
                Assert.True(status.Error.Equals(false));
                Assert.True(status.StatusCode.Equals(200));
                tresult = true;

            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls);
            Assert.True(tresult, "DeleteSpace didn't return");

            pubnub.Destroy();
        }


        [UnityTest]
        public IEnumerator TestCG()
        {

            PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
            //pnConfiguration.PubnubLog = new InternalLog();
            System.Random r = new System.Random();
            pnConfiguration.Uuid = "UnityTestCGUUID_" + r.Next(100);
            string channel = "UnityTestWithCGChannel";
            string channel2 = "UnityTestWithCGChannel2";
            List<string> channelList = new List<string>();
            channelList.Add(channel);
            channelList.Add(channel2);

            string channelGroup = "cg";
            List<string> channelGroupList = new List<string>();
            channelGroupList.Add(channelGroup);

            Pubnub pubnub = new Pubnub(pnConfiguration);
            bool tresult = false;


            pubnub.AddChannelsToChannelGroup().Channels(channelList.ToArray()).ChannelGroup(channelGroup).Execute(new PNChannelGroupsAddChannelResultExt((result, status) =>
            {
                Debug.Log("in AddChannelsToChannelGroup " + status.Error);
                if (!status.Error && result != null)
                {
                    tresult = true;
                }
                else
                {
                    Assert.Fail("AddChannelsToChannelGroup failed");
                }
            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls3);
            Assert.True(tresult, "test didn't return1");
            tresult = false;

            pubnub.ListChannelsForChannelGroup().ChannelGroup(channelGroup).Execute(new PNChannelGroupsAllChannelsResultExt((result, status) =>
            {
                if (!status.Error)
                {
                    if (result.Channels != null)
                    {
                        bool matchChannel1 = result.Channels.Contains(channel);
                        bool matchChannel2 = result.Channels.Contains(channel2);
                        Assert.IsTrue(matchChannel1);
                        Assert.IsTrue(matchChannel2);
                        tresult = matchChannel1 && matchChannel2;
                    }
                    else
                    {
                        Assert.Fail("result.Channels empty");
                    }
                }
                else
                {
                    Assert.Fail("AddChannelsToChannelGroup failed");
                }

            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls3);
            Assert.True(tresult, "test didn't return2");
            tresult = false;
            string payload = string.Format("payload {0}", pnConfiguration.Uuid);

            SubscribeCallbackExt listener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNMessageResult<object> pubMsg) 
                {
                    if (pubMsg.Message != null)
                    {
                        Debug.Log("SubscribeCallback" + pubMsg.Subscription);
                        Debug.Log("SubscribeCallback" + pubMsg.Channel);
                        Debug.Log("SubscribeCallback" + pubMsg.Message);
                        Debug.Log("SubscribeCallback" + pubMsg.Timetoken);
                        bool matchChannel = pubMsg.Channel.Equals(channel);
                        Assert.True(matchChannel);
                        bool matchSubscription = pubMsg.Subscription.Equals(channelGroup);
                        Assert.True(matchSubscription);
                        bool matchPayload = pubMsg.Message.ToString().Equals(payload);
                        Assert.True(matchPayload);
                        tresult = matchPayload && matchSubscription && matchChannel;
                    }
                },
                delegate (Pubnub pnObj, PNPresenceEventResult presenceEvnt) { /* Debug.Log("presenceEvnt = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(presenceEvnt)); */ },
                delegate (Pubnub pnObj, PNStatus pnStatus) { /*Debug.Log("pnStatus = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(pnStatus));*/ }
                );

            pubnub.AddListener(listener);

            pubnub.Subscribe<string>().ChannelGroups(channelGroupList.ToArray()).Execute();
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls2);

            //tresult = false;
            pubnub.Publish().Channel(channel).Message(payload).Execute(new PNPublishResultExt((result, status) =>
            {
                Assert.True(!result.Timetoken.Equals(0));
                Assert.True(status.Error.Equals(false));
                Assert.True(status.StatusCode.Equals(200));
            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls3);
            Assert.True(tresult, "test didn't return 3");
            tresult = false;

            Dictionary<string, object> state = new Dictionary<string, object>();
            state.Add("k1", "v1");
            pubnub.SetPresenceState().ChannelGroups(channelGroupList.ToArray()).State(state).Execute(new PNSetStateResultExt((result, status) =>
            {
                if (status.Error)
                {
                    Assert.Fail("SetPresenceState failed");
                }
                else
                {
                    if (result != null)
                    {
                        if (result.State != null)
                        {
                            Debug.Log("pubnub.JsonLibrary.SerializeToJsonString(result)" + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                            foreach (KeyValuePair<string, object> key in result.State)
                            {
                                Debug.Log("key.Key " + key.Key);
                                if (key.Key.Equals("k1"))
                                {
                                    Debug.Log("pubnub.JsonLibrary.SerializeToJsonString(key.Value) " + key.Value.ToString());
                                    bool stateMatch = key.Value.ToString().Equals("v1");
                                    Debug.Log("stateMatch = " + stateMatch.ToString());
                                    Assert.IsTrue(stateMatch);
                                    tresult = stateMatch;
                                    break;
                                }
                            }
                        }
                    }
                }
            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls3);
            Assert.True(tresult, "test didn't return 4");

            pubnub.Unsubscribe<string>().ChannelGroups(channelGroupList.ToArray()).Execute();
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls3);

            pubnub.Destroy();
        }

        [UnityTest]
        public IEnumerator TestCGRemove()
        {
            PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
            System.Random r = new System.Random();
            pnConfiguration.Uuid = "UnityTestCGUUID_" + r.Next(100);
            string channel = "UnityTestWithCGChannel";
            string channel2 = "UnityTestWithCGChannel2";
            List<string> channelList = new List<string>();
            channelList.Add(channel);
            channelList.Add(channel2);

            string channelGroup = "cg";
            List<string> channelGroupList = new List<string>();
            channelGroupList.Add(channelGroup);

            Pubnub pubnub = new Pubnub(pnConfiguration);
            bool tresult = false;


            pubnub.AddChannelsToChannelGroup().Channels(channelList.ToArray()).ChannelGroup(channelGroup).Execute(new PNChannelGroupsAddChannelResultExt((result, status) =>
            {
                Debug.Log("in AddChannelsToChannelGroup " + status.Error);
                if (!status.Error && result != null)
                {
                    tresult = true;
                }
                else
                {
                    Assert.Fail("AddChannelsToChannelGroup failed");
                }
            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls3);
            Assert.True(tresult, "test didn't return1");
            tresult = false;

            pubnub.ListChannelsForChannelGroup().ChannelGroup(channelGroup).Execute(new PNChannelGroupsAllChannelsResultExt((result, status) =>
            {
                if (!status.Error)
                {
                    if (result.Channels != null)
                    {
                        bool matchChannel1 = result.Channels.Contains(channel);
                        bool matchChannel2 = result.Channels.Contains(channel2);
                        Assert.IsTrue(matchChannel1);
                        Assert.IsTrue(matchChannel2);
                        tresult = matchChannel1 && matchChannel2;
                    }
                    else
                    {
                        Assert.Fail("result.Channels empty");
                    }
                }
                else
                {
                    Assert.Fail("AddChannelsToChannelGroup failed");
                }
            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls3);
            Assert.True(tresult, "test didn't return2");
            tresult = false;

            List<string> listChannelsRemove = new List<string> { channel };
            listChannelsRemove.Add(channel);
            pubnub.RemoveChannelsFromChannelGroup().Channels(listChannelsRemove.ToArray()).ChannelGroup(channelGroup).Execute(new PNChannelGroupsRemoveChannelResultExt((result, status) =>
            {
                Debug.Log("in RemoveChannelsFromCG");
                if (!status.Error && result != null)
                {

                    tresult = true;
                }

            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls3);
            Assert.True(tresult, "test didn't return 8");

            tresult = false;
            pubnub.ListChannelsForChannelGroup().ChannelGroup(channelGroup).Execute(new PNChannelGroupsAllChannelsResultExt((result, status) =>
            {
                if (!status.Error)
                {
                    if (result.Channels != null)
                    {
                        bool matchChannel1 = result.Channels.Contains(channel);
                        bool matchChannel2 = result.Channels.Contains(channel2);
                        Assert.IsTrue(!matchChannel1);
                        Assert.IsTrue(matchChannel2);
                        tresult = !matchChannel1 && matchChannel2;

                    }
                    else
                    {
                        Assert.Fail("result.Channels empty");
                    }
                }
                else
                {
                    Assert.Fail("AddChannelsToChannelGroup failed");
                }
                tresult = true;

            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls3);
            Assert.True(tresult, "test didn't return 9");

            tresult = false;
            pubnub.DeleteChannelGroup().ChannelGroup(channelGroup).Execute(new PNChannelGroupsDeleteGroupResultExt((result, status) =>
            {
                if (!status.Error)
                {
                    tresult = result.Message.Equals("OK");
                }
            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls3);
            Assert.True(tresult, "test didn't return 10");

            pubnub.Destroy();

        }

        //public IEnumerator TestPush() {
        //	PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
        //	System.Random r = new System.Random ();
        //	pnConfiguration.Uuid = "UnityTestCGUUID_" + r.Next (100);
        //	string channel = "UnityTestWithCGChannel";
        //	string channel2 = "UnityTestWithCGChannel2";
        //	List<string> channelList = new List<string>();
        //	channelList.Add(channel);
        //	channelList.Add(channel2);

        //	string channelGroup = "cg";
        //	List<string> channelGroupList = new List<string>();
        //	channelGroupList.Add(channelGroup);

        //	Pubnub pubnub = new Pubnub(pnConfiguration);
        //	bool tresult = false;

        //	string deviceId = "UnityTestDeviceId";
        //	PNPushType pnPushType = PNPushType.GCM;

        //	pubnub.AddPushNotificationsOnChannels().Channels(channelList.ToArray()).DeviceID(deviceId).PushType(pnPushType).Execute((result, status) => {
        //                  Debug.Log ("in AddChannelsToChannelGroup " + status.Error);
        //                  if(!status.Error){
        //				Debug.Log(result.Message);
        //				tresult = result.Message.Contains("Modified Ch");
        //			} else {
        //				Assert.Fail("AddPushNotificationsOnChannels failed");
        //			}
        //              });

        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls3);
        //	Assert.True(tresult, "test didn't return1");
        //	tresult = false;

        //	pubnub.AuditPushChannelProvisions().DeviceID(deviceId).PushType(pnPushType).Execute((result, status) => {
        //                  if(!status.Error){
        //				if(result.Channels!=null){
        //					bool matchChannel1 = result.Channels.Contains(channel);
        //					bool matchChannel2 = result.Channels.Contains(channel2);
        //					Assert.IsTrue(matchChannel1);
        //					Assert.IsTrue(matchChannel2);
        //					tresult = matchChannel1 && matchChannel2;

        //				} else {
        //					Assert.Fail("result.Channels empty");
        //				}
        //			} else {
        //				Assert.Fail("AddChannelsToChannelGroup failed");
        //			}
        //              });

        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls3);
        //	Assert.True(tresult, "test didn't return2");
        //	tresult = false;

        //	List<string> listChannelsRemove = new List<string>{channel};
        //	listChannelsRemove.Add(channel);
        //	pubnub.RemovePushNotificationsFromChannels().Channels(listChannelsRemove).DeviceID(deviceId).PushType(pnPushType).Execute((result, status) => {
        //                  Debug.Log ("in RemovePushNotificationsFromChannels");
        //			if(!status.Error){
        //                      tresult = result.Message.Equals("Modified Channels");
        //                  }
        //              });

        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls3);
        //	Assert.True(tresult, "test didn't return 8");

        //	tresult = false;
        //	pubnub.AuditPushChannelProvisions().DeviceID(deviceId).PushType(pnPushType).Execute((result, status) => {
        //                  if(!status.Error){
        //				if(result.Channels!=null){
        //					bool matchChannel1 = result.Channels.Contains(channel);
        //					bool matchChannel2 = result.Channels.Contains(channel2);
        //					Assert.IsTrue(!matchChannel1);
        //					Assert.IsTrue(matchChannel2);
        //					tresult = !matchChannel1 && matchChannel2;

        //				} else {
        //					Assert.Fail("result.Channels empty");
        //				}
        //			} else {
        //				Assert.Fail("AddChannelsToChannelGroup failed");
        //			}
        //              });
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls3);

        //	Assert.True(tresult, "test didn't return 9");

        //	tresult = false;
        //	pubnub.RemoveAllPushNotifications().DeviceID(deviceId).PushType(pnPushType).Execute((result, status) => {
        //                  if(!status.Error){
        //                      tresult = result.Message.Equals("Removed Device");
        //                  }
        //              });

        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls3);
        //	Assert.True(tresult, "test didn't return 10");

        //	pubnub.Destroy();

        //}

        //[UnityTest]
        //public IEnumerator TestPublishWithMeta() {
        //	PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
        //	System.Random r = new System.Random ();
        //	pnConfiguration.Uuid = "UnityTestConnectedUUID_" + r.Next (100);
        //	string channel = "UnityTestWithMetaChannel";
        //	string payload = string.Format("payload {0}", pnConfiguration.Uuid);

        //	pnConfiguration.FilterExpression = "region=='east'";
        //	Pubnub pubnub = new Pubnub(pnConfiguration);

        //	List<string> channelList2 = new List<string>();
        //	channelList2.Add(channel);
        //	bool tresult = false;

        //	pubnub.SubscribeCallback += (sender, e) => { 
        //		SubscribeEventEventArgs mea = e as SubscribeEventEventArgs;
        //		if(!mea.Status.Category.Equals(PNStatusCategory.PNConnectedCategory)){
        //			Debug.Log("SubscribeCallback" + mea.MessageResult.Subscription);
        //			Debug.Log("SubscribeCallback" + mea.MessageResult.Channel);
        //			Debug.Log("SubscribeCallback" + mea.MessageResult.Payload);
        //			Debug.Log("SubscribeCallback" + mea.MessageResult.Timetoken);
        //			bool matchChannel = mea.MessageResult.Channel.Equals(channel);
        //			Assert.True(matchChannel);
        //			bool matchPayload = mea.MessageResult.Payload.ToString().Equals(payload);
        //			Assert.True(matchPayload);
        //			tresult = matchPayload  && matchChannel;

        //		} 
        //	};
        //	pubnub.Subscribe<string>().Channels(channelList2.ToArray()).Execute();
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);

        //	Dictionary<string, string> metaDict = new Dictionary<string, string>();
        //          metaDict.Add("region", "east");

        //	pubnub.Publish().Channel(channel).Meta(metaDict).Message(payload).Execute((result, status) => {
        //		Assert.True(!result.Timetoken.Equals(0));
        //		Assert.True(status.Error.Equals(false));
        //		Assert.True(status.StatusCode.Equals(0), status.StatusCode.ToString());
        //		Assert.True(!result.Timetoken.Equals(0));
        //	});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls3);
        //	Assert.True(tresult, "test didn't return");
        //	pubnub.Destroy();
        //}

        [UnityTest]
        public IEnumerator TestPublishWithMetaNeg()
        {
            PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
            System.Random r = new System.Random();
            pnConfiguration.Uuid = "UnityTestConnectedUUID_" + r.Next(100);
            string channel = "UnityTestWithMetaNegChannel";
            string payload = string.Format("payload {0}", pnConfiguration.Uuid);

            pnConfiguration.FilterExpression = "region=='east'";
            Pubnub pubnub = new Pubnub(pnConfiguration);

            List<string> channelList2 = new List<string>();
            channelList2.Add(channel);
            bool tresult = false;

            SubscribeCallbackExt listener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNMessageResult<object> pubMsg)
                {
                    if (pubMsg.Message != null)
                    {
                        Debug.Log("SubscribeCallback" + pubMsg.Subscription);
                        Debug.Log("SubscribeCallback" + pubMsg.Channel);
                        Debug.Log("SubscribeCallback" + pubMsg.Message);
                        Debug.Log("SubscribeCallback" + pubMsg.Timetoken);
                        bool matchChannel = pubMsg.Channel.Equals(channel);
                        Assert.True(matchChannel);
                        bool matchPayload = pubMsg.Message.ToString().Equals(payload);
                        Assert.True(matchPayload);
                        tresult = matchPayload && matchChannel;
                    }
                },
                delegate (Pubnub pnObj, PNPresenceEventResult presenceEvnt) { /* Debug.Log("presenceEvnt = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(presenceEvnt)); */ },
                delegate (Pubnub pnObj, PNStatus pnStatus) { /*Debug.Log("pnStatus = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(pnStatus));*/ }
                );

            pubnub.AddListener(listener);

            pubnub.Subscribe<string>().Channels(channelList2.ToArray()).Execute();
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls2);

            Dictionary<string, object> metaDict = new Dictionary<string, object>();
            metaDict.Add("region", "east1");

            pubnub.Publish().Channel(channel).Meta(metaDict).Message(payload).Execute(new PNPublishResultExt((result, status) =>
            {
                Assert.True(!result.Timetoken.Equals(0));
                Assert.True(status.Error.Equals(false));
                Assert.True(status.StatusCode.Equals(200));
            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls3);
            Assert.True(!tresult, "subscribe returned");
            pubnub.Destroy();
        }

        [UnityTest]
        public IEnumerator TestPublishAndHistory()
        {
            PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
            System.Random r = new System.Random();
            pnConfiguration.Uuid = "UnityTestConnectedUUID_" + r.Next(100);
            string channel = "UnityPublishAndHistoryChannel";
            string payload = string.Format("payload no store {0}", pnConfiguration.Uuid);

            Pubnub pubnub = new Pubnub(pnConfiguration);

            List<string> channelList2 = new List<string>();
            channelList2.Add(channel);
            bool tresult = false;

            pubnub.Publish().Channel(channel).Message(payload).Execute(new PNPublishResultExt((result, status) =>
            {
                bool timetokenMatch = !result.Timetoken.Equals(0);
                bool statusError = status.Error.Equals(false);
                bool statusCodeMatch = status.StatusCode.Equals(200);
                Assert.True(timetokenMatch);
                Assert.True(statusError);
                Assert.True(statusCodeMatch, status.StatusCode.ToString());
                tresult = statusCodeMatch && statusError && timetokenMatch;
            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls3);
            Assert.True(tresult, "test didnt return 1");

            tresult = false;
            pubnub.History().Channel(channel).Count(1).Execute(new PNHistoryResultExt((result, status) =>
            {
               Assert.True(status.Error.Equals(false));
                if (!status.Error)
                {

                    if ((result.Messages != null) && (result.Messages.Count > 0))
                    {
                        PNHistoryItemResult pnHistoryItemResult = result.Messages[0] as PNHistoryItemResult;
                        Debug.Log("result.Messages[0]" + result.Messages[0].ToString());
                        if (pnHistoryItemResult != null)
                        {
                            tresult = pnHistoryItemResult.Entry.ToString().Contains(payload);
                        }
                        else
                        {
                            tresult = false;
                        }
                    }
                    else
                    {
                        tresult = false;
                    }

                }
            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls3);
            Assert.True(tresult, "test didnt return 2");

            pubnub.Destroy();
        }

        [UnityTest]
        public IEnumerator TestPublishHistoryAndFetchWithMetaAndTT()
        {
            return PublishHistoryAndFetchWithMetaCommon(true, true);
        }

        [UnityTest]
        public IEnumerator TestPublishHistoryAndFetchWithMetaWithoutTT()
        {
            return PublishHistoryAndFetchWithMetaCommon(true, false);
        }

        [UnityTest]
        public IEnumerator TestPublishHistoryAndFetchWithTTWithoutMeta()
        {
            return PublishHistoryAndFetchWithMetaCommon(false, true);
        }

        [UnityTest]
        public IEnumerator TestPublishHistoryAndFetchWithoutMetaAndTT()
        {
            return PublishHistoryAndFetchWithMetaCommon(false, false);
        }

        public IEnumerator PublishHistoryAndFetchWithMetaCommon(bool withMeta, bool withTimetoken)
        {
            PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
            System.Random r = new System.Random();
            pnConfiguration.Uuid = "UnityTestConnectedUUID_" + r.Next(100);
            string channel = "UnityPublishAndHistoryChannel" + r.Next(100); ;
            string payload = string.Format("payload {0}", pnConfiguration.Uuid);

            Pubnub pubnub = new Pubnub(pnConfiguration);

            List<string> channelList2 = new List<string>();
            channelList2.Add(channel);
            bool tresult = false;
            Dictionary<string, object> metaDict = new Dictionary<string, object>();
            metaDict.Add("region", "east");
            long retTT = 0;

            pubnub.Publish().Channel(channel).Meta(metaDict).Message(payload).Execute(new PNPublishResultExt((result, status) =>
            {
                bool timetokenMatch = !result.Timetoken.Equals(0);
                bool statusError = status.Error.Equals(false);
                bool statusCodeMatch = status.StatusCode.Equals(200);
                retTT = result.Timetoken;
                Assert.True(timetokenMatch);
                Assert.True(statusError);
                Assert.True(statusCodeMatch, status.StatusCode.ToString());
                tresult = statusCodeMatch && statusError && timetokenMatch;
            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls3);
            Assert.True(tresult, "test didnt return 1");

            tresult = false;
            bool tresultMeta = false;
            bool tresultTimetoken = false;
            pubnub.History().Channel(channel).IncludeMeta(withMeta).IncludeTimetoken(withTimetoken).Count(1).Execute(new PNHistoryResultExt((result, status) =>
            {
                Assert.True(status.Error.Equals(false));
                if (!status.Error)
                {

                    if ((result.Messages != null) && (result.Messages.Count > 0))
                    {
                        PNHistoryItemResult pnHistoryItemResult = result.Messages[0] as PNHistoryItemResult;
                        Debug.Log("result.Messages[0]" + result.Messages[0].ToString());
                        if (pnHistoryItemResult != null)
                        {
                            tresult = pnHistoryItemResult.Entry.ToString().Contains(payload);

                            if (withMeta)
                            {
                                Dictionary<string, object> metaDataDict = pnHistoryItemResult.Meta as Dictionary<string, object>;
                                object region;
                                metaDataDict.TryGetValue("region", out region);
                                tresultMeta = region.ToString().Equals("east");
                            }
                            else
                            {
                                tresultMeta = true;
                            }
                            if (withTimetoken)
                            {
                                tresultTimetoken = retTT.Equals(pnHistoryItemResult.Timetoken);
                            }
                            else
                            {
                                tresultTimetoken = true;
                            }
                        }
                        else
                        {
                            tresult = false;
                            tresultMeta = false;
                        }
                    }
                    else
                    {
                        tresult = false;
                    }

                }
            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls3);
            Assert.True(tresult, "test didnt return 2");
            Assert.True(tresultMeta, "test meta didnt return");
            Assert.True(tresultTimetoken, "tresultTimetoken didnt return");

            tresult = false;
            tresultMeta = false;
            pubnub.FetchHistory().Channels(channelList2.ToArray()).IncludeMeta(withMeta).Execute(new PNFetchHistoryResultExt((result, status) =>
            {
                if (!status.Error)
                {
                    if (result.Messages != null)
                    {
                        Dictionary<string, List<PNHistoryItemResult>> fetchResult = result.Messages as Dictionary<string, List<PNHistoryItemResult>>;
                        Debug.Log("fetchResult.Count:" + fetchResult.Count);
                        foreach (KeyValuePair<string, List<PNHistoryItemResult>> kvp in fetchResult)
                        {
                            Debug.Log("Channel:" + kvp.Key);
                            if (kvp.Key.Equals(channel))
                            {

                                foreach (PNHistoryItemResult msg in kvp.Value)
                                {
                                    //Debug.Log("msg.Channel:" + msg.Channel);
                                    Debug.Log("msg.Entry.ToString():" + msg.Entry.ToString());
                                    if (msg.Entry.ToString().Equals(payload))
                                    {
                                        tresult = true;
                                    }
                                    if (withMeta)
                                    {
                                        Dictionary<string, object> metaDataDict = msg.Meta as Dictionary<string, object>;
                                        object region;
                                        if (metaDataDict != null)
                                        {
                                            metaDataDict.TryGetValue("region", out region);
                                            tresultMeta = region.ToString().Equals("east");
                                        }
                                        else
                                        {
                                            Debug.Log("metaDataDict null" + msg.Meta);
                                        }
                                    }
                                    else
                                    {
                                        tresultMeta = true;
                                    }

                                }
                                if (!tresult && !tresultMeta)
                                {
                                    break;
                                }
                            }
                        }
                    }

                }

            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls3);
            Assert.True(tresult, "test didnt return for fetch");
            Assert.True(tresultMeta, "test meta didnt return for fetch");


            pubnub.Destroy();
        }

        [UnityTest]
        public IEnumerator TestPublishNoStore()
        {
            PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
            System.Random r = new System.Random();
            pnConfiguration.Uuid = "UnityTestConnectedUUID_" + r.Next(100);
            string channel = "UnityTestNoStoreChannel";
            string payload = string.Format("payload no store {0}", pnConfiguration.Uuid);

            Pubnub pubnub = new Pubnub(pnConfiguration);

            List<string> channelList2 = new List<string>();
            channelList2.Add(channel);
            bool tresult = false;

            pubnub.Publish().Channel(channel).Message(payload).ShouldStore(false).Execute(new PNPublishResultExt((result, status) =>
            {
                bool timetokenMatch = !result.Timetoken.Equals(0);
                bool statusError = status.Error.Equals(false);
                bool statusCodeMatch = status.StatusCode.Equals(200);
                Assert.True(timetokenMatch);
                Assert.True(statusError);
                Assert.True(statusCodeMatch, status.StatusCode.ToString());
                tresult = statusCodeMatch && statusError && timetokenMatch;
            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls3);
            Assert.True(tresult, "test didnt return 1");

            tresult = false;
            pubnub.History().Channel(channel).Count(1).Execute(new PNHistoryResultExt((result, status) =>
            {
                Assert.True(status.Error.Equals(false));
                if (!status.Error)
                {

                    if ((result.Messages != null) && (result.Messages.Count > 0))
                    {
                        PNHistoryItemResult pnHistoryItemResult = result.Messages[0] as PNHistoryItemResult;
                        Debug.Log("result.Messages[0]" + result.Messages[0].ToString());
                        if (pnHistoryItemResult != null)
                        {
                            tresult = !pnHistoryItemResult.Entry.ToString().Contains(payload);
                        }
                        else
                        {
                            tresult = false;
                        }
                    }
                    else
                    {
                        tresult = true;
                    }

                }
            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls3);
            Assert.True(tresult, "test didnt return 2");

            pubnub.Destroy();
        }

        [UnityTest]
        public IEnumerator TestPublishKeyPresent()
        {
            PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
            System.Random r = new System.Random();
            pnConfiguration.Uuid = "UnityTestPublishKeyPresentUUID_" + r.Next(100);
            string channel = "UnityPublishKeyPresentChannel";
            string payload = string.Format("payload {0}", pnConfiguration.Uuid);

            pnConfiguration.PublishKey = "";
            Pubnub pubnub = new Pubnub(pnConfiguration);

            bool tresult = false;

            try
            {
                pubnub.Publish().Channel(channel).Message(payload).Execute(new PNPublishResultExt((result, status) =>
                {
                }));
            }
            catch (MissingMemberException mme)
            {
                Debug.Log("Publish " + mme.ToString());
                tresult = true;
            }

            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls3);
            Assert.True(tresult, "test didn't return 10");

            pubnub.Destroy();
        }

        [UnityTest]
        public IEnumerator TestNullAsEmptyOnpublish()
        {
            PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
            System.Random r = new System.Random();
            pnConfiguration.Uuid = "UnityTestPublishKeyPresentUUID_" + r.Next(100);
            string channel = "UnityPublishKeyPresentChannel";

            Pubnub pubnub = new Pubnub(pnConfiguration);

            bool tresult = false;

            try
            {
                pubnub.Publish().Channel(channel).Message(null).Execute(new PNPublishResultExt((result, status) =>
                {
                }));
            }
            catch (ArgumentException ae)
            {
                Debug.Log("Publish " + ae.ToString());
                tresult = true;
            }

            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls3);
            Assert.True(tresult, "test didn't return 10");

            pubnub.Destroy();
        }

        [UnityTest]
        public IEnumerator TestFire()
        {
            PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
            System.Random r = new System.Random();
            pnConfiguration.Uuid = "UnityTestConnectedUUID_" + r.Next(100);
            string channel = "UnityTestFireChannel";
            string payload = string.Format("payload no store {0}", pnConfiguration.Uuid);

            Pubnub pubnub = new Pubnub(pnConfiguration);

            List<string> channelList2 = new List<string>();
            channelList2.Add(channel);
            bool tresult = false;

            pubnub.Fire().Channel(channel).Message(payload).Execute(new PNPublishResultExt((result, status) =>
            {
                bool timetokenMatch = !result.Timetoken.Equals(0);
                bool statusError = status.Error.Equals(false);
                bool statusCodeMatch = status.StatusCode.Equals(200);
                Assert.True(timetokenMatch);
                Assert.True(statusError);
                Assert.True(statusCodeMatch, status.StatusCode.ToString());
                tresult = statusCodeMatch && statusError && timetokenMatch;
            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls3);
            Assert.True(tresult, "test didnt return 1");

            tresult = false;
            pubnub.History().Channel(channel).Count(1).Execute(new PNHistoryResultExt((result, status) =>
            {
                Assert.True(status.Error.Equals(false));
                if (!status.Error)
                {

                    if ((result.Messages != null) && (result.Messages.Count > 0))
                    {
                        PNHistoryItemResult pnHistoryItemResult = result.Messages[0] as PNHistoryItemResult;
                        Debug.Log("result.Messages[0]" + result.Messages[0].ToString());
                        if (pnHistoryItemResult != null)
                        {
                            tresult = !pnHistoryItemResult.Entry.ToString().Contains(payload);
                        }
                        else
                        {
                            tresult = false;
                        }
                    }
                    else
                    {
                        tresult = true;
                    }

                }
            }));
            yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls3);
            Assert.True(tresult, "test didnt return 2");

            pubnub.Destroy();
        }

        //public IEnumerator TestWildcardSubscribe() {
        //	PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
        //	System.Random r = new System.Random ();
        //	pnConfiguration.Uuid = "UnityWildSubscribeUUID_" + r.Next (100);
        //	string chToPub = "UnityWildSubscribeChannel." + r.Next (100);
        //	string channel = "UnityWildSubscribeChannel.*";
        //	string payload = string.Format("payload {0}", pnConfiguration.Uuid);
        //	Pubnub pubnub = new Pubnub(pnConfiguration);

        //	List<string> channelList2 = new List<string>();
        //	channelList2.Add(channel);
        //	string whatToTest = "join1";
        //	bool tJoinResult = false;
        //	bool tLeaveResult = false;
        //	bool tresult = false;

        //	PNConfiguration pnConfiguration2 = PlayModeCommon.SetPNConfig(false);
        //	pnConfiguration2.UUID = "UnityWildSubscribeUUID2_" + r.Next (100);

        //	pubnub.SubscribeCallback += (sender, e) => { 
        //		SubscribeEventEventArgs mea = e as SubscribeEventEventArgs;
        //		if(!mea.Status.Category.Equals(PNStatusCategory.PNConnectedCategory)){
        //			switch (whatToTest){
        //				case "join1":
        //				case "join2":
        //					Debug.Log("join1 or join2");
        //					if(mea.PresenceEventResult.Event.Equals("join")){
        //						bool containsUUID = false;
        //						if(whatToTest.Equals("join1")){
        //							containsUUID = mea.PresenceEventResult.UUID.Contains(pnConfiguration.Uuid);
        //						} else {
        //							containsUUID = mea.PresenceEventResult.UUID.Contains(pnConfiguration2.UUID);
        //						}

        //						Assert.True(containsUUID);
        //						Debug.Log("containsUUID:" + containsUUID);
        //						bool containsOccupancy = mea.PresenceEventResult.Occupancy > 0;
        //						Assert.True(containsOccupancy);
        //						Debug.Log("containsOccupancy:" + containsOccupancy);

        //						bool containsTimestamp = mea.PresenceEventResult.Timestamp > 0;
        //						Assert.True(containsTimestamp);
        //						Debug.Log("containsTimestamp:" + containsTimestamp);

        //						bool containsSubscription = mea.PresenceEventResult.Subscription.Equals(channel);
        //						Assert.True(containsSubscription);
        //						Debug.Log("containsSubscription:" + containsSubscription);

        //						tJoinResult = containsTimestamp && containsOccupancy && containsUUID && containsSubscription;
        //					}	
        //				break;
        //				case "leave":
        //					if(mea.PresenceEventResult.Event.Equals("leave")){
        //						bool containsUUID = mea.PresenceEventResult.UUID.Contains(pnConfiguration2.UUID);
        //						Assert.True(containsUUID);
        //						Debug.Log(containsUUID);
        //						bool containsTimestamp = mea.PresenceEventResult.Timestamp > 0;
        //						Assert.True(containsTimestamp);
        //						bool containsSubscription = mea.PresenceEventResult.Subscription.Equals(channel);
        //						Assert.True(containsSubscription);
        //						bool containsOccupancy = mea.PresenceEventResult.Occupancy > 0;
        //						Assert.True(containsOccupancy);
        //						Debug.Log("containsSubscription:" + containsSubscription);
        //						Debug.Log("containsTimestamp:" + containsTimestamp);
        //						Debug.Log("containsOccupancy:" + containsOccupancy);
        //						Debug.Log("containsUUID:" + containsUUID);

        //						tLeaveResult = containsTimestamp && containsOccupancy && containsUUID && containsSubscription;
        //					}
        //				break;
        //				default:
        //					Debug.Log("SubscribeCallback" + mea.MessageResult.Subscription);
        //					Debug.Log("SubscribeCallback" + mea.MessageResult.Channel);
        //					Debug.Log("SubscribeCallback" + mea.MessageResult.Payload);
        //					Debug.Log("SubscribeCallback" + mea.MessageResult.Timetoken);
        //					bool matchChannel = mea.MessageResult.Channel.Equals(chToPub);
        //					Assert.True(matchChannel);
        //					bool matchPayload = mea.MessageResult.Payload.ToString().Equals(payload);
        //					Assert.True(matchPayload);

        //					bool matchSubscription = mea.MessageResult.Subscription.Equals(channel);
        //					Assert.True(matchSubscription);
        //					tresult = matchPayload  && matchChannel && matchSubscription;
        //				break;
        //			}
        //		} 
        //	};
        //	pubnub.Subscribe<string>().Channels(channelList2.ToArray()).Execute();
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);
        //	Assert.True(tJoinResult, "subscribe didn't get a join");

        //	whatToTest = "";

        //	pubnub.Publish().Channel(chToPub).Message(payload).Execute((result, status) => {
        //		bool timetokenMatch = !result.Timetoken.Equals(0);
        //		bool statusError = status.Error.Equals(false);
        //		bool statusCodeMatch = status.StatusCode.Equals(0);
        //		Assert.True(timetokenMatch);
        //		Assert.True(statusError);
        //		Assert.True(statusCodeMatch, status.StatusCode.ToString());
        //		tresult = statusCodeMatch && statusError && timetokenMatch;
        //	});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls3);

        //	Assert.True(tresult, "Subcribe didn't get a message");

        //	PubNub pubnub2 = new PubNub(pnConfiguration2);

        //	whatToTest = "join2";

        //	pubnub2.Subscribe ().Channels(channelList2.ToArray()).Execute();
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);
        //	Assert.True(tJoinResult, "subscribe2 didn't get a join");

        //	whatToTest = "leave";

        //	tresult = false;
        //	pubnub2.Unsubscribe().Channels(channelList2.ToArray()).Execute((result, status) => {
        //			Debug.Log("status.Error:" + status.Error);
        //			tresult = !status.Error;
        //		});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);
        //	Assert.True(tresult, "unsubscribe didn't return");
        //	Assert.True(tLeaveResult, "subscribe didn't get a leave");

        //	pubnub.Destroy();
        //	pubnub2.CleanUp();
        //}

        //[UnityTest]
        //public IEnumerator TestUnsubscribeAllAndUnsubscribe() {
        //	PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
        //	System.Random r = new System.Random ();
        //	pnConfiguration.Uuid = "UnityWildSubscribeUUID_" + r.Next (100);
        //	string channel = "UnityWildSubscribeChannel." + r.Next (100);
        //	string channel2 = "UnityWildSubscribeChannel." + r.Next (100);

        //	string payload = string.Format("payload {0}", pnConfiguration.Uuid);
        //	Pubnub pubnub = new Pubnub(pnConfiguration);

        //	List<string> channelList2 = new List<string>();
        //	channelList2.Add(channel);
        //	channelList2.Add(channel2);
        //	string whatToTest = "join1";
        //	bool tJoinResult = false;
        //	bool tLeaveResult = false;
        //	bool tresult = false;

        //	PNConfiguration pnConfiguration2 = PlayModeCommon.SetPNConfig(false);
        //	pnConfiguration2.UUID = "UnityWildSubscribeUUID2_" + r.Next (100);

        //	pubnub.SubscribeCallback += (sender, e) => { 
        //		SubscribeEventEventArgs mea = e as SubscribeEventEventArgs;
        //		if(!mea.Status.Category.Equals(PNStatusCategory.PNConnectedCategory)){
        //			switch (whatToTest){
        //				case "join1":
        //				case "join2":
        //					if(mea.PresenceEventResult.Event.Equals("join")){
        //						bool containsUUID = false;
        //						if(whatToTest.Equals("join1")){
        //							containsUUID = mea.PresenceEventResult.UUID.Contains(pnConfiguration.Uuid);
        //						} else {
        //							containsUUID = mea.PresenceEventResult.UUID.Contains(pnConfiguration2.UUID);
        //						}
        //						bool containsOccupancy = mea.PresenceEventResult.Occupancy > 0;
        //						Assert.True(containsOccupancy);
        //						bool containsTimestamp = mea.PresenceEventResult.Timestamp > 0;
        //						Assert.True(containsTimestamp);
        //						Debug.Log(containsUUID);
        //						bool containsChannel = mea.PresenceEventResult.Channel.Equals(channel) || mea.PresenceEventResult.Channel.Equals(channel2);
        //						Assert.True(containsChannel);
        //						Debug.Log("containsChannel:" + containsChannel);
        //						Debug.Log("containsTimestamp:" + containsTimestamp);
        //						Debug.Log("containsOccupancy:" + containsOccupancy);
        //						Debug.Log("containsUUID:" + containsUUID);

        //						tJoinResult = containsTimestamp && containsOccupancy && containsUUID && containsChannel;
        //					}	
        //				break;
        //				case "leave":
        //					if(mea.PresenceEventResult.Event.Equals("leave")){
        //						bool containsUUID = mea.PresenceEventResult.UUID.Contains(pnConfiguration2.UUID);
        //						Assert.True(containsUUID);
        //						Debug.Log(containsUUID);
        //						bool containsTimestamp = mea.PresenceEventResult.Timestamp > 0;
        //						Assert.True(containsTimestamp);
        //						bool containsChannel = mea.PresenceEventResult.Channel.Equals(channel) || mea.PresenceEventResult.Channel.Equals(channel2);
        //						Assert.True(containsChannel);
        //						bool containsOccupancy = mea.PresenceEventResult.Occupancy > 0;
        //						Assert.True(containsOccupancy);
        //						Debug.Log("containsChannel:" + containsChannel);
        //						Debug.Log("containsTimestamp:" + containsTimestamp);
        //						Debug.Log("containsOccupancy:" + containsOccupancy);
        //						Debug.Log("containsUUID:" + containsUUID);								

        //						tLeaveResult = containsTimestamp && containsOccupancy && containsUUID && containsChannel;
        //					}
        //				break;
        //				default:
        //					Debug.Log("SubscribeCallback" + mea.MessageResult.Subscription);
        //					Debug.Log("SubscribeCallback" + mea.MessageResult.Channel);
        //					Debug.Log("SubscribeCallback" + mea.MessageResult.Payload);
        //					Debug.Log("SubscribeCallback" + mea.MessageResult.Timetoken);
        //					bool matchChannel = mea.MessageResult.Channel.Equals(channel);
        //					Assert.True(matchChannel);
        //					bool matchPayload = mea.MessageResult.Payload.ToString().Equals(payload);
        //					Assert.True(matchPayload);

        //					tresult = matchPayload  && matchChannel;
        //				break;
        //			}
        //		} 
        //	};
        //	pubnub.Subscribe<string>().Channels(channelList2.ToArray()).WithPresence().Execute();
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);
        //	//Assert.True(tJoinResult, "subscribe didn't get a join");

        //	whatToTest = "join2";
        //	PubNub pubnub2 = new PubNub(pnConfiguration2);

        //	pubnub2.Subscribe ().Channels(channelList2.ToArray()).Execute();
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);
        //	Assert.True(tJoinResult, "subscribe2 didn't get a join");

        //	whatToTest = "leave";

        //	tresult = false;
        //	List<string> channelList = new List<string>();
        //	channelList.Add(channel);
        //	pubnub2.Unsubscribe().Channels(channelList.ToArray()).Execute((result, status) => {
        //			Debug.Log("status.Error:" + status.Error);
        //			tresult = !status.Error;
        //			//Debug.Log("result.Message:" + result.Message);
        //		});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);
        //	Assert.True(tresult, "unsubscribe didn't return");

        //	tresult = false;
        //	pubnub2.UnsubscribeAll().Execute((result, status) => {
        //			Debug.Log("status.Error:" + status.Error);
        //			tresult = !status.Error;
        //		});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);
        //	Assert.True(tresult, "unsubscribeAll didn't return");
        //	Assert.True(tLeaveResult, "subscribe didn't get a leave");

        //	pubnub.Destroy();
        //	pubnub2.CleanUp();
        //}

        //public IEnumerator TestReconnect() {
        //	PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
        //	System.Random r = new System.Random ();
        //	pnConfiguration.Uuid = "UnityReconnectUUID" + r.Next (100);
        //	string channel = "UnityReconnectChannel." + r.Next (100);

        //	string payload = string.Format("Reconnect payload {0}", pnConfiguration.Uuid);
        //	Pubnub pubnub = new Pubnub(pnConfiguration);

        //	List<string> channelList2 = new List<string>();
        //	channelList2.Add(channel);
        //	bool tresult = false;
        //	string whatToTest = "join1";

        //	PNConfiguration pnConfiguration2 = PlayModeCommon.SetPNConfig(false);
        //	pnConfiguration2.UUID = "UnityReconnectUUID2" + r.Next (100);

        //	pubnub.SubscribeCallback += (sender, e) => { 
        //		SubscribeEventEventArgs mea = e as SubscribeEventEventArgs;

        //		switch (whatToTest){
        //			case "connected":
        //			if(mea.Status.Category.Equals(PNStatusCategory.PNConnectedCategory)){
        //				tresult = true;
        //			} 
        //			break;
        //			case "join1":
        //			case "join2":
        //			if(!mea.Status.Category.Equals(PNStatusCategory.PNConnectedCategory)){
        //				if ((mea.PresenceEventResult!=null) && (mea.PresenceEventResult.Event.Equals("join"))){
        //					bool containsUUID = false;
        //					if(whatToTest.Equals("join1")){
        //						containsUUID = mea.PresenceEventResult.UUID.Contains(pnConfiguration.Uuid);
        //					} else {
        //						containsUUID = mea.PresenceEventResult.UUID.Contains(pnConfiguration2.UUID);
        //					}
        //					bool containsOccupancy = mea.PresenceEventResult.Occupancy > 0;
        //					Assert.True(containsOccupancy);
        //					bool containsTimestamp = mea.PresenceEventResult.Timestamp > 0;
        //					Assert.True(containsTimestamp);
        //					Debug.Log(containsUUID);
        //					bool containsChannel = mea.PresenceEventResult.Channel.Equals(channel);// || mea.PresenceEventResult.Channel.Equals(channel2);
        //					Assert.True(containsChannel);
        //					Debug.Log("containsChannel:" + containsChannel);
        //					Debug.Log("containsTimestamp:" + containsTimestamp);
        //					Debug.Log("containsOccupancy:" + containsOccupancy);
        //					Debug.Log("containsUUID:" + containsUUID);

        //					tresult = containsTimestamp && containsOccupancy && containsUUID && containsChannel;
        //				}	
        //			}
        //			break;
        //			case "leave":
        //			if(!mea.Status.Category.Equals(PNStatusCategory.PNConnectedCategory)){
        //				if((mea.PresenceEventResult!=null) && (mea.PresenceEventResult.Event.Equals("leave"))){
        //					bool containsUUID = mea.PresenceEventResult.UUID.Contains(pnConfiguration2.UUID);
        //					Assert.True(containsUUID);
        //					Debug.Log(containsUUID);
        //					bool containsTimestamp = mea.PresenceEventResult.Timestamp > 0;
        //					Assert.True(containsTimestamp);
        //					bool containsChannel = mea.PresenceEventResult.Channel.Equals(channel);// || mea.PresenceEventResult.Channel.Equals(channel2);
        //					Assert.True(containsChannel);
        //					bool containsOccupancy = mea.PresenceEventResult.Occupancy > 0;
        //					Assert.True(containsOccupancy);
        //					Debug.Log("containsChannel:" + containsChannel);
        //					Debug.Log("containsTimestamp:" + containsTimestamp);
        //					Debug.Log("containsOccupancy:" + containsOccupancy);
        //					Debug.Log("containsUUID:" + containsUUID);								

        //					tresult = containsTimestamp && containsOccupancy && containsUUID && containsChannel;
        //				}
        //			}
        //			break;
        //			default:
        //			if(!mea.Status.Category.Equals(PNStatusCategory.PNConnectedCategory)){
        //				Debug.Log("SubscribeCallback" + mea.MessageResult.Subscription);
        //				Debug.Log("SubscribeCallback" + mea.MessageResult.Channel);
        //				Debug.Log("SubscribeCallback" + mea.MessageResult.Payload);
        //				Debug.Log("SubscribeCallback" + mea.MessageResult.Timetoken);
        //				bool matchChannel = mea.MessageResult.Channel.Equals(channel);
        //				Assert.True(matchChannel);
        //				bool matchPayload = mea.MessageResult.Payload.ToString().Equals(payload);
        //				Assert.True(matchPayload);

        //				tresult = matchPayload  && matchChannel;
        //			}
        //			break;
        //		}
        //	};
        //	pubnub.Subscribe<string>().Channels(channelList2.ToArray()).WithPresence().Execute();
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);
        //	Assert.True(tresult, "didn't subscribe");

        //	whatToTest = "join2";
        //	PubNub pubnub2 = new PubNub(pnConfiguration2);

        //	tresult = false;

        //	pubnub2.Subscribe ().Channels(channelList2.ToArray()).Execute();
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);
        //	Assert.True(tresult, "subscribe2 didn't get a join");

        //	tresult = false;
        //	pubnub.Reconnect();

        //	pubnub2.Publish().Channel(channel).Message(payload).Execute((result, status) => {
        //		bool timetokenMatch = !result.Timetoken.Equals(0);
        //		bool statusError = status.Error.Equals(false);
        //		bool statusCodeMatch = status.StatusCode.Equals(0);
        //		Assert.True(timetokenMatch);
        //		Assert.True(statusError);
        //		Assert.True(statusCodeMatch, status.StatusCode.ToString());
        //		tresult = statusCodeMatch && statusError && timetokenMatch;
        //	});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls3);

        //	Assert.True(tresult, "publish didn't return");

        //	whatToTest = "";

        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls3);
        //	Assert.True(tresult, "subscribe didn't return");

        //	pubnub.Destroy();
        //	pubnub2.CleanUp();
        //}

        //public IEnumerator TestPresenceCG() {
        //	PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
        //	System.Random r = new System.Random ();
        //	pnConfiguration.Uuid = "UnityTestCGPresUUID_" + r.Next (100);
        //	string channel = "UnityTestPresWithCGChannel";
        //	string channel2 = "UnityTestPresWithCGChannel2";
        //	List<string> channelList = new List<string>();
        //	channelList.Add(channel);
        //	channelList.Add(channel2);

        //	string channelGroup = "cg";
        //	List<string> channelGroupList = new List<string>();
        //	channelGroupList.Add(channelGroup);

        //	Pubnub pubnub = new Pubnub(pnConfiguration);
        //	bool tresult = false;

        //	PNConfiguration pnConfiguration2 = PlayModeCommon.SetPNConfig(false);
        //	pnConfiguration2.UUID = "UnityReconnectUUID2" + r.Next (100);

        //	pubnub.AddChannelsToChannelGroup().Channels(channelList.ToArray()).ChannelGroup(channelGroup).Execute((result, status) => {
        //			Debug.Log ("in AddChannelsToChannelGroup " + status.Error);
        //			if(!status.Error){
        //				Debug.Log(result.Message);
        //				tresult = result.Message.Contains("OK");
        //			} else {
        //				Assert.Fail("AddChannelsToChannelGroup failed");
        //			}
        //		});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls3);
        //	Assert.True(tresult, "test didn't return1");
        //	tresult = false;
        //	string whatToTest = "join1";

        //	pubnub.SubscribeCallback += (sender, e) => { 
        //		SubscribeEventEventArgs mea = e as SubscribeEventEventArgs;

        //		switch (whatToTest){					
        //			case "join1":
        //			case "join2":
        //			if(!mea.Status.Category.Equals(PNStatusCategory.PNConnectedCategory)){
        //				if(mea.PresenceEventResult.Event.Equals("join")){
        //					bool containsUUID = false;
        //					if(whatToTest.Equals("join1")){
        //						containsUUID = mea.PresenceEventResult.UUID.Contains(pnConfiguration.Uuid);
        //					} else {
        //						containsUUID = mea.PresenceEventResult.UUID.Contains(pnConfiguration2.UUID);
        //					}
        //					bool containsOccupancy = mea.PresenceEventResult.Occupancy > 0;
        //					Assert.True(containsOccupancy);
        //					bool containsTimestamp = mea.PresenceEventResult.Timestamp > 0;
        //					Assert.True(containsTimestamp);
        //					Debug.Log(containsUUID);
        //					Debug.Log("mea.PresenceEventResult.Subscription:"+mea.PresenceEventResult.Subscription);
        //					bool containsChannel = mea.PresenceEventResult.Subscription.Equals(channelGroup);// || mea.PresenceEventResult.Channel.Equals(channel2);
        //					Assert.True(containsChannel);
        //					Debug.Log("containsChannel:" + containsChannel);
        //					Debug.Log("containsTimestamp:" + containsTimestamp);
        //					Debug.Log("containsOccupancy:" + containsOccupancy);
        //					Debug.Log("containsUUID:" + containsUUID);

        //					tresult = containsTimestamp && containsOccupancy && containsUUID && containsChannel;
        //				}	
        //			}
        //			break;
        //			case "leave":
        //			if(!mea.Status.Category.Equals(PNStatusCategory.PNConnectedCategory)){
        //				if(mea.PresenceEventResult.Event.Equals("leave")){
        //					bool containsUUID = mea.PresenceEventResult.UUID.Contains(pnConfiguration2.UUID);
        //					Assert.True(containsUUID);
        //					Debug.Log(containsUUID);
        //					bool containsTimestamp = mea.PresenceEventResult.Timestamp > 0;
        //					Assert.True(containsTimestamp);
        //					bool containsChannel = mea.PresenceEventResult.Subscription.Equals(channelGroup);// || mea.PresenceEventResult.Channel.Equals(channel2);
        //					Assert.True(containsChannel);
        //					bool containsOccupancy = mea.PresenceEventResult.Occupancy > 0;
        //					Assert.True(containsOccupancy);
        //					Debug.Log("containsChannel:" + containsChannel);
        //					Debug.Log("containsTimestamp:" + containsTimestamp);
        //					Debug.Log("containsOccupancy:" + containsOccupancy);
        //					Debug.Log("containsUUID:" + containsUUID);								

        //					tresult = containsTimestamp && containsOccupancy && containsUUID && containsChannel;
        //				}
        //			}
        //			break;
        //			default:					
        //			break;
        //		}

        //	};

        //	pubnub.Subscribe<string>().ChannelGroups(channelGroupList.ToArray()).WithPresence().Execute();
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);
        //	//Assert.True(tresult, "subscribe1 didn't get a join");

        //	whatToTest = "join2";
        //	PubNub pubnub2 = new PubNub(pnConfiguration2);

        //	tresult = false;

        //	pubnub2.Subscribe ().ChannelGroups(channelGroupList.ToArray()).Execute();
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);
        //	Assert.True(tresult, "subscribe2 didn't get a join");

        //	whatToTest = "leave";
        //	tresult = false;
        //	pubnub2.Unsubscribe().ChannelGroups(channelGroupList.ToArray()).Execute((result, status) => {
        //			Debug.Log("status.Error:" + status.Error);
        //			//tresult = !status.Error;
        //		});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);
        //	//Assert.True(tresult, "unsubscribeAll didn't return");
        //	Assert.True(tresult, "subscribe didn't get a leave");

        //	pubnub.Destroy();
        //	pubnub2.CleanUp();

        //}	

        //[UnityTest]
        //public IEnumerator TestHistory() {
        //	PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
        //	System.Random r = new System.Random ();
        //	pnConfiguration.Uuid = "UnityTestConnectedUUID_" + r.Next (100);
        //	string channel = "UnityPublishAndHistoryChannel_" + r.Next (100);
        //	string payload = string.Format("payload {0}", pnConfiguration.Uuid);

        //	Pubnub pubnub = new Pubnub(pnConfiguration);

        //	List<string> channelList2 = new List<string>();
        //	channelList2.Add(channel);
        //	bool tresult = false;

        //	long timetoken1 = 0;
        //	pubnub.Time().Execute((result, status) => {
        //		timetoken1 = result.TimeToken;
        //	});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);

        //	Assert.True(!timetoken1.Equals(0));

        //	List<string> payloadList = new List<string>();
        //	for(int i=0; i<4; i++){
        //		payloadList.Add(string.Format("{0}, seq: {1}", payload, i));
        //	}

        //	//Get Time: t1
        //	//Publish 2 msg
        //	//get time: t2
        //	//Publish 2 msg
        //	//get time: t3

        //	for(int i=0; i<2; i++){
        //		tresult = false;

        //		pubnub.Publish().Channel(channel).Message(payloadList[i]).Execute((result, status) => {
        //			bool timetokenMatch = !result.Timetoken.Equals(0);
        //			bool statusError = status.Error.Equals(false);
        //			bool statusCodeMatch = status.StatusCode.Equals(0);
        //			Assert.True(timetokenMatch);
        //			Assert.True(statusError);
        //			Assert.True(statusCodeMatch, status.StatusCode.ToString());
        //			tresult = statusCodeMatch && statusError && timetokenMatch;
        //		});
        //		yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);

        //		Assert.True(tresult, string.Format("test didnt return {0}", i));
        //	}

        //	tresult = false;

        //	long timetoken2 = 0;
        //	pubnub.Time().Execute((result, status) => {
        //		timetoken2 = result.TimeToken;
        //	});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);

        //	Assert.True(!timetoken2.Equals(0));

        //	for(int i=2; i<4; i++){
        //		tresult = false;
        //		pubnub.Publish().Channel(channel).Message(payloadList[i]).Execute((result, status) => {
        //			bool timetokenMatch = !result.Timetoken.Equals(0);
        //			bool statusError = status.Error.Equals(false);
        //			bool statusCodeMatch = status.StatusCode.Equals(0);
        //			Assert.True(timetokenMatch);
        //			Assert.True(statusError);
        //			Assert.True(statusCodeMatch, status.StatusCode.ToString());
        //			tresult = statusCodeMatch && statusError && timetokenMatch;
        //		});
        //		yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);

        //		Assert.True(tresult, string.Format("test didnt return {0}", i));
        //	}

        //	tresult = false;

        //	long timetoken3 = 0;
        //	pubnub.Time().Execute((result, status) => {
        //		timetoken3 = result.TimeToken;
        //	});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);

        //	Assert.True(!timetoken3.Equals(0));

        //	tresult = false;

        //	//History t1 - t2

        //	int testCount = 2;
        //	int testStart = 0;
        //	pubnub.History().Channel(channel).Start(timetoken1).End(timetoken2).IncludeTimetoken(true).Execute((result, status) => {
        //		Assert.True(status.Error.Equals(false));
        //		if(!status.Error){

        //			if((result.Messages!=null) && (result.Messages.Count.Equals(testCount))){
        //				List<PNHistoryItemResult> listPNHistoryItemResult = result.Messages as List<PNHistoryItemResult>;	
        //				for(int i=0; i<testCount; i++){
        //					PNHistoryItemResult pnHistoryItemResult = listPNHistoryItemResult[i] as PNHistoryItemResult;
        //					if(pnHistoryItemResult != null){
        //						bool found = false;
        //						for(int j=0; j<testCount; j++){
        //							if(pnHistoryItemResult.Entry.ToString().Contains(payloadList[j])){
        //								found = (pnHistoryItemResult.Timetoken>0);
        //								Debug.Log("found" + payloadList[j] );
        //								break;
        //							}
        //						}
        //						tresult = found;
        //						if(!tresult){
        //							break;
        //						}
        //					}
        //				}						
        //			} 
        //              } 
        //	});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);
        //	Assert.True(tresult, "history test didnt return");



        //	pubnub.Destroy();
        //}	

        //[UnityTest]
        //public IEnumerator TestHistory2() {
        //	PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
        //	System.Random r = new System.Random ();
        //	pnConfiguration.Uuid = "UnityTestConnectedUUID_" + r.Next (100);
        //	string channel = "UnityPublishAndHistoryChannel2_" + r.Next (100);
        //	string payload = string.Format("payload {0}", pnConfiguration.Uuid);

        //	Pubnub pubnub = new Pubnub(pnConfiguration);

        //	List<string> channelList2 = new List<string>();
        //	channelList2.Add(channel);
        //	bool tresult = false;

        //	long timetoken1 = 0;
        //	pubnub.Time().Execute((result, status) => {
        //		timetoken1 = result.TimeToken;
        //	});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);

        //	Assert.True(!timetoken1.Equals(0));

        //	List<string> payloadList = new List<string>();
        //	for(int i=0; i<4; i++){
        //		payloadList.Add(string.Format("{0}, seq: {1}", payload, i));
        //	}

        //	//Get Time: t1
        //	//Publish 2 msg
        //	//get time: t2
        //	//Publish 2 msg
        //	//get time: t3

        //	for(int i=0; i<2; i++){
        //		tresult = false;

        //		pubnub.Publish().Channel(channel).Message(payloadList[i]).Execute((result, status) => {
        //			bool timetokenMatch = !result.Timetoken.Equals(0);
        //			bool statusError = status.Error.Equals(false);
        //			bool statusCodeMatch = status.StatusCode.Equals(0);
        //			Assert.True(timetokenMatch);
        //			Assert.True(statusError);
        //			Assert.True(statusCodeMatch, status.StatusCode.ToString());
        //			tresult = statusCodeMatch && statusError && timetokenMatch;
        //		});
        //		yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);

        //		Assert.True(tresult, string.Format("test didnt return {0}", i));
        //	}

        //	tresult = false;

        //	long timetoken2 = 0;
        //	pubnub.Time().Execute((result, status) => {
        //		timetoken2 = result.TimeToken;
        //	});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);

        //	Assert.True(!timetoken2.Equals(0));

        //	for(int i=2; i<4; i++){
        //		tresult = false;
        //		pubnub.Publish().Channel(channel).Message(payloadList[i]).Execute((result, status) => {
        //			bool timetokenMatch = !result.Timetoken.Equals(0);
        //			bool statusError = status.Error.Equals(false);
        //			bool statusCodeMatch = status.StatusCode.Equals(0);
        //			Assert.True(timetokenMatch);
        //			Assert.True(statusError);
        //			Assert.True(statusCodeMatch, status.StatusCode.ToString());
        //			tresult = statusCodeMatch && statusError && timetokenMatch;
        //		});
        //		yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);

        //		Assert.True(tresult, string.Format("test didnt return {0}", i));
        //	}

        //	tresult = false;

        //	long timetoken3 = 0;
        //	pubnub.Time().Execute((result, status) => {
        //		timetoken3 = result.TimeToken;
        //	});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);

        //	Assert.True(!timetoken3.Equals(0));

        //	tresult = false;

        //	int testCount = 2;
        //	int testStart = 2;
        //	pubnub.History().Channel(channel).Start(timetoken2).IncludeTimetoken(true).Reverse(true).Execute((result, status) => {
        //		Assert.True(status.Error.Equals(false));
        //		if(!status.Error){

        //			if((result.Messages!=null) && (result.Messages.Count.Equals(testCount))){
        //				List<PNHistoryItemResult> listPNHistoryItemResult = result.Messages as List<PNHistoryItemResult>;	
        //				for(int i=0; i<testCount; i++){
        //					PNHistoryItemResult pnHistoryItemResult = listPNHistoryItemResult[i] as PNHistoryItemResult;
        //					if(pnHistoryItemResult != null){
        //						bool found = false;
        //						Debug.Log("finding:" + pnHistoryItemResult.Entry.ToString() );
        //						for(int j=testStart; j<testCount+testStart; j++){
        //							if(pnHistoryItemResult.Entry.ToString().Contains(payloadList[j])){
        //								found = true;
        //								Debug.Log("found:" + payloadList[j] );
        //								break;
        //							}
        //						}
        //						tresult = found;
        //						if(!tresult){
        //							break;
        //						}
        //					}
        //				}						
        //			} 
        //              } 
        //	});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);
        //	Assert.True(tresult, "history test didnt return");


        //	pubnub.Destroy();
        //}	

        ////Get Time: t1
        ////Publish 2 msg to ch 1
        ////get time: t2
        ////Publish 2 msg to ch 2
        ////get time: t3
        ////Fetch ch 1 and ch 2
        //[UnityTest]
        //public IEnumerator TestFetch() {
        //	PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
        //	System.Random r = new System.Random ();
        //	pnConfiguration.Uuid = "UnityTestFetchUUID_" + r.Next (100);
        //	string channel = "UnityPublishAndFetchChannel_" + r.Next (100);
        //	string channel2 = "UnityPublishAndFetchChannel2_" + r.Next (100);
        //	string payload = string.Format("payload {0}", pnConfiguration.Uuid);

        //	Pubnub pubnub = new Pubnub(pnConfiguration);

        //	List<string> channelList2 = new List<string>();
        //	channelList2.Add(channel);
        //	channelList2.Add(channel2);
        //	bool tresult = false;

        //	long timetoken1 = 0;
        //	pubnub.Time().Execute((result, status) => {
        //		timetoken1 = result.TimeToken;
        //	});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);

        //	Assert.True(!timetoken1.Equals(0));

        //	List<string> payloadList = new List<string>();
        //	for(int i=0; i<4; i++){
        //		payloadList.Add(string.Format("{0}, seq: {1}", payload, i));
        //	}

        //	for(int i=0; i<2; i++){
        //		tresult = false;

        //		pubnub.Publish().Channel(channel).Message(payloadList[i]).Execute((result, status) => {
        //			bool timetokenMatch = !result.Timetoken.Equals(0);
        //			bool statusError = status.Error.Equals(false);
        //			bool statusCodeMatch = status.StatusCode.Equals(0);
        //			Assert.True(timetokenMatch);
        //			Assert.True(statusError);
        //			Assert.True(statusCodeMatch, status.StatusCode.ToString());
        //			tresult = statusCodeMatch && statusError && timetokenMatch;
        //		});
        //		yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);

        //		Assert.True(tresult, string.Format("test didnt return {0}", i));
        //	}

        //	tresult = false;

        //	long timetoken2 = 0;
        //	pubnub.Time().Execute((result, status) => {
        //		timetoken2 = result.TimeToken;
        //	});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);

        //	Assert.True(!timetoken2.Equals(0));

        //	for(int i=2; i<4; i++){
        //		tresult = false;
        //		pubnub.Publish().Channel(channel2).Message(payloadList[i]).Execute((result, status) => {
        //			bool timetokenMatch = !result.Timetoken.Equals(0);
        //			bool statusError = status.Error.Equals(false);
        //			bool statusCodeMatch = status.StatusCode.Equals(0);
        //			Assert.True(timetokenMatch);
        //			Assert.True(statusError);
        //			Assert.True(statusCodeMatch, status.StatusCode.ToString());
        //			tresult = statusCodeMatch && statusError && timetokenMatch;
        //		});
        //		yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);

        //		Assert.True(tresult, string.Format("test didnt return {0}", i));
        //	}

        //	tresult = false;

        //	long timetoken3 = 0;
        //	pubnub.Time().Execute((result, status) => {
        //		timetoken3 = result.TimeToken;
        //	});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);

        //	Assert.True(!timetoken3.Equals(0));

        //	tresult = false;

        //	pubnub.FetchMessages().Channels(channelList2.ToArray()).IncludeTimetoken(true).Execute((result, status) => {
        //		Assert.True(status.Error.Equals(false));
        //		if(!status.Error){

        //			if((result.Channels != null) && (result.Channels.Count.Equals(2))){
        //				Dictionary<string, List<PNMessageResult>> fetchResult = result.Channels as Dictionary<string, List<PNMessageResult>>;
        //				Debug.Log("fetchResult.Count:" + fetchResult.Count);
        //				bool found1 = false, found2 = false;
        //				foreach(KeyValuePair<string, List<PNMessageResult>> kvp in fetchResult){
        //					Debug.Log("Channel:" + kvp.Key);
        //					if(kvp.Key.Equals(channel)){

        //						foreach(PNMessageResult msg in kvp.Value){
        //							Debug.Log("msg.Channel:" + msg.Channel);
        //							Debug.Log("msg.Payload.ToString():" + msg.Payload.ToString());
        //							if(msg.Channel.Equals(channel) && (msg.Payload.ToString().Equals(payloadList[0]) || (msg.Payload.ToString().Equals(payloadList[1])))){
        //								found1 = true;
        //							}
        //						}
        //						if(!found1){
        //							break;
        //						}
        //					}
        //					if(kvp.Key.Equals(channel2)){
        //						foreach(PNMessageResult msg in kvp.Value){
        //							Debug.Log("msg.Channel" + msg.Channel);
        //							Debug.Log("msg.Payload.ToString()" + msg.Payload.ToString());

        //							if(msg.Channel.Equals(channel2) && (msg.Payload.Equals(payloadList[2]) || (msg.Payload.Equals(payloadList[3])))){
        //								found2 = true;
        //							}
        //						}
        //						if(!found2){
        //							break;
        //						}
        //					}
        //				}
        //				tresult = found1 && found2;
        //			}

        //              } 
        //	});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);
        //	Assert.True(tresult, "fetch test didnt return");


        //	pubnub.Destroy();
        //}

        //[UnityTest]
        //public IEnumerator TestFetch3() {
        //	PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
        //	System.Random r = new System.Random ();
        //	pnConfiguration.Uuid = "UnityTestFetchUUID_" + r.Next (100);
        //	string channel = "UnityPublishAndFetchChannel_" + r.Next (100);
        //	string channel2 = "UnityPublishAndFetchChannel2_" + r.Next (100);
        //	string payload = string.Format("payload {0}", pnConfiguration.Uuid);

        //	Pubnub pubnub = new Pubnub(pnConfiguration);

        //	List<string> channelList2 = new List<string>();
        //	channelList2.Add(channel);
        //	channelList2.Add(channel2);
        //	bool tresult = false;

        //	long timetoken1 = 0;
        //	pubnub.Time().Execute((result, status) => {
        //		timetoken1 = result.TimeToken;
        //	});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);

        //	Assert.True(!timetoken1.Equals(0));

        //	List<string> payloadList = new List<string>();
        //	for(int i=0; i<4; i++){
        //		payloadList.Add(string.Format("{0}, seq: {1}", payload, i));
        //	}

        //	for(int i=0; i<2; i++){
        //		tresult = false;

        //		pubnub.Publish().Channel(channel).Message(payloadList[i]).Execute((result, status) => {
        //			bool timetokenMatch = !result.Timetoken.Equals(0);
        //			bool statusError = status.Error.Equals(false);
        //			bool statusCodeMatch = status.StatusCode.Equals(0);
        //			Assert.True(timetokenMatch);
        //			Assert.True(statusError);
        //			Assert.True(statusCodeMatch, status.StatusCode.ToString());
        //			tresult = statusCodeMatch && statusError && timetokenMatch;
        //		});
        //		yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);

        //		Assert.True(tresult, string.Format("test didnt return {0}", i));
        //	}

        //	tresult = false;

        //	long timetoken2 = 0;
        //	pubnub.Time().Execute((result, status) => {
        //		timetoken2 = result.TimeToken;
        //	});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);

        //	Assert.True(!timetoken2.Equals(0));

        //	for(int i=2; i<4; i++){
        //		tresult = false;
        //		pubnub.Publish().Channel(channel2).Message(payloadList[i]).Execute((result, status) => {
        //			bool timetokenMatch = !result.Timetoken.Equals(0);
        //			bool statusError = status.Error.Equals(false);
        //			bool statusCodeMatch = status.StatusCode.Equals(0);
        //			Assert.True(timetokenMatch);
        //			Assert.True(statusError);
        //			Assert.True(statusCodeMatch, status.StatusCode.ToString());
        //			tresult = statusCodeMatch && statusError && timetokenMatch;
        //		});
        //		yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);

        //		Assert.True(tresult, string.Format("test didnt return {0}", i));
        //	}

        //	tresult = false;

        //	long timetoken3 = 0;
        //	pubnub.Time().Execute((result, status) => {
        //		timetoken3 = result.TimeToken;
        //	});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);

        //	Assert.True(!timetoken3.Equals(0));

        //	tresult = false;
        //	pubnub.FetchMessages().Channels(channelList2.ToArray()).End(timetoken1).Execute((result, status) => {
        //		Assert.True(status.Error.Equals(false));
        //		if(!status.Error){

        //			if((result.Channels != null) && (result.Channels.Count.Equals(2))){
        //				Dictionary<string, List<PNMessageResult>> fetchResult = result.Channels as Dictionary<string, List<PNMessageResult>>;
        //				Debug.Log("fetchResult.Count:" + fetchResult.Count);
        //				bool found1 = false, found2 = false;
        //				foreach(KeyValuePair<string, List<PNMessageResult>> kvp in fetchResult){
        //					Debug.Log("Channel:" + kvp.Key);
        //					if(kvp.Key.Equals(channel)){

        //						foreach(PNMessageResult msg in kvp.Value){
        //							Debug.Log("msg.Channel:" + msg.Channel);
        //							Debug.Log("msg.Payload.ToString():" + msg.Payload.ToString());
        //							if(msg.Channel.Equals(channel) && (msg.Payload.ToString().Equals(payloadList[0]) || (msg.Payload.ToString().Equals(payloadList[1])))){
        //								found1 = true;
        //							}
        //						}
        //						if(!found1){
        //							break;
        //						}
        //					}
        //					if(kvp.Key.Equals(channel2)){
        //						foreach(PNMessageResult msg in kvp.Value){
        //							Debug.Log("msg.Channel" + msg.Channel);
        //							Debug.Log("msg.Payload.ToString()" + msg.Payload.ToString());

        //							if(msg.Channel.Equals(channel2) && (msg.Payload.Equals(payloadList[2]) || (msg.Payload.Equals(payloadList[3])))){
        //								found2 = true;
        //							}
        //						}
        //						if(!found2){
        //							break;
        //						}
        //					}
        //				}
        //				tresult = found1 && found2;

        //			}

        //              } 
        //	});
        //	yield return new WaitForSeconds (7);
        //	Assert.True(tresult, "fetch test didnt return 3");
        //	pubnub.Destroy();
        //}

        //[UnityTest]
        //public IEnumerator TestFetch2() {
        //	PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
        //	System.Random r = new System.Random ();
        //	pnConfiguration.Uuid = "UnityTestFetchUUID_" + r.Next (100);
        //	string channel = "UnityPublishAndFetchChannel_" + r.Next (100);
        //	string channel2 = "UnityPublishAndFetchChannel2_" + r.Next (100);
        //	string payload = string.Format("payload {0}", pnConfiguration.Uuid);

        //	Pubnub pubnub = new Pubnub(pnConfiguration);

        //	List<string> channelList2 = new List<string>();
        //	channelList2.Add(channel);
        //	channelList2.Add(channel2);
        //	bool tresult = false;

        //	long timetoken1 = 0;
        //	pubnub.Time().Execute((result, status) => {
        //		timetoken1 = result.TimeToken;
        //	});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);

        //	Assert.True(!timetoken1.Equals(0));

        //	List<string> payloadList = new List<string>();
        //	for(int i=0; i<4; i++){
        //		payloadList.Add(string.Format("{0}, seq: {1}", payload, i));
        //	}

        //	for(int i=0; i<2; i++){
        //		tresult = false;

        //		pubnub.Publish().Channel(channel).Message(payloadList[i]).Execute((result, status) => {
        //			bool timetokenMatch = !result.Timetoken.Equals(0);
        //			bool statusError = status.Error.Equals(false);
        //			bool statusCodeMatch = status.StatusCode.Equals(0);
        //			Assert.True(timetokenMatch);
        //			Assert.True(statusError);
        //			Assert.True(statusCodeMatch, status.StatusCode.ToString());
        //			tresult = statusCodeMatch && statusError && timetokenMatch;
        //		});
        //		yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);

        //		Assert.True(tresult, string.Format("test didnt return {0}", i));
        //	}

        //	tresult = false;

        //	long timetoken2 = 0;
        //	pubnub.Time().Execute((result, status) => {
        //		timetoken2 = result.TimeToken;
        //	});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);

        //	Assert.True(!timetoken2.Equals(0));

        //	for(int i=2; i<4; i++){
        //		tresult = false;
        //		pubnub.Publish().Channel(channel2).Message(payloadList[i]).Execute((result, status) => {
        //			bool timetokenMatch = !result.Timetoken.Equals(0);
        //			bool statusError = status.Error.Equals(false);
        //			bool statusCodeMatch = status.StatusCode.Equals(0);
        //			Assert.True(timetokenMatch);
        //			Assert.True(statusError);
        //			Assert.True(statusCodeMatch, status.StatusCode.ToString());
        //			tresult = statusCodeMatch && statusError && timetokenMatch;
        //		});
        //		yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);

        //		Assert.True(tresult, string.Format("test didnt return {0}", i));
        //	}

        //	tresult = false;

        //	long timetoken3 = 0;
        //	pubnub.Time().Execute((result, status) => {
        //		timetoken3 = result.TimeToken;
        //	});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls);

        //	Assert.True(!timetoken3.Equals(0));

        //	tresult = false;
        //	pubnub.FetchMessages().Channels(channelList2.ToArray()).Start(timetoken2).Reverse(true).Execute((result, status) => {
        //		Assert.True(status.Error.Equals(false));
        //		Debug.Log("status.Error.Equals(false)"+status.Error.Equals(false));
        //		if(!status.Error){

        //			if((result.Channels != null)){
        //				Debug.Log("(result.Channels != null) && (result.Channels.Count.Equals(1))"+((result.Channels != null) && (result.Channels.Count.Equals(1))));
        //				Dictionary<string, List<PNMessageResult>> fetchResult = result.Channels as Dictionary<string, List<PNMessageResult>>;
        //				Debug.Log("fetchResult.Count:" + fetchResult.Count);
        //				bool found1 = false, found2 = false;
        //				foreach(KeyValuePair<string, List<PNMessageResult>> kvp in fetchResult){
        //					Debug.Log("Channel:" + kvp.Key);
        //					if(kvp.Key.Equals(channel)){

        //						foreach(PNMessageResult msg in kvp.Value){
        //							Debug.Log("msg.Channel:" + msg.Channel);
        //							Debug.Log("msg.Payload.ToString():" + msg.Payload.ToString());
        //							if(msg.Channel.Equals(channel) && (msg.Payload.ToString().Equals(payloadList[0]) || (msg.Payload.ToString().Equals(payloadList[1])))){
        //								found1 = true;
        //							}
        //						}
        //						if(!found1){
        //							break;
        //						}
        //					}
        //				}
        //				tresult = found1;
        //			} else {
        //				Debug.Log("(result.Channels == null) && !(result.Channels.Count.Equals(1))" + result.Channels.Count);
        //			}

        //              } 
        //	});
        //	yield return new WaitForSeconds (10);
        //	Assert.True(tresult, "fetch test didnt return 2");
        //	pubnub.Destroy();
        //}


        //[UnityTest]
        //public IEnumerator TestUnsubscribeNoLeave() {
        //	PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
        //	System.Random r = new System.Random ();
        //	pnConfiguration.Uuid = "UnityUnsubUUID_" + r.Next (100);
        //	string channel = "UnityUnubscribeChannel." + r.Next (100);
        //	string channel2 = "UnityUnubscribeChannel." + r.Next (100);

        //	string payload = string.Format("payload {0}", pnConfiguration.Uuid);
        //	//Pubnub pubnub = new Pubnub(pnConfiguration);

        //	List<string> channelList2 = new List<string>();
        //	channelList2.Add(channel);
        //	channelList2.Add(channel2);
        //	string whatToTest = "join1";
        //	bool tJoinResult = false;
        //	bool tLeaveResult = false;
        //	bool tresult = false;

        //	PNConfiguration pnConfiguration2 = PlayModeCommon.SetPNConfig(false);
        //	pnConfiguration2.UUID = "UnityUnsubUUID2_" + r.Next (100);
        //	pnConfiguration2.SuppressLeaveEvents = true;
        //	PubNub pubnub2 = new PubNub(pnConfiguration2);

        //	pubnub2.SubscribeCallback += (sender, e) => { 
        //		SubscribeEventEventArgs mea = e as SubscribeEventEventArgs;
        //		if(!mea.Status.Category.Equals(PNStatusCategory.PNConnectedCategory)){
        //			switch (whatToTest){
        //				case "join1":
        //				case "join2":
        //					if(mea.PresenceEventResult.Event.Equals("join")){
        //						bool containsUUID = false;
        //						if(whatToTest.Equals("join1")){
        //							containsUUID = mea.PresenceEventResult.UUID.Contains(pnConfiguration.Uuid);
        //						} else {
        //							containsUUID = mea.PresenceEventResult.UUID.Contains(pnConfiguration2.UUID);
        //						}
        //						bool containsOccupancy = mea.PresenceEventResult.Occupancy > 0;
        //						Assert.True(containsOccupancy);
        //						bool containsTimestamp = mea.PresenceEventResult.Timestamp > 0;
        //						Assert.True(containsTimestamp);
        //						Debug.Log(containsUUID);
        //						bool containsChannel = mea.PresenceEventResult.Channel.Equals(channel) || mea.PresenceEventResult.Channel.Equals(channel2);
        //						Assert.True(containsChannel);
        //						Debug.Log("containsChannel:" + containsChannel);
        //						Debug.Log("containsTimestamp:" + containsTimestamp);
        //						Debug.Log("containsOccupancy:" + containsOccupancy);
        //						Debug.Log("containsUUID:" + containsUUID);

        //						tJoinResult = containsTimestamp && containsOccupancy && containsUUID && containsChannel;
        //					}	
        //				break;
        //				case "leave":
        //					if(mea.PresenceEventResult.Event.Equals("leave")){
        //						bool containsUUID = mea.PresenceEventResult.UUID.Contains(pnConfiguration2.UUID);
        //						Assert.True(containsUUID);
        //						Debug.Log(containsUUID);
        //						bool containsChannel = mea.PresenceEventResult.Channel.Equals(channel) || mea.PresenceEventResult.Channel.Equals(channel2);
        //						Assert.True(containsChannel);
        //						Debug.Log("containsChannel:" + containsChannel);
        //						Debug.Log("containsUUID:" + containsUUID);								

        //						tLeaveResult = containsUUID && containsChannel;
        //					}
        //				break;
        //				default:
        //					Debug.Log("SubscribeCallback" + mea.MessageResult.Subscription);
        //					Debug.Log("SubscribeCallback" + mea.MessageResult.Channel);
        //					Debug.Log("SubscribeCallback" + mea.MessageResult.Payload);
        //					Debug.Log("SubscribeCallback" + mea.MessageResult.Timetoken);
        //					bool matchChannel = mea.MessageResult.Channel.Equals(channel);
        //					Assert.True(matchChannel);
        //					bool matchPayload = mea.MessageResult.Payload.ToString().Equals(payload);
        //					Assert.True(matchPayload);

        //					tresult = matchPayload  && matchChannel;
        //				break;
        //			}
        //		} 
        //	};

        //	whatToTest = "join2";


        //	pubnub2.Subscribe ().Channels(channelList2.ToArray()).WithPresence().Execute();
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);
        //	Assert.True(tJoinResult, "subscribe2 didn't get a join");

        //	whatToTest = "leave";

        //	tresult = false;
        //	List<string> channelList = new List<string>();
        //	channelList.Add(channel);
        //	tLeaveResult = false;
        //	pubnub2.Unsubscribe().Channels(channelList.ToArray()).Execute((result, status) => {
        //			Debug.Log("status.Error:" + status.Error);
        //			tresult = !status.Error;
        //			//Debug.Log("result.Message:" + result.Message);

        //		});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);
        //	Assert.True(tresult, "unsubscribe didn't return");
        //	Assert.True(!tLeaveResult, "subscribe got a leave");

        //	tresult = false;
        //	tLeaveResult = false;
        //	pubnub2.UnsubscribeAll().Execute((result, status) => {
        //			Debug.Log("status.Error:" + status.Error);
        //			tresult = !status.Error;
        //			//Debug.Log("result.Message:" + result.Message);
        //		});
        //	yield return new WaitForSeconds (PlayModeCommon.WaitTimeBetweenCalls2);
        //	Assert.True(tresult, "unsubscribeAll didn't return");
        //	Assert.True(!tLeaveResult, "subscribe got a leave 2");

        //	//pubnub.Destroy();
        //	pubnub2.CleanUp();
        //}
        #endregion

        //#region "MessageCountsTests"


        //#endregion
        //[UnityTest]
        //public IEnumerator TestMessageCounts()
        //{

        //    PNConfiguration pnConfiguration = PlayModeCommon.SetPNConfig(false);
        //    //pnConfiguration.ConcurrentNonSubscribeWorkers = 5;
        //    System.Random r = new System.Random();
        //    pnConfiguration.Uuid = "UnityTestMessageCountsUUID_" + r.Next(100);
        //    string channel = "UnityPublishAndMessageCountsChannel_" + r.Next(100);
        //    string channel2 = "UnityPublishAndMessageCountsChannel2_" + r.Next(100);
        //    string payload = string.Format("payload {0}", pnConfiguration.Uuid);

        //    Pubnub pubnub = new Pubnub(pnConfiguration);

        //    List<string> channelList2 = new List<string>();
        //    channelList2.Add(channel);
        //    channelList2.Add(channel2);
        //    bool tresult = false;
        //    pubnub.MessageCounts().Channels(channelList2.ToArray()).ChannelsTimetoken(new long[] { 10, 11, 12 }).Execute(new PNMessageCountResultExt((result, status) =>
        //    {
        //        tresult = true;
        //        Assert.True(status.Error.Equals(true));

        //    }));
        //    yield return new WaitForSeconds(1);

        //    tresult = false;
        //    long timetoken1 = 0;
        //    pubnub.Time().Execute(new PNTimeResultExt((result, status) =>
        //    {
        //        timetoken1 = result.Timetoken;
        //    }));
        //    yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls);

        //    Assert.True(!timetoken1.Equals(0));

        //    List<string> payloadList = new List<string>();
        //    for (int i = 0; i < 5; i++)
        //    {
        //        payloadList.Add(string.Format("{0}, seq: {1}", payload, i));
        //    }

        //    for (int i = 0; i < 2; i++)
        //    {
        //        tresult = false;

        //        pubnub.Publish().Channel(channel).Message(payloadList[i]).Execute(new PNPublishResultExt((result, status) =>
        //        {
        //            bool timetokenMatch = !result.Timetoken.Equals(0);
        //            bool statusError = status.Error.Equals(false);
        //            bool statusCodeMatch = status.StatusCode.Equals(0);
        //            Assert.True(timetokenMatch);
        //            Assert.True(statusError);
        //            Assert.True(statusCodeMatch, status.StatusCode.ToString());
        //            //Debug.Log(status.ErrorData + "" + status.StatusCode);
        //            tresult = statusCodeMatch && statusError && timetokenMatch;
        //        }));
        //        yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls2);

        //        Assert.True(tresult, string.Format("test didnt return {0}", i));
        //    }

        //    tresult = false;

        //    long timetoken2 = 0;
        //    pubnub.Time().Execute(new PNTimeResultExt((result, status) =>
        //    {
        //        timetoken2 = result.Timetoken;
        //    }));
        //    yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls);

        //    Assert.True(!timetoken2.Equals(0));

        //    for (int i = 2; i < 5; i++)
        //    {
        //        tresult = false;
        //        pubnub.Publish().Channel(channel2).Message(payloadList[i]).Execute(new PNPublishResultExt((result, status) =>
        //        {
        //            bool timetokenMatch = !result.Timetoken.Equals(0);
        //            bool statusError = status.Error.Equals(false);
        //            bool statusCodeMatch = status.StatusCode.Equals(0);
        //            Assert.True(timetokenMatch);
        //            Assert.True(statusError);
        //            Assert.True(statusCodeMatch, status.StatusCode.ToString());
        //            tresult = statusCodeMatch && statusError && timetokenMatch;
        //        }));
        //        yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls2);

        //        Assert.True(tresult, string.Format("test didnt return {0}", i));
        //    }

        //    tresult = false;

        //    long timetoken3 = 0;
        //    pubnub.Time().Execute(new PNTimeResultExt((result, status) =>
        //    {
        //        timetoken3 = result.Timetoken;
        //    }));
        //    yield return new WaitForSeconds(PlayModeCommon.WaitTimeBetweenCalls);

        //    Assert.True(!timetoken3.Equals(0));

        //    tresult = false;
        //    pubnub.MessageCounts().Channels(channelList2.ToArray()).ChannelsTimetoken(new long[] { timetoken2, timetoken3 }).Execute(new PNMessageCountResultExt((result, status) =>
        //    {
        //        Assert.True(status.Error.Equals(false));
        //        Debug.Log("status.Error.Equals(false)" + status.Error.Equals(false));
        //        if (!status.Error)
        //        {

        //            if ((result.Channels != null))
        //            {
        //                Debug.Log(string.Format("MessageCounts, {0}", result.Channels.Count));
        //                foreach (KeyValuePair<string, long> kvp in result.Channels)
        //                {
        //                    Debug.Log(string.Format("==kvp.Key {0}, kvp.Value {1} ", kvp.Key, kvp.Value));
        //                    if (kvp.Key.Equals(channel))
        //                    {
        //                        tresult = true;
        //                        Debug.Log(string.Format("kvp.Key {0}, kvp.Value {1} ", kvp.Key, kvp.Value));
        //                        Assert.Equals(2, kvp.Value);
        //                    }
        //                    if (kvp.Key.Equals(channel2))
        //                    {
        //                        tresult = true;
        //                        Debug.Log(string.Format("kvp.Key {0}, kvp.Value {1} ", kvp.Key, kvp.Value));
        //                        Assert.Equals(3, kvp.Value);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                Debug.Log("(result.Channels == null) && !(result.Channels.Count.Equals(1))" + result.Channels.Count);
        //            }
        //        }
        //    }));
        //    yield return new WaitForSeconds(3);
        //    Assert.True(tresult, "MessageCounts test didnt return 2");

        //    tresult = false;
        //    pubnub.MessageCounts().Channels(channelList2.ToArray()).ChannelsTimetoken(new long[] { timetoken2 }).Execute(new PNMessageCountResultExt((result, status) =>
        //    {
        //        Assert.True(status.Error.Equals(false));
        //        Debug.Log("status.Error.Equals(false)" + status.Error.Equals(false));
        //        if (!status.Error)
        //        {

        //            if ((result.Channels != null))
        //            {
        //                Debug.Log(string.Format("MessageCounts, {0}", result.Channels.Count));
        //                foreach (KeyValuePair<string, long> kvp in result.Channels)
        //                {
        //                    Debug.Log(string.Format("==kvp.Key {0}, kvp.Value {1} ", kvp.Key, kvp.Value));
        //                    if (kvp.Key.Equals(channel))
        //                    {
        //                        tresult = true;
        //                        Debug.Log(string.Format("kvp.Key {0}, kvp.Value {1} ", kvp.Key, kvp.Value));
        //                        Assert.Equals(0, kvp.Value);
        //                    }
        //                    if (kvp.Key.Equals(channel2))
        //                    {
        //                        tresult = true;
        //                        Debug.Log(string.Format("kvp.Key {0}, kvp.Value {1} ", kvp.Key, kvp.Value));
        //                        Assert.Equals(3, kvp.Value);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                Debug.Log("(result.Channels == null) && !(result.Channels.Count.Equals(1))" + result.Channels.Count);
        //            }
        //        }
        //    }));
        //    yield return new WaitForSeconds(3);
        //    Assert.True(tresult, "MessageCounts test didnt return 2");
        //    pubnub.Destroy();
        //}
#endif
    }
}
