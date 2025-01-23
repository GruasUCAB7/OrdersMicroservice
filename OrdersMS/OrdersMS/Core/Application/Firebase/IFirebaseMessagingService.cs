using FirebaseAdmin.Messaging;
using FirebaseAdmin;

namespace OrdersMS.Core.Application.Firebase
{
    public interface IFirebaseMessagingService
    {
        Task SendPushNotificationAsync(string deviceToken, string? messageTitle, string? messageBody);
    }

    public interface IFirebaseMessagingClient
    {
        Task<string> SendAsync(Message message);
    }

    public interface IFirebaseAppClient
    {
        void InitializeApp(AppOptions options);
    }
}
