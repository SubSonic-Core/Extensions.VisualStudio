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
