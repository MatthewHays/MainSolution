using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientServerLib;

namespace EntityLib
{
    public class ServerEntityCache<T> : EntityCache<T>, IRequestHandler where T : Entity
    {
        private Server server;
        private int nextId = 0;


        public ServerEntityCache(Server server)
        {
            this.server = server;
            this.server.AddHandler(this);
        }

        public override void Add(T entity)
        {
            lock (syncLock)
            {
                if (entity.Id > 0)
                    nextId = Math.Max(nextId, entity.Id);
                else
                    entity.Id = nextId++;

                base.Add(entity);

                Console.WriteLine("Server Added Entity");

                if (server.ClientCount > 0)
                {
                    Console.WriteLine("Server EntityInsert BroadCast sent - " + entities.Count.ToString() + " entities");
                    Broadcast broadcast = new Broadcast("EntityInsert", new Dictionary<string, object> { { "Entity", entity } });
                    server.Broadcast(broadcast);
                }
            }
        }


        public bool HandleRequest(Request request, Responder responder)
        {
            lock (syncLock)
            {
                if (request.RequestType == "EntityBootstrap")
                {
                    string entType = request.Params["EntityType"] as string;
                    if (entType != null && entType == typeof(T).ToString())
                    {
                        Console.WriteLine("Server EntityBootstrap received, response sent " + entities.Count.ToString() + " entities");

                        responder.Respond
                            (new Response(
                                request.Id,
                                true,
                                new Dictionary<string, object> { { "Entities", new List<T>(entities.Values.Select(a => a as T)) } }));
                        return true;
                    }
                }
                else if (request.RequestType == "EntityInsert")
                {
                    Console.WriteLine("Server EntityInsert received");

                    Entity ent = request.Params["Entity"] as Entity;
                    if (ent != null && ent is T)
                    {
                        this.Add(ent as T);

                        return true;
                    }
                }
                return false;
            }
        }
    }
}
