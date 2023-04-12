using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBRecordUpdate
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("process started");
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                Console.WriteLine("crm connection ok");
                MongoDBRecordUpdateHelper mongoDBRecordUpdateHelper = new MongoDBRecordUpdateHelper(crmServiceHelper.IOrganizationService);
                Console.WriteLine("update equipments started");
                mongoDBRecordUpdateHelper.updateEquipments();
                Console.WriteLine("operation completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
