using System;

namespace RntCar.ClassLibrary
{
    public class CustomerInvoices
    {
        public string invoiceNumber { get; set; }
        public string logoInvoiceNumber { get; set; }
        public string contractNumber { get; set; }
        public decimal totalAmount { get; set; }
        public int invoiceType { get; set; }
        public string invoiceTypeName { get; set; }
        public DateTime invoiceDate { get; set; }
        public string pnrNumber { get; set; }

    }
}
