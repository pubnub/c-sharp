// *****************************************************
// Copyright 2007, Charlie Poole
//
// Licensed under the Open Software License version 3.0
// *****************************************************

using System;

namespace NUnit.Framework
{
    /// <summary>
    /// Interface implemented by a user fixture in order to
    /// validate any expected exceptions. It is only called
    /// for test methods marked with the ExpectedException
    /// attribute.
    /// </summary>
	public interface IExpectException
    {
		/// <summary>
		/// Method to handle an expected exception
		/// </summary>
		/// <param name="ex">The exception to be handled</param>
        void HandleException(Exception ex);
    }
}
