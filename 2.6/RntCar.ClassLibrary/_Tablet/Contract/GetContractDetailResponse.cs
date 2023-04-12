using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class GetContractDetailResponse : ResponseBase
    {
        public int contractType { get; set; }
        public int paymentMethod { get; set; }
        public string contractNumber { get; set; }
        public string pnrNumber { get; set; }
        public string contractId { get; set; }
        public long pickupTimestamp { get; set; }
        public long dropoffTimestamp { get; set; }
        public int statusCode { get; set; }
        public string statusName { get; set; }
        public Guid? campaignId { get; set; }
        public Branch pickupBranch { get; set; }
        public Branch dropoffBranch { get; set; }
        public Customer customer { get; set; }
        public GroupCodeInformation groupCodeInformation { get; set; }
        public EquipmentData selectedEquipment { get; set; }
        public List<CreditCardData> creditCards { get; set; }
        public decimal totalPrice { get; set; }
        public bool isEquipmentChanged { get; set; }

    }
}
