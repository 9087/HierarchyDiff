using System.Xml.Linq;

namespace HierarchyDiff.Core
{
    public class ParallelNode : Node
    {
        public override Name Name => Name.Empty;

        public override object? Value => null;

        protected Enum type;

        public override Enum Type => type;

        public override Image Icon => Image.Empty;

        protected override Dictionary<Enum, IList<Node?>> OnPopulateChildren()
        {
            return new();
        }

        public Node?[] Nodes { get; private set; }

        public ParallelNode(params Node?[] nodes)
        {
            var types = nodes.Where(x => x != null).Select(x => x!.Type).ToHashSet();
            if (types.Count != 1)
            {
                throw new ArgumentException();
            }
            this.type = types.First();
            Nodes = nodes;
        }

        public override bool Equals(Node? other)
        {
            if (other == null)
            {
                return false;
            }
            else if (other is ParallelNode parallelNode)
            {
                return ReferenceEquals(this, parallelNode);
            }
            else
            {
                return false;
            }
        }

        public void AddChild(Enum type, ParallelNode node)
        {
            if (!this.Children.TryGetValue(type, out var list))
            {
                list = new List<Node?>();
                this.Children.Add(type, list);
            }
            list.Add(node);
        }

        public void AddChild(ParallelNode node)
        {
            AddChild(node.Type, node);
        }
    }
}
