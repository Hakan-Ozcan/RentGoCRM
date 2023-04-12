using Microsoft.Xrm.Sdk;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.InvoiceItem.Create
{
    public class PostCreateInvoiceItemCalculateInvoiceRollupField : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            Entity postImage;
            initializer.PluginContext.GetContextPostImages(initializer.PostImgKey, out postImage);

            try
            {
                var invoice = postImage.GetAttributeValue<EntityReference>("rnt_invoiceid");

                XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                xrmHelper.CalculateRollupField(invoice.LogicalName, invoice.Id, "rnt_totalamount");
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
