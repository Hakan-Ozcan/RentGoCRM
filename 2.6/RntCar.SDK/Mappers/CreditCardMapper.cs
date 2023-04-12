using Microsoft.Xrm.Sdk;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Broker;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary._Mobile;
using RntCar.ClassLibrary._Web;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;

namespace RntCar.SDK.Mappers
{
    public class CreditCardMapper
    {
        public List<InstallmentData_Web> buildInstallmentData(List<InstallmentData> installmentDatas)
        {
            List<InstallmentData_Web> installmentDatas_Web = new List<InstallmentData_Web>();
            installmentDatas.ForEach(p => installmentDatas_Web.Add(new InstallmentData_Web().Map(p)));
            return installmentDatas_Web;
        }
        public List<InstallmentData_Mobile> buildInstallmentData_Mobile(List<InstallmentData> installmentDatas)
        {
            List<InstallmentData_Mobile> installmentDatas_Mobile = new List<InstallmentData_Mobile>();
            installmentDatas.ForEach(p => installmentDatas_Mobile.Add(new InstallmentData_Mobile().Map(p)));
            return installmentDatas_Mobile;
        }
        public RetrieveInstallmentParameters buildInstallmentData(RetrieveInstallmentParameters_Web retrieveInstallmentParameters_Web)
        {
            return new RetrieveInstallmentParameters
            {
                cardBin = retrieveInstallmentParameters_Web.cardBin,
                amount = retrieveInstallmentParameters_Web.amount
            };
        }
        public RetrieveInstallmentParameters buildInstallmentData_Mobile(RetrieveInstallmentParameters_Mobile retrieveInstallmentParameters_Mobile)
        {
            return new RetrieveInstallmentParameters
            {
                cardBin = retrieveInstallmentParameters_Mobile.cardBin,
                amount = retrieveInstallmentParameters_Mobile.amount
            };
        }
        public List<CreditCardData_Web> createWebCreditCardList(List<Entity> creditCards)
        {
            var convertedData = creditCards.ConvertAll(item => new CreditCardData_Web
            {
                creditCardNumber = item.GetAttributeValue<string>("rnt_name"),
                cardToken = item.GetAttributeValue<string>("rnt_cardtoken"),
                cardUserKey = item.GetAttributeValue<string>("rnt_carduserkey"),
                creditCardId = item.Id
            });
            return convertedData;
        }

        public List<CreditCardData_Mobile> createMobileCreditCardList(List<Entity> creditCards)
        {
            creditCards.RemoveAll(c => c.GetAttributeValue<OptionSetValue>("rnt_expireyear").Value == DateTime.Now.Year &&
                                                                c.GetAttributeValue<OptionSetValue>("rnt_expiremonthcode").Value < DateTime.Now.Month);


            var convertData = creditCards.ConvertAll(item => new CreditCardData_Mobile
            {
                cardHolderName = getHolderName(item.GetAttributeValue<string>("rnt_name")),
                bankName = item.Attributes.Contains("rnt_bank") ? item.GetAttributeValue<string>("rnt_bank") : string.Empty,
                cardType = item.Attributes.Contains("rnt_cardtypecode") ? getCardType(item.GetAttributeValue<OptionSetValue>("rnt_cardtypecode").Value) : getCardType(50),// 50 : Unknown
                creditCardNumber = item.GetAttributeValue<string>("rnt_creditcardnumber"),
                cardToken = item.GetAttributeValue<string>("rnt_cardtoken"),
                cardUserKey = item.GetAttributeValue<string>("rnt_carduserkey"),
                expireYear = item.GetAttributeValue<OptionSetValue>("rnt_expireyear").Value,
                expireMonth = item.GetAttributeValue<OptionSetValue>("rnt_expiremonthcode").Value,
                creditCardId = item.Id
            }); ;
            return convertData;
        }
        private string getHolderName(string name)
        {
            return name.Split('-')[0];
        }

        private string getCardType(int cardType)
        {
            return Enum.GetName(typeof(rnt_CardTypeCode), cardType);
        }
        public CreditCardData buildCreditCardData(CreditCardData_Web creditCardData_Web)
        {
            return new CreditCardData
            {
                cardUserKey = creditCardData_Web.cardUserKey,
                cardToken = creditCardData_Web.cardToken,
                creditCardId = creditCardData_Web.creditCardId == Guid.Empty ? null : (Guid?)creditCardData_Web.creditCardId,
                creditCardNumber = creditCardData_Web.creditCardNumber,
                expireMonth = creditCardData_Web.expireMonth,
                expireYear = creditCardData_Web.expireYear,
                cardHolderName = creditCardData_Web.cardHolderName,
                cvc = creditCardData_Web.cvc
            };
        }

        public CreditCardData buildCreditCardData(CreditCardData_Mobile creditCardData_mobile)
        {
            return new CreditCardData
            {
                cardUserKey = creditCardData_mobile.cardUserKey,
                cardToken = creditCardData_mobile.cardToken,
                creditCardId = creditCardData_mobile.creditCardId == Guid.Empty ? null : (Guid?)creditCardData_mobile.creditCardId,
                creditCardNumber = creditCardData_mobile.creditCardNumber,
                expireMonth = creditCardData_mobile.expireMonth,
                expireYear = creditCardData_mobile.expireYear,
                cardHolderName = creditCardData_mobile.cardHolderName,
                cvc = creditCardData_mobile.cvc
            };
        }
        public CreditCardData buildCreditCardData(CreditCardData_Broker creditCardData_Broker)
        {
            return new CreditCardData
            {
                cardUserKey = creditCardData_Broker.cardUserKey,
                cardToken = creditCardData_Broker.cardToken,
                creditCardId = creditCardData_Broker.creditCardId,
                creditCardNumber = creditCardData_Broker.creditCardNumber,
                expireMonth = creditCardData_Broker.expireMonth,
                expireYear = creditCardData_Broker.expireYear,
                cardHolderName = creditCardData_Broker.cardHolderName,
                cvc = creditCardData_Broker.cvc
            };
        }

        public CreateCreditCardParameters buildCreateCreditCardParameters(CreateCreditCardParameters_Mobile createCreditCardParameters)
        {
            return new CreateCreditCardParameters().Map(createCreditCardParameters);
        }

        public CreateCreditCardResponse_Mobile buildCreditCardResponse_Mobile(CreateCreditCardResponse createCreditCardResponse)
        {
            return new CreateCreditCardResponse_Mobile().Map(createCreditCardResponse);
        }
    }
}
