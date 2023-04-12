using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.PriceFactors.MongoDB
{
    //only triggers when rnt_mongodbintegrationtrigger,statecode,statuscode fields change for update
    public class PostCreateandUpdatePriceFactorsMongoDB : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            Entity postImg;
            initializer.PluginContext.GetContextPostImages(initializer.PostImgKey, out postImg);
            if (postImg == null)
            {
                initializer.TraceMe("image is null");
            }
            if (postImg.Attributes.Contains("rnt_mongodbintegrationtrigger"))
            {
                initializer.TraceMe("trigger ok");
                PriceFactorBL priceFactorBL = new PriceFactorBL(initializer.Service, initializer.TracingService);

                if (initializer.PluginContext.MessageName.ToLower() == GlobalEnums.MessageNames.Create.ToString().ToLower())
                {
                    initializer.TraceMe("create");
                    var createResponse = priceFactorBL.createPriceFactorInMongoDB(postImg);
                    initializer.TraceMe("create response : " + createResponse.Result);
                    if (!createResponse.Result)
                    {
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(createResponse.ExceptionDetail);
                    }
                    priceFactorBL.updateMongoDBCreateRelatedFields(postImg, createResponse.Id);
                }
                else if (initializer.PluginContext.MessageName.ToLower() == GlobalEnums.MessageNames.Update.ToString().ToLower())
                {
                    var updateResponse = priceFactorBL.updatePriceFactorInMongoDB(postImg);
                    if (!updateResponse.Result)
                    {
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(updateResponse.ExceptionDetail);
                    }
                    priceFactorBL.UpdateMongoDBUpdateRelatedFields(postImg);
                }
            }
        }
    }
}
