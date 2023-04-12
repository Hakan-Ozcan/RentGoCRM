using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;

namespace RntCar.BusinessLibrary.Business
{
    public class AdditionalDriversBL : BusinessHandler
    {
        public AdditionalDriversBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public AdditionalDriversBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public AdditionalDriversBL(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService) : base(initializer, orgService, tracingService)
        {
        }

        public void createAdditionalDrivers(List<Guid> additionalDriversList, Guid contractId)
        {
            foreach (var item in additionalDriversList)
            {
                AdditionalDriverRepository additionalDriverRepository = new AdditionalDriverRepository(this.OrgService);
                var additionalDriver = additionalDriverRepository.getActiveAdditionalDriverByContactIdandContractId(item, contractId);

                if(additionalDriver == null)
                {
                    Entity e = new Entity("rnt_additionaldriver");
                    e.Attributes["rnt_contractid"] = new EntityReference("rnt_contract", contractId);
                    e.Attributes["rnt_contactid"] = new EntityReference("contact", item);
                    this.OrgService.Create(e);
                }               
            }
        }
        public AdditionalDriverDeactivateResponse deactivateAdditionalDriverByContractandContactId(string contactId, string contractId)
        {
            AdditionalDriverRepository additionalDriverRepository = new AdditionalDriverRepository(this.OrgService);
            var additionalDriver = additionalDriverRepository.getActiveAdditionalDriverByContactIdandContractId(Guid.Parse(contactId), Guid.Parse(contractId));
            // no active records found
            if (additionalDriver != null)
            {
                this.Trace("additional driver entity found");
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                // deactivate additional driver 
                this.Trace("set state for deactivation start");
                xrmHelper.setState(additionalDriver.LogicalName, additionalDriver.Id, (int)GlobalEnums.StateCode.Passive, 2);
                this.Trace("set state for deactivation end");
                return new AdditionalDriverDeactivateResponse
                {
                    additionalDriverId = additionalDriver.Id,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
            //No active records found
            return new AdditionalDriverDeactivateResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }
    }
}
