using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class UpdateContractforRentalParameters : RequestBase
    {
        public ContractEquipmentInformation equipmentInformation { get; set; }
        public UserInformation userInformation { get; set; }
        public ContractManualDateInformation contractInformation { get; set; }
        public List<AdditonalProductDataTablet> additionalProducts { get; set; }
        public List<AdditonalProductDataTablet> otherAdditionalProducts { get; set; }
        public PaymentInformation paymentInformation { get; set; }
        public List<DamageData> damageData { get; set; }
        public OtherDocuments otherDocuments { get; set; }
        public List<HGSTransitData> transits { get; set; }
        public List<EIhlalFineData> fineList { get; set; }
        public string trackingNumber { get; set; }
        public int? operationType { get; set; }
        public bool canUserStillHasCampaignBenefit { get; set; }
        public Guid? campaignId { get; set; }
        public decimal totalAmount { get; set; }
        public int? contractDeptStatus { get; set; }

        public long dateNow;
    }
}
