using System.Text.RegularExpressions;

namespace OrdersMS.Core.Utils.RegExps
{
    public class DNIRegex
    {
        public static readonly Regex DNIRegexs = new Regex(
        @"^[0-9]{7,8}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal static bool IsMatch(string identity)
        {
            return DNIRegexs.IsMatch(identity);
        }
    }
}
