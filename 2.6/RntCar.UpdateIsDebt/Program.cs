using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Repository;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.UpdateIsDebt
{
    class Program
    {
        static void Main(string[] args)
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            //get the completed contracts for last 30 days
            ContractRepository contractRepository = new ContractRepository(crmServiceHelper.IOrganizationService);
            
            var results = contractRepository.getCompletedDepthContractsByXlastDays(300, new string[] { "rnt_totalamount", "rnt_netpayment" });

            foreach (var item in results)
            {
                var totalAmount = item.GetAttributeValue<Money>("rnt_totalamount").Value;
                var netAmount = item.GetAttributeValue<Money>("rnt_netpayment").Value;
                Entity e = new Entity("rnt_contract");
                if (totalAmount > netAmount)
                {
                    e["rnt_debit"] = true;
                }
                else
                {
                    e["rnt_debit"] = false;
                }
                e.Id = item.Id;
                try
                {
                    
                    new CrmServiceHelper(item.GetAttributeValue<EntityReference>("ownerid").Id).IOrganizationService.Update(e);
                }
                catch(Exception ex)
                {
                    crmServiceHelper.IOrganizationService.Update(e);
                }
            }
        }
    }
}
