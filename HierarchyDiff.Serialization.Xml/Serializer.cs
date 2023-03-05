using HierarchyDiff.Core;
using System.Xml;

namespace HierarchyDiff.Serialization.Xml
{
    [Format(".xml")]
    public class Serializer : ISerializer
    {
        public Serializer()
        {
        }

        public void Serialize(Node root, StreamWriter steamWriter)
        {
            throw new NotImplementedException();
        }

        public Node Deserialize(StreamReader streamReader)
        {
            var document = new XmlDocument();
            document.Load(streamReader);
            return new Element(document);
        }
    }
}