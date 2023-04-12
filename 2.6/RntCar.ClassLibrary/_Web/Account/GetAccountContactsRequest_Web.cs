using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Web
{
    public class GetAccountContactsRequest_Web : RequestBase
    {
        public Guid accountId { get; set; }
    }
}
