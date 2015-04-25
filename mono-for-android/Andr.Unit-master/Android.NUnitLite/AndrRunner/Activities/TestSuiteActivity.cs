//
// Copyright 2011-2012 Xamarin Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

using MonoDroid.Dialog;
using NUnitLite;
using System.Collections.Generic;

namespace Android.NUnitLite.UI {

    [Activity (Label = "Tests")]            
    public class TestSuiteActivity : Activity {
        
        string test_suite;
        TestSuite suite;
        Section main;
        
        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);

            test_suite = Intent.GetStringExtra ("TestSuite");
            suite = AndroidRunner.Suites [test_suite];

            var menu = new RootElement (String.Empty);
            
            main = new Section (test_suite);
            foreach (ITest test in suite.Tests) {
                TestSuite ts = test as TestSuite;
                if (ts != null)
                    main.Add (new TestSuiteElement (ts));
                else
                    main.Add (new TestCaseElement (test as TestCase));
            }
            menu.Add (main);

            Section options = new Section () {
                new ActionElement ("Run all", Run),
            };
            menu.Add (options);

            var da = new DialogAdapter (this, menu);
            var lv = new ListView (this) {
                Adapter = da
            };
            SetContentView (lv);
        }
        
        public void Run ()
        {
            Dictionary<string,string> testCases = new Dictionary<string, string>();
            int successCount = 0;
            int failureCount = 0;
            int errorCount = 0;
            int noRunCount = 0;

            AndroidRunner runner = AndroidRunner.Runner;
            if (!runner.OpenWriter ("Run " + test_suite, this))
                return;
            
            try {
                foreach (ITest test in suite.Tests) {
                    TestResult result = test.Run (runner);
                    testCases.Add(test.FullName, result.ResultState.ToString());
                    switch(result.ResultState)
                    {
                    case ResultState.Success:
                        successCount++;
                        break;
                    case ResultState.Failure:
                        failureCount++;
                        break;
                    case ResultState.Error:
                        errorCount++;
                        break;
                    case ResultState.NotRun:
                        noRunCount++;
                        break;
                    }
                }
            }
            finally {
                runner.CloseWriter ();
            }
            
            foreach (TestElement te in main) {
                te.Update ();
            }

            foreach (string key in testCases.Keys) {
                Console.WriteLine ("{0} : {1}", key, testCases [key]);
            }
            Console.WriteLine ("TestCaseCount = {0}", suite.TestCaseCount);
            Console.WriteLine ("Success Count = {0}", successCount);
            Console.WriteLine ("Fail Count = {0}", failureCount);
            Console.WriteLine ("Error Count = {0}", errorCount);
            Console.WriteLine ("Not Run count = {0}", noRunCount);
        }
    }
}