using System;

using System.Collections.Generic;
using System.Text;
using NUnitLite.Runner;

namespace PubNub_Messaging.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
#if PocketPC || WindowsCE
            string myDocs = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string path = System.IO.Path.Combine(myDocs, "TestResult.txt");
            System.IO.TextWriter writer = new System.IO.StreamWriter(path);
            new TextUI(writer).Execute(args);
            System.Diagnostics.Debug.WriteLine(writer.ToString());
            writer.Close();
#else
            new ConsoleUI().Execute(args);
#endif
        }
    }
}
