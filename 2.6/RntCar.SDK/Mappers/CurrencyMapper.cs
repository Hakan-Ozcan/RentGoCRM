using Microsoft.Xrm.Sdk;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.SDK.Mappers
{
    public class CurrencyMapper
    {
        public List<CurrencyData> createCurrencyData(List<Entity> currencies)
        {
            List<CurrencyData> currencyData = new List<CurrencyData>();

            currencies.ForEach(currency =>
            {
                currencyData.Add(new CurrencyData
                {
                    CurrencyName = currency.GetAttributeValue<string>("currencyname"),
                    CurrencyPrecision = currency.GetAttributeValue<int>("currencyprecision"),
                    CurrencySymbol = currency.GetAttributeValue<string>("currencysymbol"),
                    ExchangeRate = currency.GetAttributeValue<decimal>("exchangerate"),
                    IsoCurrencyCode = currency.GetAttributeValue<string>("isocurrencycode"),
                    TransactionCurrencyId = currency.GetAttributeValue<Guid>("transactioncurrencyid")
                });
            });

            return currencyData;
        }
    }
}
