using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Net.Sockets;

namespace EntityLib
{
    public class Client
    {
        private TcpClient client;
        private NetworkStream stream;
        private StreamWriter writer;
        private StreamReader reader;
        private Dictionary<string, Action<Response>> requestMap;
        private Thread thread;
        public event Action<Broadcast> BroadCast;

        public event Action<Client> Disconnected;
        public bool Connected { get; private set; }

        public bool Connect(string serverAddress, int serverPort)
        {
            this.requestMap = new Dictionary<string, Action<Response>>();
            this.client = new TcpClient();
            
            this.client.Connect(serverAddress, serverPort);
            this.stream = client.GetStream();
            this.writer = new StreamWriter(stream);
            this.reader = new StreamReader(stream);
            this.writer.AutoFlush = true;

            Connected = true;
            
            this.thread = new Thread(new ThreadStart(ThreadProc));
            this.thread.Start();

            return true;
        }

        public void Disconnect()
        {
            if (!Connected)
                return;

            Connected = false;
            this.client.Close();

            if (Disconnected != null)
                Disconnected(this);
        }

        public bool SendRequest(Request request, Action<Response> response)
        {
            if (this.stream == null || !Connected)
                return false;
            
            try
            {
                if (response != null)
                    this.requestMap.Add(request.Id, response);
                this.writer.WriteLine(SerializationUtils.Serialise(request));
            }
            catch (IOException)
            {
                Disconnect();
                return false;
            }

            return true;
        }

        private void ThreadProc()
        {
            try
            {
                while (this.client.Connected)
                {
                    string msg = reader.ReadLine();
                    //Console.WriteLine("Received " + msg);
                    Message m = SerializationUtils.Deserialise(msg);

                    if (m is Response && requestMap.ContainsKey(m.Id))
                        requestMap[m.Id](m as Response);
                    else if (m is Broadcast && BroadCast != null)
                        BroadCast(m as Broadcast);
                }
            }
            catch (IOException)
            {
                Disconnect();
            }
        }

    }
}
