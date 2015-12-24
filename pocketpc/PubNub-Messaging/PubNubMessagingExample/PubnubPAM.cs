using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PubNubMessagingExample
{
    public partial class PubnubPAM : Form
    {
        public event EventHandler PamRequestSubmitted;

        private string _pamRequest = "";
        private bool _isPresence = false;

        public PubnubPAM()
        {
            InitializeComponent();
        }

        public string Channel = "";
        public string ChannelGroup = "";
        public string AuthKey = "";

        public bool IsPresence
        {
            get
            {
                return _isPresence;
            }
        }

        public string PamRequest
        {
            get
            {
                return _pamRequest;
            }
            set
            {
                _pamRequest = value;
                txtPAM.Text = _pamRequest;
            }
        }

        private void radioButtonChannel_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonChannel.Checked)
            {
                txtChannelGroup.Enabled = false;
                ChannelGroup = "";
            }
            else
            {
                txtChannelGroup.Enabled = true;
            }
        }

        private void radioButtonChannelGroup_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonChannelGroup.Checked)
            {
                txtChannel.Enabled = false;
                Channel = "";
            }
            else
            {
                txtChannel.Enabled = true;
            }

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtPAM_Click(object sender, EventArgs e)
        {
            if (!radioButtonChannel.Checked && !radioButtonChannelGroup.Checked)
            {
                MessageBox.Show("Channel/ChannelGroup should be clicked");
                return;
            }

            if (txtChannel.Text.Trim() == "" && txtChannelGroup.Text.Trim() == "")
            {
                MessageBox.Show("Channel/ChannelGroup cannot be empty");
                return;
            }

            if (radioButtonChannel.Checked)
            {
                Channel = txtChannel.Text;
            }
            else
            {
                ChannelGroup = txtChannelGroup.Text;
            }

            AuthKey = txtAuthKey.Text.Trim();
            _isPresence = chBoxIsPresence.Checked;

            this.Close();
            PamRequestSubmitted(sender, e);
        }
    }
}