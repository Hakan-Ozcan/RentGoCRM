using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class FuelPriceBL : BusinessHandler
    {
        public FuelPriceBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public FuelPriceBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public FuelPriceBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public FuelPriceBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }
        public decimal? calculateFuelPriceByEquipmentId(Guid equipmentId, int fuelValue)
        {
            ProductRepository productRepository = new ProductRepository(this.OrgService, this.CrmServiceClient);
            var productData = productRepository.getProductByEquipmentId(equipmentId);

            FuelPriceRepository fuelPriceRepository = new FuelPriceRepository(this.OrgService, this.CrmServiceClient);
            var fuelPrice = fuelPriceRepository.getFuelPriceByFuelTypeAllColumns(productData.fuelTypeCode);

            var totalLiter = (productData.tankCapacity * fuelValue) / 8;
            return totalLiter * fuelPrice.GetAttributeValue<Money>("rnt_price")?.Value;
        }
    }
}
