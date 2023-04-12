using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class DashboardCustomer
    {
        public Guid customerId { get; set; }
        public string fullName { get; set; }
        public int segment { get; set; }
        public string mobilePhone { get; set; }
        public string email { get; set; }
        public Guid corporateCustomerId { get; set; }
        public int customerType { get; set; }
    }
}
