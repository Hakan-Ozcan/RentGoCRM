using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.BusinessLibrary.Validations;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;
using System;
using System.Linq;

namespace RntCar.BusinessLibrary.Helpers
{
    public class ContractHelper : HelperHandler
    {
        public ContractHelper(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public ContractHelper(IOrganizationService organizationService) : base(organizationService)
        {
        }

        public ContractHelper(CrmServiceClient crmServiceClient, IOrganizationService organizationService) : base(crmServiceClient, organizationService)
        {
        }

        public ContractHelper(IOrganizationService organizationService, ITracingService tracingService) : base(organizationService, tracingService)
        {
        }

        public ContractHelper(CrmServiceClient crmServiceClient, IOrganizationService organizationService, ITracingService tracingService) : base(crmServiceClient, organizationService, tracingService)
        {
        }

        public void calculateDebitAmount(Guid contractId)
        {
            ExecuteWorkflowRequest executeWorkflowRequest = new ExecuteWorkflowRequest
            {
                WorkflowId = StaticHelper.CalculateDebitAmount_ContractWorkflowId,
                EntityId = contractId
            };
            this.IOrganizationService.Execute(executeWorkflowRequest);
        }
        public bool checkMakePayment_CorporateContracts(int reservationType, int paymentMethod)
        {
            if ((reservationType == (int)rnt_ReservationTypeCode.Kurumsal && paymentMethod == (int)rnt_PaymentMethodCode.Current) ||
                (reservationType == (int)rnt_ReservationTypeCode.Acente && paymentMethod == (int)rnt_PaymentMethodCode.FullCredit))
            {
                return true;
            }
            return false;
        }

        public bool checkCorporateInCancellation(int reservationType, int paymentMethod)
        {
            if (reservationType != (int)rnt_ReservationTypeCode.Bireysel)
            {
                return true;
            }
            return false;
        }
        public int calculateContractAdditionalProductKM(Guid contractId)
        {
            var kmLimit = 0;
            AdditionalProductKilometerLimitActionRepository additionalProductKilometerLimitActionRepository = new AdditionalProductKilometerLimitActionRepository(this.IOrganizationService);
            var kmLimits = additionalProductKilometerLimitActionRepository.getAdditionalProductKilometerLimitActions();

            foreach (var item in kmLimits)
            {
                ContractItemRepository contractItemRepository = new ContractItemRepository(this.IOrganizationService);
                var e = contractItemRepository.getContractItemByAdditionalProductIdandContractId(contractId,
                                                                                                 item.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id);

                if (e != null)
                {
                    kmLimit = item.GetAttributeValue<int>("rnt_kilometerlimiteffect");
                    break;
                }
            }
            return kmLimit;
        }
        public bool checkMakePayment_CorporateContracts(Guid contractId)
        {
            ContractRepository contractRepository = new ContractRepository(this.IOrganizationService);
            var contract = contractRepository.getContractById(contractId, new string[] { "rnt_contracttypecode", "rnt_paymentmethodcode" });

            if (contract.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value != (int)rnt_ReservationTypeCode.Bireysel)
            {
                return true;
            }
            return false;
        }
        public ContractDeposit calculateDepositAmount_Contract(Guid reservationId)
        {
            ReservationRepository reservationRepository = new ReservationRepository(this.IOrganizationService, this.CrmServiceClient);
            var reservation = reservationRepository.getReservationById(reservationId, new string[] { "rnt_reservationtype", "rnt_paymentmethodcode" });

            if (reservation.Contains("rnt_reservationtype") &&
               reservation.Contains("rnt_paymentmethodcode") &&
               reservation.GetAttributeValue<OptionSetValue>("rnt_reservationtype").Value == (int)rnt_ReservationTypeCode.Kurumsal &&
               reservation.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value == (int)rnt_PaymentMethodCode.Current)
            {
                this.Trace("corporate reservation for current , deposit amount is zero");
                return new ContractDeposit
                {
                    willChargeDeposit = false
                };
            }
            this.Trace("we will charge deposit");
            return new ContractDeposit
            {
                willChargeDeposit = true
            };
        }

        public ContractAmountDifference calculatePaymentAmount(Guid contractId, Guid? groupCodeInformationId, int? changeType, bool fromdelivery = false)
        {
            var entity = this.retrieveById("rnt_contract", contractId, "rnt_totalamount", "rnt_depositamount", "rnt_netpayment", "rnt_paymentmethodcode", "rnt_customerid");

            var customer = entity.GetAttributeValue<EntityReference>("rnt_customerid");
            var paymentCode = entity.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value;

            var depositAmountDifference = decimal.Zero;
            bool isCustomerPersonnal = true;

            if (customer != null && customer.LogicalName == "contact")
            {
                IndividualCustomerBL individualCustomerBL = new IndividualCustomerBL(this.IOrganizationService);
                isCustomerPersonnal = individualCustomerBL.checkUserISVIPorStaff(customer.Id);
            }

            if (groupCodeInformationId != null && changeType.HasValue &&
                (changeType == (int)ClassLibrary._Enums_1033.rnt_ChangeType.Upsell ||
                 changeType == (int)ClassLibrary._Enums_1033.rnt_ChangeType.Downsell) &&
                 (paymentCode != (int)rnt_PaymentMethodCode.Corporate || paymentCode != (int)rnt_PaymentMethodCode.FullCredit || paymentCode != (int)rnt_PaymentMethodCode.Current)
                 && !isCustomerPersonnal)
            {
                var groupCode = this.retrieveById("rnt_groupcodeinformations", groupCodeInformationId.Value, "rnt_deposit");
                depositAmountDifference = groupCode.GetAttributeValue<decimal>("rnt_deposit") - entity.GetAttributeValue<Money>("rnt_depositamount").Value;
            }
            this.Trace("contract total amount " + entity.GetAttributeValue<Money>("rnt_totalamount").Value);
            this.Trace("deposit total amount " + entity.GetAttributeValue<Money>("rnt_depositamount").Value);
            this.Trace("net payment " + entity.GetAttributeValue<Money>("rnt_netpayment").Value);
            var totalAmount = entity.GetAttributeValue<Money>("rnt_totalamount").Value +
                              entity.GetAttributeValue<Money>("rnt_depositamount").Value -
                              entity.GetAttributeValue<Money>("rnt_netpayment").Value;

            if (paymentCode == (int)rnt_PaymentMethodCode.PayBroker && fromdelivery)
            {
                ContractItemRepository contractItemRepository = new ContractItemRepository(this.IOrganizationService);
                var e = contractItemRepository.getPriceFactorDifference(contractId, new string[] { "rnt_totalamount" });
                if (e != null)
                {
                    this.Trace("price factor type is not null ");

                    totalAmount -= e.GetAttributeValue<Money>("rnt_totalamount").Value * -1;
                    this.Trace("totalAmount " + totalAmount);
                }
            }

            return new ContractAmountDifference
            {
                contractDepositAmountDifference = depositAmountDifference,
                contractAmountDifference = totalAmount,
                totalDepositAmount = entity.GetAttributeValue<Money>("rnt_depositamount").Value + depositAmountDifference
            };
        }

        public string checkDoubleCreditCard(decimal contractAmount,
                                            decimal depositAmount,
                                            CreditCardData paymentCard,
                                            CreditCardData depositCard,
                                            bool isDoubleCreditCard,
                                            Guid contactId,
                                            int langId,
                                            int? changeType)
        {
            this.Trace("contract helper change type : " + changeType);
            if (changeType.HasValue &&
               (changeType == (int)rnt_ChangeType.Upgrade ||
                changeType == (int)rnt_ChangeType.Downgrade))
            {
                return string.Empty;
            }

            CreditCardValidation creditCardValidation = new CreditCardValidation(this.IOrganizationService);
            if (contractAmount > decimal.Zero || (!isDoubleCreditCard && depositAmount > decimal.Zero))
            {
                var creditCardResponse = creditCardValidation.checkCreditCard(paymentCard, contactId, langId);
                if (!creditCardResponse.ResponseResult.Result)
                {
                    return creditCardResponse.ResponseResult.ExceptionDetail;
                }
            }
            if (depositAmount > decimal.Zero && isDoubleCreditCard)
            {
                var creditCardResponse = creditCardValidation.checkCreditCard(depositCard, contactId, langId);
                if (!creditCardResponse.ResponseResult.Result)
                {
                    return creditCardResponse.ResponseResult.ExceptionDetail;
                }
            }
            return string.Empty;
        }

        public string checkPaymentCardandDepositCardIsSame(bool isDoubleCreditCard,
                                                           Guid reservationId,
                                                           CreditCardData paymentCard,
                                                           CreditCardData depositCard,
                                                           int langId)
        {
            if (!isDoubleCreditCard)
            {
                return string.Empty;
            }
            //ödenecek tutar yoktur o yüzden payment card null gelme senaryosu --> teminat kartını yeni girmiş olabilir
            if (paymentCard == null && string.IsNullOrEmpty(depositCard?.cardUserKey) && string.IsNullOrEmpty(depositCard?.cardToken))

            {
                this.Trace("paymentCard == null && string.IsNullOrEmpty(depositCard.cardUserKey) && string.IsNullOrEmpty(depositCard.cardToken)");
                PaymentRepository paymentRepository = new PaymentRepository(this.IOrganizationService);
                var payment = paymentRepository.getLastPayment_Reservation(reservationId);

                CreditCardRepository creditCardRepository = new CreditCardRepository(this.IOrganizationService);
                var paymentCardInfo = creditCardRepository.getCreditCardById(payment.GetAttributeValue<EntityReference>("rnt_customercreditcardid").Id);
                if (paymentCardInfo.GetAttributeValue<string>("rnt_creditcardnumber") == CommonHelper.formatCreditCardNumber(depositCard.creditCardNumber))
                {
                    return "Seçilen araç grubu çift kredi kartı istemektedir.Ödeme kartı ile teminat kartı aynı olamaz.";
                }

            }
            //ödenecek tutar yoktur o yüzden payment card null gelme senaryosu --> teminat kartını  listeden seçmiş olabilr
            else if (paymentCard == null && !string.IsNullOrEmpty(depositCard?.cardUserKey) && !string.IsNullOrEmpty(depositCard?.cardToken))
            {
                this.Trace("paymentCard == null && !string.IsNullOrEmpty(depositCard.cardUserKey) && !string.IsNullOrEmpty(depositCard.cardToken)");
                PaymentRepository paymentRepository = new PaymentRepository(this.IOrganizationService);
                var payment = paymentRepository.getLastPayment_Reservation(reservationId);

                CreditCardRepository creditCardRepository = new CreditCardRepository(this.IOrganizationService);
                if (payment != null)
                {
                    var paymentCardInfo = creditCardRepository.getCreditCardById(payment.GetAttributeValue<EntityReference>("rnt_customercreditcardid").Id);

                    var depositCardInfo = creditCardRepository.getCreditCardByUserKeyandCreditCardTokenByGivenColumns(depositCard.cardUserKey, depositCard.cardToken, new string[] { "rnt_creditcardnumber" });
                    if (paymentCardInfo.GetAttributeValue<string>("rnt_creditcardnumber") == depositCardInfo.GetAttributeValue<string>("rnt_creditcardnumber"))
                    {
                        return "Seçilen araç grubu çift kredi kartı istemektedir.Ödeme kartı ile teminat kartı aynı olamaz.";
                    }
                }

            }
            //iki kartıda yeni girmiş olabilir 
            else if (string.IsNullOrEmpty(paymentCard?.cardUserKey) && string.IsNullOrEmpty(paymentCard?.cardToken) &&
                     string.IsNullOrEmpty(depositCard?.cardUserKey) && string.IsNullOrEmpty(depositCard?.cardToken))
            {
                this.Trace("string.IsNullOrEmpty(paymentCard.cardUserKey) && string.IsNullOrEmpty(paymentCard.cardToken) && string.IsNullOrEmpty(depositCard.cardUserKey) && string.IsNullOrEmpty(depositCard.cardToken)");

                if (paymentCard.creditCardNumber.removeEmptyCharacters() == depositCard.creditCardNumber.removeEmptyCharacters())
                {
                    //todo replace by xml
                    //XrmHelper xrmHelper = new XrmHelper(this.IOrganizationService);
                    //return xrmHelper.GetXmlTagContentByGivenLangId("MissingCreditCardInfo", langId, this.reservationXmlPath);
                    return "Seçilen araç grubu çift kredi kartı istemektedir.Ödeme kartı ile teminat kartı aynı olamaz.";
                }
            }
            //ödeme kartını yeni girip , teminat kartını listeden seçmiş olabilir
            else if (string.IsNullOrEmpty(paymentCard?.cardUserKey) && string.IsNullOrEmpty(paymentCard?.cardToken) &&
                     !string.IsNullOrEmpty(depositCard?.cardUserKey) && !string.IsNullOrEmpty(depositCard?.cardToken))
            {
                this.Trace("!string.IsNullOrEmpty(paymentCard.cardUserKey) && !string.IsNullOrEmpty(paymentCard.cardToken) && string.IsNullOrEmpty(depositCard.cardUserKey) && string.IsNullOrEmpty(depositCard.cardToken)");

                CreditCardRepository creditCardRepository = new CreditCardRepository(this.IOrganizationService);
                var depositCardInfo = creditCardRepository.getCreditCardByUserKeyandCreditCardTokenByGivenColumns(depositCard.cardUserKey, depositCard.cardToken, new string[] { "rnt_creditcardnumber" });

                if (depositCardInfo?.GetAttributeValue<string>("rnt_creditcardnumber") == CommonHelper.formatCreditCardNumber(paymentCard.creditCardNumber))
                {
                    return "Seçilen araç grubu çift kredi kartı istemektedir.Ödeme kartı ile teminat kartı aynı olamaz.";
                }
            }
            //ödeme kartını listeden seçip , teminat kartını yeni girmiş olabilir
            else if (!string.IsNullOrEmpty(paymentCard?.cardUserKey) && !string.IsNullOrEmpty(paymentCard?.cardToken) &&
                     string.IsNullOrEmpty(depositCard?.cardUserKey) && string.IsNullOrEmpty(depositCard?.cardToken))
            {
                this.Trace("!string.IsNullOrEmpty(paymentCard.cardUserKey) && !string.IsNullOrEmpty(paymentCard.cardToken) && string.IsNullOrEmpty(depositCard.cardUserKey) && string.IsNullOrEmpty(depositCard.cardToken)");

                CreditCardRepository creditCardRepository = new CreditCardRepository(this.IOrganizationService);
                var paymentCardInfo = creditCardRepository.getCreditCardByUserKeyandCreditCardTokenByGivenColumns(paymentCard.cardUserKey, paymentCard.cardToken, new string[] { "rnt_creditcardnumber" });

                if (paymentCardInfo.GetAttributeValue<string>("rnt_creditcardnumber") == CommonHelper.formatCreditCardNumber(depositCard.creditCardNumber))
                {
                    return "Seçilen araç grubu çift kredi kartı istemektedir.Ödeme kartı ile teminat kartı aynı olamaz.";
                }
            }
            //iki  kartıda listeden seçmiştir
            else if (!string.IsNullOrEmpty(paymentCard?.cardUserKey) && !string.IsNullOrEmpty(paymentCard?.cardToken) &&
                     !string.IsNullOrEmpty(depositCard?.cardUserKey) && !string.IsNullOrEmpty(depositCard?.cardToken))
            {
                this.Trace("!string.IsNullOrEmpty(paymentCard.cardUserKey) && !string.IsNullOrEmpty(paymentCard.cardToken) && string.IsNullOrEmpty(depositCard.cardUserKey) && string.IsNullOrEmpty(depositCard.cardToken)");

                CreditCardRepository creditCardRepository = new CreditCardRepository(this.IOrganizationService);
                var paymentCardInfo = creditCardRepository.getCreditCardByUserKeyandCreditCardTokenByGivenColumns(paymentCard.cardUserKey, paymentCard.cardToken, new string[] { "rnt_creditcardnumber" });


                var depositCardInfo = creditCardRepository.getCreditCardByUserKeyandCreditCardTokenByGivenColumns(depositCard.cardUserKey, depositCard.cardToken, new string[] { "rnt_creditcardnumber" });

                if (paymentCardInfo?.GetAttributeValue<string>("rnt_creditcardnumber") == depositCardInfo?.GetAttributeValue<string>("rnt_creditcardnumber"))
                {
                    return "Seçilen araç grubu çift kredi kartı istemektedir.Ödeme kartı ile teminat kartı aynı olamaz.";
                }
            }
            return string.Empty;
        }

        public MongoDBResponse createContractItemInMongoDB(ContractItemResponse equipmentItem)
        {
            ContractItemRepository contractItemRepository = new ContractItemRepository(this.IOrganizationService);
            var newContractItem = contractItemRepository.retrieveById("rnt_contractitem", equipmentItem.contractItemId, true);

            ContractItemBL contractItemBL = new ContractItemBL(this.IOrganizationService);
            var response = contractItemBL.createContractItemInMongoDB(newContractItem);
            if (response.Result)
                contractItemBL.updateMongoDBCreateRelatedFields(newContractItem, response.Id);

            return response;
        }
        public MongoDBResponse updateContractItemInMongoDB(ContractItemResponse equipmentItem)
        {
            ContractItemRepository contractItemRepository = new ContractItemRepository(this.IOrganizationService);
            var newContractItem = contractItemRepository.retrieveById("rnt_contractitem", equipmentItem.contractItemId, true);

            ContractItemBL contractItemBL = new ContractItemBL(this.IOrganizationService);
            var response = contractItemBL.updateContractItemInMongoDB(newContractItem);
            if (response.Result)
                contractItemBL.UpdateMongoDBUpdateRelatedFields(newContractItem);

            return response;
        }
        public VirtualPosResponse getVPosIdforGivenCardNumber(CreditCardData creditCardData, int provider)
        {
            string cardBin = string.Empty;
            if (creditCardData.creditCardId.HasValue && creditCardData.creditCardId.Value != Guid.Empty)
            {
                CreditCardRepository creditCardRepository = new CreditCardRepository(this.IOrganizationService);
                var card = creditCardRepository.getCreditCardById(creditCardData.creditCardId.Value);
                cardBin = card.GetAttributeValue<string>("rnt_binnumber");
            }
            else
            {
                cardBin = creditCardData.creditCardNumber.removeEmptyCharacters().Substring(0, 6);
            }
            CreditCardBL creditCardBL = new CreditCardBL(this.IOrganizationService);
            return creditCardBL.retrieveVirtualPosIdforGivenCard(new RetrieveVirtualPosIdParameters
            {
                cardBin = cardBin,
                provider = provider
            });

        }

        public Guid createContractItemSafelyWithoutMongoDbIntegrationTrigger(Entity contractItem)
        {
            contractItem.Attributes.Remove("rnt_mongodbintegrationtrigger");
            return this.IOrganizationService.Create(contractItem);
        }
        public void updateContractItemSafelyWithoutMongoDbIntegrationTrigger(Entity contractItem)
        {
            contractItem.Attributes.Remove("rnt_mongodbintegrationtrigger");
            this.IOrganizationService.Update(contractItem);
        }

        public int calculateTotalDuration_ContractByPriceHourEffect(DateTime pickupDateTime, DateTime dropoffDateTime)
        {
            var totalDuration = CommonHelper.calculateTotalDurationInMinutes(pickupDateTime, dropoffDateTime);
            if (totalDuration < 1440)
            {
                return CommonHelper.calculateTotalDurationInDaysCheckIfzero(pickupDateTime, dropoffDateTime);
            }

            var totalDays = CommonHelper.calculateTotalDurationInDays(pickupDateTime, dropoffDateTime);
            var precisionPart = totalDays - Math.Truncate(totalDays);

            PriceHourEffectRepository priceHourEffectBL = new PriceHourEffectRepository(this.IOrganizationService, this.CrmServiceClient);
            var priceHourEffect = priceHourEffectBL.getZeroPriceEffect();
            var maximumRate = priceHourEffect.GetAttributeValue<int>("rnt_maximumminute");
            //if precision part is bigger than zero rate , than we need to round up the day
            var maximumRateInDays = maximumRate / StaticHelper.dayDurationInMinutes;

            if (precisionPart > Convert.ToDouble(maximumRateInDays))
            {
                return (int)Math.Ceiling(totalDays);
            }

            return (int)Math.Floor(totalDays);
        }

        public decimal calculateTotalDuration_ContractByPriceHourEffect_Decimal(DateTime pickupDateTime, DateTime dropoffDateTime)
        {
            var totalDuration = CommonHelper.calculateTotalDurationInMinutes(pickupDateTime, dropoffDateTime);
            if (totalDuration < 1440)
            {
                return CommonHelper.calculateTotalDurationInDaysCheckIfzero(pickupDateTime, dropoffDateTime);
            }

            var totalMinutes = CommonHelper.calculateTotalDurationInMinutes(pickupDateTime, dropoffDateTime);

            var quotient = Convert.ToInt32(Math.Floor(totalMinutes / 1440));
            var remainder = totalMinutes % 1440;

            PriceHourEffectRepository priceHourEffectRepository = new PriceHourEffectRepository(this.IOrganizationService, this.CrmServiceClient);
            var priceHourEffect = priceHourEffectRepository.getPriceHourEffectByDuration(Convert.ToInt32(remainder));
            var decimalRemainder = decimal.Zero;

            if (priceHourEffect != null)
            {
                decimalRemainder = priceHourEffect.GetAttributeValue<int>("rnt_effectrate") / 100M;
            }
            return quotient + decimalRemainder;
        }

        public void updateItemsforRental(Guid contractId, Guid latestEquipmentId, bool isEquipmentChanged)
        {
            if (!isEquipmentChanged)
            {
                ContractItemRepository contractItemRepository = new ContractItemRepository(this.IOrganizationService);
                XrmHelper xrmHelper = new XrmHelper(this.IOrganizationService);

                var entity = this.retrieveById("rnt_contract", contractId, "rnt_paymentmethodcode", "rnt_dropoffdatetime");
                var contractdroppoff = entity.GetAttributeValue<DateTime>("rnt_dropoffdatetime");
                this.Trace("contractdroppoff : " + contractdroppoff);

                if (entity.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value == (int)rnt_PaymentMethodCode.PayBroker)
                {
                    var priceItem = contractItemRepository.getPriceFactorDifference(contractId, new string[] { "rnt_name", "rnt_pickupdatetime", "rnt_dropoffdatetime", "rnt_totalamount" });
                    this.Trace("priceItem pickup : " + priceItem.GetAttributeValue<DateTime>("rnt_pickupdatetime"));
                    this.Trace("priceItem dropoff : " + priceItem.GetAttributeValue<DateTime>("rnt_dropoffdatetime"));

                    //eğer aracı broker tarihleri arasında getiriyorsa
                    //araç kalemi önceki metodlarda sıfırlanmıştı o yüzden corporate kalemi -1 çarpıp negatiflikten kurtarıyoruz
                    var item = this.retrieveById("rnt_contractitem", latestEquipmentId, "rnt_totalamount", "rnt_equipment", "rnt_name");
                    this.Trace("item totalamount : " + item.GetAttributeValue<Money>("rnt_totalamount").Value);
                    this.Trace("priceItem totalamount : " + priceItem.GetAttributeValue<Money>("rnt_totalamount").Value);

                    PriceHourEffectRepository priceHourEffectRepository = new PriceHourEffectRepository(this.IOrganizationService);
                    var priceHourEffect = priceHourEffectRepository.getZeroPriceEffect();
                    var maximumRate = priceHourEffect.GetAttributeValue<int>("rnt_maximumminute");

                    //check upsell applied
                    //brokerdan 10-15 arasında aldı. 1 gün bireyselden uzattı.
                    //15inde geri getirdi. sadece amounttan kontrol upsell gibi davrandıyor sistemi o yüzden bu kontrol eklendi.

                    var upselledItem = contractItemRepository.getUpselledItem(contractId, new string[] { });
                    var hasUpsell = upselledItem == null ? false : true;
                    var isBetween = contractdroppoff.isBetween(priceItem.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                                                               priceItem.GetAttributeValue<DateTime>("rnt_dropoffdatetime"));

                    this.Trace("hasUpsell : " + hasUpsell);
                    this.Trace("isBetween : " + isBetween);
                    //tarihler arasında ve upsell yoksa tutar kontrol etmeye gerek yok.
                    //Tarihler arasında upsell varsa tutarı sıfırlamamak için else'e gönder.
                    if (isBetween &&
                       (!hasUpsell || (hasUpsell && !(item.GetAttributeValue<Money>("rnt_totalamount").Value > priceItem.GetAttributeValue<Money>("rnt_totalamount").Value * -1))))
                    {
                        this.Trace("dropoff is between");
                        Entity e = new Entity("rnt_contractitem");
                        e.Id = priceItem.Id;
                        e["rnt_dropoffdatetime"] = contractdroppoff;
                        e["rnt_equipment"] = new EntityReference("rnt_equipment", item.GetAttributeValue<EntityReference>("rnt_equipment").Id);
                        //erken getirme canlıya alınınca acılacak
                        e["rnt_totalamount"] = new Money(item.GetAttributeValue<Money>("rnt_totalamount").Value);
                        //e["rnt_totalamount"] = new Money(priceItem.GetAttributeValue<Money>("rnt_totalamount").Value * -1);
                        e["rnt_name"] = priceItem["rnt_name"] + " - " + item.GetAttributeValue<string>("rnt_name");
                        this.IOrganizationService.Update(e);

                        Entity rentalItem = new Entity("rnt_contractitem");
                        rentalItem.Id = item.Id;
                        rentalItem["rnt_totalamount"] = new Money(decimal.Zero);
                        this.IOrganizationService.Update(rentalItem);
                        //eğer rental equipment kalemin plakası ile broker kaleminin plakası aynı değilse farklı statüye al.
                        xrmHelper.setState("rnt_contractitem", item.Id, (int)GlobalEnums.StateCode.Passive, (int)rnt_contractitem_StatusCode.Inactive);

                        //araç değişikliği varsa diğer kalemleride iptal et

                        var items = contractItemRepository.getCompletedContractItemsByContractId(contractId);
                        items = items.Where(p => p.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)rnt_contractitem_rnt_itemtypecode.Equipment).ToList();

                        foreach (var x in items)
                        {
                            Entity updateItem = new Entity("rnt_contractitem");
                            updateItem.Id = x.Id;
                            updateItem["rnt_totalamount"] = new Money(decimal.Zero);
                            this.IOrganizationService.Update(updateItem);
                            xrmHelper.setState("rnt_contractitem", x.Id, (int)GlobalEnums.StateCode.Passive, 100000010);
                        }

                    }
                    else
                    {
                        var items = contractItemRepository.getCompletedContractItemsByContractId(contractId);
                        items = items.Where(p => p.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)rnt_contractitem_rnt_itemtypecode.Equipment).ToList();

                        this.Trace("items count " + items.Count);
                        this.Trace("dropoff is not between");
                        this.Trace("price item total amount" + priceItem.GetAttributeValue<Money>("rnt_totalamount").Value);
                        Entity e = new Entity("rnt_contractitem");
                        e.Id = priceItem.Id;
                        e["rnt_equipment"] = new EntityReference("rnt_equipment", item.GetAttributeValue<EntityReference>("rnt_equipment").Id);
                        e["rnt_totalamount"] = new Money(priceItem.GetAttributeValue<Money>("rnt_totalamount").Value * -1);
                        //e["rnt_dropoffdatetime"] = contractdroppoff;
                        this.IOrganizationService.Update(e);
                        //remove rental item from list
                        items = items.Where(p => p.Id != item.Id).ToList();
                        foreach (var i in items)
                        {
                            this.Trace("itemID " + i.Id);
                            this.Trace("item amount " + i.GetAttributeValue<Money>("rnt_totalamount").Value);
                        }
                        var sum = items.Sum(p => p.GetAttributeValue<Money>("rnt_totalamount").Value);
                        this.Trace("sum " + sum);
                        this.Trace("item total amount " + item.GetAttributeValue<Money>("rnt_totalamount").Value);
                        this.Trace("priceItem total amount " + priceItem.GetAttributeValue<Money>("rnt_totalamount").Value);
                        var newTotalAmount = item.GetAttributeValue<Money>("rnt_totalamount").Value + sum - (priceItem.GetAttributeValue<Money>("rnt_totalamount").Value * -1);
                        this.Trace("total amount will be updated : " + Convert.ToString(newTotalAmount));
                        Entity e1 = new Entity("rnt_contractitem");
                        e1.Id = item.Id;
                        e1["rnt_totalamount"] = new Money(newTotalAmount);
                        this.IOrganizationService.Update(e1);
                        this.Trace("update ended");

                        this.Trace("items count " + items.Count);
                        foreach (var x in items)
                        {
                            //Entity updateItem = new Entity("rnt_contractitem");
                            //updateItem.Id = x.Id;
                            //updateItem["rnt_baseprice"] = x["rnt_totalamount"];
                            //updateItem["rnt_totalamount"] = new Money(decimal.Zero);
                            //this.IOrganizationService.Update(updateItem);
                            //inactive noshow
                            xrmHelper.setState("rnt_contractitem", x.Id, (int)GlobalEnums.StateCode.Passive, 100000010);
                        }

                    }

                    //tarihi güncellemek için. upsell senaryosunda
                    if (contractdroppoff.isBetween(priceItem.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                                                    priceItem.GetAttributeValue<DateTime>("rnt_dropoffdatetime")))
                    {
                        Entity e = new Entity("rnt_contractitem");
                        e.Id = priceItem.Id;
                        e["rnt_dropoffdatetime"] = contractdroppoff;

                        this.IOrganizationService.Update(e);
                    }
                }
                else if (entity.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value == (int)rnt_PaymentMethodCode.Individual)
                {
                    this.Trace("rnt_PaymentMethodCode individual");
                    this.Trace("contractId: " + contractId);
                    var manualDiscount = contractItemRepository.getDiscountContractItem(contractId, new string[] { "rnt_totalamount" });
                    this.Trace("manualDiscount: " + JsonConvert.SerializeObject(manualDiscount));

                    if (manualDiscount != null)
                    {
                        Entity manualDiscountItem = new Entity("rnt_contractitem");
                        manualDiscountItem.Id = manualDiscount.Id;
                        manualDiscountItem["rnt_totalamount"] = new Money(decimal.Zero);
                        this.IOrganizationService.Update(manualDiscountItem);

                        xrmHelper.setState("rnt_contractitem", manualDiscount.Id, (int)GlobalEnums.StateCode.Passive, (int)rnt_contractitem_StatusCode.Inactive);
                    }
                }
            }
        }

