using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PubNubMessagingExample
{
    public partial class PubnubPush : Form
    {
        public EventHandler PushRequestSubmitted;
        public string PushTypeRequest = "";
        public string ChannelName = "";
        public string PushToken = "";

        public PubnubPush()
        {
            InitializeComponent();
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtToken.Text))
            {
                MessageBox.Show("Enter token for push");
                return;
            }

            if (radBtnRegister.Checked)
            {
                if (string.IsNullOrEmpty(txtChannel.Text))
                {
                    MessageBox.Show("Enter channel name");
                    return;
                }
                PushTypeRequest = "RegisterDevice";
                ChannelName = txtChannel.Text;
                PushToken = txtToken.Text;
            }
            else if (radBtnUnregister.Checked)
            {
                PushTypeRequest = "UnregisterDevice";
                ChannelName = "";
                PushToken = txtToken.Text;
            }
            else if (radBtnRemoveChannel.Checked)
            {
                if (string.IsNullOrEmpty(txtChannel.Text))
                {
                    MessageBox.Show("Enter channel name");
                    return;
                }
                PushTypeRequest = "RemoveChannel";
                ChannelName = txtChannel.Text;
                PushToken = txtToken.Text;
            }
            else if (radBtnGetChannels.Checked)
            {
                PushTypeRequest = "GetChannels";
                ChannelName = "";
                PushToken = txtToken.Text;
            }
            else
            {
                MessageBox.Show("Please select valid request");
                return;
            }

            this.Close();
            PushRequestSubmitted(sender, e);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}