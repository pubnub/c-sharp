using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PubNubMessagingExample
{
    public partial class PublishForm : Form
    {
        public string Message = "";
        public bool StoreInHistory = true;

        public PublishForm()
        {
            InitializeComponent();
        }

        private void btnPublish_Click(object sender, EventArgs e)
        {
            if (txtPublishMessage.Text != "")
            {
                this.StoreInHistory = chkStoreInHistory.Checked;
                this.Message = txtPublishMessage.Text;
                this.Close();
            }
            else
            {
                MessageBox.Show("Message cannot be empty");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}