namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.NotifySmsService
{
    using System;
    using System.Configuration;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;

    public class NotifySendSmsCommandHandler : ICommandHandlerAsync<NotifySendSmsCommand>
    {
        public async Task HandleAsync(NotifySendSmsCommand command)
        {
            await this.HandleAsync(command, new CancellationToken());
        }

        public async Task HandleAsync(NotifySendSmsCommand command, CancellationToken token)
        {
            await command.ExecuteAsync(token);
        }

        public void Handle(NotifySendSmsCommand command)
        {
            throw new System.NotImplementedException();
        }
    }
}