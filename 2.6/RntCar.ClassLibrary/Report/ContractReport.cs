using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ContractReport
    {
        public int count { get; set; }
        public List<ContractDetail> contractDetail { get; set; }
        public DateTime currentDateTime { get; set; }
    }
    public class ContractDetail
    {
        public string contractId { get; set; }
        public string customerName { get; set; }
        public string contractNumber { get; set; }
        public string contractPNR { get; set; }
        public string plateNo { get; set; }
        public DateTime dropoffDateTime { get; set; }
        public string groupCodeInformationId { get; set; }
        public string groupCodeInformationName { get; set; }
        public int statusCode { get; set; }
    }
}
