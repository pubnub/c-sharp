namespace PubNubMessagingExample
{
    partial class PubnubChannelGroup
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtChannel = new System.Windows.Forms.TextBox();
            this.radBtnAddChannel = new System.Windows.Forms.RadioButton();
            this.txtChannelGroup = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.radBtnRemoveChannel = new System.Windows.Forms.RadioButton();
            this.radBtnGetChannelList = new System.Windows.Forms.RadioButton();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 56);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 20);
            this.label1.Text = "Channel";
            // 
            // txtChannel
            // 
            this.txtChannel.Location = new System.Drawing.Point(73, 55);
            this.txtChannel.Name = "txtChannel";
            this.txtChannel.Size = new System.Drawing.Size(153, 21);
            this.txtChannel.TabIndex = 1;
            // 
            // radBtnAddChannel
            // 
            this.radBtnAddChannel.Location = new System.Drawing.Point(12, 91);
            this.radBtnAddChannel.Name = "radBtnAddChannel";
            this.radBtnAddChannel.Size = new System.Drawing.Size(214, 20);
            this.radBtnAddChannel.TabIndex = 2;
            this.radBtnAddChannel.Text = "Add Channel";
            // 
            // txtChannelGroup
            // 
            this.txtChannelGroup.Location = new System.Drawing.Point(108, 19);
            this.txtChannelGroup.Name = "txtChannelGroup";
            this.txtChannelGroup.Size = new System.Drawing.Size(118, 21);
            this.txtChannelGroup.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(12, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 20);
            this.label2.Text = "ChannelGroup";
            // 
            // radBtnRemoveChannel
            // 
            this.radBtnRemoveChannel.Location = new System.Drawing.Point(13, 124);
            this.radBtnRemoveChannel.Name = "radBtnRemoveChannel";
            this.radBtnRemoveChannel.Size = new System.Drawing.Size(214, 20);
            this.radBtnRemoveChannel.TabIndex = 6;
            this.radBtnRemoveChannel.Text = "Remove Channel";
            // 
            // radBtnGetChannelList
            // 
            this.radBtnGetChannelList.Checked = true;
            this.radBtnGetChannelList.Location = new System.Drawing.Point(12, 160);
            this.radBtnGetChannelList.Name = "radBtnGetChannelList";
            this.radBtnGetChannelList.Size = new System.Drawing.Size(214, 20);
            this.radBtnGetChannelList.TabIndex = 7;
            this.radBtnGetChannelList.Text = "Get Channel List";
            // 
            // btnSubmit
            // 
            this.btnSubmit.Location = new System.Drawing.Point(13, 198);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(72, 20);
            this.btnSubmit.TabIndex = 8;
            this.btnSubmit.Text = "Submit";
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(123, 198);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(72, 20);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // PubnubChannelGroup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSubmit);
            this.Controls.Add(this.radBtnGetChannelList);
            this.Controls.Add(this.radBtnRemoveChannel);
            this.Controls.Add(this.txtChannelGroup);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.radBtnAddChannel);
            this.Controls.Add(this.txtChannel);
            this.Controls.Add(this.label1);
            this.Menu = this.mainMenu1;
            this.Name = "PubnubChannelGroup";
            this.Text = "Channel Group";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtChannel;
        private System.Windows.Forms.RadioButton radBtnAddChannel;
        private System.Windows.Forms.TextBox txtChannelGroup;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton radBtnRemoveChannel;
        private System.Windows.Forms.RadioButton radBtnGetChannelList;
        private System.Windows.Forms.Button btnSubmit;
        private System.Windows.Forms.Button btnCancel;
    }
}