// *****************************************************
// Copyright 2007, Charlie Poole
//
// Licensed under the Open Software License version 3.0
// *****************************************************

using System;
using System.Reflection;
using NUnit.Framework;

namespace NUnitLite
{
    /// <summary>
    /// ProxyTestCase class represents a test case that uses another,
    /// non-TestCase, object to represent the test fixture. All test
    /// methods, setup and teardown are methods on that object.
    /// </summary>
    public class ProxyTestCase : TestCase
    {
        private MethodInfo setup;
        private MethodInfo teardown;

        public ProxyTestCase(string name, object fixture) : base(name, fixture)
        {
            foreach (MethodInfo m in fixture.GetType().GetMethods())
            {
                if ( Reflect.HasAttribute( m, typeof(SetUpAttribute) ) )
                    this.setup = m;

                if ( Reflect.HasAttribute( m, typeof(TearDownAttribute) ) )
                    this.teardown = m;
            }

            //if ( Reflect.HasAttribute( this.method, typeof(IgnoreAttribute) ) )
            //    this.runState = RunState.Ignored;
        }

        protected override void SetUp()
        {
            if (setup != null)
            {
                Assert.That(HasValidSetUpTearDownSignature(setup), "Invalid SetUp method: must return void and have no arguments");
                InvokeMethod( setup );
            }
        }

        protected override void TearDown()
        {
            if (teardown != null)
            {
                Assert.That(HasValidSetUpTearDownSignature(teardown), "Invalid TearDown method: must return void and have no arguments");
                InvokeMethod( teardown );
            }
        }

        //protected override void InvokeMethod(MethodInfo method, params object[] args)
        //{
        //    Reflect.InvokeMethod(method, this.fixture, args);
        //}

        //protected override MethodInfo GetMethod(string name, BindingFlags flags, params Type[] argTypes)
        //{
        //    return Reflect.GetMethod(fixture.GetType(), name, flags, argTypes);
        //}

        private static bool HasValidSetUpTearDownSignature(MethodInfo method)
        {
            return method.ReturnType == typeof(void)
                && method.GetParameters().Length == 0; ;
        }
    }
}
