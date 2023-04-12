using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Invoice
{
    //this plugin works only statuscode is changed
    public class PostUpdateInvoiceStatusChange : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            try
            {
                Entity postImage;
                initializer.PluginContext.GetContextPostImages(initializer.PostImgKey, out postImage);

                InvoiceBL invoiceBL = new InvoiceBL(initializer.Service, initializer.TracingService);
                invoiceBL.updateInvoiceItemsStatusByInvoiceHeader(postImage.Id, postImage.GetAttributeValue<OptionSetValue>("statuscode").Value);
            }
            //todo log somewhere
            catch (Exception ex)
            {
                initializer.TraceMe(ex.Message);
            }
        }
    }
}
