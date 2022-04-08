using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace StorageQueueReceiverApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json");
            IConfiguration config = builder.Build();
            string connectionString = config["StorageConnectionString"];

            string queueName = "demoqueue";
            QueueClient queueClient = new QueueClient(connectionString, queueName);
            queueClient.CreateIfNotExists();

            QueueProperties properties = queueClient.GetProperties();
            int cachedMessagesCount = properties.ApproximateMessagesCount;
            Console.WriteLine($"Count of messages in the queue: {cachedMessagesCount}");

            //If you don't pass a value for the maxMessages parameter, the default is to peek at one message.
            PeekedMessage[] peekedMessages = queueClient.PeekMessages(maxMessages: 1);
            if (peekedMessages.Length != 0)
                Console.WriteLine($"Peeked message: '{peekedMessages[0].Body}'");

            // The following code updates the queue message with new contents, and sets the visibility timeout to extend  another 60 seconds.
             QueueMessage[] messages = queueClient.ReceiveMessages(); //Fetches only one message from queue (visibilitytimeout = 30)
            queueClient.UpdateMessage(
                messages[0].MessageId
                , messages[0].PopReceipt
                , "Updated contents"
                , TimeSpan.FromSeconds(0) // Make it invisible for another 60 seconds; Default 
                );

            while (true)
            {
                QueueMessage[] retrievedMessages = queueClient.ReceiveMessages(1);
                //Fetches only one message from  queue(visibilitytimeout = 30)
                if (retrievedMessages.Length == 0)
                    Console.WriteLine("No Messages in last 5 secs...");
                foreach (var msg in retrievedMessages)
                {
                    Console.WriteLine($"Dequeued message: '{msg.Body}' - DeQueue Count: {msg.DequeueCount}");
                    queueClient.DeleteMessage(msg.MessageId, msg.PopReceipt);
                }
                System.Threading.Thread.Sleep(5000);
            }
        }

    }
}
