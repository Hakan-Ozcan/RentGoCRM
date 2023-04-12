using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Reservation.Actions
{
    public class ExecuteCheckDiscount : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            try
            {
                int langId;
                initializer.PluginContext.GetContextParameter<int>("langId", out langId);

                decimal reservationTotalAmount;
                initializer.PluginContext.GetContextParameter<decimal>("reservationTotalAmount", out reservationTotalAmount);

                decimal discountAmount;
                initializer.PluginContext.GetContextParameter<decimal>("discountAmount", out discountAmount);

                initializer.TraceMe("reservationTotalAmount: " + reservationTotalAmount);
                initializer.TraceMe("discountAmount: " + discountAmount);

                if (discountAmount > 0)
                {
                    ConfigurationBL configurationBL = new ConfigurationBL(initializer.Service);
                    var productCode = configurationBL.GetConfigurationByName("additionalProducts_manuelDiscount");
                    AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(initializer.Service);
                    var additionalProduct = additionalProductRepository.getAdditionalProductByProductCode(productCode, new string[] { "rnt_pricecalculationtypecode",
                                                                                                                                                          "rnt_name"});

                    SystemUserRepository systemUserRepository = new SystemUserRepository(initializer.Service);
                    var userInformation = systemUserRepository.getSystemUserByIdWithGivenColumns(initializer.PluginContext.UserId, new string[] { "businessunitid",
                                                                                                                                                  "positionid",
                                                                                                                                                  "rnt_businessrolecode" });
                    var userBusinessUnit = userInformation.GetAttributeValue<EntityReference>("businessunitid");
                    var businessRole = userInformation.GetAttributeValue<OptionSetValue>("rnt_businessrolecode");
                    var businessRoleCode = 0;
                    var userBusinessUnitId = Guid.Empty;

                    if (businessRole != null)
                    {
                        businessRoleCode = businessRole.Value;
                    }

                    if (userBusinessUnit != null)
                    {
                        userBusinessUnitId = userBusinessUnit.Id;
                    }

                    initializer.TraceMe("businessRoleCode: " + businessRoleCode);

                    ManuelDiscountRateRepository manuelDiscountRateRepository = new ManuelDiscountRateRepository(initializer.Service);
                    var manuelDiscount = manuelDiscountRateRepository.getManuelDiscountRateByBusinessUnitIdByBusinessRoleCode(userBusinessUnitId, businessRoleCode);
                    if (manuelDiscount == null)
                    {
                        XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                        var message = xrmHelper.GetXmlTagContentByGivenLangId("UnauthorizedDiscount", langId);
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                    }

                    var manuelDiscountRate = manuelDiscount.GetAttributeValue<decimal>("rnt_discountrate");
                    var totalDiscountAmount = reservationTotalAmount * manuelDiscountRate / 100;
                    if (discountAmount > totalDiscountAmount)
                    {
                        XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                        var message = string.Format(xrmHelper.GetXmlTagContentByGivenLangId("OverrateDiscount", 1055), decimal.Round(totalDiscountAmount));
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                    }
                }

                initializer.PluginContext.OutputParameters["serviceResponse"] = JsonConvert.SerializeObject(ClassLibrary.ResponseResult.ReturnSuccess());
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
