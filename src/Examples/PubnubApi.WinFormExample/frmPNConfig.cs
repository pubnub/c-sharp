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
    public partial class frmPNConfig : Form
    {
        public string SubscribeKey
        {
            get;
            set;
        }

        public string PublishKey
        {
            get;
            set;
        }

        public string SecretKey
        {
            get;
            set;
        }

        public string CipherKey
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

        public bool Secure
        {
            get;
            set;
        }

        public frmPNConfig()
        {
            InitializeComponent();
        }

        private void frmPNConfig_Load(object sender, EventArgs e)
        {
            txtSubscribeKey.Text = SubscribeKey;
            txtPublishKey.Text = PublishKey;
            txtSecretKey.Text = SecretKey;
            txtCipherKey.Text = CipherKey;
            txtAuthKey.Text = AuthKey;
            txtUUID.Text = UUID;
            chkBoxSecure.Checked = Secure;
        }

        private void btnUpdateConfig_Click(object sender, EventArgs e)
        {
            SubscribeKey = txtSubscribeKey.Text;
            PublishKey = txtPublishKey.Text;
            SecretKey = txtSecretKey.Text;
            CipherKey = txtCipherKey.Text;
            AuthKey = txtAuthKey.Text;
            UUID = txtUUID.Text;
            Secure = chkBoxSecure.Checked;
        }
    }
}
