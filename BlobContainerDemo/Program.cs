using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace BlobContainerDemo
{
    class Program
    {
        static BlobServiceClient blobServiceClient;
        static string connectionString;
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json");
            IConfiguration config = builder.Build();
            connectionString = config["StorageConnectionString"];
            blobServiceClient = new BlobServiceClient(connectionString);

            string file1 = "F:\\azure recordings\\az-204\\6. Programming Storage\\images\\pick1.jpg";
            string file2 = "F:\\azure recordings\\az-204\\6. Programming Storage\\images\\pick2.jpg";
            string file3 = "F:\\azure recordings\\az-204\\6. Programming Storage\\images\\pick3.jpg";

            //BlobContainerClient con1 = CreateContainer("con1", false);
            //BlobContainerClient con2 = CreateContainer("con2", true);

            //UploadBlob(con1, file1);
            //UploadBlob(con1, file2);
            //UploadBlob(con2, file1);
            //UploadBlob(con2, file3);
            //ListBlobs(con1);
            //ListBlobs(con2);
            //ListBlobsAsAnonymousUser("con1");
            ListBlobsAsAnonymousUser("con2");

            Console.ReadLine();
        }
        static BlobContainerClient CreateContainer(string containerName, bool isPublic)
        {
            BlobContainerClient container = blobServiceClient.GetBlobContainerClient(containerName);
            if (!container.Exists())
            {
                container.CreateIfNotExists();
                Console.WriteLine($"{containerName} is Created\n");

                if (isPublic)
                {
                    container.SetAccessPolicy(PublicAccessType.Blob);
                }
            }
            return container;
        }
        static BlobClient UploadBlob(BlobContainerClient container, string path)
        {
            FileInfo fi = new FileInfo(path);
            BlobClient blobClient = container.GetBlobClient(fi.Name);
            blobClient.Upload(path);
            Console.WriteLine($"Access blob here - {blobClient.Uri.AbsoluteUri}");
            return blobClient;
        }
        static void ListBlobs(BlobContainerClient container)
        {
            Console.WriteLine($"List of Blobs in {container.Name} and {container.GetAccessPolicy().Value}");
            foreach (var blob in container.GetBlobs())
            {
                BlobClient blobClient = container.GetBlobClient(blob.Name);
                Console.WriteLine($"{blob.Name} - {blobClient.Uri}");

            }
            Console.WriteLine("");
        }

        static void ListBlobsAsAnonymousUser(string containerName)
        {
            BlobServiceClient blobClientForAnonymous = new BlobServiceClient(new
           Uri(@"https://kkdemostorage23.blob.core.windows.net"));

            BlobContainerClient container = blobClientForAnonymous.GetBlobContainerClient(containerName);
            foreach (var blob in container.GetBlobs())
            {
                BlobClient blobClient = container.GetBlobClient(blob.Name);
                Console.WriteLine($"{blob.Name} - {blobClient.Uri}");
            }
            Console.WriteLine("");
        }

    }
}
