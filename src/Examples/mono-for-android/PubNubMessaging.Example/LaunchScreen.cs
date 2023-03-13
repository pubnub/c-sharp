
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
using PubnubApi;
using Android.Content.PM;

namespace PubNubMessaging.Example
{
    [Activity (Label = "PubNubMessaging", MainLauncher = true, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden,
        ScreenOrientation = ScreenOrientation.Portrait)]            
    public class LaunchScreen : Activity
    {
        Dialog dialog;
        ToggleButton tgProxy;
        bool proxyEnabled;
        TextView tvProxy;

        EditText proxyUsername;
        EditText proxyPassword;
        EditText proxyServer;
        EditText proxyPort;

        EditText txtSubKey;
        EditText txtPubKey;
        EditText txtSecKey;

        Proxy proxy = null;
        internal static Pubnub pubnub { get; set; }

        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);

            SetContentView (Resource.Layout.Launch);

            Button btnLaunch = FindViewById<Button> (Resource.Id.btnLaunch);
            btnLaunch.Click += LaunchClick;

            Button btnProxy = FindViewById<Button> (Resource.Id.btnProxy);
            btnProxy.Click += ProxySettingsHandler;

            tvProxy = FindViewById<TextView> (Resource.Id.tvProxy);
            tvProxy.Text = SetProxyText (false);

