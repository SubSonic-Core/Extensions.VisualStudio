using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio
{
    public static partial class Extensions
    {
        public static TService GetService<TService>(this IServiceProvider provider, bool throwOnFailure = true)
        {
            if (provider.GetService(typeof(TService)) is TService success)
            {
                return success;
            }

            if (throwOnFailure)
            {
                throw new InvalidOperationException();
            }

            return default;
        }
        public static async Task<TService> GetServiceAsync<TService>(this IAsyncServiceProvider provider, bool throwOnFailure = true)
        {
            if (await provider.GetServiceAsync(typeof(TService)) is TService success)
            {
                return success;
            }

            if (throwOnFailure)
            {
                throw new InvalidOperationException();
            }

            return default;
        }
    }
}
