// *****************************************************
// Copyright 2007, Charlie Poole
//
// Licensed under the Open Software License version 3.0
// *****************************************************

using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;

namespace NUnitLite
{
    public class TestSuite : ITest
    {
        #region Instance Variables
        private string name;
        private string fullName;

        private RunState runState = RunState.Runnable;
        private string ignoreReason;

        private IDictionary properties = new Hashtable();

        private ArrayList tests = new ArrayList(10);
        #endregion

        #region Constructors
        public TestSuite(string name)
        {
            this.name = name;
        }

        public TestSuite(Type type)
        {
            this.name = type.Name;
            this.fullName = type.FullName;

            object[] attrs = type.GetCustomAttributes( typeof(PropertyAttribute), true);
            foreach (PropertyAttribute attr in attrs)
                this.Properties[attr.Name] = attr.Value;

            IgnoreAttribute ignore = (IgnoreAttribute)Reflect.GetAttribute(type, typeof(IgnoreAttribute));
            if (ignore != null)
            {
                this.runState = RunState.Ignored;
                this.ignoreReason = ignore.Reason;
            }

            if ( !InvalidTestSuite(type) )
            {
                foreach (MethodInfo method in type.GetMethods())
                {
                    if (IsTestMethod(method))
                        this.AddTest(HasValidSignature(method)
                            ? Reflect.ConstructTestCase(method)
                            : new InvalidTestCase(method.Name,
                                "Test methods must have signature void MethodName()"));
                }
            }
        }
        #endregion

        #region Properties
        public string Name
        {
            get { return name; }
        }

        public string FullName
        {
            get { return fullName; }
        }

        public RunState RunState
        {
            get { return runState; }
        }

        public string IgnoreReason
        {
            get { return ignoreReason; }
        }

        public IDictionary Properties
        {
            get { return properties; }
        }

        public int TestCaseCount
        {
            get
            {
                int count = 0;
                foreach (ITest test in this.tests)
                    count += test.TestCaseCount;
                return count;
            }
        }

        public IList Tests
        {
            get { return tests; }
        }
        #endregion

        #region Public Methods
        public TestResult Run()
        {
            return Run(new NullListener());
        }

        public TestResult Run(TestListener listener)
        {
            int count = 0, failures = 0, errors = 0;
            listener.TestStarted(this);
            TestResult result = new TestResult(this);

            switch (this.RunState)
            {
                case RunState.NotRunnable:
                    result.Error(this.IgnoreReason);
                    break;

                case RunState.Ignored:
                    result.NotRun(this.IgnoreReason);
                    break;

                case RunState.Runnable:
                    foreach (ITest test in tests)
                    {
                        ++count;
                        TestResult r = test.Run(listener);
                        result.AddResult(r);
                        switch (r.ResultState)
                        {
                            case ResultState.Error:
                                ++errors;
                                break;
                            case ResultState.Failure:
                                ++failures;
                                break;
                            default:
                                break;
                        }
                    }

                    if (count == 0)
                        result.NotRun("Class has no tests");
                    else if (errors > 0 || failures > 0)
                        result.Failure("One or more component tests failed");
                    else
                        result.Success();
                    break;
            }

            listener.TestFinished(result);
            return result;
        }

        public void AddTest(ITest test)
        {
            tests.Add(test);
        }
        #endregion

        #region Private Methods
        private bool InvalidTestSuite(Type type)
        {
            if (Reflect.IsTestCaseClass(type))
            {
                if (!Reflect.HasConstructor(type, typeof(string)))
                {
                    this.runState = RunState.NotRunnable;
                    this.ignoreReason = 
                        string.Format( "Class {0} has no public constructor TestCase(string name)", type.Name);
                    //this.AddInvalidTestCase(type.Name, ignoreReason);
                    return true;
                }
            }
            else
            {
                if (!Reflect.HasConstructor(type))
                {
                    this.runState = RunState.NotRunnable;
                    this.ignoreReason = string.Format( "Class {0} has no default constructor", type.Name);
                    //this.AddInvalidTestCase(type.Name, ignoreReason);
                    return true;
                }
            }

            return false;
        }

        private void AddInvalidTestCase( string name, string message)
        {
            this.AddTest(new InvalidTestCase( name, message ) );
        }

        private static bool HasValidSignature(MethodInfo method)
        {
            return method.ReturnType == typeof(void)
                && method.GetParameters().Length == 0; ;
        }

        private bool IsTestMethod(MethodInfo method)
        {
            return method.Name.ToLower().StartsWith("test")
                || Reflect.HasAttribute(method, typeof(TestAttribute));
        }
        #endregion
    }
}
