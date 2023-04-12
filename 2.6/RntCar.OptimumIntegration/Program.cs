using RntCar.Logger;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.OptimumIntegration
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                LoggerHelper logger = new LoggerHelper();
                logger.traceInfo("Optimum Integration process started");
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                logger.traceInfo("crm connection ok");
                OptimumIntegrationHelper helper = new OptimumIntegrationHelper(crmServiceHelper.IOrganizationService);
                helper.Process();
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
        }
    }
}
