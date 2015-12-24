namespace PubNubMessagingExample
{
    partial class PubnubChangeUUID
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.txtNewSessionUUID = new System.Windows.Forms.TextBox();
            this.mainMenu2 = new System.Windows.Forms.MainMenu();
            this.label2 = new System.Windows.Forms.Label();
            this.txtOldSessionUUID = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(136, 154);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(72, 20);
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click_1);
            // 
            // btnSubmit
            // 
            this.btnSubmit.Location = new System.Drawing.Point(12, 154);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(72, 20);
            this.btnSubmit.TabIndex = 11;
            this.btnSubmit.Text = "Submit";
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click_1);
            // 
            // txtNewSessionUUID
            // 
            this.txtNewSessionUUID.Location = new System.Drawing.Point(12, 109);
            this.txtNewSessionUUID.Name = "txtNewSessionUUID";
            this.txtNewSessionUUID.Size = new System.Drawing.Size(196, 21);
            this.txtNewSessionUUID.TabIndex = 10;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(12, 85);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 20);
            this.label2.Text = "Change to";
            // 
            // txtOldSessionUUID
            // 
            this.txtOldSessionUUID.Enabled = false;
            this.txtOldSessionUUID.Location = new System.Drawing.Point(12, 34);
            this.txtOldSessionUUID.Name = "txtOldSessionUUID";
            this.txtOldSessionUUID.ReadOnly = true;
            this.txtOldSessionUUID.Size = new System.Drawing.Size(196, 21);
            this.txtOldSessionUUID.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(196, 20);
            this.label1.Text = "Current Session UUID";
            // 
            // PubnubChangeUUID
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSubmit);
            this.Controls.Add(this.txtNewSessionUUID);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtOldSessionUUID);
            this.Controls.Add(this.label1);
            this.Menu = this.mainMenu1;
            this.Name = "PubnubChangeUUID";
            this.Text = "Change UUID";
            this.Load += new System.EventHandler(this.PubnubChangeUUID_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSubmit;
        private System.Windows.Forms.TextBox txtNewSessionUUID;
        private System.Windows.Forms.MainMenu mainMenu2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtOldSessionUUID;
        private System.Windows.Forms.Label label1;
    }
}