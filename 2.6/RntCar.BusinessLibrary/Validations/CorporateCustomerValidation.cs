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
    public class CorporateCustomerValidation : ValidationHandler
    {
        public CorporateCustomerValidation(IOrganizationService orgService) : base(orgService)
        {
        }

        public CorporateCustomerValidation(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }
        public bool checkDuplicateCorporateCustomer(int customerTypeCode, string taxno)
        {
            CorporateCustomerRepository repository = new CorporateCustomerRepository(this.OrgService);

            var collection = repository.getCorporateCustomersByTaxNumberandCustomerType(taxno, customerTypeCode);// get duplicate cutomer by type and tax no

            if (collection.Entities.Count > 0)
                return false;

            return true;
        }
        public bool checkDuplicateCorporateCustomerForUpdate(int customerTypeCode, string taxno, Guid accountid)
        {
            //check duplicate customer for update message
            CorporateCustomerRepository repository = new CorporateCustomerRepository(this.OrgService);
            
            var collection = repository.getCorporateCustomersByTaxNumberandCustomerTypeForUpdate(taxno, customerTypeCode, accountid);// get duplicate cutomer by type and tax no

            if (collection.Entities.Count > 0) 
                return false;

            return true;
        }
        public bool isTaxnoValid(string taxno)
        {
            try
            {
                if (taxno.Length == 10)
                {
                    var x = new int[9];
                    var y = new int[9];

                    for (int i = 0; i < 9; i++)
                    {
                        x[i] = (int.Parse(taxno[i].ToString()) + 9 - i) % 10;

                        y[i] = (x[i] * (int)Math.Pow(2, 9 - i)) % 9;

                        if (x[i] != 0 && y[i] == 0)
                        {
                            y[i] = 9;
                        }
                    }

                    return ((10 - (y.Sum() % 10)) % 10) == int.Parse(taxno[9].ToString());
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
