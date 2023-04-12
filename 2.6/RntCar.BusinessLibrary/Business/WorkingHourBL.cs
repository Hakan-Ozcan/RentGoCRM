using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class WorkingHourBL:BusinessHandler
    {
        public WorkingHourBL(IOrganizationService orgService) : base(orgService)
        {
        }
        public WorkingHourBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }
        public WorkingHourBL(GlobalEnums.ConnectionType enums = GlobalEnums.ConnectionType.default_Service, string UserId = "") : base(enums, UserId)
        {
        }

        public WorkingHourData getWorkingHourByBranchIdAndSelectedDay(Guid branchId, int dayCode)
        {
            WorkingHourRepository repository = new WorkingHourRepository(this.OrgService);
            return repository.GetWorkingHourByBranchIdAndSelectedDay(branchId, dayCode);
        }

        public List<Entity> getWorkingHourByBranchId(Guid branchId)
        {
            WorkingHourRepository repository = new WorkingHourRepository(this.OrgService);
            return repository.GetWorkingHourByBranchId(branchId);
        }
    }
}
