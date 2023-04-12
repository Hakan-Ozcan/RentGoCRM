using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Validations;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Contact
{
    public class PreUpdateValidateContact : IPlugin
    {
        // Tolga AYKURT - 11.03.2019
        public void Execute(IServiceProvider serviceProvider)
        {
            var initializer = new PluginInitializer(serviceProvider);
            var contactValidation = new IndividualCustomerValidation(initializer.Service, initializer.TracingService);

            // Target
            Entity individualCustomerEntity;
            initializer.PluginContext.GetContextInputEntity<Entity>(initializer.TargetKey, out individualCustomerEntity);

            // PreImage
            Entity preImgIndividualCustomerEntity;
            initializer.PluginContext.GetContextPreImages<Entity>(initializer.PreImgKey, out preImgIndividualCustomerEntity);

           
            if (individualCustomerEntity.Attributes.Contains("rnt_citizenshipid") == true &&
                individualCustomerEntity.Attributes.Contains("rnt_drivinglicensedate") == true &&
                individualCustomerEntity.Attributes.Contains("birthdate") == true)
            {
                if (individualCustomerEntity != null && preImgIndividualCustomerEntity != null)
                {
                    DateTime birthDate, drivingLicenseDate;
                    string citizenshipId;

                    if (individualCustomerEntity.Attributes.Contains("rnt_citizenshipid"))
                        citizenshipId = individualCustomerEntity.GetAttributeValue<EntityReference>("rnt_citizenshipid").Id.ToString();
                    else
                        citizenshipId = preImgIndividualCustomerEntity.GetAttributeValue<EntityReference>("rnt_citizenshipid").Id.ToString();

                    if (individualCustomerEntity.Attributes.Contains("rnt_drivinglicensedate"))
                        drivingLicenseDate = individualCustomerEntity.GetAttributeValue<DateTime>("rnt_drivinglicensedate");
                    else
                        drivingLicenseDate = preImgIndividualCustomerEntity.GetAttributeValue<DateTime>("rnt_drivinglicensedate");

                    if (individualCustomerEntity.Attributes.Contains("birthdate"))
                        birthDate = individualCustomerEntity.GetAttributeValue<DateTime>("birthdate");
                    else
                        birthDate = preImgIndividualCustomerEntity.GetAttributeValue<DateTime>("birthdate");

                    // Contact birthdate validation
                    bool isAlienCustomer = false;
                    bool isOKDrivingLicenseDateValidation = false;
                    var isOK = contactValidation.checkBirthDate(birthDate, citizenshipId, out isAlienCustomer);

                    ConfigurationBL configurationBL = new ConfigurationBL(initializer.Service);
                    var turkeyGuid = configurationBL.GetConfigurationByName("TurkeyGuid");
                    var langId = 1033;
                    if (new Guid(turkeyGuid.Split(';')[0]) == new Guid(citizenshipId))
                    {
                        langId = 1055;
                    }
                    initializer.TraceMe("langId : " + langId);
                    initializer.TraceMe("isOK: " + isOK);
                    if (isOK == true)
                    {
                        isOKDrivingLicenseDateValidation = contactValidation.CheckDrivingLicenseDate(drivingLicenseDate);
                    }
                    initializer.TraceMe("isOKDrivingLicenseDateValidation : " + isOKDrivingLicenseDateValidation);
                    if (isOK == true && isOKDrivingLicenseDateValidation == true)
                    {
                        // Contact birthdate and driving license date validation
                        isOK = contactValidation.checkDrivingLicenseDateAndBirthDate(citizenshipId, birthDate, drivingLicenseDate, out isAlienCustomer);
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
                                validationMessage = xrmHelper.GetXmlTagContentByGivenLangId("AlienContactBirthDateValidation", langId);
                            else
                                validationMessage = xrmHelper.GetXmlTagContentByGivenLangId("ContactBirthDateValidation", langId);
                        }

                        if (isOKDrivingLicenseDateValidation == false)
                        {
                            validationMessage = xrmHelper.GetXmlTagContentByGivenLangId("LicenseDateValidation", langId);
                        }

                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(validationMessage);
                    }
                }
            }
        }
    }
}
