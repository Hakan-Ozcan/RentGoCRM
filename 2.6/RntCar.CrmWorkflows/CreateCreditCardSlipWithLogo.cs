using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.SDK.Common;
using System;
using System.Activities;

namespace RntCar.CrmWorkflows
{
    public class CreateCreditCardSlipWithLogo : CodeActivity
    {
        [Input("Contract Reference")]
        [ReferenceTarget("rnt_contract")]
        public InArgument<EntityReference> _contract { get; set; }

        [Input("Reservation Reference")]
        [ReferenceTarget("rnt_reservation")]
        public InArgument<EntityReference> _reservation { get; set; }

        [Input("Payment Reference")]
        [ReferenceTarget("rnt_payment")]
        public InArgument<EntityReference> _payment { get; set; }

        [Input("Current Bank Name")]
        public InArgument<string> _currentBankName { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            PluginInitializer initializer = new PluginInitializer(context);

            var contractRef = _contract.Get<EntityReference>(context);
            var reservationRef = _reservation.Get<EntityReference>(context);
            var paymentRef = _payment.Get<EntityReference>(context);
            initializer.TraceMe("paymentRef : " + paymentRef.Id);
            initializer.TraceMe("paymentRef Name: " + paymentRef.Name);
            CreditCardSlipRepository creditCardSlipRepository = new CreditCardSlipRepository(initializer.Service);
            var creditCardSlip = creditCardSlipRepository.getCreditCardSlipByPaymentId(paymentRef.Id);
            CreditCardSlipBL creditCardSlipBL = new CreditCardSlipBL(initializer.Service, initializer.TracingService);
            var creditCardSlipId = Guid.Empty;

            if (creditCardSlip == null)
                creditCardSlipId = creditCardSlipBL.createCreditCarSlip(reservationRef?.Id, contractRef?.Id, paymentRef.Id);
            else
                creditCardSlipId = creditCardSlip.Id;
            try
            {
                PaymentRepository paymentRepository = new PaymentRepository(initializer.Service);
                var paymentEntity = paymentRepository.getPaymentByIdByGivenColumns(paymentRef.Id, new string[] { "rnt_transactionresult" });
                var transactionResult = paymentEntity.GetAttributeValue<OptionSetValue>("rnt_transactionresult").Value;
                if (transactionResult != (int)RntCar.ClassLibrary._Enums_1033.rnt_payment_rnt_transactionresult.Success &&
                    transactionResult != (int)RntCar.ClassLibrary._Enums_1033.rnt_payment_rnt_transactionresult.Success_WithoutPaymentTransaction)
                {
                    initializer.TraceMe("payment record status is not equal to success or Success_WithoutPaymentTransaction");
                    initializer.TraceMe("status : " + transactionResult);
                    return;
                }
                
                var response = creditCardSlipBL.handleCreateCreditCardSlipWithLogo(contractRef, reservationRef, paymentRef, creditCardSlipId);
                
            }
            catch (Exception ex)
            {
                creditCardSlipBL.updateCreditCardSlipWithInternalError(creditCardSlipId, ex.Message);
                //initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
