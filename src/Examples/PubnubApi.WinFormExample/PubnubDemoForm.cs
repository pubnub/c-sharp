﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PubnubApi.WinFormExample
{
    public partial class PubnubDemoForm : Form
    {
        static private Pubnub[] pubnub;
        PNConfiguration config1 = new PNConfiguration("uuidone");
        PNConfiguration config2 = new PNConfiguration("uuidtwo");
        string origin = "ps.pndsn.com";

        public class MyLog1 : IPubnubLog
        {
            void IPubnubLog.WriteToLog(string logText)
            {
                System.Diagnostics.Debug.WriteLine("LOGONE: " + logText);
            }
        }
        public class MyLog2 : IPubnubLog
        {
            void IPubnubLog.WriteToLog(string logText)
            {
                System.Diagnostics.Debug.WriteLine("LOGTWO: " + logText);
            }
        }

        public PubnubDemoForm()
        {
            InitializeComponent();
            pubnub = new Pubnub[2];
        }

        private void InitializeFirstInstanceConfig()
        {
            config1.Origin = origin;

            config1.Secure = false;
            config1.CipherKey = "";
            config1.SubscribeKey = "demo-36";
            config1.PublishKey = "demo-36";
            config1.SecretKey = "demo-36";
            config1.ReconnectionPolicy = PNReconnectionPolicy.LINEAR;
            config1.IncludeInstanceIdentifier = true;
            config1.IncludeRequestIdentifier = true;
            config1.LogVerbosity = PNLogVerbosity.BODY;
            config1.Uuid = "myuuid1";
            config1.PubnubLog = new MyLog1();
        }

        private void InitializeSecondInstanceConfig()
        {
            config2.Origin = origin;

            config2.Secure = false;
            config2.CipherKey = "";
            config2.SubscribeKey = "demo-36";
            config2.PublishKey = "demo-36";
            config2.SecretKey = "demo-36";
            config2.ReconnectionPolicy = PNReconnectionPolicy.LINEAR;
            config2.IncludeInstanceIdentifier = true;
            config2.IncludeRequestIdentifier = true;
            config2.LogVerbosity = PNLogVerbosity.BODY;
            config2.Uuid = "myuuid2";
            config2.PubnubLog = new MyLog2();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeFirstInstanceConfig();
            InitializeSecondInstanceConfig();
            lvResults1.View = View.List;
            lvResults2.View = View.List;

            lvResults1.Scrollable = true;
            lvResults2.Scrollable = true;

            lvResults1.FullRowSelect = true;
            lvResults1.GridLines = true;
            lvResults2.FullRowSelect = true;
            lvResults2.GridLines = true;

            //lvResults1.

            txtChannels1.Text = "aaa";
            txtChannels2.Text = "bbb";

            txtMessage1.Text = "hello world 1";
            txtMessage2.Text = "hello world 2";

            pubnub[0] = new Pubnub(config1);
            pubnub[1] = new Pubnub(config2);

            pubnub[0].AddListener(new SubscribeCallbackExt(
                (o, m) => {
                            Invoke(new Action(() => {
                                lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(m));
                            }));
                     },
                (o, p) => {
                            Invoke(new Action(() => {
                                lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(p));
                            }));
                     },
                (o, s) => {
                    Invoke(new Action(() => {
                        lvResults1.Items.Add(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    }));
                    
                }));
            pubnub[1].AddListener(new SubscribeCallbackExt(
                (o, m) => {
                    Invoke(new Action(() => {
                        lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(m));
                    }));
                },
                (o, p) => {
                    Invoke(new Action(() => {
                        lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(p));
                    }));
                },
                (o, s) => {
                    Invoke(new Action(() => {
                        lvResults2.Items.Add(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    }));
                }));
        }

        private void btnSubscribe1_Click(object sender, EventArgs e)
        {
            pubnub[0].Subscribe<string>()
                                .WithPresence()
                                .Channels(txtChannels1.Text.Split(','))
                                .ChannelGroups(txtChannelGroup1.Text.Split(','))
                                .Execute();
        }

        private void btnSubscribe2_Click(object sender, EventArgs e)
        {
            pubnub[1].Subscribe<string>()
                                .WithPresence()
                                .Channels(txtChannels2.Text.Split(','))
                                .ChannelGroups(txtChannelGroup2.Text.Split(','))
                                .Execute();
        }

        private void btnUnsub1_Click(object sender, EventArgs e)
        {
            pubnub[0].Unsubscribe<string>()
                                .Channels(txtChannels1.Text.Split(','))
                                .ChannelGroups(txtChannelGroup1.Text.Split(','))
                                .Execute();
        }

        private void btnUnsub2_Click(object sender, EventArgs e)
        {
            pubnub[1].Unsubscribe<string>()
                                .Channels(txtChannels2.Text.Split(','))
                                .ChannelGroups(txtChannelGroup2.Text.Split(','))
                                .Execute();
        }

        private void btnPublish1_Click(object sender, EventArgs e)
        {
            pubnub[0].Publish()
                .Channel(txtChannels1.Text)
                .Message(txtMessage1.Text)
                .ShouldStore(true)
                .UsePOST(false)
                .Execute(new PNPublishResultExt((r, s) => {
                    Invoke(new Action(() => {
                        if (r != null)
                        {
                            lvResults1.Items.Add(r.Timetoken.ToString());
                        }
                        else
                        {
                            lvResults1.Items.Add(s.ErrorData.Information);
                        }
                    }));
                }));

        }

        private void btnPublish2_Click(object sender, EventArgs e)
        {
            pubnub[1].Publish()
                .Channel(txtChannels2.Text)
                .Message(txtMessage2.Text)
                .ShouldStore(true)
                .UsePOST(false)
                .Execute(new PNPublishResultExt((r, s) => {
                    Invoke(new Action(() => {
                        if (r != null)
                        {
                            lvResults2.Items.Add(r.Timetoken.ToString());
                        }
                        else
                        {
                            lvResults2.Items.Add(s.ErrorData.Information);
                        }
                    }));
                }));
        }

        private void btnDisconnect1_Click(object sender, EventArgs e)
        {
            pubnub[0].Disconnect<string>();
        }

        private void btnReconnect1_Click(object sender, EventArgs e)
        {
            pubnub[0].Reconnect<string>();
        }

        private void btnDisconnect2_Click(object sender, EventArgs e)
        {
            pubnub[1].Disconnect<string>();
        }

        private void btnReconnect2_Click(object sender, EventArgs e)
        {
            pubnub[1].Reconnect<string>();
        }

        private void btnHereNow1_Click(object sender, EventArgs e)
        {
            pubnub[0].HereNow()
                .Channels(new string[] { txtChannels1.Text })
                .IncludeState(true)
                .IncludeUUIDs(true)
                .Execute(new PNHereNowResultEx((r, s) => {
                    Invoke(new Action(() => {
                        lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                    }));
                }));
        }

        private void btnGlobalHereNow1_Click(object sender, EventArgs e)
        {
            pubnub[0].HereNow()
                .IncludeState(true)
                .IncludeUUIDs(true)
                .Execute(new PNHereNowResultEx((r, s) => {
                    Invoke(new Action(() => {
                        lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                    }));
                }));
        }

        private void btnHereNow2_Click(object sender, EventArgs e)
        {
            pubnub[1].HereNow()
                .Channels(new string[] { txtChannels2.Text })
                .IncludeState(true)
                .IncludeUUIDs(true)
                .Execute(new PNHereNowResultEx((r, s) => {
                    Invoke(new Action(() => {
                        lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(r));
                    }));
                }));
        }

        private void btnGlobalHereNow2_Click(object sender, EventArgs e)
        {
            pubnub[1].HereNow()
                .IncludeState(true)
                .IncludeUUIDs(true)
                .Execute(new PNHereNowResultEx((r, s) => {
                    Invoke(new Action(() => {
                        lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(r));
                    }));
                }));
        }

        private void btnTime1_Click(object sender, EventArgs e)
        {
            pubnub[0].Time()
                .Execute(new PNTimeResultExt((r, s) => {
                    Invoke(new Action(() => {
                        lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                    }));
                }));
        }

        private void btnTime2_Click(object sender, EventArgs e)
        {
            pubnub[1].Time()
                .Execute(new PNTimeResultExt((r, s) => {
                    Invoke(new Action(() => {
                        lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(r));
                    }));
                }));
        }

        private void btnWhereNow1_Click(object sender, EventArgs e)
        {
            pubnub[0].WhereNow()
                .Uuid( config1.Uuid )
                .Execute(new PNWhereNowResultExt((r, s) => {
                    Invoke(new Action(() => {
                        lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                    }));
                }));
        }

        private void btnWhereNow2_Click(object sender, EventArgs e)
        {
            pubnub[1].WhereNow()
                .Uuid(config2.Uuid)
                .Execute(new PNWhereNowResultExt((r, s) => {
                    Invoke(new Action(() => {
                        lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(r));
                    }));
                }));
        }

        private void btnClearResult1_Click(object sender, EventArgs e)
        {
            lvResults1.Items.Clear();
        }

        private void btnClearResult2_Click(object sender, EventArgs e)
        {
            lvResults2.Items.Clear();
        }

        private void btnPAM1_Click(object sender, EventArgs e)
        {
            frmAccessManager accessManager = new frmAccessManager();
            accessManager.ChannelName = txtChannels1.Text;
            DialogResult result = accessManager.ShowDialog();
            if (result == DialogResult.OK)
            {
                pubnub[0].Grant()
                    .Channels(accessManager.ChannelName.Split(','))
                    .ChannelGroups(null)
                    .AuthKeys(null)
                    .Read(accessManager.AccessRead)
                    .Write(accessManager.AccessWrite)
                    .Manage(accessManager.AccessManage)
                    .TTL(accessManager.TTL)
                    .Execute(new PNAccessManagerGrantResultExt(
                        (r, s) =>
                        {
                            Invoke(new Action(() => {
                                if (r != null)
                                {
                                    lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                                }
                                else if (s != null)
                                {
                                    lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(s));
                                }
                                
                            }));
                            
                        })
                        );
            }
        }

        private void btnPAM2_Click(object sender, EventArgs e)
        {
            frmAccessManager accessManager = new frmAccessManager();
            accessManager.ChannelName = txtChannels2.Text;
            DialogResult result = accessManager.ShowDialog();
            if (result == DialogResult.OK)
            {
                pubnub[1].Grant()
                    .Channels(accessManager.ChannelName.Split(','))
                    .ChannelGroups(null)
                    .AuthKeys(null)
                    .Read(accessManager.AccessRead)
                    .Write(accessManager.AccessWrite)
                    .Manage(accessManager.AccessManage)
                    .TTL(accessManager.TTL)
                    .Execute(new PNAccessManagerGrantResultExt(
                        (r, s) =>
                        {
                            Invoke(new Action(() => {
                                if (r != null)
                                {
                                    lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(r));
                                }
                                else if (s != null)
                                {
                                    lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(s));
                                }

                            }));

                        })
                        );
            }
        }

        private void btnPnConfig1_Click(object sender, EventArgs e)
        {
            frmPNConfig popupConfig = new frmPNConfig();
            popupConfig.SubscribeKey = config1.SubscribeKey;
            popupConfig.PublishKey= config1.PublishKey;
            popupConfig.SecretKey = config1.SecretKey;
            popupConfig.CipherKey = config1.CipherKey;
            popupConfig.AuthKey = config1.AuthKey;
            popupConfig.UUID= config1.Uuid;
            popupConfig.Secure = config1.Secure;

            DialogResult result = popupConfig.ShowDialog();
            if (result == DialogResult.OK)
            {
                config1.SubscribeKey = popupConfig.SubscribeKey;
                config1.PublishKey = popupConfig.PublishKey;
                config1.SecretKey = popupConfig.SecretKey;
                config1.CipherKey = popupConfig.CipherKey;
                config1.AuthKey = popupConfig.AuthKey;
                config1.Uuid = popupConfig.UUID;
                config1.Secure = popupConfig.Secure;
            }
        }

        private void btnPnConfig2_Click(object sender, EventArgs e)
        {
            frmPNConfig popupConfig = new frmPNConfig();
            popupConfig.SubscribeKey = config2.SubscribeKey;
            popupConfig.PublishKey = config2.PublishKey;
            popupConfig.SecretKey = config2.SecretKey;
            popupConfig.CipherKey = config2.CipherKey;
            popupConfig.AuthKey = config2.AuthKey;
            popupConfig.UUID = config2.Uuid;
            popupConfig.Secure = config2.Secure;

            DialogResult result = popupConfig.ShowDialog();
            if (result == DialogResult.OK)
            {
                config2.SubscribeKey = popupConfig.SubscribeKey;
                config2.PublishKey = popupConfig.PublishKey;
                config2.SecretKey = popupConfig.SecretKey;
                config2.CipherKey = popupConfig.CipherKey;
                config2.AuthKey = popupConfig.AuthKey;
                config2.Uuid = popupConfig.UUID;
                config2.Secure = popupConfig.Secure;
            }
        }

        private void btnCG1_Click(object sender, EventArgs e)
        {
            frmChannelGroup cg = new frmChannelGroup();
            DialogResult result = cg.ShowDialog();
            if (result == DialogResult.OK)
            {
                string channelGroup = cg.ChannelGroup;
                string cgAuthKey = cg.AuthKey;
                switch (cg.CgRequestType.ToLower())
                {
                    case "addchannel":
                        pubnub[0].AddChannelsToChannelGroup()
                            .ChannelGroup(channelGroup)
                            .Channels(cg.ChannelName.Split(','))
                            .Execute(new PNChannelGroupsAddChannelResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "listchannel":
                        pubnub[0].ListChannelsForChannelGroup()
                            .ChannelGroup(channelGroup)
                            .Execute(new PNChannelGroupsAllChannelsResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "removechannel":
                        pubnub[0].RemoveChannelsFromChannelGroup()
                            .ChannelGroup(channelGroup)
                            .Channels(cg.ChannelName.Split(','))
                            .Execute(new PNChannelGroupsRemoveChannelResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    default:
                        break;
                }
            }
        }

        private void btnCG2_Click(object sender, EventArgs e)
        {
            frmChannelGroup cg = new frmChannelGroup();
            DialogResult result = cg.ShowDialog();
            if (result == DialogResult.OK)
            {
                string channelGroup = cg.ChannelGroup;
                string cgAuthKey = cg.AuthKey;
                switch (cg.CgRequestType.ToLower())
                {
                    case "addchannel":
                        pubnub[1].AddChannelsToChannelGroup()
                            .ChannelGroup(channelGroup)
                            .Channels(cg.ChannelName.Split(','))
                            .Execute(new PNChannelGroupsAddChannelResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "listchannel":
                        pubnub[1].ListChannelsForChannelGroup()
                            .ChannelGroup(channelGroup)
                            .Execute(new PNChannelGroupsAllChannelsResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "removechannel":
                        pubnub[1].RemoveChannelsFromChannelGroup()
                            .ChannelGroup(channelGroup)
                            .Channels(cg.ChannelName.Split(','))
                            .Execute(new PNChannelGroupsRemoveChannelResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    default:
                        break;
                }
            }
        }

        private void btnState1_Click(object sender, EventArgs e)
        {
            frmUserState userState = new frmUserState();
            userState.ChannelName = txtChannels1.Text;
            userState.ChannelGroup = txtChannelGroup1.Text;
            userState.AuthKey = config1.AuthKey;
            DialogResult result = userState.ShowDialog();
            if (result == DialogResult.OK)
            {
                string userStateChannelGroup = userState.ChannelGroup;
                string userStateChannel = userState.ChannelName;
                string cgAuthKey = userState.AuthKey;
                switch (userState.StateRequestType.ToLower())
                {
                    case "set":
                        pubnub[0].SetPresenceState()
                            .Channels(userStateChannel.Split(','))
                            .ChannelGroups(userStateChannelGroup.Split(','))
                            .State(userState.StateKV)
                            .Execute(new PNSetStateResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "get":
                        pubnub[0].GetPresenceState()
                            .Channels(userStateChannel.Split(','))
                            .ChannelGroups(userStateChannelGroup.Split(','))
                            .Uuid(userState.UUID)
                            .Execute(new PNGetStateResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    default:
                        break;
                }
            }
        }

        private void btnState2_Click(object sender, EventArgs e)
        {
            frmUserState userState = new frmUserState();
            userState.ChannelName = txtChannels2.Text;
            userState.ChannelGroup = txtChannelGroup2.Text;
            userState.AuthKey = config2.AuthKey;
            DialogResult result = userState.ShowDialog();
            if (result == DialogResult.OK)
            {
                string userStateChannelGroup = userState.ChannelGroup;
                string userStateChannel = userState.ChannelName;
                string cgAuthKey = userState.AuthKey;
                switch (userState.StateRequestType.ToLower())
                {
                    case "set":
                        pubnub[1].SetPresenceState()
                            .Channels(userStateChannel.Split(','))
                            .ChannelGroups(userStateChannelGroup.Split(','))
                            .State(userState.StateKV)
                            .Execute(new PNSetStateResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults2.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults2.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "get":
                        pubnub[1].GetPresenceState()
                            .Channels(userStateChannel.Split(','))
                            .ChannelGroups(userStateChannelGroup.Split(','))
                            .Uuid(userState.UUID)
                            .Execute(new PNGetStateResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    default:
                        break;
                }
            }

        }

        private void btnHistory1_Click(object sender, EventArgs e)
        {
            frmHistory hist = new frmHistory();
            hist.ChannelName = txtChannels1.Text;
            DialogResult result = hist.ShowDialog();
            if (result == DialogResult.OK)
            {
                pubnub[0].History()
                            .Channel(hist.ChannelName)
                            .Reverse(hist.Reverse)
                            .Count(hist.Count)
                            .IncludeTimetoken(hist.IncludeTimetoken)
                            .Execute(new PNHistoryResultExt(
                                (r, s) =>
                                {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
            }
        }

        private void btnHistory2_Click(object sender, EventArgs e)
        {
            frmHistory hist = new frmHistory();
            hist.ChannelName = txtChannels2.Text;
            DialogResult result = hist.ShowDialog();
            if (result == DialogResult.OK)
            {
                pubnub[1].History()
                            .Channel(hist.ChannelName)
                            .Reverse(hist.Reverse)
                            .Count(hist.Count)
                            .IncludeTimetoken(hist.IncludeTimetoken)
                            .Execute(new PNHistoryResultExt(
                                (r, s) =>
                                {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
            }
        }

        private void txtPush1_Click(object sender, EventArgs e)
        {
            frmPush pushForm = new frmPush();
            DialogResult result = pushForm.ShowDialog();
            if (result == DialogResult.OK)
            {
                string requesttype = pushForm.PushRequestType;
                PNPushType pushType = PNPushType.GCM;
                switch (pushForm.PushServiceType.ToLower())
                {
                    case "gcm":
                        pushType = PNPushType.GCM;
                        break;
                    case "apns":
                        pushType = PNPushType.APNS;
                        break;
                    default:
                        break;
                }
                switch (requesttype.ToLower())
                {
                    case "add":
                        pubnub[0].AddPushNotificationsOnChannels().Channels(pushForm.ChannelName.Split(','))
                                                    .PushType(pushType)
                                                    .DeviceId(pushForm.DeviceId)
                                                    .Execute(new PNPushAddChannelResultExt(
                                                        (r, s) => {
                                                            Invoke(new Action(() => {
                                                                if (r != null)
                                                                {
                                                                    lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                                                                }
                                                                else if (s != null)
                                                                {
                                                                    lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(s));
                                                                }

                                                            }));
                                                        }));
                        break;
                    case "list":
                        pubnub[0].AuditPushChannelProvisions()
                            .PushType(pushType)
                            .DeviceId(pushForm.DeviceId)
                            .Execute(new PNPushListProvisionsResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "remove":
                        pubnub[0].RemovePushNotificationsFromChannels()
                            .Channels(pushForm.ChannelName.Split(','))
                            .PushType(pushType)
                            .DeviceId(pushForm.DeviceId)
                            .Execute(new PNPushRemoveChannelResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    default:
                        break;
                }
            }
        }

        private void txtPush2_Click(object sender, EventArgs e)
        {
            frmPush pushForm = new frmPush();
            DialogResult result = pushForm.ShowDialog();
            if (result == DialogResult.OK)
            {
                string requesttype = pushForm.PushRequestType;
                PNPushType pushType = PNPushType.GCM;
                switch (pushForm.PushServiceType.ToLower())
                {
                    case "gcm":
                        pushType = PNPushType.GCM;
                        break;
                    case "apns":
                        pushType = PNPushType.APNS;
                        break;
                    default:
                        break;
                }
                switch (requesttype.ToLower())
                {
                    case "add":
                        pubnub[1].AddPushNotificationsOnChannels().Channels(pushForm.ChannelName.Split(','))
                                                    .PushType(pushType)
                                                    .DeviceId(pushForm.DeviceId)
                                                    .Execute(new PNPushAddChannelResultExt(
                                                        (r, s) => {
                                                            Invoke(new Action(() => {
                                                                if (r != null)
                                                                {
                                                                    lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(r));
                                                                }
                                                                else if (s != null)
                                                                {
                                                                    lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(s));
                                                                }

                                                            }));
                                                        }));
                        break;
                    case "list":
                        pubnub[1].AuditPushChannelProvisions()
                            .PushType(pushType)
                            .DeviceId(pushForm.DeviceId)
                            .Execute(new PNPushListProvisionsResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "remove":
                        pubnub[1].RemovePushNotificationsFromChannels()
                            .Channels(pushForm.ChannelName.Split(','))
                            .PushType(pushType)
                            .DeviceId(pushForm.DeviceId)
                            .Execute(new PNPushRemoveChannelResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    default:
                        break;
                }
            }
        }

        private void btnFire1_Click(object sender, EventArgs e)
        {
            pubnub[0].Fire()
                .Channel(txtChannels1.Text)
                .Message(txtMessage1.Text)
                .UsePOST(false)
                .Execute(new PNPublishResultExt((r, s) => {
                    Invoke(new Action(() => {
                        lvResults1.Items.Add(r.Timetoken.ToString());
                    }));
                }));
        }

        private void btnFire2_Click(object sender, EventArgs e)
        {
            pubnub[1].Fire()
                .Channel(txtChannels2.Text)
                .Message(txtMessage2.Text)
                .UsePOST(false)
                .Execute(new PNPublishResultExt((r, s) => {
                    Invoke(new Action(() => {
                        lvResults2.Items.Add(r.Timetoken.ToString());
                    }));
                }));
        }

        private void btnGrantToken1_Click(object sender, EventArgs e)
        {
            frmTokenAccessManager accessManager = new frmTokenAccessManager();
            accessManager.ChannelName = txtChannels1.Text;
            accessManager.ChannelGroup = txtChannelGroup1.Text;
            DialogResult result = accessManager.ShowDialog();
            if (result == DialogResult.OK)
            {
                PNTokenAuthValues perm = new PNTokenAuthValues
                {
                    Read = accessManager.AccessRead,
                    Write = accessManager.AccessWrite,
                    Manage = accessManager.AccessManage,
                    Delete = accessManager.AccessDelete,
                    Create = accessManager.AccessCreate
                };
                pubnub[0].GrantToken()
                    .Resources(new PNTokenResources() 
                        { 
                            Users = new Dictionary<string, PNTokenAuthValues>() { { accessManager.UserId, perm } },
                            Spaces = new Dictionary<string, PNTokenAuthValues>() { { accessManager.SpaceId, perm } },
                        })
                    .TTL(accessManager.TTL)
                    .Execute(new PNAccessManagerTokenResultExt(
                        (r, s) =>
                        {
                            Invoke(new Action(() => {
                                if (r != null)
                                {
                                    if (!string.IsNullOrEmpty(r.Token))
                                    {
                                        accessManager.AuthKey = r.Token;
                                    }
                                    lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                                }
                                else if (s != null)
                                {
                                    lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(s));
                                }

                            }));

                        })
                        );
            }
        }

        private void btnGrantToken2_Click(object sender, EventArgs e)
        {
            frmTokenAccessManager accessManager = new frmTokenAccessManager();
            accessManager.ChannelName = txtChannels2.Text;
            accessManager.ChannelGroup = txtChannelGroup2.Text;
            DialogResult result = accessManager.ShowDialog();
            if (result == DialogResult.OK)
            {
                PNTokenAuthValues perm = new PNTokenAuthValues
                {
                    Read = accessManager.AccessRead,
                    Write = accessManager.AccessWrite,
                    Manage = accessManager.AccessManage,
                    Delete = accessManager.AccessDelete,
                    Create = accessManager.AccessCreate
                };
                pubnub[1].GrantToken()
                    .Resources(new PNTokenResources()
                    {
                        Users = new Dictionary<string, PNTokenAuthValues>() { { accessManager.UserId, perm } },
                        Spaces = new Dictionary<string, PNTokenAuthValues>() { { accessManager.SpaceId, perm } },
                    })
                    .TTL(accessManager.TTL)
                    .Execute(new PNAccessManagerTokenResultExt(
                        (r, s) =>
                        {
                            Invoke(new Action(() => {
                                if (r != null)
                                {
                                    if (!string.IsNullOrEmpty(r.Token))
                                    {
                                        accessManager.AuthKey = r.Token;
                                    }
                                    lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(r));
                                }
                                else if (s != null)
                                {
                                    lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(s));
                                }

                            }));

                        })
                        );
            }
        }

        private void lvResults1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(e.Item.Text);
        }

        private void lvResults2_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(e.Item.Text);
        }

        private void btnObjects1_Click(object sender, EventArgs e)
        {
            frmObjects obj = new frmObjects();
            DialogResult result = obj.ShowDialog();
            if (result == DialogResult.OK)
            {
                switch (obj.ObjectsRequestType.ToLower())
                {
                    case "createuser":
                        pubnub[0].SetUuidMetadata()
                            .Uuid(obj.UserId)
                            .Name(obj.UserName)
                            .Execute(new PNSetUuidMetadataResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "updateuser":
                        pubnub[0].SetUuidMetadata()
                            .Uuid(obj.UserId)
                            .Name(obj.UserName)
                            .Execute(new PNSetUuidMetadataResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "deleteuser":
                        pubnub[0].RemoveUuidMetadata()
                            .Uuid(obj.UserId)
                            .Execute(new PNRemoveUuidMetadataResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "getuser":
                        pubnub[0].GetUuidMetadata()
                            .Uuid(obj.UserId)
                            .Execute(new PNGetUuidMetadataResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "createspace":
                        pubnub[0].SetChannelMetadata()
                            .Channel(obj.SpaceId)
                            .Name(obj.SpaceName)
                            .Execute(new PNSetChannelMetadataResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "updatespace":
                        pubnub[0].SetChannelMetadata()
                            .Channel(obj.SpaceId)
                            .Name(obj.SpaceName)
                            .Execute(new PNSetChannelMetadataResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "deletespace":
                        pubnub[0].RemoveChannelMetadata()
                            .Channel(obj.SpaceId)
                            .Execute(new PNRemoveChannelMetadataResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "getspace":
                        pubnub[0].GetChannelMetadata()
                            .Channel(obj.SpaceId)
                            .Execute(new PNGetChannelMetadataResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "memberadd":
                        pubnub[0].ManageChannelMembers()
                            .Channel(obj.SpaceId)
                            .Set(new List<PNChannelMember>() { new PNChannelMember { Uuid = obj.UserId } })
                            .Execute(new PNChannelMembersResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "memberremove":
                        pubnub[0].ManageChannelMembers()
                            .Channel(obj.SpaceId)
                            .Remove(new List<string>() { obj.UserId })
                            .Execute(new PNChannelMembersResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "membershipadd":
                        pubnub[0].ManageMemberships()
                            .Uuid(obj.UserId)
                            .Set(new List<PNMembership>() { new PNMembership { Channel = obj.SpaceId } })
                            .Execute(new PNMembershipsResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "membershipremove":
                        pubnub[0].ManageMemberships()
                            .Uuid(obj.UserId)
                            .Remove(new List<string>() { obj.SpaceId })
                            .Execute(new PNMembershipsResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    default:
                        break;
                }
            }
        }

        private void btnObjects2_Click(object sender, EventArgs e)
        {
            frmObjects obj = new frmObjects();
            DialogResult result = obj.ShowDialog();
            if (result == DialogResult.OK)
            {
                switch (obj.ObjectsRequestType.ToLower())
                {
                    case "createuser":
                        pubnub[1].SetUuidMetadata()
                            .Uuid(obj.UserId)
                            .Name(obj.UserName)
                            .Execute(new PNSetUuidMetadataResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "updateuser":
                        pubnub[1].SetUuidMetadata()
                            .Uuid(obj.UserId)
                            .Name(obj.UserName)
                            .Execute(new PNSetUuidMetadataResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "deleteuser":
                        pubnub[1].RemoveUuidMetadata()
                            .Uuid(obj.UserId)
                            .Execute(new PNRemoveUuidMetadataResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "getuser":
                        pubnub[1].GetUuidMetadata()
                            .Uuid(obj.UserId)
                            .Execute(new PNGetUuidMetadataResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "createspace":
                        pubnub[1].SetChannelMetadata()
                            .Channel(obj.SpaceId)
                            .Name(obj.SpaceName)
                            .Execute(new PNSetChannelMetadataResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "updatespace":
                        pubnub[1].SetChannelMetadata()
                            .Channel(obj.SpaceId)
                            .Name(obj.SpaceName)
                            .Execute(new PNSetChannelMetadataResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "deletespace":
                        pubnub[1].RemoveChannelMetadata()
                            .Channel(obj.SpaceId)
                            .Execute(new PNRemoveChannelMetadataResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "getspace":
                        pubnub[1].GetChannelMetadata()
                            .Channel(obj.SpaceId)
                            .Execute(new PNGetChannelMetadataResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "memberadd":
                        pubnub[1].ManageChannelMembers()
                            .Channel(obj.SpaceId)
                            .Set(new List<PNChannelMember>() { new PNChannelMember { Uuid = obj.UserId } })
                            .Execute(new PNChannelMembersResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "memberremove":
                        pubnub[1].ManageChannelMembers()
                            .Channel(obj.SpaceId)
                            .Remove(new List<string>() { obj.UserId })
                            .Execute(new PNChannelMembersResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "membershipadd":
                        pubnub[1].ManageMemberships()
                            .Uuid(obj.UserId)
                            .Set(new List<PNMembership>() { new PNMembership { Channel = obj.SpaceId } })
                            .Execute(new PNMembershipsResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    case "membershipremove":
                        pubnub[1].ManageMemberships()
                            .Uuid(obj.UserId)
                            .Remove(new List<string>() { obj.SpaceId })
                            .Execute(new PNMembershipsResultExt(
                                (r, s) => {
                                    Invoke(new Action(() => {
                                        if (r != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(r));
                                        }
                                        else if (s != null)
                                        {
                                            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(s));
                                        }

                                    }));
                                }));
                        break;
                    default:
                        break;
                }
            }
        }

        private void btnSetToken1_Click(object sender, EventArgs e)
        {
            pubnub[0].SetAuthToken(txtToken1.Text);
            lvResults1.Items.Add(pubnub[0].JsonPluggableLibrary.SerializeToJsonString(pubnub[0].ParseToken(txtToken1.Text)));
        }

        private void btnSetToken2_Click(object sender, EventArgs e)
        {
            pubnub[1].SetAuthToken(txtToken2.Text);
            lvResults2.Items.Add(pubnub[1].JsonPluggableLibrary.SerializeToJsonString(pubnub[1].ParseToken(txtToken2.Text)));
        }
    }
}
