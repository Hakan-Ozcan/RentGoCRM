using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RntCar.AzureBlobStorage
{
    public class AzureBlobStorageHelper
    {
        public CloudBlobClient blobClient { get; set; }
        public CloudBlobContainer blobContainer { get; set; }
        public CloudStorageAccount storageAccount { get; set; }
        public AzureBlobStorageHelper(string connectionString)
        {
            storageAccount = CloudStorageAccount.Parse(connectionString);
            blobClient = storageAccount.CreateCloudBlobClient();
        }

        public void GetContainer(string ContainerName)
        {
            // Retrieve a reference to a container.    
            blobContainer = blobClient.GetContainerReference(ContainerName.ToLower().Replace("ı", "i").Replace("ç", "c").Replace("ş", "s").Replace("ü", "u"));
            // Create the container if it doesn't already exist.    
            var a = blobContainer.CreateIfNotExists();

            BlobContainerPermissions permissions = blobContainer.GetPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Container;
            blobContainer.SetPermissions(permissions);
        }
        public string AddToBlobSTorage(string ContainerName, string BlobName, Guid identifierId, byte[] array, string contentType = "Jpeg")
        {
            GetContainer(ContainerName);
            string blobName = BlobName + "/" + identifierId.ToString();
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(blobName);
            blockBlob.Properties.ContentType = contentType;
            blockBlob.UploadFromByteArray(array, 0, array.Length);
            return blobName;
        }
        public string AddToBlobSTorage(string ContainerName, string BlobName, Guid identifierId, string path)
        {
            GetContainer(ContainerName);
            string blobName = BlobName + "/" + identifierId.ToString();
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(blobName);
            blockBlob.Properties.ContentType = "Jpeg";

            using (AutoResetEvent waitHandle = new AutoResetEvent(false))
            {
                OperationContext context = new OperationContext();
                var res = blockBlob.BeginUploadFromFile(path, null, null, context, ar => waitHandle.Set(), null);
                waitHandle.WaitOne();
                blockBlob.EndUploadFromFile(res);


            }
            //
            return blobName;
        }


        public void getDocumentForGivenDirectory(string containerName)
        {
            GetContainer(containerName);
            foreach (IListBlobItem item in blobContainer.ListBlobs(null, false))
            {
                int i = 1;
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob blockBlob = (CloudBlockBlob)item;
                    blockBlob.FetchAttributes();
                    var uri = blockBlob.Uri;

                }
                else if (item.GetType() == typeof(CloudPageBlob))
                {
                    CloudPageBlob pageBlob = (CloudPageBlob)item;
                    Console.WriteLine("Page blob of length {0}: {1}", pageBlob.Properties.Length, pageBlob.Uri);
                }
                else if (item.GetType() == typeof(CloudBlobDirectory))
                {
                    CloudBlobDirectory directory = (CloudBlobDirectory)item;
                    Console.WriteLine("Directory: {0}", directory.Uri);

                    foreach (var subBlobs in directory.ListBlobs(true, BlobListingDetails.None))
                    {
                        if ( !((Microsoft.WindowsAzure.Storage.Blob.CloudBlob)subBlobs).StorageUri.PrimaryUri.ToString().Contains("unsigned"))
                        {
                            using (WebClient wc = new WebClient())
                            {
                                Console.WriteLine(((Microsoft.WindowsAzure.Storage.Blob.CloudBlob)subBlobs).StorageUri.PrimaryUri.ToString());
                                // copy data to byte[]
                                byte[] data = wc.DownloadData(((Microsoft.WindowsAzure.Storage.Blob.CloudBlob)subBlobs).StorageUri.PrimaryUri.ToString());
                                
                                if(data.Length == 0)
                                {
                                    var c = ((Microsoft.WindowsAzure.Storage.Blob.CloudBlob)subBlobs).StorageUri.PrimaryUri.ToString().Split('/')[4];
                                    File.WriteAllBytes("pdfs/" + c + ".pdf", data);
                                }
                                
                            }
                        }

                    }
                }
                i++;
            }
        }

        public List<string> getGetDocumentsUrlByContainerandDirectory(string containerName, string directoryPath)
        {
            GetContainer(containerName);

            var directory = blobContainer.GetDirectoryReference(directoryPath);

            var urlList = new List<string>();

            foreach (var subBlobs in directory.ListBlobs(true, BlobListingDetails.None))
            {
                urlList.Add(subBlobs.StorageUri.PrimaryUri.ToString());
            }
            return urlList;
        }
    }
}
