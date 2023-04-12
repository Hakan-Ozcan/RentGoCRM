using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Validations;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.Account.Create
{
    public class PreCreateCorporateCustomer : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            Entity entity;
            initializer.PluginContext.GetContextInputEntity<Entity>(initializer.TargetKey, out entity);
            try
            {
                ConfigurationBL configurationBL = new ConfigurationBL(initializer.Service);
                var countryId = entity.GetAttributeValue<EntityReference>("rnt_countryid").Id;
                SystemParameterBL systemParameterBL = new SystemParameterBL(initializer.Service);
                var systemParameter = systemParameterBL.GetSystemParameters();

                // Customer duplicate check
                if (systemParameter.isCustomerDuplicateCheckEnabled &&
                    entity.Attributes.Contains("rnt_taxnumber") && entity.Attributes.Contains("rnt_accounttypecode"))
                {
                    var customerTypeCode = entity.GetAttributeValue<OptionSetValue>("rnt_accounttypecode").Value;

                    var taxno = entity.GetAttributeValue<string>("rnt_taxnumber");

                    CorporateCustomerValidation corporateCustomerValidation = new CorporateCustomerValidation(initializer.Service);
                    var result = corporateCustomerValidation.checkDuplicateCorporateCustomer(customerTypeCode, taxno);
                    if (!result)
                    {
                        XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                        var message = xrmHelper.GetXmlTagContent(initializer.InitiatingUserId, "DuplicateAccount");
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                    }
                }

                // Tax no and government id validations
                if (systemParameter.isTaxnoValidationEnabled && Convert.ToString(countryId) == configurationBL.GetConfigurationByName("TurkeyGuid").Split(';')[0].ToLower())
                {
                    if (systemParameter.isTaxnoValidationEnabled && entity.Attributes.Contains("rnt_taxnumber"))
                    {
                        var taxNumber = entity.GetAttributeValue<string>("rnt_taxnumber");
                        var xrmHelper = new XrmHelper(initializer.Service);
                        bool validationIsSuccess = true;
                        string errorMessage = null;

                        if (taxNumber.Length == 10 /* for company */)
                        {
                            CorporateCustomerValidation corporateCustomerValidation = new CorporateCustomerValidation(initializer.Service);
                            var result = corporateCustomerValidation.isTaxnoValid(taxNumber);
                            if (!result)
                            {
                                validationIsSuccess = false;
                                errorMessage = xrmHelper.GetXmlTagContent(initializer.InitiatingUserId, "InvalidTaxNumber");
                            }
                        }
                        else /* for private company */
                        {
                            using (var individualCustomerValidation = new IndividualCustomerValidation(initializer.Service))
                            {
                                var customerGovermentIdValidationResult = individualCustomerValidation.checkIndividualCustomerGovermentIdValidity(new IndividualCustomerCreateParameter { governmentId = taxNumber });

                                if(customerGovermentIdValidationResult == false)
                                {
                                    validationIsSuccess = false;
                                    errorMessage = xrmHelper.GetXmlTagContent(initializer.InitiatingUserId, "InvalidTCKNNumber");
                                }
                            }
                        }

                        if(validationIsSuccess == false)
                        {
                            initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(errorMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
