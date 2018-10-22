namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions;

    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
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
        public static async Task Run(
        [ServiceBusTrigger("sms-outgoing-messages", Connection = "ServiceBusConnection")]
        string queueMessage,
        TraceWriter log,
        ExecutionContext context)
        {
            log.Info($"Queue message {queueMessage}");
            currentContext = context;
            dynamic outgoingSms = JsonConvert.DeserializeObject<dynamic>(queueMessage);

            try
            {
                log.Info($"Received response from {outgoingSms?.recipient?.id}");

                string mobileNumber = outgoingSms?.from?.id;
                string templateId = Configuration.Get("NotifyTemplateId");
                var personalization = new Dictionary<string, dynamic> { { "message", outgoingSms?.messageReceived } };
                string reference = outgoingSms?.conversation?.id;
                string smsSenderId = Configuration.Get("NotifySmsSenderId");

                var sendTime = DateTime.Now;
                WaitForPreviousSmsSendOrTimeout(reference);
                SmsNotificationResponse sendSmsTask = SendSms(
                                                          mobileNumber,
                                                          templateId,
                                                          personalization,
                                                          reference,
                                                          smsSenderId);

                log.Info($"SendSmsMessage response: {sendSmsTask}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static void WaitForPreviousSmsSendOrTimeout(string notificationReference)
        {
            var timeoutTime = DateTime.Now.AddSeconds(15);

            while (SmsNotSent(notificationReference))
            {
                if (DateTime.Now > timeoutTime)
                {
                    break;
                }
                else
                {
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
                .First();

            return lastNotification.status != "delivered";
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