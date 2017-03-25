using System;
using System.Reflection;
using NUnit.Common;
using NUnitLite;

namespace Pubnub_Messaging.Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new AutoRun(typeof(Program).GetTypeInfo().Assembly).Execute(args,
                new ExtendedTextWrapper(Console.Out), Console.In);
        }
    }
}
