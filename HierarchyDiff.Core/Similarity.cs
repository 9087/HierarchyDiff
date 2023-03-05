namespace HierarchyDiff.Core
{
    public static class Similarity
    {
        private static int N = 2;

        public static float Calculate(Node origin, Node target)
        {
            if (!origin.Type.Equals(target.Type) || origin.Name != target.Name)
            {
                return 0;
            }
            float score = 0.01f;
            score += origin.Name == target.Name ? 1 : 0;
            if (origin.Value == null && target.Value == null)
            {
                score += 1;
            }
            else if (origin.Value != null && target.Value != null)
            {
                score += origin.Value.Equals(target.Value) ? 1 : 0;
            }
            else
            {
            }
            return 1 - MathF.Pow(1 - MathF.Min((float)score / N, 2), 1);
        }
    }
}