            txtSubKey = FindViewById<EditText> (Resource.Id.txtSubKey);
            txtSubKey.Text = "demo";
            txtPubKey = FindViewById<EditText> (Resource.Id.txtPubKey);
            txtPubKey.Text = "demo";
            txtSecKey = FindViewById<EditText> (Resource.Id.txtSecKey);
            txtSecKey.Text = "demo";
        }

        string SetProxyText (bool on)
        {
            if (on) {
                return String.Format ("{0} {1}", Resources.GetString (Resource.String.proxy), Resources.GetString (Resource.String.proxyOn)); 
            } else {
                return String.Format ("{0} {1}", Resources.GetString (Resource.String.proxy), Resources.GetString (Resource.String.proxyOff)); 
            }
        }

        void ProxySettingsHandler (object sender, EventArgs e)
        {
            ShowProxySettings ();
        }

        void ShowProxySettings ()
        {
            dialog = new Dialog (this);
            dialog.SetContentView (Resource.Layout.Proxy);
            dialog.SetTitle ("Proxy Settings"); 
            dialog.SetCancelable (true);

            dialog.CancelEvent += DialogDismissHandler;

            Button btnProxySave = (Button)dialog.FindViewById (Resource.Id.btnProxySave);
            Button btnProxyCancel = (Button)dialog.FindViewById (Resource.Id.btnProxyCancel);

            btnProxySave.Click += EnableProxy; 
            btnProxyCancel.Click += DisableProxy;
            
            proxyUsername = (EditText)dialog.FindViewById (Resource.Id.proxyUsername);
            proxyPassword = (EditText)dialog.FindViewById (Resource.Id.proxyPassword);
            proxyServer = (EditText)dialog.FindViewById (Resource.Id.proxyServer);
            proxyPort = (EditText)dialog.FindViewById (Resource.Id.proxyPort);
            
            tgProxy = (ToggleButton)dialog.FindViewById (Resource.Id.tbProxy);
            tgProxy.CheckedChange += ProxyCheckedChanged;
            tgProxy.Checked = true;

            if (proxy != null) {
                tgProxy.Checked = true;
                tvProxy.Text = SetProxyText (true);
            } else {
                tgProxy.Checked = false;

                tvProxy.Text = SetProxyText (false);
            }

            dialog.Show ();
        }

        void DialogDismissHandler (object sender, EventArgs e)
        {
            /* leave it empty */
        }

        void ProxyCheckedChanged (object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (e.IsChecked) {
                proxyUsername.Enabled = true;
                proxyPassword.Enabled = true;
                proxyServer.Enabled = true;
                proxyPort.Enabled = true;
            } else {
                proxyUsername.Enabled = false;
                proxyUsername.Text = "";
                proxyPassword.Enabled = false;
                proxyPassword.Text = "";
                proxyServer.Enabled = false;
                proxyServer.Text = "";
                proxyPort.Enabled = false;
                proxyPort.Text = "";
                proxy = null;
            }
        }

        void DisableProxy (object sender, EventArgs e)
        {
            if (!tgProxy.Checked) {
                proxyEnabled = false;
                tvProxy.Text = SetProxyText (false);
            } 
            dialog.Dismiss ();
        }

        void EnableProxy (object sender, EventArgs e)
        {
            int port;

            bool errorFree = true;

            if (!tgProxy.Checked) {
                proxyEnabled = false;
                tvProxy.Text = SetProxyText (false);
                dialog.Dismiss ();
            } else {
                if (string.IsNullOrWhiteSpace (proxyServer.Text)) {
                    errorFree = false;
                    ShowAlert ("Please enter proxy server."); 
                }

                if ((errorFree) && (string.IsNullOrWhiteSpace (proxyUsername.Text))) {
                    errorFree = false;
                    ShowAlert ("Please enter proxy username."); 
                }

                if ((errorFree) && (string.IsNullOrWhiteSpace (proxyPassword.Text))) {
                    errorFree = false;
                    ShowAlert ("Please enter proxy password."); 
                }

                if (errorFree) {
                    if (Int32.TryParse (proxyPort.Text, out port) && ((port >= 1) && (port <= 65535))) {
                        proxy = new Proxy (new Uri (string.Format ("{0}:{1}", proxyServer.Text, proxyPort.Text)));
                        proxy.Credentials = new System.Net.NetworkCredential (proxyUsername.Text, proxyPassword.Text);
                        proxyEnabled = true;
                        tvProxy.Text = SetProxyText (true);
                        dialog.Dismiss ();
                    } else {
                        ShowAlert ("Proxy port must be a valid integer between 1 to 65535"); 
                    }
                }
            }
        }

        void LaunchClick (object sender, EventArgs e)
        {
            EditText txtChannel = FindViewById<EditText> (Resource.Id.txtChannel);
            EditText txtChannelGroup = FindViewById<EditText> (Resource.Id.txtChannelGroup);
            EditText txtCustomUuid = FindViewById<EditText>(Resource.Id.txtCustomUuid);
            if (String.IsNullOrWhiteSpace (txtChannel.Text.Trim ()) && String.IsNullOrWhiteSpace (txtChannelGroup.Text.Trim ())) {
                ShowAlert ("Please enter a channel name or channelgroup or both");
            }
            else if (String.IsNullOrWhiteSpace(txtCustomUuid.Text.Trim()))
            {
                ShowAlert("Please enter UUID");
            }
            else {

                ToggleButton tbSsl = FindViewById<ToggleButton> (Resource.Id.tbSsl);
                EditText txtCipher = FindViewById<EditText> (Resource.Id.txtCipher);

                var mainActivity = new Intent (this, typeof(MainActivity));

                mainActivity.PutExtra ("Channel", txtChannel.Text.Trim ());
                mainActivity.PutExtra ("ChannelGroup", txtChannelGroup.Text.Trim ());

                if (tbSsl.Checked) {
                    mainActivity.PutExtra ("SslOn", "true");
                } else {
                    mainActivity.PutExtra ("SslOn", "false");
                }

                mainActivity.PutExtra ("Cipher", txtCipher.Text.Trim ());
                PNConfiguration config = new PNConfiguration (txtCustomUuid.Text);
                config.PublishKey = txtPubKey.Text.Trim ();
                config.SubscribeKey = txtSubKey.Text.Trim ();
                config.SecretKey = txtSecKey.Text.Trim ();
                config.CipherKey = txtCipher.Text.Trim ();
                config.Secure = tbSsl.Checked;

                if (!String.IsNullOrWhiteSpace (txtCustomUuid.Text.Trim ())) {
                    config.Uuid = txtCustomUuid.Text.Trim ();
                }

                bool errorFree = true;

                if (proxyEnabled) {
                    try {
                        config.Proxy = proxy;
                    } catch (MissingMemberException mse) {
                        errorFree = false;
                        System.Diagnostics.Debug.WriteLine (mse.Message);

                        ShowAlert ("Proxy settings invalid, please re-enter the details."); 
                    }
                }

                pubnub = new Pubnub (config);

                if (errorFree) {
                    StartActivity (mainActivity);
                }
            }
        }

        void ShowAlert (string message)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder (this);
            builder.SetTitle (Android.Resource.String.DialogAlertTitle);
            builder.SetIcon (Android.Resource.Drawable.IcDialogAlert);
            builder.SetMessage (message);
            builder.SetPositiveButton ("OK", (sender, e) => {
            });
            
            builder.Show ();
        }
    }
}

