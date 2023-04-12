using RntCar.ClassLibrary._Tablet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.AzureBlobStorage.Entities
{
    public class OtherBlobWrapper
    {
        private string containerName { get; set; }
        private string connectionString { get; set; }

        public OtherBlobWrapper(string _connectionString, string _containerName)
        {
            this.containerName = _containerName;
            this.connectionString = _connectionString;
        }

        public void createOtherPhotos(OtherDocuments otherDocuments, string blobName)
        {
            AzureBlobStorageHelper azureBlobStorageHelper = new AzureBlobStorageHelper(connectionString);

            if (otherDocuments.frontLicensePhoto != null && otherDocuments.frontLicensePhoto.Length > 0)
            {
                azureBlobStorageHelper.AddToBlobSTorage(this.containerName, blobName, Guid.NewGuid(), otherDocuments.frontLicensePhoto);
            }
            if (otherDocuments.rearLicensePhoto != null && otherDocuments.rearLicensePhoto.Length > 0)
            {
                azureBlobStorageHelper.AddToBlobSTorage(this.containerName, blobName, Guid.NewGuid(), otherDocuments.rearLicensePhoto);
            }
            if (otherDocuments.kmfuelPhoto != null && otherDocuments.kmfuelPhoto.Length > 0)
            {
                azureBlobStorageHelper.AddToBlobSTorage(this.containerName, blobName, Guid.NewGuid(), otherDocuments.kmfuelPhoto);
            }
        }
    }
}
