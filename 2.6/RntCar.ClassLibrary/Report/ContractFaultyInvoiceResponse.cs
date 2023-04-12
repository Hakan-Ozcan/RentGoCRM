using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ContractFaultyInvoiceResponse : ResponseBase
    {
        public List<FaultyContractDetail> contractDetail { get; set; }
    }
    public class FaultyContractDetail
    {
        public string contractId { get; set; }
        public string customerName { get; set; }
        public string contractPNR { get; set; }
        public decimal invoiceAmount { get; set; }
        public decimal totalAmount { get; set; }
        public DateTime dropoffDateTime { get; set; }
        public string dropoffDateTimeStr { get; set; }
    }
}
