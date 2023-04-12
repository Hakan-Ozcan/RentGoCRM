using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.Report
{
    public class FleetReportData
    {
        public string currentBranch { get; set; }
        public string groupCode { get; set; }
        public string name { get; set; }
        public string brand { get; set; }
        public string model { get; set; }
        public string product { get; set; }
        public string chassisNumber { get; set; }
        public int modelYearCode { get; set; }
        public string modelYearName { get; set; }
        public int gearBoxCode { get; set; }
        public string gearBoxName { get; set; }
        public int statusCode { get; set; }
        public string statusName { get; set; }
        public int transferTypeCode { get; set; }
        public string transferTypeName { get; set; }
        public int maintenancePeriodCode { get; set; }
        public string maintenancePeriodName { get; set; }
        public int currentKm { get; set; }
        public int fuelCode { get; set; }
        public string fuelName { get; set; }
        public string licenseNumber { get; set; }
        public string vehicleNo { get; set; }
        public string equipmentColor { get; set; }
        public string tireSize { get; set; }
        public string tireTypeName { get; set; }
        public int tireTypeCode { get; set; }
        public DateTime firstRegistrationDate { get; set; }
        public DateTime inspectionExpireDate { get; set; }
        public string licensePlace { get; set; }
        public string oldPlate { get; set; }
        public string hgsNumber { get; set; }
        public string hgsLabel { get; set; }
        public string mongoDbIntegrationTrigger { get; set; }
        public decimal cost { get; set; }
        public long publishDate { get; set; }
    }
}
