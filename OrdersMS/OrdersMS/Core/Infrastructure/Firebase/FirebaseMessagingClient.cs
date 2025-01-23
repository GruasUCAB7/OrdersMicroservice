using FirebaseAdmin.Messaging;
using OrdersMS.Core.Application.Firebase;

namespace OrdersMS.Core.Infrastructure.Firebase
{
    public class FirebaseMessagingClient : IFirebaseMessagingClient
    {
        public async Task<string> SendAsync(Message message)
        {
            return await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
    }
}
