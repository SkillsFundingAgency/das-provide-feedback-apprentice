using System.Collections.Generic;
using System.Threading.Tasks;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Services
{
    public interface INotificationClient
    {
        Task<Notify.Models.Responses.SmsNotificationResponse> SendSms(string mobileNumber, string templateId, Dictionary<string, dynamic> personalisation = null, string clientReference = null, string smsSenderId = null);

        Task<Notify.Models.NotificationList> GetNotifications(string templateType = "", string status = "", string reference = "", string olderThanId = "");

    }
}
