using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Validations;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.Contact
{
    public class PostCreateContactCheckMobilePhoneSingularity : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var initializer = new PluginInitializer(serviceProvider);
            try
            {
                var contactValidation = new IndividualCustomerValidation(initializer.Service, initializer.TracingService);
                Entity individualCustomerEntity;
                initializer.PluginContext.GetContextInputEntity<Entity>(initializer.TargetKey, out individualCustomerEntity);

                //if plugin is pre
                if (individualCustomerEntity.Id == Guid.Empty || individualCustomerEntity.Id == null)
                {
                    if (individualCustomerEntity.Attributes.Contains("mobilephone") == true)
                    {
                        string mobilePhone = individualCustomerEntity.GetAttributeValue<string>("mobilephone");
                        contactValidation.checkIndividualCustomerDuplicateFieldAndClear("mobilephone", mobilePhone);
                    }
                }
                //plugin is post
                else
                {
                    if (individualCustomerEntity.Attributes.Contains("mobilephone") == true)
                    {
                        string mobilePhone = individualCustomerEntity.GetAttributeValue<string>("mobilephone");
                        contactValidation.checkIndividualCustomerDuplicateFieldAndClearOthers("mobilephone", mobilePhone, individualCustomerEntity.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.ToString());
            }
        }
    }
}
