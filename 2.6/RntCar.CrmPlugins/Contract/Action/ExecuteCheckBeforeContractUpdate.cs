using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.BusinessLibrary.Validations;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.Contract.Action
{
    public class ExecuteCheckBeforeContractUpdate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            try
            {
                string contractId;
                initializer.PluginContext.GetContextParameter<string>("ContractId", out contractId);

                int langId;
                initializer.PluginContext.GetContextParameter<int>("LangId", out langId);

                ContractRepository contractRepository = new ContractRepository(initializer.Service);
                var contract = contractRepository.getContractById(Guid.Parse(contractId), new string[] { "statecode", "statuscode", "rnt_customerid" });

                #region Retrieve Individual Customer Detail Data
                var contactId = contract.GetAttributeValue<EntityReference>("rnt_customerid").Id;
                initializer.TraceMe("individualCustomer retrieve start");
                IndividualCustomerBL individualCustomerBL = new IndividualCustomerBL(initializer.Service);
                var individualCustomer = individualCustomerBL.getIndividualCustomerInformationForValidation(contactId);
                initializer.TraceMe("individualCustomer retrieve end");
                #endregion

                #region Black List Validation
                initializer.TraceMe("Black List Validation start");
                BlackListBL blackListBL = new BlackListBL(initializer.Service);
                initializer.TraceMe("black list governmentId : " + individualCustomer.governmentId);
                var blackListValidation = blackListBL.BlackListValidation(individualCustomer.governmentId);
                initializer.TraceMe("IsInBlackList : " + blackListValidation.BlackList.IsInBlackList);
                if (blackListValidation.BlackList.IsInBlackList)
                {
                    XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("BlackListValidation", langId);
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                }
                initializer.TraceMe("Black List Validation end");
                #endregion

                ContractUpdateValidation contractUpdateValidation = new ContractUpdateValidation(initializer.Service);
                var response = contractUpdateValidation.checkContractStatusForUpdate(contract);
                if (!response)
                {
                    XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("ContractUpdateStatus", langId);
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                }

                var res = contractUpdateValidation.checkContractHasEquipmentChanged(contract, langId);
                if (!res.ResponseResult.Result)
                {                    
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(res.ResponseResult.ExceptionDetail);
                }
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
