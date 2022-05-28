using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientServerLib;

namespace EntityLib
{
    public class RemoteEntityCache<T> : EntityCache<T> where T : Entity
    {
        private Client client;


        public RemoteEntityCache(Client client)
        {
            this.client = client;
            this.client.BroadCast += new Action<Broadcast>(BroadCast);

            Console.WriteLine("EntityBootstrap request sent");

            Request request = new Request("EntityBootstrap", new Dictionary<string, object> { { "EntityType", typeof(T).ToString() } });

            client.SendRequest(request, EntityBootstrapResponse);
            //should lock the entity until I receive a response?
        }

        private void EntityBootstrapResponse(Response response)
        {
            lock (syncLock)
            {
                if (response.Success)
                {
                    List<T> entities = response.Params["Entities"] as List<T>;
                    Console.WriteLine("EntityBootstrap received with " + entities.Count.ToString() + " entities");
                    foreach (Entity entity in entities)
                        base.Add(entity as T);
                }
            }
        }

        private void BroadCast(Broadcast broadcast)
        {
            lock (syncLock)
            {
                if (broadcast.BroadcastType == "EntityInsert")
                {
                    Entity ent = broadcast.Params["Entity"] as Entity;
                    if (ent != null && ent is T)
                    {
                        base.Add(ent as T);
                    }

                    Console.WriteLine("BroadCast received " + entities.Count.ToString() + " entities");
                }
            }
        }

        public override void Add(T entity)
        {
            lock (syncLock)
            {
                Console.WriteLine("EntityInsert sent");
                Request request = new Request("EntityInsert", new Dictionary<string, object> { { "Entity", entity } });

                client.SendRequest(request, null); //todo reply?
                //base.Add(entity);
            }
        }
    }
}
