using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class IndividualCustomerAnonymizationResponse : ResponseBase
    {
        public Guid individualCustomerId { get; set; }
        public DateTime? expireRequestDate { get; set; }
        public DateTime? expireDate { get; set; }
        public string anonymizationMessage { get; set; }
        public int LangId { get; set; }
    }
}
