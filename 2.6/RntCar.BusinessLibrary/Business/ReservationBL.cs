using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using RestSharp;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.BusinessLibrary.Validations;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary.MongoDB;
using RntCar.ClassLibrary.odata;
using RntCar.ClassLibrary.Reservation;
using RntCar.SDK.Common;
using RntCar.SDK.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;



namespace RntCar.BusinessLibrary.Business
{
    public class ReservationBL : BusinessHandler
    {
        public ReservationBL(IOrganizationService orgService) : base(orgService)
        {
        }
        public ReservationBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }
        public ReservationBL(GlobalEnums.ConnectionType enums = GlobalEnums.ConnectionType.default_Service, string UserId = "") : base(enums, UserId)
        {
        }

        public void fillReservationCampaign(Entity reservation)
        {
            ReservationItemRepository reservationItemRepository = new ReservationItemRepository(this.OrgService);
            var resItem = reservationItemRepository.getActiveReservationItemEquipmentByGivenColumns(reservation.Id, new string[] { "rnt_mongodbtrackingnumber", "rnt_groupcodeinformationsid" });

            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            this.Trace("responseUrl :" + responseUrl);
            //request initilization
            RestSharpHelper helper = new RestSharpHelper(responseUrl, "getCampaign", Method.POST);
            this.Trace("resItem" + responseUrl);
            helper.AddQueryParameter("trackingNumber", resItem.GetAttributeValue<string>("rnt_mongodbtrackingnumber"));
            helper.AddQueryParameter("pricingGroupCodeId", resItem.GetAttributeValue<EntityReference>("rnt_groupcodeinformationsid").Id.ToString());

            var campaign = helper.Execute<CampaignAvailability>();


            this.Trace(JsonConvert.SerializeObject(campaign));

            if (!reservation.Contains("rnt_campaignid") && campaign.campaignId != null)
            {
                this.Trace("reservation Id " + reservation.Id);
                this.Trace("rnt_reservationitem Id " + resItem.Id);

                Entity e = new Entity("rnt_reservation");
                e.Id = reservation.Id;
                e["rnt_campaignid"] = new EntityReference("rnt_campaign", new Guid(campaign.campaignId));
                this.OrgService.Update(e);

                Entity e1 = new Entity("rnt_reservationitem");
                e1.Id = resItem.Id;
                e1["rnt_campaignid"] = new EntityReference("rnt_campaign", new Guid(campaign.campaignId));
                this.OrgService.Update(e1);
            }
        }
        public void updateReservationHeader(Guid reservationId,
                                            ReservationCustomerParameters reservationCustomerParameter,
                                            ReservationPriceParameters reservationPriceParameters,
                                            ReservationDateandBranchParameters reservationDateandBranchParameter,
                                            ReservationEquipmentParameters reservationEquipmentParameter,
                                            List<ReservationAdditionalProductParameters> reservationAdditionalProductParameters,
                                            int totalDuration,
                                            int changeType)
        {
            ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
            var r = reservationRepository.getReservationById(reservationId, new string[] { "rnt_pricinggroupcodeid", "rnt_pricingtype", "rnt_corporateid", "rnt_depositamount" });

            GroupCodeInformationBL groupCodeInformationBL = new GroupCodeInformationBL(this.OrgService);
            var groupCodeInformationForDocument = groupCodeInformationBL.getGroupCodeInformationDetailForDocument(reservationEquipmentParameter.groupCodeInformationId);
            if ((changeType == (int)GlobalEnums.GroupCodeChangeType.Upgrade) ||
                (changeType == (int)GlobalEnums.GroupCodeChangeType.Downgrade))
            {
                groupCodeInformationForDocument = groupCodeInformationBL.getGroupCodeInformationDetailForDocument(r.GetAttributeValue<EntityReference>("rnt_pricinggroupcodeid").Id);
            }

            this.Trace("changeType: " + changeType);

            if ((changeType != (int)GlobalEnums.GroupCodeChangeType.Upsell) &&
                (changeType != (int)GlobalEnums.GroupCodeChangeType.Downsell))
            {
                groupCodeInformationForDocument.depositAmount = r.GetAttributeValue<Money>("rnt_depositamount").Value;
            }
            this.Trace("groupCodeInformationForDocument: " + JsonConvert.SerializeObject(groupCodeInformationForDocument));
            CalculateReservationRuleParameters calculateReservationRuleParameters = new CalculateReservationRuleParameters
            {
                corporateId = r.Contains("rnt_corporateid") ? (Guid?)r.GetAttributeValue<EntityReference>("rnt_corporateid").Id : null,
                customerType = reservationCustomerParameter?.customerType,
                pricingType = r.GetAttributeValue<string>("rnt_pricingtype"),
                groupCodeInformationDetailDataForDocument = groupCodeInformationForDocument,
                contactId = reservationCustomerParameter.contactId
            };
            this.Trace("calculateReservationRuleParameters : " + JsonConvert.SerializeObject(calculateReservationRuleParameters));
            var rules = this.calculateReservationRules(calculateReservationRuleParameters);
            this.Trace("rules : " + JsonConvert.SerializeObject(rules));

            Entity reservation = new Entity("rnt_reservation");
            reservation["rnt_dropoffbranchid"] = new EntityReference("rnt_branch", reservationDateandBranchParameter.dropoffBranchId);
            reservation["rnt_pickupbranchid"] = new EntityReference("rnt_branch", reservationDateandBranchParameter.pickupBranchId);
            reservation["rnt_pickupdatetime"] = reservationDateandBranchParameter.pickupDate.AddMinutes(StaticHelper.offset);
            reservation["rnt_dropoffdatetime"] = reservationDateandBranchParameter.dropoffDate.AddMinutes(StaticHelper.offset);
            reservation["rnt_groupcodeid"] = new EntityReference("rnt_groupcodeinformations", reservationEquipmentParameter.groupCodeInformationId);
            reservation["rnt_customerid"] = new EntityReference("contact", reservationCustomerParameter.contactId);
            // if change type eq Upgrade or Downgrade, dont chage the pricing group code
            var _groupCodeId = reservationEquipmentParameter.groupCodeInformationId;
            if ((changeType != (int)GlobalEnums.GroupCodeChangeType.Upgrade) && (changeType != (int)GlobalEnums.GroupCodeChangeType.Downgrade))
            {
                this.Trace("not upgrade or downgrade");
                reservation["rnt_pricinggroupcodeid"] = new EntityReference("rnt_groupcodeinformations", reservationEquipmentParameter.groupCodeInformationId);
                reservation["rnt_depositamount"] = new Money(rules.depositAmount);
            }
            else
            {
                this.Trace("upgrade or downgrade");
                _groupCodeId = r.GetAttributeValue<EntityReference>("rnt_pricinggroupcodeid").Id;
            }
            this.Trace("_groupCodeId" + _groupCodeId);
            reservation["rnt_segmentcode"] = new OptionSetValue(reservationEquipmentParameter.segment);
            reservation["rnt_doublecreditcard"] = groupCodeInformationForDocument.doubleCreditCard;
            reservation["rnt_findeks"] = rules.findeks;
            reservation["rnt_minimumage"] = groupCodeInformationForDocument.minimumAge;
            reservation["rnt_minimumdriverlicience"] = groupCodeInformationForDocument.minimumDriverLicense;
            reservation["rnt_youngdriverage"] = groupCodeInformationForDocument.youngDriverAge;
            reservation["rnt_youngdriverlicence"] = groupCodeInformationForDocument.youngDriverLicense;
            reservation["rnt_overkilometerprice"] = new Money(groupCodeInformationForDocument.overKilometerPrice);
            reservation["rnt_campaignid"] = null;

            if (reservationPriceParameters.campaignId.HasValue)
                reservation["rnt_campaignid"] = new EntityReference("rnt_campaign", reservationPriceParameters.campaignId.Value);

            SystemParameterBL systemParameter = new SystemParameterBL(this.OrgService);
            var reservationRelatedParameters = systemParameter.getReservationRelatedParameters();
            reservation["rnt_cancellationtime"] = reservationDateandBranchParameter.pickupDate.AddMinutes(StaticHelper.offset + reservationRelatedParameters.reservationCancellationDuration);
            reservation["rnt_noshowtime"] = reservationDateandBranchParameter.pickupDate.AddMinutes(StaticHelper.offset + reservationRelatedParameters.reservationNoShowDuration);

            //set kilometer limit
            KilometerLimitBL kilometerLimitBL = new KilometerLimitBL(this.OrgService, this.TracingService);
            var kilometer = kilometerLimitBL.getKilometerLimitForGivenDurationandGroupCode(totalDuration, _groupCodeId);
            this.Trace("before kilometer effect : " + kilometer);
            this.Trace("begin additional");
            var additionalIds = reservationAdditionalProductParameters.Select(x => x.productId).ToList();
            this.Trace("additionalIds: " + JsonConvert.SerializeObject(additionalIds));

            AdditionalProductHelper additionalProductHelper = new AdditionalProductHelper(this.OrgService);
            var kilometerLimitEffect = additionalProductHelper.getAdditionalKilometerByAdditionalIds(additionalIds);
            this.Trace("kilometerLimitEffect: " + kilometerLimitEffect);
            if (kilometerLimitEffect.HasValue)
            {
                kilometer += kilometerLimitEffect.Value;
            }
            this.Trace("kilometer: " + kilometer);
            reservation["rnt_kilometerlimit"] = kilometer;


            if (reservationPriceParameters.isMonthly)
            {
                reservation["rnt_ismonthly"] = true;
                reservation["rnt_howmanymonths"] = new OptionSetValue(reservationPriceParameters.monthValue);
                PaymentPlanBL paymentPlanBL = new PaymentPlanBL(this.OrgService);
                paymentPlanBL.deletePaymentPlansByReservationId(reservationId);
                paymentPlanBL.createPaymentPlans(reservationPriceParameters.paymentPlans, reservationId, null);

            }
            //update deposit for VIP
            IndividualCustomerBL individualCustomerBL = new IndividualCustomerBL(this.OrgService);
            var notchargeDeposit = individualCustomerBL.checkUserISVIPorStaff(reservationCustomerParameter.contactId);
            if (notchargeDeposit)
            {
                reservation["rnt_depositamount"] = new Money(decimal.Zero);
            }
            reservation.Id = reservationId;
            this.OrgService.Update(reservation);
        }
        public Guid decideReservationCurrency(ReservationCustomerParameters reservationCustomerParameters, DateTime pickupDateTime, Guid currencyId)
        {
            if (reservationCustomerParameters.customerType == (int)GlobalEnums.CustomerType.Broker ||
                reservationCustomerParameters.customerType == (int)GlobalEnums.CustomerType.Agency)
            {
                CorporateCustomerRepository corporateCustomerRepository = new CorporateCustomerRepository(this.OrgService);
                var corp = corporateCustomerRepository.getCorporateCustomerById(reservationCustomerParameters.corporateCustomerId.Value, new string[] {"rnt_pricecodeid",
                                                                                                                                                       "rnt_processindividualprices" });
                if (corp.Contains("rnt_processindividualprices") &&
                    !corp.GetAttributeValue<bool>("rnt_processindividualprices"))
                {
                    PriceListRepository priceListRepository = new PriceListRepository(this.OrgService);
                    var priceList = priceListRepository.getPriceListByPriceCodeId(corp.GetAttributeValue<EntityReference>("rnt_pricecodeid").Id,
                                                                                  pickupDateTime,
                                                                                  new string[] { "transactioncurrencyid" });
                    if (priceList != null && priceList.Contains("transactioncurrencyid"))
                        currencyId = priceList.GetAttributeValue<EntityReference>("transactioncurrencyid").Id;
                }
            }
            return currencyId;
        }
        public ReservationCreateResponse createReservation(ReservationCustomerParameters reservationCustomerParameter,
                                                           ReservationDateandBranchParameters reservationDateandBranchParameter,
                                                           ReservationEquipmentParameters reservationEquipmentParameter,
                                                           List<ReservationAdditionalProductParameters> reservationAdditionalProductParameters,
                                                           ReservationPriceParameters reservationPriceParameters,
                                                           ReservationRelatedParameters reservationRelatedParameter,
                                                           GroupCodeInformationDetailDataForDocument groupCodeInformationForDocument,
                                                           Guid transactionCurrencyId,
                                                           int totalDuration,
                                                           string couponCode = null)
        {
            this.Trace("transactionCurrencyId --> create res " + transactionCurrencyId);
            CalculateReservationRuleParameters calculateReservationRuleParameters = new CalculateReservationRuleParameters
            {
                corporateId = reservationCustomerParameter.corporateCustomerId.HasValue ? (Guid?)reservationCustomerParameter.corporateCustomerId.Value : null,
                customerType = reservationCustomerParameter?.customerType,
                pricingType = reservationPriceParameters.pricingType,
                groupCodeInformationDetailDataForDocument = groupCodeInformationForDocument,
                contactId = reservationCustomerParameter.contactId
            };
            this.Trace("calculateReservationRuleParameters : " + JsonConvert.SerializeObject(calculateReservationRuleParameters));
            var rules = this.calculateReservationRules(calculateReservationRuleParameters);
            this.Trace(JsonConvert.SerializeObject(rules));
            var dummyContactId = string.Empty;

            Entity reservation = new Entity("rnt_reservation");
            if (reservationCustomerParameter?.customerType == (int)GlobalEnums.CustomerType.Individual)
            {
                reservation["rnt_customerid"] = new EntityReference("contact", reservationCustomerParameter.contactId);
            }
            else if (reservationCustomerParameter?.customerType == (int)GlobalEnums.CustomerType.Corporate &&
                    reservationCustomerParameter.corporateCustomerId.HasValue)
            {
                reservation["rnt_customerid"] = new EntityReference("contact", reservationCustomerParameter.contactId);
                reservation["rnt_corporateid"] = new EntityReference("account", reservationCustomerParameter.corporateCustomerId.Value);
            }
            else if ((reservationCustomerParameter?.customerType == (int)GlobalEnums.CustomerType.Agency ||
                      reservationCustomerParameter?.customerType == (int)GlobalEnums.CustomerType.Broker) &&
                     reservationCustomerParameter.corporateCustomerId.HasValue)
            {
                reservation["rnt_corporateid"] = new EntityReference("account", reservationCustomerParameter.corporateCustomerId.Value);
                ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
                dummyContactId = configurationBL.GetConfigurationByName("DUMMYCONTACTID_BROKER");
                reservation["rnt_customerid"] = new EntityReference("contact", new Guid(dummyContactId));

                var dummy = JsonConvert.DeserializeObject<DummyContactData>(reservationCustomerParameter.dummyContactId);
                dummy.fullName = dummy.name + " " + dummy.surname;
                reservation["rnt_dummycontactinformation"] = JsonConvert.SerializeObject(dummy);
                reservation["rnt_processindividualprices"] = rules.processCustomerPrices;
            }

            if (reservationPriceParameters.campaignId.HasValue)
            {
                reservation["rnt_campaignid"] = new EntityReference("rnt_campaign", reservationPriceParameters.campaignId.Value);
            }

            reservation["rnt_couponcode"] = couponCode;
            reservation["rnt_dropoffbranchid"] = new EntityReference("rnt_branch", reservationDateandBranchParameter.dropoffBranchId);
            reservation["rnt_pickupbranchid"] = new EntityReference("rnt_branch", reservationDateandBranchParameter.pickupBranchId);
            reservation["rnt_pickupdatetime"] = reservationDateandBranchParameter.pickupDate.AddMinutes(StaticHelper.offset);
            reservation["rnt_dropoffdatetime"] = reservationDateandBranchParameter.dropoffDate.AddMinutes(StaticHelper.offset);
            reservation["rnt_offset"] = StaticHelper.offset;
            reservation["rnt_groupcodeid"] = new EntityReference("rnt_groupcodeinformations", reservationEquipmentParameter.groupCodeInformationId);
            reservation["rnt_pricinggroupcodeid"] = new EntityReference("rnt_groupcodeinformations", reservationEquipmentParameter.groupCodeInformationId);
            reservation["rnt_segmentcode"] = new OptionSetValue(reservationEquipmentParameter.segment);
            reservation["rnt_reservationchannelcode"] = new OptionSetValue(reservationRelatedParameter.reservationChannel);
            reservation["rnt_reservationtypecode"] = new OptionSetValue(reservationRelatedParameter.reservationTypeCode);
            reservation["transactioncurrencyid"] = new EntityReference("transactioncurrency", transactionCurrencyId);
            reservation["rnt_paymentchoicecode"] = new OptionSetValue(reservationPriceParameters.paymentType);
            var autoNum = StaticHelper.RandomDigits(1) + StaticHelper.GenerateString(1) + StaticHelper.RandomDigits(1) + StaticHelper.GenerateString(1) + StaticHelper.RandomDigits(1) + StaticHelper.GenerateString(1);
            autoNum += StaticHelper.RandomDigits(1);
            autoNum += StaticHelper.RandomDigits(1);
            reservation["rnt_pnrnumber"] = autoNum;
            //reservation["rnt_name"] = autoNum;

            reservation["rnt_doublecreditcard"] = groupCodeInformationForDocument.doubleCreditCard;
            reservation["rnt_minimumage"] = groupCodeInformationForDocument.minimumAge;
            reservation["rnt_minimumdriverlicience"] = groupCodeInformationForDocument.minimumDriverLicense;
            reservation["rnt_youngdriverage"] = groupCodeInformationForDocument.youngDriverAge;
            reservation["rnt_youngdriverlicence"] = groupCodeInformationForDocument.youngDriverLicense;
            reservation["rnt_overkilometerprice"] = new Money(groupCodeInformationForDocument.overKilometerPrice);

            SystemParameterBL systemParameter = new SystemParameterBL(this.OrgService);
            var reservationRelatedParameters = systemParameter.getReservationRelatedParameters();
            reservation["rnt_cancellationtime"] = reservationDateandBranchParameter.pickupDate.AddMinutes(StaticHelper.offset + reservationRelatedParameters.reservationCancellationDuration);
            reservation["rnt_noshowtime"] = reservationDateandBranchParameter.pickupDate.AddMinutes(StaticHelper.offset + reservationRelatedParameters.reservationNoShowDuration);

            //set kilometer limit
            KilometerLimitBL kilometerLimitBL = new KilometerLimitBL(this.OrgService, this.TracingService);
            var kilometer = kilometerLimitBL.getKilometerLimitForGivenDurationandGroupCode(totalDuration, reservationEquipmentParameter.groupCodeInformationId);
            this.Trace("begin set kilometer");
            if (kilometer == 0)
            {
                //todo will read from xml
                throw new Exception("Kilometer limit not found");
            }
            if (reservationAdditionalProductParameters != null)
            {
                this.Trace("begin additional");
                var additionalIds = reservationAdditionalProductParameters.Select(x => x.productId).ToList();
                this.Trace("additionalIds: " + JsonConvert.SerializeObject(additionalIds));

                AdditionalProductHelper additionalProductHelper = new AdditionalProductHelper(this.OrgService);
                var kilometerLimitEffect = additionalProductHelper.getAdditionalKilometerByAdditionalIds(additionalIds);
                this.Trace("kilometerLimitEffect: " + kilometerLimitEffect);
                if (kilometerLimitEffect.HasValue)
                {
                    kilometer += kilometerLimitEffect.Value;
                }
            }

            reservation["rnt_kilometerlimit"] = kilometer;

            // for individual pricing type
            reservation["rnt_pricingtype"] = reservationPriceParameters.pricingType;

            reservation["rnt_paymentmethodcode"] = new OptionSetValue(rules.paymentMethodCode);
            reservation["rnt_depositamount"] = new Money(rules.depositAmount);
            reservation["rnt_findeks"] = rules.findeks;

            if (reservationCustomerParameter?.customerType == (int)GlobalEnums.CustomerType.Agency ||
              reservationCustomerParameter?.customerType == (int)GlobalEnums.CustomerType.Broker)
            {
                if (!string.IsNullOrEmpty(reservationCustomerParameter.dummyContactId))
                {
                    var dummyDataSerialized = JsonConvert.DeserializeObject<DummyContactData>(reservationCustomerParameter.dummyContactId);
                    reservation["rnt_referencenumber"] = dummyDataSerialized.referenceNumber;
                }
            }
            if (reservationPriceParameters.isMonthly)
            {
                reservation["rnt_ismonthly"] = true;
                reservation["rnt_howmanymonths"] = new OptionSetValue(reservationPriceParameters.monthValue);
            }

            this.Trace("before create reservation");
            if (reservationCustomerParameter.contactId != Guid.Empty)
            {
                IndividualCustomerBL individualCustomerBL = new IndividualCustomerBL(this.OrgService);
                var notchargeDeposit = individualCustomerBL.checkUserISVIPorStaff(reservationCustomerParameter.contactId);
                if (notchargeDeposit)
                {
                    reservation["rnt_depositamount"] = new Money(decimal.Zero);
                }
            }


            var _id = this.OrgService.Create(reservation);
            if (reservationPriceParameters.isMonthly)
            {
                PaymentPlanBL paymentPlanBL = new PaymentPlanBL(this.OrgService);
                paymentPlanBL.createPaymentPlans(reservationPriceParameters.paymentPlans, _id, null);

            }
            return new ReservationCreateResponse
            {
                reservationId = _id,
                pnrNumber = autoNum,
                dummyContactId = dummyContactId
            };


        }

