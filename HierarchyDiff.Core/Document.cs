using System.Diagnostics;

namespace HierarchyDiff.Core
{
    public enum TraversalOrder
    {
        Preorder,
        Inorder,
        Postorder,
    }

    public class Document
    {
        public Node RootNode { get; private set; }

        public Type SerializationType { get; private set; }

        internal Document(Node rootNode, Type serializationType)
        {
            this.RootNode = rootNode;
            this.SerializationType = serializationType;
        }

        public static Document? Load(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }
            var extensionName = Path.GetExtension(path);
            var serializer = ExtensionManager.GetSerializerByExtensionName(extensionName);
            Node? rootNode = null;
            using (StreamReader reader = new StreamReader(path, new FileStreamOptions() { Access = FileAccess.Read }))
            {
                if (serializer != null)
                {
                    rootNode = serializer.Deserialize(reader);
                }
                else
                {
                    foreach (var (k, serializer_) in ExtensionManager.Serializers)
                    {
                        try
                        {
                            rootNode = serializer_.Deserialize(reader);
                        }
                        catch
                        {
                            continue;
                        }
                        serializer = serializer_;
                        break;
                    }
                }
            }
            if (rootNode == null)
            {
                return null;
            }
            Debug.Assert(serializer != null);
            var document = new Document(rootNode, serializer.GetType());
            return document;
        }

        public IEnumerable<Node?> Traverse(TraversalOrder order)
        {
            return RootNode.Traverse(order);
        }
    }
}
