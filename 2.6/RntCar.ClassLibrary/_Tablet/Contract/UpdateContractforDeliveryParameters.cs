using RntCar.ClassLibrary.PaymentPlan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class UpdateContractforDeliveryParameters : RequestBase
    {
        public ContractEquipmentInformation equipmentInformation { get; set; }
        public UserInformation userInformation { get; set; }
        public ContractManualDateInformation contractInformation { get; set; }
        public List<AdditonalProductDataTablet> additionalProducts { get; set; }
        public PaymentInformation paymentInformation { get; set; }
        public AvailabilityData changedEquipmentData { get; set; }
        public List<DamageData> damageData { get; set; }
        public OtherDocuments otherDocuments { get; set; }
        public long dateNowTimeStamp { get; set; }
        public Guid? campaignId { get; set; }
        public List<PaymentPlanData> paymentPlans { get; set; }

    }
}
