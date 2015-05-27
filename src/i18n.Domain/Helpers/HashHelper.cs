using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace i18n.Helpers
{
    /// <summary>
    /// Helpers for implementing Object.GetHashCode().
    /// http://stackoverflow.com/a/2575444/1173555
    /// </summary>
    public static class HashHelper
    {
        /// <summary>
        /// Facilitates hashcode generation using fluent interface, like this:
        /// <br />
        ///     return 0.CombineHashCode(field1).CombineHashCode(field2).CombineHashCode(field3);
        /// <br />
        /// </summary>
        /// <param name="arg">
        /// Subject object, value, or collection (IEnumerable).
        /// </param>
        public static int CombineHashCode<T>(this int hashCode, T arg)
        {
            unchecked // Overflow is fine, just wrap
            {
                return 31 * hashCode + GetHashCode(arg);
                    // 31 = prime number.
            }
        }
        /// <summary>
        /// Returns the hash code for the passed argument, with appropriate handling of
        /// null and collection types.
        /// </summary>
        /// <typeparam name="T">Type of subject object.</typeparam>
        /// <param name="arg">Subject object.</param>
        /// <returns>Hash code value.</returns>
        /// <remarks>
        /// For null object, the method simpy returns zero.
        /// For collection objects (castable to IEnumerable&lt;object&gt;) the hash code of
        /// the collection elements are combined to form the result hash code.
        /// </remarks>
        public static int GetHashCode<T>(T arg)
        {
            unchecked // Overflow is fine, just wrap
            {
                if (arg == null) {
                    return 0; }
               // Collection?
                IEnumerable<object> collection = arg as IEnumerable<object>;
                if (collection != null) {
                    int hash = 0;
                    foreach (var item in collection) {
                        hash = hash.CombineHashCode(item); }
                    return hash;
                }
               // Non-collection.
                return arg.GetHashCode();
            }
        }
    }
}
