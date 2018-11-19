namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions;
    using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Services;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Notify.Client;
    using Notify.Models.Responses;

    public static class SendSmsMessage
    {
        private static readonly Lazy<SettingsProvider> LazyConfigProvider = new Lazy<SettingsProvider>(Configure);

        private static readonly Lazy<NotificationClient> LazyNotifyClient =
            new Lazy<NotificationClient>(InitializeNotifyClient);

        private static ExecutionContext currentContext;

        public static SettingsProvider Configuration => LazyConfigProvider.Value;

        private static NotificationClient NotifyClient => LazyNotifyClient.Value;

        [FunctionName("SendSmsMessage")]
        [Singleton("OutgoingSmsQueue", Mode = SingletonMode.Listener)]
        public static async Task Run(
        [ServiceBusTrigger("sms-outgoing-messages", Connection = "ServiceBusConnection")]
        string queueMessage,
        ILogger log,
        ExecutionContext context)
        {
            currentContext = context;
            dynamic outgoingSms = JsonConvert.DeserializeObject<dynamic>(queueMessage);

            try
            {
                log.LogInformation($"Response received from bot, sending to {outgoingSms?.recipient?.id}...");

                string mobileNumber = outgoingSms?.from?.id; // TODO: [security] read mobile number from userId hash
                string templateId = Configuration.Get("NotifyTemplateId");
                var personalization = new Dictionary<string, dynamic> { { "message", outgoingSms?.messageReceived } };
                string reference = outgoingSms?.conversation?.id;
                string smsSenderId = Configuration.Get("NotifySmsSenderId");

                var sendTime = DateTime.Now;
                WaitForPreviousSmsSendOrTimeout(reference, log);
                SmsNotificationResponse sendSmsTask = SendSms(
                                                          mobileNumber,
                                                          templateId,
                                                          personalization,
                                                          reference,
                                                          smsSenderId);

                log.LogInformation($"Success!");
            }
            catch (Exception e)
            {
                log.LogError($"SendSmsMessage ERROR", e, e.Message);
                throw new BotConnectorException("Something went wrong when relaying the message to the Notify client", e);
            }
        }

        private static void WaitForPreviousSmsSendOrTimeout(string notificationReference, ILogger log)
        {
            var timeoutTime = DateTime.Now.AddSeconds(15);

            while (SmsNotSent(notificationReference))
            {
                if (DateTime.Now > timeoutTime)
                {
                    log.LogInformation($"done!");
                    break;
                }
                else
                {
                    log.LogInformation($"Waiting for previous message to send...");
                    Task.Delay(200).Wait();
                    continue;
                }
            }
        }

        private static bool SmsNotSent(string notificationReference)
        {
            var lastNotification = NotifyClient
                .GetNotifications("sms", "", notificationReference)
                .notifications
                .OrderByDescending(n => n.createdAt)
                .FirstOrDefault();

            return lastNotification != null && lastNotification.status != "delivered";
        }

        private static SmsNotificationResponse SendSms(string mobileNumber, string templateId, Dictionary<string, dynamic> personalization, string reference, string smsSenderId)
        {
            return NotifyClient.SendSms(
                mobileNumber,
                templateId,
                personalization,
                reference,
                smsSenderId);
        }

        private static SettingsProvider Configure()
        {
            if (currentContext == null)
            {
                throw new BotConnectorException("Could not initialize the settings provider, ExecutionContext is null");
            }

            return new SettingsProvider(currentContext);
        }

        private static NotificationClient InitializeNotifyClient()
        {
            string apiKey = Configuration.Get("NotifyClientApiKey");
            NotificationClient client = new NotificationClient(apiKey);

            return client;
        }
    }
}