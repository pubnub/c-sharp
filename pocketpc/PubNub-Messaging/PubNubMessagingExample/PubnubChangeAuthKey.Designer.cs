namespace PubNubMessagingExample
{
    partial class PubnubChangeAuthKey
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
            this.txtOldAuthKey = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtNewAuthKey = new System.Windows.Forms.TextBox();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(19, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 20);
            this.label1.Text = "Current Auth Key";
            // 
            // txtOldAuthKey
            // 
            this.txtOldAuthKey.Enabled = false;
            this.txtOldAuthKey.Location = new System.Drawing.Point(19, 38);
            this.txtOldAuthKey.Name = "txtOldAuthKey";
            this.txtOldAuthKey.ReadOnly = true;
            this.txtOldAuthKey.Size = new System.Drawing.Size(196, 21);
            this.txtOldAuthKey.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(19, 89);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 20);
            this.label2.Text = "Change to";
            // 
            // txtNewAuthKey
            // 
            this.txtNewAuthKey.Location = new System.Drawing.Point(19, 113);
            this.txtNewAuthKey.Name = "txtNewAuthKey";
            this.txtNewAuthKey.Size = new System.Drawing.Size(196, 21);
            this.txtNewAuthKey.TabIndex = 4;
            // 
            // btnSubmit
            // 
            this.btnSubmit.Location = new System.Drawing.Point(19, 158);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(72, 20);
            this.btnSubmit.TabIndex = 5;
            this.btnSubmit.Text = "Submit";
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(143, 158);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(72, 20);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // PubnubChangeAuthKey
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSubmit);
            this.Controls.Add(this.txtNewAuthKey);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtOldAuthKey);
            this.Controls.Add(this.label1);
            this.Menu = this.mainMenu1;
            this.Name = "PubnubChangeAuthKey";
            this.Text = "Change Auth Key";
            this.Load += new System.EventHandler(this.PubnubChangeAuthKey_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtOldAuthKey;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtNewAuthKey;
        private System.Windows.Forms.Button btnSubmit;
        private System.Windows.Forms.Button btnCancel;
    }
}