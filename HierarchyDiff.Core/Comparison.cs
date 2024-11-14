using System.Diagnostics;

namespace HierarchyDiff.Core
{
    public sealed class Comparison
    {
        public List<Document> Documents { get; private set; }

        public FileFormat FileFormat { get; private set; }

        private ParallelTreeNode? root;

        public ParallelTreeNode GetRoot() { return root!; }

        private Comparison(List<Document> documents)
        {
            this.Documents = documents;
            Debug.Assert(this.Documents != null);
            Debug.Assert(this.Documents.Count >= 2);
            Debug.Assert(this.Documents.Select(x => x.FileFormat).ToHashSet().Count() == 1);
            this.FileFormat = documents[0].FileFormat;
        }

        private LongestCommonSubsequence<TreeNode<object>> GetTreeLongestCommonSubsequence(TreeNode<object> origin, TreeNode<object> target)
        {
            var originPreorderList =
                origin
                .Traverse(TraversalOrder.Preorder)
                .ToList();
            var targetPreorderList =
                target
                .Traverse(TraversalOrder.Preorder)
                .ToList();
            var subsequence = LongestCommonSubsequence<TreeNode<object>>.Generate(originPreorderList, targetPreorderList, this.FileFormat);
            return subsequence;
        }

        private void BuildTree()
        {
            var root0 = Documents[0].GetRoot();
            var root1 = Documents[1].GetRoot();
            var subsequence = GetTreeLongestCommonSubsequence(root0, root1);
            root = TreeNode<object>.Combine(root0, root1, subsequence, (node0, node1) => new ParallelTreeNode(this, node0, node1));
        }

        public static Comparison Create(Document @base, Document current)
        {
            var comparison = new Comparison(new() { @base, current, });
            comparison.BuildTree();
            return comparison;
        }

        public static Comparison Create(Document @base, Document theirs, Document mine)
        {
            var comparison = new Comparison(new() { @base, theirs, mine, });
            comparison.BuildTree();
            return comparison;
        }
    }
}
