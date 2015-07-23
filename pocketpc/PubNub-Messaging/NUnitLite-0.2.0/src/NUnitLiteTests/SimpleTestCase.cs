// *****************************************************
// Copyright 2007, Charlie Poole
//
// Licensed under the Open Software License version 3.0
// *****************************************************

using System;
using NUnit.Framework;

namespace NUnitLite.Tests
{
    public class SimpleTestCase : TestCase
    {
        public SimpleTestCase(string name) : base(name) { }

        public void One() { }
        public void Two() { }
        public void Three() { }

        public void test1() { }
        public void test2() { }
        public void Test3() { }
        public void TEST4() { }

        internal void test5() { }
        public int test6() { return 0; }
        public void test7(int x, int y) { }  // Should not be loaded
    }

}
