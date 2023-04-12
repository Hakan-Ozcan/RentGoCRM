using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.KilometerLimit.Update
{
    public class PostUpdateKilometerLimit : IPlugin
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
                KilometerLimitBL kilometerLimitBL = new KilometerLimitBL(initializer.Service, initializer.TracingService);
                var response = kilometerLimitBL.updateKilometerLimitInMongoDB(postImg);
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
