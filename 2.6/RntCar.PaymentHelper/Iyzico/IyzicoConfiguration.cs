using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.PaymentHelper.iyzico
{
    public class IyzicoConfiguration
    {
        public string iyzico_apiKey { get; set; }
        public string iyzico_secretKey { get; set; }
        public string iyzico_baseUrl { get; set; }

        public static IyzicoConfiguration getIyzicoConfiguration()
        {
            return new IyzicoConfiguration
            {
                iyzico_apiKey = StaticHelper.GetConfiguration("iyzico_apiKey"),
                iyzico_secretKey = StaticHelper.GetConfiguration("iyzico_secretKey"),
                iyzico_baseUrl = StaticHelper.GetConfiguration("iyzico_baseUrl"),
            };
        }
    }
}
