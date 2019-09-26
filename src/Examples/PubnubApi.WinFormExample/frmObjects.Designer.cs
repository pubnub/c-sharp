namespace PubnubApi.WinFormExample
{
    partial class frmObjects
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
            this.rbCreateUser = new System.Windows.Forms.RadioButton();
            this.rbMembershipAdd = new System.Windows.Forms.RadioButton();
            this.rbMemberAdd = new System.Windows.Forms.RadioButton();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtUserId = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtSpaceName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtSpaceId = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.rbDeleteUser = new System.Windows.Forms.RadioButton();
            this.rbUpdateUser = new System.Windows.Forms.RadioButton();
            this.rbUpdateSpace = new System.Windows.Forms.RadioButton();
            this.rbDeleteSpace = new System.Windows.Forms.RadioButton();
            this.rbCreateSpace = new System.Windows.Forms.RadioButton();
            this.rbGetUser = new System.Windows.Forms.RadioButton();
            this.rbGetSpace = new System.Windows.Forms.RadioButton();
            this.rbMemberRemove = new System.Windows.Forms.RadioButton();
            this.rbMembershipRemove = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // rbCreateUser
            // 
            this.rbCreateUser.AutoSize = true;
            this.rbCreateUser.Location = new System.Drawing.Point(6, 135);
            this.rbCreateUser.Name = "rbCreateUser";
            this.rbCreateUser.Size = new System.Drawing.Size(81, 17);
            this.rbCreateUser.TabIndex = 4;
            this.rbCreateUser.TabStop = true;
            this.rbCreateUser.Text = "Create User";
            this.rbCreateUser.UseVisualStyleBackColor = true;
            // 
            // rbMembershipAdd
            // 
            this.rbMembershipAdd.AutoSize = true;
            this.rbMembershipAdd.Location = new System.Drawing.Point(149, 228);
            this.rbMembershipAdd.Name = "rbMembershipAdd";
            this.rbMembershipAdd.Size = new System.Drawing.Size(104, 17);
            this.rbMembershipAdd.TabIndex = 13;
            this.rbMembershipAdd.TabStop = true;
            this.rbMembershipAdd.Text = "Membership Add";
            this.rbMembershipAdd.UseVisualStyleBackColor = true;
            // 
            // rbMemberAdd
            // 
            this.rbMemberAdd.AutoSize = true;
            this.rbMemberAdd.Location = new System.Drawing.Point(6, 228);
            this.rbMemberAdd.Name = "rbMemberAdd";
            this.rbMemberAdd.Size = new System.Drawing.Size(85, 17);
            this.rbMemberAdd.TabIndex = 12;
            this.rbMemberAdd.TabStop = true;
            this.rbMemberAdd.Text = "Member Add";
            this.rbMemberAdd.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(203, 289);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnSubmit
            // 
            this.btnSubmit.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSubmit.Location = new System.Drawing.Point(95, 289);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(75, 23);
            this.btnSubmit.TabIndex = 4;
            this.btnSubmit.Text = "Submit";
            this.btnSubmit.UseVisualStyleBackColor = true;
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rbMembershipRemove);
            this.groupBox1.Controls.Add(this.rbMemberRemove);
            this.groupBox1.Controls.Add(this.rbGetSpace);
            this.groupBox1.Controls.Add(this.rbGetUser);
            this.groupBox1.Controls.Add(this.rbUpdateSpace);
            this.groupBox1.Controls.Add(this.rbDeleteSpace);
            this.groupBox1.Controls.Add(this.rbCreateSpace);
            this.groupBox1.Controls.Add(this.rbUpdateUser);
            this.groupBox1.Controls.Add(this.rbDeleteUser);
            this.groupBox1.Controls.Add(this.txtSpaceName);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtSpaceId);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.txtUserName);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.btnCancel);
            this.groupBox1.Controls.Add(this.btnSubmit);
            this.groupBox1.Controls.Add(this.txtUserId);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.rbMemberAdd);
            this.groupBox1.Controls.Add(this.rbMembershipAdd);
            this.groupBox1.Controls.Add(this.rbCreateUser);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(325, 312);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Objects - Create /Update/Delete/Manage users and spaces";
            // 
            // txtUserName
            // 
            this.txtUserName.Location = new System.Drawing.Point(95, 57);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(193, 20);
            this.txtUserName.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 60);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(60, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "UserName:";
            // 
            // txtUserId
            // 
            this.txtUserId.Location = new System.Drawing.Point(95, 31);
            this.txtUserId.Name = "txtUserId";
            this.txtUserId.Size = new System.Drawing.Size(193, 20);
            this.txtUserId.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "UserId:";
            // 
            // txtSpaceName
            // 
            this.txtSpaceName.Location = new System.Drawing.Point(95, 109);
            this.txtSpaceName.Name = "txtSpaceName";
            this.txtSpaceName.Size = new System.Drawing.Size(193, 20);
            this.txtSpaceName.TabIndex = 3;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 112);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(69, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "SpaceName:";
            // 
            // txtSpaceId
            // 
            this.txtSpaceId.Location = new System.Drawing.Point(95, 83);
            this.txtSpaceId.Name = "txtSpaceId";
            this.txtSpaceId.Size = new System.Drawing.Size(193, 20);
            this.txtSpaceId.TabIndex = 2;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 86);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(50, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "SpaceId:";
            // 
            // rbDeleteUser
            // 
            this.rbDeleteUser.AutoSize = true;
            this.rbDeleteUser.Location = new System.Drawing.Point(6, 181);
            this.rbDeleteUser.Name = "rbDeleteUser";
            this.rbDeleteUser.Size = new System.Drawing.Size(81, 17);
            this.rbDeleteUser.TabIndex = 6;
            this.rbDeleteUser.TabStop = true;
            this.rbDeleteUser.Text = "Delete User";
            this.rbDeleteUser.UseVisualStyleBackColor = true;
            // 
            // rbUpdateUser
            // 
            this.rbUpdateUser.AutoSize = true;
            this.rbUpdateUser.Location = new System.Drawing.Point(6, 158);
            this.rbUpdateUser.Name = "rbUpdateUser";
            this.rbUpdateUser.Size = new System.Drawing.Size(85, 17);
            this.rbUpdateUser.TabIndex = 5;
            this.rbUpdateUser.TabStop = true;
            this.rbUpdateUser.Text = "Update User";
            this.rbUpdateUser.UseVisualStyleBackColor = true;
            // 
            // rbUpdateSpace
            // 
            this.rbUpdateSpace.AutoSize = true;
            this.rbUpdateSpace.Location = new System.Drawing.Point(149, 158);
            this.rbUpdateSpace.Name = "rbUpdateSpace";
            this.rbUpdateSpace.Size = new System.Drawing.Size(94, 17);
            this.rbUpdateSpace.TabIndex = 8;
            this.rbUpdateSpace.TabStop = true;
            this.rbUpdateSpace.Text = "Update Space";
            this.rbUpdateSpace.UseVisualStyleBackColor = true;
            // 
            // rbDeleteSpace
            // 
            this.rbDeleteSpace.AutoSize = true;
            this.rbDeleteSpace.Location = new System.Drawing.Point(149, 181);
            this.rbDeleteSpace.Name = "rbDeleteSpace";
            this.rbDeleteSpace.Size = new System.Drawing.Size(90, 17);
            this.rbDeleteSpace.TabIndex = 10;
            this.rbDeleteSpace.TabStop = true;
            this.rbDeleteSpace.Text = "Delete Space";
            this.rbDeleteSpace.UseVisualStyleBackColor = true;
            // 
            // rbCreateSpace
            // 
            this.rbCreateSpace.AutoSize = true;
            this.rbCreateSpace.Location = new System.Drawing.Point(149, 135);
            this.rbCreateSpace.Name = "rbCreateSpace";
            this.rbCreateSpace.Size = new System.Drawing.Size(90, 17);
            this.rbCreateSpace.TabIndex = 8;
            this.rbCreateSpace.TabStop = true;
            this.rbCreateSpace.Text = "Create Space";
            this.rbCreateSpace.UseVisualStyleBackColor = true;
            // 
            // rbGetUser
            // 
            this.rbGetUser.AutoSize = true;
            this.rbGetUser.Location = new System.Drawing.Point(6, 205);
            this.rbGetUser.Name = "rbGetUser";
            this.rbGetUser.Size = new System.Drawing.Size(67, 17);
            this.rbGetUser.TabIndex = 7;
            this.rbGetUser.TabStop = true;
            this.rbGetUser.Text = "Get User";
            this.rbGetUser.UseVisualStyleBackColor = true;
            // 
            // rbGetSpace
            // 
            this.rbGetSpace.AutoSize = true;
            this.rbGetSpace.Location = new System.Drawing.Point(149, 204);
            this.rbGetSpace.Name = "rbGetSpace";
            this.rbGetSpace.Size = new System.Drawing.Size(76, 17);
            this.rbGetSpace.TabIndex = 11;
            this.rbGetSpace.TabStop = true;
            this.rbGetSpace.Text = "Get Space";
            this.rbGetSpace.UseVisualStyleBackColor = true;
            // 
            // rbMemberRemove
            // 
            this.rbMemberRemove.AutoSize = true;
            this.rbMemberRemove.Location = new System.Drawing.Point(6, 251);
            this.rbMemberRemove.Name = "rbMemberRemove";
            this.rbMemberRemove.Size = new System.Drawing.Size(106, 17);
            this.rbMemberRemove.TabIndex = 15;
            this.rbMemberRemove.TabStop = true;
            this.rbMemberRemove.Text = "Member Remove";
            this.rbMemberRemove.UseVisualStyleBackColor = true;
            // 
            // rbMembershipRemove
            // 
            this.rbMembershipRemove.AutoSize = true;
            this.rbMembershipRemove.Location = new System.Drawing.Point(149, 251);
            this.rbMembershipRemove.Name = "rbMembershipRemove";
            this.rbMembershipRemove.Size = new System.Drawing.Size(125, 17);
            this.rbMembershipRemove.TabIndex = 16;
            this.rbMembershipRemove.TabStop = true;
            this.rbMembershipRemove.Text = "Membership Remove";
            this.rbMembershipRemove.UseVisualStyleBackColor = true;
            // 
            // frmObjects
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 337);
            this.Controls.Add(this.groupBox1);
            this.Name = "frmObjects";
            this.Text = "Objects";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton rbCreateUser;
        private System.Windows.Forms.RadioButton rbMembershipAdd;
        private System.Windows.Forms.RadioButton rbMemberAdd;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSubmit;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtUserId;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtSpaceName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtSpaceId;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.RadioButton rbUpdateSpace;
        private System.Windows.Forms.RadioButton rbDeleteSpace;
        private System.Windows.Forms.RadioButton rbCreateSpace;
        private System.Windows.Forms.RadioButton rbUpdateUser;
        private System.Windows.Forms.RadioButton rbDeleteUser;
        private System.Windows.Forms.RadioButton rbGetSpace;
        private System.Windows.Forms.RadioButton rbGetUser;
        private System.Windows.Forms.RadioButton rbMembershipRemove;
        private System.Windows.Forms.RadioButton rbMemberRemove;
    }
}