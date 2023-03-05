using System.Diagnostics;


namespace HierarchyDiff.Core
{
    public class ComparingSession
    {
        private string originPath;
        public string OriginPath => originPath;
        
        private string targetPath;
        public string TargetPath => targetPath;

        private LongestCommonSubsequence preorderSubsequence;
        private LongestCommonSubsequence postorderSubsequence;

        private Dictionary<Node, Node> originToTarget = new();
        private Dictionary<Node, Node> targetToOrigin = new();

        private ParallelNode? root = null;
        private IEnumerator<Node?>? originEnumerator = null;
        private IEnumerator<Node?>? targetEnumerator = null;
        private Dictionary<Node, ParallelNode> cache = new();
        private bool originFinished = false;
        private bool targetFinished = false;

        public Document ParallelOriginDocument { get; private set; }
        
        public Document ParallelTargetDocument { get; private set; }

        public ComparingSession(string originPath, string targetPath)
        {
            this.originPath = Path.GetFullPath(originPath);
            this.targetPath = Path.GetFullPath(targetPath);
            Difference();
            Debug.Assert(preorderSubsequence != null);
            Debug.Assert(postorderSubsequence != null);
            Debug.Assert(ParallelOriginDocument != null);
            Debug.Assert(ParallelTargetDocument != null);
        }

        void PopulateNodeWithOrigin()
        {
            var originCurrent = originEnumerator!.Current;
            var parallelNode = new ParallelNode(originCurrent, null);
            if (originCurrent!.Parent != null)
            {
                var parentParallelNode = cache[originCurrent.Parent];
                parentParallelNode.AddChild(originCurrent.Type, parallelNode);
            }
            root ??= parallelNode;
            cache[originCurrent] = parallelNode;
            originFinished = !originEnumerator.MoveNext();
        }

        void PopulateNodeWithTarget()
        {
            var targetCurrent = targetEnumerator!.Current;
            var parallelNode = new ParallelNode(null, targetCurrent);
            if (targetCurrent!.Parent != null)
            {
                var parentParallelNode = cache[targetCurrent.Parent];
                parentParallelNode.AddChild(targetCurrent.Type, parallelNode);
            }
            root ??= parallelNode;
            cache[targetCurrent] = parallelNode;
            targetFinished = !targetEnumerator.MoveNext();
        }

        void PopulateNodeWithBoth()
        {
            var originCurrent = originEnumerator!.Current;
            var targetCurrent = targetEnumerator!.Current;

            var parallelNode = new ParallelNode(originCurrent, targetCurrent);
            var originParentParallelNode = originCurrent!.Parent == null ? null : cache[originCurrent.Parent];
            var targetParentParallelNode = targetCurrent!.Parent == null ? null : cache[targetCurrent.Parent];
            if (originParentParallelNode == null && targetParentParallelNode == null)
            {
            }
            else if (originParentParallelNode == null)
            {
                targetParentParallelNode!.AddChild(parallelNode);
            }
            else if (targetParentParallelNode == null)
            {
                originParentParallelNode!.AddChild(parallelNode);
            }
            else
            {
                if (ReferenceEquals(originParentParallelNode, targetParentParallelNode))
                {
                    originParentParallelNode.AddChild(parallelNode);
                }
                else if (originParentParallelNode.IsAncestorOf(targetParentParallelNode))
                {
                    targetParentParallelNode.AddChild(parallelNode);
                }
                else if (targetParentParallelNode.IsAncestorOf(originParentParallelNode))
                {
                    originParentParallelNode.AddChild(parallelNode);
                }
                else
                {
                    PopulateNodeWithOrigin();
                    PopulateNodeWithTarget();
                }
            }
            cache[originCurrent] = parallelNode;
            cache[targetCurrent] = parallelNode;
            root ??= parallelNode;
            originFinished = !originEnumerator.MoveNext();
            targetFinished = !targetEnumerator.MoveNext();
        }

