using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System.Activities;

namespace RntCar.CrmWorkflows
{
    public class CalculateCustomerDebitAmount : CodeActivity
    {
        protected override void Execute(CodeActivityContext context)
        {
            PluginInitializer initializer = new PluginInitializer(context);

            ContractBL contractBL = new ContractBL(initializer.Service);
            var debitResponse = contractBL.calculateDebitAmount(initializer.WorkflowContext.PrimaryEntityId);
            
            initializer.TraceMe("debt response : " + JsonConvert.SerializeObject(debitResponse));

            IndividualCustomerBL individualCustomerBL = new IndividualCustomerBL(initializer.Service);
            individualCustomerBL.updateDebitAmount(debitResponse);
        }
    }
}
