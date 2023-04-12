using Microsoft.Xrm.Sdk;
using RestSharp;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.ContractItem.Update
{
    public class PostUpdateContractItemMongoDB : IPlugin
    {
        //only triggers when rnt_mongodbintegrationtrigger fields change
        public void Execute(IServiceProvider serviceProvider)
        {

            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            initializer.TraceMe("plugin triggerd");

            Entity postImage;
            initializer.PluginContext.GetContextPostImages(initializer.PostImgKey, out postImage);
            if (postImage == null)
            {
                initializer.TraceMe("image is null");
            }
            Entity entity;
            initializer.PluginContext.GetContextInputEntity(initializer.TargetKey, out entity);
           
            var itemTypeCode = postImage.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value;
            if (itemTypeCode == (int)RntCar.ClassLibrary._Enums_1033.rnt_contractitem_rnt_itemtypecode.Equipment &&
                entity.Attributes.Contains("rnt_mongodbintegrationtrigger"))
            {
                initializer.TraceMe("rnt_mongodbintegrationtrigger is not null");
                ContractItemBL contractItemBL = new ContractItemBL(initializer.Service, initializer.TracingService);
                initializer.TraceMe("sending to mongodb");
                var response = initializer.RetryMethod<MongoDBResponse>(() => contractItemBL.updateContractItemInMongoDB(postImage), 3, 1000);
                if (!response.Result)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(response.ExceptionDetail);
                }

                contractItemBL.UpdateMongoDBUpdateRelatedFields(postImage);


                //var response = contractItemBL.updateContractItemInMongoDB(postImage);       
            }
            else
            {
                initializer.TraceMe("rnt_mongodbintegrationtrigger is null");
            }

            initializer.TraceMe("campaign control");
            if (entity.Contains("rnt_campaignid"))
            {
                initializer.TraceMe("update campaign process start");

                EntityReference campaignRef = entity.GetAttributeValue<EntityReference>("rnt_campaignid");
                string campaignId = campaignRef != null && campaignRef.Id != Guid.Empty ? Convert.ToString(campaignRef.Id) : string.Empty;

                ConfigurationBL configurationBL = new ConfigurationBL(initializer.Service, initializer.TracingService);
                var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
                initializer.TraceMe("responseUrl: " + responseUrl);

                RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "updatePriceCalculationSummariesCampaign", Method.POST);

                string trackingNumber = postImage.GetAttributeValue<string>("rnt_mongodbtrackingnumber");

                Guid contractId = postImage.GetAttributeValue<EntityReference>("rnt_contractid").Id;
                var contract = initializer.Service.Retrieve("rnt_contract", contractId, new Microsoft.Xrm.Sdk.Query.ColumnSet("rnt_pricinggroupcodeid"));

                EntityReference pricingGroupRef = contract.GetAttributeValue<EntityReference>("rnt_pricinggroupcodeid");
                string pricingGroupId = pricingGroupRef != null && pricingGroupRef.Id != Guid.Empty ? Convert.ToString(pricingGroupRef.Id) : string.Empty;


                restSharpHelper.AddJsonParameter<UpdatePriceCalculationSummariesRequest>(new UpdatePriceCalculationSummariesRequest
                {
                    groupCodeInformationId = pricingGroupId,
                    trackingNumber = trackingNumber,
                    campaignId = campaignId
                });

                initializer.TraceMe("executing updatePriceCalculationSummariesCampaign");
                var priceListResponse = restSharpHelper.Execute<MongoDBResponse>();
                initializer.TraceMe("executed updatePriceCalculationSummariesCampaign");

            }
        }
    }
}
