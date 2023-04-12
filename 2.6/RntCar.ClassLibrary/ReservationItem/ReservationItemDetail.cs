using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ReservationItemDetail
    {
        public SelectedAdditionalProductsInformation SelectedAdditionalProducts { get; set; }
        public SelectedIndividualCustomerInformation SelectedCustomer { get; set; }
        public SelectedDateAndBranchInformation SelectedDateAndBranch { get; set; }
    }
}
