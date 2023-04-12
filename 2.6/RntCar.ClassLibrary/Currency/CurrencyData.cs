using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class CurrencyData
    {
        public Guid TransactionCurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencySymbol { get; set; }
        public string IsoCurrencyCode { get; set; }
        public decimal ExchangeRate { get; set; }
        public int CurrencyPrecision { get; set; }
    }
}
