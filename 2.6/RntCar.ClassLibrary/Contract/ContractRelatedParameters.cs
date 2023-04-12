using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ContractRelatedParameters
    {
        public int contractChannel { get; set; }
        public int contractTypeCode { get; set; }
        public Guid contractId { get; set; }
        public Guid reservationId { get; set; }
        public List<Guid> reservationItemList { get; set; }
        public string reservationPNR { get; set; }
        public int statusCode { get; set; }
        public string trackingNumber { get; set; }
    }
}
