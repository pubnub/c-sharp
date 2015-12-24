namespace PubNubMessagingExample
{
    partial class PubnubUserState
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
            this.chBoxChannel = new System.Windows.Forms.CheckBox();
            this.chBoxChannelGroup = new System.Windows.Forms.CheckBox();
            this.txtChannel = new System.Windows.Forms.TextBox();
            this.txtChannelGroup = new System.Windows.Forms.TextBox();
            this.radBtnSetUserState = new System.Windows.Forms.RadioButton();
            this.radBtnGetUserState = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtKey = new System.Windows.Forms.TextBox();
            this.txtValue = new System.Windows.Forms.TextBox();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // chBoxChannel
            // 
            this.chBoxChannel.Location = new System.Drawing.Point(15, 4);
            this.chBoxChannel.Name = "chBoxChannel";
            this.chBoxChannel.Size = new System.Drawing.Size(89, 20);
            this.chBoxChannel.TabIndex = 0;
            this.chBoxChannel.Text = "Channel";
            this.chBoxChannel.CheckStateChanged += new System.EventHandler(this.chBoxChannel_CheckStateChanged);
            // 
            // chBoxChannelGroup
            // 
            this.chBoxChannelGroup.Location = new System.Drawing.Point(15, 59);
            this.chBoxChannelGroup.Name = "chBoxChannelGroup";
            this.chBoxChannelGroup.Size = new System.Drawing.Size(131, 20);
            this.chBoxChannelGroup.TabIndex = 1;
            this.chBoxChannelGroup.Text = "Channel Group";
            this.chBoxChannelGroup.CheckStateChanged += new System.EventHandler(this.chBoxChannelGroup_CheckStateChanged);
            // 
            // txtChannel
            // 
            this.txtChannel.Enabled = false;
            this.txtChannel.Location = new System.Drawing.Point(36, 30);
            this.txtChannel.Name = "txtChannel";
            this.txtChannel.Size = new System.Drawing.Size(156, 21);
            this.txtChannel.TabIndex = 2;
            // 
            // txtChannelGroup
            // 
            this.txtChannelGroup.Enabled = false;
            this.txtChannelGroup.Location = new System.Drawing.Point(36, 85);
            this.txtChannelGroup.Name = "txtChannelGroup";
            this.txtChannelGroup.Size = new System.Drawing.Size(156, 21);
            this.txtChannelGroup.TabIndex = 3;
            // 
            // radBtnSetUserState
            // 
            this.radBtnSetUserState.Location = new System.Drawing.Point(15, 131);
            this.radBtnSetUserState.Name = "radBtnSetUserState";
            this.radBtnSetUserState.Size = new System.Drawing.Size(177, 20);
            this.radBtnSetUserState.TabIndex = 4;
            this.radBtnSetUserState.TabStop = false;
            this.radBtnSetUserState.Text = "Set UserState";
            // 
            // radBtnGetUserState
            // 
            this.radBtnGetUserState.Checked = true;
            this.radBtnGetUserState.Location = new System.Drawing.Point(15, 219);
            this.radBtnGetUserState.Name = "radBtnGetUserState";
            this.radBtnGetUserState.Size = new System.Drawing.Size(100, 20);
            this.radBtnGetUserState.TabIndex = 5;
            this.radBtnGetUserState.Text = "Get UserState";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(15, 158);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 20);
            this.label1.Text = "key";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(15, 182);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 20);
            this.label2.Text = "value";
            // 
            // txtKey
            // 
            this.txtKey.Location = new System.Drawing.Point(55, 156);
            this.txtKey.Name = "txtKey";
            this.txtKey.Size = new System.Drawing.Size(137, 21);
            this.txtKey.TabIndex = 8;
            // 
            // txtValue
            // 
            this.txtValue.Location = new System.Drawing.Point(55, 180);
            this.txtValue.Name = "txtValue";
            this.txtValue.Size = new System.Drawing.Size(137, 21);
            this.txtValue.TabIndex = 9;
            // 
            // btnSubmit
            // 
            this.btnSubmit.Location = new System.Drawing.Point(15, 245);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(72, 20);
            this.btnSubmit.TabIndex = 10;
            this.btnSubmit.Text = "Submit";
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(120, 245);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(72, 20);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // PubnubUserState
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSubmit);
            this.Controls.Add(this.txtValue);
            this.Controls.Add(this.txtKey);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.radBtnGetUserState);
            this.Controls.Add(this.radBtnSetUserState);
            this.Controls.Add(this.txtChannelGroup);
            this.Controls.Add(this.txtChannel);
            this.Controls.Add(this.chBoxChannelGroup);
            this.Controls.Add(this.chBoxChannel);
            this.Menu = this.mainMenu1;
            this.Name = "PubnubUserState";
            this.Text = "UserState";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox chBoxChannel;
        private System.Windows.Forms.CheckBox chBoxChannelGroup;
        private System.Windows.Forms.TextBox txtChannel;
        private System.Windows.Forms.TextBox txtChannelGroup;
        private System.Windows.Forms.RadioButton radBtnSetUserState;
        private System.Windows.Forms.RadioButton radBtnGetUserState;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtKey;
        private System.Windows.Forms.TextBox txtValue;
        private System.Windows.Forms.Button btnSubmit;
        private System.Windows.Forms.Button btnCancel;
    }
}