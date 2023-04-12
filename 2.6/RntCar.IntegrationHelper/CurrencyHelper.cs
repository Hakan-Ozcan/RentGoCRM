using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.IntegrationHelper
{
    public class CurrencyHelper
    {
        public DailyCurrencyDataResponse getDailyCurrencies()
        {
            RestSharpHelper restSharpHelper = new RestSharpHelper("https://www.tcmb.gov.tr/", "kurlar/today.xml", RestSharp.Method.GET);
            var response = restSharpHelper.Execute();
            XmlSerializerHelper serializer = new XmlSerializerHelper();

            return serializer.Deserialize<DailyCurrencyDataResponse>(response.RawBytes, "RSP");
        }
    }
}
