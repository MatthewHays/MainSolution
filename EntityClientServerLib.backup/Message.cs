using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace ClientServerLib
{
    /// <summary>
    /// The base class for all messages
    /// </summary>
    public abstract class Message 
    {
        public Message()
        {
            this.Source    = Environment.MachineName;
            this.Id        = Guid.NewGuid().ToString();
            this.TimeStamp = DateTime.Now.ToUniversalTime();
            this.Params    = new SerializableDictionary<string, object>();
        }
        
        public string   Id        { get; set; }
        public DateTime TimeStamp { get; set; }
        public string   Source    { get; set; }
        public SerializableDictionary<string, object> Params { get; set; }
    }

    public class Request : Message
    {
        public string RequestType { get; set; }

        public Request()
        {
        }

        public Request(string requestType, IDictionary<String, object> @params) 
        {
            this.RequestType = requestType;
            
            foreach (KeyValuePair<string, object> kvp in @params)
                this.Params.Add(kvp.Key, kvp.Value);
        }
    }


    public class Response : Message
    {
        public bool   Success         { get; set; }

        public Response() { }

        public Response(string messageId, bool success, IDictionary<String, object> @params)
        {
            this.Id = messageId;
            this.Success   = success;

            foreach (KeyValuePair<string, object> kvp in @params)
                this.Params.Add(kvp.Key, kvp.Value);
        }
    }

    public class Broadcast : Message
    {
        public string BroadcastType { get; set; }

        public Broadcast()
        {
        }

        public Broadcast(string broadcastType, IDictionary<String, object> @params) 
        {
            this.BroadcastType = broadcastType;

            foreach (KeyValuePair<string, object> kvp in @params)
                this.Params.Add(kvp.Key, kvp.Value);
        }
    }
}
