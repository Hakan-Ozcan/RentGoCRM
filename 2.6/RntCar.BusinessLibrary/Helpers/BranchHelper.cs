using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary.Branch;
using System;

namespace RntCar.BusinessLibrary.Helpers
{
    public class BranchHelper : HelperHandler
    {
        public BranchHelper(IOrganizationService organizationService) : base(organizationService)
        {
        }

        public BranchInfo getBranchType(Guid? reservationId,
                                         Guid? contractId)
        {
            Guid branchId = Guid.Empty;

            if (reservationId.HasValue)
            {
                ReservationRepository reservationRepository = new ReservationRepository(this.IOrganizationService);
                var res = reservationRepository.getReservationById(reservationId.Value, new string[] { "rnt_pickupbranchid" });
                branchId = res.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id;

            }
            else if (contractId.HasValue)
            {
                ContractRepository contractRepository = new ContractRepository(this.IOrganizationService);
                var contract = contractRepository.getContractById(contractId.Value, new string[] { "rnt_pickupbranchid" });
                branchId = contract.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id;
            }

            BranchRepository branchRepository = new BranchRepository(this.IOrganizationService);
            var branch = branchRepository.getBranchById(branchId, new string[] { "rnt_branchtype", "rnt_logoaccountnumber" });

            return new BranchInfo
            {
                branchType = branch.GetAttributeValue<OptionSetValue>("rnt_branchtype").Value,
                logoAccountNumber = branch.GetAttributeValue<string>("rnt_logoaccountnumber")
            };
        }
    }
}
