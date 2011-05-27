using System;
using System.Linq;

namespace i18n.Extensions
{
    internal static class Extensions
    {
        public static bool EndsWithAnyIgnoreCase(this string input, params string[] args)
        {
            return args.Aggregate(false, (current, arg) => current | input.EndsWith(arg, StringComparison.OrdinalIgnoreCase));
        }
    }
}