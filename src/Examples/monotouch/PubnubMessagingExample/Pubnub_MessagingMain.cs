
using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using MonoTouch.Dialog;
using PubnubApi;
using CoreGraphics;

namespace PubnubMessagingExample
{
    public partial class Pubnub_MessagingMain : DialogViewController
    {
        Proxy proxy = null;
        Pubnub pubnub = null;
        PNConfiguration config = null;

        public override void ViewDidAppear (bool animated)
        {
            AppDelegate.navigation.ToolbarHidden = true;
            base.ViewDidAppear (animated);
        }

        public Pubnub_MessagingMain () : base (UITableViewStyle.Grouped, null)
        {
            UIView labelView = new UIView (new CGRect (0, 0, this.View.Bounds.Width, 24));
            int left = 20;
            string hardwareVer = DeviceHardware.Version.ToString ().ToLower ();
            if (hardwareVer.IndexOf ("ipad") >= 0) {
                left = 55;
            }

            labelView.AddSubview (new UILabel (new CGRect (left, 10, this.View.Bounds.Width - left, 24)) {
                Font = UIFont.BoldSystemFontOfSize (16),
                BackgroundColor = UIColor.Clear,
                TextColor = UIColor.FromRGB (76, 86, 108),
                Text = "Basic Settings"
            });

            var headerMultipleChannels = new UILabel (new CGRect (0, 0, this.View.Bounds.Width, 30)) {
                Font = UIFont.SystemFontOfSize (12),
                TextColor = UIColor.Brown,
                BackgroundColor = UIColor.Clear,
                LineBreakMode = UILineBreakMode.WordWrap,
                Lines = 0,
                TextAlignment = UITextAlignment.Center
            };
            headerMultipleChannels.Text = "Enter multiple channel/channelgroup names separated by comma";

            EntryElement entrySubscribeKey = new EntryElement ("Subscribe Key", "Enter Subscribe Key", "demo");
            entrySubscribeKey.AutocapitalizationType = UITextAutocapitalizationType.None;
            entrySubscribeKey.AutocorrectionType = UITextAutocorrectionType.No;

            EntryElement entryPublishKey = new EntryElement ("Publish Key", "Enter Publish Key", "demo");
            entryPublishKey.AutocapitalizationType = UITextAutocapitalizationType.None;
            entryPublishKey.AutocorrectionType = UITextAutocorrectionType.No;

            EntryElement entrySecretKey = new EntryElement ("Secret Key", "Enter Secret Key", "demo");
            entrySecretKey.AutocapitalizationType = UITextAutocapitalizationType.None;
            entrySecretKey.AutocorrectionType = UITextAutocorrectionType.No;

            EntryElement entryChannelName = new EntryElement ("Channel(s)", "Enter Channel Name", "");
            entryChannelName.AutocapitalizationType = UITextAutocapitalizationType.None;
            entryChannelName.AutocorrectionType = UITextAutocorrectionType.No;

            EntryElement entryChannelGroupName = new EntryElement ("ChannelGroup(s)", "Enter ChannelGroup Name", "");
            entryChannelGroupName.AutocapitalizationType = UITextAutocapitalizationType.None;
            entryChannelGroupName.AutocorrectionType = UITextAutocorrectionType.No;

            EntryElement entryAuthKeyName = new EntryElement ("Auth Key(s)", "Enter Auth Key(optional)", "");
            entryAuthKeyName.AutocapitalizationType = UITextAutocapitalizationType.None;
            entryAuthKeyName.AutocorrectionType = UITextAutocorrectionType.No;

            EntryElement entryCipher = new EntryElement ("Cipher", "Enter Cipher", "");
            entryCipher.AutocapitalizationType = UITextAutocapitalizationType.None;
            entryCipher.AutocorrectionType = UITextAutocorrectionType.No;

            EntryElement entryProxyServer = new EntryElement ("Server", "Enter Server", "");
            entryProxyServer.AutocapitalizationType = UITextAutocapitalizationType.None;
            entryProxyServer.AutocorrectionType = UITextAutocorrectionType.No;

            EntryElement entryProxyPort = new EntryElement ("Port", "Enter Port", "");

            EntryElement entryProxyUser = new EntryElement ("Username", "Enter Username", "");
            entryProxyUser.AutocapitalizationType = UITextAutocapitalizationType.None;
            entryProxyUser.AutocorrectionType = UITextAutocorrectionType.No;

            EntryElement entryProxyPassword = new EntryElement ("Password", "Enter Password", "", true);

            EntryElement entryCustonUuid = new EntryElement ("CustomUuid", "Enter Custom UUID", "");
            entryCustonUuid.AutocapitalizationType = UITextAutocapitalizationType.None;
            entryCustonUuid.AutocorrectionType = UITextAutocorrectionType.No;

            BooleanElement proxyEnabled = new BooleanElement ("Proxy", false);

            BooleanElement sslEnabled = new BooleanElement ("Enable SSL", true);
            Root = new RootElement ("Pubnub Messaging") {
                new Section (labelView) {
                },
                new Section (headerMultipleChannels) {
                },
                new Section ("Enter Subscribe Key.") {
                    entrySubscribeKey
                },
                new Section ("Enter Publish key.") {
                    entryPublishKey
                },
                new Section ("Enter Secret key.") {
                    entrySecretKey
                },
                new Section () {
                    entryChannelName,
                    sslEnabled
                },
                new Section(){
                    entryChannelGroupName
                },
                new Section(){
                    entryAuthKeyName
                },
                new Section ("Enter cipher key for encryption. Leave blank for unencrypted transfer.") {
                    entryCipher
                },
                new Section ("Enter custom UUID or leave blank to use the default UUID") {
                    entryCustonUuid
                },
                new Section () {
                    new RootElement ("Proxy Settings", 0, 0) {
                        new Section () {
                            proxyEnabled
                        },
                        new Section ("Configuration") {
                            entryProxyServer,
                            entryProxyPort,
                            entryProxyUser,
                            entryProxyPassword
                        },
                    }
                },
                new Section () {
                    new StyledStringElement ("Launch Example", () => {
                        bool errorFree = true;
                        errorFree = ValidateAndInitPubnub (entryChannelName.Value, entryChannelGroupName.Value, entryCipher.Value, sslEnabled.Value, 
                            entryCustonUuid.Value, proxyEnabled.Value, entryProxyPort.Value,
                            entryProxyUser.Value, entryProxyServer.Value, entryProxyPassword.Value, 
                                                           entrySubscribeKey.Value, entryPublishKey.Value, entrySecretKey.Value, entryAuthKeyName.Value
                        );

                        if (errorFree) {
                            new Pubnub_MessagingSub (entryChannelName.Value, entryChannelGroupName.Value, entryCipher.Value, sslEnabled.Value, pubnub, config);
                        }
                    }) {
                        BackgroundColor = UIColor.Blue,
                        TextColor = UIColor.White,
                        Alignment = UITextAlignment.Center
                    },
                },
                /*new Section()
                {
                    new StyledStringElement ("Launch Speed Test", () => {
                        bool errorFree = true;
                        errorFree = ValidateAndInitPubnub(entryChannelName.Value, entryCipher.Value, sslEnabled.Value, 
                                                          entryCustonUuid.Value, proxyEnabled.Value, entryProxyPort.Value,
                                                          entryProxyUser.Value, entryProxyServer.Value, entryProxyPassword.Value
                                                          );
                        
                        if(errorFree)
                        {
                            new Pubnub_MessagingSpeedTest(entryChannelName.Value, entryCipher.Value, sslEnabled.Value, pubnub);
                        }
                    })
                    {
                        BackgroundColor = UIColor.Blue,
                        TextColor = UIColor.White,
                        Alignment = UITextAlignment.Center
                    },
                }*/
            };
        }

