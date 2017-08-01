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
    public partial class frmChannelGroup : Form
    {
        public frmChannelGroup()
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

        public string CgRequestType
        {
            get;
            set;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            ChannelGroup = txtCg.Text;
            AuthKey = txtAuthKey.Text;
            if (rbAddChannel.Checked)
            {
                ChannelName = txtChAdd.Text;
                CgRequestType = "AddChannel";
            }
            else if (rbListChannel.Checked)
            {
                ChannelName = "";
                CgRequestType = "ListChannel";
            }
            else if (rbRemoveChannel.Checked)
            {
                ChannelName = txtChRemove.Text;
                CgRequestType = "RemoveChannel";
            }

        }
    }
}
