using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
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

            string file1 = "F:\\azure recordings\\az-204\\6. Programming Storage\\upload images\\pick1.jpg";
            string file2 = "F:\\azure recordings\\az-204\\6. Programming Storage\\upload images\\pick2.jpg";
            string file3 = "F:\\azure recordings\\az-204\\6. Programming Storage\\upload images\\pick3.jpg";

            BlobContainerClient con1 = CreateContainer("con1", false);
            BlobContainerClient con2 = CreateContainer("con2", true);

            //UploadBlob(con1, file1);
            //UploadBlob(con1, file2);
            //UploadBlob(con2, file1);
            //UploadBlob(con2, file3);
            //ListBlobs(con1);
            //ListBlobs(con2);
            //ListBlobsAsAnonymousUser("con1");
            //ListBlobsAsAnonymousUser("con2");

            //LeaseDemo(con1);
            DownloadBlobs(con2);
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

        static void LeaseDemo(BlobContainerClient con)
        {
            FileInfo fi = new FileInfo("F:\\azure recordings\\az-204\\6. Programming Storage\\demo.txt");
            AppendBlobClient blob = con.GetAppendBlobClient(fi.Name);
            blob.CreateIfNotExists(new AppendBlobCreateOptions() { });

            MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("\nThis is Line1\n"));
            blob.AppendBlock(ms);
            ms.Close();

            BlobLeaseClient lease = blob.GetBlobLeaseClient();
            lease.Acquire(TimeSpan.FromSeconds(15));
            Console.WriteLine("Blob lease acquired. Lease = {0}", lease);

            // Update blob without using lease.
            // This operation will not succeed becuase we acquired lease on the blob and we must specify the lease id.
            try
            {
                ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("\nThis is line3\n"));
                blob.AppendBlock(ms); //Failes because Lease is not mentioned
                ms.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            // Update blob using lease. This operation will succeed
            AppendBlobRequestConditions conditions = new AppendBlobRequestConditions()
            {
                LeaseId = lease.LeaseId
            };
            ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("\nThis is Line2\n"));
            blob.AppendBlock(ms, null, conditions); //Will success as lease is mentioned

            Console.WriteLine("Blob updated using an exclusive lease");
            ms.Close();
            lease.Release();
        }

        static void DownloadBlobs(BlobContainerClient container)
        {
            Console.WriteLine($"List of Blobs in {container.Name} and {container.GetAccessPolicy().Value}");
            foreach (var blob in container.GetBlobs())
            {
                BlobClient blobClient = container.GetBlobClient(blob.Name);
                var contentResult = blobClient.DownloadContent().Value;
                var bytes = contentResult.Content.ToArray();
                File.WriteAllBytes("F:\\azure recordings\\az-204\\6. Programming Storage\\downlodblobs\\" + blob.Name, bytes);
                Console.WriteLine($"{blob.Name} - {blobClient.Uri}");
            }
            Console.WriteLine("");
        }

    }
}
