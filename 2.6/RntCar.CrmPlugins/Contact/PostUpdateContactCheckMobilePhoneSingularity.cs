using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Validations;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Contact
{
    public class PostUpdateContactCheckMobilePhoneSingularity : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var initializer = new PluginInitializer(serviceProvider);
            try
            {
                Entity individualCustomerEntity;
                initializer.PluginContext.GetContextInputEntity<Entity>(initializer.TargetKey, out individualCustomerEntity);
                ConfigurationBL configurationBL = new ConfigurationBL(initializer.Service);
                var dummyBroker = configurationBL.GetConfigurationByName("DUMMYCONTACTID_BROKER");

                if (new Guid(dummyBroker) == individualCustomerEntity.Id)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>("Sanal müşteri güncellenemez");

                }

                if (initializer.PluginContext.Depth > 2)
                {
                    return;
                }
                var contactValidation = new IndividualCustomerValidation(initializer.Service, initializer.TracingService);
                

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
