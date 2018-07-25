namespace ESFA.ProvideFeedback.ApprenticeBot
{
    public class UserState
    {
        public UserState()
        {
            SmsId = 10000;
            PhoneNumber = "07880256082";
        }

        public int SmsId { get; set; }
        public string PhoneNumber { get; set; }
    }
}