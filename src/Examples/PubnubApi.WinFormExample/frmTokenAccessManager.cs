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
    public partial class frmTokenAccessManager : Form
    {
        public frmTokenAccessManager()
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

        public string UserId
        {
            get;
            set;
        }

        public string SpaceId
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

        public bool AccessDelete
        {
            get;
            set;
        }

        public bool AccessCreate
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
            UserId = txtUserId.Text;
            SpaceId = txtSpaceId.Text;
            AuthKey = txtAuthKey.Text;

            AccessRead = chkBoxRead.Checked;
            AccessWrite = chkBoxWrite.Checked;
            AccessManage = chkBoxManage.Checked;
            AccessDelete = chkBoxDelete.Checked;
            AccessCreate = chkBoxCreate.Checked;

            int ttlValue;
            if (Int32.TryParse(txtTTL.Text, out ttlValue))
            {
                TTL = ttlValue;
            }
        }
    }
}
