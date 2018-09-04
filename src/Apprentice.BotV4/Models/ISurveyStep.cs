namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models
{
    using System.Collections.Generic;

    public interface ISurveyStep
    {
        string Id { get; set; }

        bool IsValid { get; set; }

        int Order { get; set; }

        ICollection<IResponse> Responses { get; set; }
    }
}