using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;

namespace RntCar.BusinessLibrary.Validations
{
    public class CreditCardValidation : ValidationHandler
    {
        public CreditCardValidation(IOrganizationService orgService) : base(orgService)
        {
        }

        public CreditCardValidation(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public CreditCardValidation(IOrganizationService orgService, Guid userId) : base(orgService, userId)
        {
        }

        public CreditCardValidationResponse checkCreditCard(CreditCardData creditCardData, Guid contactId, int langId)
        {
            if (creditCardData == null)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                var message = xrmHelper.GetXmlTagContentByGivenLangId("MissingCreditCardInfo", langId, this.reservationXmlPath);
                return new CreditCardValidationResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }
            if (creditCardData.creditCardId != (Guid?)null)
            {
                this.Trace("customer credit cardId is not null");
                if (string.IsNullOrEmpty(creditCardData.cardUserKey) || string.IsNullOrEmpty(creditCardData.cardToken))
                {
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("MissingCreditCardInfo", langId, this.reservationXmlPath);
                    return new CreditCardValidationResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(message)
                    };
                }
            }
            else
            {
                if ((string.IsNullOrEmpty(creditCardData.cardHolderName) || string.IsNullOrEmpty(creditCardData.creditCardNumber) ||
                     creditCardData.expireMonth == null || creditCardData.expireYear == null && creditCardData.cvc == null))
                {
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("MissingCreditCardInfo", langId, this.reservationXmlPath);
                    return new CreditCardValidationResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(message)
                    };
                }
                else
                {
                    IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(this.OrgService);
                    var customer = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(contactId, new string[] { "fullname" });
                    var customerName = customer.GetAttributeValue<string>("fullname").ToLower();

                    var formattedCrmValue = customerName.removeEmptyCharacters().ToLower().replaceTurkishCharacters();
                    var formattedInputValue = creditCardData.cardHolderName.removeEmptyCharacters().ToLowerInvariant().replaceTurkishCharacters();

                    this.Trace("crm name : " + formattedCrmValue);
                    this.Trace("input name : formattedInputValue" + creditCardData.cardHolderName.removeEmptyCharacters().ToLowerInvariant().replaceTurkishCharacters());

                    if (formattedCrmValue != formattedInputValue)
                    {
                        XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                        var message = xrmHelper.GetXmlTagContentByGivenLangId("CreditCardHolderNameValidation", langId, this.reservationXmlPath);
                        return new CreditCardValidationResponse
                        {
                            ResponseResult = ResponseResult.ReturnError(message)
                        };
                    }
                }

            }
            return new CreditCardValidationResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }

        //public CreditCardValidationResponse<bool> checkCreditCard_BeforeContract(ContractPriceParameters contractPriceParameters, int langId)
        //{
        //    if (!string.IsNullOrEmpty(contractPriceParameters.customerCreditCardId))
        //    {
        //        this.Trace("customer credit cardId is not null");
        //        if (string.IsNullOrEmpty(contractPriceParameters.cardUserKey) || string.IsNullOrEmpty(contractPriceParameters.cardToken))
        //        {
        //            XrmHelper xrmHelper = new XrmHelper(this.OrgService);
        //            var message = xrmHelper.GetXmlTagContentByGivenLangId("MissingCreditCardInfo", langId, this.reservationXmlPath);
        //            return new CreditCardValidationResponse<bool>
        //            {
        //                ResponseResult = ResponseResult<bool>.ReturnError(message)
        //            };
        //        }
        //    }
        //    else
        //    {
        //        if ((string.IsNullOrEmpty(contractPriceParameters.cardHolderName) || string.IsNullOrEmpty(contractPriceParameters.creditCardNumber) ||
        //             contractPriceParameters.expireMonth == null || contractPriceParameters.expireYear == null && contractPriceParameters.cvc == null))
        //        {
        //            XrmHelper xrmHelper = new XrmHelper(this.OrgService);
        //            var message = xrmHelper.GetXmlTagContentByGivenLangId("MissingCreditCardInfo", langId, this.reservationXmlPath);
        //            return new CreditCardValidationResponse<bool>
        //            {
        //                ResponseResult = ResponseResult<bool>.ReturnError(message)
        //            };
        //        }

        //    }
        //    return new CreditCardValidationResponse<bool>
        //    {
        //        ResponseResult = ResponseResult<bool>.ReturnSuccess()
        //    };
        //}
    }
}
