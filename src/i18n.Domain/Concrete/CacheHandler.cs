using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;

namespace i18n.Domain.Concrete
{
    public class CacheHandler
    {
        private CacheDependency _cacheDependency;

        public CacheHandler(CacheDependency cacheDependency)
        {
            _cacheDependency = cacheDependency;
        }
    }
}
