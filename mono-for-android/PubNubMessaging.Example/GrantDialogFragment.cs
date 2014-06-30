using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics.Drawables;
using Android.Graphics;

namespace PubNubMessaging.Example
{
    internal class GrantEventArgs : EventArgs
    {
        internal int ttl;
        internal string channel;
        internal bool valToSet1;
        internal bool valToSet2;
        internal bool isPresence;
        internal CommonDialogStates cds;
    }

    public class GrantDialogFragment : Android.Support.V4.App.DialogFragment
    {
        public event EventHandler<EventArgs> GrantPerms;

        Button btnDismiss;
        Button btnGrant;
        TextView lblInput1, lblInput2, lblInput3, lblAuth;
        ToggleButton tbRead;
        EditText txtVal, txtauth;
        View view;

        public bool IsPresenceGrant {
            get;
            set;
        }

        CommonDialogStates cds;

        public GrantDialogFragment(CommonDialogStates cds){
            this.cds = cds;
        }

        void ButtonGrantClick (object sender, EventArgs e)
        {
            if (cds == CommonDialogStates.Grant) {

                ToggleButton tbCanWrite = view.FindViewById<ToggleButton> (Resource.Id.tbWrite);
                ToggleButton tbCanRead = view.FindViewById<ToggleButton> (Resource.Id.tbRead);
                EditText txtttl = view.FindViewById<EditText> (Resource.Id.txtttl);
                EditText txtauth = view.FindViewById<EditText> (Resource.Id.txtauth);
                int iTtl;

                Int32.TryParse (txtttl.Text, out iTtl);
                if (iTtl == 0) {
                    iTtl = 1440;
                    txtttl.Text = "1440";
                }

                FireEvent (iTtl, tbCanRead.Checked, tbCanWrite.Checked, IsPresenceGrant, txtauth.Text);

                Dismiss ();
            } else if ((cds == CommonDialogStates.HereNow) 
                ||  (cds == CommonDialogStates.GlobalHereNow)) {
                ToggleButton tbCanWrite = view.FindViewById<ToggleButton> (Resource.Id.tbWrite);
                ToggleButton tbCanRead = view.FindViewById<ToggleButton> (Resource.Id.tbRead);
                EditText txtval = view.FindViewById<EditText> (Resource.Id.txtttl);

                FireEvent (0, tbCanRead.Checked, tbCanWrite.Checked, IsPresenceGrant, txtval.Text);

                Dismiss ();

            }
        }

        public void FireEvent(int iTtl, bool canRead, bool canWrite, bool isPresence, string channel){
            if(GrantPerms != null)
            {
                GrantEventArgs cea = new GrantEventArgs ();
                cea.valToSet2 = canRead;
                cea.valToSet1 = canWrite;
                cea.ttl = iTtl;
                cea.isPresence = isPresence;
                cea.cds = cds;
                cea.channel = channel;
                GrantPerms(this, cea);
            }
        }

        public override Android.Views.View OnCreateView(Android.Views.LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Android 3.x+ still wants to show title: disable
            Dialog.Window.RequestFeature(WindowFeatures.NoTitle);

            // Create our view
            view = inflater.Inflate(Resource.Layout.GrantDialog, container, true);

            if (cds == CommonDialogStates.Grant) {

                var tvGrantLabel = view.FindViewById<TextView> (Resource.Id.tvGrantLabel);
                tvGrantLabel.Text = "Subscribe Grant";
                if (IsPresenceGrant) {
                    tvGrantLabel.Text = "Presence Grant";
                }

                // Handle dismiss button click
                btnGrant = view.FindViewById<Button> (Resource.Id.btnGrant);
                btnGrant.Click += ButtonGrantClick;

                btnDismiss = view.FindViewById<Button> (Resource.Id.btnCancel);
                btnDismiss.Click += ButtonDismissClick;
            } else if (cds == CommonDialogStates.HereNow) {
                var tvGrantLabel = view.FindViewById<TextView> (Resource.Id.tvGrantLabel);
                tvGrantLabel.SetText(Resource.String.hereNow);

                lblInput1 = view.FindViewById<TextView> (Resource.Id.lblGrant1);
                lblInput1.SetText (Resource.String.showUuid);

                lblInput2 = view.FindViewById<TextView> (Resource.Id.lblGrant2);
                lblInput2.SetText (Resource.String.showUserState);

                lblInput3 = view.FindViewById<TextView> (Resource.Id.lblGrant3);
                lblInput3.SetText (Resource.String.channel);

                txtVal = view.FindViewById<EditText> (Resource.Id.txtttl);
                txtVal.InputType = Android.Text.InputTypes.TextFlagAutoComplete;

                txtauth = view.FindViewById<EditText> (Resource.Id.txtauth);
                txtauth.Visibility = ViewStates.Invisible;

                lblAuth = view.FindViewById<TextView> (Resource.Id.lblinput1);
                lblAuth.Visibility = ViewStates.Invisible;

                tbRead = view.FindViewById<ToggleButton> (Resource.Id.tbRead);
                tbRead.Checked = true;

                // Handle dismiss button click
                btnGrant = view.FindViewById<Button> (Resource.Id.btnGrant);
                btnGrant.SetText (Resource.String.hereNow);
                btnGrant.Click += ButtonGrantClick;

                btnDismiss = view.FindViewById<Button> (Resource.Id.btnCancel);
                btnDismiss.Click += ButtonDismissClick;
            } else if (cds == CommonDialogStates.GlobalHereNow) {
                var tvGrantLabel = view.FindViewById<TextView> (Resource.Id.tvGrantLabel);
                tvGrantLabel.SetText(Resource.String.globalHereNow);

                lblInput1 = view.FindViewById<TextView> (Resource.Id.lblGrant1);
                lblInput1.SetText (Resource.String.showUuid);

                lblInput2 = view.FindViewById<TextView> (Resource.Id.lblGrant2);
                lblInput2.SetText (Resource.String.showUserState);

                lblInput3 = view.FindViewById<TextView> (Resource.Id.lblGrant3);
                lblInput3.Visibility = ViewStates.Invisible;

                txtVal = view.FindViewById<EditText> (Resource.Id.txtttl);
                txtVal.Visibility = ViewStates.Invisible;

                txtauth = view.FindViewById<EditText> (Resource.Id.txtauth);
                txtauth.Visibility = ViewStates.Invisible;

                lblAuth = view.FindViewById<TextView> (Resource.Id.lblinput1);
                lblAuth.Visibility = ViewStates.Invisible;

                tbRead = view.FindViewById<ToggleButton> (Resource.Id.tbRead);
                tbRead.Checked = true;

                // Handle dismiss button click
                btnGrant = view.FindViewById<Button> (Resource.Id.btnGrant);
                btnGrant.SetText (Resource.String.globalHereNow);
                btnGrant.Click += ButtonGrantClick;

                btnDismiss = view.FindViewById<Button> (Resource.Id.btnCancel);
                btnDismiss.Click += ButtonDismissClick;
            }

            return view;
        }

        public override void OnResume()
        {
            // Auto size the dialog based on it's contents
            Dialog.Window.SetLayout(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent);
            
            // Make sure there is no background behind our view
            Dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
            
            // Disable standard dialog styling/frame/theme: our custom view should create full UI
            SetStyle(Android.Support.V4.App.DialogFragment.StyleNoFrame, Android.Resource.Style.Theme);
            
            base.OnResume();
        }

        private void ButtonDismissClick (object sender, EventArgs e)
        {
            Dismiss();
        }
        
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            // Unwire event
            if (disposing)
                btnDismiss.Click -= ButtonDismissClick;
        }
        
    }
}

