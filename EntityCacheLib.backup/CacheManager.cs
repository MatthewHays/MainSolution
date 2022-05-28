using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntityLib
{
    public class CacheManager
    {
        private Dictionary<Type, EntityCache> caches = new Dictionary<Type, EntityCache>();

        public void Add<T>(EntityCache<T> cache) where T : Entity
        {
            caches.Add(typeof(T), cache);
        }

        public EntityCache<T> GetCache<T>() where T : Entity
        {
            return caches[typeof(T)] as EntityCache<T>;
        }
    }
}
