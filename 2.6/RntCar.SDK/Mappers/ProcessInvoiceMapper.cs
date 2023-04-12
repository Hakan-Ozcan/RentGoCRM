using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Mobile;
using RntCar.ClassLibrary._Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.SDK.Mappers
{
    public class ProcessInvoiceMapper
    {
        public InvoiceAddressCreateParameters buildInvoiceAddressCreateParameters(ProcessCustomerInvoiceAddressRequest_Web processCustomerInvoiceAddressRequest_Web)
        {
            return new InvoiceAddressCreateParameters
            {
                addressDetail = processCustomerInvoiceAddressRequest_Web.addressDetail,
                individualCustomerId = processCustomerInvoiceAddressRequest_Web.individualCustomerId,
                firstName = processCustomerInvoiceAddressRequest_Web.firstName,
                lastName = processCustomerInvoiceAddressRequest_Web.lastName,
                governmentId = processCustomerInvoiceAddressRequest_Web.governmentId,
                companyName = processCustomerInvoiceAddressRequest_Web.companyName,
                taxNumber = processCustomerInvoiceAddressRequest_Web.taxNumber,
                taxOfficeId = processCustomerInvoiceAddressRequest_Web.taxOfficeId,
                invoiceType = processCustomerInvoiceAddressRequest_Web.invoiceType,
                addressCountryId = processCustomerInvoiceAddressRequest_Web.addressCountryId,
                addressCityId = processCustomerInvoiceAddressRequest_Web.addressCityId,
                addressDistrictId = processCustomerInvoiceAddressRequest_Web.addressDistrictId,
                invoiceAddressId = processCustomerInvoiceAddressRequest_Web.invoiceAddressId,
                invoiceName = processCustomerInvoiceAddressRequest_Web.invoiceName
            };
        }
        public InvoiceAddressCreateParameters buildInvoiceAddressCreateParameters(ProcessCustomerInvoiceAddressRequest_Mobile processCustomerInvoiceAddressRequest_Mobile)
        {
            return new InvoiceAddressCreateParameters
            {
                addressDetail = processCustomerInvoiceAddressRequest_Mobile.addressDetail,
                individualCustomerId = processCustomerInvoiceAddressRequest_Mobile.individualCustomerId,
                firstName = processCustomerInvoiceAddressRequest_Mobile.firstName,
                lastName = processCustomerInvoiceAddressRequest_Mobile.lastName,
                governmentId = processCustomerInvoiceAddressRequest_Mobile.governmentId,
                companyName = processCustomerInvoiceAddressRequest_Mobile.companyName,
                taxNumber = processCustomerInvoiceAddressRequest_Mobile.taxNumber,
                taxOfficeId = processCustomerInvoiceAddressRequest_Mobile.taxOfficeId,
                invoiceType = processCustomerInvoiceAddressRequest_Mobile.invoiceType,
                addressCountryId = processCustomerInvoiceAddressRequest_Mobile.addressCountryId,
                addressCityId = processCustomerInvoiceAddressRequest_Mobile.addressCityId,
                addressDistrictId = processCustomerInvoiceAddressRequest_Mobile.addressDistrictId,
                invoiceAddressId = processCustomerInvoiceAddressRequest_Mobile.invoiceAddressId,
                invoiceName = processCustomerInvoiceAddressRequest_Mobile.invoiceName
            };
        }
    }
}
