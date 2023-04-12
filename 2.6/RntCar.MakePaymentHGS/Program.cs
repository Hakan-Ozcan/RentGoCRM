using RntCar.IntegrationHelper;
using RntCar.SDK.Common;

namespace RntCar.MakePaymentHGS
{
    class Program
    {
        static void Main(string[] args)
        {
            HGSHelper hgsHelper = new HGSHelper(new CrmServiceHelper().IOrganizationService);
            hgsHelper.processHGSBatch();
           //hgsHelper.processHGSBatch(new System.Guid("ad6be534-2169-46f9-ab02-572a97007789"));
        }
    }
}
