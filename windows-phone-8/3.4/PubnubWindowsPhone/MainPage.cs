using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PubnubWindowsPhone.Resources;
namespace PubnubWindowsPhone
{
    /// <summary>
    /// Partial class MainPage was created in order to avoid MainPage.xaml file copy conflicts for Nuget install
    /// </summary>
    public partial class MainPage : PhoneApplicationPage
    {
        // NOTE: IF you get error like "..already defines a member called 'OnNavigatedTo' with the same parameter types", 
        //       merge this method code to MainPage.xaml.cs file and remove MainPage.cs file.
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NavigationService.Navigate(new Uri("/PubnubDemoStart.xaml", UriKind.Relative));
        }

    }
}
