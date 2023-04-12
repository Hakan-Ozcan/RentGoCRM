using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Validations;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.Account.Update
{
    public class PreUpdateCorporateCustomer : IPlugin
    {
        //filter : rnt_taxnumber and rnt_accounttypecode
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            Entity entity;
            initializer.PluginContext.GetContextInputEntity<Entity>(initializer.TargetKey, out entity);

            Entity preImg;
            initializer.PluginContext.GetContextPreImages(initializer.PreImgKey, out preImg);

            try
            {
                ConfigurationBL configurationBL = new ConfigurationBL(initializer.Service);

                var countryId = entity.Attributes.Contains("rnt_countryid") ?
                    entity.GetAttributeValue<EntityReference>("rnt_countryid").Id : preImg.GetAttributeValue<EntityReference>("rnt_countryid").Id;
                
                SystemParameterBL systemParameterBL = new SystemParameterBL(initializer.Service);
                var systemParameter = systemParameterBL.GetSystemParameters();

                var customerTypeCode = entity.Attributes.Contains("rnt_accounttypecode") ?
                    entity.GetAttributeValue<OptionSetValue>("rnt_accounttypecode").Value : preImg.GetAttributeValue<OptionSetValue>("rnt_accounttypecode").Value;

                var taxno = entity.Attributes.Contains("rnt_taxnumber") ?
                    entity.GetAttributeValue<string>("rnt_taxnumber") : preImg.GetAttributeValue<string>("rnt_taxnumber");

                if (systemParameter.isCustomerDuplicateCheckEnabled)
                {
                    CorporateCustomerValidation corporateCustomerValidation = new CorporateCustomerValidation(initializer.Service);
                    var result = corporateCustomerValidation.checkDuplicateCorporateCustomerForUpdate(customerTypeCode, taxno, entity.Id);
                    if (!result)
                    {
                        XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                        var message = xrmHelper.GetXmlTagContent(initializer.InitiatingUserId, "DuplicateAccount");
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                    }
                }
                if (Convert.ToString(countryId) == configurationBL.GetConfigurationByName("TurkeyGuid").Split(';')[0].ToLower())
                {
                    if (systemParameter.isTaxnoValidationEnabled)
                    {
                        CorporateCustomerValidation corporateCustomerValidation = new CorporateCustomerValidation(initializer.Service);
                        var result = corporateCustomerValidation.isTaxnoValid(taxno);
                        if (!result)
                        {
                            XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                            var message = xrmHelper.GetXmlTagContent(initializer.InitiatingUserId, "InvalidTaxNumber");
                            initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
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
