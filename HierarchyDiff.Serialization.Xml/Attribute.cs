using HierarchyDiff.Core;

namespace HierarchyDiff.Serialization.Xml
{
    public class Attribute : Node
    {
        protected string name;

        public override Name Name => new Name($"{name}");

        public override Enum Type => NodeType.Attribute;

        public override Image Icon => Icons.Property;

        protected object value;

        public override object Value => value;

        protected override Dictionary<Enum, IList<Node?>> OnPopulateChildren()
        {
            return new();
        }

        public Attribute(string name, object value)
        {
            this.name = name;
            this.value = value;
        }

        public override bool Equals(Node? other)
        {
            if (other is null)
            {
                return false;
            }
            if (other is Attribute attribute)
            {
                return Name == attribute.Name && value == attribute.Value;
            }
            else
            {
                return false;
            }
        }
    }
}
