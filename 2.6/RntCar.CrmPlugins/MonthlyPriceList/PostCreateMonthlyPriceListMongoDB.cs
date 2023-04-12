using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.MonthlyPriceList
{
    public class PostCreateMonthlyPriceListMongoDB : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            Entity postImg;
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            initializer.PluginContext.GetContextPostImages(initializer.PostImgKey, out postImg);


            if (postImg == null)
            {
                initializer.TraceMe("image is null");
            }
            try
            {
                MonthlyPriceListBL monthlyPriceListBL = new MonthlyPriceListBL(initializer.Service, initializer.TracingService);
                var response = monthlyPriceListBL.createMonthlyPriceListInMongoDB(postImg);
                initializer.TraceMe("Response : " + response.Result);
                if (!response.Result)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(response.ExceptionDetail);
                }

            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
