using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Validations
{
    public class InvoiceCreationValidation : ValidationHandler
    {
        public InvoiceCreationValidation(IOrganizationService orgService) : base(orgService)
        {
        }

        public InvoiceCreationValidation(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }
        public CreateInvoiceWithLogoResponse checkContractStatus(int contractStatus, int langId)
        {
            if (contractStatus == (int)(int)ClassLibrary._Enums_1033.rnt_contract_StatusCode.Rental ||
                contractStatus == (int)(int)ClassLibrary._Enums_1033.rnt_contract_StatusCode.WaitingForDelivery)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                var message = xrmHelper.GetXmlTagContentByGivenLangId("ContractStatusError", langId, this.invoiceXmlPath);
                return new CreateInvoiceWithLogoResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                    // invoice can only be created for completed or cancelled contracts
                };
            }
            return new CreateInvoiceWithLogoResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }
        public CreateInvoiceWithLogoResponse checkContractStatusForCancellation(List<Entity> contractItems, int contractStatus, int langId)
        {
            if (contractStatus == (int)ContractEnums.CancellationReason.ByCustomer ||
                contractStatus == (int)ContractEnums.CancellationReason.ByRentgo)
            {
                ConfigurationRepository configurationRepository = new ConfigurationRepository(this.OrgService);
                var productCode = configurationRepository.GetConfigurationByKey("additionalProduct_cancellationFeeCode");

                AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);
                var additionalProduct = additionalProductRepository.getAdditionalProductByProductCode(productCode);

                var cancellationFeeItem = contractItems.Where(item => item.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id == additionalProduct.Id).FirstOrDefault();
                if (cancellationFeeItem == null)
                {
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("CancellationFeeItemNotFound", langId, this.invoiceXmlPath);
                    return new CreateInvoiceWithLogoResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(message)
                    };
                    //cancellation fee amount must be greater than zero
                }
            }
            return new CreateInvoiceWithLogoResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }
        public CreateInvoiceWithLogoResponse checkContractHasActiveOrFaultyInvoice(List<Entity> invoices, int langId)
        {
            if (invoices.Count < 1)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                var message = xrmHelper.GetXmlTagContentByGivenLangId("NoFaultyOrActiveInvoice", langId, this.invoiceXmlPath);
                return new CreateInvoiceWithLogoResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
                //There is no faulty or active invoice found
            }
            return new CreateInvoiceWithLogoResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }
        public CreateInvoiceWithLogoResponse checkReservationHasActiveOrFaultyInvoice(Entity invoice, int langId)
        {
            if (invoice == null)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                var message = xrmHelper.GetXmlTagContentByGivenLangId("NoFaultyOrActiveInvoice", langId, this.invoiceXmlPath);
                return new CreateInvoiceWithLogoResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
                //There is no faulty or active invoice found
            }
            return new CreateInvoiceWithLogoResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }
        public CreateInvoiceWithLogoResponse checkReservationStatus(int reservationStatus, int langId)
        {
            if (reservationStatus != (int)ClassLibrary._Enums_1033.rnt_reservation_StatusCode.CancelledByRentgo &&
                reservationStatus != (int)ClassLibrary._Enums_1033.rnt_reservation_StatusCode.CancelledByCustomer)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                var message = xrmHelper.GetXmlTagContentByGivenLangId("ReservationStatusError", langId, this.invoiceXmlPath);
                return new CreateInvoiceWithLogoResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                    // invoice can only be created for cancelled reservations
                };
            }
            return new CreateInvoiceWithLogoResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }
        public CreateInvoiceWithLogoResponse checkReservationHasCancellationFeeItem(Entity cancellationFee, int langId)
        {
            if (cancellationFee == null)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                var message = xrmHelper.GetXmlTagContentByGivenLangId("CancellationFeeItemNotFound", langId, this.invoiceXmlPath);
                return new CreateInvoiceWithLogoResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message),
                };
            }
            return new CreateInvoiceWithLogoResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }
        public CreateInvoiceWithLogoResponse checkDocumentTotalAmount(decimal documentTotalAmount, decimal invoiceTotalAmount, int langId)
        {

            if (invoiceTotalAmount != documentTotalAmount)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                var message = xrmHelper.GetXmlTagContentByGivenLangId("TotalAmountValidation", langId, this.invoiceXmlPath);
                return new CreateInvoiceWithLogoResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
                //items total amount must be eq to invoice header amount
            }
            return new CreateInvoiceWithLogoResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }
        public CreateInvoiceWithLogoResponse checkTaxNumberFormat(Entity invoice, int langId)
        {
            var invoiceType = invoice.GetAttributeValue<OptionSetValue>("rnt_invoicetypecode").Value;
            var taxNumber = invoice.GetAttributeValue<string>("rnt_taxnumber");

            if (invoiceType == (int)ClassLibrary._Enums_1033.rnt_invoice_rnt_invoicetypecode.Corporate)
            {
                CorporateCustomerValidation corporateCustomerValidation = new CorporateCustomerValidation(this.OrgService);
                var result = corporateCustomerValidation.isTaxnoValid(taxNumber);

                if (!result)
                {
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("InvalidTaxNumber", langId);

                    return new CreateInvoiceWithLogoResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(message)
                    };
                }

                return new CreateInvoiceWithLogoResponse
                {
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }

            return new CreateInvoiceWithLogoResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }
        public InvoiceValidationResponse checkInvoiceIdentiyKey(int invoiceType, string key)
        {
            //todo xml
            if (invoiceType == (int)rnt_invoice_rnt_invoicetypecode.Individual && key.Length != 11)
            {
                return new InvoiceValidationResponse
                {
                    result = false,
                    exceptionDetail = "TCKN 11 hane olmalıdır"
                };
            }
            else if (invoiceType == (int)rnt_invoice_rnt_invoicetypecode.Corporate && key.Length != 10)
            {
                return new InvoiceValidationResponse
                {
                    result = false,
                    exceptionDetail = "Vergi No 10 hane olmalıdır"
                };
            }
            if (invoiceType == (int)rnt_invoice_rnt_invoicetypecode.Individual)
            {
                if (!CommonHelper.checkGovermentIdValidity(key))
                {
                    return new InvoiceValidationResponse
                    {
                        result = false,
                        exceptionDetail = "Geçersiz TCKN"
                    };
                }
            }
            else
            {
                CorporateCustomerValidation corporateCustomerValidation = new CorporateCustomerValidation(this.OrgService);
                if (!corporateCustomerValidation.isTaxnoValid(key))
                {
                    return new InvoiceValidationResponse
                    {
                        result = false,
                        exceptionDetail = "Geçersiz Vergi numarası"
                    };
                }
            }

            return new InvoiceValidationResponse
            {
                result = true
            };
        }
    }
}
