using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using NUnit.Common;
using NUnitLite;

namespace PubNub_Messaging.Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new AutoRun(typeof(Program).GetTypeInfo().Assembly)
                .Execute(args, new ExtendedTextWrapper(Console.Out), Console.In);

            Console.ReadLine();
        }
    }
}
