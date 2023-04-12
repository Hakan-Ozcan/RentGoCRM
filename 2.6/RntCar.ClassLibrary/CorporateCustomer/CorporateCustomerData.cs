using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class CorporateCustomerData
    {
        public String companyName { get; set; }

        public String taxNumber { get; set; }

        public String taxOffice { get; set; }

        public String taxOfficeId { get; set; }

        public String telephone { get; set; }

        public Guid corporateCustomerId { get; set; }

    }
}
