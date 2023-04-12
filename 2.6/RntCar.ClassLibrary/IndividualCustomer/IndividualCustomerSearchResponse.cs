using System.Collections.Generic;

namespace RntCar.ClassLibrary
{
    public class IndividualCustomerSearchResponse : ResponseBase
    {        
        public List<IndividualCustomerData> IndividualCustomerData { get; set; }        
    }
}
