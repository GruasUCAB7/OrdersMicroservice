using System.Text.RegularExpressions;

namespace OrdersMS.Core.Utils.RegExps
{
    public class PhoneNumberRegex
    {
        public static readonly Regex PhoneNumberRegexs = new Regex(@"^\+?[1-9]\d{1,14}$", RegexOptions.Compiled);

        internal static bool IsMatch(string phone)
        {
            throw new NotImplementedException();
        }
    }
}
