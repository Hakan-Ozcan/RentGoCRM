using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RntCar.ClassLibrary.TransactionCurrency
{
    public class CurrencyRoot
    {
        [XmlElement("Currency")]
        public List<Currency> Currencies { get; set; }
    }
}
