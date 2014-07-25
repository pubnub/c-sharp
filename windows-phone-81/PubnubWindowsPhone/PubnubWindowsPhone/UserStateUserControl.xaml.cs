using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PubnubWindowsPhone
{
    public sealed partial class UserStateUserControl : UserControl
    {
        public bool IsOKButtonEntered = false;
        public bool IsGetUserState = false;
        public bool IsSetUserState = false;

        public UserStateUserControl()
        {
            this.InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            IsOKButtonEntered = true;
            if (radGetUserState.IsChecked.Value)
            {
                IsGetUserState = true;
                IsSetUserState = false;
            }
            else if (radSetUserState.IsChecked.Value)
            {
                IsSetUserState = true;
                IsGetUserState = false;
            }
            ClosePopupWindow();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ClosePopupWindow();
        }

        private void ClosePopupWindow()
        {
            StackPanel sp = this.Parent as StackPanel;
            if (sp != null)
            {
                Border b = sp.Parent as Border;
                if (b != null)
                {
                    Popup popup = b.Parent as Popup;
                    popup.IsOpen = false;
                }
            }
        }

    }
}
