namespace PubNubMessagingExample
{
    partial class PubnubInitForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.label1 = new System.Windows.Forms.Label();
            this.txtOrigin = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPublishKey = new System.Windows.Forms.TextBox();
            this.txtSubscribeKey = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtSecretKey = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtCipherKey = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.chkEnableSSL = new System.Windows.Forms.CheckBox();
            this.btnNext = new System.Windows.Forms.Button();
            this.txtSessionUUID = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.chkReconnect = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtPresenceHeartbeat = new System.Windows.Forms.TextBox();
            this.txtPresenceHeartbeatInterval = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 20);
            this.label1.Text = "Origin";
            // 
            // txtOrigin
            // 
            this.txtOrigin.Location = new System.Drawing.Point(98, 0);
            this.txtOrigin.MaxLength = 2048;
            this.txtOrigin.Name = "txtOrigin";
            this.txtOrigin.Size = new System.Drawing.Size(139, 25);
            this.txtOrigin.TabIndex = 1;
            this.txtOrigin.Text = "pubsub.pubnub.com";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(3, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 20);
            this.label2.Text = "Publish Key";
            // 
            // txtPublishKey
            // 
            this.txtPublishKey.Location = new System.Drawing.Point(98, 27);
            this.txtPublishKey.MaxLength = 64;
            this.txtPublishKey.Name = "txtPublishKey";
            this.txtPublishKey.Size = new System.Drawing.Size(139, 25);
            this.txtPublishKey.TabIndex = 2;
            this.txtPublishKey.Text = "demo";
            // 
            // txtSubscribeKey
            // 
            this.txtSubscribeKey.Location = new System.Drawing.Point(98, 54);
            this.txtSubscribeKey.MaxLength = 64;
            this.txtSubscribeKey.Name = "txtSubscribeKey";
            this.txtSubscribeKey.Size = new System.Drawing.Size(139, 25);
            this.txtSubscribeKey.TabIndex = 3;
            this.txtSubscribeKey.Text = "demo";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(0, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 20);
            this.label3.Text = "Subscribe Key";
            // 
            // txtSecretKey
            // 
            this.txtSecretKey.Location = new System.Drawing.Point(98, 81);
            this.txtSecretKey.MaxLength = 64;
            this.txtSecretKey.Name = "txtSecretKey";
            this.txtSecretKey.Size = new System.Drawing.Size(139, 25);
            this.txtSecretKey.TabIndex = 4;
            this.txtSecretKey.Text = "demo";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(0, 85);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 20);
            this.label4.Text = "Secret Key";
            // 
            // txtCipherKey
            // 
            this.txtCipherKey.Location = new System.Drawing.Point(98, 108);
            this.txtCipherKey.MaxLength = 64;
            this.txtCipherKey.Name = "txtCipherKey";
            this.txtCipherKey.Size = new System.Drawing.Size(139, 25);
            this.txtCipherKey.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(0, 112);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(100, 20);
            this.label5.Text = "Cipher Key";
            // 
            // chkEnableSSL
            // 
            this.chkEnableSSL.Location = new System.Drawing.Point(3, 213);
            this.chkEnableSSL.Name = "chkEnableSSL";
            this.chkEnableSSL.Size = new System.Drawing.Size(100, 20);
            this.chkEnableSSL.TabIndex = 7;
            this.chkEnableSSL.Text = "Enable SSL";
            // 
            // btnNext
            // 
            this.btnNext.Location = new System.Drawing.Point(162, 271);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(72, 20);
            this.btnNext.TabIndex = 9;
            this.btnNext.Text = "Next";
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // txtSessionUUID
            // 
            this.txtSessionUUID.Location = new System.Drawing.Point(98, 135);
            this.txtSessionUUID.MaxLength = 36;
            this.txtSessionUUID.Name = "txtSessionUUID";
            this.txtSessionUUID.Size = new System.Drawing.Size(139, 25);
            this.txtSessionUUID.TabIndex = 6;
            this.txtSessionUUID.Text = "netcfuuid";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(0, 139);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(100, 20);
            this.label6.Text = "SessionUUID";
            // 
            // chkReconnect
            // 
            this.chkReconnect.Checked = true;
            this.chkReconnect.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkReconnect.Location = new System.Drawing.Point(3, 239);
            this.chkReconnect.Name = "chkReconnect";
            this.chkReconnect.Size = new System.Drawing.Size(234, 20);
            this.chkReconnect.TabIndex = 8;
            this.chkReconnect.Text = "Enable Resume On Reconnect";
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(3, 163);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(120, 20);
            this.label7.Text = "Presence HB (in sec)";
            // 
            // txtPresenceHeartbeat
            // 
            this.txtPresenceHeartbeat.Location = new System.Drawing.Point(137, 162);
            this.txtPresenceHeartbeat.MaxLength = 13;
            this.txtPresenceHeartbeat.Name = "txtPresenceHeartbeat";
            this.txtPresenceHeartbeat.Size = new System.Drawing.Size(100, 25);
            this.txtPresenceHeartbeat.TabIndex = 16;
            this.txtPresenceHeartbeat.Text = "0";
            this.txtPresenceHeartbeat.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPresenceHeartbeat_OnKeyPress);
            // 
            // txtPresenceHeartbeatInterval
            // 
            this.txtPresenceHeartbeatInterval.Location = new System.Drawing.Point(178, 189);
            this.txtPresenceHeartbeatInterval.MaxLength = 13;
            this.txtPresenceHeartbeatInterval.Name = "txtPresenceHeartbeatInterval";
            this.txtPresenceHeartbeatInterval.Size = new System.Drawing.Size(59, 25);
            this.txtPresenceHeartbeatInterval.TabIndex = 18;
            this.txtPresenceHeartbeatInterval.Text = "0";
            this.txtPresenceHeartbeatInterval.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPresenceHeartbeatInterval_KeyPressed);
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(3, 190);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(169, 20);
            this.label8.Text = "Presence HB Interval (in sec)";
            // 
            // PubnubInitForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 294);
            this.Controls.Add(this.txtPresenceHeartbeatInterval);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtPresenceHeartbeat);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.chkReconnect);
            this.Controls.Add(this.txtSessionUUID);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.chkEnableSSL);
            this.Controls.Add(this.txtCipherKey);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtSecretKey);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtSubscribeKey);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtPublishKey);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtOrigin);
            this.Controls.Add(this.label1);
            this.Name = "PubnubInitForm";
            this.Text = "Initialize";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtOrigin;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPublishKey;
        private System.Windows.Forms.TextBox txtSubscribeKey;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtSecretKey;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtCipherKey;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox chkEnableSSL;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.TextBox txtSessionUUID;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkReconnect;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtPresenceHeartbeat;
        private System.Windows.Forms.TextBox txtPresenceHeartbeatInterval;
        private System.Windows.Forms.Label label8;
    }
}