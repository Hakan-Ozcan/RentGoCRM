using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary._Tablet;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class ServiceUserBL : BusinessHandler
    {
        public ServiceUserBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public ServiceUserBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public ServiceUserBL(IOrganizationService orgService, Guid userId) : base(orgService, userId)
        {
        }

        public ServiceUserBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public ServiceUserBL(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService) : base(initializer, orgService, tracingService)
        {
        }

        public LoginResponse checkServiceUser(LoginParameters loginParameters)
        {
            ServiceUserRepository serviceUserRepository = new ServiceUserRepository(this.OrgService,this.CrmServiceClient);
            var user =  serviceUserRepository.getServiceUserByUserNameandPasswordByGivenColumns(loginParameters.userName, loginParameters.password,new string[] { "rnt_name", "rnt_branchid", "rnt_contractearlyclosetime" });

            if(user == null)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                return new LoginResponse
                {
                    responseResult = ResponseResult.ReturnError( xrmHelper.GetXmlTagContentByGivenLangId("WrongUser",
                                                                             loginParameters.langId,
                                                                             this.tabletXmlPath))
                };             
            }

            return new LoginResponse
            {
                fullname = user.GetAttributeValue<string>("rnt_name"),
                userId = user.Id,
                earlyCloseTime = user.GetAttributeValue<int>("rnt_contractearlyclosetime"),
                userBranch = new Branch
                {
                    branchId = user.GetAttributeValue<EntityReference>("rnt_branchid").Id,
                    branchName = user.GetAttributeValue<EntityReference>("rnt_branchid").Name,
                },
                responseResult = ResponseResult.ReturnSuccess(),
            };
        }
    }
}
