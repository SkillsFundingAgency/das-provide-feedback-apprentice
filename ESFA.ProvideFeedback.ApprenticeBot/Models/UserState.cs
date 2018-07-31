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
            this[PhoneNumberKey] = "01234567890";
            this[UserNameKey] = "Steeeeve";
        }
        public string PhoneNumber
        {
            get => (string)this[PhoneNumberKey];
            set => this[PhoneNumberKey] = value;
        }

        public string UserName
        {
            get => (string)this[UserNameKey];
            set => this[UserNameKey] = value;
        }

    }
}