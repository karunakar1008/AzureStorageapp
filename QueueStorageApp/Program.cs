using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System;
using System.Threading.Tasks;

namespace QueueStorageApp
{
    class Program
    {
        static string connectionstring = "DefaultEndpointsProtocol=https;AccountName=kpmgtaxstorage;AccountKey=fWKHQsGcHGWTGYBBAugfrWhRpaIUJ6l+uV4dN3/aqxZqrX6iO+TDZflHzzrUNvoejwiaLFBm1gGSE4SfFcFZzQ==;EndpointSuffix=core.windows.net";

        static void Main(string[] args)
        {
            Console.WriteLine("Queue storage demo");

            //Create queue1 in the Queue storage.
            //CreateQueue();
            //CreateQueueAsync().Wait();

            //Read messages
            //ReadMessages();

            //Delete queue if there are no messages
            QueueClient queueclinet = new QueueClient(connectionstring, "test");
            RetrieveNextMessageAsync(queueclinet).Wait();

            Console.WriteLine("Completed");
        }

        /// <summary>
        /// Create queue
        /// </summary>
        static void CreateQueue()
        {
            QueueServiceClient serviceClient = new QueueServiceClient(connectionstring);
            QueueClient theQueue = serviceClient.CreateQueue("test");

        }

        /// <summary>
        /// Create Queue if not exist and insert 20 messages
        /// </summary>
        /// <returns></returns>
        public static async Task CreateQueueAsync()
        {

            QueueServiceClient queueServiceClient = new QueueServiceClient(connectionstring);
            QueueClient queueClient = queueServiceClient.GetQueueClient("test");
            queueClient.CreateIfNotExists();

            for (int i = 1; i <= 20; i++)
            {
                Order1 order = new Order1();
                // await queueClient.SendMessageAsync(order.ToString());
                await queueClient.SendMessageAsync(order.ToString(), default, TimeSpan.FromSeconds(-1), default); //This message will never expire as we set past time

            }
        }

        //Read message and pop/delete the message from queue
        public static void ReadMessages()
        {
            //Connect to queue storage
            QueueServiceClient queueServiceClient = new QueueServiceClient(connectionstring);
            //var queues = queueServiceClient.GetQueues();

            //get the reference of queue
            QueueClient queueClient = queueServiceClient.GetQueueClient("test");

            //Azure.Response<QueueMessage[]> responses = queueClient.ReceiveMessages();

            //get the references of top 20 messages in the queue
            QueueMessage[] retrievedMessage = queueClient.ReceiveMessages(20);
            foreach (var message in retrievedMessage)
            {
                Console.WriteLine($"Dequeued message: '{message.Body}'");

                // Delete the message
                queueClient.DeleteMessage(message.MessageId, message.PopReceipt);
                Console.WriteLine("Message Deleted" + message.MessageId);
            }

        }



        static async Task<string> RetrieveNextMessageAsync(QueueClient theQueue)
        {
            if (await theQueue.ExistsAsync())
            {
                QueueProperties properties = await theQueue.GetPropertiesAsync();

                if (properties.ApproximateMessagesCount > 0)
                {
                    QueueMessage[] retrievedMessage = await theQueue.ReceiveMessagesAsync(1);
                    string theMessage = retrievedMessage[0].MessageText;
                    await theQueue.DeleteMessageAsync(retrievedMessage[0].MessageId, retrievedMessage[0].PopReceipt);
                    return theMessage;
                }
                else
                {
                    Console.Write("The queue is empty. Attempt to delete it? (Y/N) ");
                    string response = Console.ReadLine();

                    if (response.ToUpper() == "Y")
                    {
                        await theQueue.DeleteIfExistsAsync();
                        return "The queue was deleted.";
                    }
                    else
                    {
                        return "The queue was not deleted.";
                    }
                }
            }
            else
            {
                return "The queue does not exist. Add a message to the command line to create the queue and store the message.";
            }
        }

    }
}
