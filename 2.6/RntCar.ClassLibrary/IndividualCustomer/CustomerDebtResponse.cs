using System;

namespace RntCar.ClassLibrary
{
    public class CustomerDebtResponse
    {
        public decimal debtAmount { get; set; }
        public Guid customerId { get; set; }
    }
}
