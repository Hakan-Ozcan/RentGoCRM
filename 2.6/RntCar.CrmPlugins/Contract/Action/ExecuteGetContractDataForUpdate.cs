using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.CrmPlugins.Contract.Action
{
    public class ExecuteGetContractDataForUpdate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            try
            {

                string contractId;
                initializer.PluginContext.GetContextParameter<string>("ContractId", out contractId);

                string dateandBranch;
                initializer.PluginContext.GetContextParameter<string>("DateandBranch", out dateandBranch);

                int langId;
                initializer.PluginContext.GetContextParameter<int>("LangId", out langId);

                var dateandBranchParameter = JsonConvert.DeserializeObject<ContractDateandBranchParameters>(dateandBranch);

                BusinessLibrary.Business.ContractBL contractBL = new BusinessLibrary.Business.ContractBL(initializer.Service, initializer.TracingService);

                var response = contractBL.getContractDataForUpdate(contractId, dateandBranchParameter, langId);
                response.additionalProducts.ForEach(p => p.productDescription = null);

                ContractRepository contractRepository = new ContractRepository(initializer.Service);
                var con = contractRepository.getContractById(new Guid(contractId), new string[] { "rnt_ismonthly", "rnt_howmanymonths", "rnt_cancontinuewithmonthly" });

                response.canContinueMonthly = con.Contains("rnt_cancontinuewithmonthly") ? con.GetAttributeValue<bool>("rnt_cancontinuewithmonthly") : false;
                response.isMonthly = con.Contains("rnt_ismonthly") ? con.GetAttributeValue<bool>("rnt_ismonthly") : false;
                response.howManyMonths = con.Contains("rnt_howmanymonths") ? con.GetAttributeValue<OptionSetValue>("rnt_howmanymonths").Value : 0;
                //sözleşme güncelleşme ekranında gözükmesini istemiyor ama sözleşmeye eklenmişse.
                foreach (var item in response.additionalProducts)
                {
                    if (item.showOnContractUpdate == false && item.value >= 1)
                    {
                        item.isMandatory = true;
                    }
                }

                if (response.isMonthly)
                {
                    response.additionalProducts = response.additionalProducts.Where(p => p.showOnContractUpdateForMonthly == true || (p.showOnContractUpdateForMonthly == false && p.isMandatory == true)).ToList();
                }
                else
                {
                    response.additionalProducts = response.additionalProducts.Where(p => p.showOnContractUpdate == true || (p.showOnContractUpdate == false && p.isMandatory == true)).ToList();
                }
                initializer.TraceMe("response : " + JsonConvert.SerializeObject(response));

                ContractHelper contractHelper = new ContractHelper(initializer.Service);
                var oneWayFeeResponse = contractHelper.calculateNewOneWayFeeAmount(new Guid(contractId),
                                                                                   dateandBranchParameter.dropoffBranchId);

                if (oneWayFeeResponse.amount != decimal.Zero)
                {
                    var oneWayProduct = response.additionalProducts.Where(p => p.productId == oneWayFeeResponse.additionalProductId).FirstOrDefault();
                    oneWayProduct.actualAmount = oneWayFeeResponse.amount;
                    oneWayProduct.actualTotalAmount = oneWayFeeResponse.amount;
                }

                
                //aylıklarda hiçbir ek ürün eklenemez 
                //List<AdditionalProductData> excluded = new List<AdditionalProductData>();
                //if (response.isMonthly)
                //{
                //    foreach (var item in response.additionalProducts)
                //    {
                //        if (item.value == 0 && !item.showOnContractUpdateForMontly)
                //        {
                //            excluded.Add(item);
                //        }
                //    }
                //    response.additionalProducts = response.additionalProducts.Except(excluded).ToList();
                //}
                initializer.TraceMe("(response : " + JsonConvert.SerializeObject(response));
                initializer.TraceMe("ContractId : " + contractId);
                initializer.TraceMe("dateandBranch paramaters : " + dateandBranch);
                initializer.PluginContext.OutputParameters["ContractDataResponse"] = JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }

        }
    }
}
