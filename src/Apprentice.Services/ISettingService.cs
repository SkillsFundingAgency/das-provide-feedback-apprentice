namespace ESFA.DAS.ProvideFeedback.Apprentice.Services
{
    public interface ISettingService
    {
        string Get(string parameterName);

        int GetInt(string parameterName);
    }
}