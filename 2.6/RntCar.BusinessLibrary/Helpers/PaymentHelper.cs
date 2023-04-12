using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.PaymentHelper;
using RntCar.PaymentHelper.iyzico;
using RntCar.PaymentHelper.NetTahsilat;
using RntCar.SDK.Common;
using System;
using System.Linq;

namespace RntCar.BusinessLibrary.Helpers
{
    public class PaymentHelper : HelperHandler
    {
        public PaymentHelper(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public PaymentHelper(IOrganizationService organizationService) : base(organizationService)
        {
        }

        public PaymentHelper(IOrganizationService organizationService, ITracingService tracingService) : base(organizationService, tracingService)
        {
        }

        public PaymentHelper(CrmServiceClient crmServiceClient, IOrganizationService organizationService) : base(crmServiceClient, organizationService)
        {
        }

        public decimal getInstallmentRatio(CreditCardData creditCardData, int installmentNumber)
        {
            InstallmentResponse installmentResponse = null;
            ConfigurationBL configurationBL = new ConfigurationBL(this.IOrganizationService);
            var iyzicoInfo = configurationBL.GetConfigurationByName("iyzicoInformation");
            //parse configs
            var configs = IyzicoHelper.setConfigurationValues(iyzicoInfo);
            IyzicoHelper iyzicoHelper = new IyzicoHelper(configs);

            if (string.IsNullOrEmpty(creditCardData.cardToken) && string.IsNullOrEmpty(creditCardData.cardUserKey))
            {
                this.Trace("creditCardData.cardToken is null");
                installmentResponse = iyzicoHelper.retrieveInstallmentforGivenCardBin(creditCardData.creditCardNumber.removeEmptyCharacters().Substring(0, 6), StaticHelper.hundred);
            }
            else
            {
                this.Trace("creditCardData.cardToken not null");
                CreditCardRepository creditCardRepository = new CreditCardRepository(this.IOrganizationService, this.CrmServiceClient);
                var bin = creditCardRepository.getCreditCardByUserKeyandCreditCardTokenByGivenColumns(creditCardData.cardUserKey, creditCardData.cardToken, new string[] { "rnt_binnumber" });
                installmentResponse = iyzicoHelper.retrieveInstallmentforGivenCardBin(bin.GetAttributeValue<string>("rnt_binnumber"), StaticHelper.hundred);
            }

            //now calculate the installment ratio
            var relatedInstallment = installmentResponse.installmentData.FirstOrDefault().Where(p => p.installmentNumber == installmentNumber).FirstOrDefault();
            var relatedInstallmentOne = installmentResponse.installmentData.FirstOrDefault().Where(p => p.installmentNumber == 1).FirstOrDefault();

            this.Trace("relatedInstallment.totalAmount" + relatedInstallment.totalAmount);
            this.Trace("relatedInstallmentOne.totalAmount" + relatedInstallmentOne.totalAmount);
            var installmentRatio = ((relatedInstallment.totalAmount * 100) / relatedInstallmentOne.totalAmount) - 100;

            this.Trace("installmentRatio" + installmentRatio);
            return installmentRatio;
        }

        public decimal getInstallmentRatio(string binNumber, int installmentNumber, int provider)
        {
            RetrieveInstallmentParameters retrieveInstallment = new RetrieveInstallmentParameters()
            {
                cardBin = binNumber,
                amount = StaticHelper.hundred
            };
            InstallmentResponse installmentResponse = null;
            IPaymentProvider paymentProvider = null;
            ConfigurationBL configurationBL = new ConfigurationBL(this.IOrganizationService);
            if (provider == (int)GlobalEnums.PaymentProvider.iyzico)
            {
                paymentProvider = Activator.CreateInstance(typeof(IyzicoProvider)) as IPaymentProvider;
                var iyzicoInfo = configurationBL.GetConfigurationByName("iyzicoInformation");
                var configs = IyzicoHelper.setConfigurationValues(iyzicoInfo);
                //setting auth info
                retrieveInstallment.baseurl = configs.iyzico_baseUrl;
                retrieveInstallment.secretKey = configs.iyzico_secretKey;
                retrieveInstallment.apikey = configs.iyzico_apiKey;
            }
            else if (provider == (int)GlobalEnums.PaymentProvider.nettahsilat)
            {
                paymentProvider = Activator.CreateInstance(typeof(NetTahsilatProvider)) as IPaymentProvider;
                var netTahsilatInfo = configurationBL.GetConfigurationByName("nettahsilatconfiguration");
                var configs = NetTahsilatHelper.setConfigurationValues(netTahsilatInfo);

                retrieveInstallment.userName = configs.nettahsilat_username;
                retrieveInstallment.password = configs.nettahsilat_password;
                retrieveInstallment.baseurl = configs.nettahsilat_url;
                retrieveInstallment.vendorCode = configs.nettahsilat_vendorcode;
                retrieveInstallment.vendorId = configs.nettahsilat_vendorid;
            }
            

            installmentResponse = paymentProvider.retrieveInstallment(retrieveInstallment);

            //now calculate the installment ratio
            var relatedInstallment = installmentResponse.installmentData.FirstOrDefault().Where(p => p.installmentNumber == installmentNumber).FirstOrDefault();
            var relatedInstallmentOne = installmentResponse.installmentData.FirstOrDefault().Where(p => p.installmentNumber == 1).FirstOrDefault();

            var installmentRatio = 1.0M;
            if (relatedInstallment!=null)
            {
                this.Trace("relatedInstallment.totalAmount" + relatedInstallment.totalAmount); 
                installmentRatio = ((relatedInstallment.totalAmount * 100) / relatedInstallmentOne.totalAmount) - 100;
            }
            
            this.Trace("relatedInstallmentOne.totalAmount" + relatedInstallmentOne.totalAmount);
            this.Trace("installmentRatio" + installmentRatio);
            return installmentRatio;
        }
    }
}
