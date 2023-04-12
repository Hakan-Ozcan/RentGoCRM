using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.CreditCard
{
    public class ExecuteGetCustomerCreditCards : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            string customerId;
            initializer.PluginContext.GetContextParameter<string>("customerId", out customerId);
            initializer.TraceMe("customerId" + customerId);
            string reservationId;
            initializer.PluginContext.GetContextParameter<string>("reservationId", out reservationId);
            initializer.TraceMe("reservationId" + reservationId);
            string contractId;
            initializer.PluginContext.GetContextParameter<string>("contractId", out contractId);
            initializer.TraceMe("contractId" + contractId);
            string pickupBranchId;
            initializer.PluginContext.GetContextParameter<string>("pickupBranchId", out pickupBranchId);
            initializer.TraceMe("pickupBranchId" + pickupBranchId);

            try
            {

                CreditCardBL creditCardBL = new CreditCardBL(initializer.Service, initializer.TracingService);
                var reservationGuid = string.IsNullOrWhiteSpace(reservationId) ? Guid.Empty : new Guid(reservationId);
                var contractGuid = string.IsNullOrWhiteSpace(contractId) ? Guid.Empty : new Guid(contractId);
                var pickupBranchGuid = string.IsNullOrWhiteSpace(pickupBranchId) ? Guid.Empty : new Guid(pickupBranchId);
                var response = creditCardBL.getCustomerCreditCards(customerId, reservationGuid, contractGuid, pickupBranchGuid);
                initializer.TraceMe("response:" + JsonConvert.SerializeObject(response));
                initializer.PluginContext.OutputParameters["CreditCardResponse"] = JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
