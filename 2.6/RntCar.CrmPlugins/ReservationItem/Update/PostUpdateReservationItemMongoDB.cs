using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.ReservationItem.Update
{
    public class PostUpdateReservationItemMongoDB : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            initializer.TraceMe("initializer parent " + initializer.PluginContext.ParentContext != null ?
                                                        initializer.PluginContext.ParentContext.MessageName :
                                                        "null");


            Entity postImage;
            initializer.PluginContext.GetContextPostImages(initializer.PostImgKey, out postImage);

            var itemTypeCode = postImage.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value;
            if (itemTypeCode == (int)ClassLibrary._Enums_1033.rnt_reservationitem_rnt_itemtypecode.Equipment)
            {
                ReservationItemBL reservationItemBL = new ReservationItemBL(initializer.Service, initializer.TracingService);
                initializer.TraceMe("messagename " + initializer.PluginContext.MessageName.ToLower());
                //var res = reservationItemBL.UpdateReservationItemInMongoDB(postImage);
                var res  = initializer.RetryMethod<MongoDBResponse>(() => reservationItemBL.UpdateReservationItemInMongoDB(postImage), StaticHelper.retryCount, StaticHelper.retrySleep);
                reservationItemBL.UpdateMongoDBUpdateRelatedFields(postImage, res);
                //var actionResponse = reservationItemBL.callReservationItemActionInMongoDB(postImage, initializer.PluginContext.MessageName);

                if (!res.Result)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(res.ExceptionDetail);
                }
            }
        }
    }
}