        public ReservationUpdateResponse updateReservation(ReservationCustomerParameters reservationCustomerParameter,
                                                           ReservationDateandBranchParameters reservationDateandBranchParameter,
                                                           ReservationEquipmentParameters reservationEquipmentParameter,
                                                           ReservationPriceParameters reservationPriceParameters,
                                                           ReservationRelatedParameters reservationRelatedParameter,
                                                           List<ReservationAdditionalProductParameters> reservationAdditionalProductParameter,
                                                           Guid transactionCurrencyId,
                                                           int totalDuration,
                                                           string trackingNumber,
                                                           int changeType,
                                                           bool isContract)
        {

            //first collect all active reservationItems , we will use them everywhere
            ReservationItemRepository reservationItemRepository = new ReservationItemRepository(this.OrgService);
            var reservationItems = reservationItemRepository.getActiveReservationItemsByReservationId(reservationRelatedParameter.reservationId);
            var corpItem = reservationItemRepository.getReservationEquipmentCorporate(reservationRelatedParameter.reservationId,
                                                                                        new string[] {"rnt_totalamount",
                                                                                                      "rnt_pickupdatetime",
                                                                                                      "rnt_dropoffdatetime"});
            //this.Trace("ReservationItemRepository " + reservationItems.Count);
            var reservationEquipmentItem = reservationItems.Where(p => p.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)GlobalEnums.ItemTypeCode.Equipment).FirstOrDefault();
            //decide dates or branchs changed
            var isDateorBranchChanged = CommonHelper.isDateorBranchChanged(reservationEquipmentItem.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                                                                           reservationEquipmentItem.GetAttributeValue<DateTime>("rnt_dropoffdatetime"),
                                                                           reservationEquipmentItem.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id,
                                                                           reservationEquipmentItem.GetAttributeValue<EntityReference>("rnt_dropoffbranchid").Id,
                                                                           reservationDateandBranchParameter.pickupDate.AddMinutes(StaticHelper.offset),
                                                                           reservationDateandBranchParameter.dropoffDate.AddMinutes(StaticHelper.offset),
                                                                           reservationDateandBranchParameter.pickupBranchId,
                                                                           reservationDateandBranchParameter.dropoffBranchId);
            //set lastItem into globalObject 
            //var _itemNo = Convert.ToInt32(reservationItems.Last().GetAttributeValue<string>("rnt_itemno")) + 10;
            //check group code repository

