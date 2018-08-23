using System;
using System.Text;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Services
{
    using System.Threading.Tasks;

    public interface ISmsService
    {
        Task SendSmsAsync(string destinationNumber, string content, string reference = null);
    }
}
