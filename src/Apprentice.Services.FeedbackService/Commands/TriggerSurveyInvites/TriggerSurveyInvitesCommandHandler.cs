using System;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;
using ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Apprentice.Domain.Dto;
using ESFA.DAS.ProvideFeedback.Apprentice.Domain.Messages;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.FeedbackService.Commands.TriggerSurveyInvites
{
    public class TriggerSurveyInvitesCommandHandler : ICommandHandlerAsync<TriggerSurveyInvitesCommand>
    {
        private readonly IStoreApprenticeSurveyDetails _surveyDetailsRepo;
        private readonly ISettingService _settingService;
        private readonly IQueueClient _queueClient;
        private readonly ILogger<TriggerSurveyInvitesCommandHandler> _logger;

        public TriggerSurveyInvitesCommandHandler(
            IStoreApprenticeSurveyDetails surveyDetailsRepo,
            ISettingService settingService,
            IQueueClientFactory  queueClientFactory,
            ILogger<TriggerSurveyInvitesCommandHandler> logger)
        {
            _surveyDetailsRepo = surveyDetailsRepo;
            _settingService = settingService;
            _queueClient = queueClientFactory.Create<SmsIncomingMessage>();
            _logger = logger;
        }

        public void Handle(TriggerSurveyInvitesCommand command)
        {
            throw new NotImplementedException();
        }

        public async Task HandleAsync(TriggerSurveyInvitesCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            var batchSize = _settingService.GetInt("ApprenticeBatchSize");
            var apprenticeDetails = await _surveyDetailsRepo.GetApprenticeSurveyInvitesAsync(batchSize);

            foreach (var apprenticeDetail in apprenticeDetails)
            {
                var now = DateTime.Now;
                var trigger = new IncomingSms()
                {
                    Type = SmsType.SurveyInvitation,
                    Id = Guid.NewGuid().ToString(),
                    SourceNumber = apprenticeDetail.MobileNumber.ToString(),
                    DestinationNumber = null,
                    Message = $"bot--dialog--start {apprenticeDetail.SurveyCode}",
                    DateReceived = now,
                    UniqueLearnerNumber = apprenticeDetail.UniqueLearnerNumber,
                    StandardCode = apprenticeDetail.StandardCode,
                    ApprenticeshipStartDate = apprenticeDetail.ApprenticeshipStartDate
                };
                
                await _surveyDetailsRepo.SetApprenticeSurveySentAsync(apprenticeDetail.Id);

                try
                {
                    var serviceBusMessage = new SmsIncomingMessage(trigger);
                    await _queueClient.SendAsync(serviceBusMessage);
                }
                catch(Exception ex)
                {
                    _logger.LogError($"Failed to put message on the queue for apprentice survey detail id: {apprenticeDetail.Id}. Reverting survey sent status");
                    await _surveyDetailsRepo.SetApprenticeSurveyNotSentAsync(apprenticeDetail.Id);
                }
            }
        }
    }
}
