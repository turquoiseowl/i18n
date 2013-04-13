using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace i18n.Domain
{
    public static class Helpers
    {
        public static IEnumerable<T> Append<T>(this IEnumerable<T> lhs, IEnumerable<T> rhs)
        {
            List<T> list = lhs.ToList();
            list.AddRange(rhs);
            return list;
        }
    }
}
