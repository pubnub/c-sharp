using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Reflection;

namespace Launcher
{
    class MainClass
    {
        public static void Main (string[] args)
        {
            string exec = @" ";
            string exportFile = @"/Users/Shared/Jenkins/tests/sim-results.log";
            FileInfo lastReport = new FileInfo (exportFile);
            if (lastReport.Exists) {
                lastReport.Delete ();
            }
            //Process.Start (new ProcessStartInfo (exec){ UseShellExecute = false });

            Process.Start ("/bin/chmod", "+x /Users/Shared/Jenkins/git/c-sharp/monotouch/Launcher/bin/Debug/launch.sh");
            Process p = new Process ();                        
            //p.StartInfo.FileName = "/Applications/X code.app/Contents/Developer/usr/bin/make";
            //p.StartInfo.Arguments = @"-C '/Users/rajat-ml/Projects/Xamarin.iOS/3.5/Pubnub-Messaging modified tests/Touch.Unit' run run-simulator";
            //p.StartInfo.FileName = "mono";
            //p.StartInfo.Arguments = "--debug '/Users/rajat-ml/Projects/Xamarin.iOS/3.5/Pubnub-Messaging modified tests/Touch.Server/bin/Debug/Touch.Server.exe' --launchsim bin/iPhoneSimulator/Debug/TouchUnit.app -autoexit -logfile=" + exportFile;

            //TODO: set in cl arg
            // Cl 2 Touch.Server.exe
            // cl 3 TouchUnit.app
            // Cl 4 exportFile
            p.StartInfo.FileName = "/Users/Shared/Jenkins/git/c-sharp/monotouch/Launcher/bin/Debug/launch.sh";
            p.StartInfo.UseShellExecute = false;
            p.Start ();

            /*while(true){
                if(lastReport.Exists){
                    Console.WriteLine ("File found");
                    break;
                }else{
                    Thread.Sleep(1000);
                }
            }*/
            Console.WriteLine ("Sleeping");
            Thread.Sleep (900000);
            if (!p.HasExited) {
                Console.WriteLine ("Killing");
                p.Kill ();
                p.Close ();
                p.Dispose ();
            }
            Console.WriteLine ("Exiting");
        }
    }
}