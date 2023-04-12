using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services.Protocols;

namespace RntCar.BrokerServicesWebXml
{
    public class AuthHeader : SoapHeader
    {
        public string UserName;
        public string Password;
    }
}