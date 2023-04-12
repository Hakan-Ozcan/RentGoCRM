using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using System.Collections.Generic;

namespace RntCar.BusinessLibrary.Business
{
    public class BranchBL : BusinessHandler
    {

        public BranchBL(GlobalEnums.ConnectionType enums = GlobalEnums.ConnectionType.default_Service) : base(enums)
        {
        }
        public BranchBL(IOrganizationService orgService) : base(orgService)
        {
        }
     
        public List<BranchData> getActiveBranchs()
        {
            List<BranchData> data = new List<BranchData>();
            BranchRepository repository = new BranchRepository(this.OrgService);
            data = repository.getActiveBranchs();
            return data;
        }
    }
}
