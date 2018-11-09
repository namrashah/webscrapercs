using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Amqp.Serialization;
using Microsoft.Azure.ServiceBus;
using WebScraperModularized.data;
using WebScraperModularized.helpers;
using WebScraperModularized.parsers;
using WebScraperModularized.wrappers;

namespace WebScraperModularized.queue
{
    public class AzureServiceBusService : IQueueService
    {
        private static AzureServiceBusService instance = null;
        private static readonly object padlock = new object();


        private IQueueClient zipcodeQueueClient;
        private IQueueClient propertyQueueClient;
        private IQueueClient apartmentQueueClient;


        private ApartmentService _apartmentService;
        private PropertyService _propertyService;
        private ZipcodeService _zipcodeService;


        private Hashtable mapOfQueues;

        public static AzureServiceBusService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new AzureServiceBusService();
                        }
                    }
                }

                return instance;
            }
        }

        private AzureServiceBusService()
        {
            string ServiceBusConnectionString = new MyConfigurationHelper().getServiceBusConnectionString();
            string zipcodequeue = "zipcodequeue";
            string propertyqueue = "propertyqueue";
            string apartmentqueue = "apartmentqueue";
            zipcodeQueueClient = new QueueClient(ServiceBusConnectionString, zipcodequeue);
            propertyQueueClient = new QueueClient(ServiceBusConnectionString, propertyqueue);
            apartmentQueueClient = new QueueClient(ServiceBusConnectionString, apartmentqueue);
            mapOfQueues = new Hashtable();
            mapOfQueues.Add(zipcodequeue, zipcodeQueueClient);
            mapOfQueues.Add(propertyqueue, propertyQueueClient);
            mapOfQueues.Add(apartmentqueue, apartmentQueueClient);
            _zipcodeService = new ZipcodeService();
            _propertyService = new PropertyService();
            _apartmentService = new ApartmentService();
            RegisterOnMessageHandlerAndReceiveMessages();
        }

        public void sendMessageToQueue(String messageContent, String queueName)
        {
            var message = new Message(Encoding.UTF8.GetBytes(messageContent));
            SendMessageAsync((IQueueClient) mapOfQueues[queueName], message);
        }

        public void sendListOfMessagesToQueue(List<String> messages, String queueName)
        {
            List<Message> listOfMessages = new List<Message>();
            foreach (var each in messages)
            {
                var message = new Message(Encoding.UTF8.GetBytes(each));
                listOfMessages.Add(message);
            }

            SendListOfMessageAsync((IQueueClient) mapOfQueues[queueName], listOfMessages);
        }

        private async Task SendMessageAsync(IQueueClient queueClient, Message message)
        {
            try
            {
                await queueClient.SendAsync(message);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }

        private async Task SendListOfMessageAsync(IQueueClient queueClient, List<Message> listOfMessages)
        {
            try
            {
                await queueClient.SendAsync(listOfMessages);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }

        private void RegisterOnMessageHandlerAndReceiveMessages()
        {
            // Configure the MessageHandler Options in terms of exception handling, number of concurrent messages to deliver etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of Concurrent calls to the callback `ProcessMessagesAsync`, set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 10,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = false
            };

            // Register the function that will process messages
            zipcodeQueueClient.RegisterMessageHandler(ProcessZipcodeMessagesAsync,
                messageHandlerOptions);
            propertyQueueClient.RegisterMessageHandler(ProcessPropertyMessagesAsync,
                messageHandlerOptions);
            apartmentQueueClient.RegisterMessageHandler(ProcessApartmentMessagesAsync,
                messageHandlerOptions);
        }

        public async Task ProcessZipcodeMessagesAsync(Message message, CancellationToken token)
        {
            Console.WriteLine(
                $"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
            String zipcode = Encoding.UTF8.GetString(message.Body);

            List<String> listOfUrlsToBeParsedForTheGivenPostcode = _zipcodeService.ProcessZipcodeMessagesAsync(zipcode);
            // This can be done only if the queueClient is created in ReceiveMode.PeekLock mode (which is default).
            foreach (var each in listOfUrlsToBeParsedForTheGivenPostcode)
            {
                Console.Out.WriteLine(each);
            }
            sendListOfMessagesToQueue(listOfUrlsToBeParsedForTheGivenPostcode,"propertyqueue");
            await zipcodeQueueClient.CompleteAsync(message.SystemProperties.LockToken);
        }


        public async Task ProcessPropertyMessagesAsync(Message message, CancellationToken token)
        {
            Console.WriteLine(
                $"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
            String url = Encoding.UTF8.GetString(message.Body);

            List<String> listOfPropertyUrl = _propertyService.getPropertyUrlsFromPropertyListPage(url);

            sendListOfMessagesToQueue(listOfPropertyUrl, "apartmentqueue");
            
            foreach (var each in listOfPropertyUrl)
            {
                Console.Out.WriteLine(each);
            }

            await propertyQueueClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        public async Task ProcessApartmentMessagesAsync(Message message, CancellationToken token)
        {
            Console.WriteLine(
                $"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
            String url = Encoding.UTF8.GetString(message.Body);

            _apartmentService.processAndSaveApartmentsFromPropertyPage(url);

            await apartmentQueueClient.CompleteAsync(message.SystemProperties.LockToken);
        }
    }
}