namespace PubNubMessagingExample
{
    partial class PubnubResult
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
            this.lbResult = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // lbResult
            // 
            this.lbResult.Location = new System.Drawing.Point(3, 4);
            this.lbResult.Name = "lbResult";
            this.lbResult.Size = new System.Drawing.Size(234, 282);
            this.lbResult.TabIndex = 0;
            // 
            // PubnubResult
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 294);
            this.Controls.Add(this.lbResult);
            this.Name = "PubnubResult";
            this.Text = "Messages";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lbResult;
    }
}