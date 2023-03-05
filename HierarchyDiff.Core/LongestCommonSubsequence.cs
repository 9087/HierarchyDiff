using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HierarchyDiff.Core
{
    internal enum Direction
    {
        None,
        Horizontal,
        Vertical,
        Tilt,
    }

    internal struct Score
    {
        public Direction Direction;
        public float Value;
    }

    public class LongestCommonSubsequence
    {
        public List<Node?> OriginSubsequence { get; private set; }

        public List<Node?> TargetSubsequence { get; private set; }

        public LongestCommonSubsequence(List<Node?> originSubsequence, List<Node?> targetSubsequence)
        {
            this.OriginSubsequence = originSubsequence;
            this.TargetSubsequence = targetSubsequence;
        }

        public static LongestCommonSubsequence Generate(List<Node?> originList, List<Node?> targetList)
        {
            var scores = new Score[originList.Count, targetList.Count];
            int o = 0;
            int t;
            foreach (var origin in originList)
            {
                t = 0;
                foreach (var target in targetList)
                {
                    Debug.Assert(origin != null && target != null);
                    var similarity = Similarity.Calculate(origin, target);
                    float tilt = (o == 0 || t == 0) ? 0 : scores[o - 1, t - 1].Value + similarity;
                    float up = (t == 0) ? 0 : scores[o, t - 1].Value;
                    float left = (o == 0) ? 0 : scores[o - 1, t].Value;
                    if (tilt >= up && tilt >= left)
                    {
                        scores[o, t] = new Score { Direction = Direction.Tilt, Value = tilt, };
                    }
                    else if (up >= tilt && up >= left)
                    {
                        scores[o, t] = new Score { Direction = Direction.Vertical, Value = up, };
                    }
                    else if (left >= tilt && left >= up)
                    {

                        scores[o, t] = new Score { Direction = Direction.Horizontal, Value = left, };
                    }
                    else
                    {
                        throw new Exception();
                    }
                    t++;
                }
                o++;
            }
            PrintLongestCommonSubsequenceScoreTable(originList, targetList, scores);
            o = originList.Count - 1;
            t = targetList.Count - 1;
            List<Node?> originSubsequence = new();
            List<Node?> targetSubsequence = new();
            while (o >= 0 && t >= 0)
            {
                switch (scores[o, t].Direction)
                {
                    case Direction.Tilt:
                        originSubsequence.Add(originList[o]);
                        targetSubsequence.Add(targetList[t]);
                        o--;
                        t--;
                        break;
                    case Direction.Horizontal:
                        o--;
                        break;
                    case Direction.Vertical:
                        t--;
                        break;
                }
            }
            originSubsequence.Reverse();
            targetSubsequence.Reverse();
            return new LongestCommonSubsequence(originSubsequence, targetSubsequence);
        }

        private static void PrintCell(string? x)
        {
            const int CellSize = 16;
            var s = $"{x}";
            Console.Write(s + string.Join("", Enumerable.Repeat(" ", CellSize - s.Length)) + " ");
        }

        private static void PrintLongestCommonSubsequenceScoreTable(List<Node?> originList, List<Node?> targetList, Score[,] scores)
        {
#if !DEBUG || !VERBOSE
            return;
#endif
            for (int t = 0; t < targetList.Count; t++)
            {
                if (t == 0)
                {
                    PrintCell("");
                    for (int o = 0; o < originList.Count; o++)
                    {
                        PrintCell(originList[o]?.ToString());
                    }
                    Console.Write("\n");
                }
                for (int o = 0; o < originList.Count; o++)
                {
                    if (o == 0)
                    {
                        PrintCell(targetList[t]?.ToString());
                    }
                    PrintCell($"({scores[o, t].Value},{scores[o, t].Direction.ToString()[0]}) ");
                }
                Console.Write("\n");
            }
        }
    }
}