        private void Difference()
        {
            var originDocument = Document.Load(originPath);
            if (originDocument == null)
            {
                throw new Exception();
            }
            var targetDocument = Document.Load(targetPath);
            if (targetDocument == null)
            {
                throw new Exception();
            }
            var originPreorderList = originDocument
                .Traverse(TraversalOrder.Preorder)
                .Select((node, index) =>
                {
                    if (node == null)
                    {
                        return null;
                    }
                    node.Preorder = index;
                    return node;
                })
                .ToList();
            var targetPreorderList = targetDocument
                .Traverse(TraversalOrder.Preorder)
                .Select((node, index) =>
                {
                    if (node == null)
                    {
                        return null;
                    }
                    node.Preorder = index;
                    return node;
                })
                .ToList();
#if DEBUG && VERBOSE
            Console.WriteLine("STEP 1: ");
#endif
            preorderSubsequence = LongestCommonSubsequence.Generate(originPreorderList, targetPreorderList);
#if DEBUG && VERBOSE
            Console.Write("Origin Preorder Common Subsequence: ");
            foreach (var node in preorderSubsequence.OriginSubsequence)
            {
                Console.Write($"{node} ");
            }
            Console.Write("\n");
            Console.Write("Target Preorder Common Subsequence: ");
            foreach (var node in preorderSubsequence.TargetSubsequence)
            {
                Console.Write($"{node} ");
            }
            Console.Write("\n");
#endif
            var originMatchedSet = preorderSubsequence.OriginSubsequence.ToHashSet();
            var targetMatchedSet = preorderSubsequence.TargetSubsequence.ToHashSet();

            var originPostorderList = originDocument
                .Traverse(TraversalOrder.Postorder)
                .Where(x => originMatchedSet.Contains(x))
                .ToList();
            var targetPostorderList = targetDocument
                .Traverse(TraversalOrder.Postorder)
                .Where(x => targetMatchedSet.Contains(x))
                .ToList();

#if DEBUG && VERBOSE
            Console.WriteLine("STEP 2: ");
#endif
            postorderSubsequence = LongestCommonSubsequence.Generate(originPostorderList, targetPostorderList);
#if DEBUG && VERBOSE
            Console.Write("Origin Postorder Common Subsequence: ");
            foreach (var node in postorderSubsequence.OriginSubsequence)
            {
                Console.Write($"{node} ");
            }
            Console.Write("\n");
            Console.Write("Target Postorder Common Subsequence: ");
            foreach (var node in postorderSubsequence.TargetSubsequence)
            {
                Console.Write($"{node} ");
            }
            Console.Write("\n");
#endif
            var finalSubsequence = postorderSubsequence;
            Debug.Assert(finalSubsequence.OriginSubsequence.Count == finalSubsequence.TargetSubsequence.Count);
            int count = finalSubsequence.OriginSubsequence.Count;
            for (int i = 0; i < count; i++)
            {
                var origin = finalSubsequence.OriginSubsequence[i];
                var target = finalSubsequence.TargetSubsequence[i];
                Debug.Assert(origin != null && target != null);
                originToTarget[origin] = target;
                targetToOrigin[target] = origin;
            }

            root = null;
            originEnumerator = originDocument.RootNode.Traverse(TraversalOrder.Preorder).GetEnumerator();
            targetEnumerator = targetDocument.RootNode.Traverse(TraversalOrder.Preorder).GetEnumerator();
            originFinished = !originEnumerator.MoveNext();
            targetFinished = !targetEnumerator.MoveNext();
            int commonSubsequenceIndex = 0;
            finalSubsequence.OriginSubsequence.Add(null);
            finalSubsequence.TargetSubsequence.Add(null);

            Dictionary<int, int> preorderMap = new();

            while (!originFinished || !targetFinished)
            {
                var origin = originEnumerator.Current;
                var target = targetEnumerator.Current;
                var subsequenceOrigin = finalSubsequence.OriginSubsequence[commonSubsequenceIndex];
                var subsequenceTarget = finalSubsequence.TargetSubsequence[commonSubsequenceIndex];
                if (!originFinished && !targetFinished && origin!.Preorder == subsequenceOrigin!.Preorder && target!.Preorder == subsequenceTarget!.Preorder)
                {
                    var originParent = origin!.Parent;
                    var targetParent = target!.Parent;
                    if (originParent == null && targetParent == null)
                    {
                        preorderMap[origin.Preorder] = target.Preorder;
                        PopulateNodeWithBoth();
                    }
                    else if (originParent == null)
                    {
                        throw new NotImplementedException();
                    }
                    else if (targetParent == null)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        if (preorderMap.TryGetValue(originParent.Preorder, out var targetParentPreorder) && targetParentPreorder == targetParent.Preorder)
                        {
                            preorderMap[origin.Preorder] = target.Preorder;
                            PopulateNodeWithBoth();
                        }
                        else
                        {
                            PopulateNodeWithOrigin();
                            PopulateNodeWithTarget();
                        }
                    }
                    commonSubsequenceIndex++;
                }
                else if (!originFinished && origin!.Preorder < (subsequenceOrigin?.Preorder ?? int.MaxValue))
                {
                    PopulateNodeWithOrigin();
                }
                else if (!targetFinished && target!.Preorder < (subsequenceTarget?.Preorder ?? int.MaxValue))
                {
                    PopulateNodeWithTarget();
                }
                else
                {
                    throw new Exception();
                }
            }
            ParallelOriginDocument = new Document(new PartialNode(root!, 0), originDocument.SerializationType);
            ParallelTargetDocument = new Document(new PartialNode(root!, 1), targetDocument.SerializationType);
        }
    }
}
