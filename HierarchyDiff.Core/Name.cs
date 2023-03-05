using System.Drawing;

namespace HierarchyDiff.Core
{
    public class Name : IEquatable<Name>
    {
        protected string? name;
        private Style? normal = null;
        private Style? selected = null;

        public Style? Normal { get => normal; set => normal = value; }

        public Style? Selected { get => selected ?? normal; set => selected = value; }

        public Name(string? name)
        {
            this.name = name;
        }

        public static Name Empty => new Name(null);

        public override bool Equals(object? @object)
        {
            return Equals(@object as Name);
        }

        public static bool Equals(Name? a, Name? b)
        {
            if (a is null)
            {
                return b is null;
            }
            return a.Equals(b);
        }

        public static bool operator ==(Name? a, Name? b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(Name? a, Name? b)
        {
            return !(a == b);
        }

        public bool Equals(Name? other)
        {
            if (other == null)
            {
                return false;
            }
            if (other is Name name_)
            {
                return name == name_.name;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return name == null ? 0 : name.GetHashCode();
        }

        public override string ToString()
        {
            return name == null ? "null" : name.ToString();
        }

        public string ToMarkup(bool selected)
        {
            var current = (selected ? Selected : Normal) ?? Normal;
            var backgroundColor = current?.BackgroundColor ?? Normal?.BackgroundColor ?? Color.Transparent;
            var textColor = current?.TextColor ?? Normal?.TextColor ?? Color.Black;
            var padding = current?.Padding ?? Normal?.Padding ?? 0;
            var italic = current?.Italic ?? Normal?.Italic ?? false;
            var bold = current?.Bold ?? Normal?.Bold ?? false;
            var format = current?.Format ?? Normal?.Format ?? "{0}";
            var markup = "<span"
                + (backgroundColor == Color.Transparent
                    ? ""
                    : $" background='#{backgroundColor.ToRgbCode()}'")
                + (textColor == Color.Transparent
                    ? ""
                    : $" color='#{textColor.ToRgbCode()}'")
                + ">"
                + new string(' ', padding)
                + string.Format(format, ToString())
                + new string(' ', padding)
                + "</span>";
            if (italic)
            {
                markup = "<i>" + markup + "</i>";
            }
            if (bold)
            {
                markup = "<b>" + markup + "</b>";
            }
            return markup;
        }
    }
}
