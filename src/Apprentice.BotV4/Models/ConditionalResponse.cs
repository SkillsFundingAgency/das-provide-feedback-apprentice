namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models
{
    using System.Collections.Generic;
    using System.Data;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;

    public abstract class ConditionalResponse<T> : IResponse
    {
        public string Id { get; set; }

        public string Prompt { get; set; }

        public abstract bool IsValid(T userResponse);
    }
}