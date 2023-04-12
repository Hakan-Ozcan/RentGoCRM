using RntCar.BusinessLibrary.Business;
using RntCar.Logger;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGSFtpEntegration
{
    class Program
    {
        static void Main(string[] args)
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            LoggerHelper loggerHelper = new LoggerHelper();

            //ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
            //var a = configurationBL.GetConfigurationByName("");
            //var configs = IyzicoHelper.setConfigurationValues(iyzicoInfo);
        }
    }
}
