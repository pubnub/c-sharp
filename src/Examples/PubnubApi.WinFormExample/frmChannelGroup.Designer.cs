namespace PubnubApi.WinFormExample
{
    partial class frmChannelGroup
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
            this.rbAddChannel = new System.Windows.Forms.RadioButton();
            this.rbListChannel = new System.Windows.Forms.RadioButton();
            this.rbRemoveChannel = new System.Windows.Forms.RadioButton();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtChRemove = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtChAdd = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtCg = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtAuthKey = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // rbAddChannel
            // 
            this.rbAddChannel.AutoSize = true;
            this.rbAddChannel.Location = new System.Drawing.Point(9, 103);
            this.rbAddChannel.Name = "rbAddChannel";
            this.rbAddChannel.Size = new System.Drawing.Size(86, 17);
            this.rbAddChannel.TabIndex = 0;
            this.rbAddChannel.TabStop = true;
            this.rbAddChannel.Text = "Add Channel";
            this.rbAddChannel.UseVisualStyleBackColor = true;
            // 
            // rbListChannel
            // 
            this.rbListChannel.AutoSize = true;
            this.rbListChannel.Location = new System.Drawing.Point(9, 163);
            this.rbListChannel.Name = "rbListChannel";
            this.rbListChannel.Size = new System.Drawing.Size(88, 17);
            this.rbListChannel.TabIndex = 0;
            this.rbListChannel.TabStop = true;
            this.rbListChannel.Text = "List Channels";
            this.rbListChannel.UseVisualStyleBackColor = true;
            // 
            // rbRemoveChannel
            // 
            this.rbRemoveChannel.AutoSize = true;
            this.rbRemoveChannel.Location = new System.Drawing.Point(9, 196);
            this.rbRemoveChannel.Name = "rbRemoveChannel";
            this.rbRemoveChannel.Size = new System.Drawing.Size(107, 17);
            this.rbRemoveChannel.TabIndex = 0;
            this.rbRemoveChannel.TabStop = true;
            this.rbRemoveChannel.Text = "Remove Channel";
            this.rbRemoveChannel.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(199, 276);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnSubmit
            // 
            this.btnSubmit.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSubmit.Location = new System.Drawing.Point(96, 276);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(75, 23);
            this.btnSubmit.TabIndex = 4;
            this.btnSubmit.Text = "Submit";
            this.btnSubmit.UseVisualStyleBackColor = true;
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtAuthKey);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txtChRemove);
            this.groupBox1.Controls.Add(this.btnCancel);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.btnSubmit);
            this.groupBox1.Controls.Add(this.txtChAdd);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtCg);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.rbRemoveChannel);
            this.groupBox1.Controls.Add(this.rbListChannel);
            this.groupBox1.Controls.Add(this.rbAddChannel);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(299, 299);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Channel Group - Add / List / Remove";
            // 
            // txtChRemove
            // 
            this.txtChRemove.Location = new System.Drawing.Point(98, 219);
            this.txtChRemove.Name = "txtChRemove";
            this.txtChRemove.Size = new System.Drawing.Size(193, 20);
            this.txtChRemove.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 222);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Channel Name:";
            // 
            // txtChAdd
            // 
            this.txtChAdd.Location = new System.Drawing.Point(98, 124);
            this.txtChAdd.Name = "txtChAdd";
            this.txtChAdd.Size = new System.Drawing.Size(193, 20);
            this.txtChAdd.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 127);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Channel Name:";
            // 
            // txtCg
            // 
            this.txtCg.Location = new System.Drawing.Point(95, 31);
            this.txtCg.Name = "txtCg";
            this.txtCg.Size = new System.Drawing.Size(193, 20);
            this.txtCg.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Channel Group:";
            // 
            // txtAuthKey
            // 
            this.txtAuthKey.Location = new System.Drawing.Point(93, 62);
            this.txtAuthKey.Name = "txtAuthKey";
            this.txtAuthKey.Size = new System.Drawing.Size(193, 20);
            this.txtAuthKey.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 65);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Auth Key:";
            // 
            // frmChannelGroup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(339, 324);
            this.Controls.Add(this.groupBox1);
            this.Name = "frmChannelGroup";
            this.Text = "ChannelGroup";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton rbAddChannel;
        private System.Windows.Forms.RadioButton rbListChannel;
        private System.Windows.Forms.RadioButton rbRemoveChannel;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSubmit;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtChRemove;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtChAdd;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtCg;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtAuthKey;
        private System.Windows.Forms.Label label4;
    }
}