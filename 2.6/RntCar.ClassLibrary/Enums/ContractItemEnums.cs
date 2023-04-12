using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ContractItemEnums
    {
        public enum CancellationReason
        {
            ByRentgo = 100000008,
            ByCustomer = 100000009
        }
        public enum StatusCode
        {
            New = 1,
            NoShow = 100000000,
            Rental = 100000001,
            WaitingforDelivery = 100000002,
            Completed = 100000003,
            CustomerDemand = 100000007
        }
        public enum ItemTypeCode
        {
            Equipment = 1,
            AdditionalProduct = 2,
            Fine = 3
        };
        public enum ChangeReason
        {
            Damage = 100000000,
            CustomerDemand = 20
        };
    }
}
