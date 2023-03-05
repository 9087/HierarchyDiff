using System.Diagnostics;
using System.Xml.Linq;

namespace HierarchyDiff.Core
{
    public enum DifferenceType
    {
        None,
        Add,
        Remove,
        Modify,
        Same,
    }

    public class PartialNode : Node
    {
        public ParallelNode ParallelNode { get; private set; }

        public Node? Current => ParallelNode.Nodes[Index];

        public Node? Other
        {
            get
            {
                Debug.Assert(Index <= 1);
                return ParallelNode.Nodes[1 - Index];
            }
        }

        public override Name Name => Current!.Name;

        public override Enum Type => Current!.Type;

        public override Image Icon => Current!.Icon;

        public override object? Value => Current?.Value ?? null;

        public int Index { get; private set; }

        public DifferenceType DifferenceType
        {
            get
            {
                var parallelNode = this.ParallelNode;
                Debug.Assert(parallelNode.Nodes.Count() == 2);
                if (this.Current == null)
                {
                    return DifferenceType.None;
                }
                if (this.Index == 0 && parallelNode.Nodes[1] == null)
                {
                    return DifferenceType.Remove;
                }
                else if (this.Index == 1 && parallelNode.Nodes[0] == null)
                {
                    return DifferenceType.Add;
                }
                else
                {
                    var node0 = parallelNode.Nodes[0];
                    var node1 = parallelNode.Nodes[1];
                    Debug.Assert(node0 != null);
                    Debug.Assert(node1 != null);
                    if (node0.Value == null ^ node1.Value == null)
                    {
                        return DifferenceType.Modify;
                    }
                    else if (node0.Value != null && node1.Value != null)
                    {
                        if (node0.Value.GetType() != node1.Value.GetType() || !node0.Value.Equals(node1.Value))
                        {
                            return DifferenceType.Modify;
                        }
                    }
                }
                return DifferenceType.Same;
            }
        }

        public PartialNode(ParallelNode parallelNode, int index)
        {
            this.ParallelNode = parallelNode;
            this.Index = index;
        }

        protected override Dictionary<Enum, IList<Node?>> OnPopulateChildren()
        {
            Dictionary<Enum, IList<Node?>> children = new();
            foreach (var child in ParallelNode)
            {
                if (!children.TryGetValue(child.Type, out var list))
                {
                    list = new List<Node?>();
                    children[child.Type] = list;
                }
                var typed = child as ParallelNode;
                Debug.Assert(typed != null);
                list.Add(new PartialNode(typed, Index));
            }
            return children;
        }


        public override bool Equals(Node? other)
        {
            if (other == null)
            {
                return false;
            }
            if (other is PartialNode partialNode)
            {
                return ReferenceEquals(ParallelNode, partialNode.ParallelNode);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return ParallelNode.GetHashCode() ^ Index.GetHashCode();
        }
    }
}
