namespace HierarchyDiff.Core
{
    public interface ISerializer
    {
        public void Serialize(Node root, StreamWriter writer);

        public Node Deserialize(StreamReader reader);
    }
}