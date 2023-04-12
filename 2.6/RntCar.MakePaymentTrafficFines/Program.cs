using RntCar.IntegrationHelper;
using RntCar.SDK.Common;

namespace RntCar.MakePaymentTrafficFines
{
    class Program
    {
        static void Main(string[] args)
        {
            EIhlalHelper eihlalHelper = new EIhlalHelper(new CrmServiceHelper().CrmServiceClient);
            eihlalHelper.processTrafficFineBatch();
        }
    }
}
