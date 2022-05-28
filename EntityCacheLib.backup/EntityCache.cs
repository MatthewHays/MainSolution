using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using ClientServerLib;

namespace EntityLib
{
    public abstract class EntityCache
    {
        protected Dictionary<int, Entity> entities = new Dictionary<int, Entity>();
    }

    public class EntityCache<T> : EntityCache where T : Entity
    {
        public event Action<EntityCache<T>, T> EntityAdded;
        public event Action<EntityCache<T>, T> EntityRemoved;
        public event Action<EntityCache<T>, T> EntityUpdate;

        protected object syncLock = new object();

        public virtual void Add(T entity)
        {
            lock (syncLock)
            {
                entities.Add(entity.Id, entity);

                if (EntityAdded != null)
                    EntityAdded(this, entity);
            }
        }

        public virtual void Remove(int id)
        {
            lock (syncLock)
            {
                T entity = entities[id] as T;

                entities.Remove(id);

                if (EntityRemoved != null)
                    EntityRemoved(this, entity);
            }
        }

        public virtual void Update(T entity)
        {
            lock (syncLock)
            {
                entities.Add(entity.Id, entity);

                if (EntityUpdate != null)
                    EntityUpdate(this, entity);
            }
        }

        public IList<Entity> Entities
        {
            get
            {
                lock (syncLock)
                {
                    return new List<Entity>(entities.Values); /*.AsReadOnly();*/
                }
            }
        }
    }  
}
