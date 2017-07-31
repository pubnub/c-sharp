namespace PubnubApi.WinFormExample
{
    partial class PubnubDemoForm
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
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtMessage2 = new System.Windows.Forms.TextBox();
            this.txtMessage1 = new System.Windows.Forms.TextBox();
            this.btnSubscribe2 = new System.Windows.Forms.Button();
            this.btnSubscribe1 = new System.Windows.Forms.Button();
            this.lvResults1 = new System.Windows.Forms.ListView();
            this.lvResults2 = new System.Windows.Forms.ListView();
            this.txtChannels1 = new System.Windows.Forms.TextBox();
            this.txtChannels2 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtChannelGroup1 = new System.Windows.Forms.TextBox();
            this.txtChannelGroup2 = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.btnPublish1 = new System.Windows.Forms.Button();
            this.btnPublish2 = new System.Windows.Forms.Button();
            this.btnUnsub1 = new System.Windows.Forms.Button();
            this.btnUnsub2 = new System.Windows.Forms.Button();
            this.btnDisconnect1 = new System.Windows.Forms.Button();
            this.btnReconnect1 = new System.Windows.Forms.Button();
            this.btnReconnect2 = new System.Windows.Forms.Button();
            this.btnDisconnect2 = new System.Windows.Forms.Button();
            this.btnHereNow1 = new System.Windows.Forms.Button();
            this.btnHereNow2 = new System.Windows.Forms.Button();
            this.btnTime1 = new System.Windows.Forms.Button();
            this.btnTime2 = new System.Windows.Forms.Button();
            this.btnWhereNow1 = new System.Windows.Forms.Button();
            this.btnWhereNow2 = new System.Windows.Forms.Button();
            this.btnClearResult1 = new System.Windows.Forms.Button();
            this.btnClearResult2 = new System.Windows.Forms.Button();
            this.btnPAM1 = new System.Windows.Forms.Button();
            this.btnPAM2 = new System.Windows.Forms.Button();
            this.btnPnConfig1 = new System.Windows.Forms.Button();
            this.btnPnConfig2 = new System.Windows.Forms.Button();
            this.btnCG1 = new System.Windows.Forms.Button();
            this.btnCG2 = new System.Windows.Forms.Button();
            this.btnState1 = new System.Windows.Forms.Button();
            this.btnState2 = new System.Windows.Forms.Button();
            this.btnHistory1 = new System.Windows.Forms.Button();
            this.btnHistory2 = new System.Windows.Forms.Button();
            this.txtPush1 = new System.Windows.Forms.Button();
            this.txtPush2 = new System.Windows.Forms.Button();
            this.btnFire1 = new System.Windows.Forms.Button();
            this.btnFire2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(67, -2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Instance 1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(658, -2);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Instance 2";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(599, 100);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Message 2 Pub:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 100);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(84, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Message 2 Pub:";
            // 
            // txtMessage2
            // 
            this.txtMessage2.Location = new System.Drawing.Point(689, 97);
            this.txtMessage2.Name = "txtMessage2";
            this.txtMessage2.Size = new System.Drawing.Size(319, 20);
            this.txtMessage2.TabIndex = 4;
            // 
            // txtMessage1
            // 
            this.txtMessage1.Location = new System.Drawing.Point(94, 97);
            this.txtMessage1.Name = "txtMessage1";
            this.txtMessage1.Size = new System.Drawing.Size(319, 20);
            this.txtMessage1.TabIndex = 5;
            // 
            // btnSubscribe2
            // 
            this.btnSubscribe2.Location = new System.Drawing.Point(523, 126);
            this.btnSubscribe2.Name = "btnSubscribe2";
            this.btnSubscribe2.Size = new System.Drawing.Size(75, 25);
            this.btnSubscribe2.TabIndex = 6;
            this.btnSubscribe2.Text = "Subscribe";
            this.btnSubscribe2.UseVisualStyleBackColor = true;
            this.btnSubscribe2.Click += new System.EventHandler(this.btnSubscribe2_Click);
            // 
            // btnSubscribe1
            // 
            this.btnSubscribe1.Location = new System.Drawing.Point(7, 127);
            this.btnSubscribe1.Name = "btnSubscribe1";
            this.btnSubscribe1.Size = new System.Drawing.Size(75, 25);
            this.btnSubscribe1.TabIndex = 7;
            this.btnSubscribe1.Text = "Subscribe";
            this.btnSubscribe1.UseVisualStyleBackColor = true;
            this.btnSubscribe1.Click += new System.EventHandler(this.btnSubscribe1_Click);
            // 
            // lvResults1
            // 
            this.lvResults1.Location = new System.Drawing.Point(3, 226);
            this.lvResults1.Name = "lvResults1";
            this.lvResults1.Size = new System.Drawing.Size(490, 239);
            this.lvResults1.TabIndex = 8;
            this.lvResults1.UseCompatibleStateImageBehavior = false;
            // 
            // lvResults2
            // 
            this.lvResults2.Location = new System.Drawing.Point(512, 226);
            this.lvResults2.Name = "lvResults2";
            this.lvResults2.Size = new System.Drawing.Size(490, 239);
            this.lvResults2.TabIndex = 9;
            this.lvResults2.UseCompatibleStateImageBehavior = false;
            // 
            // txtChannels1
            // 
            this.txtChannels1.Location = new System.Drawing.Point(94, 43);
            this.txtChannels1.Name = "txtChannels1";
            this.txtChannels1.Size = new System.Drawing.Size(319, 20);
            this.txtChannels1.TabIndex = 13;
            // 
            // txtChannels2
            // 
            this.txtChannels2.Location = new System.Drawing.Point(689, 43);
            this.txtChannels2.Name = "txtChannels2";
            this.txtChannels2.Size = new System.Drawing.Size(319, 20);
            this.txtChannels2.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(4, 46);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Channels";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(599, 46);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(51, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Channels";
            // 
            // txtChannelGroup1
            // 
            this.txtChannelGroup1.Location = new System.Drawing.Point(94, 68);
            this.txtChannelGroup1.Name = "txtChannelGroup1";
            this.txtChannelGroup1.Size = new System.Drawing.Size(319, 20);
            this.txtChannelGroup1.TabIndex = 17;
            // 
            // txtChannelGroup2
            // 
            this.txtChannelGroup2.Location = new System.Drawing.Point(689, 68);
            this.txtChannelGroup2.Name = "txtChannelGroup2";
            this.txtChannelGroup2.Size = new System.Drawing.Size(319, 20);
            this.txtChannelGroup2.TabIndex = 16;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(599, 71);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(80, 13);
            this.label8.TabIndex = 14;
            this.label8.Text = "ChannelGroups";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(4, 71);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(80, 13);
            this.label7.TabIndex = 18;
            this.label7.Text = "ChannelGroups";
            // 
            // btnPublish1
            // 
            this.btnPublish1.Location = new System.Drawing.Point(9, 158);
            this.btnPublish1.Name = "btnPublish1";
            this.btnPublish1.Size = new System.Drawing.Size(75, 25);
            this.btnPublish1.TabIndex = 19;
            this.btnPublish1.Text = "Publish";
            this.btnPublish1.UseVisualStyleBackColor = true;
            this.btnPublish1.Click += new System.EventHandler(this.btnPublish1_Click);
            // 
            // btnPublish2
            // 
            this.btnPublish2.Location = new System.Drawing.Point(523, 157);
            this.btnPublish2.Name = "btnPublish2";
            this.btnPublish2.Size = new System.Drawing.Size(75, 25);
            this.btnPublish2.TabIndex = 20;
            this.btnPublish2.Text = "Publish";
            this.btnPublish2.UseVisualStyleBackColor = true;
            this.btnPublish2.Click += new System.EventHandler(this.btnPublish2_Click);
            // 
            // btnUnsub1
            // 
            this.btnUnsub1.Location = new System.Drawing.Point(89, 127);
            this.btnUnsub1.Name = "btnUnsub1";
            this.btnUnsub1.Size = new System.Drawing.Size(75, 25);
            this.btnUnsub1.TabIndex = 21;
            this.btnUnsub1.Text = "Unsubscribe";
            this.btnUnsub1.UseVisualStyleBackColor = true;
            this.btnUnsub1.Click += new System.EventHandler(this.btnUnsub1_Click);
            // 
            // btnUnsub2
            // 
            this.btnUnsub2.Location = new System.Drawing.Point(602, 127);
            this.btnUnsub2.Name = "btnUnsub2";
            this.btnUnsub2.Size = new System.Drawing.Size(75, 25);
            this.btnUnsub2.TabIndex = 22;
            this.btnUnsub2.Text = "Unsubscribe";
            this.btnUnsub2.UseVisualStyleBackColor = true;
            this.btnUnsub2.Click += new System.EventHandler(this.btnUnsub2_Click);
            // 
            // btnDisconnect1
            // 
            this.btnDisconnect1.Location = new System.Drawing.Point(170, 160);
            this.btnDisconnect1.Name = "btnDisconnect1";
            this.btnDisconnect1.Size = new System.Drawing.Size(75, 23);
            this.btnDisconnect1.TabIndex = 23;
            this.btnDisconnect1.Text = "Disconnect";
            this.btnDisconnect1.UseVisualStyleBackColor = true;
            this.btnDisconnect1.Click += new System.EventHandler(this.btnDisconnect1_Click);
            // 
            // btnReconnect1
            // 
            this.btnReconnect1.Location = new System.Drawing.Point(251, 160);
            this.btnReconnect1.Name = "btnReconnect1";
            this.btnReconnect1.Size = new System.Drawing.Size(75, 23);
            this.btnReconnect1.TabIndex = 24;
            this.btnReconnect1.Text = "Reconnect";
            this.btnReconnect1.UseVisualStyleBackColor = true;
            this.btnReconnect1.Click += new System.EventHandler(this.btnReconnect1_Click);
            // 
            // btnReconnect2
            // 
            this.btnReconnect2.Location = new System.Drawing.Point(765, 160);
            this.btnReconnect2.Name = "btnReconnect2";
            this.btnReconnect2.Size = new System.Drawing.Size(75, 23);
            this.btnReconnect2.TabIndex = 26;
            this.btnReconnect2.Text = "Reconnect";
            this.btnReconnect2.UseVisualStyleBackColor = true;
            this.btnReconnect2.Click += new System.EventHandler(this.btnReconnect2_Click);
            // 
            // btnDisconnect2
            // 
            this.btnDisconnect2.Location = new System.Drawing.Point(684, 160);
            this.btnDisconnect2.Name = "btnDisconnect2";
            this.btnDisconnect2.Size = new System.Drawing.Size(75, 23);
            this.btnDisconnect2.TabIndex = 25;
            this.btnDisconnect2.Text = "Disconnect";
            this.btnDisconnect2.UseVisualStyleBackColor = true;
            this.btnDisconnect2.Click += new System.EventHandler(this.btnDisconnect2_Click);
            // 
            // btnHereNow1
            // 
            this.btnHereNow1.Location = new System.Drawing.Point(251, 131);
            this.btnHereNow1.Name = "btnHereNow1";
            this.btnHereNow1.Size = new System.Drawing.Size(75, 23);
            this.btnHereNow1.TabIndex = 27;
            this.btnHereNow1.Text = "HereNow";
            this.btnHereNow1.UseVisualStyleBackColor = true;
            this.btnHereNow1.Click += new System.EventHandler(this.btnHereNow1_Click);
            // 
            // btnHereNow2
            // 
            this.btnHereNow2.Location = new System.Drawing.Point(765, 128);
            this.btnHereNow2.Name = "btnHereNow2";
            this.btnHereNow2.Size = new System.Drawing.Size(75, 23);
            this.btnHereNow2.TabIndex = 29;
            this.btnHereNow2.Text = "HereNow";
            this.btnHereNow2.UseVisualStyleBackColor = true;
            this.btnHereNow2.Click += new System.EventHandler(this.btnHereNow2_Click);
            // 
            // btnTime1
            // 
            this.btnTime1.Location = new System.Drawing.Point(88, 160);
            this.btnTime1.Name = "btnTime1";
            this.btnTime1.Size = new System.Drawing.Size(75, 23);
            this.btnTime1.TabIndex = 31;
            this.btnTime1.Text = "Time";
            this.btnTime1.UseVisualStyleBackColor = true;
            this.btnTime1.Click += new System.EventHandler(this.btnTime1_Click);
            // 
            // btnTime2
            // 
            this.btnTime2.Location = new System.Drawing.Point(602, 160);
            this.btnTime2.Name = "btnTime2";
            this.btnTime2.Size = new System.Drawing.Size(75, 23);
            this.btnTime2.TabIndex = 32;
            this.btnTime2.Text = "Time";
            this.btnTime2.UseVisualStyleBackColor = true;
            this.btnTime2.Click += new System.EventHandler(this.btnTime2_Click);
            // 
            // btnWhereNow1
            // 
            this.btnWhereNow1.Location = new System.Drawing.Point(333, 131);
            this.btnWhereNow1.Name = "btnWhereNow1";
            this.btnWhereNow1.Size = new System.Drawing.Size(75, 23);
            this.btnWhereNow1.TabIndex = 33;
            this.btnWhereNow1.Text = "WhereNow";
            this.btnWhereNow1.UseVisualStyleBackColor = true;
            this.btnWhereNow1.Click += new System.EventHandler(this.btnWhereNow1_Click);
            // 
            // btnWhereNow2
            // 
            this.btnWhereNow2.Location = new System.Drawing.Point(846, 127);
            this.btnWhereNow2.Name = "btnWhereNow2";
            this.btnWhereNow2.Size = new System.Drawing.Size(75, 23);
            this.btnWhereNow2.TabIndex = 34;
            this.btnWhereNow2.Text = "WhereNow";
            this.btnWhereNow2.UseVisualStyleBackColor = true;
            this.btnWhereNow2.Click += new System.EventHandler(this.btnWhereNow2_Click);
            // 
            // btnClearResult1
            // 
            this.btnClearResult1.Location = new System.Drawing.Point(385, 471);
            this.btnClearResult1.Name = "btnClearResult1";
            this.btnClearResult1.Size = new System.Drawing.Size(108, 24);
            this.btnClearResult1.TabIndex = 39;
            this.btnClearResult1.Text = "Clear Results";
            this.btnClearResult1.UseVisualStyleBackColor = true;
            this.btnClearResult1.Click += new System.EventHandler(this.btnClearResult1_Click);
            // 
            // btnClearResult2
            // 
            this.btnClearResult2.Location = new System.Drawing.Point(894, 471);
            this.btnClearResult2.Name = "btnClearResult2";
            this.btnClearResult2.Size = new System.Drawing.Size(108, 24);
            this.btnClearResult2.TabIndex = 40;
            this.btnClearResult2.Text = "Clear Results";
            this.btnClearResult2.UseVisualStyleBackColor = true;
            this.btnClearResult2.Click += new System.EventHandler(this.btnClearResult2_Click);
            // 
            // btnPAM1
            // 
            this.btnPAM1.Location = new System.Drawing.Point(333, 159);
            this.btnPAM1.Name = "btnPAM1";
            this.btnPAM1.Size = new System.Drawing.Size(75, 23);
            this.btnPAM1.TabIndex = 41;
            this.btnPAM1.Text = "PAM";
            this.btnPAM1.UseVisualStyleBackColor = true;
            this.btnPAM1.Click += new System.EventHandler(this.btnPAM1_Click);
            // 
            // btnPAM2
            // 
            this.btnPAM2.Location = new System.Drawing.Point(846, 160);
            this.btnPAM2.Name = "btnPAM2";
            this.btnPAM2.Size = new System.Drawing.Size(75, 23);
            this.btnPAM2.TabIndex = 42;
            this.btnPAM2.Text = "PAM";
            this.btnPAM2.UseVisualStyleBackColor = true;
            this.btnPAM2.Click += new System.EventHandler(this.btnPAM2_Click);
            // 
            // btnPnConfig1
            // 
            this.btnPnConfig1.Location = new System.Drawing.Point(96, 14);
            this.btnPnConfig1.Name = "btnPnConfig1";
            this.btnPnConfig1.Size = new System.Drawing.Size(75, 23);
            this.btnPnConfig1.TabIndex = 43;
            this.btnPnConfig1.Text = "PN Config";
            this.btnPnConfig1.UseVisualStyleBackColor = true;
            this.btnPnConfig1.Click += new System.EventHandler(this.btnPnConfig1_Click);
            // 
            // btnPnConfig2
            // 
            this.btnPnConfig2.Location = new System.Drawing.Point(691, 14);
            this.btnPnConfig2.Name = "btnPnConfig2";
            this.btnPnConfig2.Size = new System.Drawing.Size(75, 23);
            this.btnPnConfig2.TabIndex = 44;
            this.btnPnConfig2.Text = "PN Config";
            this.btnPnConfig2.UseVisualStyleBackColor = true;
            this.btnPnConfig2.Click += new System.EventHandler(this.btnPnConfig2_Click);
            // 
            // btnCG1
            // 
            this.btnCG1.Location = new System.Drawing.Point(170, 129);
            this.btnCG1.Name = "btnCG1";
            this.btnCG1.Size = new System.Drawing.Size(75, 23);
            this.btnCG1.TabIndex = 45;
            this.btnCG1.Text = "CG";
            this.btnCG1.UseVisualStyleBackColor = true;
            this.btnCG1.Click += new System.EventHandler(this.btnCG1_Click);
            // 
            // btnCG2
            // 
            this.btnCG2.Location = new System.Drawing.Point(683, 128);
            this.btnCG2.Name = "btnCG2";
            this.btnCG2.Size = new System.Drawing.Size(75, 23);
            this.btnCG2.TabIndex = 46;
            this.btnCG2.Text = "CG";
            this.btnCG2.UseVisualStyleBackColor = true;
            this.btnCG2.Click += new System.EventHandler(this.btnCG2_Click);
            // 
            // btnState1
            // 
            this.btnState1.Location = new System.Drawing.Point(414, 131);
            this.btnState1.Name = "btnState1";
            this.btnState1.Size = new System.Drawing.Size(75, 23);
            this.btnState1.TabIndex = 47;
            this.btnState1.Text = "State";
            this.btnState1.UseVisualStyleBackColor = true;
            this.btnState1.Click += new System.EventHandler(this.btnState1_Click);
            // 
            // btnState2
            // 
            this.btnState2.Location = new System.Drawing.Point(927, 127);
            this.btnState2.Name = "btnState2";
            this.btnState2.Size = new System.Drawing.Size(75, 23);
            this.btnState2.TabIndex = 48;
            this.btnState2.Text = "State";
            this.btnState2.UseVisualStyleBackColor = true;
            this.btnState2.Click += new System.EventHandler(this.btnState2_Click);
            // 
            // btnHistory1
            // 
            this.btnHistory1.Location = new System.Drawing.Point(414, 160);
            this.btnHistory1.Name = "btnHistory1";
            this.btnHistory1.Size = new System.Drawing.Size(75, 23);
            this.btnHistory1.TabIndex = 49;
            this.btnHistory1.Text = "History";
            this.btnHistory1.UseVisualStyleBackColor = true;
            this.btnHistory1.Click += new System.EventHandler(this.btnHistory1_Click);
            // 
            // btnHistory2
            // 
            this.btnHistory2.Location = new System.Drawing.Point(927, 158);
            this.btnHistory2.Name = "btnHistory2";
            this.btnHistory2.Size = new System.Drawing.Size(75, 23);
            this.btnHistory2.TabIndex = 50;
            this.btnHistory2.Text = "History";
            this.btnHistory2.UseVisualStyleBackColor = true;
            this.btnHistory2.Click += new System.EventHandler(this.btnHistory2_Click);
            // 
            // txtPush1
            // 
            this.txtPush1.Location = new System.Drawing.Point(9, 189);
            this.txtPush1.Name = "txtPush1";
            this.txtPush1.Size = new System.Drawing.Size(75, 23);
            this.txtPush1.TabIndex = 51;
            this.txtPush1.Text = "Push";
            this.txtPush1.UseVisualStyleBackColor = true;
            this.txtPush1.Click += new System.EventHandler(this.txtPush1_Click);
            // 
            // txtPush2
            // 
            this.txtPush2.Location = new System.Drawing.Point(523, 189);
            this.txtPush2.Name = "txtPush2";
            this.txtPush2.Size = new System.Drawing.Size(75, 23);
            this.txtPush2.TabIndex = 52;
            this.txtPush2.Text = "Push";
            this.txtPush2.UseVisualStyleBackColor = true;
            this.txtPush2.Click += new System.EventHandler(this.txtPush2_Click);
            // 
            // btnFire1
            // 
            this.btnFire1.Location = new System.Drawing.Point(88, 187);
            this.btnFire1.Name = "btnFire1";
            this.btnFire1.Size = new System.Drawing.Size(75, 25);
            this.btnFire1.TabIndex = 53;
            this.btnFire1.Text = "Fire";
            this.btnFire1.UseVisualStyleBackColor = true;
            this.btnFire1.Click += new System.EventHandler(this.btnFire1_Click);
            // 
            // btnFire2
            // 
            this.btnFire2.Location = new System.Drawing.Point(602, 188);
            this.btnFire2.Name = "btnFire2";
            this.btnFire2.Size = new System.Drawing.Size(75, 25);
            this.btnFire2.TabIndex = 54;
            this.btnFire2.Text = "Fire";
            this.btnFire2.UseVisualStyleBackColor = true;
            this.btnFire2.Click += new System.EventHandler(this.btnFire2_Click);
            // 
            // PubnubDemoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1022, 503);
            this.Controls.Add(this.btnFire2);
            this.Controls.Add(this.btnFire1);
            this.Controls.Add(this.txtPush2);
            this.Controls.Add(this.txtPush1);
            this.Controls.Add(this.btnHistory2);
            this.Controls.Add(this.btnHistory1);
            this.Controls.Add(this.btnState2);
            this.Controls.Add(this.btnState1);
            this.Controls.Add(this.btnCG2);
            this.Controls.Add(this.btnCG1);
            this.Controls.Add(this.btnPnConfig2);
            this.Controls.Add(this.btnPnConfig1);
            this.Controls.Add(this.btnPAM2);
            this.Controls.Add(this.btnPAM1);
            this.Controls.Add(this.btnClearResult2);
            this.Controls.Add(this.btnClearResult1);
            this.Controls.Add(this.btnWhereNow2);
            this.Controls.Add(this.btnWhereNow1);
            this.Controls.Add(this.btnTime2);
            this.Controls.Add(this.btnTime1);
            this.Controls.Add(this.btnHereNow2);
            this.Controls.Add(this.btnHereNow1);
            this.Controls.Add(this.btnReconnect2);
            this.Controls.Add(this.btnDisconnect2);
            this.Controls.Add(this.btnReconnect1);
            this.Controls.Add(this.btnDisconnect1);
            this.Controls.Add(this.btnUnsub2);
            this.Controls.Add(this.btnUnsub1);
            this.Controls.Add(this.btnPublish2);
            this.Controls.Add(this.btnPublish1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtChannelGroup1);
            this.Controls.Add(this.txtChannelGroup2);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtChannels1);
            this.Controls.Add(this.txtChannels2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.lvResults2);
            this.Controls.Add(this.lvResults1);
            this.Controls.Add(this.btnSubscribe1);
            this.Controls.Add(this.btnSubscribe2);
            this.Controls.Add(this.txtMessage1);
            this.Controls.Add(this.txtMessage2);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "PubnubDemoForm";
            this.Text = "Multi-Instance Example using same keys";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtMessage2;
        private System.Windows.Forms.TextBox txtMessage1;
        private System.Windows.Forms.Button btnSubscribe2;
        private System.Windows.Forms.Button btnSubscribe1;
        private System.Windows.Forms.ListView lvResults1;
        private System.Windows.Forms.ListView lvResults2;
        private System.Windows.Forms.TextBox txtChannels1;
        private System.Windows.Forms.TextBox txtChannels2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtChannelGroup1;
        private System.Windows.Forms.TextBox txtChannelGroup2;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnPublish1;
        private System.Windows.Forms.Button btnPublish2;
        private System.Windows.Forms.Button btnUnsub1;
        private System.Windows.Forms.Button btnUnsub2;
        private System.Windows.Forms.Button btnDisconnect1;
        private System.Windows.Forms.Button btnReconnect1;
        private System.Windows.Forms.Button btnReconnect2;
        private System.Windows.Forms.Button btnDisconnect2;
        private System.Windows.Forms.Button btnHereNow1;
        private System.Windows.Forms.Button btnHereNow2;
        private System.Windows.Forms.Button btnTime1;
        private System.Windows.Forms.Button btnTime2;
        private System.Windows.Forms.Button btnWhereNow1;
        private System.Windows.Forms.Button btnWhereNow2;
        private System.Windows.Forms.Button btnClearResult1;
        private System.Windows.Forms.Button btnClearResult2;
        private System.Windows.Forms.Button btnPAM1;
        private System.Windows.Forms.Button btnPAM2;
        private System.Windows.Forms.Button btnPnConfig1;
        private System.Windows.Forms.Button btnPnConfig2;
        private System.Windows.Forms.Button btnCG1;
        private System.Windows.Forms.Button btnCG2;
        private System.Windows.Forms.Button btnState1;
        private System.Windows.Forms.Button btnState2;
        private System.Windows.Forms.Button btnHistory1;
        private System.Windows.Forms.Button btnHistory2;
        private System.Windows.Forms.Button txtPush1;
        private System.Windows.Forms.Button txtPush2;
        private System.Windows.Forms.Button btnFire1;
        private System.Windows.Forms.Button btnFire2;
    }
}