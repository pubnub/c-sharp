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
    internal class SetAuthEventArgs : EventArgs
    {
        internal string authKey;
    }

    public class AuthDialogFragment : Android.Support.V4.App.DialogFragment
    {
        public event EventHandler<EventArgs> SetAuth;

        Button btnDismiss;
        Button btnSet;
        View view;

        void ButtonSetClick (object sender, EventArgs e)
        {
            EditText txtauth = view.FindViewById<EditText> (Resource.Id.txtauth);

            FireEvent (txtauth.Text);

            Dismiss();
        }

        public void FireEvent(string authKey){
            if(SetAuth != null)
            {
                SetAuthEventArgs cea = new SetAuthEventArgs ();
                cea.authKey = authKey;
                SetAuth(this, cea);
            }
        }

        public override Android.Views.View OnCreateView(Android.Views.LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Android 3.x+ still wants to show title: disable
            Dialog.Window.RequestFeature(WindowFeatures.NoTitle);

            // Create our view
            view = inflater.Inflate(Resource.Layout.AuthKey, container, true);

            // Handle dismiss button click
            btnSet = view.FindViewById<Button>(Resource.Id.btnSet);
            btnSet.Click += ButtonSetClick;

            btnDismiss = view.FindViewById<Button>(Resource.Id.btnCancel);
            btnDismiss.Click += ButtonDismissClick;

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

