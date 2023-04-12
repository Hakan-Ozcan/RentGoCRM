using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MarkettingPermission;
using RntCar.SDK.Common;
using RntCar.SDK.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.MarkettingPermission.Actions
{
    public class UpdateMarketingPermission : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            string _contactId;
            initializer.PluginContext.GetContextParameter<string>("contactId", out _contactId);
            bool smsPermission;
            initializer.PluginContext.GetContextParameter<bool>("smsPermission", out smsPermission);
            bool emailPermission;
            initializer.PluginContext.GetContextParameter<bool>("emailPermission", out emailPermission);
            bool notificationPermission;
            initializer.PluginContext.GetContextParameter<bool>("notificationPermission", out notificationPermission);
            int channelCode;
            initializer.PluginContext.GetContextParameter<int>("channelCode", out channelCode);
            int operationType;
            initializer.PluginContext.GetContextParameter<int>("operationType", out operationType);

            initializer.TraceMe("contactId" + _contactId);
            initializer.TraceMe("smsPermission" + smsPermission);
            initializer.TraceMe("notificationPermission" + notificationPermission);
            initializer.TraceMe("emailPermission" + emailPermission);
            initializer.TraceMe("channelCode" + channelCode);
            initializer.TraceMe("operationType" + operationType);

            try
            {
                var contactId = new Guid(_contactId);
                initializer.TraceMe("contactId " + contactId.ToString() + "    - gtype" + contactId.GetType());
                var marketinPermissionId = Guid.Empty;
                var response = new UpdatingMarketingPermissionResponse();

                initializer.TraceMe("Start update permission");
                MarkettingPermissionsRepository markettingPermissionsRepository = new MarkettingPermissionsRepository(initializer.Service);
                var markettingPermission = markettingPermissionsRepository.getMarkettingPermissionByContactId(contactId);
                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(initializer.Service);
                var contact = individualCustomerRepository.getIndividualCustomerById(contactId);

                initializer.TraceMe("start mapping");
                var marketingPermissionData = new MarketingPermissionMapper().createMarketingPermissionData(markettingPermission);

                initializer.TraceMe("marketingPermissionData: " + JsonConvert.SerializeObject(marketingPermissionData));

                marketingPermissionData.notificationPermission = notificationPermission;
                marketingPermissionData.smsPermission = smsPermission;
                marketingPermissionData.emailPermission = emailPermission;
                marketingPermissionData.channelCode = channelCode;
                marketingPermissionData.marketingPermissionId = null;
                marketingPermissionData.operationType = operationType;

                MarkettingPermissionsBL markettingPermissionsBL = new MarkettingPermissionsBL(initializer.Service);
                if (markettingPermission != null)
                {
                    initializer.TraceMe("start deactive");
                    markettingPermissionsBL.deactiveMarkettingPermissions(markettingPermission.Id);
                    initializer.TraceMe("end deactive");
                    
                }
                else
                {
                    marketingPermissionData.contactId = contactId;
                }

                initializer.TraceMe("start create");
                marketinPermissionId = markettingPermissionsBL.createMarkettingPermissions(marketingPermissionData, contact);
                initializer.TraceMe("end create");
                initializer.TraceMe("marketinPermissionId" + marketinPermissionId);

                if (marketinPermissionId == null)
                {
                    response.ResponseResult = ResponseResult.ReturnError("Marketing Permission is null!");
                    initializer.PluginContext.OutputParameters["UpdatingMarketingPermissionResponse"] = JsonConvert.SerializeObject(response);
                    return;
                }
                response.marketingPermissionId = marketinPermissionId;
                response.ResponseResult = ResponseResult.ReturnSuccess();

                initializer.PluginContext.OutputParameters["UpdatingMarketingPermissionResponse"] = JsonConvert.SerializeObject(response);

            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
