using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Invoice
{
    public class PostSendEMailWhenInvoiceInformationsChange : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            try
            {
                Entity preImage;
                initializer.PluginContext.GetContextPreImages<Entity>(initializer.PreImgKey, out preImage);

                Entity entity;
                initializer.PluginContext.GetContextInputEntity(initializer.TargetKey, out entity);

                bool sendEmail = false;

                initializer.TraceMe("preImage: " + JsonConvert.SerializeObject(preImage));
                initializer.TraceMe("postImage: " + JsonConvert.SerializeObject(entity));

                if (entity != null && preImage != null)
                {
                    if (entity.Attributes.Contains("rnt_taxnumber") &&
                        !string.IsNullOrEmpty(entity.GetAttributeValue<string>("rnt_taxnumber")))
                    {
                        initializer.TraceMe("contains taxnumber");
                        initializer.TraceMe("entity" + entity.GetAttributeValue<string>("rnt_taxnumber"));
                        initializer.TraceMe("preimage" + preImage.GetAttributeValue<string>("rnt_taxnumber"));

                        if (entity.GetAttributeValue<string>("rnt_taxnumber") != preImage.GetAttributeValue<string>("rnt_taxnumber"))
                        {
                            sendEmail = true;
                        }
                    }

                    if (entity.Attributes.Contains("rnt_govermentid") &&
                        !string.IsNullOrEmpty(entity.GetAttributeValue<string>("rnt_govermentid")))
                    {
                        initializer.TraceMe("contains rnt_govermentid");
                        initializer.TraceMe("entity"  + entity.GetAttributeValue<string>("rnt_govermentid"));
                        initializer.TraceMe("preimage" + preImage.GetAttributeValue<string>("rnt_govermentid"));
                        
                        if (entity.GetAttributeValue<string>("rnt_govermentid") != preImage.GetAttributeValue<string>("rnt_govermentid"))
                        {
                            sendEmail = true;
                        }
                    }
                }

                initializer.TraceMe("sendEmail: " + sendEmail);

                if (sendEmail)
                {
                    ExecuteWorkflowRequest request = new ExecuteWorkflowRequest()
                    {
                        EntityId = entity.Id,
                        WorkflowId = StaticHelper.sendEMailWhenInvoiceInformationsChangeWorkflowId
                    };
                    ExecuteWorkflowResponse response = (ExecuteWorkflowResponse)initializer.Service.Execute(request);
                }
            }
            catch (Exception ex)
            {
                initializer.TraceMe(ex.Message);
            }
        }
    }
}
