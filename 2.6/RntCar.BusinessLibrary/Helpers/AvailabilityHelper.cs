using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary.MongoDB;
using System;

namespace RntCar.BusinessLibrary.Helpers
{
    public class AvailabilityHelper : HelperHandler
    {
        public AvailabilityHelper(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public AvailabilityHelper(IOrganizationService organizationService) : base(organizationService)
        {
        }

        public AvailabilityHelper(IOrganizationService organizationService, ITracingService tracingService) : base(organizationService, tracingService)
        {
        }

        public CalculatePricesForUpdateContractResponse calculateAvailability_Contract(Entity contractInformation,
                                                                                       Guid dropoffBranchId,
                                                                                       DateTime dropoffDateTime,
                                                                                       DateTime utcpickupTime,
                                                                                       int langId,
                                                                                       bool isMonthly)
        {

            var customerType = 0;
            //fucking stupid enum wrong using!
            if (contractInformation.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value == (int)rnt_ReservationTypeCode.Broker)
            {
                customerType = (int)rnt_ReservationTypeCode.Acente / 10;
            }
            else if (contractInformation.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value == (int)rnt_ReservationTypeCode.Acente)
            {
                customerType = (int)rnt_ReservationTypeCode.Broker / 10;
            }
            else
            {
                customerType = contractInformation.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value / 10;
            }

            ContractInvoiceDateRepository contractInvoiceDateRepository = new ContractInvoiceDateRepository(this.IOrganizationService);
            Entity ent = contractInvoiceDateRepository.getLastInvoiceofMonth_Monthly(contractInformation.Id.ToString());
            this.Trace("ent" + ent == null ? "ent is null" : "ent is not null");

            AvailibilityBL availibilityBL = new AvailibilityBL(this.IOrganizationService);
            //todo offset with different timezones will work unproperly
            var availabilityParam = new ClassLibrary.MongoDB.AvailabilityParameters
            {
                dropOffBranchId = dropoffBranchId,
                dropoffDateTime = dropoffDateTime,
                contractId = Convert.ToString(contractInformation.Id),
                customerType = customerType,
                corporateCustomerId = contractInformation.Attributes.Contains("rnt_corporateid") ?
                                       Convert.ToString(contractInformation.GetAttributeValue<EntityReference>("rnt_corporateid").Id) :
                                       null,
                pickupBranchId = contractInformation.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id,
                pickupDateTime = isMonthly ? dropoffDateTime.AddDays(-30) : utcpickupTime,
                //todo will change after corporate , agenta , broker
                individualCustomerId = contractInformation.GetAttributeValue<EntityReference>("rnt_customerid").Id.ToString(),
                isMonthly = isMonthly,
                //sözleşme güncellemede hep 1 aylık
                monthValue = isMonthly ? 1 : 0,
                operationType = 100,
                month_dropoffdatetime = ent != null ? ent.GetAttributeValue<DateTime>("rnt_dropoffdatetime") : DateTime.MinValue,
                month_pickupdatetime = ent != null ? ent.GetAttributeValue<DateTime>("rnt_pickupdatetime") : DateTime.MinValue
            };
           
            this.Trace("calculateAvailibilityandReturnString end");

            var response = availibilityBL.calculateAvailibility(availabilityParam, langId);
          
            var serializedResponse = JsonConvert.DeserializeObject<AvailabilityResponse>(response);
            this.Trace("availability response : " + response);
            this.Trace("availability params " + JsonConvert.SerializeObject(availabilityParam));
            var calculatePricesForUpdateContractResponse = availibilityBL.calculatePricesUpdateContract(serializedResponse, contractInformation.GetAttributeValue<EntityReference>("rnt_pricinggroupcodeid").Id, contractInformation.Id);
            this.Trace("before CalculatePricesforUpdateContractResponse" + JsonConvert.SerializeObject(calculatePricesForUpdateContractResponse));

            return calculatePricesForUpdateContractResponse;
        }
    }
}
