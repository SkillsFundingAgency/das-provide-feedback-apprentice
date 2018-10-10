using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models;
using Newtonsoft.Json;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.State
{
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;

    [Serializable]
    public class SurveyState
    {
        public SurveyState()
        {
        }

        public List<BinaryQuestionResponse> Responses { get; set; } = new List<BinaryQuestionResponse>();

        public string SurveyId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public ProgressState Progress { get; set; }

        public int Score => this.Responses.Sum(pqr => pqr?.Score ?? 0);
    }
}