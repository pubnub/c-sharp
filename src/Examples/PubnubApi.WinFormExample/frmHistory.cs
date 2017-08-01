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
    public partial class frmHistory : Form
    {
        public frmHistory()
        {
            InitializeComponent();
        }

        public string ChannelName
        {
            get;
            set;
        }

        public int Count
        {
            get;
            set;
        }

        public bool Reverse
        {
            get;
            set;
        }

        public long Start
        {
            get;
            set;
        }

        public long End
        {
            get;
            set;
        }

        public string AuthKey
        {
            get;
            set;
        }

        public bool IncludeTimetoken
        {
            get;
            set;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            ChannelName = txtChannelName.Text;

            int historyCount;
            if (Int32.TryParse(txtCount.Text, out historyCount))
            {
                Count = historyCount;
            }

            Reverse = cbReverse.Checked;

            IncludeTimetoken = cbIncludeTimetoken.Checked;
        }

        private void frmHistory_Load(object sender, EventArgs e)
        {
            txtChannelName.Text = ChannelName;
        }
    }
}