        bool ValidateAndInitPubnub (string channelName, string channelGroupName, string cipher, bool ssl, 
                                    string customUuid, bool proxyEnabled, string proxyPort,
                                    string proxyUser, string proxyServer, string proxyPass,
                                    string subscribeKey, string publishKey, string secretKey, string authKey
        )
        {
            bool errorFree = true;
            if (String.IsNullOrWhiteSpace (channelName) && String.IsNullOrWhiteSpace (channelGroupName)) {
                errorFree = false;
                new UIAlertView ("Error!", "Please enter either channel name or channelgroup name or both", null, "OK").Show (); 
            }
            

            if (errorFree) {
                config = new PNConfiguration ();
                config.PublishKey = publishKey;
                config.SubscribeKey = subscribeKey;
                config.SecretKey = secretKey;
                config.CipherKey = cipher;
                config.Secure = ssl;
                if (!String.IsNullOrWhiteSpace (customUuid.Trim ())) {
                    config.Uuid = customUuid.Trim ();
                }
                config.AuthKey = authKey;
            } 
            
            if ((errorFree) && (proxyEnabled)) {
                int port;
                if (Int32.TryParse (proxyPort, out port) && ((port >= 1) && (port <= 65535))) {
                    proxy = new Proxy (new Uri(string.Format("{0}:{1}",proxyServer, proxyPort)));
                    proxy.Credentials = new System.Net.NetworkCredential (proxyUser, proxyPass);

                    try {
                        config.Proxy = proxy;
                    } catch (MissingMemberException mse) {
                        errorFree = false;
                        proxyEnabled = false;
                        Console.WriteLine (mse.Message);
                        new UIAlertView ("Error!", "Proxy settings invalid, please re-enter the details.", null, "OK").Show (); 
                    }
                } else {
                    errorFree = false;
                    new UIAlertView ("Error!", "Proxy port must be a valid integer between 1 to 65535", null, "OK").Show (); 
                }
            }
            if (errorFree) {
                pubnub = new Pubnub (config);
            }
            return errorFree;
        }
    }
}
