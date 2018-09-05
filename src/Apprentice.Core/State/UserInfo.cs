using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.State
{
    /// <summary>
    /// User state information.
    /// </summary>
    [Serializable]
    public class UserInfo
    {
        public SurveyState SurveyState { get; set; }
    }
}