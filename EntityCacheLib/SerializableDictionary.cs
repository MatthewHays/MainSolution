using System.Collections.Generic;

using System.Xml.Serialization;
using System;

namespace EntityLib
{
    [XmlRoot("dictionary")]
    public class SerializableDictionary<TKey, TValue>
        : Dictionary<TKey, TValue>, IXmlSerializable
    {
        private XmlSerializer keySerializer = new XmlSerializer(typeof(TKey), new Type[] { typeof(Request), typeof(Response), typeof(Broadcast), typeof(List<Entity>), typeof(List<Entity>), typeof(List<MattsEntity>) });
        
        private XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue), new Type[] { typeof(Request), typeof(Response), typeof(Broadcast), typeof(Entity), typeof(MattsEntity), typeof(List<Entity>), typeof(List<MattsEntity>) });
        private XmlSerializerNamespaces customNamespace = new XmlSerializerNamespaces();
        

        public SerializableDictionary() : base()
        {
            customNamespace.Add(string.Empty, string.Empty);
        }

        #region IXmlSerializable Members
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }
 
        public void ReadXml(System.Xml.XmlReader reader)
        {
            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();
 
            if (wasEmpty)
                return;
 
            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");
 
                reader.ReadStartElement("key");
                TKey key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();
 
                reader.ReadStartElement("value");
                TValue value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();
 
                this.Add(key, value);
 
                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }
 
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            foreach (TKey key in this.Keys)
            {
                writer.WriteStartElement("item");
 
                writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key, customNamespace);
                writer.WriteEndElement();
 
                writer.WriteStartElement("value");
                TValue value = this[key];
                valueSerializer.Serialize(writer, value, customNamespace);
                writer.WriteEndElement();
 
                writer.WriteEndElement();
            }
        }
        #endregion
    }
}
