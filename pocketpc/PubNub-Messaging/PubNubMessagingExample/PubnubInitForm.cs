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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNext_Click(object sender, EventArgs e)
        {
            // Check mandetory Inputs
           if (string.IsNullOrEmpty(txtOrigin.Text.Trim()))
            {
                MessageBox.Show("Please enter origin");
                return;
            }
            if (string.IsNullOrEmpty(txtPublishKey.Text.Trim()))
            {
                MessageBox.Show("Please enter publish key");
                return;
            }
            if (string.IsNullOrEmpty(txtSubscribeKey.Text.Trim()))
            {
                MessageBox.Show("Please enter subscribe key");
                return;
            }

           
            
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
        
        /// <summary>
        /// Numeric Inputs Only
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtPresenceHeartbeatInterval_KeyPressed(object sender, KeyPressEventArgs e)
        {
            if (!Char.IsDigit(e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }
        /// <summary>
        /// Numeric Inputs Only
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtPresenceHeartbeat_OnKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Char.IsDigit(e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        
    }
}