using System.Diagnostics;
using System.Xml.Linq;

namespace HierarchyDiff.Core
{
    public enum TraversalOrder
    {
        Preorder,
        Postorder,
    }

    public enum TreeNodeType
    {
        Element = 0,
        Attribute = 1,
    }

    public interface ITreeNode
    {
        public object GetObject();
    }

    public class TreeNode<T> : ITreeNode where T : class
    {
        public TreeNode<T>? Parent { get; private set; }

        public T Object { get; private set; }

        public object GetObject() => (object)Object!;

        protected TreeNode(T @object)
        {
            this.Object = @object;
        }

        public static TreeNode<T> Create(T @object)
        {
            return new TreeNode<T>(@object);
        }

        internal TreeNode<T> Link(TreeNode<T> parent)
        {
            this.Parent = parent;
            return this;
        }

        private List<TreeNode<T>>? children;

        public List<TreeNode<T>> GetChildren()
        {
            return children!;
        }

        public int GetChildCount()
        {
            return children?.Count ?? 0;
        }

        public U? GetChildChecked<U>(int index) where U : TreeNode<T>
        {
            if (children == null || index < 0 || index >= children.Count)
            {
                return null;
            }
            Debug.Assert(children[index] is U);
            return children[index] as U;
        }

        internal void SetChildren(List<TreeNode<T>> children)
        {
            this.children = children;
        }

        public void AddChild(TreeNode<T> child)
        {
            this.children ??= new List<TreeNode<T>>();
            this.children.Add(child);
            child.Parent = this;
        }

        public bool IsAncestorOf(TreeNode<T>? node)
        {
            while (node != null)
            {
                node = node.Parent;
                if (node == this)
                {
                    return true;
                }
            }
            return false;
        }

        public override string ToString()
        {
            return this.Object?.ToString() ?? "None";
        }

        public IEnumerable<TreeNode<T>> Traverse(TraversalOrder order)
        {
            return Traverse(this, order);
        }

        public static IEnumerable<TreeNode<T>> Traverse(TreeNode<T> node, TraversalOrder order)
        {
            if (order == TraversalOrder.Preorder)
            {
                yield return node;
            }
            foreach (var child in node.GetChildren())
            {
                foreach (var n in Traverse(child, order))
                {
                    yield return n;
                }
            }
            if (order == TraversalOrder.Postorder)
            {
                yield return node;
            }
        }

        public TreeNode<TResult> Select<TResult>(Func<TreeNode<T>, TreeNode<TResult>> predicate) where TResult : class
        {
            TreeNode<TResult> node = predicate(this);
            node.SetChildren(this.GetChildren().Select(x => x.Select<TResult>(predicate)).ToList());
            return node!;
        }

        private class Iterator
        {
            private IEnumerator<TreeNode<T>> enumerator;

            public bool Ended { get; private set; }

            public Iterator(IEnumerator<TreeNode<T>> enumerator)
            {
                this.enumerator = enumerator;
                MoveNext();
            }

            public TreeNode<T> Current => this.enumerator.Current;

            public bool MoveNext()
            {
                this.Ended = !enumerator.MoveNext();
                return this.Ended;
            }
        }

        internal static U Combine<U>(TreeNode<T> root0, TreeNode<T> root1, LongestCommonSubsequence<TreeNode<T>> subsequence, Func<TreeNode<T>?, TreeNode<T>?, U> construct) where U : TreeNode<Tuple<T?, T?>>
        {
            U? root = default(U);
            var subsequenceIterator0 = new Iterator(subsequence.OriginSubsequence.GetEnumerator());
            var subsequenceIterator1 = new Iterator(subsequence.TargetSubsequence.GetEnumerator());
            var iterator0 = new Iterator(root0.Traverse(TraversalOrder.Preorder).GetEnumerator());
            var iterator1 = new Iterator(root1.Traverse(TraversalOrder.Preorder).GetEnumerator());
            Dictionary<TreeNode<T>, U> Map = new();
            while (true)
            {
                TreeNode<T>? node0 = null;
                TreeNode<T>? node1 = null;
                if (!iterator0.Ended && iterator0.Current != subsequenceIterator0.Current)
                {
                    node0 = iterator0.Current;
                    iterator0.MoveNext();
                }
                else if (!iterator1.Ended && iterator1.Current != subsequenceIterator1.Current)
                {
                    node1 = iterator1.Current;
                    iterator1.MoveNext();
                }
                else if (!iterator0.Ended && !iterator1.Ended)
                {
                    Debug.Assert(iterator0.Current == subsequenceIterator0.Current);
                    Debug.Assert(iterator1.Current == subsequenceIterator1.Current);
                    node0 = iterator0.Current;
                    node1 = iterator1.Current;
                    iterator0.MoveNext();
                    iterator1.MoveNext();
                    subsequenceIterator0.MoveNext();
                    subsequenceIterator1.MoveNext();
                }
                else
                {
                    break;
                }
                var combined = construct(node0, node1);
                if (node0 != null) { Map[node0] = combined; }
                if (node1 != null) { Map[node1] = combined; }
                var parent0 = node0?.Parent;
                var parent1 = node1?.Parent;
                if (parent0 == null && parent1 == null)
                {
                    root = combined;
                }
                else if (parent0 == null)  // parent1 != null
                {
                    Map.GetValueOrDefault(parent1!)!.AddChild(combined);
                }
                else if (parent1 == null)  // parent0 != null
                {
                    Map.GetValueOrDefault(parent0!)!.AddChild(combined);
                }
                else // parent0 != null && parent1 != null
                {
                    var p0 = Map.GetValueOrDefault(parent0!)!;
                    var p1 = Map.GetValueOrDefault(parent1!)!;
                    if (p0.IsAncestorOf(p1)) { p1.AddChild(combined); } else { p0.AddChild(combined); }
                }
            }
            return root!;
        }
    }
}
