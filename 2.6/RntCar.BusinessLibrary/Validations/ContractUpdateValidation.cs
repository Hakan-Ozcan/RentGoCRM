using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Validations
{
    public class ContractUpdateValidation : ValidationHandler
    {
        public ContractUpdateValidation(IOrganizationService orgService) : base(orgService)
        {
        }

        public ContractUpdateValidation(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }
        /// <summary>
        /// Car change request not allowed for waiting for delivery
        /// </summary>
        /// <param name="contractId"></param>
        /// <returns></returns>
        public ContractUpdateResponse checkContractStatusForCarChangeRequest(Guid contractId, int? langId)
        {
            ContractRepository contractRepository = new ContractRepository(this.OrgService);
            var contract = contractRepository.getContractById(contractId, new string[] { "statuscode" });
            var contractStatusCode = contract.Attributes.Contains("statuscode") ? contract.GetAttributeValue<OptionSetValue>("statuscode").Value : 0;

            if (contractStatusCode == (int)ContractEnums.StatusCode.WaitingforDelivery)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                var message = xrmHelper.GetXmlTagContentByGivenLangId("ContractUpdateCarChangeValidation", langId, this.contractXmlPath);
                return new ContractUpdateResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }

            return new ContractUpdateResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }
        public bool checkContractStatusForUpdate(Entity contract)
        {
            var contractStatusCode = contract.Attributes.Contains("statuscode") ? contract.GetAttributeValue<OptionSetValue>("statuscode").Value : 0;
            if (contractStatusCode == (int)ClassLibrary._Enums_1033.rnt_contract_StatusCode.Completed ||
                contractStatusCode == (int)ClassLibrary._Enums_1033.rnt_contract_StatusCode.CancelledbyCustomer ||
                contractStatusCode == (int)ClassLibrary._Enums_1033.rnt_contract_StatusCode.CancelledbyRentGo )
            {
                return false;
            }
            return true;
        }

        public ContractValidationResponse checkContractHasEquipmentChanged(Entity contract, int langId)
        {
            ContractItemRepository contractItemRepository = new ContractItemRepository(this.OrgService);
            var contractItems = contractItemRepository.getRentalandWaitingforDeliveryEquipmentContractItemsByContractId(contract.Id, new string[] { });
            if (contractItems.Count == 2)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                var message = xrmHelper.GetXmlTagContentByGivenLangId("EquipmentChangeValidation", langId, this.contractXmlPath);
                return new ContractValidationResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }
            return new ContractValidationResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }

        public ContractValidationResponse checkMonthlyValidations(Entity contract, DateTime dropoffDateTime,bool passValidation,bool carChanged)
        {
            if (passValidation)
            {
                return new ContractValidationResponse
                {
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
            if (contract.GetAttributeValue<bool>("rnt_ismonthly"))
            {            
                var between = dropoffDateTime.isBetweenNotEqualEnd(contract.GetAttributeValue<DateTime>("rnt_pickupdatetime"), contract.GetAttributeValue<DateTime>("rnt_dropoffdatetime"));
                if (between)
                {
                    if (!carChanged)
                    {
                        return new ContractValidationResponse
                        {
                            ResponseResult = ResponseResult.ReturnError("Aylık devam eden bir sözleşme , kısaltılamaz.Lütfen tabletten iade alınız.")
                        };
                    }
                }
                else 
                {
                    if (carChanged && dropoffDateTime != contract.GetAttributeValue<DateTime>("rnt_dropoffdatetime"))
                    {
                        return new ContractValidationResponse
                        {
                            ResponseResult = ResponseResult.ReturnError("Aylık sözleşmelerde , Araç değişimi ve uzama aynı anda yapılamaz.Araç değişimini tamamladıktan sonra , uzama işlemini gerçekleştirebilirsiniz.")
                        };
                    }
                }


            }
            return new ContractValidationResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }
    }
}
