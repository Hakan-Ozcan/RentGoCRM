using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.MongoDB
{
    public class EquipmentData
    {
        public string EquipmentId { get; set; }
        public string Name { get; set; }
        public string ChassisNumber { get; set; }
        public string PlateNumber { get; set; }
        public string LicenseNumber { get; set; }
        public string HGSNumber { get; set; }
        public int CurrentKM { get; set; }
        public string ProductName { get; set; }
        public string ProductId { get; set; }
        public string OwnerBranchName { get; set; }
        public string OwnerBranchId { get; set; }
        public string CurrentBranchName { get; set; }
        public string CurrentBranchId { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string GroupCodeInformationId { get; set; }
        public string GroupCodeInformationName { get; set; }
        public int GroupCodeSegmentValue { get; set; }
        public string GroupCodeSegmentName{ get; set; }
        public int MinimumAge { get; set; }
        public int MinimumDriverLcense { get; set; }
        public int YoungDriverAge { get; set; }
        public int YoungDriverLicense { get; set; }
        public decimal Deposit { get; set; }
        public string CarImage { get; set; }
        public int StateCode { get; set; }
        public int StatusCode { get; set; }
        public int fuel { get; set; }
        public int nofDoor { get; set; }
        public int nofLuggage { get; set; }
        public int nofSeat { get; set; }
        public string brandId { get; set; }
        public string brandName { get; set; }
        public string modelId { get; set; }
        public string modelName { get; set; }

        public int fuelType { get; set; }
        public int tranmissionType { get; set; }
    }
}
