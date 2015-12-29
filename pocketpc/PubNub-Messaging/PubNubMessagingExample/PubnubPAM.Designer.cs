namespace PubNubMessagingExample
{
    partial class PubnubPAM
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MainMenu mainMenu1;

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
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.radioButtonChannel = new System.Windows.Forms.RadioButton();
            this.radioButtonChannelGroup = new System.Windows.Forms.RadioButton();
            this.txtChannel = new System.Windows.Forms.TextBox();
            this.txtChannelGroup = new System.Windows.Forms.TextBox();
            this.txtPAM = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.chBoxIsPresence = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtAuthKey = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // radioButtonChannel
            // 
            this.radioButtonChannel.Location = new System.Drawing.Point(14, 4);
            this.radioButtonChannel.Name = "radioButtonChannel";
            this.radioButtonChannel.Size = new System.Drawing.Size(80, 20);
            this.radioButtonChannel.TabIndex = 0;
            this.radioButtonChannel.Text = "Channel";
            this.radioButtonChannel.CheckedChanged += new System.EventHandler(this.radioButtonChannel_CheckedChanged);
            // 
            // radioButtonChannelGroup
            // 
            this.radioButtonChannelGroup.Location = new System.Drawing.Point(14, 52);
            this.radioButtonChannelGroup.Name = "radioButtonChannelGroup";
            this.radioButtonChannelGroup.Size = new System.Drawing.Size(178, 20);
            this.radioButtonChannelGroup.TabIndex = 1;
            this.radioButtonChannelGroup.Text = "Channel Group";
            this.radioButtonChannelGroup.CheckedChanged += new System.EventHandler(this.radioButtonChannelGroup_CheckedChanged);
            // 
            // txtChannel
            // 
            this.txtChannel.Location = new System.Drawing.Point(36, 31);
            this.txtChannel.Name = "txtChannel";
            this.txtChannel.Size = new System.Drawing.Size(180, 21);
            this.txtChannel.TabIndex = 2;
            // 
            // txtChannelGroup
            // 
            this.txtChannelGroup.Location = new System.Drawing.Point(36, 79);
            this.txtChannelGroup.Name = "txtChannelGroup";
            this.txtChannelGroup.Size = new System.Drawing.Size(180, 21);
            this.txtChannelGroup.TabIndex = 3;
            // 
            // txtPAM
            // 
            this.txtPAM.Location = new System.Drawing.Point(43, 197);
            this.txtPAM.Name = "txtPAM";
            this.txtPAM.Size = new System.Drawing.Size(72, 20);
            this.txtPAM.TabIndex = 4;
            this.txtPAM.Text = "PAM";
            this.txtPAM.Click += new System.EventHandler(this.txtPAM_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(144, 196);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(72, 20);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // chBoxIsPresence
            // 
            this.chBoxIsPresence.Location = new System.Drawing.Point(3, 122);
            this.chBoxIsPresence.Name = "chBoxIsPresence";
            this.chBoxIsPresence.Size = new System.Drawing.Size(223, 20);
            this.chBoxIsPresence.TabIndex = 6;
            this.chBoxIsPresence.Text = "Presence Channel/ChannelGroup?";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(14, 158);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 20);
            this.label1.Text = "Auth Key\'";
            // 
            // txtAuthKey
            // 
            this.txtAuthKey.Location = new System.Drawing.Point(79, 158);
            this.txtAuthKey.Name = "txtAuthKey";
            this.txtAuthKey.Size = new System.Drawing.Size(137, 21);
            this.txtAuthKey.TabIndex = 8;
            // 
            // PubnubPAM
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.txtAuthKey);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chBoxIsPresence);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.txtPAM);
            this.Controls.Add(this.txtChannelGroup);
            this.Controls.Add(this.txtChannel);
            this.Controls.Add(this.radioButtonChannelGroup);
            this.Controls.Add(this.radioButtonChannel);
            this.Menu = this.mainMenu1;
            this.Name = "PubnubPAM";
            this.Text = "PAM";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton radioButtonChannel;
        private System.Windows.Forms.RadioButton radioButtonChannelGroup;
        private System.Windows.Forms.TextBox txtChannel;
        private System.Windows.Forms.TextBox txtChannelGroup;
        private System.Windows.Forms.Button txtPAM;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox chBoxIsPresence;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtAuthKey;
    }
}