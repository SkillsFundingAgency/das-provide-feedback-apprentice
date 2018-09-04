namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models
{
    public class NegativeResponse : ConditionalResponse<BinaryQuestionResponse>
    {
        public override bool IsValid(BinaryQuestionResponse userResponse)
        {
            return !userResponse.IsPositive;
        }
    }
}