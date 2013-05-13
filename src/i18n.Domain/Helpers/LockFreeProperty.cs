using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace i18n.Domain.Helpers
{
    /// <summary>
    /// Helper class for implementing a reference property with a lock-free thread-safe accessor.
    /// Null value may be set and remembered.
    /// </summary>
    public class LockFreeProperty<T> where T : class
    {
        readonly object m_sync = new object();

        /// <summary>
        /// Wrapped property value. 
        /// </summary>
        private T m_prop;
        
        /// <summary>
        /// Indicates whether or not m_prop has been set, irrespective of whether 
        /// it is null or not.
        /// </summary>
        private bool m_set;

        /// <summary>
        /// If a value has been previously set (by a call to Set or Get) returns that
        /// value; otherwise calls the factory method to get a value and sets that value
        /// before returning it.
        /// </summary>
        /// <param name="factory">Method called when the property value is not yet set to create a new value.</param>
        public T Get(Func<T> factory)
        {
            if (m_set) { // Read attempt 1 of lock-free read.
                return m_prop; }
            lock (m_sync) {
                if (m_set) { // Read attempt 2 of lock-free read.
                    return m_prop; }
                m_prop = factory(); 
                m_set = true;
                return m_prop;
            }
        }

        /// <summary>
        /// Explicitly sets the property value, or clears it.
        /// </summary>
        /// <param name="value">Value to set. Null is considered a valid value.</param>
        /// <param name="set">true to mark the property as set; false to mark as unset.</param>
        public void Set(T value, bool set)
        {
            lock (m_sync) {
                m_prop = value;
                m_set = set;
            }
        }

        /// <summary>
        /// Marks the property value as not set.
        /// </summary>
        public void Reset()
        {
            Set(null, false);
        }
    }
}
