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
        internal bool canWrite;
        internal bool canRead;
        internal bool isPresence;
    }

    public class GrantDialogFragment : Android.Support.V4.App.DialogFragment
    {
        public event EventHandler<EventArgs> GrantPerms;

        Button btnDismiss;
        Button btnGrant;
        View view;

        public bool IsPresenceGrant {
            get;
            set;
        }

        void ButtonGrantClick (object sender, EventArgs e)
        {
            ToggleButton tbCanWrite = view.FindViewById<ToggleButton> (Resource.Id.tbWrite);
            ToggleButton tbCanRead = view.FindViewById<ToggleButton> (Resource.Id.tbRead);
            EditText txtttl = view.FindViewById<EditText> (Resource.Id.txtttl);
            int iTtl;

            Int32.TryParse(txtttl.Text, out iTtl);
            if (iTtl == 0) 
            {
                iTtl = 1440;
                txtttl.Text = "1440";
            }

            FireEvent (iTtl, tbCanRead.Checked, tbCanWrite.Checked, IsPresenceGrant);

            Dismiss();
        }

        public void FireEvent(int iTtl, bool canRead, bool canWrite, bool isPresence){
            if(GrantPerms != null)
            {
                GrantEventArgs cea = new GrantEventArgs ();
                cea.canRead = canRead;
                cea.canWrite = canWrite;
                cea.ttl = iTtl;
                cea.isPresence = isPresence;
                GrantPerms(this, cea);
            }
        }

        public override Android.Views.View OnCreateView(Android.Views.LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Android 3.x+ still wants to show title: disable
            Dialog.Window.RequestFeature(WindowFeatures.NoTitle);

            // Create our view
            view = inflater.Inflate(Resource.Layout.GrantDialog, container, true);

            var tvGrantLabel = view.FindViewById<TextView>(Resource.Id.tvGrantLabel);
            tvGrantLabel.Text = "Subscribe Grant";
            if (IsPresenceGrant) 
            {
                tvGrantLabel.Text = "Presence Grant";
            }

            // Handle dismiss button click
            btnGrant = view.FindViewById<Button>(Resource.Id.btnGrant);
            btnGrant.Click += ButtonGrantClick;

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

