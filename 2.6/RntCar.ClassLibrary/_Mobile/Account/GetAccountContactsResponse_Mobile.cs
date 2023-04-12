using RntCar.ClassLibrary._Mobile.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Mobile
{
    public class GetAccountContactsResponse_Mobile : ResponseBase
    {
        public List<AccountContactsData_Mobile> accountContactsData { get; set; }
    }
}
