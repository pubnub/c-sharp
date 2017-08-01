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
    public partial class frmAccessManager : Form
    {
        public frmAccessManager()
        {
            InitializeComponent();
        }

        public string ChannelName
        {
            get;
            set;
        }

        public string ChannelGroup
        {
            get;
            set;
        }

        public string AuthKey
        {
            get;
            set;
        }

        public bool AccessRead
        {
            get;
            set;
        }

        public bool AccessWrite
        {
            get;
            set;
        }

        public bool AccessManage
        {
            get;
            set;
        }

        public int TTL
        {
            get;
            set;
        }

        private void frmAccessManager_Load(object sender, EventArgs e)
        {
            txtPAMChannel.Text = ChannelName;
            txtChannelGroup.Text = ChannelGroup;
            txtAuthKey.Text = AuthKey;
        }

        private void btnGrant_Click(object sender, EventArgs e)
        {
            ChannelName = txtPAMChannel.Text;
            ChannelGroup = txtChannelGroup.Text;
            AuthKey = txtAuthKey.Text;

            AccessRead = chkBoxRead.Checked;
            AccessWrite = chkBoxWrite.Checked;
            AccessManage = chkBoxManage.Checked;

            int ttlValue;
            if (Int32.TryParse(txtTTL.Text, out ttlValue))
            {
                TTL = ttlValue;
            }
        }
    }
}
