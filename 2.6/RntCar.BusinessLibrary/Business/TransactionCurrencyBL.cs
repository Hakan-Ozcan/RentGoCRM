using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary.TransactionCurrency;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace RntCar.BusinessLibrary.Business
{
    public class TransactionCurrencyBL : BusinessHandler
    {
        public TransactionCurrencyBL(IOrganizationService orgService) : base(orgService)
        {
        }
        public void UpdateCurrencies(List<Entity> currencies, string XML_URL)
        {
            var tcmbCurrencies = GetTCMBCurrencies(XML_URL);//Get currencies by the Central Bank of Turkey

            foreach (var oldCurrency in currencies)
            {
                foreach (var newCurrency in tcmbCurrencies.Currencies)
                {
                    if (oldCurrency.GetAttributeValue<string>("isocurrencycode") == newCurrency.CurrencyCode)
                    {
                        Entity e = new Entity("transactioncurrency");
                        CultureInfo cultureInfo = new CultureInfo("En");
                        e["exchangerate"] = (1 / Double.Parse(newCurrency.ForexBuying, cultureInfo)); // 1 TRY -> USD 
                        e.Id = oldCurrency.Id;
                        this.OrgService.Update(e);
                        break;
                    }
                }
            }

        }

        public CurrencyRoot GetTCMBCurrencies(string XML_URL)
        {
            RestSharpHelper restSharpHelper = new RestSharpHelper(XML_URL, "", RestSharp.Method.POST);
            var response = restSharpHelper.Execute();

            XmlSerializerHelper serializer = new XmlSerializerHelper();
            var xmlSeraializer = serializer.Deserialize<CurrencyRoot>(response.RawBytes, "Tarih_Date");

            return xmlSeraializer;
        }
    }
}