            this.Trace("isDateorBranchChanged " + isDateorBranchChanged);
            ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
            var reservation = reservationRepository.getReservationById(reservationRelatedParameter.reservationId, new string[] { "rnt_paymentmethodcode",
                                                                                                                                 "rnt_corporatetotalamount",
                                                                                                                                 "rnt_groupcodeid",
                                                                                                                                 "rnt_pnrnumber",
                                                                                                                                 "rnt_paymentmethodcode",
                                                                                                                                 "transactioncurrencyid"});
            //decide that group code changes. If it is , deactivate current reservationItem and create a new one
            if (reservation.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id != reservationEquipmentParameter.groupCodeInformationId)
            {
                #region group code changes operations 
                var columns = new string[] { "rnt_campaignid" };
                var reservationItem = reservationItemRepository.getActiveReservationItemEquipmentByGivenColumns(reservationRelatedParameter.reservationId, columns);
                //deactivate current one
                ReservationItemBL reservationItemBL = new ReservationItemBL(this.OrgService, this.TracingService);

                reservationItemBL.updateChangedEquipmentChangeTypeandChangeReason(reservationItem.Id, changeType);
                reservationItemBL.deactiveReservationItemById(reservationItem.Id, reservationRelatedParameter.statusCode);

                var reservationItemId = reservationItemBL.createReservationItemForEquipment(reservationCustomerParameter,
                                                                                            reservationDateandBranchParameter,
                                                                                            reservationEquipmentParameter,
                                                                                            reservationPriceParameters,
                                                                                            reservation.Id,
                                                                                            transactionCurrencyId,
                                                                                            totalDuration,
                                                                                            trackingNumber,
                                                                                            string.Empty,
                                                                                            reservationItem.GetAttributeValue<EntityReference>("rnt_campaignid"),
                                                                                            changeType,
                                                                                            reservationRelatedParameter.reservationChannel);
                if (reservation.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value == (int)rnt_PaymentMethodCode.PayBroker)
                {
                    Entity entity = new Entity("rnt_reservationitem");
                    entity.Id = reservationItemId;
                    entity["rnt_billingtypecode"] = new OptionSetValue((int)rnt_BillingTypeCode.Individual);
                    this.OrgService.Update(entity);
                }
                #endregion
            }
            //update current reservation item equipment
            else
            {
                ReservationItemBL reservationItemBL = new ReservationItemBL(this.OrgService, this.TracingService);
                reservationItemBL.UpdateReservationItemEquipment(reservationEquipmentItem.Id,
                                                                 reservationCustomerParameter,
                                                                 reservationDateandBranchParameter,
                                                                 reservationEquipmentParameter,
                                                                 reservationPriceParameters,
                                                                 totalDuration,
                                                                 trackingNumber,
                                                                 isDateorBranchChanged,
                                                                 isContract);
                if (reservation.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value == (int)rnt_PaymentMethodCode.PayBroker)
                {
                    Entity entity = new Entity("rnt_reservationitem");
                    entity.Id = reservationEquipmentItem.Id;
                    entity["rnt_billingtypecode"] = new OptionSetValue((int)rnt_BillingTypeCode.Individual);
                    this.OrgService.Update(entity);
                }
            }

            //paybrokerda corporate kalemini iade kalemi olarak ata
            if (reservation.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value == (int)rnt_PaymentMethodCode.PayBroker)
            {
                this.Trace("pay broker");

                this.Trace("corpItem ID" + corpItem.Id);
                this.Trace("corpItem totalamoun" + corpItem.GetAttributeValue<Money>("rnt_totalamount").Value);
                ReservationItemBL reservationItemBL = new ReservationItemBL(this.OrgService);
                var _c = new ReservationDateandBranchParameters();
                _c.Map(reservationDateandBranchParameter);
                _c.pickupDate = corpItem.GetAttributeValue<DateTime>("rnt_pickupdatetime").AddMinutes(-StaticHelper.offset);
                _c.dropoffDate = corpItem.GetAttributeValue<DateTime>("rnt_dropoffdatetime").AddMinutes(-StaticHelper.offset);
                var _r = new ReservationAdditionalProductParameters();
                this.Trace("before configuration");
                ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
                var differenceAdditionalCode = configurationBL.GetConfigurationByName("additionalProduct_priceDifference");

                AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);
                var additionalProduct = additionalProductRepository.getAdditionalProductByProductCode(differenceAdditionalCode);

                _r.productId = additionalProduct.Id;
                _r.productCode = additionalProduct.GetAttributeValue<string>("rnt_additionalproductcode");
                _r.actualAmount = corpItem.GetAttributeValue<Money>("rnt_totalamount").Value * -1;
                _r.actualTotalAmount = corpItem.GetAttributeValue<Money>("rnt_totalamount").Value * -1;
                this.Trace("before create createReservationItemForAdditionalProduct");
                var _id = reservationItemBL.createReservationItemForAdditionalProduct(reservationCustomerParameter,
                                                                                      _c,
                                                                                      reservationEquipmentParameter,
                                                                                      _r,
                                                                                      reservationRelatedParameter.reservationId,
                                                                                      reservation.GetAttributeValue<EntityReference>("transactioncurrencyid").Id,
                                                                                      totalDuration,
                                                                                      reservationPriceParameters.paymentType,
                                                                                      (int)rnt_reservationitem_rnt_itemtypecode.PriceDifference,
                                                                                      reservationRelatedParameter.reservationChannel,
                                                                                      ((int)rnt_PaymentMethodCode.PayBroker).ToString());

