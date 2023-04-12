using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ContractSearchData
    {
        public string contractNumber { get; set; }
        public string pnrNumber { get; set; }
        public string contactName { get; set; }
        public Guid contactId { get; set; }
        public string corporateName { get; set; }
        public Guid corporateId { get; set; }
        public Guid pickupBranchId { get; set; }
        public string pickupBranchName { get; set; }
        public DateTime pickupDateTime { get; set; }
        public Guid dropoffBranchId { get; set; }
        public string dropoffBranchName { get; set; }
        public DateTime dropoffDateTime { get; set; }
        public Guid groupCodeId { get; set; }
        public string groupCodeName { get; set; }
        public int paymentType { get; set; }
        public int status { get; set; }
        public string statusName { get; set; }
        public int state { get; set; }
        public decimal? totalAmount { get; set; }
        public Guid contractId { get; set; }
        public int contractType { get; set; }
        public string pricingType { get; set; }
        public int paymentMethod { get; set; }
        public decimal? corporateTotalAmount { get; set; }
        public string currencySymbol { get; set; }
        public decimal exchangeRate { get; set; }
    }
}
