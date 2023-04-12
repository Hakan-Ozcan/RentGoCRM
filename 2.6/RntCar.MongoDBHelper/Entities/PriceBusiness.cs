using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary.MongoDB;
using RntCar.ClassLibrary.PaymentPlan;
using RntCar.MongoDBHelper.Helper;
using RntCar.MongoDBHelper.Model;
using RntCar.MongoDBHelper.Price;
using RntCar.MongoDBHelper.Repository;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using static RntCar.ClassLibrary.GlobalEnums;

namespace RntCar.MongoDBHelper.Entities
{

    public class PriceBusiness
    {
        internal class internalCustomer
        {
            public int customerType { get; set; }
            public int cType { get; set; }
        }
        internal internalCustomer calculateCustomerTypes(int customerType, DocumentData selectedReservation, int? corporateType, bool processIndividualPrices)
        {
            //crm'deki enumla , availabilitydeki enum karışılıklığından!
            if (customerType == (int)GlobalEnums.CustomerType.Broker)
            {
                customerType = 4;
            }
            else if (customerType == (int)GlobalEnums.CustomerType.Agency)
            {
                customerType = 3;
            }
            var cType = (int)GlobalEnums.CustomerType.Individual;
            #region Get Price MetaData and Validations
            //existing document
            //varolan döküman için brokerda burası her zaman uzama senayosudur
            if (selectedReservation?.documentType == (int)rnt_ReservationTypeCode.Broker ||
                selectedReservation?.documentType == (int)rnt_ReservationTypeCode.Acente)
            {
                cType = (int)GlobalEnums.CustomerType.Individual;
                customerType = (int)GlobalEnums.CustomerType.Individual;
            }
            else if (corporateType.HasValue && (corporateType.Value == (int)rnt_AccountTypeCode.Broker || corporateType.Value == (int)rnt_AccountTypeCode.Agency))
            {
                if (processIndividualPrices)
                {
                    cType = (int)GlobalEnums.CustomerType.Individual;
                    customerType = (int)GlobalEnums.CustomerType.Individual;
                }
                else
                {
                    cType = (int)GlobalEnums.CustomerType.Corporate;
                }
            }
            else if (customerType == (int)GlobalEnums.CustomerType.Corporate)
            {
                cType = (int)GlobalEnums.CustomerType.Corporate;
            }
            return new internalCustomer
            {
                cType = cType,
                customerType = customerType
            };
        }

        public DateTime queryPickupDateTime { get; set; }
        public DateTime queryDropoffDateTime { get; set; }
        public int totalDuration { get; set; }
        public string reservationId { get; set; }

