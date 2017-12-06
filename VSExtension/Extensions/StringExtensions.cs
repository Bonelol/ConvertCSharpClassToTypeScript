using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSExtension.Extensions
{
    internal static class StringExtensions
    {
        // Convert the string to Pascal case.
        public static string ToPascalCase(this string name)
        {
            // If there are 0 or 1 characters, just return the string.
            if (name == null) return null;
            if (name.Length < 2) return name.ToUpper();

            // Split the string into words.
            var words = name.Split(new char[] { }, StringSplitOptions.RemoveEmptyEntries);

            // Combine the words.

            return words.Aggregate("", (current, word) => current + word.Substring(0, 1).ToUpper() + word.Substring(1));
        }

        // Convert the string to camel case.
        public static string ToCamelCase(this string name)
        {
            // If there are 0 or 1 characters, just return the string.
            if (name == null || name.Length < 2)
                return name;

            // Split the string into words.
            var words = name.Split(new char[] { }, StringSplitOptions.RemoveEmptyEntries);

            // Combine the words.
            var result = words[0].ToLower();

            for (var i = 1; i < words.Length; i++)
            {
                result += words[i].Substring(0, 1).ToUpper() + words[i].Substring(1);
            }

            return result;
        }
    }
}
