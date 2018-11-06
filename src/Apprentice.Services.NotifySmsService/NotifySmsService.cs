namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.NotifySmsService
{
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;
    using ESFA.DAS.ProvideFeedback.Apprentice.Services.Helpers;
    using ESFA.DAS.ProvideFeedback.Apprentice.Services.NotifySmsService.Commands;

    using Microsoft.Extensions.Options;

    using Notify.Client;

    using NotifyConfig = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Notify;

    public class NotifySmsService : ISmsService
    {
        private readonly ICommandHandlerAsync<SendBasicSmsCommand> sendSmsCommandHandler;

        public NotifySmsService(
            ICommandHandlerAsync<SendBasicSmsCommand> sendSmsCommandHandler)
        {
            this.sendSmsCommandHandler = sendSmsCommandHandler;
        }

        public void SendSms(string destinationNumber, string messageToSend, string reference = null)
        {
            AsyncHelper.RunSync(() => this.SendSmsAsync(destinationNumber, messageToSend));
        }

        public async Task SendSmsAsync(string destinationNumber, string messageToSend, string reference = null)
        {
            SendBasicSmsCommand command = new SendBasicSmsCommand()
                .To(destinationNumber)
                .Message(messageToSend)
                .WithReference(reference);

            await this.sendSmsCommandHandler.HandleAsync(command);
        }
    }
}