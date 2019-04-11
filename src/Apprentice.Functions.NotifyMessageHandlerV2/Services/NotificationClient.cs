using System.Collections.Generic;
using System.Threading.Tasks;
using Notify.Models;
using Notify.Models.Responses;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Services
{
    public class NotificationClient : INotificationClient
    {
        private readonly Notify.Client.NotificationClient _client;        

        public NotificationClient(Notify.Client.NotificationClient notificationClient)
        {
            _client = notificationClient;
        }

        public Task<NotificationList> GetNotifications(string templateType = "", string status = "", string reference = "", string olderThanId = "")
        {
            return Task.Run(
               () => _client.GetNotifications(
                   templateType,
                   status,
                   reference,
                   olderThanId));
        }

        public Task<SmsNotificationResponse> SendSms(string mobileNumber, string templateId, Dictionary<string, dynamic> personalisation = null, string clientReference = null, string smsSenderId = null)
        {
            return Task.Run(
                () => _client.SendSms(
                    mobileNumber,
                    templateId,
                    personalisation,
                    clientReference,
                    smsSenderId));
        }
    }
}
