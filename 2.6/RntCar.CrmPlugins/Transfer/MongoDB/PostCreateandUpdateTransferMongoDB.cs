using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Validations;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Transfer.MongoDB
{
    // removed modified, created, and uniq identifier fields from attributes
    public class PostCreateandUpdateTransferMongoDB : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            try
            {
                Entity postImg;
                initializer.PluginContext.GetContextPostImages(initializer.PostImgKey, out postImg);
                if (postImg == null)
                    initializer.TraceMe("image is null");

                initializer.TraceMe("validation started");
                TransferCreationValidation transferCreationValidation = new TransferCreationValidation(initializer.Service, initializer.TracingService);
                var result = transferCreationValidation.validateTransfer(postImg);

                if (!result.Result)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(result.Message);
                }
                initializer.TraceMe("validation end");
                TransferBL transferBL = new TransferBL(initializer, initializer.Service, initializer.TracingService);

                if (initializer.PluginContext.MessageName.ToLower() == GlobalEnums.MessageNames.Create.ToString().ToLower())
                {
                    initializer.TraceMe("create");
                    var createResponse = transferBL.createTransferInMongoDB(postImg);
                    initializer.TraceMe("create response : " + createResponse.Result);
                    if (!createResponse.Result)
                    {
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(createResponse.ExceptionDetail);
                    }
                    transferBL.updateMongoDBCreateRelatedFields(postImg, createResponse.Id);
                }
                else if (initializer.PluginContext.MessageName.ToLower() == GlobalEnums.MessageNames.Update.ToString().ToLower())
                {
                    var updateResponse = transferBL.updateTransferInMongoDB(postImg);
                    if (!updateResponse.Result)
                    {
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(updateResponse.ExceptionDetail);
                    }
                    transferBL.UpdateMongoDBUpdateRelatedFields(postImg);
                }
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
