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
    internal class AddRemoveFromCgEventArgs : EventArgs
    {
        internal string channel;
        internal string channelGroup;
        internal CommonDialogStates cds;
    }

    public class AddRemoveFromChannelGroupDialogFragment: Android.Support.V4.App.DialogFragment
    {
        public event EventHandler<EventArgs> AddRemoveFromCgPerms;

        Button btnDismiss;
        Button btnSet;
        View view;

        CommonDialogStates cds;
        Context ctx;

        public AddRemoveFromChannelGroupDialogFragment (CommonDialogStates cds, Context context)
        {
            this.cds = cds;
            this.ctx = context;
        }

        void ButtonSetClick (object sender, EventArgs e)
        {
            if (cds == CommonDialogStates.AddToChannelGroup) {
                EditText txtChannel = view.FindViewById<EditText> (Resource.Id.txtChannel);
                EditText txtChannelGroup = view.FindViewById<EditText> (Resource.Id.txtChannelGroup);

                if (txtChannel.Text.Trim ().Length == 0 || txtChannelGroup.Text.Trim ().Length == 0) {
                    ShowAlert ("Please enter channel/channelgroup name");
                } else {
                    FireEvent (txtChannel.Text.Trim (), txtChannelGroup.Text.Trim ());

                    Dismiss ();
                }
            } else if (cds == CommonDialogStates.RemoveFromChannelGroup) {
                EditText txtChannel = view.FindViewById<EditText> (Resource.Id.txtChannel);
                EditText txtChannelGroup = view.FindViewById<EditText> (Resource.Id.txtChannelGroup);

                if (txtChannel.Text.Trim ().Length == 0 || txtChannelGroup.Text.Trim ().Length == 0) {
                    ShowAlert ("Please enter channel/channelgroup name");
                } else {
                    FireEvent (txtChannel.Text.Trim (), txtChannelGroup.Text.Trim ());

                    Dismiss ();
                }

            } else if (cds == CommonDialogStates.GetChannelGroup) {
                EditText txtChannelGroup = view.FindViewById<EditText> (Resource.Id.txtChannelGroup);

                if (txtChannelGroup.Text.Trim ().Length == 0) {
                    ShowAlert ("Please enter channelgroup name");
                } else {
                    FireEvent ("", txtChannelGroup.Text.Trim ());

                    Dismiss ();
                }

            }
        }

        public void FireEvent (string channel, string channelgroup)
        {
            if (AddRemoveFromCgPerms != null) {
                AddRemoveFromCgEventArgs cea = new AddRemoveFromCgEventArgs ();
                cea.channel = channel;
                cea.channelGroup = channelgroup;
                cea.cds = cds;
                AddRemoveFromCgPerms (this, cea);
            }
        }

        public override Android.Views.View OnCreateView (Android.Views.LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Android 3.x+ still wants to show title: disable
            Dialog.Window.RequestFeature (WindowFeatures.NoTitle);

            // Create our view
            view = inflater.Inflate (Resource.Layout.AddRemoveFromChannelGroup, container, true);

            if (cds == CommonDialogStates.AddToChannelGroup) {

                // Handle dismiss button click
                btnSet = view.FindViewById<Button> (Resource.Id.btnSet);
                btnSet.Click += ButtonSetClick;

                btnDismiss = view.FindViewById<Button> (Resource.Id.btnCancel);
                btnDismiss.Click += ButtonDismissClick;
            } else if (cds == CommonDialogStates.RemoveFromChannelGroup) {

                btnSet = view.FindViewById<Button> (Resource.Id.btnSet);
                btnSet.Click += ButtonSetClick;

                btnDismiss = view.FindViewById<Button> (Resource.Id.btnCancel);
                btnDismiss.Click += ButtonDismissClick;
            } else if (cds == CommonDialogStates.GetChannelGroup) {

                TextView tvChannel = view.FindViewById<TextView> (Resource.Id.lblChannel);
                tvChannel.Visibility = ViewStates.Gone;
                EditText txtChannel = view.FindViewById<EditText> (Resource.Id.txtChannel);
                txtChannel.Visibility = ViewStates.Gone;

                btnSet = view.FindViewById<Button> (Resource.Id.btnSet);
                btnSet.Click += ButtonSetClick;

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

