using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.State
{
    /// <inheritdoc />
    /// <summary>
    /// Conversation state information.
    /// We are also using this directly for dialog state, which needs an <see cref="T:System.Collections.Generic.IDictionary`2" /> object.
    /// </summary>
    public class ConversationInfo : Dictionary<string, object>
    {
        //private const string BadResponsesKey = "BadResponses";

        //public ConversationInfo()
        //{
        //    this[BadResponsesKey] = 0;
        //}

        //public long BadResponses
        //{
        //    get => (int)this[BadResponsesKey];
        //    set => this[BadResponsesKey] = value;
        //}

        public long Attempts
        {
            get => this.GetProperty<long>(nameof(Attempts));
            set => this[nameof(Attempts)] = (object)value;
        }

        protected T GetProperty<T>(string propertyName)
        {
            if (this.ContainsKey(propertyName))
                return (T)this[propertyName];
            return default(T);
        }
    }


}