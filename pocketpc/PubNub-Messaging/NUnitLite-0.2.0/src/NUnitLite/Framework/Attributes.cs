// *****************************************************
// Copyright 2007, Charlie Poole
//
// Licensed under the Open Software License version 3.0
// *****************************************************

using System;

namespace NUnit.Framework
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class TestFixtureAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class TestAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
    public class PropertyAttribute : Attribute
    {
        private string propertyName;
        private object propertyValue;

        public PropertyAttribute(string propertyName, object propertyValue)
        {
            this.propertyName = propertyName;
            this.propertyValue = propertyValue;
        }

        protected PropertyAttribute(object propertyValue)
        {
            this.propertyName = this.GetType().Name;
            if (propertyName.EndsWith("Attribute"))
                propertyName = propertyName.Substring(0, propertyName.Length - 9);
            this.propertyValue = propertyValue;
        }

        public string Name
        {
            get { return propertyName; }
        }

        public virtual object Value
        {
            get { return propertyValue; }
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class DescriptionAttribute : PropertyAttribute
    {
        public DescriptionAttribute(string description) : base( description) { }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class IgnoreAttribute : Attribute
    {
        private string reason;

        public IgnoreAttribute(string reason) 
        {
            this.reason = reason;
        }

        public string Reason
        {
            get { return reason; }
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class SetUpAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class TearDownAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ExpectedExceptionAttribute : Attribute
    {
        private Type exceptionType;
        private string handler;

        public ExpectedExceptionAttribute() { }

        public ExpectedExceptionAttribute(Type exceptionType)
        {
            this.exceptionType = exceptionType;
        }

        public Type ExceptionType
        {
            get { return exceptionType; }
        }

        public string Handler
        {
            get { return handler; }
            set { handler = value; }
        }
    }
}
