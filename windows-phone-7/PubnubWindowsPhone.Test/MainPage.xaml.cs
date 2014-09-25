using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Phone.Testing;
using Microsoft.Phone.Testing.Harness;
using System.IO.IsolatedStorage;
using System.IO;


namespace PubnubWindowsPhone.Test
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            const bool runUnitTests = true;

            if (runUnitTests)
            {
                UnitTestSettings settings = UnitTestSystem.CreateDefaultSettings();
#if AUTOUNITTEST
                settings.StartRunImmediately = true;
                settings.ShowTagExpressionEditor = false;
                settings.TestHarness.TestHarnessCompleted += TestRunCompletedCallback;
#endif


                Content = UnitTestSystem.CreateTestPage(settings);

            }

        }

        void TestRunCompletedCallback(object sender, TestHarnessCompletedEventArgs e)
        {
            int testsPassed = 0;
            int testsFailed = 0;
            int testsInconclusive = 0;
            UnitTestHarness testHarness = sender as UnitTestHarness;
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream stream = isoStore.OpenFile("pubnublog.txt", FileMode.OpenOrCreate))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        foreach (ScenarioResult result in testHarness.Results)
                        {
                            writer.WriteLine(result.ToString());
                            System.Diagnostics.Debug.WriteLine(result.ToString());
                            switch (result.Result)
                            {
                                case TestOutcome.Passed:
                                case TestOutcome.Completed:
                                    testsPassed++;
                                    break;
                                case TestOutcome.Inconclusive:
                                    testsInconclusive++;
                                    break;
                                default:
                                    testsFailed++;
                                    // must be a failure of some kind
                                    // perform some outputting
                                    break;
                            }
                        }
                        writer.WriteLine("Total Tests = " + testHarness.Results.Count);
                        writer.WriteLine("Tests Passed = " + testsPassed);
                        writer.WriteLine("Tests Failed = " + testsFailed);
                        writer.WriteLine("Tests Inconclusive = " + testsInconclusive);

                        System.Diagnostics.Debug.WriteLine("Total Tests = " + testHarness.Results.Count);
                        System.Diagnostics.Debug.WriteLine("Tests Passed = " + testsPassed);
                        System.Diagnostics.Debug.WriteLine("Tests Failed = " + testsFailed);
                        System.Diagnostics.Debug.WriteLine("Tests Inconclusive = " + testsInconclusive);

                        writer.Close();
                    }
                    stream.Close();
                }
            }
        }
    }


}