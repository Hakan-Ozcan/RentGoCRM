using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ContractEnums
    {
        public enum CancellationReason
        {
            ByRentgo = 100000003,
            ByCustomer = 100000004
        }
        public enum StatusCode
        {          
            WaitingforDelivery = 1,
            Rental = 100000000,
            Completed = 100000001,
            EmniyetSuistimal = 100000006
        }
        
    }
}
