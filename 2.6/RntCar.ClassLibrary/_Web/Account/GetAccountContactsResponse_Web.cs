using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Web
{
    public class GetAccountContactsResponse_Web : ResponseBase
    {
        public List<AccountContactsData_Web> accountContactsData { get; set; }
    }
}
