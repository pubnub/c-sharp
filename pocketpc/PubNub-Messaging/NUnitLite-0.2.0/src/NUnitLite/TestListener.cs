// *****************************************************
// Copyright 2007, Charlie Poole
//
// Licensed under the Open Software License version 3.0
// *****************************************************

namespace NUnitLite
{
    public interface TestListener
    {
        void TestStarted(ITest test);
        void TestFinished(TestResult result);
    }
}
