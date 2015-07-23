using System;
using NUnit.Framework;

namespace NUnitLite.Tests
{
    class DummyTestSuite : TestSuite
    {
        public DummyTestSuite(string recipe)
            : base("Dummy")
        {
            foreach (char c in recipe)
            {
                DummyTestCase test = new DummyTestCase("TheTest");

                switch (c)
                {
                    case 'E':
                        test.simulateTestError = true;
                        break;
                    case 'F':
                        test.simulateTestFailure = true;
                        break;
                    default:
                        break;
                }

                this.AddTest(test);
            }
        }
    }
}
