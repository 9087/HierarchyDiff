using Diskordia.UndoRedo;
using System.Diagnostics;

namespace HierarchyDiff.Core
{
    public sealed class Document
    {
        public object? Object { get; private set; }

        public FileFormat FileFormat { get; private set; }

        public bool Valid => Object != null && FileFormat != null;

        public string FilePath { get; private set; }

        private Document(FileFormat fileFormat, string filePath)
        {
            this.FileFormat = fileFormat;
            this.Object = fileFormat.Load(filePath);
            this.FilePath = filePath;
        }

        private void BuildTree()
        {
            if (!Valid || this.root != null)
            {
                return;
            }
            Debug.Assert(Object != null);
            Debug.Assert(FileFormat != null);
            this.root = FileFormat.GetRoot(this);
            var bfs = new Queue<TreeNode<object>>();
            bfs.Enqueue(this.root);
            while (bfs.Count > 0)
            {
                var node = bfs.Dequeue();
                if (node == null)
                {
                    continue;
                }
                node.SetChildren(this.FileFormat!.GetChildren(node));
                foreach (var child in node.GetChildren())
                {
                    bfs.Enqueue(child);
                }
            }
        }

        public static Document? Load(string filePath)
        {
            var extensionName = Path.GetExtension(filePath);
            if (extensionName.StartsWith('.'))
            {
                extensionName = extensionName.Substring(1);
            }
            var fileFormat = FileFormatRegistry.Instance.GetOrCreateFileFormat(extensionName);
            if (fileFormat == null)
            {
                return null;
            }
            var document = new Document(fileFormat, filePath);
            document.BuildTree();
            return document;
        }

        private TreeNode<object>? root = null;

        public TreeNode<object> GetRoot() => root!;

        #region Undo

        public UndoManager Undo { get; } = new UndoManager();

        public void Undoable_ChangeValue(object? node, string? value)
        {
            var oldValue = FileFormat.GetValue(node);
            FileFormat.SetValue(node, value ?? string.Empty);
            Undo.RegisterInvokation(this, document => document.Undoable_ChangeValue(node, oldValue));
        }

        #endregion
    }
}
