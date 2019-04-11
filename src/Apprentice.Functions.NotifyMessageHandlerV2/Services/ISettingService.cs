namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Services
{
    public interface ISettingService
    {
        string Get(string parameterName);

        int GetInt(string parameterName);
    }
}