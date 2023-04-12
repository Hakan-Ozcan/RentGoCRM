using RntCar.ClassLibrary;
using System;

namespace RntCar.PaymentHelper.NetTahsilat
{
    public class NetTahsilatProvider : IPaymentProvider
    {
        public NetTahsilatProvider()
        {

        }
        public PaymentResponse makePayment(CreatePaymentParameters createPaymentParameters)
        {
            NetTahsilatHelper netTahsilatHelper = new NetTahsilatHelper(new NetTahsilatConfiguration
            {
                nettahsilat_password = createPaymentParameters.password,
                nettahsilat_username = createPaymentParameters.userName,
                nettahsilat_url = createPaymentParameters.baseurl,
                nettahsilat_vendorcode = createPaymentParameters.vendorCode,
                nettahsilat_vendorid = createPaymentParameters.vendorId,
            });
            return netTahsilatHelper.makePayment(createPaymentParameters);
        }

        public RefundResponse makeRefund(CreateRefundParameters createRefundParameters)
        {
            NetTahsilatHelper netTahsilatHelper = new NetTahsilatHelper(new NetTahsilatConfiguration
            {
                nettahsilat_password = createRefundParameters.password,
                nettahsilat_username = createRefundParameters.userName,
                nettahsilat_url = createRefundParameters.baseurl
            });

            return netTahsilatHelper.makeRefund(createRefundParameters);
        }

        public InstallmentResponse retrieveInstallment(RetrieveInstallmentParameters retrieveInstallmentParameters)
        {
            NetTahsilatHelper netTahsilatHelper = new NetTahsilatHelper(new NetTahsilatConfiguration
            {
                nettahsilat_password = retrieveInstallmentParameters.password,
                nettahsilat_username = retrieveInstallmentParameters.userName,
                nettahsilat_url = retrieveInstallmentParameters.baseurl
            });
            return netTahsilatHelper.retrieveInstallmentforGivenCardBin(retrieveInstallmentParameters.cardBin, retrieveInstallmentParameters.amount);
        }

        public VirtualPosResponse retrieveDefaultPosIdforGivenCard(RetrieveVirtualPosIdParameters retrieveVirtualPosIdParameters)
        {
            NetTahsilatHelper netTahsilatHelper = new NetTahsilatHelper(new NetTahsilatConfiguration
            {
                nettahsilat_password = retrieveVirtualPosIdParameters.password,
                nettahsilat_username = retrieveVirtualPosIdParameters.userName,
                nettahsilat_url = retrieveVirtualPosIdParameters.baseurl,
            });
            return netTahsilatHelper.retrieveVPosIdforGivenCard(retrieveVirtualPosIdParameters);
        }

        public Reservation3dPaymentReturnResponse Payment3dReturn(Iyzico3DReturnParameter createThreedsPaymentRequestForReturn3D)
        {
            throw new NotImplementedException();
        }

        public CreateCreditCardResponse createCreditCard(CreateCreditCardParameters createCreditCardParameters)
        {
            NetTahsilatHelper netTahsilatHelper = new NetTahsilatHelper(new NetTahsilatConfiguration
            {
                nettahsilat_password = createCreditCardParameters.password,
                nettahsilat_username = createCreditCardParameters.userName,
                nettahsilat_url = createCreditCardParameters.baseurl,
                nettahsilat_vendorcode = createCreditCardParameters.vendorCode,
                nettahsilat_vendorid = createCreditCardParameters.vendorId
            });
            return netTahsilatHelper.createCreditCard(createCreditCardParameters);

        }
    }
}
