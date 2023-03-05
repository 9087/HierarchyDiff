using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HierarchyDiff
{
    internal class Utility
    {
        public static string Dump(object? @object)
        {
            switch (@object)
            {
                case null:
                    return "null";
                case bool @bool:
                    return @bool ? "true" : "false";
                case int @int:
                    return $"{@int}i";
                case float @float:
                    return $"{@float}f";
                case string @string:
                    return $"\"{@string}\"";
                default:
                    return @object?.ToString() ?? Dump(null);
            }
        }
    }
}
