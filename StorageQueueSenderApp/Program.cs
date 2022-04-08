using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace StorageQueueSenderApp
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

            while (true)
            {
                Console.Write("Enter a message to be sent to myqueue:");
                var msg = Console.ReadLine();
                if (msg == "exit")
                    break;
                queueClient.SendMessage(msg);
            }
        }
    }
}
