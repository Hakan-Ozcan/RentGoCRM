using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Contract.Action
{
    public class ExecuteCalculateFineAmountContract : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            try
            {
                string contractId;
                initializer.PluginContext.GetContextParameter<string>("contractId", out contractId);
                
                string pnrNumber;
                initializer.PluginContext.GetContextParameter<string>("pnrNumber", out pnrNumber);

                int langId;
                initializer.PluginContext.GetContextParameter<int>("langId", out langId);

                initializer.TraceMe("langId" + langId);
                initializer.TraceMe("contractId" + contractId);
                initializer.TraceMe("pnrNumber" + pnrNumber);

                ContractBL contractBL = new ContractBL(initializer.Service, initializer.TracingService);
                var validationResponse = contractBL.checkBeforeContractCancellation(new ClassLibrary.ContractCancellationParameters
                {
                    pnrNumber = pnrNumber,
                    contractId = new Guid(contractId)
                }, langId);

                initializer.TraceMe("validationResponse : " + JsonConvert.SerializeObject(validationResponse));
                if (!validationResponse.ResponseResult.Result)
                {
                    initializer.PluginContext.OutputParameters["ContractFineAmountResponse"] = JsonConvert.SerializeObject(validationResponse);
                    return;
                }

                validationResponse = contractBL.calculateCancellationAmountForGivenContract(validationResponse,
                                                                                            new Guid(contractId),
                                                                                            validationResponse.willChargeFromUser,
                                                                                            langId);
                initializer.TraceMe("validationResponse1 : " + JsonConvert.SerializeObject(validationResponse));

                initializer.PluginContext.OutputParameters["ContractFineAmountResponse"] = JsonConvert.SerializeObject(validationResponse);
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
