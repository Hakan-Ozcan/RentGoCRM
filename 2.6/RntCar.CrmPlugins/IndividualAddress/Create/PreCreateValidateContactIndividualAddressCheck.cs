using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Validations;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.IndividualAddress.Create
{
    //triggers in create event
    public class PreCreateValidateContactIndividualAddressCheck : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            IndividualCustomerDefaultAddressValidation contactDefaultAddressValidation = new IndividualCustomerDefaultAddressValidation(initializer.Service,initializer.TracingService);
            try
            {
                Entity individualCustomerAddress;
                initializer.PluginContext.GetContextInputEntity<Entity>(initializer.TargetKey, out individualCustomerAddress);

                if (individualCustomerAddress.GetAttributeValue<Boolean>("rnt_isdefaultaddress") == true)
                {                   
                    var contactId = individualCustomerAddress.GetAttributeValue<EntityReference>("rnt_contactid").Id;
                    initializer.TracingService.Trace("contactId : " + contactId);

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
           
        }
    }
}
