using System;

namespace RntCar.ClassLibrary
{
    public class UserBasedBonusCalculationData
    {
        public DateTime QueryDate { get; set; }
        public int UserType { get; set; }
        public string UserName { get; set; }
        public string AdditionalProductName { get; set; }
        public string AdditionalProductId { get; set; }
        public decimal Amount { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string ContractId { get; set; }
        public string ContractNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
    }
    public class UserBasedBonusCalculationAdditionalProductData
    {
        public DateTime QueryDate { get; set; }        
        public string AdditionalProductName { get; set; }
        public string Branch { get; set; }
        public Guid AdditionalProductId { get; set; }
        public decimal Amount { get; set; }
        
        public Guid ContractId { get; set; }
        public string ContractNumber { get; set; }
        public string InvoiceDate { get; set; }

        public string UserName { get; set; }
        public Guid UserId { get; set; }

        public string TabletUserName { get; set; }
        public Guid TabletUserId { get; set; }

        public string ContractUserName { get; set; }
        public Guid ContractUserId { get; set; }

        public string ResUserName { get; set; }
        public Guid ResUserId { get; set; }
    }
}
