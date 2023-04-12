using Microsoft.Xrm.Sdk;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.ContractItem.Update
{
    public class PostUpdateContractItem : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            Entity postImage;
            initializer.PluginContext.GetContextPostImages(initializer.PostImgKey, out postImage);

            XrmHelper xrmHelper = new XrmHelper(initializer.Service);

            List<string> fields = new List<string>();
            fields.Add("rnt_taxamount");
            fields.Add("rnt_totalamount");
            fields.Add("rnt_totalfineamount");
            fields.Add("rnt_corporatetotalamount");
            
            try
            {
                var contractRef = postImage.GetAttributeValue<EntityReference>("rnt_contractid");

                xrmHelper.CalculateRollupField(contractRef.LogicalName, contractRef.Id, fields);
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