        public NewOneWayData calculateNewOneWayFeeAmount(Guid contractId, Guid dropOffBranchId)
        {
            var amount = decimal.Zero;
            ConfigurationBL configurationBL = new ConfigurationBL(this.IOrganizationService);
            var oneWay = configurationBL.GetConfigurationByName("OneWayFeeCode");

            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.IOrganizationService);
            var product = additionalProductRepository.getAdditionalProductByProductCode(oneWay);

            ContractItemRepository contractItemRepository = new ContractItemRepository(this.IOrganizationService);
            var contractItem = contractItemRepository.getContractItemByAdditionalProductIdandContractId(contractId, product.Id);

            if (contractItem != null &&
               contractItem.GetAttributeValue<EntityReference>("rnt_dropoffbranch").Id != dropOffBranchId)
            {
                OneWayFeeRepository oneWayFeeRepository = new OneWayFeeRepository(this.IOrganizationService);
                var entity = oneWayFeeRepository.getOneWayFeeByPickupandDropoffBranch(contractItem.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id, dropOffBranchId);
                amount = entity.GetAttributeValue<Money>("rnt_price").Value;
            }
            return new NewOneWayData
            {
                additionalProductId = product.Id,
                contractItemId = contractItem?.Id,
                amount = amount
            };
        }

    }
}
