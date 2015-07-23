using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PubNubMessagingExample
{
    public partial class PubnubChannelGroup : Form
    {
        public EventHandler ChannelGroupRequestSubmitted;
        public string ChannelGroupRequestType = "";
        public string ChannelName = "";
        public string ChannelGroupName = "";

        public PubnubChannelGroup()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtChannelGroup.Text))
            {
                MessageBox.Show("Enter channel group");
                return;
            }

            if (radBtnAddChannel.Checked)
            {
                if (string.IsNullOrEmpty(txtChannel.Text))
                {
                    MessageBox.Show("Enter channel name");
                    return;
                }
                ChannelName = txtChannel.Text.Trim();
                ChannelGroupName = txtChannelGroup.Text.Trim();
                ChannelGroupRequestType = "AddChannel";
            }
            else if (radBtnRemoveChannel.Checked)
            {
                if (string.IsNullOrEmpty(txtChannel.Text))
                {
                    MessageBox.Show("Enter channel name");
                    return;
                }
                ChannelName = txtChannel.Text.Trim();
                ChannelGroupName = txtChannelGroup.Text.Trim();
                ChannelGroupRequestType = "RemoveChannel";
            }
            else if (radBtnGetChannelList.Checked)
            {
                ChannelGroupName = txtChannelGroup.Text.Trim();
                ChannelGroupRequestType = "GetChannelList";
            }

            this.Close();
            ChannelGroupRequestSubmitted(sender, e);
        }
    }
}