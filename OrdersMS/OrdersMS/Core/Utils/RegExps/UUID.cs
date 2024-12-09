using System.Text.RegularExpressions;

namespace OrdersMS.Core.Utils.RegExps
{
    public static class UUIDRegExps
    {
        public static readonly Regex UUIDRegExp = new Regex(
            @"^[0-9A-Za-z]{8}-[0-9A-Za-z]{4}-4[0-9A-Za-z]{3}-[89ABab][0-9A-Za-z]{3}-[0-9A-Za-z]{12}$",
            RegexOptions.Compiled);
    }
}
