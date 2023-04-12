using RntCar.ClassLibrary._Tablet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.AzureBlobStorage.Entities
{
    public class DamageBlobWrapper
    {
        private string containerName { get; set; }
        private string connectionString { get; set; }

        public DamageBlobWrapper(string _connectionString , string _containerName)
        {
            this.containerName = _containerName;
            this.connectionString = _connectionString;
        }
        public void createDamagePhotos(List<DamageData> damageData,string blobName)
        {
            AzureBlobStorageHelper azureBlobStorageHelper = new AzureBlobStorageHelper(connectionString);

            foreach (var item in damageData)
            {
                try
                {
                    //azureBlobStorageHelper.AddToBlobSTorage(containerName,
                    //                                   blobName,
                    //                                   item.damageId.Value,
                    //                                   item.damagePhoto);
                }
                catch
                { 

                }               
            }            
        }
    }
}
