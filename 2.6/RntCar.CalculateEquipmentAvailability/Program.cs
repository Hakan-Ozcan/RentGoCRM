using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MongoDB;
using RntCar.MongoDBHelper.Entities;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CalculateEquipmentAvailability
{
    class Program
    {
        static void Main(string[] args)
        {

            var mongoDBHostName = StaticHelper.GetConfiguration("MongoDBHostName");
            var mongoDBDatabaseName = StaticHelper.GetConfiguration("MongoDBDatabaseName");

            try
            {
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                EquipmentAvailabilityBusiness equipmentAvailabilityBusiness = new EquipmentAvailabilityBusiness(mongoDBHostName, mongoDBDatabaseName, args);
                EquipmentAvailabilityBL equipmentAvailabilityBL = new EquipmentAvailabilityBL(crmServiceHelper.IOrganizationService);


                string filePath = Convert.ToString(StaticHelper.GetConfiguration("filePath"));
                DateTime publishDate = Convert.ToDateTime(StaticHelper.GetConfiguration("publishDate"));
                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    var equipmentAvailabilityDataCSV = ReadCSV(filePath, publishDate);
                    equipmentAvailabilityDataCSV = equipmentAvailabilityBL.calculateEquipmentAvailabilityForCSV(equipmentAvailabilityDataCSV, publishDate);
                    foreach (var item in equipmentAvailabilityDataCSV)
                    {
                        equipmentAvailabilityBusiness.createEquipmentAvailability(item);
                    }
                }
                else
                {
                    var equipmentAvailabilityData = equipmentAvailabilityBL.calculateEquipmentAvailability(args);

                    foreach (var item in equipmentAvailabilityData)
                    {
                        equipmentAvailabilityBusiness.createEquipmentAvailability(item);
                    }
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static List<EquipmentAvailabilityData> ReadCSV(string filePath, DateTime publishDate)
        {
            List<EquipmentAvailabilityData> equipmentAvailabilityItems = new List<EquipmentAvailabilityData>();
            DataTable dtCSV = new DataTable();

            string[] lines = System.IO.File.ReadAllLines(filePath, System.Text.UTF8Encoding.Default);
            int rows = lines.Count();
            for (int r = 1; r < rows; r++)
            {
                string line = lines[r];
                string[] columns = line.Split(',');
                EquipmentAvailabilityData equipmentAvailability = new EquipmentAvailabilityData();
                equipmentAvailability.CurrentBranchId = new Guid(columns[0]);
                equipmentAvailability.CurrentBranch = columns[1];
                equipmentAvailability.Total = Convert.ToInt64(columns[2]);
                equipmentAvailability.RentalCount = Convert.ToInt64(columns[3]);
                equipmentAvailability.AvailableCount = Convert.ToInt64(columns[4]);
                equipmentAvailability.InServiceCount = Convert.ToInt64(columns[5]);
                equipmentAvailability.InTransferCount = Convert.ToInt64(columns[6]);
                equipmentAvailability.LostStolenCount = Convert.ToInt64(columns[7]);
                equipmentAvailability.FirstTransferCount = Convert.ToInt64(columns[8]);
                equipmentAvailability.Availability = Convert.ToInt64(columns[10].Replace('%',' '));
                equipmentAvailability.PublishDate = publishDate.Ticks;
                equipmentAvailability.DailyReservationCount = 0;
                equipmentAvailability.DailyRevenue = 0;
                equipmentAvailability.FurtherReservationCount = 0;
                equipmentAvailability.GroupCode = "F";
                equipmentAvailability.GroupCodeInformationId = new Guid("0d57e9ff-24b2-e911-a851-000d3a2dd1b4");
                equipmentAvailability.IsFranchise = false;
                equipmentAvailability.LongTermTransferCount = 0;
                equipmentAvailability.MissingInventoriesCount = 0;
                equipmentAvailability.OthersCount = 0;
                equipmentAvailability.OutgoingContractCount = 0;
                equipmentAvailability.PertCount = 0;
                equipmentAvailability.ReachedRevenue = 0;
                equipmentAvailability.RegionManager = null;
                equipmentAvailability.RegionManagerId = null;
                equipmentAvailability.RemainingReservationCount = 0;
                equipmentAvailability.RentalContractCount = 0;
                equipmentAvailability.ReservationCount = 0;
                equipmentAvailability.SecondHandTransferCount = 0;
                equipmentAvailability.SecondHandTransferWaitingConfirmationCount = 0;
                equipmentAvailability.WaitingForMaintenanceCount = 0;
                equipmentAvailabilityItems.Add(equipmentAvailability);
            }
            return equipmentAvailabilityItems;
        }
    }
}
