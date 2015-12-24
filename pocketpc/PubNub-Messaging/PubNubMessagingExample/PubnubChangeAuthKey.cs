using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PubNubMessagingExample
{
    public partial class PubnubChangeAuthKey : Form
    {
        public EventHandler ChangeAuthKeySubmitted;

        public string NewAuthKey = "";
        public string OldAuthKey = "";

        public PubnubChangeAuthKey()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            string oldAuthKey = txtOldAuthKey.Text.Trim();
            string newAuthKey = txtNewAuthKey.Text.Trim();

            if (oldAuthKey == newAuthKey)
            {
                MessageBox.Show("No change in auth key");
                return;
            }
            NewAuthKey = newAuthKey;
            this.Close();
            ChangeAuthKeySubmitted(sender, e);
        }

        private void PubnubChangeAuthKey_Load(object sender, EventArgs e)
        {
            txtOldAuthKey.Text = OldAuthKey;
        }
    }
}