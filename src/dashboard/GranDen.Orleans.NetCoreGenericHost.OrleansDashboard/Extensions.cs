using System.Net;
using Microsoft.AspNetCore.Http;

namespace GranDen.Orleans.NetCoreGenericHost.OrleansDashboard
{
    internal static class Extensions
    {
        internal static string ToValue(this PathString path)
        {
            return WebUtility.UrlDecode(path.ToString().Substring(1));
        }
    }
}
