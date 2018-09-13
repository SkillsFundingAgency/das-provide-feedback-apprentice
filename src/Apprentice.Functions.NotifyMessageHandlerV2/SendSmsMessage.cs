namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions;

    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;

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
            [QueueTrigger("sms-outgoing-messages")]
            dynamic outgoingSms, // cast to Bot.Core queue object
            TraceWriter log,
            ExecutionContext context)
        {
            currentContext = context;

            try
            {
                log.Info($"Received response from {outgoingSms?.recipient?.id}");

                string mobileNumber = outgoingSms?.from?.id;
                string templateId = Configuration.Get("NotifyTemplateId");
                var personalization = new Dictionary<string, dynamic> { { "message", outgoingSms?.messageReceived } };
                string reference = outgoingSms?.conversation?.id;
                string smsSenderId = Configuration.Get("NotifySmsSenderId");

                SmsNotificationResponse sendSmsTask = await Task.Run(() => SendSms(
                                                          mobileNumber, 
                                                          templateId, 
                                                          personalization, 
                                                          reference, 
                                                          smsSenderId));

                log.Info($"SendSmsMessage response: {sendSmsTask}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
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