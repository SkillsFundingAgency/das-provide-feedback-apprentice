using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models;
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

    [Serializable]
    public class UserProfile
    {
        public SurveyState SurveyState { get; set; }

        public string TelephoneNumber { get; set; }

        public string IlrNumber { get; set; }

        public string StandardOrProgram { get; set; }
    }

    public class TopicState
    {
        public string Prompt { get; set; } = "askName";

        public List<BinaryQuestionResponse> Responses { get; set; } = new List<BinaryQuestionResponse>();

        public string SurveyId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public ProgressState Progress { get; set; }

        public int Score => this.Responses.Sum(pqr => pqr?.Score ?? 0);
    }


}