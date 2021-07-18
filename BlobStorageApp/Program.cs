using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BlobStorageApp
{
    class Program
    {
        static string storageconnstring = "DefaultEndpointsProtocol=https;AccountName=kpmgtaxstorage;AccountKey=fWKHQsGcHGWTGYBBAugfrWhRpaIUJ6l+uV4dN3/aqxZqrX6iO+TDZflHzzrUNvoejwiaLFBm1gGSE4SfFcFZzQ==;EndpointSuffix=core.windows.net";
        static string containerName = "demo2";
        static string filename = "DSC_0005.JPG";
        static string filepath = "E:\\demo blobs\\DSC_0005.JPG";

        static string downloadpath = "E:\\demo blobs\\downloads";


        public static async Task Main(string[] args)
        {
            Console.WriteLine("Demo Blob storage");

            //await CreateContainer();

            await CreateBlob();

            Console.WriteLine("Job done");
        }

        //Create container in the blob storage
        static async Task CreateContainer()
        {

            BlobServiceClient blobServiceClient = new BlobServiceClient(storageconnstring);

            BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);
        }


        static async Task CreateBlob()
        {
            //Connect to blob storage with connection string
            BlobServiceClient blobServiceClient = new BlobServiceClient(storageconnstring);

            //Get contaniner reference
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            
            //Create continer if not exist
            containerClient.CreateIfNotExists();

            //Create blob instance
            BlobClient blobClient = containerClient.GetBlobClient(filename); //getting the reference to the blob with blob name


            using FileStream uploadFileStream = File.OpenRead(filepath); //reading the file content 

            await blobClient.UploadAsync(uploadFileStream, true);
            uploadFileStream.Close();
        }

        static async Task deleteblob()
        {

            BlobServiceClient blobServiceClient = new BlobServiceClient(storageconnstring);

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);


            BlobClient blobClient = containerClient.GetBlobClient(filename); //getting the reference to the blob

            blobClient.DeleteIfExists();
        }

        static async Task Deletecontainer()
        {

            BlobServiceClient blobServiceClient = new BlobServiceClient(storageconnstring);

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            containerClient.DeleteIfExists();


        }


        //Update batch insertions
        public static async Task Uploadblobs()
        {
            BlobServiceClient serviceclinet = new BlobServiceClient(storageconnstring);
            BlobContainerClient bc = serviceclinet.GetBlobContainerClient(containerName);

            var files = Directory.GetFiles("E:/demo blobs");

            foreach (string file in files)
            {

                BlobClient b = bc.GetBlobClient(Path.GetFileName(file));

                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict.Add("CreatedBy", "Karunkar");
                dict.Add("FileName", Path.GetFileName(file));
                dict.Add("FirstVersion", DateTime.Now.ToString());
                dict.Add("Department", "IT");
                dict.Add("ProjectName", "Internal");

                b.DeleteIfExists();

                bc.SetAccessPolicy(PublicAccessType.Blob);
                using FileStream filestream = File.OpenRead(file);
                await b.UploadAsync(filestream, true);
                await b.SetMetadataAsync(dict);
                //var leaseclinet = b.GetBlobLeaseClient();
                //TimeSpan leaseTime = TimeSpan.FromSeconds(60);
                //var b1 = await leaseclinet.AcquireAsync(leaseTime);

                //BlobProperties bp = b.GetProperties();

                //Console.WriteLine($"Blob state : {bp.LeaseState } , Lease Status : {bp.LeaseStatus }");
                //filestream.Close();
                Console.WriteLine("File Uplaoded" + file);

            }



        }


        static async Task GetBlobsNames()
        {

            BlobServiceClient blobServiceClient = new BlobServiceClient(storageconnstring);

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);


            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                Console.WriteLine("\t" + blobItem.Name);
            }

        }

        //Download batch of blobs
        public static async Task GetAllBlobDownload()
        {
            BlobServiceClient serviceclinet = new BlobServiceClient(storageconnstring);
            BlobContainerClient bc = serviceclinet.GetBlobContainerClient(containerName);

            await foreach (BlobItem item in bc.GetBlobsAsync())
            {
                BlobClient clinet = bc.GetBlobClient(item.Name);

                BlobDownloadInfo bd = await clinet.DownloadAsync();
                using (FileStream fs = File.OpenWrite(downloadpath + "\\" + item.Name))
                {
                    await bd.Content.CopyToAsync(fs);
                    fs.Close();
                }
            }


        }

        static async Task GetSingleBlob()
        {

            BlobServiceClient blobServiceClient = new BlobServiceClient(storageconnstring);

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            BlobClient blob = containerClient.GetBlobClient(filename);

            BlobDownloadInfo blobdata = await blob.DownloadAsync();


            using (FileStream downloadFileStream = File.OpenWrite(downloadpath))
            {
                await blobdata.Content.CopyToAsync(downloadFileStream);
                downloadFileStream.Close();
            }


            // Read the new file
            using (FileStream downloadFileStream = File.OpenRead(downloadpath))
            {
                using var strreader = new StreamReader(downloadFileStream);
                string line;
                while ((line = strreader.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                }
            }

        }
    }
}