                Entity entity = new Entity("rnt_reservationitem");
                entity.Id = _id;
                entity["rnt_billingtypecode"] = new OptionSetValue((int)rnt_BillingTypeCode.Corporate);
                entity["statuscode"] = new OptionSetValue((int)rnt_reservationitem_StatusCode.Completed);
                entity["rnt_totalamount"] = new Money(_r.actualAmount.Value);
                entity["rnt_name"] = "Broker ödemeli araç fiyatı";
                this.OrgService.Update(entity);
            }

            var existingAdditionalProducts = new List<Entity>();
            existingAdditionalProducts.AddRange(reservationItems.Where(p => p.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)GlobalEnums.ItemTypeCode.AdditionalProduct).ToList());
            //reservationAdditionalProductParameter doesnt include removed ones

            var itemTypeCode = (int)RntCar.ClassLibrary._Enums_1033.rnt_reservationitem_rnt_itemtypecode.AdditionalProduct;

            foreach (var item in reservationAdditionalProductParameter)
            {
                var producttobeCheck = existingAdditionalProducts.Where(p => p.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id.Equals(item.productId)).ToList();
                //if producttobeCheck length == 0 , means we dont have this product before, so we need to add.
                //tobe careful if it is maxpieces == 1 or higher
                //because maxpieces > 1 different reservation items in crm
                if (producttobeCheck.Count == 0)
                {
                    this.Trace("producttobeCheck.Count == 0");
                    #region Create Additional Product

                    if (item.maxPieces == 1)
                    {
                        //this.Trace("item.MaxPieces == 1 create");
                        ReservationItemBL reservationItemBL = new ReservationItemBL(this.OrgService, this.TracingService);
                        reservationItemBL.createReservationItemForAdditionalProduct(reservationCustomerParameter,
                                                                                    reservationDateandBranchParameter,
                                                                                    reservationEquipmentParameter,
                                                                                    item,
                                                                                    reservationRelatedParameter.reservationId,
                                                                                    transactionCurrencyId,
                                                                                    totalDuration,
                                                                                    reservationPriceParameters.paymentType,
                                                                                    itemTypeCode,
                                                                                    reservationRelatedParameter.reservationChannel,
                                                                                    reservationPriceParameters.pricingType);
                    }
                    else if (item.maxPieces > 1)
                    {

                        for (int i = 0; i < item.value; i++)
                        {
                            this.Trace("item.MaxPieces> 1 create");

                            ReservationItemBL reservationItemBL = new ReservationItemBL(this.OrgService, this.TracingService);
                            reservationItemBL.createReservationItemForAdditionalProduct(reservationCustomerParameter,
                                                                                        reservationDateandBranchParameter,
                                                                                        reservationEquipmentParameter,
                                                                                        item,
                                                                                        reservationRelatedParameter.reservationId,
                                                                                        transactionCurrencyId,
                                                                                        totalDuration,
                                                                                        reservationPriceParameters.paymentType,
                                                                                        itemTypeCode,
                                                                                        reservationRelatedParameter.reservationChannel,
                                                                                        reservationPriceParameters.pricingType);
                        }
                    }
                    #endregion
                }
                //if producttobeCheck length == 1 , means we found only one item in crm , create or update operation will decide by maxpieces = 1 or > 1
                else if (producttobeCheck.Count == 1)
                {
                    this.Trace("producttobeCheck.Count == 1");
                    this.Trace("item.name: " + item.productName);

                    //check its max pieces == 1
                    if (item.maxPieces == 1)
                    {
                        this.Trace("item.maxPieces == 1");
                        this.Trace("item.name: " + item.productName);

                        #region Update existing reservation item with the new date fields
                        //just update the reservation item with the new date fields.
                        //todo check the dates are changed.if not avoid from unneccessary update
                        ReservationItemBL reservationItemBL = new ReservationItemBL(this.OrgService, this.TracingService);
                        var response = reservationItemBL.UpdateReservationItemAdditionalProductAmountandDateBranchFields(producttobeCheck.FirstOrDefault().Id,
                                                                                                                         reservationCustomerParameter,
                                                                                                                         reservationDateandBranchParameter,
                                                                                                                         item,
                                                                                                                         reservationRelatedParameter.reservationId,
                                                                                                                         reservationEquipmentParameter,
                                                                                                                         totalDuration,
                                                                                                                         reservationPriceParameters.paymentType,
                                                                                                                         isDateorBranchChanged,
                                                                                                                         isContract);


                        #endregion
                    }
                    else if (item.maxPieces > 1)
                    {
                        //check max pieces added more 
                        //this scenerio is user didnt add any more item just update the date fields is enough
                        if (item.value == 1)
                        {
                            this.Trace("item.value == 1");
                            this.Trace("item.name: " + item.productName);

                            #region Update existing reservation item with the new date fields
                            //just update the reservation item with the new date fields.
                            //todo check the dates are changed.if not avoid from unneccessary update

                            ReservationItemBL reservationItemBL = new ReservationItemBL(this.OrgService, this.TracingService);
                            reservationItemBL.UpdateReservationItemAdditionalProductAmountandDateBranchFields(producttobeCheck.FirstOrDefault().Id,
                                                                                                              reservationCustomerParameter,
                                                                                                              reservationDateandBranchParameter,
                                                                                                              item,
                                                                                                              reservationRelatedParameter.reservationId,
                                                                                                              reservationEquipmentParameter,
                                                                                                              totalDuration,
                                                                                                              reservationPriceParameters.paymentType,
                                                                                                              isDateorBranchChanged,
                                                                                                              isContract);

                            #endregion
                        }
                        else if (item.value > 1)
                        {
                            this.Trace("item.value > 1");
                            this.Trace("item.name: " + item.productName);

                            // we need to create just by the difference( -1 means removing existing reservationitem with the same productId type)
                            for (int i = 0; i < item.value - 1; i++)
                            {
                                #region Create Additional Product
                                ReservationItemBL reservationItemBL = new ReservationItemBL(this.OrgService, this.TracingService);
                                reservationItemBL.createReservationItemForAdditionalProduct(reservationCustomerParameter,
                                                                                            reservationDateandBranchParameter,
                                                                                            reservationEquipmentParameter,
                                                                                            item,
                                                                                            reservationRelatedParameter.reservationId,
                                                                                            transactionCurrencyId,
                                                                                            totalDuration,
                                                                                            reservationPriceParameters.paymentType,
                                                                                            itemTypeCode,
                                                                                            reservationRelatedParameter.reservationChannel,
                                                                                            reservationPriceParameters.pricingType);

                                reservationItemBL.UpdateReservationItemAdditionalProductAmountandDateBranchFields(producttobeCheck.FirstOrDefault().Id,
                                                                                                              reservationCustomerParameter,
                                                                                                              reservationDateandBranchParameter,
                                                                                                              item,
                                                                                                              reservationRelatedParameter.reservationId,
                                                                                                              reservationEquipmentParameter,
                                                                                                              totalDuration,
                                                                                                              reservationPriceParameters.paymentType,
                                                                                                              isDateorBranchChanged,
                                                                                                              isContract);
                                #endregion
                            }
                        }
                    }
                }
                //if we found more than one more , it is definetely maxpieces > 1 typeof additional product
                //just need to check the counts
                else if (producttobeCheck.Count > 1)
                {
                    this.Trace("producttobeCheck.Count > 1");
                    this.Trace("item.name: " + item.productName);

                    //nothing added just update reservation items with new date fields
                    if (producttobeCheck.Count == item.value)
                    {
                        this.Trace("producttobeCheck.Count == item.Value");
                        this.Trace("item.name: " + item.productName);

                        foreach (var sameProduct in producttobeCheck)
                        {
                            #region Update existing reservation item with the new date fields
                            //just update the reservation item with the new date fields.
                            //todo check the dates are changed.if not avoid from unneccessary update

                            this.Trace("UpdateReservationItemAdditionalProductAmountandDateBranchFields");
                            ReservationItemBL reservationItemBL = new ReservationItemBL(this.OrgService, this.TracingService);
                            reservationItemBL.UpdateReservationItemAdditionalProductAmountandDateBranchFields(sameProduct.Id,
                                                                                                              reservationCustomerParameter,
                                                                                                              reservationDateandBranchParameter,
                                                                                                              item,
                                                                                                              reservationRelatedParameter.reservationId,
                                                                                                              reservationEquipmentParameter,
                                                                                                              totalDuration,
                                                                                                              reservationPriceParameters.paymentType,
                                                                                                              isDateorBranchChanged,
                                                                                                              isContract);

                            #endregion
                        }


                    }
                    //some products are removed so need to deactivate them
                    else if (producttobeCheck.Count > item.value)
                    {
                        this.Trace("producttobeCheck.Count > item.Value");
                        this.Trace("item.name: " + item.productName);

                        var diff = producttobeCheck.Count - item.value;
                        #region Need to deactivate the 
                        for (int j = 0; j < diff; j++)
                        {
                            ReservationItemBL reservationItemBL = new ReservationItemBL(this.OrgService, this.TracingService);
                            if (!isContract)
                            {
                                reservationItemBL.updateReservationItemChangeReason(producttobeCheck[j].Id, (int)ReservationItemEnums.ChangeReason.CustomerDemand);
                            }

                            reservationItemBL.deactiveReservationItemById(producttobeCheck[j].Id, (int)ReservationItemEnums.StatusCode.CustomerDemand);
                        }
                        #endregion
                    }
                    else if (producttobeCheck.Count < item.value)
                    {
                        this.Trace("producttobeCheck.Count < item.Value");
                        this.Trace("item.name: " + item.productName);

                        var diff = item.value - producttobeCheck.Count;
                        for (int i = 0; i < diff; i++)
                        {
                            //this.Trace("createReservationItemForAdditionalProduct");

                            #region Create Additional Product
                            ReservationItemBL reservationItemBL = new ReservationItemBL(this.OrgService, this.TracingService);
                            reservationItemBL.createReservationItemForAdditionalProduct(reservationCustomerParameter,
                                                                                        reservationDateandBranchParameter,
                                                                                        reservationEquipmentParameter,
                                                                                        item,
                                                                                        reservationRelatedParameter.reservationId,
                                                                                        transactionCurrencyId,
                                                                                        totalDuration,
                                                                                        reservationPriceParameters.paymentType,
                                                                                        itemTypeCode,
                                                                                        reservationRelatedParameter.reservationChannel,
                                                                                        reservationPriceParameters.pricingType);
                            #endregion
                        }

                        foreach (var sameProduct in producttobeCheck)
                        {
                            //just update the reservation item with the new date fields.
                            //todo check the dates are changed.if not avoid from unneccessary update
                            ReservationItemBL reservationItemBL = new ReservationItemBL(this.OrgService, this.TracingService);
                            reservationItemBL.UpdateReservationItemAdditionalProductAmountandDateBranchFields(sameProduct.Id,
                                                                                                              reservationCustomerParameter,
                                                                                                              reservationDateandBranchParameter,
                                                                                                              item,
                                                                                                              reservationRelatedParameter.reservationId,
                                                                                                              reservationEquipmentParameter,
                                                                                                              totalDuration,
                                                                                                              reservationPriceParameters.paymentType,
                                                                                                              isDateorBranchChanged,
                                                                                                              isContract);
                        }
                    }
                }

                existingAdditionalProducts.RemoveAll(p => p.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id.Equals(item.productId));
            }
            //if we still some values on the list lets deactivate them
            foreach (var item in existingAdditionalProducts)
            {
                ReservationItemBL reservationItemBL = new ReservationItemBL(this.OrgService, this.TracingService);
                //todo status will decide
                if (!isContract)
                {
                    reservationItemBL.updateReservationItemChangeReason(item.Id, (int)ReservationItemEnums.ChangeReason.CustomerDemand);
                }
                reservationItemBL.deactiveReservationItemById(item.Id, (int)ReservationItemEnums.StatusCode.CustomerDemand);
            }

            //also check removed ones
            //update reservation header finally;
            this.updateReservationHeader(reservation.Id,
                                        reservationCustomerParameter,
                                        reservationPriceParameters,
                                        reservationDateandBranchParameter,
                                        reservationEquipmentParameter,
                                        reservationAdditionalProductParameter,
                                        totalDuration,
                                        changeType);

            return new ReservationUpdateResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess(),
                reservationId = reservation.Id,
                pnrNumber = reservation.GetAttributeValue<string>("rnt_pnrnumber"),
                corporateAmount = corpItem == null ? decimal.Zero : corpItem.GetAttributeValue<Money>("rnt_totalamount").Value
            };
        }

        public ReservationSearchResponse searchReservationByParameters(ReservationSearchParameters searchParameters, int langId)
        {
            try
            {
                ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
                if (searchParameters.pickupDate.HasValue)
                    searchParameters.pickupDate = searchParameters.pickupDate.Value.AddMinutes(StaticHelper.offset);
                if (searchParameters.dropoffDate.HasValue)
                    searchParameters.dropoffDate = searchParameters.dropoffDate.Value.AddMinutes(StaticHelper.offset);

                this.Trace("params : " + JsonConvert.SerializeObject(searchParameters));
                this.Trace("langId : " + langId);
                var data = reservationRepository.getReservationsBySearchParameters(searchParameters, langId);
                return new ReservationSearchResponse
                {
                    ResponseResult = ResponseResult.ReturnSuccess(),
                    ReservationData = data
                };

            }
            catch (Exception ex)
            {
                //todo will replace more user friendly message
                return new ReservationSearchResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }


        public ReservationCancellationResponse checkBeforeReservationCancellation(ReservationCancellationParameters reservationCancellationParameter,
                                                                                  int langId)
        {
            SystemParameterBL systemParameterBL = new SystemParameterBL(this.OrgService);
            var reservationRelatedParameters = systemParameterBL.getReservationRelatedParameters();

            var parameters = reservationCancellationParameter;

            ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
            var columns = new string[] { "rnt_reservationnumber",
                                         "rnt_pnrnumber",
                                         "statuscode",
                                         "statecode",
                                         "rnt_pickupdatetime",
                                         "rnt_paymentchoicecode" ,
                                         "rnt_reservationtypecode",
                                         "rnt_paymentmethodcode",
                                         "rnt_totalamount"};
            var entity = reservationRepository.getReservationByPnrNumberByGivenColumns(parameters.pnrNumber, columns);
            if (entity == null)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);

                var message = xrmHelper.GetXmlTagContentByGivenLangId("NotExistedPnr", langId);
                return new ReservationCancellationResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }
            var result = true;
            ReservationCancellationValidation reservationCancellationValidation = new ReservationCancellationValidation(this.OrgService, this.TracingService);
            result = reservationCancellationValidation.checkPNRandReservationNumber(entity);
            if (!result)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);

                var message = xrmHelper.GetXmlTagContentByGivenLangId("NullPnrorReservationNumber", langId);
                return new ReservationCancellationResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }

            result = reservationCancellationValidation.checkReservationStatus(entity);
            if (!result)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);

                var message = xrmHelper.GetXmlTagContentByGivenLangId("UnavailableStatusReservation", langId, this.reservationXmlPath);
                return new ReservationCancellationResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }

            result = reservationCancellationValidation.checkReservationIsActive(entity);
            if (!result)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);

                var message = xrmHelper.GetXmlTagContentByGivenLangId("AlreadyInactiveReservation", langId);
                return new ReservationCancellationResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }
            var _totalAmount = decimal.Zero;

            ReservationItemRepository reservationItemRepository = new ReservationItemRepository(this.OrgService);
            var equipment = reservationItemRepository.getActiveReservationItemEquipmentByGivenColumns(entity.Id, new string[] { "rnt_campaignid" });
            bool isCampaignCancelable = true;
            if (equipment.Attributes.Contains("rnt_campaignid"))
            {
                CampaignRepository campaignRepository = new CampaignRepository(this.OrgService);
                var campaign = campaignRepository.getCampaignById(equipment.GetAttributeValue<EntityReference>("rnt_campaignid").Id, new string[] { "rnt_iscancelable" });
                isCampaignCancelable = campaign.GetAttributeValue<bool>("rnt_iscancelable");
            }

            this.Trace("isCampaignCancelable: " + isCampaignCancelable);

            ContractHelper contractHelper = new ContractHelper(this.OrgService);
            var isCorpReservation = contractHelper.checkCorporateInCancellation(entity.GetAttributeValue<OptionSetValue>("rnt_reservationtypecode").Value,
                                                                                       entity.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value);
            this.Trace("isCorpReservation : " + isCorpReservation);
            if (isCorpReservation)
            {
                PaymentRepository paymentRepository = new PaymentRepository(this.OrgService);
                var payments = paymentRepository.getNotRefundedSalePayment_Reservation(parameters.reservationId);
                _totalAmount = payments.Sum(p => p.GetAttributeValue<Money>("rnt_amount").Value);
            }
            else
            {
                _totalAmount = (decimal)entity.GetAttributeValue<Money>("rnt_totalamount")?.Value;
            }

            var _discountAmount = decimal.Zero;//this.getReservationManuelDiscountAmount(parameters.reservationId);

            this.Trace("_totalAmount" + _totalAmount);
            return new ReservationCancellationResponse
            {
                totalAmount = _totalAmount,
                discountAmount = _discountAmount,
                ResponseResult = ResponseResult.ReturnSuccess(),
                isCorporateReservation = isCorpReservation,
                isCampaignCancelable = isCampaignCancelable,
                reservationPaymetType = entity.GetAttributeValue<OptionSetValue>("rnt_paymentchoicecode").Value,
                willChargeFromUser = reservationCancellationValidation.checkReservationWillChargeFromUser(reservationRelatedParameters.reservationCancellationFineDuration,
                                                                                                          entity)
            };
        }

        public ReservationFineAmountResponse calculateFineAmountForGivenReservationFromMongoDB(Guid reservationId, int langId)
        {
            ReservationItemRepository reservationItemRepository = new ReservationItemRepository(this.OrgService);
            var activeReservationItem = reservationItemRepository.getNewNoShowReservationItemEquipmentByGivenColumns(reservationId, new string[] { });
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
            //request initilization
            RestSharpHelper helper = new RestSharpHelper(responseUrl, "getFineAmountforReservation", Method.POST);
            //prepare parameters
            helper.AddQueryParameter("reservationItemId", Convert.ToString(activeReservationItem.Id));
            // execute the request
            var response = helper.Execute<ReservationFineAmountResponse>();
            if (!response.ResponseResult.Result)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                response.ResponseResult = ResponseResult.ReturnError(xrmHelper.GetXmlTagContentByGivenLangId("CannotFindFirstDayPrice", langId));
            }
            return response;
            //check response
        }

        public void cancelReservation(string reservationId, int statusCode, decimal fineAmount, bool isCorpReservation, int cancelSubReason)
        {
            ReservationItemRepository reservationItemRepository = new ReservationItemRepository(this.OrgService);
            var reservationItems = reservationItemRepository.getNewNoShowReservationItemsByReservationId(new Guid(reservationId));
            if (fineAmount != decimal.Zero)
            {
                Guid? invoiceId = null;
                // add 
                InvoiceRepository invoiceRepository = new InvoiceRepository(this.OrgService);
                var invoice = invoiceRepository.getInvoiceByReservationId(Guid.Parse(reservationId));
                if (invoice != null)
                    invoiceId = invoice.Id;

                ReservationItemBL reservationItemBL = new ReservationItemBL(this.OrgService);
                reservationItemBL.createAdditionalProductForCancelReservationFromReservationItemWithInitializeFromRequest(reservationItems.FirstOrDefault().Id, invoiceId, fineAmount);
            }
            bool existsIgnoreCancelAdditionalProduct = false;
            var ignoreCancelAdditionalProdcutIdList = StaticHelper.ignoreCanceAdditionalProdcutIds.Split(';').ToList();
            ignoreCancelAdditionalProdcutIdList.RemoveAll(item => item == "");

            foreach (var item in reservationItems)
            {
                XrmHelper h = new XrmHelper(this.OrgService);

                var additionalProductEntityRef = item.GetAttributeValue<EntityReference>("rnt_additionalproductid");
                if (additionalProductEntityRef != null)
                {
                    Entity additionalProductEntity = OrgService.Retrieve(additionalProductEntityRef.LogicalName, additionalProductEntityRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("rnt_additionalproductcode"));
                    var additionalProductCode = additionalProductEntity.GetAttributeValue<string>("rnt_additionalproductcode");
                    if (ignoreCancelAdditionalProdcutIdList.Contains(additionalProductCode))
                    {
                        existsIgnoreCancelAdditionalProduct = true;
                        h.setState("rnt_reservationitem", item.Id, (int)GlobalEnums.StateCode.Active, (int)rnt_reservationitem_StatusCode.Completed);
                    }
                    else
                    {
                        existsIgnoreCancelAdditionalProduct = false;
                    }

                }

                if (!existsIgnoreCancelAdditionalProduct)
                {
                    if (statusCode == (int)ReservationEnums.CancellationReason.ByRentgo)
                        h.setState("rnt_reservationitem", item.Id, (int)GlobalEnums.StateCode.Active, (int)rnt_reservationitem_StatusCode.CancelledByRentgo);
                    else if (statusCode == (int)ReservationEnums.CancellationReason.ByCustomer)
                        h.setState("rnt_reservationitem", item.Id, (int)GlobalEnums.StateCode.Active, (int)rnt_reservationitem_StatusCode.CancelledByCustomer);
                }

            }
            //now deactivate header
            XrmHelper h1 = new XrmHelper(this.OrgService);
            Entity entity = new Entity("rnt_reservation", new Guid(reservationId));
            entity.Attributes["rnt_reservationcancelreasons"] = new OptionSetValue(cancelSubReason);
            h1.IOrganizationService.Update(entity);
            h1.setState("rnt_reservation", new Guid(reservationId), (int)GlobalEnums.StateCode.Active, statusCode);

        }

        public ReservationCancellationResponse calculateCancellationAmountForGivenReservationByCancellationReason(ReservationCancellationResponse validationResponse,
                                                                                                                  Guid reservationId,
                                                                                                                  bool willChargeFromUser,
                                                                                                                  int langId,
                                                                                                                  int cancellationReason)
        {
            PaymentRepository paymentRepository = new PaymentRepository(this.OrgService);
            var payments = paymentRepository.getCountofNotRefundedPayments_Reservation(reservationId);
            ContractHelper contractHelper = new ContractHelper(this.OrgService);

            //check reservation has payment record and 
            //reservation start date is close enough to check fine amount
            if (payments > 0)
            {
                this.Trace("validationResponse.willChargeFromUser " + willChargeFromUser);
                this.Trace("cancellationReason == (int)ReservationEnums.CancellationReason.ByCustomer" + (cancellationReason == (int)ReservationEnums.CancellationReason.ByCustomer ? true : false));

                if (validationResponse.willChargeFromUser && cancellationReason == (int)ReservationEnums.CancellationReason.ByCustomer && !validationResponse.isCorporateReservation)
                {
                    var fineResponse = this.calculateFineAmountForGivenReservationFromMongoDB(reservationId, langId);

                    //if first day price couldnt found into system no need to continue
                    if (!fineResponse.ResponseResult.Result)
                    {
                        validationResponse.ResponseResult = fineResponse.ResponseResult;
                        return validationResponse;
                    }

                    ReservationItemRepository reservationItemRepository = new ReservationItemRepository(this.OrgService);
                    var item = reservationItemRepository.getDiscountReservationItem(reservationId, new string[] { "rnt_totalamount" });
                    var discountAmount = item == null ? decimal.Zero : item.GetAttributeValue<Money>("rnt_totalamount").Value;

                    validationResponse.fineAmount = fineResponse.firstDayAmount - (-1 * discountAmount);
                    var IgnoreCancelProduct = CalculateIgnoreCancelAdditionalProductAmountInReservation(reservationId);
                    if (!IgnoreCancelProduct.existsIgnoreCancelItem)
                    {
                        validationResponse.refundAmount = validationResponse.totalAmount + (-1 * discountAmount) - fineResponse.firstDayAmount;
                    }
                    else
                    {
                        validationResponse.refundAmount = validationResponse.totalAmount - IgnoreCancelProduct.totalAmount;
                    }

                    if (!validationResponse.isCampaignCancelable)
                    {
                        this.Trace("campaign is not cancelable");
                        validationResponse.fineAmount = validationResponse.totalAmount;
                        validationResponse.refundAmount = decimal.Zero;
                    }
                }
                else
                {
                    validationResponse.fineAmount = decimal.Zero;
                    validationResponse.refundAmount = validationResponse.totalAmount;
                }
            }
            return validationResponse;
        }


        public ReservationCancellationResponse calculateCancellationAmountForGivenReservation(ReservationCancellationResponse validationResponse,
                                                                                              Guid reservationId,
                                                                                              bool willChargeFromUser,
                                                                                              int langId)
        {
            PaymentRepository paymentRepository = new PaymentRepository(this.OrgService);
            var payments = paymentRepository.getCountofNotRefundedPayments_Reservation(reservationId);
            //check reservation has payment record and 
            //reservation start date is close enough to check fine amount

            ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
            var res = reservationRepository.getReservationById(reservationId, new string[] { "rnt_reservationtypecode", "rnt_paymentmethodcode" });

            ContractHelper contractHelper = new ContractHelper(this.OrgService);
            var isCorpReservation = contractHelper.checkCorporateInCancellation(res.GetAttributeValue<OptionSetValue>("rnt_reservationtypecode").Value,
                                                                                       res.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value);
            this.Trace("isCorpReservation : " + isCorpReservation);
            if (payments > 0 || isCorpReservation)
            {
                this.Trace("validationResponse.willChargeFromUser " + willChargeFromUser);
                if (isCorpReservation)
                {
                    validationResponse.fineAmount = decimal.Zero;
                    validationResponse.refundAmount = validationResponse.totalAmount;
                }
                else if (!validationResponse.isCampaignCancelable)
                {
                    this.Trace("campaign is not cancelable");
                    validationResponse.fineAmount = validationResponse.totalAmount;
                    validationResponse.refundAmount = decimal.Zero;
                }
                else if (validationResponse.willChargeFromUser)
                {
                    var fineResponse = this.calculateFineAmountForGivenReservationFromMongoDB(reservationId, langId);
                    //if first day price couldnt found into system no need to continue
                    if (!fineResponse.ResponseResult.Result)
                    {
                        validationResponse.ResponseResult = fineResponse.ResponseResult;
                        return validationResponse;
                    }
                    this.Trace("fineResponse.firstDayAmount First = " + fineResponse.firstDayAmount);

                    ReservationItemRepository reservationItemRepository = new ReservationItemRepository(this.OrgService);
                    var item = reservationItemRepository.getDiscountReservationItem(reservationId, new string[] { "rnt_totalamount" });
                    var discountAmount = item == null ? decimal.Zero : item.GetAttributeValue<Money>("rnt_totalamount").Value;

                    validationResponse.fineAmount = fineResponse.firstDayAmount - (-1 * discountAmount);
                    validationResponse.refundAmount = validationResponse.totalAmount + (-1 * discountAmount) - fineResponse.firstDayAmount;


                    this.Trace("validationResponse.fineAmount  = " + fineResponse.firstDayAmount);
                }
                else
                {
                    validationResponse.fineAmount = decimal.Zero;
                    validationResponse.refundAmount = validationResponse.totalAmount;
                }
                this.Trace("validationResponse.totalAmount = " + validationResponse.totalAmount);
                this.Trace("validationResponse.refundAmount  = " + (validationResponse.refundAmount));
            }
            return validationResponse;
        }

        public void completeReservationHeaderandItemsForContract(Guid reservationId)
        {
            ReservationItemRepository reservationItemRepository = new ReservationItemRepository(this.OrgService);
            var items = reservationItemRepository.getActiveReservationItemsByReservationId(reservationId);

            foreach (var item in items)
            {
                ReservationItemBL reservationItemBL = new ReservationItemBL(this.OrgService, this.TracingService);
                reservationItemBL.completeReservationItem(item.Id);
            }
            this.completeReservation(reservationId);
        }
        public void calcultePaymentRelatedRollupFields(Guid reservationId, int paymentTransactionType)
        {
            XrmHelper xrmHelper = new XrmHelper(this.OrgService);
            if ((int)PaymentEnums.PaymentTransactionType.DEPOSIT == paymentTransactionType ||
                (int)PaymentEnums.PaymentTransactionType.SALE == paymentTransactionType)
            {
                xrmHelper.CalculateRollupField("rnt_reservation", reservationId, "rnt_paidamount");
            }
            else if ((int)PaymentEnums.PaymentTransactionType.REFUND == paymentTransactionType)
            {
                xrmHelper.CalculateRollupField("rnt_reservation", reservationId, "rnt_refundamount");
            }

        }
        public void updateReservationHeaderForContract(Guid reservationId, Guid contractId)
        {
            Entity entity = new Entity("rnt_reservation");
            entity.Id = reservationId;
            entity.Attributes["rnt_contractnumber"] = new EntityReference("rnt_contract", contractId);
            this.OrgService.Update(entity);
        }

        public void updateReservationIsWalkin(Entity res)
        {
            //this.Trace("res: " + JsonConvert.SerializeObject(res));
            this.Trace("now: " + DateTime.Now);
            this.Trace("createdon: " + res.GetAttributeValue<DateTime>("createdon"));
            this.Trace("total minutes: " + (DateTime.Now - res.GetAttributeValue<DateTime>("createdon")).TotalMinutes);

            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var walkinMinute = Convert.ToDouble(configurationBL.GetConfigurationByName("walkinMinute"));

            if ((DateTime.Now - res.GetAttributeValue<DateTime>("createdon")).TotalMinutes <= walkinMinute)
            {
                this.Trace("Reservation is walk-in");
                Entity e = new Entity("rnt_reservation");
                e.Id = res.Id;
                e["rnt_iswalkin"] = true;
                this.OrgService.Update(e);
            }
        }

        public decimal? getReservationDiffAmount(Guid reservationId)
        {
            ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
            var reservationEntity = reservationRepository.getReservationById(reservationId, new string[] { "rnt_totalamount", "rnt_netpayment" });

            var diff = reservationEntity.GetAttributeValue<Money>("rnt_totalamount")?.Value - reservationEntity.GetAttributeValue<Money>("rnt_netpayment")?.Value;

            return diff;
        }
        public decimal? getReservationTotalAmount(Guid reservationId)
        {
            ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
            var reservationEntity = reservationRepository.getReservationById(reservationId, new string[] { "rnt_totalamount" });

            var totalAmount = reservationEntity.GetAttributeValue<Money>("rnt_totalamount")?.Value;

            return totalAmount;
        }

        public decimal? getReservationDepositAmount(Guid reservationId)
        {
            ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
            var reservationEntity = reservationRepository.getReservationById(reservationId, new string[] { "rnt_depositamount" });

            var depositAmount = reservationEntity.GetAttributeValue<Money>("rnt_depositamount")?.Value;

            return depositAmount;
        }

        public void updateReservationTotalDuration(Guid reservationId, DateTime pickupDateTime, DateTime dropoffDateTime)
        {
            var contractHelper = new RntCar.BusinessLibrary.Helpers.ContractHelper(this.OrgService);
            var totalDuration = contractHelper.calculateTotalDuration_ContractByPriceHourEffect_Decimal(pickupDateTime, dropoffDateTime);

            this.Trace("totalDuration: " + totalDuration);

            Entity e = new Entity("rnt_reservation");
            e.Id = reservationId;
            e["rnt_duration"] = totalDuration;

            this.OrgService.Update(e);
        }

        public Entity getIndividualCustomerInfo(Guid contactId)
        {
            IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(this.OrgService);
            string[] columns = new string[] { "firstname", "lastname", "mobilephone", "emailaddress1", "rnt_customerexternalid", "governmentid", "rnt_passportnumber" };
            return individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(contactId, columns);
        }

        public List<Entity> getNoShowReservations(DateTime currentDate)
        {
            try
            {
                ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);

                var data = reservationRepository.getNoShowReservations(currentDate);
                return data;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public decimal getReservationManuelDiscountAmount(Guid reservationId)
        {
            ConfigurationRepository configurationRepository = new ConfigurationRepository(this.OrgService);
            var productCode = configurationRepository.GetConfigurationByKey("additionalProducts_manuelDiscount");

            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);
            var additionalProduct = additionalProductRepository.getAdditionalProductByProductCode(productCode);

            ReservationItemRepository reservationItemRepository = new ReservationItemRepository(this.OrgService);
            var reservationItem = reservationItemRepository.getReservationItemByAdditionalProductIdWithGivenColumns(reservationId, additionalProduct.Id, new string[] { "rnt_totalamount" });
            var manuelDiscountAmount = decimal.Zero;
            if (reservationItem != null)
                manuelDiscountAmount = (reservationItem.GetAttributeValue<Money>("rnt_totalamount").Value * -1);

            return manuelDiscountAmount;
        }

        public CalculateReservationRuleResponse calculateReservationRules(CalculateReservationRuleParameters calculateReservationRuleParameters)
        {
            CalculateReservationRuleResponse calculateReservationRuleResponse = new CalculateReservationRuleResponse();
            if (calculateReservationRuleParameters?.customerType == (int)GlobalEnums.CustomerType.Individual)
            {
                this.Trace("reservationPriceParameters.pricingType" + calculateReservationRuleParameters.pricingType);
                //individual pricing type
                //reservation is individual also pricing type is individual
                if (new Guid(calculateReservationRuleParameters.pricingType) == calculateReservationRuleParameters.contactId)
                {
                    this.Trace("pricing type is individual");
                    calculateReservationRuleResponse.paymentMethodCode = (int)ReservationEnums.ReservationPaymentType.INDIVIDUAL;
                }
                //reservation type is individual but pricing type is corporate
                else
                {
                    this.Trace("pricing type is corporate");
                    calculateReservationRuleResponse.paymentMethodCode = (int)ReservationEnums.ReservationPaymentType.CORPORATE;
                }
                calculateReservationRuleResponse.depositAmount = calculateReservationRuleParameters.groupCodeInformationDetailDataForDocument.depositAmount;
                calculateReservationRuleResponse.findeks = calculateReservationRuleParameters.groupCodeInformationDetailDataForDocument.findeks;
            }
            else if (calculateReservationRuleParameters?.customerType == (int)GlobalEnums.CustomerType.Corporate)
            {

                var parsedPricingType = Convert.ToInt32(calculateReservationRuleParameters.pricingType);
                calculateReservationRuleResponse.paymentMethodCode = parsedPricingType;


                CorporateCustomerRepository corporateCustomerRepository = new CorporateCustomerRepository(this.OrgService);
                var corp = corporateCustomerRepository.getCorporateCustomerById(calculateReservationRuleParameters.corporateId.Value, new string[] { "rnt_checkfindeks", "rnt_chargedeposit" });

                if (parsedPricingType != (int)rnt_PaymentMethodCode.Current &&
                    corp.Contains("rnt_checkfindeks") && corp.GetAttributeValue<bool>("rnt_checkfindeks"))
                {
                    calculateReservationRuleResponse.findeks = calculateReservationRuleParameters.groupCodeInformationDetailDataForDocument.findeks;
                }
                else
                {
                    calculateReservationRuleResponse.findeks = 0;
                }
                if (parsedPricingType != (int)rnt_PaymentMethodCode.Current && corp.Contains("rnt_chargedeposit") && corp.GetAttributeValue<bool>("rnt_chargedeposit"))
                {
                    calculateReservationRuleResponse.depositAmount = calculateReservationRuleParameters.groupCodeInformationDetailDataForDocument.depositAmount;
                }
                else
                {
                    calculateReservationRuleResponse.depositAmount = decimal.Zero;
                }

            }
            else if (calculateReservationRuleParameters?.customerType == (int)GlobalEnums.CustomerType.Agency ||
                     calculateReservationRuleParameters?.customerType == (int)GlobalEnums.CustomerType.Broker)
            {
                CorporateCustomerRepository corporateCustomerRepository = new CorporateCustomerRepository(this.OrgService);
                var corp = corporateCustomerRepository.getCorporateCustomerById(calculateReservationRuleParameters.corporateId.Value, new string[] { "rnt_processindividualprices" });

                var parsedPricingType = Convert.ToInt32(calculateReservationRuleParameters.pricingType);
                calculateReservationRuleResponse.paymentMethodCode = parsedPricingType;
                calculateReservationRuleResponse.depositAmount = calculateReservationRuleParameters.groupCodeInformationDetailDataForDocument.depositAmount;
                calculateReservationRuleResponse.findeks = calculateReservationRuleParameters.groupCodeInformationDetailDataForDocument.findeks;
                calculateReservationRuleResponse.processCustomerPrices = corp.Contains("rnt_processindividualprices") ? corp.GetAttributeValue<bool>("rnt_processindividualprices") : false;

                if (parsedPricingType == (int)rnt_PaymentMethodCode.FullCredit)
                {
                    calculateReservationRuleResponse.depositAmount = decimal.Zero;
                }
            }
            return calculateReservationRuleResponse;

        }
        private void completeReservation(Guid reservationId)
        {
            XrmHelper xrmHelper = new XrmHelper(this.OrgService);
            xrmHelper.setState("rnt_reservation", reservationId, (int)GlobalEnums.StateCode.Active, (int)ReservationEnums.StatusCode.Completed);
        }
        private void rentReservation(Guid reservationId)
        {
            XrmHelper xrmHelper = new XrmHelper(this.OrgService);
            xrmHelper.setState("rnt_reservation", reservationId, (int)GlobalEnums.StateCode.Active, (int)ReservationEnums.StatusCode.Rental);
        }
        public void setReservationState(Guid reservationId, int statusCode)
        {
            if (Enum.IsDefined(typeof(ReservationEnums.StatusCode), statusCode))
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                xrmHelper.setState("rnt_reservation", reservationId, (int)GlobalEnums.StateCode.Active, statusCode);
            }
        }

        private void operationsforBroker(ReservationCustomerParameters reservationCustomerParameter,
                                         ReservationDateandBranchParameters reservationDateandBranchParameter,
                                         ReservationEquipmentParameters reservationEquipmentParameter,
                                         ReservationPriceParameters reservationPriceParameters,
                                         ReservationRelatedParameters reservationRelatedParameter,
                                         Entity reservationEquipmentItem,
                                         Entity reservation,
                                         Guid transactionCurrencyId,
                                         int totalDuration,
                                         string trackingNumber,
                                         int changeType,
                                         bool groupCodeChanges)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var columns = new string[] { "rnt_campaignid", "rnt_totalamount" };
            ReservationItemRepository reservationItemRepository = new ReservationItemRepository(this.OrgService);
            var reservationItem = reservationItemRepository.getActiveReservationItemEquipmentByGivenColumns(reservationRelatedParameter.reservationId, columns);
            XrmHelper xrmHelper = new XrmHelper(this.OrgService);
            xrmHelper.setState("rnt_reservationitem", reservationItem.Id, 0, (int)rnt_reservationitem_StatusCode.Completed);
            var tmpTime = reservationDateandBranchParameter.pickupDate;

            reservationDateandBranchParameter.pickupDate = reservationEquipmentItem.GetAttributeValue<DateTime>("rnt_dropoffdatetime").AddMinutes(-StaticHelper.offset);
            var tmpamount = reservationPriceParameters.price;
            if (groupCodeChanges)
            {
                if (changeType == (int)rnt_ChangeType.Upsell || changeType == (int)rnt_ChangeType.Downsell)
                {
                    var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
                    RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "getPriceCalculationSummaries", RestSharp.Method.POST);

                    this.Trace("mongodb contract build params start");
                    var getPriceCalculationSummariesRequest = new GetPriceCalculationSummariesRequest
                    {
                        groupCodeInformationId = Convert.ToString(reservationEquipmentParameter.groupCodeInformationId),
                        trackingNumber = trackingNumber
                    };
                    this.Trace("mongodb contract build params end");

                    restSharpHelper.AddJsonParameter<GetPriceCalculationSummariesRequest>(getPriceCalculationSummariesRequest);

                    var response = restSharpHelper.Execute<GetPriceCalculationSummariesResponse>();

                    reservationPriceParameters.price = response.dailyPrices.Sum(p => p.totalAmount);
                }

            }
            else
            {
                reservationPriceParameters.price = reservationPriceParameters.price - reservationItem.GetAttributeValue<Money>("rnt_totalamount").Value;
            }
            ReservationItemBL reservationItemBL = new ReservationItemBL(this.OrgService, this.TracingService);
            var reservationItemId = reservationItemBL.createReservationItemForEquipment(reservationCustomerParameter,
                                                                                        reservationDateandBranchParameter,
                                                                                        reservationEquipmentParameter,
                                                                                        reservationPriceParameters,
                                                                                        reservation.Id,
                                                                                        transactionCurrencyId,
                                                                                        totalDuration,
                                                                                        trackingNumber,
                                                                                        string.Empty,
                                                                                        reservationItem.GetAttributeValue<EntityReference>("rnt_campaignid"),
                                                                                        changeType,
                                                                                        reservationRelatedParameter.reservationChannel);

            if (groupCodeChanges)
            {
                if (tmpamount - reservationItem.GetAttributeValue<Money>("rnt_totalamount").Value - reservationPriceParameters.price > 0)
                {
                    var differenceAdditionalCode = configurationBL.GetConfigurationByName("additionalProduct_priceDifference");

                    AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);
                    var additionalProduct = additionalProductRepository.getAdditionalProductByProductCode(differenceAdditionalCode);

                    reservationPriceParameters.price = tmpamount - reservationItem.GetAttributeValue<Money>("rnt_totalamount").Value - reservationPriceParameters.price;
                    var itemId = reservationItemBL.createReservationItemForEquipment(reservationCustomerParameter,
                                                                                    reservationDateandBranchParameter,
                                                                                    reservationEquipmentParameter,
                                                                                    reservationPriceParameters,
                                                                                    reservation.Id,
                                                                                    transactionCurrencyId,
                                                                                    totalDuration,
                                                                                    trackingNumber,
                                                                                    string.Empty,
                                                                                    reservationItem.GetAttributeValue<EntityReference>("rnt_campaignid"),
                                                                                    changeType,
                                                                                    reservationRelatedParameter.reservationChannel,
                                                                                    5);

                    Entity e = new Entity("rnt_reservationitem");
                    e.Id = itemId;
                    e["rnt_name"] = additionalProduct.GetAttributeValue<string>("rnt_name");

                    e["rnt_additionalproductid"] = new EntityReference("rnt_additionalproduct", additionalProduct.Id);
                    e["statuscode"] = new OptionSetValue((int)rnt_reservationitem_StatusCode.Completed);
                    this.OrgService.Update(e);
                }

            }



            reservationDateandBranchParameter.pickupDate = tmpTime;
        }

        public bool checkCreditLimit(ReservationCustomerParameters reservationCustomerParameters, ReservationPriceParameters reservationPriceParameters, List<ReservationAdditionalProductParameters> reservationItemAdditionalProductParameter, int totalDuration)
        {
            bool resultLimit = true;
            CorporateCustomerRepository corporateCustomerRepository = new CorporateCustomerRepository(this.OrgService);
            var corp = corporateCustomerRepository.getCorporateCustomerById(reservationCustomerParameters.corporateCustomerId.Value, new string[] { "creditlimit", "rnt_taxnumber", "rnt_logobalance", "rnt_isopencreditlimitcontrol" });
            var creditLimit = corp.GetAttributeValue<Money>("creditlimit");
            var creditLimitValue = creditLimit.Value;
            var logoCRMBalanceValue = corp.GetAttributeValue<Money>("rnt_logobalance") != null ? corp.GetAttributeValue<Money>("rnt_logobalance").Value : 0;
            var taxNumber = corp.GetAttributeValue<string>("rnt_taxnumber");
            var isCreditLimitControl = corp.GetAttributeValue<bool>("rnt_isopencreditlimitcontrol");
            if (!isCreditLimitControl)
            {
                return resultLimit;
            }
            decimal totalAmount = reservationPriceParameters.price;

            foreach (var item in reservationItemAdditionalProductParameter)
            {
                if (item.maxPieces <= 1)
                {
                    totalAmount = totalAmount + CommonHelper.calculateAdditionalProductPrice(item.priceCalculationType,
                                                                                          item.monthlyPackagePrice,
                                                                                          item.actualAmount.Value,
                                                                                          totalDuration);
                }
                else
                {
                    for (int i = 0; i < item.value; i++)
                    {
                        totalAmount = totalAmount + CommonHelper.calculateAdditionalProductPrice(item.priceCalculationType,
                                                                                            item.monthlyPackagePrice,
                                                                                            (decimal)item.actualAmount,
                                                                                             totalDuration);
                    }
                }
            }

            string[] columns = new string[] { "rnt_generaltotalamount" };
            decimal generalTotalAmountValue = 0;
            ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
            List<Entity> reservationList = reservationRepository.getActiveReservationsByCorporateId(columns, reservationCustomerParameters.corporateCustomerId.Value);
            foreach (var reservation in reservationList)
            {
                Money generalTotalAmount = reservation.GetAttributeValue<Money>("rnt_generaltotalamount");
                generalTotalAmountValue += generalTotalAmount.Value;
            }
            LogoHelper logoHelper = new LogoHelper(this.OrgService);
            var logoBalance = logoHelper.getAccountBalance(taxNumber);
            if (creditLimitValue < Convert.ToDecimal(logoBalance) + generalTotalAmountValue + totalAmount)
            {
                resultLimit = false;
            }

            if (logoCRMBalanceValue != Convert.ToDecimal(logoBalance))
            {
                Entity updateCorp = new Entity(corp.LogicalName, corp.Id);
                updateCorp["rnt_logobalance"] = new Money(Convert.ToDecimal(logoBalance) + generalTotalAmountValue);
                this.OrgService.Update(updateCorp);
            }
            return resultLimit;
        }

        public ReservationIgnoreCancelAdditionalProductResponse CalculateIgnoreCancelAdditionalProductAmountInReservation(Guid reservationId)
        {
            bool existsIgnoreCancelAdditionalProduct = false;
            decimal existsIgnoreCancelAdditionalProductAmount = 0;
            ReservationItemRepository reservationItemRepository = new ReservationItemRepository(this.OrgService);
            var reservationItems = reservationItemRepository.getNewNoShowReservationItemsByReservationId(reservationId);

            var ignoreCancelAdditionalProdcutIdList = StaticHelper.ignoreCanceAdditionalProdcutIds.Split(';').ToList();
            ignoreCancelAdditionalProdcutIdList.RemoveAll(item => item == "");

            foreach (var item in reservationItems)
            {
                var additionalProductEntityRef = item.GetAttributeValue<EntityReference>("rnt_additionalproductid");
                if (additionalProductEntityRef != null)
                {
                    Entity additionalProductEntity = OrgService.Retrieve(additionalProductEntityRef.LogicalName, additionalProductEntityRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("rnt_additionalproductcode"));
                    var additionalProductCode = additionalProductEntity.GetAttributeValue<string>("rnt_additionalproductcode");
                    if (ignoreCancelAdditionalProdcutIdList.Contains(additionalProductCode))
                    {
                        existsIgnoreCancelAdditionalProductAmount += item.GetAttributeValue<Money>("rnt_totalamount").Value;
                        existsIgnoreCancelAdditionalProduct = true;
                    }
                }
            }
            return new ReservationIgnoreCancelAdditionalProductResponse()
            {
                totalAmount = existsIgnoreCancelAdditionalProductAmount,
                existsIgnoreCancelItem = existsIgnoreCancelAdditionalProduct
            };
        }

        public bool checkActiveReservationForCustomer(Guid contactId)
        {
            int[] statusCodeList = new int[] { (int)rnt_reservationitem_StatusCode.New
                ,(int)rnt_reservationitem_StatusCode.WaitingForDelivery};
            QueryExpression getReservationQuery = new QueryExpression("rnt_reservation");
            getReservationQuery.Criteria.AddCondition("rnt_customerid", ConditionOperator.Equal, contactId);
            getReservationQuery.Criteria.AddCondition(new ConditionExpression("statuscode", ConditionOperator.In, statusCodeList));
            EntityCollection reservationList = this.OrgService.RetrieveMultiple(getReservationQuery);
            return reservationList.Entities.Count == 0;
        }
    }
}
