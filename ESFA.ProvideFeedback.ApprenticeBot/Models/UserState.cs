using System.Collections.Generic;

namespace ESFA.ProvideFeedback.ApprenticeBot.Models
{
    /// <summary>
    /// Stores the current conversation state
    /// </summary>
    public class UserState : Dictionary<string, object>
    {
        private const string PhoneNumberKey = "PhoneNumber";
        private const string UserNameKey = "UserName";

        public UserState()
        {
            this[PhoneNumberKey] = 07880256082;
            this[UserNameKey] = "Steeeeeeve";
        }
        public int PhoneNumber
        {
            get => (int)this[PhoneNumberKey];
            set => this[PhoneNumberKey] = value;
        }

        public int UserName
        {
            get => (int)this[UserNameKey];
            set => this[UserNameKey] = value;
        }

    }
}