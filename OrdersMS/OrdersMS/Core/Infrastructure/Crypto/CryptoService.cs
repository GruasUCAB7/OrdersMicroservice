using OrdersMS.Core.Application.Crypto;

namespace OrdersMS.Core.Infrastructure.Crypto
{
    public class BcryptCryptoService : Application.Crypto.ICrypto
    {
        public Task<string> Encrypt(string value)
        {
            return Task.FromResult(BCrypt.Net.BCrypt.HashPassword(value));
        }

        public Task<bool> Compare(string text, string encrypted)
        {
            return Task.FromResult(BCrypt.Net.BCrypt.Verify(text, encrypted));
        }
    }
}
