namespace OrdersMS.Core.Application.Crypto
{
    public interface ICrypto
    {
        Task<string> Encrypt(string value);

        Task<bool> Compare(string text, string encrypted);
    }
}