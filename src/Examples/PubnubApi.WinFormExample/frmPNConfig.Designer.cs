namespace PubnubApi.WinFormExample
{
    partial class frmPNConfig
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
            this.txtSubscribeKey = new System.Windows.Forms.TextBox();
            this.txtPublishKey = new System.Windows.Forms.TextBox();
            this.txtSecretKey = new System.Windows.Forms.TextBox();
            this.txtCipherKey = new System.Windows.Forms.TextBox();
            this.txtAuthKey = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.chkBoxSecure = new System.Windows.Forms.CheckBox();
            this.txtUUID = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.btnUpdateConfig = new System.Windows.Forms.Button();
            this.btnCancelConfig = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Subscribe Key:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 59);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Publish Key:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(30, 88);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Secret Key:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(31, 117);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(61, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Cipher Key:";
            // 
            // txtSubscribeKey
            // 
            this.txtSubscribeKey.Location = new System.Drawing.Point(98, 25);
            this.txtSubscribeKey.Name = "txtSubscribeKey";
            this.txtSubscribeKey.Size = new System.Drawing.Size(311, 20);
            this.txtSubscribeKey.TabIndex = 4;
            // 
            // txtPublishKey
            // 
            this.txtPublishKey.Location = new System.Drawing.Point(98, 56);
            this.txtPublishKey.Name = "txtPublishKey";
            this.txtPublishKey.Size = new System.Drawing.Size(311, 20);
            this.txtPublishKey.TabIndex = 5;
            // 
            // txtSecretKey
            // 
            this.txtSecretKey.Location = new System.Drawing.Point(98, 85);
            this.txtSecretKey.Name = "txtSecretKey";
            this.txtSecretKey.Size = new System.Drawing.Size(311, 20);
            this.txtSecretKey.TabIndex = 6;
            // 
            // txtCipherKey
            // 
            this.txtCipherKey.Location = new System.Drawing.Point(98, 114);
            this.txtCipherKey.Name = "txtCipherKey";
            this.txtCipherKey.Size = new System.Drawing.Size(311, 20);
            this.txtCipherKey.TabIndex = 7;
            // 
            // txtAuthKey
            // 
            this.txtAuthKey.Location = new System.Drawing.Point(98, 202);
            this.txtAuthKey.Name = "txtAuthKey";
            this.txtAuthKey.Size = new System.Drawing.Size(311, 20);
            this.txtAuthKey.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(39, 205);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Auth Key:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(47, 183);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(44, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Secure:";
            // 
            // chkBoxSecure
            // 
            this.chkBoxSecure.AutoSize = true;
            this.chkBoxSecure.Location = new System.Drawing.Point(98, 182);
            this.chkBoxSecure.Name = "chkBoxSecure";
            this.chkBoxSecure.Size = new System.Drawing.Size(15, 14);
            this.chkBoxSecure.TabIndex = 11;
            this.chkBoxSecure.UseVisualStyleBackColor = true;
            // 
            // txtUUID
            // 
            this.txtUUID.Location = new System.Drawing.Point(98, 143);
            this.txtUUID.Name = "txtUUID";
            this.txtUUID.Size = new System.Drawing.Size(311, 20);
            this.txtUUID.TabIndex = 13;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(54, 146);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(37, 13);
            this.label7.TabIndex = 12;
            this.label7.Text = "UUID:";
            // 
            // btnUpdateConfig
            // 
            this.btnUpdateConfig.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnUpdateConfig.Location = new System.Drawing.Point(98, 236);
            this.btnUpdateConfig.Name = "btnUpdateConfig";
            this.btnUpdateConfig.Size = new System.Drawing.Size(100, 23);
            this.btnUpdateConfig.TabIndex = 14;
            this.btnUpdateConfig.Text = "Update Config";
            this.btnUpdateConfig.UseVisualStyleBackColor = true;
            this.btnUpdateConfig.Click += new System.EventHandler(this.btnUpdateConfig_Click);
            // 
            // btnCancelConfig
            // 
            this.btnCancelConfig.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelConfig.Location = new System.Drawing.Point(215, 236);
            this.btnCancelConfig.Name = "btnCancelConfig";
            this.btnCancelConfig.Size = new System.Drawing.Size(100, 23);
            this.btnCancelConfig.TabIndex = 15;
            this.btnCancelConfig.Text = "Cancel";
            this.btnCancelConfig.UseVisualStyleBackColor = true;
            // 
            // frmPNConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(421, 271);
            this.Controls.Add(this.btnCancelConfig);
            this.Controls.Add(this.btnUpdateConfig);
            this.Controls.Add(this.txtUUID);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.chkBoxSecure);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtAuthKey);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtCipherKey);
            this.Controls.Add(this.txtSecretKey);
            this.Controls.Add(this.txtPublishKey);
            this.Controls.Add(this.txtSubscribeKey);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "frmPNConfig";
            this.Text = "frmPNConfigcs";
            this.Load += new System.EventHandler(this.frmPNConfig_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtSubscribeKey;
        private System.Windows.Forms.TextBox txtPublishKey;
        private System.Windows.Forms.TextBox txtSecretKey;
        private System.Windows.Forms.TextBox txtCipherKey;
        private System.Windows.Forms.TextBox txtAuthKey;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkBoxSecure;
        private System.Windows.Forms.TextBox txtUUID;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnUpdateConfig;
        private System.Windows.Forms.Button btnCancelConfig;
    }
}