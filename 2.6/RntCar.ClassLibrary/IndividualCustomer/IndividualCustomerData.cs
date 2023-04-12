using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class IndividualCustomerData
    {
        public Guid IndividualCustomerId { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String GovernmentId { get; set; }
        public String Email { get; set; }
        public String MobilePhone { get; set; }
    }
}
