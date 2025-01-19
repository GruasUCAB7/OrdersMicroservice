using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.src.Contracts.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace OrdersMS.src.Contracts.Domain.ValueObjects
{
    public class ClientEmail : IValueObject<ClientEmail>
    {
        private static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        private string Email { get; }

        public ClientEmail(string email)
        {
            if (!EmailRegex.IsMatch(email))
            {
                throw new InvalidClientEmailException();
            }
            Email = email;
        }

        public string GetValue()
        {
            return Email;
        }

        public bool Equals(ClientEmail other)
        {
            return Email == other.Email;
        }
    }
}
