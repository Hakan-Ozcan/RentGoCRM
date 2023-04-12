using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;

namespace RntCar.BusinessLibrary.Validations
{
    public class PaymentValidations : ValidationHandler
    {
        public PaymentValidations(IOrganizationService orgService) : base(orgService)
        {
        }

        public PaymentValidations(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public PaymentValidations(IOrganizationService orgService, Guid userId) : base(orgService, userId)
        {
        }

        public ResponseResult checkMakePaymentCreditCardParameters(int langId, CreateCreditCardParameters createCreditCardParameters, ExistingCreditCardParamaters ExistingCreditCardParamaters)
        {
            if (createCreditCardParameters != null && ExistingCreditCardParamaters != null)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                return ResponseResult.ReturnError(xrmHelper.GetXmlTagContentByGivenLangId("InvalidCustomerId",
                                                                                                      createCreditCardParameters.langId,
                                                                                                      this.paymentXmlPath));
            }
            return ResponseResult.ReturnSuccess();
        }
    }
}
