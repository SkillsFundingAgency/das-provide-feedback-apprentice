﻿using System.Text;
using ESFA.DAS.ProvideFeedback.Apprentice.Domain.Dto;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Domain.Messages
{
    public class SmsIncomingMessage : Message
    {
        public SmsIncomingMessage(IncomingSms trigger) : base(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(trigger)))
        {
        }
    }
}
