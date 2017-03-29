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
        internal string message;
        internal bool valToSet1;
        internal bool valToSet2;
        internal bool isPresence;
        internal string authKey;
        internal CommonDialogStates cds;
    }

    public class GrantDialogFragment : Android.Support.V4.App.DialogFragment
    {
        public event EventHandler<EventArgs> GrantPerms;

        Button btnDismiss;
        Button btnGrant;
        TextView lblInput1, lblInput2, lblInput3, lblAuth;
        ToggleButton tbRead, tbWrite;
        EditText txtVal, txtauth; //, txtChannel, txtChannelGroup;
        View view;

        public bool IsPresenceGrant {
            get;
            set;
        }

        CommonDialogStates cds;
        Context ctx;

        public GrantDialogFragment (CommonDialogStates cds, Context context)
        {
            this.cds = cds;
            this.ctx = context;
        }

        void ButtonGrantClick (object sender, EventArgs e)
        {
            if (cds == CommonDialogStates.Grant) {
                EditText txtChannel = view.FindViewById<EditText> (Resource.Id.txtChannel);
                ToggleButton tbCanWrite = view.FindViewById<ToggleButton> (Resource.Id.tbWrite);
                ToggleButton tbCanRead = view.FindViewById<ToggleButton> (Resource.Id.tbRead);
                EditText txtttl = view.FindViewById<EditText> (Resource.Id.txtttl);
                EditText txtauth = view.FindViewById<EditText> (Resource.Id.txtauth);
                int iTtl;

                Int32.TryParse (txtttl.Text, out iTtl);
                if (iTtl < 0) {
                    iTtl = 1440;
                    txtttl.Text = "1440";
                }

                if (txtChannel.Text.Trim ().Length == 0) {
                    ShowAlert ("Please enter channel name");
                } else {
                    FireEvent (iTtl, tbCanRead.Checked, tbCanWrite.Checked, IsPresenceGrant, txtChannel.Text, txtauth.Text, "");

                    Dismiss ();
                }
            } else if ((cds == CommonDialogStates.HereNow)
                || (cds == CommonDialogStates.GlobalHereNow)) {
                EditText txtChannel = view.FindViewById<EditText> (Resource.Id.txtChannel);
                ToggleButton tbIncludeUserState = view.FindViewById<ToggleButton> (Resource.Id.tbWrite);
                ToggleButton tbShowUUID = view.FindViewById<ToggleButton> (Resource.Id.tbRead);

                if (txtChannel.Text.Trim ().Length == 0) {
                    ShowAlert ("Please enter channel name");
                } else {
                    HereNowFireEvent (txtChannel.Text, tbShowUUID.Checked, tbIncludeUserState.Checked);
                    Dismiss ();
                }
            } else if (cds == CommonDialogStates.Publish) {
                ToggleButton tbStore = view.FindViewById<ToggleButton> (Resource.Id.tbRead);
                EditText txtChannel = view.FindViewById<EditText> (Resource.Id.txtChannel);
                EditText txtMessage = view.FindViewById<EditText> (Resource.Id.txtttl);

                if (txtChannel.Text.Trim ().Length == 0) {
                    ShowAlert ("Please enter channel name");
                } else {
                    PublishFireEvent (txtChannel.Text, txtMessage.Text, tbStore.Checked);
                    Dismiss ();
                }
            }
        }

        public void FireEvent (int iTtl, bool canRead, bool canWrite, bool isPresence, string channel, string authKey, string message)
        {
            if (GrantPerms != null) {
                GrantEventArgs cea = new GrantEventArgs ();
                cea.valToSet2 = canRead;
                cea.valToSet1 = canWrite;
                cea.ttl = iTtl;
                cea.isPresence = isPresence;
                cea.cds = cds;
                cea.channel = channel;
                cea.authKey = authKey;
                cea.message = message;
                GrantPerms (this, cea);
            }
        }

        public void PublishFireEvent (string channel, string message, bool showUUID)
        {
            if (GrantPerms != null) {
                GrantEventArgs cea = new GrantEventArgs ();
                cea.valToSet2 = showUUID;
                cea.cds = cds;
                cea.channel = channel;
                cea.message = message;
                GrantPerms (this, cea);
            }
        }

        public void HereNowFireEvent (string channel, bool showUUID, bool includeUserState)
        {
            if (GrantPerms != null) {
                GrantEventArgs cea = new GrantEventArgs ();
                cea.valToSet2 = showUUID;
                cea.valToSet1 = includeUserState;
                cea.cds = cds;
                cea.channel = channel;
                GrantPerms (this, cea);
            }
        }

        public override Android.Views.View OnCreateView (Android.Views.LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Android 3.x+ still wants to show title: disable
            Dialog.Window.RequestFeature (WindowFeatures.NoTitle);

            // Create our view
            view = inflater.Inflate (Resource.Layout.GrantDialog, container, true);

            if (cds == CommonDialogStates.Grant) {

                var tvGrantLabel = view.FindViewById<TextView> (Resource.Id.tvGrantLabel);
                tvGrantLabel.Text = "Grant Access";
                if (IsPresenceGrant) {
                    tvGrantLabel.Text = "Presence Grant Access";
                }
                TextView tvGrantChannelGroup = view.FindViewById<TextView> (Resource.Id.lblGrantChannelGroup);
                tvGrantChannelGroup.Visibility = ViewStates.Gone;
                EditText txtChannelGroup = view.FindViewById<EditText> (Resource.Id.txtChannelGroup);
                txtChannelGroup.Visibility = ViewStates.Gone;


                // Handle dismiss button click
                btnGrant = view.FindViewById<Button> (Resource.Id.btnGrant);
                btnGrant.Click += ButtonGrantClick;

                btnDismiss = view.FindViewById<Button> (Resource.Id.btnCancel);
                btnDismiss.Click += ButtonDismissClick;
            } else if (cds == CommonDialogStates.HereNow) {
                var tvGrantLabel = view.FindViewById<TextView> (Resource.Id.tvGrantLabel);
                tvGrantLabel.SetText (Resource.String.hereNow);

                lblInput1 = view.FindViewById<TextView> (Resource.Id.lblGrant1);
                lblInput1.SetText (Resource.String.showUuid);

                lblInput2 = view.FindViewById<TextView> (Resource.Id.lblGrant2);
                lblInput2.SetText (Resource.String.showUserState);

                lblInput3 = view.FindViewById<TextView> (Resource.Id.lblGrant3);
                lblInput3.Visibility = ViewStates.Gone;
                //lblInput3.SetText (Resource.String.channel);

                txtVal = view.FindViewById<EditText> (Resource.Id.txtttl);
                txtVal.Visibility = ViewStates.Gone;
                //txtVal.InputType = Android.Text.InputTypes.TextFlagAutoComplete;

                txtauth = view.FindViewById<EditText> (Resource.Id.txtauth);
                txtauth.Visibility = ViewStates.Invisible;

                lblAuth = view.FindViewById<TextView> (Resource.Id.lblinput1);
                lblAuth.Visibility = ViewStates.Invisible;

                tbRead = view.FindViewById<ToggleButton> (Resource.Id.tbRead);
                tbRead.Checked = true;

                TextView tvGrantChannelGroup = view.FindViewById<TextView> (Resource.Id.lblGrantChannelGroup);
                tvGrantChannelGroup.Visibility = ViewStates.Gone;
                EditText txtChannelGroup = view.FindViewById<EditText> (Resource.Id.txtChannelGroup);
                txtChannelGroup.Visibility = ViewStates.Gone;

                // Handle dismiss button click
                btnGrant = view.FindViewById<Button> (Resource.Id.btnGrant);
                btnGrant.SetText (Resource.String.hereNow);
                btnGrant.Click += ButtonGrantClick;

                btnDismiss = view.FindViewById<Button> (Resource.Id.btnCancel);
                btnDismiss.Click += ButtonDismissClick;
            } else if (cds == CommonDialogStates.Publish) {
                var tvGrantLabel = view.FindViewById<TextView> (Resource.Id.tvGrantLabel);
                tvGrantLabel.SetText (Resource.String.publish);

                lblInput1 = view.FindViewById<TextView> (Resource.Id.lblGrant1);
                lblInput1.SetText (Resource.String.inStore);

                lblInput3 = view.FindViewById<TextView> (Resource.Id.lblGrant3);
                lblInput3.SetText (Resource.String.message);

                tbRead = view.FindViewById<ToggleButton> (Resource.Id.tbRead);
                tbRead.Checked = true;

                lblInput2 = view.FindViewById<TextView> (Resource.Id.lblGrant2);
                lblInput2.Visibility = ViewStates.Invisible;

                tbWrite = view.FindViewById<ToggleButton> (Resource.Id.tbWrite);
                tbWrite.Visibility = ViewStates.Invisible;

                lblAuth = view.FindViewById<TextView> (Resource.Id.lblinput1);
                lblAuth.Visibility = ViewStates.Gone;
                txtauth = view.FindViewById<EditText> (Resource.Id.txtauth);
                txtauth.Visibility = ViewStates.Gone;

                //lblAuth.SetText (Resource.String.channel);

                txtVal = view.FindViewById<EditText> (Resource.Id.txtttl);
                txtVal.InputType = Android.Text.InputTypes.TextFlagAutoComplete;

                TextView tvGrantChannelGroup = view.FindViewById<TextView> (Resource.Id.lblGrantChannelGroup);
                tvGrantChannelGroup.Visibility = ViewStates.Gone;
                EditText txtChannelGroup = view.FindViewById<EditText> (Resource.Id.txtChannelGroup);
                txtChannelGroup.Visibility = ViewStates.Gone;

                // Handle dismiss button click
                btnGrant = view.FindViewById<Button> (Resource.Id.btnGrant);
                btnGrant.SetText (Resource.String.publish);
                btnGrant.Click += ButtonGrantClick;

                btnDismiss = view.FindViewById<Button> (Resource.Id.btnCancel);
                btnDismiss.Click += ButtonDismissClick;
            } else if (cds == CommonDialogStates.GlobalHereNow) {
                var tvGrantLabel = view.FindViewById<TextView> (Resource.Id.tvGrantLabel);
                tvGrantLabel.SetText (Resource.String.globalHereNow);

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

                TextView tvGrantChannelGroup = view.FindViewById<TextView> (Resource.Id.lblGrantChannelGroup);
                tvGrantChannelGroup.Visibility = ViewStates.Gone;
                EditText txtChannelGroup = view.FindViewById<EditText> (Resource.Id.txtChannelGroup);
                txtChannelGroup.Visibility = ViewStates.Gone;

                // Handle dismiss button click
                btnGrant = view.FindViewById<Button> (Resource.Id.btnGrant);
                btnGrant.SetText (Resource.String.globalHereNow);
                btnGrant.Click += ButtonGrantClick;

                btnDismiss = view.FindViewById<Button> (Resource.Id.btnCancel);
                btnDismiss.Click += ButtonDismissClick;
            }

            return view;
        }

        public override void OnResume ()
        {
            // Auto size the dialog based on it's contents
            Dialog.Window.SetLayout (LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent);
            
            // Make sure there is no background behind our view
            Dialog.Window.SetBackgroundDrawable (new ColorDrawable (Color.Transparent));
            
            // Disable standard dialog styling/frame/theme: our custom view should create full UI
            SetStyle (Android.Support.V4.App.DialogFragment.StyleNoFrame, Android.Resource.Style.Theme);
            
            base.OnResume ();
        }

        private void ButtonDismissClick (object sender, EventArgs e)
        {
            Dismiss ();
        }

        protected override void Dispose (bool disposing)
        {
            base.Dispose (disposing);

            // Unwire event
            if (disposing)
                btnDismiss.Click -= ButtonDismissClick;
        }
     
        void ShowAlert (string message)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder (this.ctx);
            builder.SetTitle (Android.Resource.String.DialogAlertTitle);
            builder.SetIcon (Android.Resource.Drawable.IcDialogAlert);
            builder.SetMessage (message);
            builder.SetPositiveButton ("OK", (sender, e) => {
            });

            builder.Show ();
        }
    }
}

