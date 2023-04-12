using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.AbuseOfConfidence
{
    class Program
    {
        static void Main(string[] args)
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            ContractRepository contractRepository = new ContractRepository(crmServiceHelper.IOrganizationService);

            try
            {
                var contracts = contractRepository.getAllRentalContracts(new string[] { "rnt_dropoffdatetime", "statuscode" });
                var abusedContracts = contracts.Where(p => p.GetAttributeValue<DateTime>("rnt_dropoffdatetime").AddDays(3) <= DateTime.Now).ToList();

                abusedContracts.ForEach(contract =>
                {
                    Entity e = new Entity("rnt_contract");
                    e.Id = contract.Id;
                    e["statuscode"] = new OptionSetValue((int)ContractEnums.StatusCode.EmniyetSuistimal);

                    crmServiceHelper.IOrganizationService.Update(e);
                });
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
