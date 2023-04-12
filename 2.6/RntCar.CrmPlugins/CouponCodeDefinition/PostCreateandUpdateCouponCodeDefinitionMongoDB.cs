using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.CouponCodeDefinition
{
    //update - only triggers when rnt_mongodbintegrationtrigger,statecode,statuscode fields change
    public class PostCreateandUpdateCouponCodeDefinitionMongoDB : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            Entity postImg;
            initializer.PluginContext.GetContextPostImages(initializer.PostImgKey, out postImg);

            if (postImg == null)
            {
                initializer.TracingService.Trace("image is null");
            }
            CouponCodeDefinitionBL couponCodeDefinitionBL = new CouponCodeDefinitionBL(initializer.Service, initializer.TracingService);

            if (initializer.PluginContext.MessageName.ToLower() == GlobalEnums.MessageNames.Create.ToString().ToLower())
            {
                var response = couponCodeDefinitionBL.createCouponCodeDefinitionInMongoDB(postImg);
                if (!response.Result)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(response.ExceptionDetail);
                }
                couponCodeDefinitionBL.updateMongoDBCreateRelatedFields(postImg, response.Id);
            }
            else if (initializer.PluginContext.MessageName.ToLower() == GlobalEnums.MessageNames.Update.ToString().ToLower())
            {
                var response = couponCodeDefinitionBL.updateCouponCodeDefinitionInMongoDB(postImg);
                if (!response.Result)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(response.ExceptionDetail);
                }
                couponCodeDefinitionBL.UpdateMongoDBUpdateRelatedFields(postImg);
            }
        }
    }
}
