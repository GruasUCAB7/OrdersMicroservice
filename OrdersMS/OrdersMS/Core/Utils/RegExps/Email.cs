using System.Text.RegularExpressions;

namespace OrdersMS.Core.Utils.RegExps
{
    public class Email
    {
        public static readonly Regex EmailRegex = new Regex(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }
}
