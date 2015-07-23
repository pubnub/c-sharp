// *****************************************************
// Copyright 2007, Charlie Poole
//
// Licensed under the Open Software License version 3.0
// *****************************************************

using System;
using System.Reflection;
using System.Collections;
using NUnit.Framework;

namespace NUnitLite
{
    public class TestCase : ITest
    {
        #region Instance Variables
        private string name;
        protected string fullName;

        protected object fixture;
        protected MethodInfo method;

        protected RunState runState = RunState.Runnable;
        protected string ignoreReason;

        private IDictionary properties;
        #endregion

        #region Constructors
        public TestCase(string name)
        {
            Initialize(name, this);
        }

        protected TestCase(string name, object fixture)
        {
            Initialize(name, fixture);
        }

        private void Initialize(string name, object fixture)
        {
            this.name = name;
            this.fixture = fixture;
            this.fullName = this.fixture.GetType().FullName + "." + name;
            this.method = Reflect.GetMethod(this.fixture.GetType(), name);
            if (this.method == null)
                this.runState = RunState.NotRunnable;
            else 
            {
                IgnoreAttribute ignore = (IgnoreAttribute)Reflect.GetAttribute(this.method, typeof(IgnoreAttribute));
                if (ignore != null)
                {
                    this.runState = RunState.Ignored;
                    this.ignoreReason = ignore.Reason;
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

        public System.Collections.IDictionary Properties
        {
            get 
            {
                if (properties == null)
                {
                    properties = new Hashtable();

                    object[] attrs = this.method.GetCustomAttributes(typeof(PropertyAttribute), true);
                    foreach (PropertyAttribute attr in attrs)
                        this.Properties[attr.Name] = attr.Value;
                }

                return properties; 
            }
        }

        public int TestCaseCount
        {
            get { return 1; }
        }
        #endregion

        #region Public Methods
        public TestResult Run()
        {
            return Run( new NullListener() );
        }

        public TestResult Run(TestListener listener)
        {
            listener.TestStarted(this);

            TestResult result = new TestResult(this);
            Run(result, listener);

            listener.TestFinished(result);

            return result;
        }
        #endregion

        #region Protected Methods
        protected virtual void SetUp() { }

        protected virtual void TearDown() { }

        protected virtual void Run(TestResult result, TestListener listener)
        {
            //this.method = GetMethod(this.Name, BindingFlags.Public | BindingFlags.Instance);
            IgnoreAttribute ignore = (IgnoreAttribute)Reflect.GetAttribute(method, typeof(IgnoreAttribute));
            if ( ignore != null )
                result.NotRun(ignore.Reason);
            else
            {
                try
                {
                    RunBare();
                    result.Success();
                }
                catch (NUnitLiteException nex)
                {
                    result.RecordException(nex.InnerException);
                }
#if !NETCF_1_0
                catch (System.Threading.ThreadAbortException)
                {
                    throw;
                }
#endif
                catch (Exception ex)
                {
                    result.RecordException(ex);
                }
            }
        }

        protected void RunBare()
        {
            SetUp();
            try
            {
                RunTest();
            }
            finally
            {
                TearDown();
            }
        }

        protected virtual void RunTest()
        {
            try
            {
                InvokeMethod( this.method );
                ProcessNoException(this.method);
            }
            catch (NUnitLiteException ex)
            {
                ProcessException(this.method, ex.InnerException);
            }
        }

        protected void InvokeMethod(MethodInfo method, params object[] args)
        {
            Reflect.InvokeMethod(method, this.fixture, args);
        }
        #endregion

        #region Private Methods       
        private static void ProcessNoException(MethodInfo method)
        {
            ExpectedExceptionAttribute exceptionAttribute =
                (ExpectedExceptionAttribute)Reflect.GetAttribute(method, typeof(ExpectedExceptionAttribute));

            if (exceptionAttribute != null)
                Assert.Fail("Expected Exception of type <{0}>, but none was thrown", exceptionAttribute.ExceptionType);
        }

        private void ProcessException(MethodInfo method, Exception caughtException)
        {
            ExpectedExceptionAttribute exceptionAttribute =
                (ExpectedExceptionAttribute)Reflect.GetAttribute(method, typeof(ExpectedExceptionAttribute));

            if (exceptionAttribute == null)
                throw new NUnitLiteException("", caughtException);

            Type expectedType = exceptionAttribute.ExceptionType;
            if ( expectedType != null && expectedType != caughtException.GetType() )
                Assert.Fail("Expected Exception of type <{0}>, but was <{1}>", exceptionAttribute.ExceptionType, caughtException.GetType());

            MethodInfo handler = GetExceptionHandler(method.ReflectedType, exceptionAttribute.Handler);

            if (handler != null)
                InvokeMethod( handler, caughtException );
        }

        private MethodInfo GetExceptionHandler(Type type, string handlerName)
        {
            if (handlerName == null && Reflect.HasInterface( type, typeof(IExpectException) ) )
                handlerName = "HandleException";

            if (handlerName == null)
                return null;

            MethodInfo handler = Reflect.GetMethod( type, handlerName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static,
                new Type[] { typeof(Exception) });

            if (handler == null)
                Assert.Fail("The specified exception handler {0} was not found", handlerName);

            return handler;
        }
        #endregion
    }
}
