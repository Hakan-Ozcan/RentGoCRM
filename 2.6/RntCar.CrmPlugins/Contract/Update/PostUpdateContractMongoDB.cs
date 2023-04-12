using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RestSharp;
using RntCar.BusinessLibrary;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.Contract.Update
{
    public class PostUpdateContractMongoDB : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {

            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            initializer.TraceMe("plugin triggerd");

            Entity preImage;
            initializer.PluginContext.GetContextPreImages(initializer.PreImgKey, out preImage);
            if (preImage == null)
            {
                initializer.TraceMe("image is null");
            }

            Entity entity;
            initializer.PluginContext.GetContextInputEntity(initializer.TargetKey, out entity);
            if (entity.Contains("rnt_pricinggroupcodeid"))
            {
                EntityReference prePricingGroupRef = preImage.GetAttributeValue<EntityReference>("rnt_pricinggroupcodeid");

                EntityReference pricingGroupRef = entity.GetAttributeValue<EntityReference>("rnt_pricinggroupcodeid");

                EntityReference equipmentRef = entity.GetAttributeValue<EntityReference>("rnt_equipmentid");


                QueryExpression getContractItem = new QueryExpression("rnt_contractitem");
                getContractItem.ColumnSet = new ColumnSet("rnt_contractitemid", "rnt_mongodbtrackingnumber");
                getContractItem.Criteria.AddCondition("rnt_equipment", ConditionOperator.Equal, equipmentRef.Id);
                getContractItem.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, 1);
                getContractItem.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, entity.Id);
                var contractItem = initializer.Service.RetrieveMultiple(getContractItem).Entities.FirstOrDefault();

                var trackingNumber = contractItem.GetAttributeValue<string>("rnt_mongodbtrackingnumber");

                ConfigurationBL configurationBL = new ConfigurationBL(initializer.Service, initializer.TracingService);
                var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
                initializer.TraceMe("responseUrl: " + responseUrl);

                RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "updatePriceCalculationSummariesPricingGroup", Method.POST);

                restSharpHelper.AddJsonParameter<UpdatePriceCalculationSummariesRequest>(new UpdatePriceCalculationSummariesRequest
                {
                    groupCodeInformationId = Convert.ToString(pricingGroupRef.Id),
                    trackingNumber = trackingNumber,
                    preGroupCodeInformationId = Convert.ToString(prePricingGroupRef.Id),
                    groupCodeInformationName = prePricingGroupRef.Name
                });

                initializer.TraceMe("executing updatePriceCalculationSummariesCampaign");
                var priceListResponse = restSharpHelper.Execute<MongoDBResponse>();
                initializer.TraceMe("executed updatePriceCalculationSummariesCampaign");


            }
            if (entity.Contains("rnt_isclosedamountzero"))
            {
                QueryExpression getContractItem = new QueryExpression("rnt_contractitem");
                getContractItem.ColumnSet = new ColumnSet(true);
                getContractItem.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, 1);
                getContractItem.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, entity.Id);
                var contractItemList = initializer.Service.RetrieveMultiple(getContractItem);

                foreach (var contractItem in contractItemList.Entities)
                {
                    ContractItemBL contractItemBL = new ContractItemBL(initializer.Service, initializer.TracingService);
                    var response = contractItemBL.updateContractItemInMongoDB(contractItem);

                }
            }
        }
    }
}