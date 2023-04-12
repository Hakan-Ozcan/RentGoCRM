using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;
using System;
using System.Linq;

namespace RntCar.BusinessLibrary.Validations
{
    public class AvailabilityValidation : ValidationHandler
    {
        public AvailabilityValidation(IOrganizationService orgService) : base(orgService)
        {
        }

        public AvailabilityValidation(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public AvailabilityResponse checkMinimumReservationDuration(AvailabilityParameters availabilityParameters, int langId, int channel)
        {
            AvailabilityFactorsRepository availabilityFactorsRepository = new AvailabilityFactorsRepository(this.OrgService);
            var availabilityMinimumDayFactor = availabilityFactorsRepository.getMinimumDayAvailabilityByChannel(availabilityParameters.pickupDateTime,
                                                                                                                availabilityParameters.dropoffDateTime,
                                                                                                                channel);
            //if there is no definition , no need to check anything
            if (availabilityMinimumDayFactor == null)
            {
                return new AvailabilityResponse
                {
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }


            var branches = availabilityMinimumDayFactor.GetAttributeValue<OptionSetValueCollection>("rnt_multiselectbranchcode");
            //first select from multioptionset mappings
            MultiSelectMappingRepository multiSelectMappingRepository = new MultiSelectMappingRepository(this.OrgService);
            var ids = multiSelectMappingRepository.getSelectedItemsGuidListByOptionValueandGivenColumn("rnt_branch",
                                                                                                        branches);

            var branchDefinition = ids.Where(p => p.Equals(availabilityParameters.pickupBranchId)).FirstOrDefault();
            //if there is no definition to our current branch we dont need to continue
            if (branchDefinition == Guid.Empty)
            {
                return new AvailabilityResponse
                {
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
            var minDuration = availabilityMinimumDayFactor.GetAttributeValue<int>("rnt_minimumreservationday");
            var totalMinutes = decimal.Round(Convert.ToDecimal((availabilityParameters.dropoffDateTime - availabilityParameters.pickupDateTime).TotalMinutes),
                                            MidpointRounding.AwayFromZero);
            this.Trace("min duration" + minDuration);
            this.Trace("totalMinutes" + totalMinutes);

            var minDurationInMinutes = minDuration * 60 * 24;
            this.Trace("minDurationInMinutes" + minDurationInMinutes);
            if (minDurationInMinutes > totalMinutes)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                var hours = decimal.Round((decimal)minDurationInMinutes / 1440, 2);

                var message = xrmHelper.GetXmlTagContentByGivenLangId("MinimumReservationDuration", langId).Replace("@reservationday", hours.ToString());
                return new AvailabilityResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }
            return new AvailabilityResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }

        public AvailabilityResponse checkBrokerReservation(AvailabilityParameters availabilityParameters, int langId)
        {
            ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
            var res = reservationRepository.getReservationById(new Guid(availabilityParameters.reservationId), new string[] { "rnt_paymentmethodcode",
                                                                                                                              "rnt_pickupdatetime",
                                                                                                                              "rnt_dropoffdatetime",
                                                                                                                              "rnt_reservationtypecode"});

            var paymentMethod = res.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value;
            var resType = res.GetAttributeValue<OptionSetValue>("rnt_reservationtypecode").Value;
            this.Trace("rnt_paymentmethodcode" + paymentMethod);
            if (paymentMethod == (int)rnt_PaymentMethodCode.PayBroker ||
               paymentMethod == (int)rnt_PaymentMethodCode.PayOffice ||
               resType == (int)rnt_ReservationTypeCode.Acente)
            {
                ContractHelper contractHelper = new ContractHelper(this.OrgService, this.TracingService);
                var resDuration = contractHelper.calculateTotalDuration_ContractByPriceHourEffect_Decimal(res.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                                                                                                          res.GetAttributeValue<DateTime>("rnt_dropoffdatetime"));

                var queryDuration = contractHelper.calculateTotalDuration_ContractByPriceHourEffect_Decimal(availabilityParameters.pickupDateTime,
                                                                                                           availabilityParameters.dropoffDateTime);
                this.Trace("resDuration" + resDuration);
                this.Trace("queryDuration" + queryDuration);
                if (resDuration > queryDuration)
                {
                    return new AvailabilityResponse
                    {
                        //todo langid and xml
                        ResponseResult = ResponseResult.ReturnError("Broker/Acenta rezervasyonları'nın tarihi azaltılamaz.")
                    };
                }

            }
            return new AvailabilityResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }

        public AvailabilityResponse checkBrokerReservation_Contract(AvailabilityParameters availabilityParameters, int langId)
        {
            ContractRepository contractRepository = new ContractRepository(this.OrgService);
            var contract = contractRepository.getContractById(new Guid(availabilityParameters.contractId), new string[] { "rnt_paymentmethodcode", "rnt_contracttypecode" });

            var paymentMethod = contract.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value;
            var conractType = contract.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value;
            this.Trace("rnt_paymentmethodcode" + paymentMethod);
            if (paymentMethod == (int)rnt_PaymentMethodCode.PayBroker ||
                paymentMethod == (int)rnt_PaymentMethodCode.PayOffice ||
                conractType == (int)rnt_ReservationTypeCode.Acente)
            {
                Entity contractItem = null;
                if (conractType == (int)rnt_ReservationTypeCode.Acente)
                {                    
                    contractItem = contractRepository.getContractById(new Guid(availabilityParameters.contractId),
                                                                      new string[] { "rnt_pickupdatetime", "rnt_dropoffdatetime" });
                }
                else
                {
                    ContractItemRepository contractItemRepository = new ContractItemRepository(this.OrgService);
                    contractItem = contractItemRepository.getPriceFactorDifference(new Guid(availabilityParameters.contractId), new string[] { "rnt_pickupdatetime",
                                                                                                                                               "rnt_dropoffdatetime" });
                }

                ContractHelper contractHelper = new ContractHelper(this.OrgService, this.TracingService);
                var resDuration = contractHelper.calculateTotalDuration_ContractByPriceHourEffect_Decimal(contractItem.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                                                                                                          contractItem.GetAttributeValue<DateTime>("rnt_dropoffdatetime"));

                var queryDuration = contractHelper.calculateTotalDuration_ContractByPriceHourEffect_Decimal(availabilityParameters.pickupDateTime,
                                                                                                           availabilityParameters.dropoffDateTime);
                this.Trace("resDuration" + resDuration);
                this.Trace("queryDuration" + queryDuration);
                if (resDuration > queryDuration)
                {
                    return new AvailabilityResponse
                    {
                        //todo langid and xml
                        ResponseResult = ResponseResult.ReturnError("Broker/Acenta rezervasyonları'nın tarihi azaltılamaz.")
                    };
                }

            }
            return new AvailabilityResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }
    }
}
