using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.ContractItem
{
    public class PostContractItemTotalDurationCalculationPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            try
            {
                Entity contractItem;
                initializer.PluginContext.GetContextPostImages<Entity>(initializer.PostImgKey, out contractItem);
                var pickupDateTime = contractItem.GetAttributeValue<DateTime>("rnt_pickupdatetime");
                var dropoffDateTime = contractItem.GetAttributeValue<DateTime>("rnt_dropoffdatetime");
                var itemTypeCode = contractItem.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value;
                var contractItemId = contractItem.Id;

                ContractItemBL contractItemBL = new ContractItemBL(initializer.Service, initializer.TracingService);
                contractItemBL.updateContractItemTotalDuration(contractItemId, itemTypeCode, pickupDateTime, dropoffDateTime);

            }
            catch (Exception ex)
            {
                initializer.TraceMe("PostContractItemTotalDurationCalculationPlugin exception : " + ex.Message);
            }
        }
    }
}
