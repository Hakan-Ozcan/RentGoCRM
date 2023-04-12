using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.MongoDB
{
    public class TransferData
    {
        public string transferId { get; set; }
        public string transferName { get; set; }
        public string transferNumber { get; set; }
        public string serviceName { get; set; }
        public string description { get; set; }
        public string equipmentId { get; set; }
        public string equipmentName { get; set; }
        public string groupCodeId { get; set; }
        public string groupCodeName { get; set; }
        public int pickupFuelCode { get; set; }
        public int pickupKilometer { get; set; }
        public int dropoffFuelCode { get; set; }
        public int dropoffKilometer { get; set; }
        public int transferTypeCode { get; set; }
        public string pickupBranchId { get; set; }
        public string pickupBranchName { get; set; }
        public string dropoffBranchId { get; set; }
        public string dropoffBranchName { get; set; }
        public DateTime estimatedPickupDate { get; set; }
        public DateTime estimatedDropoffDate { get; set; }
        public long estimatedPickupDateTimeStamp { get; set; }
        public long estimatedDropoffDateTimeStamp { get; set; }
        public DateTime? actualPickupDate { get; set; }
        public DateTime? actualDropoffDate { get; set; }
        public long? actualPickupDateTimeStamp { get; set; }
        public long? actualDropoffDateTimeStamp { get; set; }
        public int statecode { get; set; }
        public int statuscode { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
    }
}
