using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace EntityLib
{
    [Serializable]
    public abstract class Entity /*: ISerializable */
    {
        private int id;
        //private readonly string entityType;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        /*public string EntityType
        {
            get { return entityType; }
        }*/

        public Entity()
        {
            id = -1; //TODO this must be unique and negative ?
        }

        /*public Entity(SerializationInfo info, StreamingContext context)
        {
            id = info.GetInt32("Id");

            //find all properties and restore them
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", id);

            //find all properties in subclass and persist them
        }*/
    }
}
