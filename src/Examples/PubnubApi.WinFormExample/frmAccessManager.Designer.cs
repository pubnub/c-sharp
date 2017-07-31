namespace PubnubApi.WinFormExample
{
    partial class frmAccessManager
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
            this.btnGrant = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtPAMChannel = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.chkBoxRead = new System.Windows.Forms.CheckBox();
            this.chkBoxWrite = new System.Windows.Forms.CheckBox();
            this.chkBoxManage = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtTTL = new System.Windows.Forms.TextBox();
            this.txtChannelGroup = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtAuthKey = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnGrant
            // 
            this.btnGrant.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnGrant.Location = new System.Drawing.Point(100, 253);
            this.btnGrant.Name = "btnGrant";
            this.btnGrant.Size = new System.Drawing.Size(75, 23);
            this.btnGrant.TabIndex = 0;
            this.btnGrant.Text = "Grant";
            this.btnGrant.UseVisualStyleBackColor = true;
            this.btnGrant.Click += new System.EventHandler(this.btnGrant_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Channel Name:";
            // 
            // txtPAMChannel
            // 
            this.txtPAMChannel.Location = new System.Drawing.Point(100, 22);
            this.txtPAMChannel.Name = "txtPAMChannel";
            this.txtPAMChannel.Size = new System.Drawing.Size(206, 20);
            this.txtPAMChannel.TabIndex = 2;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(190, 253);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 125);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Read Access:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 154);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Write Access:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 181);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(87, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Manage Access:";
            // 
            // chkBoxRead
            // 
            this.chkBoxRead.AutoSize = true;
            this.chkBoxRead.Checked = true;
            this.chkBoxRead.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBoxRead.Location = new System.Drawing.Point(100, 125);
            this.chkBoxRead.Name = "chkBoxRead";
            this.chkBoxRead.Size = new System.Drawing.Size(15, 14);
            this.chkBoxRead.TabIndex = 7;
            this.chkBoxRead.UseVisualStyleBackColor = true;
            // 
            // chkBoxWrite
            // 
            this.chkBoxWrite.AutoSize = true;
            this.chkBoxWrite.Checked = true;
            this.chkBoxWrite.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBoxWrite.Location = new System.Drawing.Point(100, 153);
            this.chkBoxWrite.Name = "chkBoxWrite";
            this.chkBoxWrite.Size = new System.Drawing.Size(15, 14);
            this.chkBoxWrite.TabIndex = 8;
            this.chkBoxWrite.UseVisualStyleBackColor = true;
            // 
            // chkBoxManage
            // 
            this.chkBoxManage.AutoSize = true;
            this.chkBoxManage.Checked = true;
            this.chkBoxManage.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBoxManage.Location = new System.Drawing.Point(100, 180);
            this.chkBoxManage.Name = "chkBoxManage";
            this.chkBoxManage.Size = new System.Drawing.Size(15, 14);
            this.chkBoxManage.TabIndex = 9;
            this.chkBoxManage.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 213);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(86, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "TTL (in minutes):";
            // 
            // txtTTL
            // 
            this.txtTTL.Location = new System.Drawing.Point(100, 210);
            this.txtTTL.Name = "txtTTL";
            this.txtTTL.Size = new System.Drawing.Size(137, 20);
            this.txtTTL.TabIndex = 11;
            // 
            // txtChannelGroup
            // 
            this.txtChannelGroup.Location = new System.Drawing.Point(100, 58);
            this.txtChannelGroup.Name = "txtChannelGroup";
            this.txtChannelGroup.Size = new System.Drawing.Size(206, 20);
            this.txtChannelGroup.TabIndex = 13;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 58);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(81, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Channel Group:";
            // 
            // txtAuthKey
            // 
            this.txtAuthKey.Location = new System.Drawing.Point(100, 94);
            this.txtAuthKey.Name = "txtAuthKey";
            this.txtAuthKey.Size = new System.Drawing.Size(206, 20);
            this.txtAuthKey.TabIndex = 15;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(37, 97);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 13);
            this.label7.TabIndex = 14;
            this.label7.Text = "Auth Key:";
            // 
            // frmAccessManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(454, 311);
            this.Controls.Add(this.txtAuthKey);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtChannelGroup);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtTTL);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.chkBoxManage);
            this.Controls.Add(this.chkBoxWrite);
            this.Controls.Add(this.chkBoxRead);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.txtPAMChannel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnGrant);
            this.Name = "frmAccessManager";
            this.Text = "AccessManager";
            this.Load += new System.EventHandler(this.frmAccessManager_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnGrant;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPAMChannel;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkBoxRead;
        private System.Windows.Forms.CheckBox chkBoxWrite;
        private System.Windows.Forms.CheckBox chkBoxManage;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtTTL;
        private System.Windows.Forms.TextBox txtChannelGroup;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtAuthKey;
        private System.Windows.Forms.Label label7;
    }
}