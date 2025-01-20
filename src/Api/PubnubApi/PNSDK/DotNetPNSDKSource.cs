using System.Globalization;
using System.Reflection;

namespace PubnubApi.PNSDK;

public class DotNetPNSDKSource : IPNSDKSource
{
    public string GetPNSDK()
    {
        var assembly = typeof(Pubnub).GetTypeInfo().Assembly;
        var assemblyName = new AssemblyName(assembly.FullName);
        string assemblyVersion = assemblyName.Version.ToString();
        var targetFramework = assembly.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>()?.FrameworkDisplayName?.Replace(".",string.Empty).Replace(" ", string.Empty);
            
        return string.Format(CultureInfo.InvariantCulture, "{0}/CSharp/{1}", targetFramework??"UNKNOWN", assemblyVersion);
    }
}