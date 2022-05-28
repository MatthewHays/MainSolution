using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace ClientServerLib
{
    public static class SerializationUtils
    {
        static XmlSerializer serialiser = new XmlSerializer(typeof(Message), new Type[] { typeof(Request), typeof(Response), typeof(Broadcast) });

        //XMLFormatter, XMLSerializer, SoapFormatter, BinaryFormatter
        public static Message Deserialise(string message)
        {
            MemoryStream memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(message));

            Message returnMessage = (Message)serialiser.Deserialize(memoryStream);

            return returnMessage;
        }

        public static string Serialise(Message message)
        {
            MemoryStream memoryStream = new MemoryStream();

            XmlSerializerNamespaces customNamespace = new XmlSerializerNamespaces();
            customNamespace.Add(string.Empty, string.Empty);

            serialiser.Serialize(memoryStream, message, customNamespace);

            string str = Encoding.ASCII.GetString(memoryStream.ToArray());

            return str.Replace("\r\n", string.Empty);
        }
    }
}
