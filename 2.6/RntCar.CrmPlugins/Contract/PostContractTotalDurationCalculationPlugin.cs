using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Contract
{
    public class PostContractTotalDurationCalculationPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            try
            {
                Entity contract;
                initializer.PluginContext.GetContextPostImages<Entity>(initializer.PostImgKey, out contract);
                var pickupDateTime = contract.GetAttributeValue<DateTime>("rnt_pickupdatetime");
                var dropoffDateTime = contract.GetAttributeValue<DateTime>("rnt_dropoffdatetime");
                var contractId = contract.GetAttributeValue<Guid>("rnt_contractid");

                initializer.TraceMe("pickupDateTime: " + pickupDateTime);
                initializer.TraceMe("dropoffDateTime: " + dropoffDateTime);

                ContractBL contractBL = new ContractBL(initializer.Service, initializer.TracingService);
                contractBL.updateContractTotalDuration(contractId, pickupDateTime, dropoffDateTime);
            }
            catch (Exception ex)
            {
                initializer.TraceMe("PostContractTotalDurationCalculationPlugin exception : " + ex.Message);
            }
        }
    }
}
