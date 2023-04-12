using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.Reservation
{
    public class ReservationCustomerParameters
    {
        public Guid contactId { get; set; }
        public int customerType { get; set; }
        public Guid? corporateCustomerId { get; set; }
        public InvoiceAddressData invoiceAddress { get; set; }
        public string dummyContactId { get; set; }
    }
}
