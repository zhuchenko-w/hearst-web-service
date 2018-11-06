using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HearstWebService.Common.Extensions
{
    public static class CommonExtensions
    {
        public static Task WaitForExitAsync(this Process process)
        {
            var tcs = new TaskCompletionSource<object>();

            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(null);

            if (process.HasExited)
            {
                tcs.TrySetResult(null);
            }

            return tcs.Task;
        }

        public static string GetEnumDescription(this Enum value)
        {
            var fi = value.GetType().GetField(value.ToString());
            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes( typeof(DescriptionAttribute), false);

            return attributes != null && attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }
    }
}
