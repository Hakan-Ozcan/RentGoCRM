using Microsoft.Xrm.Sdk;
using RntCar.ClassLibrary._Tablet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class UpdateContractandContractItemWithNewGroupCodeParamaters
    {
        public string contractItemName { get; set; }
        public Guid contractId { get; set; }
        public int changeType { get; set; }
        public decimal totalPrice { get; set; }
        public string trackingNumber { get; set; }
        public Guid groupCodeInformationId { get; set; }
        public long pickupTimeStamp { get; set; }
        public long dropoffTimeStamp { get; set; }
        public long manualPickupTimeStamp { get; set; }
        public bool isManualProcess { get; set; }
        public Guid equipmentId { get; set; }
        public EntityReference pricingGroupCode { get; set; }
        public List<AdditonalProductDataTablet> additionalProducts { get; set; }
        public UserInformation userInformation { get; set; }
    }
}
