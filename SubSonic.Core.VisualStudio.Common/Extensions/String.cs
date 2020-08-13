using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.Common
{
    public static partial class Extensions
    {
        public static string ToConnectionKey(this string connectionKey)
        {
            int index = connectionKey.IndexOf("#");

            if (index > 0)
            {
                return connectionKey.Substring(index).Replace("#", "NS").Replace(".", "_");
            }
            return connectionKey;
        }
    }
}
