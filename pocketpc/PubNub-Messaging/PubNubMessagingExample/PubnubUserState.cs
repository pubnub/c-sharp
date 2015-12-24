using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PubNubMessagingExample
{
    public partial class PubnubUserState : Form
    {
        public event EventHandler UserStateRequestSubmitted;
        private string _userStateRequest = "";

        public string ChannelName = "";
        public string ChannelGroupName = "";
        public string StateKey = "";
        public string StateValue = "";

        public PubnubUserState()
        {
            InitializeComponent();
        }

        public string UserStateRequest
        {
            get
            {
                return _userStateRequest;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (!chBoxChannel.Checked && !chBoxChannelGroup.Checked)
            {
                MessageBox.Show("Enter channel/channel group name");
                return;
            }

            if (chBoxChannel.Checked)
            {
                if (!string.IsNullOrEmpty(txtChannel.Text))
                {
                    ChannelName = txtChannel.Text;
                }
                else
                {
                    MessageBox.Show("Enter channel name");
                    return;
                }
            }
            else
            {
                txtChannel.Text = "";
                ChannelName = "";
            }

            if (chBoxChannelGroup.Checked)
            {
                if (!string.IsNullOrEmpty(txtChannelGroup.Text))
                {
                    ChannelGroupName = txtChannelGroup.Text;
                }
                else
                {
                    MessageBox.Show("Enter channel group name");
                    return;
                }
            }
            else
            {
                txtChannelGroup.Text = "";
                ChannelGroupName = "";
            }

            if (radBtnGetUserState.Checked)
            {
                this._userStateRequest = "GetUserState";
            }
            else
            {
                this._userStateRequest = "SetUserState";
                if (string.IsNullOrEmpty(txtKey.Text) || string.IsNullOrEmpty(txtValue.Text))
                {
                    MessageBox.Show("Enter valid key/value pair");
                    return;
                }
                StateKey = txtKey.Text;
                StateValue = txtValue.Text;
            }
            this.Close();
            UserStateRequestSubmitted(sender, e);
        }

        private void chBoxChannel_CheckStateChanged(object sender, EventArgs e)
        {
            if (chBoxChannel.Checked)
            {
                txtChannel.Enabled = true;
            }
            else
            {
                txtChannel.Enabled = false;
                txtChannel.Text = "";
            }
        }

        private void chBoxChannelGroup_CheckStateChanged(object sender, EventArgs e)
        {
            if (chBoxChannelGroup.Checked)
            {
                txtChannelGroup.Enabled = true;
            }
            else
            {
                txtChannelGroup.Enabled = false;
                txtChannelGroup.Text = "";
            }
        }
    }
}