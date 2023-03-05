using HierarchyDiff.Core;
using System.Drawing;
using System.Xml;
using System.Xml.Linq;

namespace HierarchyDiff.Serialization.Xml
{
    public class Element : Node
    {
        protected System.Xml.XmlNode data;

        public Element(System.Xml.XmlNode xmlDocument)
        {
            this.data = xmlDocument;
        }

        public override Name Name
        {
            get
            {
                switch (data.NodeType)
                {
                    case System.Xml.XmlNodeType.Document:
                    case System.Xml.XmlNodeType.Text:
                        return new Name($"{data.NodeType.ToString().ToUpper()}")
                        {
                            Normal = new Style
                            {
                                Format =
                                    "{{" +
                                    "{0}" +
                                    "}}",
                            }
                        };
                    case System.Xml.XmlNodeType.Element:
                        return new Name(this.data.Name)
                        {
                            Normal = new Style
                            {
                                Bold = true,
                            }
                        };
                    default:
                        throw new NotImplementedException($"Unsupported node type: {data.NodeType}");
                }
            }
        }

        public override Enum Type => NodeType.Element;

        public override Image Icon => Image.Empty;

        public override object? Value
        {
            get
            {
                switch (data.NodeType)
                {
                    case System.Xml.XmlNodeType.Document:
                        return null;
                    case System.Xml.XmlNodeType.Text:
                        return this.data.Value;
                    case System.Xml.XmlNodeType.Element:
                        return null;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        protected override Dictionary<Enum, IList<Node?>> OnPopulateChildren()
        {
            Dictionary<Enum, IList<Node?>> children = new();
            {
                var properties = new List<Node?>();
                if (data.Attributes != null)
                {
                    foreach (System.Xml.XmlAttribute attribute in data.Attributes)
                    {
                        properties.Add(new Attribute(attribute.Name, attribute.Value));
                    }
                }
                children[NodeType.Attribute] = properties;
            }
            {
                var elements = new List<Node?>();
                foreach (System.Xml.XmlNode child in data.ChildNodes)
                {
                    elements.Add(child != null ? new Element(child) : null);
                }
                children[NodeType.Element] = elements;
            }
            return children;
        }

        public override bool Equals(Node? other)
        {
            if (other is null)
            {
                return false;
            }
            if (other is Element element)
            {
                return data == element.data;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ data.GetHashCode();
        }

        public override string ToString()
        {
            if (!Children.TryGetValue(NodeType.Attribute, out var attributes) || attributes.Count() == 0)
            {
                return $"<{this.Name}>";
            }
            var attributeList = string.Join(' ', attributes.Select((x) => $"{x.Name}=\"{x.Value}\""));
            return $"<{this.Name} {attributeList}>";
        }
    }
}
