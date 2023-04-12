using System;

namespace RntCar.ClassLibrary._Tablet
{
    public class CalculateContractRemainingAmountParameters : RequestBase
    {
        public Branch dropoffBranch { get; set; }
        public ContractInformation contractInformation { get; set; }
        public ContractEquipmentInformation equipmentInformation { get; set; }
  
    }
}
