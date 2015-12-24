using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PubNubMessagingExample
{
    public partial class PubnubChangeUUID : Form
    {
        public EventHandler ChangeUUIDSubmitted;

        public string NewUUID = "";
        public string OldUUID = "";

        public PubnubChangeUUID()
        {
            InitializeComponent();
        }

        private void btnCancel_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSubmit_Click_1(object sender, EventArgs e)
        {
            string oldUUID = txtOldSessionUUID.Text.Trim();
            string newUUID = txtNewSessionUUID.Text.Trim();

            if (oldUUID == newUUID)
            {
                MessageBox.Show("No change in UUID");
                return;
            }
            NewUUID = newUUID;
            ChangeUUIDSubmitted(sender, e);
            this.Close();
        }

        private void PubnubChangeUUID_Load(object sender, EventArgs e)
        {
            txtOldSessionUUID.Text = OldUUID;
        }
    }
}