        public PriceBusiness()
        {

        }
        public PriceBusiness(DateTime _queryPickupDateTime, DateTime _queryDropoffDateTime)
        {
            queryPickupDateTime = _queryPickupDateTime;
            queryDropoffDateTime = _queryDropoffDateTime;

            DurationHelper durationHelper = new DurationHelper(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                               StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            var totalDays = durationHelper.calculateDocumentDurationByPriceHourEffect_Decimal(_queryPickupDateTime, _queryDropoffDateTime);
            totalDuration = Convert.ToInt32(totalDays * 1440);
        }
        public PriceResponse calculatePriceByGivenDay(Guid groupCodeInformationId,
                                                      int equipmentCountByGroupCode,
                                                      DateTime relatedDay,
                                                      int reservationsCountOnSelectedDay,
                                                      int channelCode,
                                                      int? segment,
                                                      DocumentData selectedReservation,
                                                      string pickupBranchId,
                                                      int customerType,
                                                      string priceCodeId,
                                                      int? corporateType,
                                                      bool processIndividualPrices,
                                                      decimal exchangeRate,
                                                      string accountGroup,
                                                      int operationType = 0)
        {

            var types = this.calculateCustomerTypes(customerType, selectedReservation, corporateType, processIndividualPrices);
            //1 means individual
            PriceListDataMongoDB priceListData = null;
            if (types.cType == (int)GlobalEnums.CustomerType.Individual)
            {
                priceListData = this.getRelatedPriceList(types.cType, relatedDay);

            }
            else if (types.cType == (int)GlobalEnums.CustomerType.Corporate)
            {
                priceListData = this.getRelatedPriceListByPriceCode(types.cType, relatedDay, priceCodeId);
            }

            if (priceListData == null)
            {
                return PriceResponse.notCalculated();
            }
            var groupCodePriceLists = new GroupCodeListPriceDataMongoDB();

            groupCodePriceLists = this.getRelatedGroupCodeList(priceListData.PriceListId, groupCodeInformationId);

            if (groupCodePriceLists == null)
            {
                return PriceResponse.notCalculated();
            }
            var availabilityPrices = new AvailabilityPriceListDataMongoDB();

            #endregion

            var basePrice = groupCodePriceLists.ListPrice;

            #region Apply price factors except pay factor 

            var availabilityByDay = this.calculateAvailabilityRatio(equipmentCountByGroupCode, reservationsCountOnSelectedDay);

            PriceCalculator priceCalculator = new PriceCalculator(new Guid(priceListData.PriceListId), priceListData.PriceListName);
            if (types.customerType == (int)GlobalEnums.CustomerType.Individual)
            {
                availabilityPrices = this.getAvailabilityPricesByDurationByGroupCode(priceListData.PriceListId, Convert.ToInt32(availabilityByDay), groupCodeInformationId);
                if (availabilityPrices == null)
                {
                    //eğer grup code bulamazsa grup olmayanda al
                    availabilityPrices = this.getAvailabilityPricesByDuration(priceListData.PriceListId, Convert.ToInt32(availabilityByDay));
                    if (availabilityPrices == null)
                    {
                        //means contract update
                        //sözleşme güncellemede doluluk bulamazsa en yakın doluluk oranını al
                        //operationtype 50 calculatecontractremaining amount
                        //operation type 30 calculate avalability --> from tablet
                        if (operationType == 100 || operationType == 50 || operationType == 30)
                        {
                            var list = this.getAvailabilityPricesByDurationByGroupCode(priceListData.PriceListId, groupCodeInformationId);
                            if (list.Count == 0)
                            {
                                return PriceResponse.notCalculated();
                            }
                            availabilityPrices = list.OrderBy(x => Math.Abs((long)x.MaximumAvailability - availabilityByDay)).First();

                            if (availabilityPrices == null)
                            {
                                return PriceResponse.notCalculated();
                            }
                        }
                        else
                        {
                            return PriceResponse.notCalculated();
                        }
                    }
                }
                priceCalculator.applyAvailabilityFactor(relatedDay.converttoTimeStamp(), basePrice, availabilityPrices.PriceChangeRate);
            }
            else
            {
                if (availabilityByDay < 100)
                {
                    priceCalculator.Prices.priceAfterAvailabilityFactor = basePrice;
                    priceCalculator.Prices.totalPrice = basePrice;
                }
                else
                {
                    return PriceResponse.notCalculated();
                }
            }
            priceCalculator.applyChannel(relatedDay.converttoTimeStamp(), priceCalculator.Prices.totalPrice, priceCalculator.Prices.priceAfterAvailabilityFactor, channelCode, pickupBranchId, Convert.ToString(groupCodeInformationId), types.customerType, accountGroup);
            priceCalculator.applyWeekdays(relatedDay.converttoTimeStamp(), priceCalculator.Prices.totalPrice, priceCalculator.Prices.priceAfterChannelFactor, pickupBranchId, Convert.ToString(groupCodeInformationId), types.customerType, accountGroup);
            priceCalculator.applySpecialDays(relatedDay.converttoTimeStamp(), priceCalculator.Prices.totalPrice, priceCalculator.Prices.priceAfterWeekDaysFactor, pickupBranchId, Convert.ToString(groupCodeInformationId), types.customerType, accountGroup);
            priceCalculator.applyCustomer(relatedDay.converttoTimeStamp(), priceCalculator.Prices.totalPrice, priceCalculator.Prices.priceAfterSpecialDaysFactor, segment, pickupBranchId, Convert.ToString(groupCodeInformationId), types.customerType, accountGroup);
            priceCalculator.applyBranch(relatedDay.converttoTimeStamp(), priceCalculator.Prices.totalPrice, priceCalculator.Prices.priceAfterCustomerFactor, pickupBranchId, Convert.ToString(groupCodeInformationId), types.customerType, accountGroup);
            priceCalculator.applyBranch2(relatedDay.converttoTimeStamp(), priceCalculator.Prices.totalPrice, priceCalculator.Prices.priceAfterBranchFactor, pickupBranchId, Convert.ToString(groupCodeInformationId), types.customerType, accountGroup);
            priceCalculator.applyEquality(relatedDay.converttoTimeStamp(), priceCalculator.Prices.totalPrice, priceCalculator.Prices.priceAfterBranch2Factor, pickupBranchId, Convert.ToString(groupCodeInformationId), types.customerType, accountGroup);

            #endregion
            //set without tick price after factors
            var withoutTickDayPrice = priceCalculator.Prices.totalPrice;
            priceCalculator.Prices.payLaterPrice = priceCalculator.Prices.totalPrice;

            #region TickDay Price Calculation
            var hasTickTime = false;
            var tickTimeRate = decimal.Zero;
            //this parameter for total amount
            var amount = priceCalculator.Prices.totalPrice;

            //this parameter for before applying tick rules
            priceCalculator.applyPayMethod(priceCalculator.Prices.totalPrice, pickupBranchId, Convert.ToString(groupCodeInformationId), types.customerType, accountGroup);
            var payNowWithoutTickPrice = priceCalculator.Prices.payNowPrice;
            var payLaterWithoutTickPrice = priceCalculator.Prices.payLaterPrice;
            //eğer sözleşme update/rez update'den geliyorsa 
            //querydropoff ile dökümanın dropoff'u arasında 1 gün veya fazla varsa tick değildir!
            var bypassTick = false;
            if (selectedReservation != null && selectedReservation.type == DocumentType.contract)
            {
                ContractItemRepository contractItemRepository = new ContractItemRepository(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var contractItems = contractItemRepository.getContractItemsEquipment(selectedReservation.documentId);

                var deliveredItems = contractItems.Where(p => p.statuscode == (int)RntCar.ClassLibrary._Enums_1033.rnt_contractitem_StatusCode.WaitingForDelivery).ToList();
                var rentalItems = contractItems.Where(p => p.statuscode == (int)RntCar.ClassLibrary._Enums_1033.rnt_contractitem_StatusCode.Rental).ToList();
                var completedItems = contractItems.Where(p => p.statuscode == (int)RntCar.ClassLibrary._Enums_1033.rnt_contractitem_StatusCode.Completed).ToList();

                if (completedItems.Count > 0 && (deliveredItems.Count > 0 || rentalItems.Count > 0) && operationType != 50)
                {
                    DurationHelper durationHelper = new DurationHelper(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                    bypassTick = durationHelper.checkPriceHourEffectIsBiggerEnoughHundredPercent(selectedReservation.dropoffTimeStamp.Value.converttoDateTime(), this.queryDropoffDateTime);
                }

            }

            //CommonHelper.checkBiggerThanDay(this.queryPickupDateTime, this.queryDropoffDateTime)  --> purporse of this validation for queries for less than one day
            if (CommonHelper.checkBiggerThanDay(this.queryPickupDateTime, this.queryDropoffDateTime) &&
               !CommonHelper.checkBiggerThanDay(relatedDay, this.queryDropoffDateTime) &&
               !bypassTick)
            {
                var minutes = CommonHelper.calculateTotalDurationInMinutes(relatedDay.AddMinutes(StaticHelper.offset).converttoTimeStamp(), this.queryDropoffDateTime.AddMinutes(StaticHelper.offset).converttoTimeStamp());
                if (minutes < 0)
                {
                    minutes = minutes * -1;
                }

                PriceHourEffectRepository priceHourEffectRepository = new PriceHourEffectRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                    StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var priceHourEffect = priceHourEffectRepository.getPriceHourEffectByDuration(minutes);
                if (priceHourEffect == null)
                {
                    return PriceResponse.notCalculated();
                }
                if (selectedReservation?.paymentChoice == (int)PaymentEnums.PaymentType.PayNow)
                {
                    priceCalculator.Prices.payNowPrice = (priceHourEffect.effectRate * priceCalculator.Prices.payNowPrice) / 100;
                    amount = priceCalculator.Prices.payNowPrice;
                }
                else if (selectedReservation?.paymentChoice == (int)PaymentEnums.PaymentType.PayLater)
                {
                    priceCalculator.Prices.payLaterPrice = (priceHourEffect.effectRate * priceCalculator.Prices.payLaterPrice) / 100;
                    amount = priceCalculator.Prices.payLaterPrice;

                    if (selectedReservation != null &&
                        (selectedReservation.documentType == (int)rnt_ReservationTypeCode.Broker ||
                         selectedReservation.documentType == (int)rnt_ReservationTypeCode.Acente))
                    {
                        priceCalculator.Prices.payNowPrice = (priceHourEffect.effectRate * priceCalculator.Prices.payNowPrice) / 100;
                        amount = priceCalculator.Prices.payNowPrice;
                        //bireysel fiyatlardan calıs diyorsa pay now'u pay later'a eşitleyelim

                        priceCalculator.Prices.payLaterPrice = priceCalculator.Prices.payNowPrice;

                        if (exchangeRate != decimal.Zero &&
                            exchangeRate != 1M)
                        {
                            priceCalculator.Prices.payLaterPrice = priceCalculator.Prices.payLaterPrice * exchangeRate;
                            priceCalculator.Prices.payNowPrice = priceCalculator.Prices.payNowPrice * exchangeRate;
                            amount = amount * exchangeRate;
                        }
                    }
                }
                else
                {
                    //if not selected rez
                    priceCalculator.Prices.payLaterPrice = (priceHourEffect.effectRate * priceCalculator.Prices.payLaterPrice) / 100;
                    priceCalculator.Prices.payNowPrice = (priceHourEffect.effectRate * priceCalculator.Prices.payNowPrice) / 100;
                    amount = priceCalculator.Prices.payLaterPrice;

                    if (processIndividualPrices && selectedReservation == null &&
                        (corporateType.HasValue && (corporateType.Value == (int)rnt_AccountTypeCode.Broker || corporateType.Value == (int)rnt_AccountTypeCode.Agency)))
                    {
                        priceCalculator.Prices.payLaterPrice = priceCalculator.Prices.payNowPrice;
                        amount = priceCalculator.Prices.payLaterPrice;

                        if (exchangeRate != decimal.Zero &&
                            exchangeRate != 1M)
                        {
                            priceCalculator.Prices.payLaterPrice = priceCalculator.Prices.payLaterPrice * exchangeRate;
                            priceCalculator.Prices.payNowPrice = priceCalculator.Prices.payNowPrice * exchangeRate;
                            amount = amount * exchangeRate;
                        }
                    }
                }
                hasTickTime = true;
                //eğer %100 etki yapıyorsa , tick time değildir.
                tickTimeRate = priceHourEffect.effectRate;
            }
            else
            {
                if (selectedReservation?.paymentChoice == (int)PaymentEnums.PaymentType.PayNow)
                {
                    amount = priceCalculator.Prices.payNowPrice;
                }
                else if (selectedReservation?.paymentChoice == (int)PaymentEnums.PaymentType.PayLater)
                {
                    amount = priceCalculator.Prices.payLaterPrice;
                }

            }
            #endregion

            if ((selectedReservation?.documentType == (int)rnt_ReservationTypeCode.Broker ||
                 selectedReservation?.documentType == (int)rnt_ReservationTypeCode.Acente) && !hasTickTime)
            {

                priceCalculator.Prices.payLaterPrice = priceCalculator.Prices.payNowPrice;
                amount = priceCalculator.Prices.payLaterPrice;

                if (exchangeRate != decimal.Zero &&
                    exchangeRate != 1M)
                {
                    priceCalculator.Prices.payLaterPrice = priceCalculator.Prices.payLaterPrice * exchangeRate;
                    priceCalculator.Prices.payNowPrice = priceCalculator.Prices.payNowPrice * exchangeRate;
                    amount = amount * exchangeRate;
                }

            }
            else if (corporateType.HasValue &&
                    (corporateType.Value == (int)rnt_AccountTypeCode.Broker || corporateType.Value == (int)rnt_AccountTypeCode.Agency) &&
                    !hasTickTime)
            {
                if (processIndividualPrices)
                {
                    priceCalculator.Prices.payLaterPrice = priceCalculator.Prices.payNowPrice;
                    amount = priceCalculator.Prices.payLaterPrice;

                    if (exchangeRate != decimal.Zero &&
                        exchangeRate != 1M)
                    {
                        priceCalculator.Prices.payLaterPrice = priceCalculator.Prices.payLaterPrice * exchangeRate;
                        priceCalculator.Prices.payNowPrice = priceCalculator.Prices.payNowPrice * exchangeRate;
                        amount = amount * exchangeRate;
                    }
                }
            }

            return new PriceResponse
            {
                priceAfterAvailabilityFactor = priceCalculator.Prices.priceAfterAvailabilityFactor,
                priceAfterChannelFactor = priceCalculator.Prices.priceAfterChannelFactor,
                priceAfterCustomerFactor = priceCalculator.Prices.priceAfterCustomerFactor,
                priceAfterSpecialDaysFactor = priceCalculator.Prices.priceAfterSpecialDaysFactor,
                priceAfterWeekDaysFactor = priceCalculator.Prices.priceAfterWeekDaysFactor,
                priceAfterBranchFactor = priceCalculator.Prices.priceAfterBranchFactor,
                totalAmount = amount,
                payLaterAmount = priceCalculator.Prices.payLaterPrice,
                payNowAmount = priceCalculator.Prices.payNowPrice,
                baseAmount = basePrice,
                availibilityRatio = availabilityByDay,
                selectedPriceList = priceListData,
                selectedGroupCodeListPrice = groupCodePriceLists,
                selectedAvailabilityPriceList = availabilityPrices,
                relatedDay = relatedDay,
                payNowWithoutTickDayAmount = payNowWithoutTickPrice,
                payLaterWithoutTickDayAmount = payLaterWithoutTickPrice,
                isTickDay = tickTimeRate == 100 ? false : hasTickTime,
                priceAfterBranch2Factor = priceCalculator.Prices.priceAfterBranch2Factor,
                priceAfterEqualityFactor = priceCalculator.Prices.priceAfterEqualityFactor,
                currencyId = Convert.ToString(priceListData.transactionCurrencyId),
                currencyCode = priceListData.currencyCode,

            };
        }

        public PriceResponse calculatePriceByMonthly(Guid groupCodeInformationId,
                                                     string pickupBranchId,
                                                     DateTime relatedDay,
                                                     int customerType,
                                                     DocumentData selectedReservation,
                                                     int? corporateType,
                                                     bool processIndividualPrices,
                                                     string accountGroup,
                                                     Guid priceCodeId,
                                                     int dayCount,
                                                     List<PaymentPlanData> paymentPlans)
        {
            var d_dayCount = Convert.ToDecimal(dayCount);
            var relatedPlan = paymentPlans.Where(p => p.month == relatedDay.Month).FirstOrDefault();
            //yaratma esnasınad yada var olan aylar içinde bulamıyorsa yeniden al
            if (paymentPlans.Count == 0 || relatedPlan == null)
            {
                #region Create
                var types = this.calculateCustomerTypes(customerType, selectedReservation, corporateType, processIndividualPrices);

                PriceListDataMongoDB priceListData = null;
                if (types.cType == (int)GlobalEnums.CustomerType.Individual)
                {
                    priceListData = this.getIndividualMonthlyPriceList();

                }
                else if (types.cType == (int)GlobalEnums.CustomerType.Corporate)
                {
                    priceListData = this.getCorporateMonthlyPriceList(priceCodeId);
                }

                if (priceListData == null)
                {
                    return PriceResponse.notCalculated();
                }

                var groupCodePriceLists = new GroupCodeListPriceDataMongoDB();

                groupCodePriceLists = this.getMonthlyRelatedGroupCodeList(priceListData.PriceListId, groupCodeInformationId, relatedDay);

                if (groupCodePriceLists == null)
                {
                    return PriceResponse.notCalculated();
                }


                PriceCalculator priceCalculator = new PriceCalculator(new Guid(priceListData.PriceListId), priceListData.PriceListName);
                priceCalculator.Prices.payLaterPrice = groupCodePriceLists.ListPrice / d_dayCount;
                priceCalculator.Prices.totalPrice = priceCalculator.Prices.payLaterPrice;
                priceCalculator.applyPayMethodMonthly(priceCalculator.Prices.totalPrice, pickupBranchId, groupCodeInformationId.ToString(), types.cType, accountGroup);
                return new PriceResponse
                {
                    payLaterAmount = priceCalculator.Prices.payLaterPrice,
                    payNowAmount = priceCalculator.Prices.payNowPrice,
                    totalAmount = priceCalculator.Prices.totalPrice,
                    selectedPriceList = priceListData,
                    selectedGroupCodeListPrice = groupCodePriceLists,
                    currencyId = priceListData.transactionCurrencyId.ToString(),
                    currencyCode = priceListData.currencyCode
                };
                #endregion
            }
            PriceListDataMongoDB priceListDataExisting = this.getIndividualMonthlyPriceListById(relatedPlan.priceListId.ToString());
            return new PriceResponse
            {
                payLaterAmount = selectedReservation.paymentChoice == 10 ? relatedPlan.payNowAmount / d_dayCount : relatedPlan.payLaterAmount / d_dayCount,
                payNowAmount = selectedReservation.paymentChoice == 10 ? relatedPlan.payNowAmount / d_dayCount : relatedPlan.payLaterAmount / d_dayCount,
                totalAmount = selectedReservation.paymentChoice == 10 ? relatedPlan.payNowAmount / d_dayCount : relatedPlan.payLaterAmount / d_dayCount,
                selectedPriceList = priceListDataExisting,
                selectedGroupCodeListPrice = new GroupCodeListPriceDataMongoDB
                {
                    GroupCodeInformationId = Convert.ToString(groupCodeInformationId),
                    GroupCodeInformationName = Convert.ToString(groupCodeInformationId),
                    ListPrice = selectedReservation.paymentChoice == 10 ? relatedPlan.payNowAmount / d_dayCount : relatedPlan.payLaterAmount / d_dayCount,
                },
                currencyId = priceListDataExisting.transactionCurrencyId.ToString(),
                currencyCode = priceListDataExisting.currencyCode
            };


        }
        public PriceListDataMongoDB getIndividualMonthlyPriceList()
        {
            MonthlyPriceListRepository monthlyPriceListRepository = new MonthlyPriceListRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                   StaticHelper.GetConfiguration("MongoDBDatabaseName"));

            var monthly = monthlyPriceListRepository.getIndividualMonthlyPriceList();

            if (monthly == null)
            {
                return null;
            }
            return new PriceListDataMongoDB
            {
                PriceListId = monthly.monthlyPriceListId,
                currencyCode = monthly.currencyCode,
                transactionCurrencyId = new Guid(monthly.transactionCurrencyId),
                transactionCurrencyName = monthly.transactionCurrencyName
            };

        }
        public PriceListDataMongoDB getCorporateMonthlyPriceList(Guid priceCodeId)
        {
            MonthlyPriceListRepository monthlyPriceListRepository = new MonthlyPriceListRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                   StaticHelper.GetConfiguration("MongoDBDatabaseName"));

            var monthly = monthlyPriceListRepository.getCorporateMonthlyPriceList(priceCodeId);

            if (monthly == null)
            {
                return null;
            }
            return new PriceListDataMongoDB
            {
                PriceListId = monthly.monthlyPriceListId,
                currencyCode = monthly.currencyCode,
                transactionCurrencyId = new Guid(monthly.transactionCurrencyId),
                transactionCurrencyName = monthly.transactionCurrencyName
            };

        }
        public PriceListDataMongoDB getIndividualMonthlyPriceListById(string priceListId)
        {
            MonthlyPriceListRepository monthlyPriceListRepository = new MonthlyPriceListRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                   StaticHelper.GetConfiguration("MongoDBDatabaseName"));

            var monthly = monthlyPriceListRepository.getIndividualMonthlyPriceListById(priceListId);

            if (monthly == null)
            {
                return null;
            }
            return new PriceListDataMongoDB
            {
                PriceListId = monthly.monthlyPriceListId,
                currencyCode = monthly.currencyCode,
                transactionCurrencyId = new Guid(monthly.transactionCurrencyId),
                transactionCurrencyName = monthly.transactionCurrencyName
            };

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="priceListType">1 = individual , 2 = corporate </param>
        public PriceListDataMongoDB getRelatedPriceList(int priceListType, DateTime relatedDay)
        {
            PriceListRepository priceListRepository = new PriceListRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                               StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            //PriceType = 1 --> individual
            //PriceType = 2 --> corporate
            return priceListRepository.getActivePriceListByPriceType(priceListType, relatedDay);

        }
        public PriceListDataMongoDB getRelatedPriceListByPriceCode(int priceListType, DateTime relatedDay, string priceCodeId)
        {
            PriceListRepository priceListRepository = new PriceListRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                               StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            //PriceType = 1 --> individual
            //PriceType = 2 --> corporate
            return priceListRepository.getRelatedPriceListByPriceCode(priceListType, relatedDay, priceCodeId);

        }
        public GroupCodeListPriceDataMongoDB getRelatedGroupCodeList(string priceListId, Guid groupCodeInformationId)
        {
            GroupCodeListPriceRepository groupCodeListPriceRepository = new GroupCodeListPriceRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                         StaticHelper.GetConfiguration("MongoDBDatabaseName"));

            //get active groupcode priceLists
            return groupCodeListPriceRepository.getGroupCodeListPricesByPriceListandGroupCodeWithDuration(priceListId, groupCodeInformationId, this.totalDuration);

        }
        public GroupCodeListPriceDataMongoDB getMonthlyRelatedGroupCodeList(string priceListId, Guid groupCodeInformationId, DateTime relatedDate)
        {
            MonthlyGroupCodePriceListRepository monthlyGroupCodePriceListRepository = new MonthlyGroupCodePriceListRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                                              StaticHelper.GetConfiguration("MongoDBDatabaseName"));

