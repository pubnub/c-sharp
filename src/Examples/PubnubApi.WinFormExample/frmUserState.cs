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
    public partial class frmUserState : Form
    {
        public frmUserState()
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

        public string UUID
        {
            get;
            set;
        }

        public string StateRequestType
        {
            get;
            set;
        }

        public Dictionary<string, object> StateKV
        {
            get;
            set;
        } = new Dictionary<string, object>();

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            ChannelName = txtChannel.Text;
            ChannelGroup = txtCg.Text;
            AuthKey = txtAuthKey.Text;
            
            if (rbSetState.Checked)
            {
                StateRequestType = "set";
                UUID = "";
                StateKV.Add(txtKey.Text, txtVal.Text);
            }
            else if (rbGetState.Checked)
            {
                StateRequestType = "get";
                UUID = txtUUID.Text;
                StateKV.Clear();
            }
        }

        private void frmUserState_Load(object sender, EventArgs e)
        {
            txtChannel.Text = ChannelName;
            txtCg.Text = ChannelGroup;
            txtAuthKey.Text = AuthKey;
        }
    }
}
