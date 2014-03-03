using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Silverlight.Testing;

namespace PubnubSilverlight
{
    public class Instance
    {
        public static UIElement GetPage
        {
            get
            {
                UnitTestSettings settings = UnitTestSystem.CreateDefaultSettings();
                settings.TestService = null;
                
                settings.ShowTagExpressionEditor = false;
                settings.StartRunImmediately = true;
                UIElement page = UnitTestSystem.CreateTestPage(settings);
                return page;
            }
        }
    }
}
