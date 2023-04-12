using System;

namespace RntCar.ClassLibrary
{
    public class CalculatePricesForUpdateContractParameters
    {
        public Guid contractId { get; set; }
        public DateTime dropoffDateTime { get; set; }
        public Guid dropoffBranchId { get; set; } 
        public int langId { get; set; }
        public bool isMonthly { get; set; } = false;                    
        public bool passValidation { get; set; } = false;
    }
}
