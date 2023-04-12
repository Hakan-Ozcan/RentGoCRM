using RntCar.BusinessLibrary.Business;
using RntCar.MongoDBHelper.Entities;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.DailyFleetReport
{
    class Program
    {
        static void Main(string[] args)
        {
            var mongoDBHostName = StaticHelper.GetConfiguration("MongoDBHostName");
            var mongoDBDatabaseName = StaticHelper.GetConfiguration("MongoDBDatabaseName");

            var startDate = StaticHelper.GetConfiguration("startDate");
            var hour = StaticHelper.GetConfiguration("hour");

            int? hourValue = null;
            DateTime? startDateValue = null;

            if (!string.IsNullOrWhiteSpace(startDate) && !string.IsNullOrWhiteSpace(hour))
            {
                startDateValue = DateTime.Parse(startDate);
                hourValue = int.Parse(hour);
            }

            try
            {
                if (DateTime.Now.Hour == 5
                    || DateTime.Now.Hour == 8
                    || DateTime.Now.Hour == 11
                    || DateTime.Now.Hour == 14
                    || DateTime.Now.Hour == 17
                    || DateTime.Now.Hour == 20
                    || DateTime.Now.Hour == 23)
                {
                    CrmServiceHelper crmServiceHelper = new CrmServiceHelper();

                    EquipmentBL equipmentBL = new EquipmentBL(crmServiceHelper.IOrganizationService);
                    EquipmentBusiness equipmentBusiness = new EquipmentBusiness(mongoDBHostName, mongoDBDatabaseName);

                    var fleetReportData = equipmentBL.getEquipmentDataForFleetReport(startDateValue, hourValue);
                    foreach (var item in fleetReportData)
                    {
                        equipmentBusiness.createFleetReportData(item);
                    }
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
