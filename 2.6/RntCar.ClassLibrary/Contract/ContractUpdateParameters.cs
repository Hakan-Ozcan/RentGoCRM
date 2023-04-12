using System;
using System.Collections.Generic;

namespace RntCar.ClassLibrary
{
    public class ContractUpdateParameters
    {
        public Guid contractId { get; set; }
        public Guid contactId { get; set; }
        public Guid groupCodeInformationId { get; set; }
        public Guid currency { get; set; }
        public int totalDuration { get; set; }
        public int customerType { get; set; }
        public int changedReason { get; set; }
        public int contractStatusCode { get; set; }
        public int contractItemStatusCode { get; set; }
        public bool isDateorBranchChanged { get; set; }
        public bool isCarChanged { get; set; }
        public string trackingNumber { get; set; }
        public bool canContinueMonthly { get; set; }
        public List<Guid> additionalDrivers { get; set; }
        public ContractDateandBranchParameters dateAndBranch { get; set; }
        public ContractUpdateAdditionalProductParameters additionalProduct { get; set; }
        public ContractUpdateEquipmentParameters contractEquipment { get; set; }
        public InvoiceAddressData addressData { get; set; }
        public ContractPriceParameters priceParameters { get; set; }
        public int channelCode { get; set; }
    }
}
