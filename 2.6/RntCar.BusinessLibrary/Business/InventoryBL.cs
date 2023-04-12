using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Tablet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class InventoryBL : BusinessHandler
    {
        public InventoryBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }
       
    }
}
