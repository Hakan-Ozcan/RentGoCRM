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
using RntCar.ClassLibrary.Enums;
using RntCar.ClassLibrary.MongoDB;
using RntCar.ClassLibrary.odata;
using RntCar.SDK.Common;
using RntCar.SDK.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RntCar.BusinessLibrary.Business
{
    public class ContractBL : BusinessHandler
    {

        public ContractBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public ContractBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }




        public void createContractInvoiceDate(CreateContractInvoiceDateParameters createContractInvoiceDateParameters)
        {
            var name = createContractInvoiceDateParameters.type == 1 ? "Aylık" : "Bireysel / Kurumsal";

            name += " " + createContractInvoiceDateParameters.pickupDatime.ToString("dd/MM/yyyy") + " " + createContractInvoiceDateParameters.dropoffDateTime.ToString("dd/MM/yyyy");
            Entity e = new Entity("rnt_contractinvoicedate");
            e["rnt_contractid"] = new EntityReference("rnt_contract", createContractInvoiceDateParameters.contractId);
            e["rnt_amount"] = new Money(createContractInvoiceDateParameters.amount);
            e["rnt_invoicedate"] = createContractInvoiceDateParameters.invoiceDate;
            e["rnt_pickupdatetime"] = createContractInvoiceDateParameters.pickupDatime;
            e["rnt_dropoffdatetime"] = createContractInvoiceDateParameters.dropoffDateTime;
            e["rnt_extensiontypecode"] = new OptionSetValue(createContractInvoiceDateParameters.type);
            e["rnt_name"] = name;
            e["rnt_invoicetemplate"] = createContractInvoiceDateParameters.templates;
            this.OrgService.Create(e);
        }

        public CustomerDebtResponse calculateDebitAmount(Guid contractId)
        {
            var debitAmount = decimal.Zero;

            ContractRepository contractRepository = new ContractRepository(this.OrgService);
            var c = contractRepository.getContractById(contractId, new string[] { "rnt_customerid" });
            string sum = string.Format(@"<fetch distinct='false' mapping='logical' aggregate='true'> 
                                           <entity name='rnt_contract'> 
                                              <attribute name='rnt_debitamount' alias='debitsum' aggregate='sum' /> 
                                                 <filter type='and'>
                                                      <condition attribute='statuscode' operator='in'>
                                                        <value>100000008</value>
                                                        <value>100000007</value>
                                                        <value>{0}</value>
                                                      </condition>
                                                    <condition attribute='rnt_customerid' operator='eq'  uitype='contact' value='{1}' />
                                                 </filter>
                                           </entity> 
                                        </fetch>",
                                        (int)rnt_contract_StatusCode.Completed,
                                        c.GetAttributeValue<EntityReference>("rnt_customerid").Id);

            var res = this.OrgService.RetrieveMultiple(new FetchExpression(sum));
            if (res.Entities.FirstOrDefault() != null)
            {
                debitAmount = ((Money)(res.Entities.FirstOrDefault().GetAttributeValue<AliasedValue>("debitsum").Value)).Value;
            }
            return new CustomerDebtResponse
            {
                debtAmount = debitAmount,
                customerId = c.GetAttributeValue<EntityReference>("rnt_customerid").Id
            };

        }

        public void updateContractItemsandContractTurkishCurrency(Guid contractId, Guid turkishCurrencyId)
        {
            Entity contract = new Entity("rnt_contract");
            contract["rnt_dummymoneyfield"] = new Money(Convert.ToDecimal(StaticHelper.RandomDigits(3)));
            contract.Id = contractId;
            this.updateEntity(contract);

            ContractRepository contractRepository = new ContractRepository(this.OrgService);
            var c = contractRepository.getContractById(contractId, new string[] { "exchangerate" });
            var rate = c.GetAttributeValue<decimal>("exchangerate");
            this.Trace("rate : " + rate);

            ContractItemRepository contractItemRepository = new ContractItemRepository(this.OrgService);
            var items = contractItemRepository.getActiveContractItems(contractId);


            this.Trace("items count " + items.Count);
            foreach (var item in items)
            {
                Entity currencyItem = new Entity("rnt_contractitem");
                currencyItem.Id = item.Id;
                var _totalAmount = item.GetAttributeValue<Money>("rnt_totalamount").Value / rate;
                var _basePrice = item.GetAttributeValue<Money>("rnt_baseprice").Value / rate;
                var monthlyPrice = item.Contains("rnt_monthlypackageprice") ? item.GetAttributeValue<Money>("rnt_monthlypackageprice").Value / rate : decimal.Zero;
                this.Trace("_totalAmount : " + _totalAmount);
                this.Trace("_basePrice : " + _basePrice);
                this.Trace("monthlyPrice : " + monthlyPrice);
                currencyItem["rnt_totalamount"] = new Money(_totalAmount);
                currencyItem["rnt_baseprice"] = new Money(_basePrice);
                currencyItem["rnt_monthlypackageprice"] = new Money(monthlyPrice);
                currencyItem["transactioncurrencyid"] = new EntityReference("transactioncurrency", turkishCurrencyId);
                this.updateEntity(currencyItem);
            }

            Entity contractCurrency = new Entity("rnt_contract");
            contractCurrency["transactioncurrencyid"] = new EntityReference("transactioncurrency", turkishCurrencyId);
            contractCurrency.Id = contractId;
            this.updateEntity(contractCurrency);


            List<string> fields = new List<string>();
            fields.Add("rnt_taxamount");
            fields.Add("rnt_totalamount");
            fields.Add("rnt_totalfineamount");
            fields.Add("rnt_corporatetotalamount");


            XrmHelper xrmHelper = new XrmHelper(this.OrgService);
            xrmHelper.CalculateRollupField("rnt_contract", contractId, fields);

            //also update with payments
            //rex yaratma sırasında teminat çekilemediği durumda çekilen para 35 dolar olsun
            //TL karsılıgını dynamicsin hesaplaması için dummy field update mantıgı
            PaymentRepository paymentRepository = new PaymentRepository(this.OrgService);
            var lastPayment = paymentRepository.getLastPayment_Contract(contractId);
            if (lastPayment != null)
            {
                Entity pay = new Entity("rnt_payment");
                pay["rnt_comission"] = new Money(decimal.Zero);
                pay.Id = lastPayment.Id;
                this.OrgService.Update(pay);
            }
        }

        public void updateExchangeRateContract(Guid contractId)
        {
            Entity e = new Entity("rnt_contract");
            e["rnt_dummymoneyfield"] = new Money(Convert.ToDecimal(StaticHelper.RandomDigits(3)));
            e.Id = contractId;
            this.updateEntity(e);
        }
        public ContractCreateResponse createContractWithInitializeFromRequest(Guid reservationId,
                                                                              string reservationPNR,
                                                                              int channelCode,
                                                                              Guid campaignId,
                                                                              string couponCode = null,
                                                                              string birdId = null)
        {
            XrmHelper xrmHelper = new XrmHelper(this.OrgService);
            Entity contract = xrmHelper.initializeFromRequest("rnt_reservation", reservationId, "rnt_contract");
            if (contract != null)
            {
                var autoNum = CommonHelper.generateRandomStringByGivenNumber(6);
                contract["rnt_name"] = autoNum;
                contract["statuscode"] = new OptionSetValue((int)ContractEnums.StatusCode.WaitingforDelivery);
                contract["rnt_contractchannelcode"] = new OptionSetValue(channelCode);
                if (!string.IsNullOrEmpty(couponCode))
                    contract["rnt_couponcode"] = couponCode;
                if (!string.IsNullOrEmpty(birdId))
                    contract["rnt_birdid"] = birdId;
                if (campaignId != Guid.Empty)
                    contract["rnt_campaignid"] = new EntityReference("rnt_campaign", campaignId);

                var _id = this.OrgService.Create(contract);

                return new ContractCreateResponse
                {
                    contractId = _id,
                    pnrNumber = reservationPNR
                };
            }

            var message = xrmHelper.GetXmlTagContent(this.UserId, "EntityNull", this.ErrorMessageXml);
            return new ContractCreateResponse
            {
                ResponseResult = ResponseResult.ReturnError(message)
            };
        }

        public ContractCreateResponse createContract(ContractCustomerParameters contractCustomerParameter,
                                                     ContractDateandBranchParameters contractDateandBranchParameter,
                                                     ContractEquipmentParameters contractEquipmentParameter,
                                                     ContractPriceParameters contractPriceParameters,
                                                     ContractRelatedParameters contractRelatedParameter,
                                                     Guid transactionCurrencyId,
                                                     int totalDuration)
        {
            Entity contract = new Entity("rnt_contract");

            if (contractCustomerParameter?.customerType == (int)GlobalEnums.CustomerType.Individual ||
              contractCustomerParameter?.customerType == (int)GlobalEnums.CustomerType.Corporate)
            {
                contract["rnt_customerid"] = new EntityReference("contact", contractCustomerParameter.contactId);
            }
            if (contractCustomerParameter?.customerType == (int)GlobalEnums.CustomerType.Corporate && contractCustomerParameter.corporateCustomerId.HasValue)
            {
                contract["rnt_corporateid"] = new EntityReference("account", contractCustomerParameter.corporateCustomerId.Value);
            }

            if (contractPriceParameters.campaignId.HasValue)
            {
                contract["rnt_campaignid"] = new EntityReference("rnt_campaign", contractPriceParameters.campaignId.Value);
            }

            contract["statuscode"] = new OptionSetValue((int)ContractEnums.StatusCode.WaitingforDelivery);
            contract["rnt_reservationid"] = new EntityReference("rnt_reservation", contractRelatedParameter.reservationId);
            contract["rnt_dropoffbranchid"] = new EntityReference("rnt_branch", contractDateandBranchParameter.dropoffBranchId);
            contract["rnt_pickupbranchid"] = new EntityReference("rnt_branch", contractDateandBranchParameter.pickupBranchId);
            contract["rnt_pickupdatetime"] = contractDateandBranchParameter.pickupDate.AddMinutes(StaticHelper.offset);
            contract["rnt_dropoffdatetime"] = contractDateandBranchParameter.dropoffDate.AddMinutes(StaticHelper.offset);
            contract["rnt_offset"] = StaticHelper.offset;
            contract["rnt_groupcodeid"] = new EntityReference("rnt_groupcodeinformations", contractEquipmentParameter.groupCodeInformationId);
            contract["rnt_pricinggroupcodeid"] = new EntityReference("rnt_groupcodeinformations", contractEquipmentParameter.groupCodeInformationId);
            contract["rnt_depositamount"] = new Money(contractEquipmentParameter.depositAmount);
            contract["rnt_segmentcode"] = new OptionSetValue(contractEquipmentParameter.segment);
            contract["rnt_contractchannelcode"] = new OptionSetValue(contractRelatedParameter.contractChannel);
            contract["rnt_contracttypecode"] = new OptionSetValue(contractRelatedParameter.contractTypeCode);
            contract["transactioncurrencyid"] = new EntityReference("transactioncurrency", transactionCurrencyId);
            contract["rnt_paymentchoicecode"] = new OptionSetValue(contractPriceParameters.paymentType);
            var autoNum = StaticHelper.RandomDigits(1) + StaticHelper.GenerateString(1) + StaticHelper.RandomDigits(1) + StaticHelper.GenerateString(1) + StaticHelper.RandomDigits(1) + StaticHelper.GenerateString(1);
            contract["rnt_pnrnumber"] = contractRelatedParameter.reservationPNR;
            contract["rnt_name"] = autoNum;

            GroupCodeInformationBL groupCodeInformationBL = new GroupCodeInformationBL(this.OrgService);
            var groupCodeInformationForDocument = groupCodeInformationBL.getGroupCodeInformationDetailForDocument(contractEquipmentParameter.groupCodeInformationId);

            contract["rnt_doublecreditcard"] = groupCodeInformationForDocument.doubleCreditCard;
            contract["new_findeks"] = groupCodeInformationForDocument.findeks;
            contract["new_minimumage"] = groupCodeInformationForDocument.minimumAge;
            contract["new_minimumdriverlicense"] = groupCodeInformationForDocument.minimumDriverLicense;
            contract["new_youngdriverage"] = groupCodeInformationForDocument.youngDriverAge;
            contract["new_youngdriverlicense"] = groupCodeInformationForDocument.youngDriverLicense;

            //set kilometer limit
            KilometerLimitBL kilometerLimitBL = new KilometerLimitBL(this.OrgService, this.TracingService);
            var kilometer = kilometerLimitBL.getKilometerLimitForGivenDurationandGroupCode(totalDuration, contractEquipmentParameter.groupCodeInformationId);
            contract["rnt_kilometerlimit"] = kilometer;

            var _id = this.OrgService.Create(contract);

            return new ContractCreateResponse
            {
                contractId = _id,
                pnrNumber = contractRelatedParameter.reservationPNR
            };
        }

        public ContractSearchResponse searchContractByParameters(ContractSearchParameters searchParameters, int langId)
        {
            try
            {
                if (searchParameters.pickupDate.HasValue)
                    searchParameters.pickupDate = searchParameters.pickupDate.Value.AddMinutes(StaticHelper.offset);
                if (searchParameters.dropoffDate.HasValue)
                    searchParameters.dropoffDate = searchParameters.dropoffDate.Value.AddMinutes(StaticHelper.offset);

                this.Trace("params : " + JsonConvert.SerializeObject(searchParameters));
                this.Trace("langId : " + langId);

                ContractRepository contractRepository = new ContractRepository(this.OrgService);
                var contracts = contractRepository.getContractsBySearchParameters(searchParameters);

                ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
                var currencyTL = configurationBL.GetConfigurationByName("currency_TRY");
                var currencyCode = StaticHelper.tlSymbol;
                var exchangeRate = decimal.Zero;

                List<ContractSearchData> contractSearchDatas = new List<ContractSearchData>();
                foreach (var contract in contracts)
                {
                    if (new Guid(currencyTL) != contract.GetAttributeValue<EntityReference>("transactioncurrencyid").Id)
                    {
                        CurrencyRepository currencyRepository = new CurrencyRepository(this.OrgService);
                        var c = currencyRepository.getCurrencyById(contract.GetAttributeValue<EntityReference>("transactioncurrencyid").Id,
                                                                   new string[] { "currencysymbol", "exchangerate" });
                        currencyCode = c.GetAttributeValue<string>("currencysymbol");
                        exchangeRate = c.GetAttributeValue<decimal>("exchangerate");
                    }
                    var item = contract;
                    var totalFineAmount = item.Attributes.Contains("rnt_totalfineamount") ? item.GetAttributeValue<Money>("rnt_totalfineamount").Value : 0;
                    contractSearchDatas.Add(new ContractSearchData
                    {
                        exchangeRate = exchangeRate,
                        currencySymbol = currencyCode,
                        corporateTotalAmount = item.GetAttributeValue<Money>("rnt_corporatetotalamount")?.Value < 0 ? 0 : item.GetAttributeValue<Money>("rnt_corporatetotalamount")?.Value,
                        contractNumber = item.GetAttributeValue<string>("rnt_contractnumber"),
                        pnrNumber = item.GetAttributeValue<string>("rnt_pnrnumber"),
                        contactName = item.Attributes.Contains("rnt_customerid") ? item.GetAttributeValue<EntityReference>("rnt_customerid").Name : null,
                        contactId = item.Attributes.Contains("rnt_customerid") ? item.GetAttributeValue<EntityReference>("rnt_customerid").Id : Guid.Empty,
                        pickupBranchId = item.Attributes.Contains("rnt_pickupbranchid") ? item.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id : Guid.Empty,
                        pickupBranchName = item.Attributes.Contains("rnt_pickupbranchid") ? item.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Name : null,
                        pickupDateTime = item.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                        dropoffBranchId = item.Attributes.Contains("rnt_dropoffbranchid") ? item.GetAttributeValue<EntityReference>("rnt_dropoffbranchid").Id : Guid.Empty,
                        dropoffBranchName = item.Attributes.Contains("rnt_dropoffbranchid") ? item.GetAttributeValue<EntityReference>("rnt_dropoffbranchid").Name : null,
                        dropoffDateTime = item.GetAttributeValue<DateTime>("rnt_dropoffdatetime"),
                        groupCodeId = item.Attributes.Contains("rnt_groupcodeid") ? item.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id : Guid.Empty,
                        groupCodeName = item.Attributes.Contains("rnt_groupcodeid") ? item.GetAttributeValue<EntityReference>("rnt_groupcodeid").Name : string.Empty,
                        paymentType = item.Attributes.Contains("rnt_paymentchoicecode") ? item.GetAttributeValue<OptionSetValue>("rnt_paymentchoicecode").Value : 0,
                        status = item.GetAttributeValue<OptionSetValue>("statuscode").Value,
                        statusName = Convert.ToString(item.FormattedValues["statuscode"]),
                        state = item.GetAttributeValue<OptionSetValue>("statecode").Value,
                        totalAmount = item.Attributes.Contains("rnt_totalamount") ? item.GetAttributeValue<Money>("rnt_totalamount").Value - totalFineAmount : 0,
                        contractId = item.Id,
                        contractType = item.Attributes.Contains("rnt_contracttypecode") ? item.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value : 0,
                        corporateName = item.Attributes.Contains("rnt_corporateid") ? item.GetAttributeValue<EntityReference>("rnt_corporateid").Name : string.Empty,
                        corporateId = item.Attributes.Contains("rnt_corporateid") ? item.GetAttributeValue<EntityReference>("rnt_corporateid").Id : Guid.Empty,
                        pricingType = item.Attributes.Contains("rnt_pricingtype") ? item.GetAttributeValue<string>("rnt_pricingtype") : string.Empty,
                        paymentMethod = item.Attributes.Contains("rnt_paymentmethodcode") ? item.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value : 0
                    });

                }
                return new ContractSearchResponse
                {
                    ResponseResult = ResponseResult.ReturnSuccess(),
                    contractSearchData = contractSearchDatas
                };
            }
            catch (Exception ex)
            {
                return new ContractSearchResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }

        }
        public void updateContractDeliveryRelatedFields(Guid contractId,
                                                        long currentPickupTimestamp,
                                                        long currentDropoffTimestamp,
                                                        long pickupTimestamp,
                                                        bool isManualProcess,
                                                        DateTime utcNow,
                                                        Guid groupCodeInformationId,
                                                        List<RntCar.ClassLibrary._Tablet.AdditonalProductDataTablet> additionalProductDataTablets,
                                                        Guid userId,
                                                        Guid equipmentId,
                                                        bool equipmentChangedBefore)
        {

            var minutes = CommonHelper.calculateTotalDurationInMinutes(currentPickupTimestamp, currentDropoffTimestamp);
            ContractHelper contractHelper = new ContractHelper(this.OrgService);
            var totalDays = contractHelper.calculateTotalDuration_ContractByPriceHourEffect(currentPickupTimestamp.converttoDateTime(), currentDropoffTimestamp.converttoDateTime());
            Entity e = new Entity("rnt_contract");
            if (!equipmentChangedBefore)
            {
                if (isManualProcess)
                {
                    e["rnt_pickupdatetime"] = pickupTimestamp.converttoDateTime().AddMinutes(StaticHelper.offset);
                    e["rnt_dropoffdatetime"] = pickupTimestamp.converttoDateTime().AddMinutes(minutes).AddMinutes(StaticHelper.offset);
                }
                else
                {
                    e["rnt_pickupdatetime"] = utcNow.AddMinutes(StaticHelper.offset);
                    e["rnt_dropoffdatetime"] = utcNow.AddMinutes(minutes).AddMinutes(StaticHelper.offset);
                }
            }

            e["statuscode"] = new OptionSetValue((int)ContractEnums.StatusCode.Rental);

            e.Id = contractId;

            ContractItemBL contractItemBL = new ContractItemBL(this.OrgService, this.TracingService);
            var kilometer = contractItemBL.calculateKmforContractDelivery(totalDays, groupCodeInformationId, additionalProductDataTablets);
            e["rnt_contractdeliveryuserid"] = new EntityReference("rnt_serviceuser", userId);
            e["rnt_kilometerlimit"] = kilometer;
            e["rnt_equipmentid"] = new EntityReference("rnt_equipment", equipmentId);
            this.OrgService.Update(e);
        }
        public void updateContractRentalRelatedFields(Guid contractId,
                                                      Guid? dropoffBranchId,
                                                      long droppoffDateTime,
                                                      bool isManualProcess,
                                                      int? deptStatus)
        {
            Entity e = new Entity("rnt_contract");

            e["statuscode"] = new OptionSetValue((int)ContractEnums.StatusCode.Completed);
            if (isManualProcess)
            {
                e["rnt_dropoffdatetime"] = droppoffDateTime.converttoDateTime().AddMinutes(StaticHelper.offset);
            }
            else
            {
                e["rnt_dropoffdatetime"] = DateTime.UtcNow.AddMinutes(StaticHelper.offset);
            }
            if (dropoffBranchId.HasValue)
                e["rnt_dropoffbranchid"] = new EntityReference("rnt_branch", dropoffBranchId.Value);
            if (deptStatus.HasValue)
                e["rnt_deptstatus"] = new OptionSetValue(deptStatus.Value);

            e.Id = contractId;
            this.OrgService.Update(e);
        }
        public void updateContractHeader(Guid contractId,
                                         ContractDateandBranchParameters dateandBranchParameters,
                                         List<ContractAdditionalProductParameters> contractAdditionalProductParameters,
                                         int totalDuration)
        {
            Entity contract = new Entity("rnt_contract");
            contract["rnt_dropoffbranchid"] = new EntityReference("rnt_branch", dateandBranchParameters.dropoffBranchId);
            contract["rnt_pickupbranchid"] = new EntityReference("rnt_branch", dateandBranchParameters.pickupBranchId);
            contract["rnt_pickupdatetime"] = dateandBranchParameters.pickupDate.AddMinutes(StaticHelper.offset);
            contract["rnt_dropoffdatetime"] = dateandBranchParameters.dropoffDate.AddMinutes(StaticHelper.offset);

            //set kilometer limit
            KilometerLimitBL kilometerLimitBL = new KilometerLimitBL(this.OrgService, this.TracingService);

            ContractRepository contractRepository = new ContractRepository(this.OrgService);
            var pricingContract = contractRepository.getContractById(contractId, new string[] { "rnt_pricinggroupcodeid" });
            var pricingGroupCode = pricingContract.GetAttributeValue<EntityReference>("rnt_pricinggroupcodeid").Id;

            var kilometer = kilometerLimitBL.getKilometerLimitForGivenDurationandGroupCode(totalDuration, pricingGroupCode);
            this.Trace("kilometer : " + kilometer);
            this.Trace("rnt_pricinggroupcodeid : " + pricingGroupCode);
            if (contractAdditionalProductParameters != null)
            {
                var additionalIds = contractAdditionalProductParameters.Select(x => x.productId).ToList();

                AdditionalProductHelper additionalProductHelper = new AdditionalProductHelper(this.OrgService);
                var kilometerLimitEffect = additionalProductHelper.getAdditionalKilometerByAdditionalIds(additionalIds);
                if (kilometerLimitEffect.HasValue)
                {
                    kilometer += kilometerLimitEffect.Value;
                }

            }

            contract["rnt_kilometerlimit"] = kilometer;

            contract.Id = contractId;
            this.OrgService.Update(contract);
        }

        public void updateContractDepositAmount(Guid contractId, decimal depositAmount)
        {
            Entity e = new Entity("rnt_contract");
            e.Id = contractId;
            e["rnt_depositamount"] = new Money(depositAmount);
            this.OrgService.Update(e);
        }

        public ContractValidationResponse checkBeforeContractCreationWithParameters(Entity reservation, Guid contactId, Guid userId, bool isQuickContract, int langId)
        {
            ConfigurationRepository configurationRepository = new ConfigurationRepository(this.OrgService);
            var adminId = new Guid(configurationRepository.GetConfigurationByKey("CrmAdminGuid"));

            SystemParameterBL systemParameterBL = new SystemParameterBL(this.OrgService);
            var systemparameters = systemParameterBL.getContractRelatedSystemParameters();

            #region Retrieve Individual Customer Detail Data
            this.Trace("individualCustomer retrieve start");
            IndividualCustomerBL individualCustomerBL = new IndividualCustomerBL(this.OrgService);
            var individualCustomer = individualCustomerBL.getIndividualCustomerInformationForValidation(contactId);
            this.Trace("individualCustomer retrieve end");
            #endregion

            #region Black List Validation
            this.Trace("Black List Validation start");
            BlackListBL blackListBL = new BlackListBL(this.OrgService);
            this.Trace("black list governmentId : " + individualCustomer.governmentId);
            var blackListValidation = blackListBL.BlackListValidation(individualCustomer.governmentId);
            this.Trace("IsInBlackList : " + blackListValidation.BlackList.IsInBlackList);
            if (blackListValidation.BlackList.IsInBlackList)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                var message = xrmHelper.GetXmlTagContentByGivenLangId("BlackListValidation", langId);
                return new ContractValidationResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }
            this.Trace("Black List Validation end");
            #endregion

            ContractCreationValidation contractCreationValidation = new ContractCreationValidation(this.OrgService, this.TracingService);

            var result = contractCreationValidation.checkReservationStatusForCancel(reservation);

            if (!result)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                var message = xrmHelper.GetXmlTagContentByGivenLangId("ContractCancelStatusValidation", langId, this.contractXmlPath);
                return new ContractValidationResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }

            var result3DStatus = contractCreationValidation.checkReservationStatusFor3DWaiting(reservation);

            if (!result3DStatus)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                var message = xrmHelper.GetXmlTagContentByGivenLangId("Waiting3DStatusValidation", langId, this.contractXmlPath);
                return new ContractValidationResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }

            if (reservation.Contains("rnt_contractnumber"))
            {
                return new ContractValidationResponse
                {
                    ResponseResult = ResponseResult.ReturnError("Bu rezervasyonun sözleşmesi oluşturulmuştur.")
                };
            }

            if (systemparameters.checkUserBranch && adminId != userId)
            {
                result = contractCreationValidation.checkReservationBranchByUserId(reservation, userId);
                if (!result)
                {
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("OtherBranchsValidation", langId, this.contractXmlPath);
                    return new ContractValidationResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(message)
                    };
                }
            }

            result = contractCreationValidation.checkReservationStatusForExisting(reservation);

            if (!result)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                var message = xrmHelper.GetXmlTagContentByGivenLangId("ContractExistingValidation", langId, this.contractXmlPath);
                return new ContractValidationResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }

            if (isQuickContract)
            {
                var contactParameter = individualCustomerBL.getIndividualCustomerDetailContractRelationDataById(contactId);

                IndividualCustomerValidation individualCustomerValidation = new IndividualCustomerValidation(this.OrgService, this.TracingService);
                var contactValidation = individualCustomerValidation.checkIndividualCustomerFieldsForQuickContract(contactParameter);
                if (!contactValidation)
                {
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("QuickContractMissingCustomerInfo", langId, this.contractXmlPath);
                    return new ContractValidationResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(message)
                    };
                }
                result = contractCreationValidation.checkReservationPaymentTypeBeforeQuickContractCreation(reservation);

                if (!result)
                {
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("QuickContractPaymentChoiceValidation", langId, this.contractXmlPath);
                    return new ContractValidationResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(message)
                    };
                }

                result = contractCreationValidation.checkDateTimeBeforeQuickContractCreation(reservation,
                                                                                             Convert.ToInt32(systemparameters.quickContractMinimumDuration));

                if (!result)
                {
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("QuickContractDateTimeValidation", langId, this.contractXmlPath);
                    return new ContractValidationResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(message)
                    };
                }

                result = contractCreationValidation.checkReservationHasAdditionalDriver(reservation);
                if (!result)
                {
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("QuickContractAdditionalDriverValidation", langId, this.contractXmlPath);
                    return new ContractValidationResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(message)
                    };
                }
            }
            else
            {
                result = contractCreationValidation.checkDateTimeBeforeContractCreation(reservation,
                                                                                        Convert.ToInt32(systemparameters.contractMinimumDuration));
                if (!result)
                {
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                    var message = String.Format(xrmHelper.GetXmlTagContentByGivenLangId("ContractDateTimeValidation", langId, this.contractXmlPath), systemparameters.contractMinimumDuration.converttoHours());
                    return new ContractValidationResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(message)
                    };
                }
            }

            return new ContractValidationResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }

        public ContractCancellationResponse checkBeforeContractCancellation(ContractCancellationParameters parameters, int langId)
        {
            SystemParameterBL systemParameterBL = new SystemParameterBL(this.OrgService);
            var contractRelatedParameters = systemParameterBL.getContractRelatedSystemParameters();

            ContractRepository contractRepository = new ContractRepository(this.OrgService);
            var columns = new string[] { "rnt_contractnumber",
                                         "rnt_pnrnumber",
                                         "statuscode",
                                         "statecode",
                                         "rnt_pickupdatetime",
                                         "rnt_paymentchoicecode" ,
                                         "rnt_paymentmethodcode",
                                         "rnt_totalamount"};
            var contract = contractRepository.getContractByPnrNumber(parameters.pnrNumber, columns);
            if (contract == null)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);

                var message = xrmHelper.GetXmlTagContentByGivenLangId("NotExistedPnr", langId, this.contractXmlPath);
                return new ContractCancellationResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }

            var result = true;

            ContractCancellationValidation cancellationValidation = new ContractCancellationValidation(this.OrgService, this.TracingService);

            result = cancellationValidation.checkPNRandContractNumber(contract);

            if (!result)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);

                var message = xrmHelper.GetXmlTagContentByGivenLangId("NullPnrorContractNumber", langId, this.contractXmlPath);
                return new ContractCancellationResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }

            result = cancellationValidation.checkContractIsActive(contract);

            if (!result)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);

                var message = xrmHelper.GetXmlTagContentByGivenLangId("AlreadyInactiveContract", langId, this.contractXmlPath);
                return new ContractCancellationResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }

            result = cancellationValidation.checkContractStatus(contract);
            this.Trace("result before checkContractStatus");
            if (!result)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);

                var message = xrmHelper.GetXmlTagContentByGivenLangId("UnavailableStatusContract", langId, this.contractXmlPath);
                return new ContractCancellationResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }

            result = cancellationValidation.checkContractIsWaitingForDelivery(contract);

            if (!result)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);

                var message = xrmHelper.GetXmlTagContentByGivenLangId("UnavailableStatusContract", langId, this.contractXmlPath);
                return new ContractCancellationResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }

            //result = cancellationValidation.checkContractHasCompletedEquipment(contract);

            //if (!result)
            //{
            //    XrmHelper xrmHelper = new XrmHelper(this.OrgService);

            //    var message = xrmHelper.GetXmlTagContentByGivenLangId("UnavailableStatusContract", langId, this.contractXmlPath);
            //    return new ContractCancellationResponse
            //    {
            //        ResponseResult = ResponseResult.ReturnError(message)
            //    };
            //}

            ContractItemRepository contractItemRepository = new ContractItemRepository(this.OrgService);
            var equipment = contractItemRepository.getContractEquipmentByGivenColumns(contract.Id, new string[] { "rnt_campaignid" });
            bool isCampaignCancelable = true;
            if (equipment != null && equipment.Contains("rnt_campaignid"))
            {
                CampaignRepository campaignRepository = new CampaignRepository(this.OrgService);
                var campaign = campaignRepository.getCampaignById(equipment.GetAttributeValue<EntityReference>("rnt_campaignid").Id, new string[] { "rnt_iscancelable" });
                isCampaignCancelable = campaign.GetAttributeValue<bool>("rnt_iscancelable");
            }

            this.Trace("isCampaignCancelable: " + isCampaignCancelable);

            var _totalAmount = decimal.Zero;
            var isCorporateContract = false;
            this.Trace("before corp condition _totalAmount: " + _totalAmount);

            if (contract.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value != (int)rnt_PaymentMethodCode.Individual)
            {
                PaymentRepository paymentRepository = new PaymentRepository(this.OrgService);
                var payments = paymentRepository.getNotRefundedSalePayment_Contract(contract.Id);
                _totalAmount = payments.Sum(p => p.GetAttributeValue<Money>("rnt_amount").Value);
                isCorporateContract = true;
            }
            else
            {
                _totalAmount = (decimal)contract.GetAttributeValue<Money>("rnt_totalamount")?.Value;
            }
            this.Trace("after corp condition _totalAmount: " + _totalAmount);
            return new ContractCancellationResponse
            {
                totalAmount = _totalAmount,
                ResponseResult = ResponseResult.ReturnSuccess(),
                isCorporateContract = isCorporateContract,
                isCampaignCancelable = isCampaignCancelable,
                contractPaymetType = contract.GetAttributeValue<OptionSetValue>("rnt_paymentchoicecode").Value,
                willChargeFromUser = cancellationValidation.checkContractWillChargeFromUser(contractRelatedParameters.contractCancellationFineDuration, contract)
            };
        }

        public ContractFineAmountResponse calculateFineAmountForGivenContractFromMongoDB(Guid contractId, int langId)
        {
            ContractItemRepository contractItemRepository = new ContractItemRepository(this.OrgService);
            var contractItem = contractItemRepository.getContractEquipmentByGivenColumns(contractId, new string[] { });

            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
            //request initilization
            RestSharpHelper helper = new RestSharpHelper(responseUrl, "getFineAmountforContract", Method.POST);
            //prepare parameters
            helper.AddQueryParameter("contractItemId", Convert.ToString(contractItem.Id));
            // execute the request
            var response = helper.Execute<ContractFineAmountResponse>();
            if (!response.ResponseResult.Result)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                response.ResponseResult = ResponseResult.ReturnError(xrmHelper.GetXmlTagContentByGivenLangId("CannotFindFirstDayPrice", langId));
            }
            return response;
            //check response
        }

        public void calcultePaymentRelatedRollupFields(Guid contractId, int paymentTransactionType)
        {
            XrmHelper xrmHelper = new XrmHelper(this.OrgService);
            if ((int)PaymentEnums.PaymentTransactionType.DEPOSIT == paymentTransactionType ||
                (int)PaymentEnums.PaymentTransactionType.SALE == paymentTransactionType)
            {
                xrmHelper.CalculateRollupField("rnt_contract", contractId, "rnt_paidamount");
            }
            else if ((int)PaymentEnums.PaymentTransactionType.REFUND == paymentTransactionType)
            {
                xrmHelper.CalculateRollupField("rnt_contract", contractId, "rnt_refundamount");
            }

        }

        public ContractCancellationResponse calculateCancellationAmountForGivenContractByCancellationReason(ContractCancellationResponse validationResponse,
                                                                                                          Guid contractId,
                                                                                                          bool willChargeFromUser,
                                                                                                          int langId,
                                                                                                          int cancellationReason)
        {
            //PaymentRepository paymentRepository = new PaymentRepository(this.OrgService);
            //var payments = paymentRepository.getCountofNotRefundedPayments_Contract(contractId);

            ContractRepository contractRepository = new ContractRepository(this.OrgService);
            var payment = contractRepository.getContractById(contractId, new string[] { "rnt_netpayment", "rnt_paymentmethodcode" });
            //check reservation has payment record and 
            //reservation start date is close enough to check fine amount
            if (payment.GetAttributeValue<Money>("rnt_netpayment").Value > 0)
            {
                this.Trace("validationResponse.willChargeFromUser" + willChargeFromUser);
                if (validationResponse.willChargeFromUser && cancellationReason == (int)ContractEnums.CancellationReason.ByCustomer && !validationResponse.isCorporateContract)
                {
                    this.Trace("cancellationReason is by customer :" + cancellationReason);

                    var fineResponse = this.calculateFineAmountForGivenContractFromMongoDB(contractId, langId);
                    //if first day price couldnt found into system no need to continue
                    if (!fineResponse.ResponseResult.Result)
                    {
                        validationResponse.ResponseResult = fineResponse.ResponseResult;
                        return validationResponse;
                    }
                    ContractItemRepository contractItemRepository = new ContractItemRepository(this.OrgService);
                    var item = contractItemRepository.getDiscountContractItem(contractId, new string[] { "rnt_totalamount" });
                    var discountAmount = item == null ? decimal.Zero : item.GetAttributeValue<Money>("rnt_totalamount").Value;

                    validationResponse.fineAmount = fineResponse.firstDayAmount - (-1 * discountAmount);
                    validationResponse.refundAmount = validationResponse.totalAmount + (-1 * discountAmount) - fineResponse.firstDayAmount;

                    if (!validationResponse.isCampaignCancelable)
                    {
                        this.Trace("campaign is not cancelable");
                        validationResponse.fineAmount = validationResponse.totalAmount;
                        validationResponse.refundAmount = decimal.Zero;
                    }

                    this.Trace("fineAmount : " + validationResponse.fineAmount);
                    this.Trace("refundAmount : " + validationResponse.refundAmount);
                }
                else
                {
                    this.Trace("cancellationReason is by rentgo " + cancellationReason);
                    validationResponse.fineAmount = decimal.Zero;
                    validationResponse.refundAmount = validationResponse.totalAmount;
                }
            }
            return validationResponse;
        }

        public ContractCancellationResponse calculateCancellationAmountForGivenContract(ContractCancellationResponse validationResponse,
                                                                                        Guid contractId,
                                                                                        bool willChargeFromUser,
                                                                                        int langId)
        {
            PaymentRepository paymentRepository = new PaymentRepository(this.OrgService);
            var payments = paymentRepository.getCountofNotRefundedPayments_Contract(contractId);

            ContractHelper contractHelper = new ContractHelper(this.OrgService);
            var isCorpContract = contractHelper.checkMakePayment_CorporateContracts(contractId);
            //check contract has payment record and 
            //contract start date is close enough to check fine amount
            this.Trace("isCorpContract : " + isCorpContract);
            if (payments > 0 || isCorpContract)
            {
                this.Trace("validationResponse " + JsonConvert.SerializeObject(validationResponse));
                if (isCorpContract)
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
                    var fineResponse = this.calculateFineAmountForGivenContractFromMongoDB(contractId, langId);
                    //if first day price couldnt found into system no need to continue
                    if (!fineResponse.ResponseResult.Result)
                    {
                        validationResponse.ResponseResult = fineResponse.ResponseResult;
                        return validationResponse;
                    }
                    //discount check 


                    ContractItemRepository contractItemRepository = new ContractItemRepository(this.OrgService);
                    var item = contractItemRepository.getDiscountContractItem(contractId, new string[] { "rnt_totalamount" });
                    var discountAmount = item == null ? decimal.Zero : item.GetAttributeValue<Money>("rnt_totalamount").Value;

                    validationResponse.fineAmount = fineResponse.firstDayAmount - (-1 * discountAmount);
                    validationResponse.refundAmount = validationResponse.totalAmount + (-1 * discountAmount) - fineResponse.firstDayAmount;
                }
                else
                {
                    validationResponse.fineAmount = decimal.Zero;
                    validationResponse.refundAmount = validationResponse.totalAmount;
                }
            }
            return validationResponse;
        }

        public void cancelContract(string contractId, int statusCode, decimal fineAmount, ContractItemResponse contractItemResponse, bool isCorpContract)
        {
            ContractItemRepository contractItemRepository = new ContractItemRepository(this.OrgService);
            var contractItems = contractItemRepository.getWaitingForDeliveryContractItemsByContractId(new Guid(contractId));

            var contractEquipment = contractItems.Where(item => item.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value ==
                                                            (int)ClassLibrary._Enums_1033.rnt_contractitem_rnt_itemtypecode.Equipment).FirstOrDefault();

            contractItemResponse.contractItemId = contractEquipment.Id;
            contractItemResponse.itemTypeCode = (int)ClassLibrary._Enums_1033.rnt_contractitem_rnt_itemtypecode.Equipment;
            contractItemResponse.status = statusCode == (int)ContractEnums.CancellationReason.ByCustomer ?
                                            (int)ContractItemEnums.CancellationReason.ByCustomer : (int)ContractItemEnums.CancellationReason.ByRentgo;

            if (fineAmount != decimal.Zero)
            {
                InvoiceRepository invoiceRepository = new InvoiceRepository(this.OrgService);
                var invoices = invoiceRepository.getInvoicesByContractId(Guid.Parse(contractId));
                var firstInvoice = invoices.FirstOrDefault();
                this.Trace("firstInvoice  " + firstInvoice == null ? "isnull" : "is not null");
                invoices.Remove(firstInvoice);
                this.Trace("invoices length  " + invoices.Count);

                InvoiceBL invoiceBL = new InvoiceBL(this.OrgService);
                foreach (var item in invoices)
                {
                    invoiceBL.deactiveInvoice(item.Id, (int)InvoiceEnums.DeactiveStatus.Cancelled);
                }

                ContractItemBL contractItemBL = new ContractItemBL(this.OrgService);
                contractItemBL.createAdditionalProductForCancelContractFromContractItemWithInitializeFromRequest(contractItems.FirstOrDefault().Id, firstInvoice.Id, fineAmount);
            }
            InvoiceItemBL invoiceItemBL = new InvoiceItemBL(this.OrgService);

            foreach (var item in contractItems)
            {
                XrmHelper h = new XrmHelper(this.OrgService);

                InvoiceItemRepository invoiceItemRepository = new InvoiceItemRepository(this.OrgService);
                var invoiceItem = invoiceItemRepository.getInvoiceItemsByContractItemIdByGivenColumns(item.Id, new string[] { });
                this.Trace("item in contract items; Name: " + item.LogicalName);

                if (invoiceItem != null)
                {
                    invoiceItemBL.deactiveInvoiceItem(invoiceItem.Id, (int)rnt_invoiceitem_StatusCode.Inactive);
                }


                if (statusCode == (int)ContractEnums.CancellationReason.ByRentgo)
                    h.setState("rnt_contractitem", item.Id, (int)GlobalEnums.StateCode.Active, (int)ContractItemEnums.CancellationReason.ByRentgo);
                else if (statusCode == (int)ContractEnums.CancellationReason.ByCustomer)
                    h.setState("rnt_contractitem", item.Id, (int)GlobalEnums.StateCode.Active, (int)ContractItemEnums.CancellationReason.ByCustomer);
            }
            //now deactivate header
            XrmHelper h1 = new XrmHelper(this.OrgService);
            h1.setState("rnt_contract", new Guid(contractId), (int)GlobalEnums.StateCode.Active, statusCode);
        }

        public ContractRetrieveResponse getContractDataForUpdate(string contractId, ContractDateandBranchParameters selectedDateAndBranchData, int langid)
        {
            try
            {

                ContractHelper contractHelper = new ContractHelper(this.OrgService);
                var totalDuration = contractHelper.calculateTotalDuration_ContractByPriceHourEffect(selectedDateAndBranchData.pickupDate,
                                                                                                    selectedDateAndBranchData.dropoffDate);
                this.Trace("totalDuration : " + totalDuration);
                AdditionalProductsBL additionalProductsBL = new AdditionalProductsBL(this.OrgService, this.TracingService);
                var additionalProductsResponse = additionalProductsBL.GetAdditionalProductForUpdateContract(contractId, selectedDateAndBranchData, totalDuration, langid);
                //this.Trace("additionalProductsResponse" + JsonConvert.SerializeObject(additionalProductsResponse));
                if (!additionalProductsResponse.ResponseResult.Result)
                {
                    return new ContractRetrieveResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(additionalProductsResponse.ResponseResult.ExceptionDetail)
                    };
                }

                var existingAdditionalProducts = additionalProductsResponse.AdditionalProducts.Where(p => p.value >= 1).ToList();
                this.Trace("existingAdditionalProducts count : " + existingAdditionalProducts.Count);
                //recalculate the additionalproducts 
                selectedDateAndBranchData.pickupDate = DateTime.UtcNow;
                this.Trace("selectedDateAndBranchData.pickupDate : " + selectedDateAndBranchData.pickupDate);
                totalDuration = contractHelper.calculateTotalDuration_ContractByPriceHourEffect(selectedDateAndBranchData.pickupDate,
                                                                                                selectedDateAndBranchData.dropoffDate);
                this.Trace("totalDuration : " + totalDuration);

                var additionalProductsResponse_utc = additionalProductsBL.GetAdditionalProductForUpdateContract(contractId, selectedDateAndBranchData, totalDuration, langid);
                if (!additionalProductsResponse_utc.ResponseResult.Result)
                {
                    return new ContractRetrieveResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(additionalProductsResponse.ResponseResult.ExceptionDetail)
                    };
                }
                var notexistingAdditionalProducts = additionalProductsResponse_utc.AdditionalProducts.Where(p => p.value == 0).ToList();
                this.Trace("notexistingAdditionalProducts count : " + notexistingAdditionalProducts.Count);

                AdditionalDriverRepository additionalDriverRepository = new AdditionalDriverRepository(this.OrgService);
                var additionalDrivers = additionalDriverRepository.getAdditionalDriversByContractId(Guid.Parse(contractId));
                List<AdditionalDriverData> additionalDriverData = new List<AdditionalDriverData>();
                foreach (var item in additionalDrivers)
                {
                    IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(this.OrgService);
                    var customer = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(item.GetAttributeValue<EntityReference>("rnt_contactid").Id,
                                                                                                          new string[] { "firstname",
                                                                                                                         "lastname",
                                                                                                                         "governmentid",
                                                                                                                         "rnt_passportnumber",
                                                                                                                         "mobilephone",
                                                                                                                         "rnt_dialcode",
                                                                                                                         "birthdate",
                                                                                                                         "rnt_drivinglicensedate",
                                                                                                                         "rnt_isturkishcitizen",
                                                                                                                         "rnt_citizenshipid"});
                    additionalDriverData.Add(new AdditionalDriverData
                    {
                        nationalityId = customer.Attributes.Contains("rnt_citizenshipid") ?
                                    (Guid?)customer.GetAttributeValue<EntityReference>("rnt_citizenshipid").Id :
                                    null,
                        nationalityIdName = customer.Attributes.Contains("rnt_citizenshipid") ?
                                      customer.GetAttributeValue<EntityReference>("rnt_citizenshipid").Name :
                                      null,
                        additionalDriverId = item.Id,
                        contactId = customer.Id,
                        contractId = Guid.Parse(contractId),
                        firstName = customer.GetAttributeValue<string>("firstname"),
                        lastName = customer.GetAttributeValue<string>("lastname"),
                        governmentId = customer.Attributes.Contains("governmentid") ? customer.GetAttributeValue<string>("governmentid") : string.Empty,
                        passportNumber = customer.Attributes.Contains("rnt_passportnumber") ? customer.GetAttributeValue<string>("rnt_passportnumber") : string.Empty,
                        mobilePhone = customer.Attributes.Contains("mobilephone") ? customer.GetAttributeValue<string>("mobilephone") : string.Empty,
                        dialCode = customer.Attributes.Contains("rnt_dialcode") ? customer.GetAttributeValue<string>("rnt_dialcode") : string.Empty,
                        birthDate = customer.GetAttributeValue<DateTime>("birthdate"),
                        drivingLicenseDate = customer.GetAttributeValue<DateTime>("rnt_drivinglicensedate"),
                        isTurkishCitizen = customer.GetAttributeValue<bool>("rnt_isturkishcitizen")
                    });
                }


                ContractRepository contractRepository = new ContractRepository(this.OrgService);
                var contract = contractRepository.getContractById(Guid.Parse(contractId), new string[] { "new_findeks" ,
                                                                                                         "new_minimumage" ,
                                                                                                         "new_minimumdriverlicense",
                                                                                                         "new_youngdriverage",
                                                                                                         "new_youngdriverlicense",
                                                                                                         "rnt_depositamount",
                                                                                                         "rnt_kilometerlimit",
                                                                                                         "rnt_paymentmethodcode",
                                                                                                         "rnt_contracttypecode",
                                                                                                         "rnt_customerid",
                                                                                                         "rnt_pricinggroupcodeid"});
                var groupCodeInformation = new GroupCodeInformationDetailData
                {
                    findeks = contract.Attributes.Contains("new_findeks") ? contract.GetAttributeValue<int>("new_findeks") : 0,
                    minimumAge = contract.Attributes.Contains("new_minimumage") ? contract.GetAttributeValue<int>("new_minimumage") : 0,
                    minimumDriverLicense = contract.Attributes.Contains("new_minimumdriverlicense") ? contract.GetAttributeValue<int>("new_minimumdriverlicense") : 0,
                    youngDriverAge = contract.Attributes.Contains("new_youngdriverage") ? contract.GetAttributeValue<int>("new_youngdriverage") : 0,
                    youngDriverMinimumLicense = contract.Attributes.Contains("new_youngdriverlicense") ? contract.GetAttributeValue<int>("new_youngdriverlicense") : 0,
                    deposit = contract.Attributes.Contains("rnt_depositamount") ? contract.GetAttributeValue<Money>("rnt_depositamount").Value : decimal.Zero
                };

                var priceGroupCodeId = contract.Attributes.Contains("rnt_pricinggroupcodeid") ? contract.GetAttributeValue<EntityReference>("rnt_pricinggroupcodeid").Id : Guid.Empty;

                ContractItemBL contractItemBL = new ContractItemBL(this.OrgService, this.TracingService);
                var equipmentPrice = contractItemBL.getContractEquipmentBasePrice(contractId);

                existingAdditionalProducts.AddRange(notexistingAdditionalProducts);
                this.Trace("final count " + existingAdditionalProducts.Count);
                return new ContractRetrieveResponse
                {
                    contactId = contract.GetAttributeValue<EntityReference>("rnt_customerid").Id,
                    contractType = contract.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value,
                    paymentMethod = contract.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value,
                    additionalProducts = existingAdditionalProducts,
                    groupCodeInformation = groupCodeInformation,
                    priceGroupCodeId = priceGroupCodeId,
                    additionalDrivers = additionalDriverData,
                    contractKilometerLimit = contract.Attributes.Contains("rnt_kilometerlimit") ? contract.GetAttributeValue<int>("rnt_kilometerlimit") : 0,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new ContractRetrieveResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        public void updateContractRentalUser(Guid contractId, Guid userId)
        {
            Entity e = new Entity("rnt_contract");
            e["rnt_contractrentaluserid"] = new EntityReference("rnt_serviceuser", userId);
            e.Id = contractId;
            this.OrgService.Update(e);
        }
        public void updateContractDeliveryUser(Guid contractId, Guid userId)
        {
            Entity e = new Entity("rnt_contract");
            e["rnt_contractdeliveryuserid"] = new EntityReference("rnt_serviceuser", userId);
            e.Id = contractId;
            this.OrgService.Update(e);
        }
        public ContractUpdateResponse updateContract(ContractUpdateParameters parameters, List<ContractItemResponse> contractItemResponses)
        {
            var utcNow = DateTime.UtcNow;
            this.Trace("utcNow : " + utcNow);

            ContractItemRepository contractItemRepository = new ContractItemRepository(this.OrgService);
            #region Calculate total Duration
            ContractHelper contractHelper = new ContractHelper(this.OrgService);
            var duration = contractHelper.calculateTotalDuration_ContractByPriceHourEffect(parameters.dateAndBranch.pickupDate,
                                                                                           parameters.dateAndBranch.dropoffDate);

            parameters.totalDuration = duration;
            //this.Trace("parameters.totalDuration : " + parameters.totalDuration);
            #endregion

            #region Get Contract related columns
            // get all waiting for delivery contract items
            ContractRepository contractRepository = new ContractRepository(this.OrgService);
            var contract = contractRepository.getContractById(parameters.contractId, new string[] { "statuscode", "rnt_pnrnumber", "rnt_paymentchoicecode", "rnt_pricinggroupcodeid" });

            var pnrNumber = contract.Attributes.Contains("rnt_pnrnumber") ? contract.GetAttributeValue<string>("rnt_pnrnumber") : string.Empty;
            var paymentChoice = contract.GetAttributeValue<OptionSetValue>("rnt_paymentchoicecode").Value;
            parameters.contractStatusCode = contract.GetAttributeValue<OptionSetValue>("statuscode").Value;
            #endregion

            #region Get Contract Items
            var contractItems = parameters.contractStatusCode == (int)ContractEnums.StatusCode.WaitingforDelivery ?
                                   contractItemRepository.getWaitingForDeliveryContractItemsByContractId(parameters.contractId)
                                   :
                                       parameters.contractStatusCode == (int)ContractEnums.StatusCode.Rental ?
                                           contractItemRepository.getRentalContractItemsByContractId(parameters.contractId)
                                           :
                                               new List<Entity>();
            this.Trace("contractItems retrieveds count : " + contractItems.Count);
            #endregion

            #region Get Contract Equipment
            var contractEquipmentItem = contractItems.Where(x => x.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value ==
                                                                 (int)ContractItemEnums.ItemTypeCode.Equipment).FirstOrDefault();
            #endregion
            this.Trace("parameters : " + JsonConvert.SerializeObject(parameters));
            this.Trace("enum limited " + ((int)PaymentEnums.PaymentMethodType.LIMITEDCREDIT).ToString());

            // Check if the equipment billing type is changed (limited agency)
            this.Trace("billing start");
            if (parameters.priceParameters.pricingType == ((int)PaymentEnums.PaymentMethodType.LIMITEDCREDIT).ToString())
            {
                this.Trace("parameters.contractEquipment.billingType: " + parameters.contractEquipment.billingType.ToString());
                this.Trace("contractEquipmentItem.rnt_billingtype: " + contractEquipmentItem.GetAttributeValue<OptionSetValue>("rnt_billingtype").Value.ToString());
                this.Trace("setting parameter value");
                Entity entity = new Entity("rnt_contractitem");
                entity.Id = contractEquipmentItem.Id;
                entity["rnt_billingtype"] = new OptionSetValue((int)parameters.contractEquipment.billingType);
                this.OrgService.Update(entity);
            }
            this.Trace("billing end");

            #region Check Date are changed
            parameters.isDateorBranchChanged = CommonHelper.isDateorBranchChanged(contractEquipmentItem.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                                                                                   contractEquipmentItem.GetAttributeValue<DateTime>("rnt_dropoffdatetime"),
                                                                                   contractEquipmentItem.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id,
                                                                                   contractEquipmentItem.GetAttributeValue<EntityReference>("rnt_dropoffbranch").Id,
                                                                                   parameters.dateAndBranch.pickupDate.AddMinutes(StaticHelper.offset),
                                                                                   parameters.dateAndBranch.dropoffDate.AddMinutes(StaticHelper.offset),
                                                                                   parameters.dateAndBranch.pickupBranchId,
                                                                                   parameters.dateAndBranch.dropoffBranchId);
            this.Trace("parameters.isDateorBranchChanged" + parameters.isDateorBranchChanged);
            #endregion

            #region Contract header status mapping to contract items
            ContractItemBL contractItemBL = new ContractItemBL(this.OrgService);
            parameters.contractItemStatusCode = ContractMapper.getContractItemStatusCodeByContractStatusCode(parameters.contractStatusCode);
            #endregion

            #region Get First Invoice
            InvoiceRepository invoiceRepository = new InvoiceRepository(this.OrgService);
            var invoice = invoiceRepository.getFirstInvoiceByContractId(parameters.contractId);
            #endregion

            #region If Equipment changed
            if (parameters.isCarChanged)
            {
                #region Get user branch
                SystemUserRepository systemUserRepository = new SystemUserRepository(this.OrgService);
                BranchRepository branchRepository = new BranchRepository(this.OrgService);

                var user = systemUserRepository.getCurrentUser(new string[] { "businessunitid" });
                var businessUnitId = user.GetAttributeValue<EntityReference>("businessunitid").Id;
                var branch = branchRepository.getBranchByBusinessUnitId(businessUnitId).FirstOrDefault();
                #endregion

                #region Price Calculation --> Split, Clone operations
                this.Trace("parameters.trackingNumber : " + parameters.trackingNumber);
                this.Trace("pricing groupcodeid : " + contract.GetAttributeValue<EntityReference>("rnt_pricinggroupcodeid").Id);
                var priceResponse = this.updateEquipmentPrice(contractEquipmentItem.Id.ToString(),
                                                              parameters.trackingNumber,
                                                              contract.GetAttributeValue<EntityReference>("rnt_pricinggroupcodeid").Id.ToString(),
                                                              Convert.ToString(parameters.dateAndBranch.pickupBranchId),
                                                              Convert.ToString(parameters.contractId),
                                                              parameters.priceParameters.campaignId,
                                                              paymentChoice,
                                                              utcNow);

                this.Trace("price response" + JsonConvert.SerializeObject(priceResponse));
                #endregion

                #region Create New Contract Equipment
                var contractItemId = contractItemBL.createContractItemForEqiupmentFromContractItemWithInitializeFromRequest(contractEquipmentItem.Id,
                                                                                                                            priceResponse.newEquipmentPrice,
                                                                                                                            priceResponse.newTrackingNumber,
                                                                                                                            utcNow,
                                                                                                                            parameters.dateAndBranch.dropoffDate,
                                                                                                                            branch);
                this.Trace("after createContractItemForEqiupmentFromContractItemWithInitializeFromRequest");
                #endregion

                #region Add to mongodb List
                contractItemResponses.Add(new ContractItemResponse
                {
                    contractItemId = contractItemId,
                    status = (int)rnt_contractitem_StatusCode.WaitingForDelivery
                });
                #endregion

                #region Update old contract item equipment
                var dateandBranch = new ContractDateandBranchParameters
                {
                    pickupDate = parameters.dateAndBranch.pickupDate,
                    pickupBranchId = parameters.dateAndBranch.pickupBranchId,
                    dropoffDate = parameters.dateAndBranch.dropoffDate,
                    dropoffBranchId = branch != null ? branch.Id : parameters.dateAndBranch.dropoffBranchId
                };

                this.Trace("updateContractItemforEquipmentChangeOnContractUpdate" + JsonConvert.SerializeObject(dateandBranch));
                this.Trace("pickup date removed");

                contractItemBL.updateContractItemforEquipmentChangeOnContractUpdate(contractEquipmentItem.Id,
                                                                                    priceResponse.oldEquipmentPrice,
                                                                                    dateandBranch,
                                                                                    priceResponse.oldTrackingNumber,
                                                                                    utcNow,
                                                                                    (int)ContractItemEnums.StatusCode.Rental);

                #endregion

                #region Add to mongodb List
                contractItemResponses.Add(new ContractItemResponse
                {
                    contractItemId = contractEquipmentItem.Id,
                    status = (int)rnt_contractitem_StatusCode.Rental
                });

                this.Trace("contractItemBL.updateContractEquipmentDateandBranch");
                #endregion
            }
            #endregion

            #region if equipment is not changed and date and branch changed
            if (parameters.isDateorBranchChanged && !parameters.isCarChanged)
            {
                this.Trace("parameters.isDateorBranchChanged && !parameters.isCarChanged");

                contractItemBL.updateContractEquipmentDateandBranch(parameters.contractId,
                                                                    contractEquipmentItem.Id,
                                                                    parameters.priceParameters.price,
                                                                    parameters.dateAndBranch,
                                                                    parameters.trackingNumber,
                                                                    utcNow,
                                                                    parameters.contractStatusCode,
                                                                    parameters.isCarChanged);
                contractItemResponses.Add(new ContractItemResponse
                {
                    contractItemId = contractEquipmentItem.Id,
                    status = (int)rnt_contractitem_StatusCode.Rental
                });
            }
            #endregion

            this.Trace("Update Additional Products start");
            #region Update Additional Products
            //this.Trace("updateContract parameters: " + JsonConvert.SerializeObject(parameters));
            AdditionalProductsBL additionalProductsBL = new AdditionalProductsBL(this.OrgService, this.TracingService);
            additionalProductsBL.updateAdditionalProductsForContract(parameters, contractItems, invoice.Id, parameters.channelCode, null, true);
            #endregion

            #region Update Contract Header
            this.Trace("contractItemBL.updateContractHeader before");
            //update contract header finally;
            this.updateContractHeader(parameters.contractId,
                                      parameters.dateAndBranch,
                                      parameters.additionalProduct.selectedAdditionalProductsData,
                                      parameters.totalDuration);
            this.Trace("contractItemBL.updateContractHeader end");
            #endregion

            return new ContractUpdateResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess(),
                contractId = parameters.contractId,
                pnrNumber = pnrNumber
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="equipmentTrackingNumber">This tracking number value in Old Contract Item for daily prices</param>
        /// <param name="newEquipmentTrackingNumber">This parameter is required for getting price calculation summaries and if dates are changed new days tracking number</param>
        /// <param name="groupCodeInformationId"></param>
        public UpdateContractEquipmentPrices updateEquipmentPrice(string contractItemId,
                                                                  string parametersTrackingNumber,
                                                                  string groupCodeInformationId,
                                                                  string pickupBranchId,
                                                                  string documentId,
                                                                  Guid? campaignId,
                                                                  int paymentChoice,
                                                                  DateTime utcNow)
        {
            this.Trace("contractItemId" + contractItemId);
            var splitLastDayPrices = true;
            AvailibilityBL availibilityBL = new AvailibilityBL(this.OrgService, this.TracingService);
            var response = availibilityBL.getPriceCalculationPriceSummaries(parametersTrackingNumber,
                                                                            groupCodeInformationId,
                                                                            Convert.ToString(campaignId),
                                                                            pickupBranchId,
                                                                            documentId);
            // this.Trace("getPriceCalculationPriceSummaries : " + JsonConvert.SerializeObject(response));

            // this.Trace("response.dailyPrices" + JsonConvert.SerializeObject(response.dailyPrices));
            var beforeSplitDays = response.dailyPrices.Where(p => p._priceDateTimeStamp.converttoDateTime() < utcNow).ToList();
            var afterSplitDays = response.dailyPrices.Where(p => p._priceDateTimeStamp.converttoDateTime() > utcNow).ToList();

            //iade günü araç değişiklği yapılıyorsa!
            if (afterSplitDays.Count == 0)
            {
                var x = beforeSplitDays.OrderByDescending(p => p.priceDate).FirstOrDefault();
                x.dailyPricesId = Guid.NewGuid();
                x.totalAmount = decimal.Zero;
                afterSplitDays.Add(x);
                splitLastDayPrices = false;
            }
            this.Trace("campaignId : " + Convert.ToString(campaignId));
            //this.Trace("beforeSplitDays : " + JsonConvert.SerializeObject(beforeSplitDays));
            //this.Trace("afterSplitDays : " + JsonConvert.SerializeObject(afterSplitDays));

            var splitDay = beforeSplitDays.OrderByDescending(x => x._priceDateTimeStamp.converttoDateTime()).FirstOrDefault();

            var splittedDayAmount = paymentChoice == (int)PaymentEnums.PaymentType.PayLater ?
                                    splitDay.payLaterAmount :
                                    splitDay.payNowAmount;
            //this.Trace("beforeSplitDays" + JsonConvert.SerializeObject(beforeSplitDays));
            //this.Trace("afterSplitDays" + JsonConvert.SerializeObject(afterSplitDays));
            //this.Trace("splitDay" + JsonConvert.SerializeObject(splitDay));

            //araç değişikliğinde kesişen günleri kopyalayıp , getirdiği saate göre bir önceki ve bir sonraki güne yedirir
            var cloneJson = JsonConvert.SerializeObject(splitDay);
            var clonedSplitDay = JsonConvert.DeserializeObject<DailyPrice>(cloneJson);
            //this.Trace("clonedSplitDay" + JsonConvert.SerializeObject(clonedSplitDay));
            var totalMinutesDifference = (utcNow - clonedSplitDay._priceDateTimeStamp.converttoDateTime()).TotalMinutes;
            var totalMinutesDifferenceAmount = (splittedDayAmount * Convert.ToInt32(totalMinutesDifference)) / 1440;
            //this.Trace("totalMinutesDifference" + totalMinutesDifference);
            //this.Trace("totalMinutesDifferenceAmount" + totalMinutesDifferenceAmount);

            clonedSplitDay.totalAmount = splittedDayAmount - totalMinutesDifferenceAmount;
            //this.Trace("clonedSplitDay.totalAmount" + clonedSplitDay.totalAmount);

            var oldEquipmentPrice = splittedDayAmount - decimal.Round(clonedSplitDay.totalAmount, 2, MidpointRounding.AwayFromZero);
            var newEquipmentPrice = decimal.Round(clonedSplitDay.totalAmount, 2, MidpointRounding.AwayFromZero);
            //his.Trace("oldEquipmentPrice" + oldEquipmentPrice);
            //this.Trace("newEquipmentPrice" + newEquipmentPrice);

            //old days    
            #region oldDays
            if (paymentChoice == (int)PaymentEnums.PaymentType.PayLater)
            {
                splitDay.payLaterAmount = oldEquipmentPrice;
            }
            else
            {
                splitDay.payNowAmount = oldEquipmentPrice;
            }
            splitDay.totalAmount = oldEquipmentPrice;

            var oldDays = beforeSplitDays.Where(p => p.dailyPricesId != splitDay.dailyPricesId).ToList();
            oldDays.Add(splitDay);
            var oldDaysTrackingNumber = Guid.NewGuid().ToString();
            foreach (var item in oldDays)
            {
                item.trackingNumber = oldDaysTrackingNumber;
            }
            // this.Trace("oldDays" + JsonConvert.SerializeObject(oldDays));
            #endregion

            //new days       
            #region newDays
            if (paymentChoice == (int)PaymentEnums.PaymentType.PayLater)
            {
                clonedSplitDay.payLaterAmount = newEquipmentPrice;
            }
            else
            {
                clonedSplitDay.payNowAmount = newEquipmentPrice;
            }
            clonedSplitDay.totalAmount = newEquipmentPrice;

            var newDays = afterSplitDays.Where(p => p.dailyPricesId != clonedSplitDay.dailyPricesId).ToList();
            newDays.Add(clonedSplitDay);
            var newDaysTrackingNumber = Guid.NewGuid().ToString();

            foreach (var item in newDays.OrderBy(p => p.priceDate).ToList())
            {
                item.trackingNumber = newDaysTrackingNumber;
                item.priceDate = utcNow;
                item._priceDateTimeStamp = utcNow.converttoTimeStamp();
                utcNow = utcNow.AddDays(1);
            }
            // this.Trace("newDays" + JsonConvert.SerializeObject(newDays));
            #endregion

            ContractRepository contractRepository = new ContractRepository(this.OrgService);
            var c = contractRepository.getContractById(new Guid(documentId), new string[] { "rnt_dropoffdatetime" });

            var maxDate = newDays.OrderByDescending(p => p.priceDate).FirstOrDefault();
            //eğer fiyatın son günü , sözleşmenin kapanış tarihine eşit oluyorsa günleri yedir.
            if (maxDate.priceDate.Date == c.GetAttributeValue<DateTime>("rnt_dropoffdatetime").Date && splitLastDayPrices)
            {
                newDays = newDays.Where(p => p.dailyPricesId != maxDate.dailyPricesId).ToList();
                if (newDays.Count == 0)
                {
                    throw new Exception("Araç değişikliği sırasında fiyatlar hesaplanamadı");
                }
                var x = maxDate.totalAmount / newDays.Count;
                // günün fiyatını yedirelim
                foreach (var item in newDays)
                {
                    item.totalAmount += maxDate.totalAmount / newDays.Count;
                    item.payLaterAmount = item.totalAmount;
                    item.payNowAmount = item.totalAmount;
                    this.Trace("date : " + item.priceDate);
                }
                AnnotationBL annotationBL = new AnnotationBL(this.OrgService);
                annotationBL.createNewAnnotation(new AnnotationData
                {
                    NoteText = maxDate.priceDate.Date + " günü fiyatı " + newDays.Count + " bölünerek , diğer günlere eklendi.( eklenen tutar : " + x + " )",
                    ObjectId = new Guid(documentId),
                    ObjectName = "rnt_contract",
                    Subject = "Fiyat Yedirme"
                });
            }

            #region MongoDB Price Operations
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService, this.TracingService);
            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "createContractDailyPricesFromScratch", Method.POST);

            var totalDays = new List<DailyPrice>();
            totalDays.AddRange(oldDays);
            totalDays.AddRange(newDays);

            //now delete old daily prices and insert new ones respectively
            var createContractDailyPricesFromScratchParameters = new CreateContractDailyPricesFromScratchParameters
            {
                dailyPriceList = totalDays,
                groupCodeId = Convert.ToString(groupCodeInformationId), // because in contract car change always same group code,
                contractItemId = contractItemId,
            };
            restSharpHelper.AddJsonParameter<CreateContractDailyPricesFromScratchParameters>(createContractDailyPricesFromScratchParameters);
            var responseDailyPrices = restSharpHelper.Execute<CreateContractDailyPricesFromScratchResponse>();
            if (!responseDailyPrices.ResponseResult.Result)
            {
                throw new Exception(responseDailyPrices.ResponseResult.ExceptionDetail);
            }
            //todo paylater pay now check
            var oldDaysTotal = decimal.Zero;
            if (paymentChoice == (int)PaymentEnums.PaymentType.PayLater)
            {
                oldDaysTotal = oldDays.Sum(p => p.payLaterAmount);
            }
            else
            {
                oldDaysTotal = oldDays.Sum(p => p.payNowAmount);

            }

            var newDaysTotal = decimal.Zero;
            if (paymentChoice == (int)PaymentEnums.PaymentType.PayLater)
            {
                newDaysTotal = newDays.Sum(p => p.payLaterAmount);
            }
            else
            {
                newDaysTotal = newDays.Sum(p => p.payNowAmount);
            }

            this.Trace("oldDaysTotal" + oldDaysTotal);
            this.Trace("newDaysTotal" + newDaysTotal);

            var returnObject = new UpdateContractEquipmentPrices
            {
                oldEquipmentPrice = oldDaysTotal,
                oldTrackingNumber = oldDaysTrackingNumber,
                newEquipmentPrice = newDaysTotal,
                newTrackingNumber = newDaysTrackingNumber
            };
            this.Trace("returnObject" + JsonConvert.SerializeObject(returnObject));


            return returnObject;

            #endregion


        }
        /// <summary>
        /// move reservation payments on the contract
        /// </summary>
        /// <param name="contractId"></param>
        /// <param name="reservationId"></param>
        public void updateReservationPaymentsWithContract(Guid contractId, Guid reservationId)
        {
            PaymentRepository paymentRepository = new PaymentRepository(this.OrgService);
            var payments = paymentRepository.getAllPaymentandRefund_Reservation(reservationId);
            foreach (var item in payments)
            {
                Entity e = new Entity("rnt_payment");
                e.Attributes["rnt_contractid"] = new EntityReference("rnt_contract", contractId);
                e.Id = item.Id;
                this.OrgService.Update(e);
            }
        }
        public void updateReservationCreditCardSlipsWithContract(Guid contractId, Guid reservationId)
        {
            CreditCardSlipRepository creditCardSlipRepository = new CreditCardSlipRepository(this.OrgService);
            var creditCardSlips = creditCardSlipRepository.getCreditCardSlipsByReservationId(reservationId, new string[] { });
            this.Trace("updateReservationCreditCardSlipsWithContract --> creditCardSlips length : " + creditCardSlips.Count);
            foreach (var item in creditCardSlips)
            {
                Entity e = new Entity("rnt_creditcardslip");
                e.Attributes["rnt_contractid"] = new EntityReference("rnt_contract", contractId);
                e.Id = item.Id;
                this.OrgService.Update(e);
            }
        }

        public ContractCreateResponse makeContractPayment(decimal? diffAmount,
                                                          decimal? depositAmount,
                                                          CreditCardData paymentCard,
                                                          CreditCardData depositCard,
                                                          InvoiceAddressData invoiceAddress,
                                                          Guid currency,
                                                          Guid contactId,
                                                          Guid contractId,
                                                          Guid reservationId,
                                                          string pnrNumber,
                                                          PaymentStatus paymentStatus,
                                                          int langId,
                                                          int? virtualPosId,
                                                          int installment,
                                                          rnt_PaymentChannelCode paymentChannelCode,
                                                          bool use3DSecure,
                                                          string callBackUrl)
        {
            PaymentBL paymentBL = new PaymentBL(this.OrgService, this.TracingService);

            var response = new CreatePaymentResponse();
            var createPaymentParameters = new CreatePaymentParameters();
            var refundEntities = new List<Entity>();
            this.Trace("contractId : " + contractId);
            this.Trace("reservationId : " + reservationId);
            if (diffAmount < 0)
            {
                this.Trace("diff < 0 : " + diffAmount);

                refundEntities = paymentBL.createRefund(new CreateRefundParameters
                {
                    isDepositRefund = false,
                    refundAmount = ((decimal)-1 * diffAmount).Value,
                    contractId = contractId
                });
            }
            else
            {
                this.Trace("!paymentStatus.isReservationPaid " + !paymentStatus.isReservationPaid);
                this.Trace("diffAmount.Value" + diffAmount.Value);

                if (!paymentStatus.isReservationPaid)
                {
                    try
                    {
                        // if it's not have any diff, get reservation payment
                        if (diffAmount > StaticHelper._one_)
                        {
                            this.Trace("start payment for : " + diffAmount.Value);
                            createPaymentParameters = new CreatePaymentParameters
                            {
                                contractId = contractId,
                                transactionCurrencyId = currency,
                                individualCustomerId = contactId,
                                conversationId = pnrNumber,
                                langId = langId,
                                paymentTransactionType = (int)PaymentEnums.PaymentTransactionType.SALE,
                                creditCardData = paymentCard,
                                installment = installment, // installment can not be selected during contract creation
                                paidAmount = diffAmount.Value,
                                invoiceAddressData = invoiceAddress,
                                virtualPosId = virtualPosId,
                                paymentChannelCode = paymentChannelCode,
                                use3DSecure = use3DSecure,
                                callBackUrl = callBackUrl,
                                pnrNumber = pnrNumber,
                                rollbackOperation = true
                            };
                            this.Trace("!paymentStatus.isDepositPaid" + !paymentStatus.isDepositPaid);
                            response = paymentBL.callMakePaymentAction(createPaymentParameters);

                            //this.Trace("payment response : " + JsonConvert.SerializeObject(response));
                        }
                        else
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(paymentCard.cardToken) && string.IsNullOrEmpty(paymentCard.cardUserKey) && paymentCard != null)
                                {
                                    var creditCardParams = new CreateCreditCardParameters
                                    {
                                        cardAlias = paymentCard.creditCardNumber,
                                        cardHolderName = paymentCard.cardHolderName,
                                        creditCardNumber = paymentCard.creditCardNumber.removeEmptyCharacters(),
                                        cvc = paymentCard.cvc,
                                        expireMonth = paymentCard.expireMonth,
                                        expireYear = paymentCard.expireYear,
                                        individualCustomerId = Convert.ToString(contactId),
                                        langId = langId
                                    };
                                    OrganizationRequest organizationRequest = new OrganizationRequest("rnt_CreateCustomerCreditCard");
                                    organizationRequest["creditCardParameters"] = Convert.ToString(JsonConvert.SerializeObject(creditCardParams));
                                    this.OrgService.Execute(organizationRequest);
                                }
                            }
                            catch (Exception ex)
                            {

                                this.Trace("create credit car error :" + ex.Message);
                            }


                        }
                        // if payment could not take, contract should not be created
                    }
                    catch (Exception ex)
                    {
                        return new ContractCreateResponse
                        {
                            ResponseResult = ResponseResult.ReturnError(ex.Message),
                            paymentStatus = new PaymentStatus
                            {
                                isReservationPaid = false,
                                isDepositPaid = false
                            }
                        };
                        //throw new InvalidPluginExecutionException(JsonConvert.SerializeObject(errResponse));
                    }
                }
            }

            this.Trace("!paymentStatus.isDepositPaid" + !paymentStatus.isDepositPaid);

            if (!paymentStatus.isDepositPaid)
            {
                //if it's double credit card, take deposit payment from deposit card
                try
                {
                    if (depositAmount < 0)
                    {
                        this.Trace("depositAmount diff < 0 : " + depositAmount);

                        paymentBL.createRefund(new CreateRefundParameters
                        {
                            isDepositRefund = true,
                            refundAmount = ((decimal)-1 * depositAmount).Value,
                            contractId = contractId
                        });
                    }
                    else
                    {
                        this.Trace("deposit amount : " + depositAmount);
                        if (depositCard != null && depositAmount > 0)
                        {
                            var depositCreatePaymentParameters = new CreatePaymentParameters
                            {
                                contractId = contractId,
                                transactionCurrencyId = currency,
                                individualCustomerId = contactId,
                                conversationId = pnrNumber,
                                langId = langId, // todo langid will be added from actin parameters 
                                paymentTransactionType = (int)PaymentEnums.PaymentTransactionType.DEPOSIT,
                                creditCardData = depositCard != null ? depositCard : paymentCard,
                                installment = 1,
                                paidAmount = depositAmount.Value,
                                paymentChannelCode = paymentChannelCode,
                                invoiceAddressData = invoiceAddress,
                                virtualPosId = virtualPosId,
                                use3DSecure = use3DSecure,
                                callBackUrl = callBackUrl,
                                pnrNumber = pnrNumber,
                                rollbackOperation = true
                            };
                            var depositResponse = paymentBL.callMakePaymentAction(depositCreatePaymentParameters);
                        }
                    }
                }
                catch (Exception ex)
                {

                    if (diffAmount < 0 && refundEntities.Count > 0)
                    {
                        this.Trace("refund entities will be created");

                        createPaymentParameters.reservationId = null;
                        createPaymentParameters.contractId = null;

                        if (reservationId != Guid.Empty)
                            createPaymentParameters.reservationId = reservationId;
                        else
                            createPaymentParameters.contractId = contractId;

                        List<RefundData> refundDatas = new List<RefundData>();
                        foreach (var item in refundEntities)
                        {
                            //this.Trace("item" + JsonConvert.SerializeObject(item));
                            refundDatas.Add(new RefundData
                            {
                                contractId = contractId,
                                reservationId = reservationId,
                                refundAmount = item.GetAttributeValue<Money>("rnt_amount").Value,
                                paymentResultId = item.GetAttributeValue<string>("rnt_paymentresultid"),
                                paymentTransactionId = item.GetAttributeValue<string>("rnt_paymenttransactionid"),
                                transactionResult = item.Attributes.Contains("rnt_refundstatuscode") ? (int)rnt_payment_rnt_transactionresult.Success : (int)rnt_payment_rnt_transactionresult.Error,
                                paymentChannelCode = paymentChannelCode,
                                customerCreditCard = item.GetAttributeValue<EntityReference>("rnt_customercreditcardid")?.Id,
                                parentPaymentId = item.GetAttributeValue<EntityReference>("rnt_parentpaymentid")?.Id,
                                contactId = item.GetAttributeValue<EntityReference>("rnt_contactid")?.Id,
                                transactionCurrencyId = currency
                            });
                        }
                        this.Trace("refundEntities : " + JsonConvert.SerializeObject(refundDatas));
                        var createPaymentWithServiceParameters = new CreatePaymentWithServiceParameters
                        {
                            refundEntities = refundDatas
                        };
                        paymentBL.callCreateRefundRecordService(createPaymentWithServiceParameters);
                        this.Trace("before sleep" + DateTime.UtcNow);
                        Thread.Sleep(5000);
                        this.Trace("after sleep" + DateTime.UtcNow);
                    }
                    else if (!string.IsNullOrEmpty(response.paymentId))
                    {
                        this.Trace("response.paymentId : " + response.paymentId);

                        var paymentResponse = new PaymentResponse
                        {
                            paymentId = response.externalPaymentId,
                            paymentTransactionId = response.externalPaymentTransactionId,
                            status = true
                        };
                        this.Trace("paymentResponse : " + JsonConvert.SerializeObject(paymentResponse));
                        createPaymentParameters.reservationId = null;
                        createPaymentParameters.contractId = null;

                        if (reservationId != Guid.Empty)
                            createPaymentParameters.reservationId = reservationId;
                        else
                            createPaymentParameters.contractId = contractId;

                        var createPaymentWithServiceParameters = new CreatePaymentWithServiceParameters
                        {
                            createPaymentParameters = createPaymentParameters,
                            paymentResponse = paymentResponse
                        };
                        paymentBL.callCreatePaymentRecordService(createPaymentWithServiceParameters);
                        this.Trace("before sleep" + DateTime.UtcNow);
                        Thread.Sleep(5000);
                        this.Trace("after sleep" + DateTime.UtcNow);

                        //this.Trace("serviceResponse : " + JsonConvert.SerializeObject(serviceResponse));
                    }
                    return new ContractCreateResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(ex.Message),
                        paymentStatus = new PaymentStatus
                        {
                            isReservationPaid = true,
                            isDepositPaid = false
                        }
                    };

                    //throw new InvalidPluginExecutionException(JsonConvert.SerializeObject(errResponse));
                }
            }
            return new ContractCreateResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess(),
                paymentStatus = new PaymentStatus
                {
                    isReservationPaid = true,
                    isDepositPaid = true
                }
            };
        }

        public Entity getIndividualCustomerInfo(Guid contactId)
        {
            IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(this.OrgService);
            string[] columns = new string[] { "firstname", "lastname", "mobilephone", "emailaddress1", "rnt_customerexternalid", "governmentid", "rnt_passportnumber" };
            return individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(contactId, columns);
        }

        public void updateContractTotalDuration(Guid contractId, DateTime pickupDateTime, DateTime dropoffDateTime)
        {
            var contractHelper = new RntCar.BusinessLibrary.Helpers.ContractHelper(this.OrgService);
            var totalDuration = contractHelper.calculateTotalDuration_ContractByPriceHourEffect_Decimal(pickupDateTime, dropoffDateTime);

            Entity e = new Entity("rnt_contract");
            e.Id = contractId;
            e["rnt_totalduration"] = Convert.ToDecimal(totalDuration);
            e["rnt_contractduration"] = Convert.ToDecimal(totalDuration);

            this.OrgService.Update(e);
        }

        public decimal? getContractDiffAmount(Guid reservationId)
        {
            ContractRepository contractRepository = new ContractRepository(this.OrgService);
            var contractEntity = contractRepository.getContractById(reservationId, new string[] { "rnt_totalamount", "rnt_netpayment" });

            var diff = contractEntity.GetAttributeValue<Money>("rnt_totalamount")?.Value - contractEntity.GetAttributeValue<Money>("rnt_netpayment")?.Value;

            return diff;
        }
        public decimal? getContractDepositAmount(Guid reservationId)
        {
            ContractRepository contractRepository = new ContractRepository(this.OrgService);
            var contractEntity = contractRepository.getContractById(reservationId, new string[] { "rnt_depositamount" });

            var depositAmount = contractEntity.GetAttributeValue<Money>("rnt_depositamount")?.Value;

            return depositAmount;
        }


        public bool checkActiveContractForCustomer(Guid contactId)
        {
            int[] statusCodeList = new int[] { (int)rnt_contractitem_StatusCode.New
                ,(int)rnt_contractitem_StatusCode.WaitingForDelivery
                ,(int)rnt_contractitem_StatusCode.Rental};
            QueryExpression getContractQuery = new QueryExpression("rnt_contract");
            getContractQuery.Criteria.AddCondition("rnt_customerid", ConditionOperator.Equal, contactId);
            getContractQuery.Criteria.AddCondition(new ConditionExpression("statuscode", ConditionOperator.In, statusCodeList));
            EntityCollection contractList = this.OrgService.RetrieveMultiple(getContractQuery);
            return contractList.Entities.Count == 0;
        }
    }
}
