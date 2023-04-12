using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;

namespace RntCar.BusinessLibrary.Helpers
{
    public class AdditionalProductHelper : HelperHandler
    {
        public AdditionalProductHelper(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public AdditionalProductHelper(IOrganizationService organizationService) : base(organizationService)
        {
        }

        public AdditionalProductHelper(CrmServiceClient crmServiceClient, IOrganizationService organizationService) : base(crmServiceClient, organizationService)
        {
        }

        public AdditionalProductHelper(IOrganizationService organizationService, ITracingService tracingService) : base(organizationService, tracingService)
        {
        }

        public AdditionalProductHelper(CrmServiceClient crmServiceClient, IOrganizationService organizationService, ITracingService tracingService) : base(crmServiceClient, organizationService, tracingService)
        {
        }

        public AdditionalProductService_Contract getAdditionalProductService_Contract(Guid additionalProductId, Guid contractId)
        {
            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.IOrganizationService, this.CrmServiceClient);
            var subProduct = additionalProductRepository.getAdditonalProductByParentAdditionalProductId(additionalProductId);

            if (subProduct == null)
            {
                return new AdditionalProductService_Contract();
            }
            var priceCalculationAdditionalProduct = additionalProductRepository.getAdditionalProductById(additionalProductId, new string[] { "rnt_pricecalculationtypecode" });

            Entity serviceItem = null;
            if (priceCalculationAdditionalProduct.GetAttributeValue<OptionSetValue>("rnt_pricecalculationtypecode").Value == (int)ClassLibrary._Enums_1033.rnt_PriceCalculationTypeCode.Fixed)
            {
                ContractItemRepository contractItemRepository = new ContractItemRepository(this.IOrganizationService, this.CrmServiceClient);
                serviceItem = contractItemRepository.getContractItemFineAdditionalProductByContractIdandAdditionalProductIdByGivenColumns(contractId,
                                                                                                                                          subProduct.Id,
                                                                                                                                          new string[] { });
            };

            return new AdditionalProductService_Contract
            {
                serviceItem = serviceItem,
                subProduct = subProduct
            };

        }

        public Entity getFineAdditionalProduct()
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.IOrganizationService, this.CrmServiceClient);
            var value = configurationBL.GetConfigurationByName("additionalProduct_trafficFineProductCode");

            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.IOrganizationService, this.CrmServiceClient);
            return additionalProductRepository.getAdditionalProductByProductCode(value);
        }

        public decimal calculateFineProductServicePrice(Guid? fineProductId, Guid currentProductId, decimal fineAmount, decimal subFineAmount)
        {
            decimal _subFineAmount = decimal.Zero;

            ConfigurationBL configurationBL = new ConfigurationBL(this.IOrganizationService);
            var value = configurationBL.GetConfigurationByName("additionalProduct_HGS");

            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.IOrganizationService);
            var hgsProductId = additionalProductRepository.getAdditionalProductByProductCode(value)?.Id;

            //vat

            FineRateRepository fineRateRepository = new FineRateRepository(this.IOrganizationService);
            var amount = fineRateRepository.getFineAmount(currentProductId, fineAmount);
            if (amount == decimal.Zero)
            {
                _subFineAmount = subFineAmount;
            }
            else
            {
                _subFineAmount = amount;
            }
            if (hgsProductId != currentProductId)
            {
                _subFineAmount = _subFineAmount * 1.18M;
            }
            return _subFineAmount;
        }

        public void changeAdditionalProductsPriceByGivenCurrency()
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.IOrganizationService);
            var _turkishCurrency = configurationBL.GetConfigurationByName("currency_TRY");
        }

        public int? getAdditionalKilometerByAdditionalIds(List<Guid> additionalIds)
        {
            int? kilometer = 0;
            AdditionalProductKilometerLimitActionRepository additionalProductKilometerLimitActionRepository = new AdditionalProductKilometerLimitActionRepository(this.IOrganizationService);
            var additionalProductKilometerLimitActions = additionalProductKilometerLimitActionRepository.getAdditionalProductKilometerLimitActions();

            foreach (var item in additionalProductKilometerLimitActions)
            {
                var additionalProductIdInAction = item.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id;
                if (additionalIds.Contains(additionalProductIdInAction))
                {
                    kilometer += item.GetAttributeValue<int>("rnt_kilometerlimiteffect");
                    break;
                }
            }

            return kilometer;
        }
    }
}