            //get active groupcode priceLists
            var monthlyGroupCode = monthlyGroupCodePriceListRepository.getMonthlyGroupCodePriceListByGivenPriceList(priceListId, Convert.ToString(groupCodeInformationId), relatedDate.Month);

            if (monthlyGroupCode == null)
            {
                return null;
            }
            return new GroupCodeListPriceDataMongoDB
            {
                GroupCodeListPriceId = monthlyGroupCode.monthlyGroupCodeListId,
                PriceListId = monthlyGroupCode.monthlyPriceListId,
                PriceListName = monthlyGroupCode.monthlyPriceListName,
                Name = monthlyGroupCode.name,
                GroupCodeInformationId = monthlyGroupCode.groupCodeId,
                GroupCodeInformationName = monthlyGroupCode.groupCodeName,
                ListPrice = monthlyGroupCode.amount
            };

        }

        public AvailabilityPriceListDataMongoDB getAvailabilityPricesByDuration(string priceListId, int availabilityRatio)
        {
            AvailabilityPriceListRepository availabilityPriceListRepository = new AvailabilityPriceListRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                            StaticHelper.GetConfiguration("MongoDBDatabaseName"));

            return availabilityPriceListRepository.getAvailabilityPriceByPriceListWithDuration(priceListId, availabilityRatio);
        }

        public AvailabilityPriceListDataMongoDB getAvailabilityPricesByDurationByGroupCode(string priceListId, int availabilityRatio, Guid groupCodeInformationId)
        {
            AvailabilityPriceListRepository availabilityPriceListRepository = new AvailabilityPriceListRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                            StaticHelper.GetConfiguration("MongoDBDatabaseName"));

            return availabilityPriceListRepository.getAvailabilityPriceByPriceListWithDurationByGroupCode(priceListId, availabilityRatio, groupCodeInformationId);
        }
        public List<AvailabilityPriceListDataMongoDB> getAvailabilityPricesByDurationByGroupCode(string priceListId, Guid groupCodeInformationId)
        {
            AvailabilityPriceListRepository availabilityPriceListRepository = new AvailabilityPriceListRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                                  StaticHelper.GetConfiguration("MongoDBDatabaseName"));

            return availabilityPriceListRepository.getAvailabilityPriceByPriceListWithDurationByGroupCode(priceListId, groupCodeInformationId);
        }
        public CalculatedCampaignPrice CalculateCampaignPrice(CampaignData campaignData, decimal payNowDailyPrice, decimal payLaterDailyPrice, bool isTickDay, string groupCodeId = null)
        {
            var calculatedCampaingPrices = new CalculatedCampaignPrice();
            calculatedCampaingPrices.CampaignInfo = campaignData;

            if (campaignData.campaignType == 1 /* Discount */)
            {
                calculatedCampaingPrices.payNowDailyPrice = payNowDailyPrice - (payNowDailyPrice * campaignData.payNowDiscountRatio / 100);
                calculatedCampaingPrices.payLaterDailyPrice = payLaterDailyPrice - (payLaterDailyPrice * campaignData.payLaterDiscountRatio / 100);
            }
            //main price kampanyası
            else if (campaignData.campaignType == 4)
            {
                var x = campaignData.groupCodePrices?.Where(p => p.groupcodeId == groupCodeId).FirstOrDefault();
                if (x != null)
                {

                    calculatedCampaingPrices.payNowDailyPrice = x.paynowPrice;
                    calculatedCampaingPrices.payLaterDailyPrice = x.paylaterPrice;
                }
                else
                {
                    calculatedCampaingPrices.payNowDailyPrice = payNowDailyPrice;
                    calculatedCampaingPrices.payLaterDailyPrice = payLaterDailyPrice;

                }
            }
            else /* Fix Price */
            {
                //sabit fiyat kampanyası tick day fiyatını ezmemesi için
                if (isTickDay)
                {
                    calculatedCampaingPrices.payNowDailyPrice = payNowDailyPrice;
                    calculatedCampaingPrices.payLaterDailyPrice = payLaterDailyPrice;
                }
                else
                {
                    calculatedCampaingPrices.payNowDailyPrice = campaignData.payNowDailyPrice;
                    calculatedCampaingPrices.payLaterDailyPrice = campaignData.payLaterDailyPrice;
                }

            }

            return calculatedCampaingPrices;
        }
        private decimal calculateAvailabilityRatio(int equipmentCountByGroupCode, int reservationsCountOnSelectedDay)
        {
            //avoid zero divide exception need to check equipmentCountByGroupCode
            decimal availabilityByDay = decimal.Zero;
            if (equipmentCountByGroupCode == 0 && reservationsCountOnSelectedDay > 0)
            {
                availabilityByDay = 100;
            }
            else if (equipmentCountByGroupCode == 0 && reservationsCountOnSelectedDay == 0)
            {
                availabilityByDay = 0;
            }
            else
            {
                availabilityByDay = (reservationsCountOnSelectedDay * 100) / equipmentCountByGroupCode;
            }

            return availabilityByDay;
        }

    }

}
