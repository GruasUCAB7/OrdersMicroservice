using FirebaseAdmin;
using OrdersMS.Core.Application.Firebase;

namespace OrdersMS.Core.Infrastructure.Firebase
{
    public class FirebaseAppClient : IFirebaseAppClient
    {
        public void InitializeApp(AppOptions options)
        {
            FirebaseApp.Create(options);
        }
    }
}
