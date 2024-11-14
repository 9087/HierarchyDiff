using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HierarchyDiff.Core
{
    internal class LongestCommonSubsequence<T> where T : ITreeNode
    {
        public List<T> OriginSubsequence { get; private set; }

        public List<T> TargetSubsequence { get; private set; }

        private LongestCommonSubsequence(List<T> originSubsequence, List<T> targetSubsequence)
        {
            this.OriginSubsequence = originSubsequence;
            this.TargetSubsequence = targetSubsequence;
        }

        private enum Direction
        {
            None,
            Horizontal,
            Vertical,
            Tilt,
        }

        private struct Score
        {
            public Direction Direction;
            public float Value;
            public float Similarity;

            public Score(Direction direction, float value, float similarity)
            {
                this.Direction = direction;
                this.Value = value;
                this.Similarity = similarity;
            }
        }

        public static LongestCommonSubsequence<T> Generate(List<T> origins, List<T> targets, FileFormat fileFormat)
        {
            var scores = new Score[origins.Count, targets.Count];
            int o = 0;
            int t;
            foreach (var origin in origins)
            {
                t = 0;
                foreach (var target in targets)
                {
                    var similarity = fileFormat.Compare(origin.GetObject(), target.GetObject());
                    if (similarity < 0 || similarity > 1)
                    {
                        throw new Exception("The return value of Compare must be between 0 and 1.");
                    }
                    float tilt = (o == 0 || t == 0) ? 0 : scores[o - 1, t - 1].Value + similarity;
                    float h = (t == 0) ? 0 : scores[o, t - 1].Value;
                    float v = (o == 0) ? 0 : scores[o - 1, t].Value;
                    if (tilt >= h && tilt >= v)
                    {
                        scores[o, t] = new(Direction.Tilt, tilt, similarity);
                    }
                    else if (h >= tilt && h >= v)
                    {
                        scores[o, t] = new(Direction.Vertical, h, similarity);
                    }
                    else if (v >= tilt && v >= h)
                    {
                        scores[o, t] = new(Direction.Horizontal, v, similarity);
                    }
                    else
                    {
                        throw new Exception();
                    }
                    t++;
                }
                o++;
            }
            o = origins.Count - 1;
            t = targets.Count - 1;
            List<T> originSubsequence = new();
            List<T> targetSubsequence = new();
            while (o >= 0 && t >= 0)
            {
                switch (scores[o, t].Direction)
                {
                    case Direction.Tilt:
                        originSubsequence.Add(origins[o]);
                        targetSubsequence.Add(targets[t]);
                        o--;
                        t--;
                        break;
                    case Direction.Horizontal:
                        o--;
                        break;
                    case Direction.Vertical:
                        t--;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            originSubsequence.Reverse();
            targetSubsequence.Reverse();
            return new(originSubsequence, targetSubsequence);
        }
    }
}
