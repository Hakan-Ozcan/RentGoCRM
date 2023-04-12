﻿using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.PriceList.Update
{

    public class PostUpdatePriceListMongoDB : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            Entity postImage;
            initializer.PluginContext.GetContextPostImages(initializer.PostImgKey, out postImage);
            initializer.TracingService.Trace("lets start");

            PriceFactorValidation priceListBL = new PriceFactorValidation(initializer.Service, initializer.TracingService);
            var res = priceListBL.UpdatePriceListInMongoDB(postImage);           
            priceListBL.UpdateMongoDBUpdateRelatedFields(postImage);
        }
    }
}
