using System;

namespace RntCar.ClassLibrary
{
    public class CreateContractInvoiceDateParameters
    {
        public Guid contractId { get; set; }
        public decimal amount { get; set; }
        public DateTime invoiceDate { get; set; }
        public DateTime pickupDatime { get; set; }
        public DateTime dropoffDateTime { get; set; }
        public int type { get; set; }
        public string templates { get; set; }
    }

    public class InvoiceItemTemplate
    {
        public int itemType { get; set; }
        public Guid equipmentId { get; set; }
        public Guid additionalProductId { get; set; }
        public Guid contractItemId { get; set; }
        public decimal amount { get; set; }
    }
}
