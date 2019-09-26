namespace PubnubApi.WinFormExample
{
    partial class frmTokenAccessManager
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
            this.chkBoxDelete = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.chkBoxCreate = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtSpaceId = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtUserId = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnGrant
            // 
            this.btnGrant.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnGrant.Location = new System.Drawing.Point(100, 276);
            this.btnGrant.Name = "btnGrant";
            this.btnGrant.Size = new System.Drawing.Size(75, 23);
            this.btnGrant.TabIndex = 0;
            this.btnGrant.Text = "Grant Token";
            this.btnGrant.UseVisualStyleBackColor = true;
            this.btnGrant.Click += new System.EventHandler(this.btnGrant_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Channel Name:";
            // 
            // txtPAMChannel
            // 
            this.txtPAMChannel.Location = new System.Drawing.Point(102, 9);
            this.txtPAMChannel.Name = "txtPAMChannel";
            this.txtPAMChannel.Size = new System.Drawing.Size(206, 20);
            this.txtPAMChannel.TabIndex = 2;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(219, 276);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 162);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Read Access:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 191);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Write Access:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 218);
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
            this.chkBoxRead.Location = new System.Drawing.Point(103, 162);
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
            this.chkBoxWrite.Location = new System.Drawing.Point(103, 190);
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
            this.chkBoxManage.Location = new System.Drawing.Point(103, 217);
            this.chkBoxManage.Name = "chkBoxManage";
            this.chkBoxManage.Size = new System.Drawing.Size(15, 14);
            this.chkBoxManage.TabIndex = 9;
            this.chkBoxManage.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 250);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(86, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "TTL (in minutes):";
            // 
            // txtTTL
            // 
            this.txtTTL.Location = new System.Drawing.Point(103, 247);
            this.txtTTL.Name = "txtTTL";
            this.txtTTL.Size = new System.Drawing.Size(137, 20);
            this.txtTTL.TabIndex = 11;
            // 
            // txtChannelGroup
            // 
            this.txtChannelGroup.Location = new System.Drawing.Point(101, 39);
            this.txtChannelGroup.Name = "txtChannelGroup";
            this.txtChannelGroup.Size = new System.Drawing.Size(206, 20);
            this.txtChannelGroup.TabIndex = 13;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(14, 39);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(81, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Channel Group:";
            // 
            // txtAuthKey
            // 
            this.txtAuthKey.Location = new System.Drawing.Point(103, 131);
            this.txtAuthKey.Name = "txtAuthKey";
            this.txtAuthKey.Size = new System.Drawing.Size(206, 20);
            this.txtAuthKey.TabIndex = 15;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(40, 134);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 13);
            this.label7.TabIndex = 14;
            this.label7.Text = "Auth Key:";
            // 
            // chkBoxDelete
            // 
            this.chkBoxDelete.AutoSize = true;
            this.chkBoxDelete.Checked = true;
            this.chkBoxDelete.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBoxDelete.Location = new System.Drawing.Point(300, 163);
            this.chkBoxDelete.Name = "chkBoxDelete";
            this.chkBoxDelete.Size = new System.Drawing.Size(15, 14);
            this.chkBoxDelete.TabIndex = 17;
            this.chkBoxDelete.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(216, 163);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(79, 13);
            this.label8.TabIndex = 16;
            this.label8.Text = "Delete Access:";
            // 
            // chkBoxCreate
            // 
            this.chkBoxCreate.AutoSize = true;
            this.chkBoxCreate.Checked = true;
            this.chkBoxCreate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBoxCreate.Location = new System.Drawing.Point(300, 190);
            this.chkBoxCreate.Name = "chkBoxCreate";
            this.chkBoxCreate.Size = new System.Drawing.Size(15, 14);
            this.chkBoxCreate.TabIndex = 19;
            this.chkBoxCreate.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(216, 190);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(79, 13);
            this.label9.TabIndex = 18;
            this.label9.Text = "Create Access:";
            // 
            // txtSpaceId
            // 
            this.txtSpaceId.Location = new System.Drawing.Point(102, 104);
            this.txtSpaceId.Name = "txtSpaceId";
            this.txtSpaceId.Size = new System.Drawing.Size(206, 20);
            this.txtSpaceId.TabIndex = 23;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(43, 107);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(50, 13);
            this.label10.TabIndex = 22;
            this.label10.Text = "SpaceId:";
            // 
            // txtUserId
            // 
            this.txtUserId.Location = new System.Drawing.Point(103, 74);
            this.txtUserId.Name = "txtUserId";
            this.txtUserId.Size = new System.Drawing.Size(206, 20);
            this.txtUserId.TabIndex = 21;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(52, 77);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(41, 13);
            this.label11.TabIndex = 20;
            this.label11.Text = "UserId:";
            // 
            // frmTokenAccessManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(454, 311);
            this.Controls.Add(this.txtSpaceId);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.txtUserId);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.chkBoxCreate);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.chkBoxDelete);
            this.Controls.Add(this.label8);
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
            this.Name = "frmTokenAccessManager";
            this.Text = "TokenAccessManager";
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
        private System.Windows.Forms.CheckBox chkBoxDelete;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox chkBoxCreate;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtSpaceId;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtUserId;
        private System.Windows.Forms.Label label11;
    }
}