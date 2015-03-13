using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace EntityLib
{
    public class Responder
    {
        private StreamWriter writer;
        
        internal Responder(StreamWriter writer)
        {
            this.writer = writer;
        }

        public void Respond(Response response)
        {
            try
            {
                if (writer == null)
                    return;

                string responseStr = SerializationUtils.Serialise(response);
                writer.WriteLine(responseStr);
            }
            catch (IOException)
            {
                //client has disconnected, we cant respond
            }
        }
    }


    public interface IRequestHandler
    {
        bool HandleRequest(Request message, Responder responder);
    }


    public class Server
    {
        private List<ClientConnection> clients;
        private TcpListener listener;
        private List<IRequestHandler> requestHandlers = new List<IRequestHandler>();
        private object syncRoot = new object();

        public bool Active { get; private set; }

        public void Start(int port)
        {
            if (Active)
                return;

            this.clients = new List<ClientConnection>();
            this.listener = new TcpListener(IPAddress.Any, port);

            try
            {
                this.listener.Start();
            }
            catch (SocketException)
            {   
                return;
            }
            
            Active = true;

            this.listener.BeginAcceptTcpClient(new AsyncCallback(AcceptClient), null);
        }

        

        private void AcceptClient(IAsyncResult asyncResult)
        {
            if (listener == null)
                return;

            TcpClient client = listener.EndAcceptTcpClient(asyncResult);

            ClientConnection clientConnection = new ClientConnection(client, requestHandlers);
            lock (syncRoot)
            {
                this.clients.Add(clientConnection);
            }

            this.listener.BeginAcceptTcpClient(new AsyncCallback(AcceptClient), null);
        }

        public void Stop()
        {
            if (!Active)
                return;

            lock (syncRoot)
            {
                foreach (ClientConnection clientConnection in this.clients)
                {
                    clientConnection.Close();
                }
            }
            Active = false;
            
            this.listener.Stop();
            this.listener = null;
        }

        public void AddHandler(IRequestHandler requestHandler)
        {
            requestHandlers.Add(requestHandler);
        }

        public void Broadcast(Broadcast message)
        {
            if (!Active)
                return;

            lock (syncRoot)
            {
                foreach (ClientConnection clientConnection in this.clients)
                {
                    clientConnection.Broadcast(message);
                }
            }
        }
    }


    internal class ClientConnection
    {
        private TcpClient client;
        private Thread thread;
        private StreamWriter writer;
        private StreamReader reader;
        private NetworkStream stream;
        private List<IRequestHandler> requestHandlers;
        private Responder responder;

        public bool Connected { get; set; }


        public ClientConnection(TcpClient client, List<IRequestHandler> requestHandlers)
        {
            this.client = client;
            this.requestHandlers = requestHandlers;
            this.stream = client.GetStream();
            this.writer = new StreamWriter(stream);
            this.writer.AutoFlush = true;
            this.reader = new StreamReader(stream);
            this.responder = new Responder(writer);

            Connected = true;

            this.thread = new Thread(new ThreadStart(ThreadProc));
            this.thread.Start();
        }

        private void ThreadProc()
        {
            while (Connected)
            {
                if (reader.Peek() > 0)
                {
                    string msg = reader.ReadLine();
                    Message message = SerializationUtils.Deserialise(msg);
                    if (message is Request)
                    {
                        foreach (IRequestHandler requestHandler in requestHandlers)
                            if (requestHandler.HandleRequest(message as Request, responder))
                                break;
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        public void Broadcast(Broadcast message)
        {
            if (!Connected)
                return;

            try
            {
                string messageStr = SerializationUtils.Serialise(message);
                writer.WriteLine(messageStr);
            }
            catch (IOException)
            {
                Close();
            }
        }

        public void Close()
        {
            if (!Connected)
                return;

            Connected = false;
            this.client.Close();
            this.client = null;
            this.writer = null;
            this.reader = null;
        }
    }
}
