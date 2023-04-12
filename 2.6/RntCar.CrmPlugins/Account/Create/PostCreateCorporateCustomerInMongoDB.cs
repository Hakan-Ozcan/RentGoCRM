using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.Account.Create
{
    public class PostCreateCorporateCustomerInMongoDB : IPlugin
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
                CorporateCustomerBL corporateCustomerBL = new CorporateCustomerBL(initializer.Service, initializer.TracingService);
                var response = corporateCustomerBL.createCorporateCustomerInMongoDB(postImg);
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
