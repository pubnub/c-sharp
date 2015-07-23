using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PubNubMessagingExample
{
    public partial class PubnubInitForm : Form
    {
        public PubnubInitForm()
        {
            InitializeComponent();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            PubnubDemo demoForm = new PubnubDemo();
            demoForm.Origin = txtOrigin.Text;
            demoForm.PublishKey = txtPublishKey.Text;
            demoForm.SubscribeKey = txtSubscribeKey.Text;
            demoForm.SecretKey = txtSecretKey.Text;
            demoForm.CipherKey = txtCipherKey.Text;
            demoForm.EnableSSL = chkEnableSSL.Checked;
            demoForm.SessionUUID = txtSessionUUID.Text;
            demoForm.EnableResumeOnReconnect = chkReconnect.Checked;
            try
            {
                demoForm.PresenceHeartbeatInSec = Int32.Parse(txtPresenceHeartbeat.Text);
            }
            catch { }
            
            try
            {
                demoForm.PresenceHeartbeatIntervalInSec = Int32.Parse(txtPresenceHeartbeatInterval.Text);
            }
            catch { }

            demoForm.PubnubInitialize();

            demoForm.Show();

            this.Hide();
        }

        
    }
}