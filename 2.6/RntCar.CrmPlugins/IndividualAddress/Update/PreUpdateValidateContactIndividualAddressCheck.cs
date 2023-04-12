using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Validations;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.IndividualAddress.Update
{
    //triggers in update event
    //for update evet have a filtering attribute : rnt_isdefaultaddress
    public class PreUpdateValidateContactIndividualAddressCheck : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            IndividualCustomerDefaultAddressValidation contactDefaultAddressValidation = new IndividualCustomerDefaultAddressValidation(initializer.Service);
            try
            {
                Entity individualCustomerAddress;
                initializer.PluginContext.GetContextInputEntity<Entity>(initializer.TargetKey, out individualCustomerAddress);

                Entity preImgindividualCustomerAddress;
                initializer.PluginContext.GetContextPreImages<Entity>(initializer.PreImgKey, out preImgindividualCustomerAddress);

                if (individualCustomerAddress.GetAttributeValue<bool>("rnt_isdefaultaddress"))
                {
                    var contactId = preImgindividualCustomerAddress.GetAttributeValue<EntityReference>("rnt_contactid").Id;
                    var result = contactDefaultAddressValidation.CheckCustomerDefaultAddress(contactId);
                    if (!result)
                    {
                        XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                        var message = xrmHelper.GetXmlTagContent(initializer.InitiatingUserId, "IndividualCustomerDefaultAddressCheck");
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                    }
                }

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                //initializer.TraceFlush();
            }
        }
    }
}
