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
    public partial class frmObjects : Form
    {
        public frmObjects()
        {
            InitializeComponent();
        }

        public string UserId
        {
            get;
            set;
        }
        public string UserName
        {
            get;
            set;
        }

        public string SpaceId
        {
            get;
            set;
        }
        public string SpaceName
        {
            get;
            set;
        }

        public string AuthKey
        {
            get;
            set;
        }

        public string ObjectsRequestType
        {
            get;
            set;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {

            if (rbCreateUser.Checked)
            {
                UserId = txtUserId.Text;
                UserName = txtUserName.Text;
                ObjectsRequestType = "CreateUser";
            }
            else if (rbUpdateUser.Checked)
            {
                UserId = txtUserId.Text;
                UserName = txtUserName.Text;
                ObjectsRequestType = "UpdateUser";
            }
            else if (rbDeleteUser.Checked)
            {
                UserId = txtUserId.Text;
                ObjectsRequestType = "DeleteUser";
            }
            else if (rbGetUser.Checked)
            {
                UserId = txtUserId.Text;
                ObjectsRequestType = "GetUser";
            }
            else if (rbCreateSpace.Checked)
            {
                SpaceId = txtSpaceId.Text;
                SpaceName = txtSpaceName.Text;
                ObjectsRequestType = "CreateSpace";
            }
            else if (rbUpdateSpace.Checked)
            {
                SpaceId = txtSpaceId.Text;
                SpaceName = txtSpaceName.Text;
                ObjectsRequestType = "UpdateSpace";
            }
            else if (rbDeleteSpace.Checked)
            {
                SpaceId = txtSpaceId.Text;
                ObjectsRequestType = "DeleteSpace";
            }
            else if (rbGetSpace.Checked)
            {
                SpaceId = txtSpaceId.Text;
                ObjectsRequestType = "GetSpace";
            }
            else if (rbMemberAdd.Checked)
            {
                UserId = txtUserId.Text;
                SpaceId = txtSpaceId.Text;
                ObjectsRequestType = "MemberAdd";
            }
            else if (rbMemberRemove.Checked)
            {
                UserId = txtUserId.Text;
                SpaceId = txtSpaceId.Text;
                ObjectsRequestType = "MemberRemove";
            }
            else if (rbMembershipAdd.Checked)
            {
                UserId = txtUserId.Text;
                SpaceId = txtSpaceId.Text;
                ObjectsRequestType = "MembershipAdd";
            }
            else if (rbMembershipRemove.Checked)
            {
                UserId = txtUserId.Text;
                SpaceId = txtSpaceId.Text;
                ObjectsRequestType = "MembershipRemove";
            }
            else
            {
                ObjectsRequestType = "";
            }
        }
    }
}
