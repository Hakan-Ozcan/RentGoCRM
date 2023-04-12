using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class IndividualCustomerCorporateRelationData
    {
        public string companyName { get; set; }       

        public Guid corporateCustomerId { get; set; }

        public int relationType { get; set; }

        public string roleType { get; set; }

        public string individualCustomerName { get; set; }

        public Guid individualCustomerId { get; set; }

    }
}
