namespace ESFA.DAS.ProvideFeedback.Apprentice.Services
{
    using System.Collections.Generic;

    public class BotChannelDataDictionary : IBotChannelData
    {
        public Dictionary<string, dynamic> Content { get; set; }
    }
}