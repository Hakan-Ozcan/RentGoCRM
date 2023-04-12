using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.Payment
{
    public class PostUpdatePaymentCalculateRollupFieldsPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            if (initializer.PluginContext.Depth >= 5)
            {
                initializer.TraceMe("depth is : " + initializer.PluginContext.Depth);
                initializer.TraceMe("returning");
                return;
            }
            try
            {
                Entity postImage;
                initializer.PluginContext.GetContextPostImages(initializer.PostImgKey, out postImage);

                PaymentBL paymentBL = new PaymentBL(initializer.Service, initializer.TracingService);
                paymentBL.calculateRollupRelatedFields(postImage);
            }
            //todo log somewhere
            catch (Exception ex)
            {
                initializer.TraceMe(ex.Message);

            }
        }
    }
}
