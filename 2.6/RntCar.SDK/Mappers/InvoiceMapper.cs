using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Web;
using RntCar.SDK.Common;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.SDK.Mappers
{
    public class InvoiceMapper
    {
        public List<CustomerInvoices_Web> builWebInvoiceData(List<CustomerInvoices> invoices)
        {
            return invoices.ConvertAll(p => new CustomerInvoices_Web().Map(p)).ToList();
        }
    }
}
