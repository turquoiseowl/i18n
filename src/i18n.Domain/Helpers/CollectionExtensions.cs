using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace i18n.Helpers
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Returns a copy of a collection with the contents of another collection appended to it.
        /// </summary>
        public static IEnumerable<T> Append<T>(this IEnumerable<T> lhs, IEnumerable<T> rhs)
        {
            List<T> list = lhs.ToList();
            list.AddRange(rhs);
            return list;
        }

        /// <summary>
        /// Returns a copy of a collection with a new single item added to it.
        /// </summary>
        public static IEnumerable<T> Append<T>(this IEnumerable<T> lhs, T rhs)
        {
            List<T> list = lhs.ToList();
            list.Add(rhs);
            return list;
        }
    }
}
