using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubSonic.Core.VisualStudio.Common
{
    public static class Extensions
    {
        public static void AddIfNotExist<TType>(this ICollection<TType> collection, TType element)
        {
            if (!collection.Any(x => x.Equals(element)))
            {
                collection.Add(element);
            }
        }
    }
}
