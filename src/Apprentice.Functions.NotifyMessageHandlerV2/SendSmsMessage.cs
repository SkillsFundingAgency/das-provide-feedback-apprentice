namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Dto;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions;
    using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.DependecyInjection;
    using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Services;

    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.InteropExtensions;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json;
    using Notify.Client;
    using Notify.Models.Responses;

    using MessageLockLostException = Microsoft.Azure.ServiceBus.MessageLockLostException;

    public static class SendSmsMessage
    {
        private static readonly Lazy<NotificationClient> LazyNotifyClient =
            new Lazy<NotificationClient>(InitializeNotifyClient);

        private static ExecutionContext currentContext;

        public static SettingsProvider Configuration;

        private static NotificationClient NotifyClient => LazyNotifyClient.Value;

        [FunctionName("SendSmsMessage")]
        public static async Task Run(
        [ServiceBusTrigger("sms-outgoing-messages", Connection = "ServiceBusConnection")]
        string queueMessage,
        [Inject] SettingsProvider configuration,
        ILogger log,
        ExecutionContext context)
        {
            Configuration = configuration;
            currentContext = context;
            OutgoingSms outgoingSms = JsonConvert.DeserializeObject<OutgoingSms>(queueMessage);

            try
            {
                string mobileNumber = outgoingSms.From.UserId; // TODO: [security] read mobile number from userId hash
                string templateId = Configuration.Get("NotifyTemplateId");
                var personalization = new Dictionary<string, dynamic> { { "message", outgoingSms.Message } };
                string reference = outgoingSms.Conversation.ConversationId;
                string smsSenderId = Configuration.Get("NotifySmsSenderId");

                log.LogInformation($"sending message to {outgoingSms.From.UserId}...");

                await WaitForPreviousSmsSendOrTimeout(reference, log);
                SmsNotificationResponse sendSmsResponse = await SendSms(
                    mobileNumber,
                    templateId,
                    personalization,
                    reference,
                    smsSenderId);

                log.LogInformation($"Sent! Reference: {sendSmsResponse.reference}");
            }
            catch (Exception e)
            {
                log.LogError($"SendSmsMessage ERROR: {e.Message}", e, e.Message);
                throw new BotConnectorException("Something went wrong when relaying the message to the Notify client", e);
            }
        }

        private static async Task WaitForPreviousSmsSendOrTimeout(string reference, ILogger log)
        {
            int processingDelay = Configuration.GetInt("NotifyQueueProcessingDelayMs");
            int queueRetryDuration = Configuration.GetInt("NotifyQueueRetryDurationMs");
            int queueRetriesPerSecond = Configuration.GetInt("NotifyQueueRetriesPerSecond");

            DateTime timeoutTime = DateTime.Now.AddMilliseconds(queueRetryDuration);

            if (processingDelay > 0)
            {
                await Task.Delay(processingDelay);
            }

            while (SmsNotSent(reference))
            {
                log.LogInformation($"message blocked, waiting for previous message to send...");
                if (DateTime.Now > timeoutTime)
                {
                    throw new BotConnectorException("timed out waiting for previous SMS to send. Returning message to the queue");
                }

                await Task.Delay(1000 / queueRetriesPerSecond);
            }
        }

        private static bool SmsNotSent(string notificationReference)
        {
            var lastNotification = NotifyClient
                .GetNotifications("sms", string.Empty, notificationReference)
                .notifications
                .OrderByDescending(n => n.createdAt)
                .FirstOrDefault();

            return lastNotification != null && lastNotification.status != "delivered";
        }

        private static Task<SmsNotificationResponse> SendSms(string mobileNumber, string templateId, Dictionary<string, dynamic> personalization, string reference, string smsSenderId)
        {
            return Task.Run(
                () => NotifyClient.SendSms(
                    mobileNumber,
                    templateId,
                    personalization,
                    reference,
                    smsSenderId));
        }

        private static NotificationClient InitializeNotifyClient()
        {
            string apiKey = Configuration.Get("NotifyClientApiKey");
            NotificationClient client = new NotificationClient(apiKey);

            return client;
        }
    }
}