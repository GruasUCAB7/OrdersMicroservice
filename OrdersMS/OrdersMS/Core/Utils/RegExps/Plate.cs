using System.Text.RegularExpressions;

namespace OrdersMS.Core.Utils.RegExps
{
    public class PlateRegex
    {
        public static readonly Regex PlateRegexs = new Regex(
        @"^[A-Z]{2}[0-9]{3}[A-Z]{2}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal static bool IsMatch(string rif)
        {
            return PlateRegexs.IsMatch(rif);
        }
    }
}
