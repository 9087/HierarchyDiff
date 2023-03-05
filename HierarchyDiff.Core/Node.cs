using System.Collections;

namespace HierarchyDiff.Core
{
    public abstract class Node : IEquatable<Node>, IEnumerable<Node>
    {
        public abstract Name Name { get; }

        public abstract Enum Type { get; }

        public abstract Image Icon { get; }

        public int Preorder { get; internal set; } = -1;

        private Dictionary<Enum, IList<Node?>>? children = null;

        public virtual Dictionary<Enum, IList<Node?>> Children
        {
            get
            {
                if (children == null)
                {
                    children = OnPopulateChildren();
                    foreach (var type in children.Keys)
                    {
                        foreach (var child in children[type])
                        {
                            if (child != null)
                            {
                                child.Parent = this;
                            }
                        }
                    }
                }
                return children;
            }
        }

        public Node? Parent { get; private set; }

        protected abstract Dictionary<Enum, IList<Node?>> OnPopulateChildren();

        public abstract object? Value { get; }

        public override string ToString()
        {
            return $"<{this.Name}>";
        }

        public IEnumerable<Node?> Traverse(TraversalOrder order)
        {
            return Traverse(this, order);
        }

        public static IEnumerable<Node?> Traverse(Node? node, TraversalOrder order)
        {
            yield return node;
            if (node == null)
            {
                yield break;
            }
            foreach (var child in node)
            {
                foreach (var n in Traverse(child, order))
                {
                    yield return n;
                }
            }
        }

        public virtual bool Equals(Node? other)
        {
            if (other == null)
            {
                return false;
            }
            return ReferenceEquals(this.Parent, other.Parent);
        }

        public static bool operator ==(Node? a, Node? b)
        {
            if (a is null)
            {
                return b is null;
            }
            return a.Equals(b);
        }

        public static bool operator !=(Node? a, Node? b)
        {
            return !(a == b);
        }

        public override bool Equals(object? @object)
        {
            return Equals(@object as Node);
        }

        public override int GetHashCode()
        {
            var hashCode = Name.GetHashCode();
            if (Value != null)
            {
                hashCode ^= Value.GetHashCode();
            }
            return hashCode;
        }

        public IEnumerator<Node> GetEnumerator()
        {
            foreach (var type in this.Children.Keys)
            {
                foreach (var child in this.Children[type])
                {
                    yield return child!;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool IsAncestorOf(Node? node)
        {
            while (node != null)
            {
                node = node.Parent;
                if (ReferenceEquals(this, node))
                {
                    return true;
                }
            }
            return false;
        }
    }
}