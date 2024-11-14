using Diskordia.UndoRedo;
using System.Diagnostics;

namespace HierarchyDiff.Core
{
    public class ParallelTreeNode : TreeNode<Tuple<object?, object?>>
    {
        public Comparison Comparison { get; private set; }

        public FileFormat FileFormat => Comparison.FileFormat;

        private readonly List<object?> objects = new();

        public ParallelTreeNode(Comparison comparison, TreeNode<object>? node0, TreeNode<object>? node1)
            : base(new Tuple<object?, object?>(node0?.Object, node1?.Object))
        {
            objects.Add(node0?.Object);
            objects.Add(node1?.Object);
            Comparison = comparison;
        }

        public object? Get(SlotIndex index) => objects[(int)index];

        public void Undoable_ChangeValue(SlotIndex index, string value)
        {
            var document = Comparison.Documents[(int)index]!;
            var oldValue = GetValue(index);
            SetValue(index, value);
            document.Undo.RegisterInvokation(this, node => node.Undoable_ChangeValue(index, oldValue));
        }

        public TreeNodeStyle GetStyle(SlotIndex index, TreeNodeStyle @default)
        {
            return FileFormat.GetStyle(Get(index), @default);
        }

        public string GetName(SlotIndex index)
        {
            return FileFormat.GetName(Get(index));
        }

        public Action<SlotIndex, string>? ValueChanged { get; set; } = null;

        public string GetValue(SlotIndex index)
        {
            return FileFormat.GetValue(Get(index)) ?? string.Empty;
        }

        public void SetValue(SlotIndex index, string value)
        {
            if (GetValue(index) == value)
            {
                return;
            }
            FileFormat.SetValue(Get(index), value);
            ValueChanged?.Invoke(index, value);
        }

        public bool IsNull(SlotIndex index)
        {
            return Get(index) == null;
        }

        public void GetPath(List<ParallelTreeNode> path)
        {
            path.Clear();
            var current = this;
            while (current != null)
            {
                path.Add(current);
                current = current.Parent as ParallelTreeNode;
            }
            path.Reverse();
        }

        public List<ParallelTreeNode> GetPath()
        {
            List<ParallelTreeNode> path = new();
            this.GetPath(path);
            return path;
        }

        public bool IsPredecessorOf(ParallelTreeNode other)
        {
            if (other == this)
            {
                return false;
            }
            if (this.Comparison != other.Comparison)
            {
                return false;
            }
            var thisPath = this.GetPath();
            var otherPath = other.GetPath();
            Debug.Assert(thisPath.First() == otherPath.First());
            var sharedParent = thisPath.First();
            for (int i = 1; ; i++)
            {
                var thisCurrent = i >= thisPath.Count ? null : thisPath[i];
                var otherCurrent = i >= otherPath.Count ? null : otherPath[i];
                if (thisCurrent == otherCurrent)
                {
                    Debug.Assert(thisCurrent != null);
                    sharedParent = thisCurrent!;
                    continue;
                }
                if (thisCurrent == null)
                {
                    return true;
                }
                if (otherCurrent == null)
                {
                    return false;
                }
                var thisIndex = sharedParent.GetChildren().IndexOf(thisCurrent);
                var otherIndex = sharedParent.GetChildren().IndexOf(otherCurrent);
                Debug.Assert(thisIndex != otherIndex);
                if (thisIndex < otherIndex)
                {
                    return true;
                }
                if (thisIndex > otherIndex)
                {
                    return false;
                }
            }
        }

        public ParallelTreeNode? GetNextSibling()
        {
            var parent = this.Parent;
            if (parent == null)
            {
                return null;
            }
            int index = parent.GetChildren().IndexOf(this);
            if (index + 1 >= parent.GetChildren().Count)
            {
                return null;
            }
            return parent.GetChildren()[index + 1] as ParallelTreeNode;
        }

        public bool Traverse(Func<ParallelTreeNode, bool> callback)
        {
            if (!callback(this))
            {
                return false;
            }
            var children = GetChildren();
            if (children != null)
            {
                foreach (ParallelTreeNode child in children)
                {
                    if (!child.Traverse(callback))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
