using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HierarchyDiff
{
    public class HierarchyParallelModel : IEnumerable<HierarchyModel>
    {
        private List<HierarchyModel> models;

        public HierarchyModel this[int index]
        {
            get
            {
                return models[index];
            }
        }

        public HierarchyParallelModel(uint count)
        {
            models = Enumerable
                .Repeat<object?>(null, (int)count)
                .Select(_ => new HierarchyModel())
                .ToList();
        }

        public IEnumerator<HierarchyModel> GetEnumerator()
        {
            return models.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return models.GetEnumerator();
        }
    }
}
