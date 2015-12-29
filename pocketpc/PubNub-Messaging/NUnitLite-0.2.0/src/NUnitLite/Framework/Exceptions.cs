// *****************************************************
// Copyright 2007, Charlie Poole
//
// Licensed under the Open Software License version 3.0
// *****************************************************

using System;

namespace NUnit.Framework
{
    //[Serializable]
    public class AssertionException : Exception
    {
        public AssertionException(string message) : base(message) { }
    }

    /// <summary>
    /// This class is used to wrap other exceptions, in order
    /// to preserve InnerException, including its stack trace
    /// </summary>
    //[Serializable]
    class NUnitLiteException : Exception
    {
        public NUnitLiteException(string message, Exception inner)
            : base(message, inner) { }
    }
}
