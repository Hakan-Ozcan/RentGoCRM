using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.ContractItem.Create
{
    public class PostCreateContractItemMongoDB : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            Entity postImage;
            initializer.PluginContext.GetContextPostImages(initializer.PostImgKey, out postImage);

            if (postImage == null)
            {
                initializer.TracingService.Trace("image is null");
            }
            if (postImage.Attributes.Contains("rnt_mongodbintegrationtrigger"))
            {
                var itemTypeCode = postImage.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value;
                if (itemTypeCode == (int)RntCar.ClassLibrary._Enums_1033.rnt_contractitem_rnt_itemtypecode.Equipment)
                {
                    ContractItemBL contractItemBL = new ContractItemBL(initializer.Service, initializer.TracingService);

                    initializer.TraceMe("started");
                    if (postImage.Attributes.Contains("rnt_mongodbintegrationtrigger"))
                    {
                        var response = contractItemBL.createContractItemInMongoDB(postImage);
                        initializer.TraceMe("operations finished");
                        if (!response.Result)
                        {
                            initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(response.ExceptionDetail);
                        }

                        contractItemBL.updateMongoDBCreateRelatedFields(postImage, response.Id);
                    }
                }
            }
            else
            {
                initializer.TraceMe("rnt_mongodbintegrationtrigger not contains");
            }
        }
    }
}
