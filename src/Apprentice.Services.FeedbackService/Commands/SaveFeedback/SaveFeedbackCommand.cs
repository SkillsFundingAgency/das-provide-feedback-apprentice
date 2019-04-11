namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.FeedbackService.Commands.SaveFeedback
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Feedback;
    using ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories;

    using ApprenticeFeedbackDto = ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto.ApprenticeFeedback;

    public class SaveFeedbackCommand : ICommandAsync
    {
        public ApprenticeFeedback Feedback { get; set; }
    }
}