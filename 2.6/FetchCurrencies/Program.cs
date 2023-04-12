using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.MongoDBHelper.Entities;
using RntCar.SDK.Common;
using System;

namespace RntCar.FetchCurrencies
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string XML_URL = "https://www.tcmb.gov.tr/kurlar/today.xml";

                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                TransactionCurrencyRepository transactionCurrencyRepository = new TransactionCurrencyRepository(crmServiceHelper.IOrganizationService);
                TransactionCurrencyBL transactionCurrencyBL = new TransactionCurrencyBL(crmServiceHelper.IOrganizationService);

                var currencies = transactionCurrencyRepository.GetAllCurrencies();
                transactionCurrencyBL.UpdateCurrencies(currencies, XML_URL);

                var mongoDBHostName = StaticHelper.GetConfiguration("MongoDBHostName");
                var mongoDBDatabaseName = StaticHelper.GetConfiguration("MongoDBDatabaseName");

                currencies = transactionCurrencyRepository.GetAllCurrencies();
                CurrencyBusiness currencyBusiness = new CurrencyBusiness(mongoDBHostName, mongoDBDatabaseName);
                foreach (var item in currencies)
                {
                    currencyBusiness.upsertCurrency(new MongoDBHelper.Model.CurrencyDataMongoDB
                    {                        
                        exchangerate = item.GetAttributeValue<decimal>("exchangerate")
                    },
                    item.Id);
                }
               
            }
            catch(Exception ex)
            {

                
            }
        }
    }
}
