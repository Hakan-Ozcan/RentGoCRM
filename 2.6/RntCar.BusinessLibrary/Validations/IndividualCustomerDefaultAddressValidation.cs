using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Validations
{
    public class IndividualCustomerDefaultAddressValidation : ValidationHandler
    {
        public IndividualCustomerDefaultAddressValidation(IOrganizationService orgService) : base(orgService)
        {
        }

        public IndividualCustomerDefaultAddressValidation(IOrganizationService orgService, Guid userId) : base(orgService, userId)
        {
        }

        public IndividualCustomerDefaultAddressValidation(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public bool CheckCustomerDefaultAddress(Guid customerId)
        {
            var validationResult = true;
            IndividualAddressRepository repository = new IndividualAddressRepository(this.OrgService);
            try
            {
                var result = repository.getDefaultIndividualAddressByCustomerId(customerId);
                if (result != null)
                    validationResult = false;
            }
            catch (Exception ex)
            {
                validationResult =  false;
                       
            }
            return validationResult;
        }
    }
}
