using System;
using System.Collections.Generic;
using Microsoft.Azure.ServiceBus;

namespace WebScraperModularized.queue
{
    public interface IQueueService
    {
        void sendMessageToQueue(String message, String queueName);
        void sendListOfMessagesToQueue(List<String> messages, String queueName);
    }
}