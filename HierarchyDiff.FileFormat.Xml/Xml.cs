using HierarchyDiff.Core;
using System;
using System.Diagnostics;
using System.Xml;

namespace HierarchyDiff.FileFormat.Xml
{
    public class Xml : HierarchyDiff.Core.FileFormat
    {
        public override object? Load(string filePath)
        {
            using var streamReader = new StreamReader(filePath);
            if (streamReader == null)
            {
                return null;
            }
            var xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.Load(streamReader);
            return xmlDocument;
        }

        public override void Save(object document, string filePath)
        {
            Debug.Assert(document != null);
            using var streamReader = new StreamReader(filePath);
            (document as XmlDocument)!.Save(filePath + "_t");
        }

        protected override object GetRoot(object document)
        {
            return (document as XmlDocument)!;
        }

        protected override IEnumerable<object> GetChildren(object node)
        {
            XmlNode xmlNode = (node as XmlNode)!;
            if (xmlNode is XmlAttribute)
            {
                yield break;
            }
            if (xmlNode.Attributes != null)
            {
                foreach (XmlAttribute? xmlAttribute in xmlNode.Attributes)
                {
                    if (xmlAttribute != null)
                    {
                        yield return xmlAttribute;
                    }
                }
            }
            foreach (var child in xmlNode.ChildNodes)
            {
                switch (child)
                {
                    case XmlWhitespace:
                        continue;
                }
                yield return child;
            }
        }

        public override float Compare(object a, object b)
        {
            if (a == null || b == null) return a == b ? 1 : 0;
            if (a.GetType() != b.GetType()) { return 0; }
            switch (a)
            {
                case XmlElement:
                {
                    var element0 = (XmlElement)a;
                    var element1 = (XmlElement)b;
                    return
                        element0.Name == element1.Name
                        ? 1
                        : 0;
                }
                case XmlAttribute:
                {
                    var attribute0 = (XmlAttribute)a;
                    var attribute1 = (XmlAttribute)b;
                    if (attribute0.Name != attribute1.Name) return 0.1f;
                    if (attribute0.Value != attribute1.Value) return 0.5f;
                    return 1;
                }
                case XmlDocument:
                {
                    return 1;
                }
                default:
                    throw new NotSupportedException();
            }
        }

        public override string ToString(object? node)
        {
            if (node == null)
            {
                return string.Empty;
            }
            XmlNode xmlNode = (node as XmlNode)!;
            if (xmlNode.Value == null)
            {
                return xmlNode.Name;
            }
            else
            {
                return string.Format("{0}={1}", xmlNode.Name, xmlNode.Value);
            }
        }

        public override string GetName(object? node)
        {
            XmlNode? xmlNode = node as XmlNode;
            return xmlNode?.Name ?? string.Empty;
        }
        public override string? GetValue(object? node)
        {
            XmlNode? xmlNode = node as XmlNode;
            return xmlNode?.Value ?? string.Empty;
        }

        public override bool SetValue(object? node, string value)
        {
            XmlNode? xmlNode = node as XmlNode;
            if (xmlNode == null)
            {
                return false;
            }
            xmlNode.Value = value;
            return true;
        }

        public override TreeNodeStyle GetStyle(object? node, TreeNodeStyle? style)
        {
            style ??= new();
            return style;
        }
    }
}
