using RntCar.ClassLibrary;

namespace RntCar.PaymentHelper
{
    public interface IPaymentProvider
    {
        PaymentResponse makePayment(CreatePaymentParameters createPaymentParameters);

        RefundResponse makeRefund(CreateRefundParameters createRefundParameters);

        InstallmentResponse retrieveInstallment(RetrieveInstallmentParameters retrieveInstallmentParameters);

        VirtualPosResponse retrieveDefaultPosIdforGivenCard(RetrieveVirtualPosIdParameters retrieveVirtualPosIdParameters);

        Reservation3dPaymentReturnResponse Payment3dReturn(Iyzico3DReturnParameter createThreedsPaymentRequestForReturn3D);

        CreateCreditCardResponse createCreditCard(CreateCreditCardParameters createCreditCardParameters);

    }
}
