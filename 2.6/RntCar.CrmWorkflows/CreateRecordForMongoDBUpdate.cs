using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using RntCar.BusinessLibrary.Repository;
using RntCar.SDK.Common;
using System.Activities;

namespace RntCar.CrmWorkflows
{
    public class CreateRecordForMongoDBUpdate : CodeActivity
    {
        [Input("Group Code Information")]
        [ReferenceTarget("rnt_groupcodeinformations")]
        public InArgument<EntityReference> _groupCodeInformation { get; set; }
        [Input("Product")]
        [ReferenceTarget("rnt_product")]
        public InArgument<EntityReference> _product { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            PluginInitializer initializer = new PluginInitializer(context);
            var groupCodeInformation = _groupCodeInformation.Get<EntityReference>(context);
            var product = _product.Get<EntityReference>(context);
            if (groupCodeInformation != null)
            {
                MongoDBUpdateRecordsRepository mongoDBUpdateRecordsRepository = new MongoDBUpdateRecordsRepository(initializer.Service);
                var hasActiveRecord = mongoDBUpdateRecordsRepository.getActiveRecordByGroupCodeInformationId(groupCodeInformation.Id);
                if (hasActiveRecord == null)
                {
                    Entity entity = new Entity("rnt_updatedrecordsmongodb");
                    entity["rnt_groupcodeinformationid"] = groupCodeInformation;
                    initializer.Service.Create(entity);
                }
            }
            else if (product != null)
            {
                MongoDBUpdateRecordsRepository mongoDBUpdateRecordsRepository = new MongoDBUpdateRecordsRepository(initializer.Service);
                var hasActiveRecord = mongoDBUpdateRecordsRepository.getActiveRecordByProductId(product.Id);
                if (hasActiveRecord == null)
                {
                    Entity entity = new Entity("rnt_updatedrecordsmongodb");
                    entity["rnt_productid"] = product;
                    initializer.Service.Create(entity);
                }
            }
        }
    }
}
