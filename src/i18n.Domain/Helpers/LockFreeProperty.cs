using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace i18n.Domain.Helpers
{
    /// <summary>
    /// Helper class for implementing a property with 'typically' lock-free thread-safe accessors.
    /// </summary>
    public class LockFreeProperty<T>
    {
        readonly object m_sync = new object();

        private T m_prop;

        public T Get(Func<T> factory)
        {
            if (m_prop != null) { // Read attempt 1 of lock-free read.
                return m_prop; }
            lock (m_sync) {
                if (m_prop != null) { // Read attempt 2 of lock-free read.
                    return m_prop; }
                m_prop = factory(); 
                return m_prop;
            }
        }
        public void Set(T value)
        {
            m_prop = value;
        }
    }
}
