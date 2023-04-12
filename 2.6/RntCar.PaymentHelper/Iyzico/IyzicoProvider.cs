using Iyzipay.Request;
using RntCar.ClassLibrary; 

namespace RntCar.PaymentHelper.iyzico
{
    public class IyzicoProvider : IPaymentProvider
    {
        public PaymentResponse makePayment(CreatePaymentParameters createPaymentParameters)
        {
            IyzicoHelper iyzicoHelper = new IyzicoHelper(new IyzicoConfiguration
            {
                iyzico_baseUrl = createPaymentParameters.baseurl,
                iyzico_apiKey = createPaymentParameters.apikey,
                iyzico_secretKey = createPaymentParameters.secretKey
            });

            return iyzicoHelper.makePayment(createPaymentParameters);
        }

        public RefundResponse makeRefund(CreateRefundParameters createRefundParameters)
        {
            IyzicoHelper iyzicoHelper = new IyzicoHelper(new IyzicoConfiguration
            {
                iyzico_baseUrl = createRefundParameters.baseurl,
                iyzico_apiKey = createRefundParameters.apikey,
                iyzico_secretKey = createRefundParameters.secretKey
            });
            return iyzicoHelper.makeRefund(createRefundParameters);
        }

        public VirtualPosResponse retrieveDefaultPosIdforGivenCard(RetrieveVirtualPosIdParameters retrieveVirtualPosIdParameters)
        {
            throw new System.NotImplementedException();
        }

        public InstallmentResponse retrieveInstallment(RetrieveInstallmentParameters retrieveInstallmentParameters)
        {
            IyzicoHelper iyzicoHelper = new IyzicoHelper(new IyzicoConfiguration
            {
                iyzico_baseUrl = retrieveInstallmentParameters.baseurl,
                iyzico_apiKey = retrieveInstallmentParameters.apikey,
                iyzico_secretKey = retrieveInstallmentParameters.secretKey
            });
            return iyzicoHelper.retrieveInstallmentforGivenCardBin(retrieveInstallmentParameters.cardBin,retrieveInstallmentParameters.amount);
        }

        public Reservation3dPaymentReturnResponse Payment3dReturn(Iyzico3DReturnParameter createThreedsPaymentRequestForReturn3D)
        {
            IyzicoHelper iyzicoHelper = new IyzicoHelper(new IyzicoConfiguration
            {
                iyzico_baseUrl = createThreedsPaymentRequestForReturn3D.baseurl,
                iyzico_apiKey = createThreedsPaymentRequestForReturn3D.apikey,
                iyzico_secretKey = createThreedsPaymentRequestForReturn3D.secretKey
            }); 
            return iyzicoHelper.Payment3DReturn(createThreedsPaymentRequestForReturn3D);
        }

        public CreateCreditCardResponse createCreditCard(CreateCreditCardParameters createCreditCardParameters)
        {
            IyzicoHelper iyzicoHelper = new IyzicoHelper(new IyzicoConfiguration
            {
                iyzico_baseUrl = createCreditCardParameters.baseurl,
                iyzico_apiKey = createCreditCardParameters.apikey,
                iyzico_secretKey = createCreditCardParameters.secretKey
            });
            return iyzicoHelper.createCreditCard(createCreditCardParameters);

        }
    }
}
