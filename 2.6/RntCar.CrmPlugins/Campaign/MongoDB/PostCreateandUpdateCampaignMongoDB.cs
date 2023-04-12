using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.Campaign.MongoDB
{
    //only triggers when rnt_mongodbintegrationtrigger,statecode,statuscode fields change for update
    public class PostCreateandUpdateCampaignMongoDB : IPlugin
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
            CampaignBL campaignBL = new CampaignBL(initializer.Service);

            if (initializer.PluginContext.MessageName.ToLower() == GlobalEnums.MessageNames.Create.ToString().ToLower())
            {
                var response = campaignBL.createCampaignMongoDB(postImg);
                if (!response.Result)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(response.ExceptionDetail);
                }
                campaignBL.updateMongoDBCreateRelatedFields(postImg, response.Id);
            }
            else if (initializer.PluginContext.MessageName.ToLower() == GlobalEnums.MessageNames.Update.ToString().ToLower())
            {
                var response = campaignBL.updateCampaignMongoDB(postImg);
                if (!response.Result)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(response.ExceptionDetail);
                }
                campaignBL.UpdateMongoDBUpdateRelatedFields(postImg);
            }

        }
    }
}
