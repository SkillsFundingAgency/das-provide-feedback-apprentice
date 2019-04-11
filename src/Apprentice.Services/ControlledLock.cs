using Microsoft.WindowsAzure.Storage.Blob;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Services
{
    class ControlledLock
    {
        public string Id { get; set; }
        public string LeaseId { get; set; }
        public CloudBlockBlob Blob { get; set; }

        public ControlledLock(string id, string leaseId, CloudBlockBlob blob)
        {
            Id = id;
            LeaseId = leaseId;
            Blob = blob;
        }
    }
}
