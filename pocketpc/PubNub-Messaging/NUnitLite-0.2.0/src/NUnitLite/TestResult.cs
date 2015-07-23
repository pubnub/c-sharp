// *****************************************************
// Copyright 2007, Charlie Poole
//
// Licensed under the Open Software License version 3.0
// *****************************************************

using System;
using System.Collections;
using NUnit.Framework;

namespace NUnitLite
{
    public enum ResultState
    {
        NotRun,
        Success,
        Failure,
        Error
    }

    public class TestResult
    {
        private ITest test;

        private ResultState resultState = ResultState.NotRun;

        private string message;
#if !NETCF_1_0
        private string stackTrace;
#endif

        private ArrayList results;

        public TestResult(ITest test)
        {
            this.test = test;
        }

        public ITest Test
        {
            get { return test; }
        }

        public ResultState ResultState
        {
            get { return resultState; }
        }

        public IList Results
        {
            get 
            {
                if (results == null)
                    results = new ArrayList();

                return results;
            }
        }

        public bool Executed
        {
            get { return resultState != ResultState.NotRun; }
        }

        public bool IsSuccess
        {
            get { return resultState == ResultState.Success; }
        }

        public bool IsFailure
        {
            get { return resultState == ResultState.Failure; }
        }

        public bool IsError
        {
            get { return resultState == ResultState.Error; }
        }

        public string Message
        {
            get { return message; }
        }

#if !NETCF_1_0
        public string StackTrace
        {
            get { return stackTrace; }
        }
#endif

        public void AddResult(TestResult result)
        {
            if (results == null)
                results = new ArrayList();

            results.Add(result);

            switch (result.ResultState)
            {
                case ResultState.Error:
                case ResultState.Failure:
                    this.Failure("Component test failure");
                    break;
                default:
                    break;
            }
        }

        public void Success()
        {
            this.resultState = ResultState.Success;
            this.message = null;
        }


	    public void Failure(string message)
	    {
                this.resultState = ResultState.Failure;
                if (this.message == null || this.message == string.Empty)
                    this.message = message;
                else
                    this.message = this.message + NUnitLite.Env.NewLine + message;
            }

        public void Error(string message)
        {
            this.resultState = ResultState.Error;
            if (this.message == null || this.message == string.Empty)
                this.message = message;
            else
                this.message = this.message + NUnitLite.Env.NewLine + message;
        }

#if !NETCF_1_0
        public void Failure(string message, string stackTrace)
        {
            this.Failure(message);
            this.stackTrace = stackTrace;
        }
#endif

        public void Error(Exception ex)
        {
            this.resultState = ResultState.Error;
            this.message = ex.GetType().ToString() + " : " + ex.Message;
#if !NETCF_1_0
            this.stackTrace = ex.StackTrace;
#endif
        }

        public void NotRun(string message)
        {
            this.resultState = ResultState.NotRun;
            this.message = message;
        }

        public void RecordException(Exception ex)
        {
            if (ex is AssertionException)
#if NETCF_1_0
		this.Failure(ex.Message);
#else
                this.Failure(ex.Message, StackFilter.Filter(ex.StackTrace));
#endif
            else
                this.Error(ex);
        }
    }
}
