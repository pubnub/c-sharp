namespace PubNubMessagingExample
{
    partial class PubnubDemo
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MainMenu menuPAM;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuPAM = new System.Windows.Forms.MainMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItemRevoke = new System.Windows.Forms.MenuItem();
            this.menuItemAudit = new System.Windows.Forms.MenuItem();
            this.menuItemGrant = new System.Windows.Forms.MenuItem();
            this.menuItemSettings = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuItemNetworkDisconnect = new System.Windows.Forms.MenuItem();
            this.menuItemNetworkConnect = new System.Windows.Forms.MenuItem();
            this.menuItemDisconAutoReconnect = new System.Windows.Forms.MenuItem();
            this.menuItemChannelGroup = new System.Windows.Forms.MenuItem();
            this.menuItemCGAddRemoveChannel = new System.Windows.Forms.MenuItem();
            this.menuItemChangeAuthKey = new System.Windows.Forms.MenuItem();
            this.menuItemChangeUUID = new System.Windows.Forms.MenuItem();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.btnTime = new System.Windows.Forms.Button();
            this.lbResult = new System.Windows.Forms.ListBox();
            this.lblChannel = new System.Windows.Forms.Label();
            this.txtChannel = new System.Windows.Forms.TextBox();
            this.txtChannelGroup = new System.Windows.Forms.TextBox();
            this.lblChannelGroup = new System.Windows.Forms.Label();
            this.btnPublish = new System.Windows.Forms.Button();
            this.btnPresence = new System.Windows.Forms.Button();
            this.btnSubscribe = new System.Windows.Forms.Button();
            this.btnHereNow = new System.Windows.Forms.Button();
            this.btnDetailedHistory = new System.Windows.Forms.Button();
            this.btnPresenceUnsub = new System.Windows.Forms.Button();
            this.btnUnsubscribe = new System.Windows.Forms.Button();
            this.btnWhereNow = new System.Windows.Forms.Button();
            this.btnGlobalHereNow = new System.Windows.Forms.Button();
            this.btnUserState = new System.Windows.Forms.Button();
            this.btnPush = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // menuPAM
            // 
            this.menuPAM.MenuItems.Add(this.menuItem1);
            this.menuPAM.MenuItems.Add(this.menuItemSettings);
            this.menuPAM.MenuItems.Add(this.menuItem2);
            this.menuPAM.MenuItems.Add(this.menuItemChannelGroup);
            this.menuPAM.MenuItems.Add(this.menuItem4);
            // 
            // menuItem1
            // 
            this.menuItem1.MenuItems.Add(this.menuItemRevoke);
            this.menuItem1.MenuItems.Add(this.menuItemAudit);
            this.menuItem1.MenuItems.Add(this.menuItemGrant);
            this.menuItem1.Text = "PAM";
            // 
            // menuItemRevoke
            // 
            this.menuItemRevoke.Text = "Revoke";
            this.menuItemRevoke.Click += new System.EventHandler(this.menuItemRevoke_Click);
            // 
            // menuItemAudit
            // 
            this.menuItemAudit.Text = "Audit";
            this.menuItemAudit.Click += new System.EventHandler(this.menuItemAudit_Click);
            // 
            // menuItemGrant
            // 
            this.menuItemGrant.Text = "Grant";
            this.menuItemGrant.Click += new System.EventHandler(this.menuItemGrant_Click);
            // 
            // menuItemSettings
            // 
            this.menuItemSettings.MenuItems.Add(this.menuItem3);
            this.menuItemSettings.Text = "Settings";
            this.menuItemSettings.Click += new System.EventHandler(this.menuItemSettings_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Text = "Reinitialize";
            // 
            // menuItem2
            // 
            this.menuItem2.MenuItems.Add(this.menuItemNetworkDisconnect);
            this.menuItem2.MenuItems.Add(this.menuItemNetworkConnect);
            this.menuItem2.MenuItems.Add(this.menuItemDisconAutoReconnect);
            this.menuItem2.Text = "Network";
            // 
            // menuItemNetworkDisconnect
            // 
            this.menuItemNetworkDisconnect.Text = "Disconnect";
            this.menuItemNetworkDisconnect.Click += new System.EventHandler(this.menuItemNetworkDisconnect_Click);
            // 
            // menuItemNetworkConnect
            // 
            this.menuItemNetworkConnect.Checked = true;
            this.menuItemNetworkConnect.Text = "Connect";
            this.menuItemNetworkConnect.Click += new System.EventHandler(this.menuItemNetworkConnect_Click);
            // 
            // menuItemDisconAutoReconnect
            // 
            this.menuItemDisconAutoReconnect.Text = "Disconnect & Auto Reconnect";
            this.menuItemDisconAutoReconnect.Click += new System.EventHandler(this.menuItemDisconAutoReconnect_Click);
            // 
            // menuItemChannelGroup
            // 
            this.menuItemChannelGroup.MenuItems.Add(this.menuItemCGAddRemoveChannel);
            this.menuItemChannelGroup.MenuItems.Add(this.menuItemChangeAuthKey);
            this.menuItemChannelGroup.MenuItems.Add(this.menuItemChangeUUID);
            this.menuItemChannelGroup.Text = "Others";
            // 
            // menuItemCGAddRemoveChannel
            // 
            this.menuItemCGAddRemoveChannel.Text = "Add/Remove/Get Channel from Channel \\Group";
            this.menuItemCGAddRemoveChannel.Click += new System.EventHandler(this.menuItemCGAddRemoveChannel_Click);
            // 
            // menuItemChangeAuthKey
            // 
            this.menuItemChangeAuthKey.Text = "Change AuthKey";
            this.menuItemChangeAuthKey.Click += new System.EventHandler(this.menuItemChangeAuthKey_Click);
            // 
            // menuItemChangeUUID
            // 
            this.menuItemChangeUUID.Text = "Change UUID";
            this.menuItemChangeUUID.Click += new System.EventHandler(this.menuItemChangeUUID_Click);
            // 
            // menuItem4
            // 
            this.menuItem4.Text = "Close";
            this.menuItem4.Click += new System.EventHandler(this.OnClickClose);
            // 
            // btnTime
            // 
            this.btnTime.Location = new System.Drawing.Point(163, 104);
            this.btnTime.Name = "btnTime";
            this.btnTime.Size = new System.Drawing.Size(62, 20);
            this.btnTime.TabIndex = 6;
            this.btnTime.Text = "Time";
            this.btnTime.Click += new System.EventHandler(this.btnTime_Click);
            // 
            // lbResult
            // 
            this.lbResult.Location = new System.Drawing.Point(12, 165);
            this.lbResult.Name = "lbResult";
            this.lbResult.Size = new System.Drawing.Size(213, 86);
            this.lbResult.TabIndex = 7;
            // 
            // lblChannel
            // 
            this.lblChannel.Location = new System.Drawing.Point(12, 2);
            this.lblChannel.Name = "lblChannel";
            this.lblChannel.Size = new System.Drawing.Size(50, 20);
            this.lblChannel.Text = "Channel";
            // 
            // txtChannel
            // 
            this.txtChannel.Location = new System.Drawing.Point(59, 1);
            this.txtChannel.Name = "txtChannel";
            this.txtChannel.Size = new System.Drawing.Size(166, 21);
            this.txtChannel.TabIndex = 1;
            this.txtChannel.Text = "a";
            // 
            // txtChannelGroup
            // 
            this.txtChannelGroup.Location = new System.Drawing.Point(91, 25);
            this.txtChannelGroup.Name = "txtChannelGroup";
            this.txtChannelGroup.Size = new System.Drawing.Size(134, 21);
            this.txtChannelGroup.TabIndex = 2;
            // 
            // lblChannelGroup
            // 
            this.lblChannelGroup.Location = new System.Drawing.Point(12, 29);
            this.lblChannelGroup.Name = "lblChannelGroup";
            this.lblChannelGroup.Size = new System.Drawing.Size(82, 20);
            this.lblChannelGroup.Text = "ChannelGroup";
            // 
            // btnPublish
            // 
            this.btnPublish.Location = new System.Drawing.Point(163, 52);
            this.btnPublish.Name = "btnPublish";
            this.btnPublish.Size = new System.Drawing.Size(62, 20);
            this.btnPublish.TabIndex = 5;
            this.btnPublish.Text = "Publish";
            this.btnPublish.Click += new System.EventHandler(this.btnPublish_Click);
            // 
            // btnPresence
            // 
            this.btnPresence.Location = new System.Drawing.Point(12, 52);
            this.btnPresence.Name = "btnPresence";
            this.btnPresence.Size = new System.Drawing.Size(64, 20);
            this.btnPresence.TabIndex = 3;
            this.btnPresence.Text = "Presence";
            this.btnPresence.Click += new System.EventHandler(this.btnPresence_Click);
            // 
            // btnSubscribe
            // 
            this.btnSubscribe.Location = new System.Drawing.Point(83, 53);
            this.btnSubscribe.Name = "btnSubscribe";
            this.btnSubscribe.Size = new System.Drawing.Size(74, 20);
            this.btnSubscribe.TabIndex = 4;
            this.btnSubscribe.Text = "Sub";
            this.btnSubscribe.Click += new System.EventHandler(this.btnSubscribe_Click);
            // 
            // btnHereNow
            // 
            this.btnHereNow.Location = new System.Drawing.Point(12, 105);
            this.btnHereNow.Name = "btnHereNow";
            this.btnHereNow.Size = new System.Drawing.Size(64, 20);
            this.btnHereNow.TabIndex = 8;
            this.btnHereNow.Text = "HereNow";
            this.btnHereNow.Click += new System.EventHandler(this.btnHereNow_Click);
            // 
            // btnDetailedHistory
            // 
            this.btnDetailedHistory.Location = new System.Drawing.Point(163, 78);
            this.btnDetailedHistory.Name = "btnDetailedHistory";
            this.btnDetailedHistory.Size = new System.Drawing.Size(62, 20);
            this.btnDetailedHistory.TabIndex = 9;
            this.btnDetailedHistory.Text = "History";
            this.btnDetailedHistory.Click += new System.EventHandler(this.btnDetailedHistory_Click);
            // 
            // btnPresenceUnsub
            // 
            this.btnPresenceUnsub.Location = new System.Drawing.Point(12, 78);
            this.btnPresenceUnsub.Name = "btnPresenceUnsub";
            this.btnPresenceUnsub.Size = new System.Drawing.Size(64, 20);
            this.btnPresenceUnsub.TabIndex = 10;
            this.btnPresenceUnsub.Tag = "Presence Unsubscribe";
            this.btnPresenceUnsub.Text = "P-Unsub";
            this.btnPresenceUnsub.Click += new System.EventHandler(this.btnPresenceUnsub_Click);
            // 
            // btnUnsubscribe
            // 
            this.btnUnsubscribe.Location = new System.Drawing.Point(83, 79);
            this.btnUnsubscribe.Name = "btnUnsubscribe";
            this.btnUnsubscribe.Size = new System.Drawing.Size(74, 20);
            this.btnUnsubscribe.TabIndex = 11;
            this.btnUnsubscribe.Text = "Unsub";
            this.btnUnsubscribe.Click += new System.EventHandler(this.btnUnsubscribe_Click);
            // 
            // btnWhereNow
            // 
            this.btnWhereNow.Location = new System.Drawing.Point(83, 105);
            this.btnWhereNow.Name = "btnWhereNow";
            this.btnWhereNow.Size = new System.Drawing.Size(74, 20);
            this.btnWhereNow.TabIndex = 14;
            this.btnWhereNow.Text = "WhereNow";
            this.btnWhereNow.Click += new System.EventHandler(this.btnWhereNow_Click);
            // 
            // btnGlobalHereNow
            // 
            this.btnGlobalHereNow.Location = new System.Drawing.Point(83, 132);
            this.btnGlobalHereNow.Name = "btnGlobalHereNow";
            this.btnGlobalHereNow.Size = new System.Drawing.Size(74, 20);
            this.btnGlobalHereNow.TabIndex = 15;
            this.btnGlobalHereNow.Text = "GHereNow";
            this.btnGlobalHereNow.Click += new System.EventHandler(this.btnGlobalHereNow_Click);
            // 
            // btnUserState
            // 
            this.btnUserState.Location = new System.Drawing.Point(12, 131);
            this.btnUserState.Name = "btnUserState";
            this.btnUserState.Size = new System.Drawing.Size(64, 20);
            this.btnUserState.TabIndex = 16;
            this.btnUserState.Text = "State";
            this.btnUserState.Click += new System.EventHandler(this.btnUserState_Click);
            // 
            // btnPush
            // 
            this.btnPush.Location = new System.Drawing.Point(164, 130);
            this.btnPush.Name = "btnPush";
            this.btnPush.Size = new System.Drawing.Size(61, 20);
            this.btnPush.TabIndex = 19;
            this.btnPush.Text = "Push";
            this.btnPush.Click += new System.EventHandler(this.btnPush_Click);
            // 
            // PubnubDemo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.ControlBox = false;
            this.Controls.Add(this.btnPush);
            this.Controls.Add(this.btnUserState);
            this.Controls.Add(this.btnGlobalHereNow);
            this.Controls.Add(this.btnWhereNow);
            this.Controls.Add(this.btnUnsubscribe);
            this.Controls.Add(this.btnPresenceUnsub);
            this.Controls.Add(this.btnDetailedHistory);
            this.Controls.Add(this.btnHereNow);
            this.Controls.Add(this.btnSubscribe);
            this.Controls.Add(this.btnPresence);
            this.Controls.Add(this.btnPublish);
            this.Controls.Add(this.txtChannelGroup);
            this.Controls.Add(this.lblChannelGroup);
            this.Controls.Add(this.txtChannel);
            this.Controls.Add(this.lblChannel);
            this.Controls.Add(this.lbResult);
            this.Controls.Add(this.btnTime);
            this.Menu = this.menuPAM;
            this.MinimizeBox = false;
            this.Name = "PubnubDemo";
            this.Text = "PubNub Demo";
            this.Closed += new System.EventHandler(this.OnFormClosed);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnTime;
        private System.Windows.Forms.ListBox lbResult;
        private System.Windows.Forms.Label lblChannel;
        private System.Windows.Forms.TextBox txtChannel;
        private System.Windows.Forms.TextBox txtChannelGroup;
        private System.Windows.Forms.Label lblChannelGroup;
        private System.Windows.Forms.Button btnPublish;
        private System.Windows.Forms.Button btnPresence;
        private System.Windows.Forms.Button btnSubscribe;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItemRevoke;
        private System.Windows.Forms.MenuItem menuItemAudit;
        private System.Windows.Forms.MenuItem menuItemGrant;
        private System.Windows.Forms.Button btnHereNow;
        private System.Windows.Forms.Button btnDetailedHistory;
        private System.Windows.Forms.Button btnPresenceUnsub;
        private System.Windows.Forms.Button btnUnsubscribe;
        private System.Windows.Forms.MenuItem menuItemSettings;
        private System.Windows.Forms.Button btnWhereNow;
        private System.Windows.Forms.Button btnGlobalHereNow;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem menuItemNetworkDisconnect;
        private System.Windows.Forms.MenuItem menuItemNetworkConnect;
        private System.Windows.Forms.Button btnUserState;
        private System.Windows.Forms.MenuItem menuItemDisconAutoReconnect;
        private System.Windows.Forms.MenuItem menuItem3;
        private System.Windows.Forms.Button btnPush;
        private System.Windows.Forms.MenuItem menuItemChannelGroup;
        private System.Windows.Forms.MenuItem menuItemCGAddRemoveChannel;
        private System.Windows.Forms.MenuItem menuItemChangeAuthKey;
        private System.Windows.Forms.MenuItem menuItemChangeUUID;
        private System.Windows.Forms.MenuItem menuItem4;
    }
}

