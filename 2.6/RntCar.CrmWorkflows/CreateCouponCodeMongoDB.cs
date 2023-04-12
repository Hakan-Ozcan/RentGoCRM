using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.SDK.Common;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmWorkflows
{
    public class CreateCouponCodeMongoDB : CodeActivity
    {
        [Input("Coupon Code Definition")]
        [ReferenceTarget("rnt_couponcodedefinition")]
        public InArgument<EntityReference> _couponCodeDefinition { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            PluginInitializer initializer = new PluginInitializer(context);

            var couponCodeDefinitionRef = _couponCodeDefinition.Get<EntityReference>(context);
            initializer.TraceMe("couponCodeDefinition : " + couponCodeDefinitionRef.Id);

            try
            {
                CouponCodeDefinitionBL couponCodeDefinitionBL = new CouponCodeDefinitionBL(initializer.Service, initializer.TracingService);

                CouponCodeDefinitionRepository couponCodeDefinitionRepository = new CouponCodeDefinitionRepository(initializer.Service);
                var couponCodeDefinition = couponCodeDefinitionRepository.getCouponCodeDefinitionById(couponCodeDefinitionRef.Id);

                var couponCodeResponse = couponCodeDefinitionBL.createCouponCodeListInMongoDB(couponCodeDefinition);
                if (!couponCodeResponse.Result)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(couponCodeResponse.ExceptionDetail);
                }
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
