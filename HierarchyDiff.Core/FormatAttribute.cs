namespace HierarchyDiff.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class FormatAttribute : Attribute
    {
        public string ExtensionName { get; set; }

        public FormatAttribute(string extensionName)
        {
            this.ExtensionName = extensionName;
        }
    }
}
