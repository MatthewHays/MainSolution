using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using EntityLib;
using ClientServerLib;

namespace ConnectionTest
{

    public class MattsEntity : Entity
    {
        public MattsEntity()
        {
        }

        public MattsEntity(int a)
        {
            Value = a;
        }

        public int Value { get; set; }
    }
     
    class Program
    {
        static void Main(string[] args)
        {
            string a = typeof(MattsEntity).ToString();
            Type t = Type.GetType(a);

            Server server = new Server();
            server.Start(15002);

            CacheManager serverCacheManager = new CacheManager();
            serverCacheManager.Add(new ServerEntityCache<MattsEntity>(server));
            serverCacheManager.GetCache<MattsEntity>().Add(new MattsEntity(1));

            Thread clientThread1 = new Thread(new ParameterizedThreadStart(ClientProc));
            clientThread1.Start(null);

            /*Thread.Sleep(1000);
            serverCacheManager.GetCache<MattsEntity>().Add(new MattsEntity(3));
            Thread.Sleep(5000);*/
            
            System.Console.ReadLine();
        }

        public static void ClientProc(object obj) 
        {
            
            Client client = new Client();
            client.Connect("localhost", 15002);
            System.Console.WriteLine("Client connected");
            
            Thread.Sleep(1000);

            CacheManager clientCacheManager = new CacheManager();
            clientCacheManager.Add(new RemoteEntityCache<MattsEntity>(client));
            Console.WriteLine("Remote  Added Entity");
            clientCacheManager.GetCache<MattsEntity>().Add(new MattsEntity(2));


            while (clientCacheManager.GetCache<MattsEntity>().Entities.Count() < 2)
            {
                Thread.Sleep(0);
            }

            Console.WriteLine(clientCacheManager.GetCache<MattsEntity>().Entities.Count.ToString());
            
        }
    }
}
