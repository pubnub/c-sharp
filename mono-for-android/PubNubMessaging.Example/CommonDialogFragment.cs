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
    internal class SetEventArgs : EventArgs
    {
        internal string valueToSet;
        internal string valueToSet2;
        internal string channel;
        internal CommonDialogStates cds;
    }

    public enum CommonDialogStates
    {
        HereNow,
        Auth,
        PresenceHeartbeat,
        PresenceHeartbeatInterval,
        AddUserStateKeyValue,
        DeleteUserState,
        SetUserStateJson,
        GetUserState,
        WhereNow,
        GlobalHereNow,
        ChangeUuid,
        Grant,
        AuditSubscribe,
        AuditPresence,
        RevokeSubscribe,
        RevokePresence
    }

    public class CommonDialogFragment : Android.Support.V4.App.DialogFragment
    {
        public event EventHandler<EventArgs> SetValues;

        Button btnDismiss;
        Button btnSet;
        Button btnSet2;
        TextView tvAuthLabel;
        TextView tvinput1, tvinput2;
        TextView lblInput0, lblInput2;
        TextView txtChannel;
        View view;

        CommonDialogStates cds;
        Context ctx;

        public CommonDialogFragment(CommonDialogStates cds, Context ctx){
            this.cds = cds;
            this.ctx = ctx;
        }

        void ShowAlert (string message)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this.ctx);
            builder.SetTitle(Android.Resource.String.DialogAlertTitle);
            builder.SetIcon(Android.Resource.Drawable.IcDialogAlert);
            builder.SetMessage(message);
            builder.SetPositiveButton("OK", (sender, e) =>
                {
                });

            builder.Show();
        }

        void ButtonSetClick (object sender, EventArgs e)
        {
            if ((cds == CommonDialogStates.Auth)
                || (cds == CommonDialogStates.ChangeUuid)
                || (cds == CommonDialogStates.WhereNow)
                || (cds == CommonDialogStates.AuditPresence)
                || (cds == CommonDialogStates.AuditSubscribe)
                || (cds == CommonDialogStates.RevokePresence) 
                || (cds == CommonDialogStates.RevokeSubscribe)
            ) {
                EditText txtauth = view.FindViewById<EditText> (Resource.Id.txtauth);

                FireEvent (txtauth.Text, "", "");

                Dismiss ();
            } else if (cds == CommonDialogStates.GetUserState) {
                EditText txtuuid = view.FindViewById<EditText> (Resource.Id.txtauth);
                EditText txtChannel = view.FindViewById<EditText> (Resource.Id.txtChannel);

                FireEvent (txtuuid.Text, txtChannel.Text, "");

                Dismiss ();
            } else if (cds == CommonDialogStates.DeleteUserState) {
                EditText txtkey = view.FindViewById<EditText> (Resource.Id.txtauth);
                EditText txtChannel = view.FindViewById<EditText> (Resource.Id.txtChannel);

                FireEvent (txtkey.Text, txtChannel.Text,"");

                Dismiss ();
            } else if (cds == CommonDialogStates.PresenceHeartbeatInterval) {
                EditText txtkey = view.FindViewById<EditText> (Resource.Id.txtauth);
                int iInterval;

                Int32.TryParse(txtkey.Text, out iInterval);
                if (iInterval == 0) {
                    ShowAlert ("Please enter an integer value.");
                } else {
                    FireEvent (txtkey.Text, "", "");
                    Dismiss ();
                }
            } else if (cds == CommonDialogStates.PresenceHeartbeat) {
                EditText txtkey = view.FindViewById<EditText> (Resource.Id.txtauth);
                int iHeartbeat;
                Int32.TryParse(txtkey.Text, out iHeartbeat);
                if (iHeartbeat == 0) {
                    ShowAlert ("Please enter an integer value.");
                } else {
                    FireEvent (txtkey.Text, "", "");
                    Dismiss ();
                }
            } else if (cds == CommonDialogStates.AddUserStateKeyValue){
                EditText txtkey = view.FindViewById<EditText> (Resource.Id.txtauth);
                EditText txtChannel = view.FindViewById<EditText> (Resource.Id.txtChannel);
                EditText txtInput2 = view.FindViewById<EditText> (Resource.Id.txtinput2);

                FireEvent (txtkey.Text, txtChannel.Text, txtInput2.Text);

                Dismiss ();

            } else if (cds == CommonDialogStates.SetUserStateJson){
                EditText txtkey = view.FindViewById<EditText> (Resource.Id.txtauth);
                EditText txtChannel = view.FindViewById<EditText> (Resource.Id.txtChannel);
                EditText txtInput2 = view.FindViewById<EditText> (Resource.Id.txtinput2);

                FireEvent (txtkey.Text, txtChannel.Text, txtInput2.Text);

                Dismiss ();
            } 

        }

        public void FireEvent(string valueToSet1, string channel, string valueToSet2){
            if(SetValues != null)
            {
                SetEventArgs cea = new SetEventArgs ();
                cea.valueToSet = valueToSet1;
                cea.valueToSet2 = valueToSet2;
                cea.cds = cds;
                cea.channel = channel;
                SetValues(this, cea);
            }
        }

        void ButtonSetClick2 (object sender, EventArgs e)
        {
            EditText txtauth = view.FindViewById<EditText> (Resource.Id.txtauth);

            FireEvent (txtauth.Text, "", "");

            Dismiss();
        }

        public override Android.Views.View OnCreateView(Android.Views.LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Android.Content.Res.Resources res = this.Resources;
            // Android 3.x+ still wants to show title: disable
            Dialog.Window.RequestFeature(WindowFeatures.NoTitle);

            // Create our view
            view = inflater.Inflate (Resource.Layout.AuthKey, container, true);

            if (cds == CommonDialogStates.Auth) {
                //auth
                lblInput0 = view.FindViewById<TextView> (Resource.Id.lblinput0);
                lblInput0.Visibility = ViewStates.Invisible;
                lblInput2 = view.FindViewById<TextView> (Resource.Id.lblinput2);
                lblInput2.Visibility = ViewStates.Invisible;
                tvinput2 = view.FindViewById<TextView> (Resource.Id.txtinput2);
                tvinput2.Visibility = ViewStates.Invisible;

                txtChannel = view.FindViewById<TextView> (Resource.Id.txtChannel);
                txtChannel.Visibility = ViewStates.Invisible;

                tvAuthLabel = view.FindViewById<TextView> (Resource.Id.tvAuthLabel);
                tvAuthLabel.SetText(Resource.String.auth);

                tvinput1 = view.FindViewById<TextView> (Resource.Id.lblinput1);
                tvinput1.SetText(Resource.String.authkey);

                // Handle dismiss button click
                btnSet = view.FindViewById<Button> (Resource.Id.btnSet);
                btnSet.Click += ButtonSetClick;

                btnSet2 = view.FindViewById<Button> (Resource.Id.btnSet2);
                btnSet2.Visibility = ViewStates.Invisible;

                btnDismiss = view.FindViewById<Button> (Resource.Id.btnCancel);
                btnDismiss.Click += ButtonDismissClick;
                //end auth
            } else if ((cds == CommonDialogStates.AuditPresence) ||
                (cds == CommonDialogStates.AuditSubscribe) ||
                (cds == CommonDialogStates.RevokePresence) ||
                (cds == CommonDialogStates.RevokeSubscribe))
            {
                //auth
                lblInput0 = view.FindViewById<TextView> (Resource.Id.lblinput0);
                lblInput0.Visibility = ViewStates.Invisible;
                lblInput2 = view.FindViewById<TextView> (Resource.Id.lblinput2);
                lblInput2.Visibility = ViewStates.Invisible;
                tvinput2 = view.FindViewById<TextView> (Resource.Id.txtinput2);
                tvinput2.Visibility = ViewStates.Invisible;

                txtChannel = view.FindViewById<TextView> (Resource.Id.txtChannel);
                txtChannel.Visibility = ViewStates.Invisible;

                tvAuthLabel = view.FindViewById<TextView> (Resource.Id.tvAuthLabel);
                tvAuthLabel.SetText(Resource.String.auth);

                tvinput1 = view.FindViewById<TextView> (Resource.Id.lblinput1);
                tvinput1.SetText(Resource.String.authopt);

                // Handle dismiss button click
                btnSet = view.FindViewById<Button> (Resource.Id.btnSet);
                btnSet.Click += ButtonSetClick;

                btnSet2 = view.FindViewById<Button> (Resource.Id.btnSet2);
                btnSet2.Visibility = ViewStates.Invisible;

                btnDismiss = view.FindViewById<Button> (Resource.Id.btnCancel);
                btnDismiss.Click += ButtonDismissClick;
                //end auth
            } else if (cds == CommonDialogStates.ChangeUuid){
                lblInput0 = view.FindViewById<TextView> (Resource.Id.lblinput0);
                lblInput0.Visibility = ViewStates.Invisible;
                txtChannel = view.FindViewById<TextView> (Resource.Id.txtChannel);
                txtChannel.Visibility = ViewStates.Invisible;
                lblInput2 = view.FindViewById<TextView> (Resource.Id.lblinput2);
                lblInput2.Visibility = ViewStates.Invisible;
                tvinput2 = view.FindViewById<TextView> (Resource.Id.txtinput2);
                tvinput2.Visibility = ViewStates.Invisible;


                tvAuthLabel = view.FindViewById<TextView> (Resource.Id.tvAuthLabel);
                tvAuthLabel.SetText(Resource.String.btnChangeUuid);

                tvinput1 = view.FindViewById<TextView> (Resource.Id.lblinput1);
                tvinput1.SetText(Resource.String.enterUuid);

                // Handle dismiss button click
                btnSet = view.FindViewById<Button> (Resource.Id.btnSet);
                btnSet.SetText(Resource.String.btnChangeUuid);
                btnSet.Click += ButtonSetClick;

                btnSet2 = view.FindViewById<Button> (Resource.Id.btnSet2);
                btnSet2.Visibility = ViewStates.Invisible;

                btnDismiss = view.FindViewById<Button> (Resource.Id.btnCancel);
                btnDismiss.Click += ButtonDismissClick;
            } else if (cds == CommonDialogStates.PresenceHeartbeat){
                lblInput0 = view.FindViewById<TextView> (Resource.Id.lblinput0);
                lblInput0.Visibility = ViewStates.Invisible;
                txtChannel = view.FindViewById<TextView> (Resource.Id.txtChannel);
                txtChannel.Visibility = ViewStates.Invisible;
                lblInput2 = view.FindViewById<TextView> (Resource.Id.lblinput2);
                lblInput2.Visibility = ViewStates.Invisible;
                tvinput2 = view.FindViewById<TextView> (Resource.Id.txtinput2);
                tvinput2.Visibility = ViewStates.Invisible;


                tvAuthLabel = view.FindViewById<TextView> (Resource.Id.tvAuthLabel);
                tvAuthLabel.SetText(Resource.String.enterHeartbeatTime);

                tvinput1 = view.FindViewById<TextView> (Resource.Id.lblinput1);
                tvinput1.SetText(Resource.String.enterHeartbeatTime);

                // Handle dismiss button click
                btnSet = view.FindViewById<Button> (Resource.Id.btnSet);
                btnSet.SetText(Resource.String.btnset);
                btnSet.Click += ButtonSetClick;

                btnSet2 = view.FindViewById<Button> (Resource.Id.btnSet2);
                btnSet2.Visibility = ViewStates.Invisible;

                btnDismiss = view.FindViewById<Button> (Resource.Id.btnCancel);
                btnDismiss.Click += ButtonDismissClick;
            } else if (cds == CommonDialogStates.PresenceHeartbeatInterval){
                lblInput0 = view.FindViewById<TextView> (Resource.Id.lblinput0);
                lblInput0.Visibility = ViewStates.Invisible;
                txtChannel = view.FindViewById<TextView> (Resource.Id.txtChannel);
                txtChannel.Visibility = ViewStates.Invisible;
                lblInput2 = view.FindViewById<TextView> (Resource.Id.lblinput2);
                lblInput2.Visibility = ViewStates.Invisible;
                tvinput2 = view.FindViewById<TextView> (Resource.Id.txtinput2);
                tvinput2.Visibility = ViewStates.Invisible;


                tvAuthLabel = view.FindViewById<TextView> (Resource.Id.tvAuthLabel);
                tvAuthLabel.SetText(Resource.String.enterHeartbeatInterval);

                tvinput1 = view.FindViewById<TextView> (Resource.Id.lblinput1);
                tvinput1.SetText(Resource.String.enterHeartbeatInterval);

                // Handle dismiss button click
                btnSet = view.FindViewById<Button> (Resource.Id.btnSet);
                btnSet.SetText(Resource.String.btnset);
                btnSet.Click += ButtonSetClick;

                btnSet2 = view.FindViewById<Button> (Resource.Id.btnSet2);
                btnSet2.Visibility = ViewStates.Invisible;

                btnDismiss = view.FindViewById<Button> (Resource.Id.btnCancel);
                btnDismiss.Click += ButtonDismissClick;
            } else if (cds == CommonDialogStates.DeleteUserState){
                tvAuthLabel = view.FindViewById<TextView> (Resource.Id.tvAuthLabel);
                tvAuthLabel.SetText(Resource.String.deleteUserState);
                lblInput2 = view.FindViewById<TextView> (Resource.Id.lblinput2);
                lblInput2.Visibility = ViewStates.Invisible;
                tvinput2 = view.FindViewById<TextView> (Resource.Id.txtinput2);
                tvinput2.Visibility = ViewStates.Invisible;


                tvinput1 = view.FindViewById<TextView> (Resource.Id.lblinput1);
                tvinput1.SetText(Resource.String.enterUserStateKey);

                // Handle dismiss button click
                btnSet = view.FindViewById<Button> (Resource.Id.btnSet);
                btnSet.SetText(Resource.String.btnDelUserStateAndExit);
                btnSet.Click += ButtonSetClick;

                btnSet2 = view.FindViewById<Button> (Resource.Id.btnSet2);
                btnSet2.SetText(Resource.String.btnDelUserStateAndMore);
                btnSet2.Visibility = ViewStates.Invisible;

                btnDismiss = view.FindViewById<Button> (Resource.Id.btnCancel);
                btnDismiss.Click += ButtonDismissClick;
            } else if (cds == CommonDialogStates.GetUserState){
                tvAuthLabel = view.FindViewById<TextView> (Resource.Id.tvAuthLabel);
                tvAuthLabel.SetText(Resource.String.btnGetUserState);
                lblInput2 = view.FindViewById<TextView> (Resource.Id.lblinput2);
                lblInput2.Visibility = ViewStates.Invisible;
                tvinput2 = view.FindViewById<TextView> (Resource.Id.txtinput2);
                tvinput2.Visibility = ViewStates.Invisible;


                tvinput1 = view.FindViewById<TextView> (Resource.Id.lblinput1);
                 
                tvinput1.Text = string.Format("{0} ({1})", res.GetString(Resource.String.uuid), res.GetString(Resource.String.optional));

                // Handle dismiss button click
                btnSet = view.FindViewById<Button> (Resource.Id.btnSet);
                btnSet.SetText(Resource.String.btnGetUserState);
                btnSet.Click += ButtonSetClick;

                btnSet2 = view.FindViewById<Button> (Resource.Id.btnSet2);
                btnSet2.Visibility = ViewStates.Invisible;

                btnDismiss = view.FindViewById<Button> (Resource.Id.btnCancel);
                btnDismiss.Click += ButtonDismissClick;
            } else if (cds == CommonDialogStates.WhereNow){
                lblInput0 = view.FindViewById<TextView> (Resource.Id.lblinput0);
                lblInput0.Visibility = ViewStates.Invisible;
                txtChannel = view.FindViewById<TextView> (Resource.Id.txtChannel);
                txtChannel.Visibility = ViewStates.Invisible;
                lblInput2 = view.FindViewById<TextView> (Resource.Id.lblinput2);
                lblInput2.Visibility = ViewStates.Invisible;
                tvinput2 = view.FindViewById<TextView> (Resource.Id.txtinput2);
                tvinput2.Visibility = ViewStates.Invisible;


                tvAuthLabel = view.FindViewById<TextView> (Resource.Id.tvAuthLabel);
                tvAuthLabel.SetText(Resource.String.btnWhereNow);

                tvinput1 = view.FindViewById<TextView> (Resource.Id.lblinput1);
                tvinput1.Text = string.Format("{0} ({1})", res.GetString(Resource.String.uuid), res.GetString(Resource.String.optional));

                // Handle dismiss button click
                btnSet = view.FindViewById<Button> (Resource.Id.btnSet);
                btnSet.SetText(Resource.String.btnWhereNow);
                btnSet.Click += ButtonSetClick;

                btnSet2 = view.FindViewById<Button> (Resource.Id.btnSet2);
                btnSet2.Visibility = ViewStates.Invisible;

                btnDismiss = view.FindViewById<Button> (Resource.Id.btnCancel);
                btnDismiss.Click += ButtonDismissClick;
            } else if (cds == CommonDialogStates.AddUserStateKeyValue){
                tvAuthLabel = view.FindViewById<TextView> (Resource.Id.tvAuthLabel);
                tvAuthLabel.SetText(Resource.String.addUserState);

                lblInput2 = view.FindViewById<TextView> (Resource.Id.lblinput2);
                lblInput2.SetText (Resource.String.enterUserStateValue);

                tvinput1 = view.FindViewById<TextView> (Resource.Id.lblinput1);
                tvinput1.SetText(Resource.String.enterUserStateKey);

                // Handle dismiss button click
                btnSet = view.FindViewById<Button> (Resource.Id.btnSet);
                btnSet.SetText(Resource.String.btnSaveUserStateAndExit);
                btnSet.Click += ButtonSetClick;

                btnSet2 = view.FindViewById<Button> (Resource.Id.btnSet2);
                btnSet2.SetText(Resource.String.btnDelUserStateAndMore);
                btnSet2.Visibility = ViewStates.Invisible;

                btnDismiss = view.FindViewById<Button> (Resource.Id.btnCancel);
                btnDismiss.Click += ButtonDismissClick;
            } else if (cds == CommonDialogStates.SetUserStateJson){
                tvAuthLabel = view.FindViewById<TextView> (Resource.Id.tvAuthLabel);
                tvAuthLabel.SetText(Resource.String.btnSetUserState);

                lblInput2 = view.FindViewById<TextView> (Resource.Id.lblinput2);
                lblInput2.SetText (Resource.String.btnUserStateInst);
                lblInput2.SetHeight(100);

                tvinput1 = view.FindViewById<TextView> (Resource.Id.lblinput1);
                tvinput1.SetText(Resource.String.enterUserStateKey);
                tvinput1.Text = string.Format("{0} ({1})", res.GetString(Resource.String.uuid), res.GetString(Resource.String.optional));

                // Handle dismiss button click
                btnSet = view.FindViewById<Button> (Resource.Id.btnSet);
                btnSet.SetText(Resource.String.btnSetUserState);
                btnSet.Click += ButtonSetClick;

                btnSet2 = view.FindViewById<Button> (Resource.Id.btnSet2);
                btnSet2.Visibility = ViewStates.Invisible;

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

