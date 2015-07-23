namespace PubNubMessagingExample
{
    partial class PubnubPush
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
            this.label2 = new System.Windows.Forms.Label();
            this.txtToken = new System.Windows.Forms.TextBox();
            this.radBtnRegister = new System.Windows.Forms.RadioButton();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.radBtnUnregister = new System.Windows.Forms.RadioButton();
            this.radBtnRemoveChannel = new System.Windows.Forms.RadioButton();
            this.radBtnGetChannels = new System.Windows.Forms.RadioButton();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(16, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 20);
            this.label1.Text = "Channel";
            // 
            // txtChannel
            // 
            this.txtChannel.Location = new System.Drawing.Point(86, 4);
            this.txtChannel.Name = "txtChannel";
            this.txtChannel.Size = new System.Drawing.Size(130, 21);
            this.txtChannel.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(16, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 20);
            this.label2.Text = "Token";
            // 
            // txtToken
            // 
            this.txtToken.Location = new System.Drawing.Point(86, 35);
            this.txtToken.Name = "txtToken";
            this.txtToken.Size = new System.Drawing.Size(130, 21);
            this.txtToken.TabIndex = 3;
            // 
            // radBtnRegister
            // 
            this.radBtnRegister.Location = new System.Drawing.Point(16, 72);
            this.radBtnRegister.Name = "radBtnRegister";
            this.radBtnRegister.Size = new System.Drawing.Size(200, 20);
            this.radBtnRegister.TabIndex = 4;
            this.radBtnRegister.Text = "Register Device";
            // 
            // btnSubmit
            // 
            this.btnSubmit.Location = new System.Drawing.Point(16, 194);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(72, 20);
            this.btnSubmit.TabIndex = 5;
            this.btnSubmit.Text = "Submit";
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // radBtnUnregister
            // 
            this.radBtnUnregister.Location = new System.Drawing.Point(16, 99);
            this.radBtnUnregister.Name = "radBtnUnregister";
            this.radBtnUnregister.Size = new System.Drawing.Size(200, 20);
            this.radBtnUnregister.TabIndex = 6;
            this.radBtnUnregister.Text = "Unregister Device";
            // 
            // radBtnRemoveChannel
            // 
            this.radBtnRemoveChannel.Location = new System.Drawing.Point(16, 126);
            this.radBtnRemoveChannel.Name = "radBtnRemoveChannel";
            this.radBtnRemoveChannel.Size = new System.Drawing.Size(200, 20);
            this.radBtnRemoveChannel.TabIndex = 7;
            this.radBtnRemoveChannel.Text = "Remove Channel";
            // 
            // radBtnGetChannels
            // 
            this.radBtnGetChannels.Checked = true;
            this.radBtnGetChannels.Location = new System.Drawing.Point(16, 153);
            this.radBtnGetChannels.Name = "radBtnGetChannels";
            this.radBtnGetChannels.Size = new System.Drawing.Size(200, 20);
            this.radBtnGetChannels.TabIndex = 8;
            this.radBtnGetChannels.Text = "Get Current Channels";
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(108, 194);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(72, 20);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // PubnubPush
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.radBtnGetChannels);
            this.Controls.Add(this.radBtnRemoveChannel);
            this.Controls.Add(this.radBtnUnregister);
            this.Controls.Add(this.btnSubmit);
            this.Controls.Add(this.radBtnRegister);
            this.Controls.Add(this.txtToken);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtChannel);
            this.Controls.Add(this.label1);
            this.Menu = this.mainMenu1;
            this.Name = "PubnubPush";
            this.Text = "MPNS Push";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtChannel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtToken;
        private System.Windows.Forms.RadioButton radBtnRegister;
        private System.Windows.Forms.Button btnSubmit;
        private System.Windows.Forms.RadioButton radBtnUnregister;
        private System.Windows.Forms.RadioButton radBtnRemoveChannel;
        private System.Windows.Forms.RadioButton radBtnGetChannels;
        private System.Windows.Forms.Button btnCancel;
    }
}