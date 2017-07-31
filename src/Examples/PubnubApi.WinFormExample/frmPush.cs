using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PubnubApi.WinFormExample
{
    public partial class frmPush : Form
    {
        public frmPush()
        {
            InitializeComponent();
        }

        public string DeviceId
        {
            get;
            set;
        }

        public string ChannelName
        {
            get;
            set;
        }

        public string AuthKey
        {
            get;
            set;
        }

        public string PushRequestType
        {
            get;
            set;
        }

        public string PushServiceType
        {
            get;
            set;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            DeviceId = txtDeviceId.Text;
            AuthKey = txtAuthKey.Text;
            PushServiceType = ddlPushService.SelectedText;

            if (rbAddChannel.Checked)
            {
                ChannelName = txtChannel.Text;
                PushRequestType = "add";
            }
            else if (rbListChannel.Checked)
            {
                ChannelName = "";
                PushRequestType = "list";
            }
            else if (rbRemoveChannel.Checked)
            {
                ChannelName = txtChRemove.Text;
                PushRequestType = "remove";
            }
        }

        private void frmPush_Load(object sender, EventArgs e)
        {
            txtAuthKey.Text = AuthKey;
            txtDeviceId.Text = DeviceId;
            ddlPushService.SelectedIndex = 0;
        }
    }
}
