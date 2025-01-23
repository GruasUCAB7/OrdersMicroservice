using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;
using OrdersMS.Core.Application.Firebase;

namespace OrdersMS.Core.Infrastructure.Firebase
{
    
public class FirebaseMessagingService : IFirebaseMessagingService
    {
        private readonly ILogger<FirebaseMessagingService> _logger;
        private readonly FirebaseMessagingSettings _firebaseMessagingSettings;
        private readonly IFirebaseMessagingClient _firebaseMessagingClient;
        private readonly IFirebaseAppClient _firebaseAppClient;

        public FirebaseMessagingService(
            ILogger<FirebaseMessagingService> logger,
            IOptions<FirebaseMessagingSettings> firebaseMessagingSettings,
            IFirebaseMessagingClient firebaseMessagingClient,
            IFirebaseAppClient firebaseAppClient)
        {
            _logger = logger;
            _firebaseMessagingSettings = firebaseMessagingSettings.Value;
            _firebaseMessagingClient = firebaseMessagingClient;
            _firebaseAppClient = firebaseAppClient;

            _firebaseAppClient.InitializeApp(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(@"C:\Users\joywg\Downloads\gruasucab-179d3-firebase-adminsdk-6u5r3-0069a51909.json")
            });
        }

        public async Task SendPushNotificationAsync(string deviceToken, string? messageTitle, string? messageBody)
        {
            try
            {
                _logger.LogInformation("FirebaseMessagingService.SendPushNotificationAsync  {Object}", deviceToken);

                var message = new Message()
                {
                    Token = deviceToken,
                    Notification = new Notification() { Title = messageTitle, Body = messageBody },
                    Android = new AndroidConfig()
                    {
                        Priority = Priority.High,
                        Notification = new AndroidNotification()
                        {
                            Title = messageTitle,
                            Body = messageBody,
                            Priority = NotificationPriority.HIGH,
                            Sound = _firebaseMessagingSettings.MessageSound,
                            DefaultSound = false,
                            ChannelId = _firebaseMessagingSettings.ChannelId
                        }
                    }
                };

                await _firebaseMessagingClient.SendAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error SendPushNotificationAsync. {Message}", ex.Message);
            }
        }
    }
}
