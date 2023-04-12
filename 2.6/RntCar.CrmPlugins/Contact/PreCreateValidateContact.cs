using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Validations;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Contact
{  
    public class PreCreateValidateContact : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var initializer = new PluginInitializer(serviceProvider);
            var contactValidation = new IndividualCustomerValidation(initializer.Service, initializer.TracingService);
            Entity individualCustomerEntity;
            initializer.PluginContext.GetContextInputEntity<Entity>(initializer.TargetKey, out individualCustomerEntity);

            if (individualCustomerEntity.Attributes.Contains("rnt_citizenshipid") == true &&
                   individualCustomerEntity.Attributes.Contains("rnt_drivinglicensedate") == true &&
                   individualCustomerEntity.Attributes.Contains("birthdate") == true)
            {
                // Contact birthdate validation
                bool isAlienCustomer = false;
                bool isOKDrivingLicenseDateValidation = false;

                var isOK = contactValidation.checkBirthDate(
                    individualCustomerEntity.GetAttributeValue<DateTime>("birthdate"),
                    individualCustomerEntity.GetAttributeValue<EntityReference>("rnt_citizenshipid").Id.ToString(),
                    out isAlienCustomer);

                if (isOK == true)
                {
                    isOKDrivingLicenseDateValidation = contactValidation.CheckDrivingLicenseDate(individualCustomerEntity.GetAttributeValue<DateTime>("rnt_drivinglicensedate"));
                }

                if (isOK == true && isOKDrivingLicenseDateValidation == true)
                {
                    // Contact birthdate and driving license date validation
                    isOK = contactValidation.checkDrivingLicenseDateAndBirthDate(
                       individualCustomerEntity.GetAttributeValue<EntityReference>("rnt_citizenshipid").Id.ToString(),
                       individualCustomerEntity.GetAttributeValue<DateTime>("birthdate"), individualCustomerEntity.GetAttributeValue<DateTime>("rnt_drivinglicensedate"),
                       out isAlienCustomer);
                }
                initializer.TraceMe("isOK 2: " + isOK);
                initializer.TraceMe("isOKDrivingLicenseDateValidation 2: " + isOKDrivingLicenseDateValidation);
                if (isOK == false || isOKDrivingLicenseDateValidation == false)
                {
                    string validationMessage = string.Empty;
                    var xrmHelper = new XrmHelper(initializer.Service);

                    if (isOK == false)
                    {
                        if (isAlienCustomer)
                            validationMessage = xrmHelper.GetXmlTagContent(initializer.InitiatingUserId, "AlienContactBirthDateValidation");
                        else
                            validationMessage = xrmHelper.GetXmlTagContent(initializer.InitiatingUserId, "ContactBirthDateValidation");
                    }

                    if (isOKDrivingLicenseDateValidation == false)
                    {
                        validationMessage = xrmHelper.GetXmlTagContent(initializer.InitiatingUserId, "LicenseDateValidation");
                    }

                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(validationMessage);
                }
            }
        }
    }
}
