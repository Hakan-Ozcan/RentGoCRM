using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.PriceHourEffect
{
    public class PostCreateandUpdatePriceHourEffectMongoDB : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            if (initializer.PluginContext.Depth > 1)
                return;

            Entity postImg;
            initializer.PluginContext.GetContextPostImages(initializer.PostImgKey, out postImg);

            try
            {
                PriceHourEffectBL priceHourEffectBL = new PriceHourEffectBL(initializer.Service, initializer.TracingService);

                if (initializer.PluginContext.MessageName.ToLower() == GlobalEnums.MessageNames.Create.ToString().ToLower())
                {
                    initializer.TraceMe("create message");
                    initializer.TraceMe("postImg" + postImg == null ? "postimg is null " : "post img is not null");
                    var createResponse = priceHourEffectBL.createPriceHourEffectInMongoDB(postImg);
                    if (!createResponse.Result)
                    {
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(createResponse.ExceptionDetail);
                    }
                    priceHourEffectBL.updateMongoDBCreateRelatedFields(postImg, createResponse.Id);
                }
                else if (initializer.PluginContext.MessageName.ToLower() == GlobalEnums.MessageNames.Update.ToString().ToLower())
                {
                    initializer.TraceMe("create message");
                    initializer.TraceMe("postImg" + postImg == null ? "postimg is null " : "post img is not null");
                    var createResponse = priceHourEffectBL.updatePriceHourEffectInMongoDB(postImg);
                    if (!createResponse.Result)
                    {
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(createResponse.ExceptionDetail);
                    }
                    priceHourEffectBL.UpdateMongoDBUpdateRelatedFields(postImg);
                }
                }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
