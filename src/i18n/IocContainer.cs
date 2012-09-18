using System;
using System.Collections.Generic;

namespace i18n
{
    public class IocContainer
    {
        private readonly Dictionary<Type, Func<IocContainer, object>> _typeBuilders;
        private readonly Dictionary<Type, object> _cache;
        public IocContainer()
        {
            _typeBuilders = new Dictionary<Type, Func<IocContainer, object>>();
            _cache = new Dictionary<Type, object>();
        }
        public void Register<TContract>(Func<IocContainer, TContract> factory)
        {
            _typeBuilders[typeof(TContract)] = c => factory(c);
        }
        public T Resolve<T>()
        {
            var contract = typeof(T);
            object entry;
            if(!_cache.TryGetValue(contract, out entry))
            {
                _cache[contract] = _typeBuilders[contract](this);
            }
            return (T)entry;
        }
    }
}