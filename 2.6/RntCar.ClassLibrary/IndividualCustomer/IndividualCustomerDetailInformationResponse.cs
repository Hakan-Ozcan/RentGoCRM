using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class IndividualCustomerDetailInformationResponse : ResponseBase
    {
        public IndividualCustomerDetailData IndividualCustomerDetailData { get; set; }
        public List<IndividualCustomerCorporateRelationData>  IndividualCustomerCorporateRelationData { get; set; }
    }
}
