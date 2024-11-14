namespace HierarchyDiff.Core
{
    public abstract class FileFormat
    {
        public abstract object? Load(string filePath);

        public abstract void Save(object document, string filePath);

        protected abstract object GetRoot(object document);

        public TreeNode<object> GetRoot(Document document)
        {
            return TreeNode<object>.Create(this.GetRoot(document.Object!));
        }

        protected abstract IEnumerable<object> GetChildren(object node);

        public List<TreeNode<object>> GetChildren(TreeNode<object> node)
        {
            return this.GetChildren(node.Object!)
                .Select(x => TreeNode<object>.Create(x).Link(node))
                .ToList();
        }

        public abstract float Compare(object a, object b);

        public abstract string ToString(object? node);

        public abstract string GetName(object? node);

        public abstract string? GetValue(object? node);

        public abstract bool SetValue(object? node, string value);

        public abstract TreeNodeStyle GetStyle(object? node, TreeNodeStyle? style);
    }
}
