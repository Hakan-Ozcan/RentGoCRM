using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Validations;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Contact
{
    public class PreValidateContactCitizenShipCheck : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            IndividualCustomerValidation contactValidation = new IndividualCustomerValidation(initializer.Service, initializer.TracingService);
            Entity individualCustomerEntity;
            if (initializer.PluginContext.MessageName.ToLower() == "update")
                initializer.PluginContext.GetContextPostImages<Entity>(initializer.PostImgKey, out individualCustomerEntity);
            else
                initializer.PluginContext.GetContextInputEntity<Entity>(initializer.TargetKey, out individualCustomerEntity);

            try
            {
                var xrmHelper = new XrmHelper(initializer.Service);
                initializer.TraceMe("mernis check");
                    
                #region Check Citizenship
                SystemParameterBL systemParameterBL = new SystemParameterBL(initializer.Service);
                var parameters = systemParameterBL.GetSystemParameters();

                ConfigurationBL configurationBL = new ConfigurationBL(initializer.Service);
                var turkeyGuid = configurationBL.GetConfigurationByName("TurkeyGuid");

                initializer.TraceMe("turkeyGuid : " + turkeyGuid);
                initializer.TraceMe("parameters.isMernisEnabled : " + parameters.isMernisEnabled);

                if (parameters.isMernisEnabled &&
                    individualCustomerEntity.Attributes.Contains("rnt_citizenshipid") &&
                    individualCustomerEntity.GetAttributeValue<EntityReference>("rnt_citizenshipid").Id == new Guid(turkeyGuid.Split(';')[0]))
                {
                    initializer.TraceMe("mernis check start" + DateTime.Now);
                    initializer.TraceMe("last name " + individualCustomerEntity.GetAttributeValue<string>("lastname"));
                    initializer.TraceMe("first name " + individualCustomerEntity.GetAttributeValue<string>("firstname"));
                    initializer.TraceMe("birthdate " + individualCustomerEntity.GetAttributeValue<DateTime>("birthdate").Year);
                    initializer.TraceMe("governmentid " + individualCustomerEntity.GetAttributeValue<string>("governmentid"));

                    initializer.TraceMe("last name upper : " + individualCustomerEntity.GetAttributeValue<string>("lastname").ToUpper());
                    initializer.TraceMe("first name upper : " + individualCustomerEntity.GetAttributeValue<string>("firstname").ToUpper());


                    var result = contactValidation.checkCitizenshipNumber(individualCustomerEntity.GetAttributeValue<string>("firstname"),
                                                             individualCustomerEntity.GetAttributeValue<string>("lastname"),
                                                             individualCustomerEntity.GetAttributeValue<DateTime>("birthdate").Year,
                                                             (long)Convert.ToDouble(individualCustomerEntity.GetAttributeValue<string>("governmentid")));
                    contactValidation.TracingService.Trace("mernis check end " + DateTime.Now);

                    if (!result)
                    {
                        var message = xrmHelper.GetXmlTagContent(initializer.InitiatingUserId, "InvalidTCKNNumber");
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
