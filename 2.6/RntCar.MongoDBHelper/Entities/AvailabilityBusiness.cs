using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
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
using System.Threading.Tasks;
using static RntCar.ClassLibrary.GlobalEnums;

namespace RntCar.MongoDBHelper.Entities
{
    public class AvailabilityBusiness
    {
        public AvailabilityParameters availabilityParameters { get; set; }
        private DocumentData selectedDocumentData { get; set; }
        public string trackingNumber { get; set; }
        private int shiftDifference { get; set; } = 0;
        private DocumentUpdateType documentUpdateType { get; set; }
        private bool userCanBenefitFromCampaign { get; set; } = false;
        private double _totalMinutes { get; set; } = 0;
        private Dictionary<DateTime, bool> campaignDates = new Dictionary<DateTime, bool>();
        private List<AvailabilityFactorData> availabilityFactorIncreaseCapacity { get; set; }

        private PricingType _pricingType { get; set; }

        public AvailabilityBusiness(AvailabilityParameters _availabilityParameters)
        {
            availabilityParameters = _availabilityParameters;
            trackingNumber = Guid.NewGuid().ToString();  //StaticHelper.RandomDigits(20);
        }

        public List<AvailabilityData> calculateAvailability()
        {
            //remove miliseconds , add offset vs..
            this.prepareParameters();
            //this parameter is gathering the available equipments
            List<AvailableEquipments> equipments = new List<AvailableEquipments>();
            //collect current equipments and incoming from reservations
            equipments.AddRange(this.collectEquipments());
            equipments = equipments.DistinctBy(p => p.equipmentId).ToList();
            //group by group codes
            var results = equipments.groupByGroupCodes();
            //distinct groupcodes
            //this list is very important because all availability related days will be calculated respectively by that list
            var branchGroupCodes = equipments.DistinctBy(p => p.groupCodeInfoId).Select(p => p.groupCodeInfoId).ToList();

            //apply increase  ( Availability Factor )
            //get availability factor increase capacity
            this.getActiveIncreaseCapacityAvailabilityFactor();
            if (this.availabilityFactorIncreaseCapacity.Count > 0)
            {
                List<string> s = new List<string>();
                availabilityFactorIncreaseCapacity.ForEach(p => s.Add(p.groupCodeList));
                var groupCodeValuesForIncreaseCapacity = new List<AvailabilityFactorGroupCodes>();
                foreach (var item in s)
                {
                    groupCodeValuesForIncreaseCapacity.AddRange(JsonConvert.DeserializeObject<List<AvailabilityFactorGroupCodes>>(item));
                }
                groupCodeValuesForIncreaseCapacity = groupCodeValuesForIncreaseCapacity.DistinctBy(p => p.id).ToList();

                // if the result does not contain increase capacity group code value then add value to result
                groupCodeValuesForIncreaseCapacity.ForEach(item =>
                {
                    var id = results.Where(p => new Guid(p.id) == new Guid(item.id)).FirstOrDefault();
                    if (id == null)
                    {
                        results.Add(new Results { id = item.id.ToString(), name = item.name, count = 0 });
                        branchGroupCodes.Add(item.id.ToString());
                    }
                });
            }

            this.prepareSelectedDocumentData(branchGroupCodes);
            //if it is not existing reservation 
            //we can remove group codes for availability factors
            //check availability closure
            if (this.availabilityParameters.checkGroupClosure)
            {
                if (!this.isExistingDocument())
                {
                    this.availabilityClosureByAvailabilityFactor(ref branchGroupCodes, ref results, true);
                }
                else if (this.isExistingDocument())
                {
                    //remove all group codes even reservation itself
                    if (this.documentUpdateType != DocumentUpdateType.notChanged &&
                        this.documentUpdateType != DocumentUpdateType.shiftedWithAllowedDuration)
                    {
                        this.availabilityClosureByAvailabilityFactor(ref branchGroupCodes, ref results, true);
                    }
                    else
                    {
                        this.availabilityClosureByAvailabilityFactor(ref branchGroupCodes, ref results, false);
                    }
                    //if there is no equipment for selected group code , add it manually
                    if (branchGroupCodes.Where(p => new Guid(p) == new Guid(this.selectedDocumentData.pricingGroupCodeId)).FirstOrDefault() == null)
                    {
                        branchGroupCodes.Add(this.selectedDocumentData.pricingGroupCodeId);
                        branchGroupCodes = branchGroupCodes.DistinctBy(p => p).ToList();

                        results.Add(new Results { count = 1, id = this.selectedDocumentData.pricingGroupCodeId, name = this.selectedDocumentData.pricingGroupCodeName });
                        results = results.DistinctBy(p => p.id).ToList();

                    }
                }
            }
            else
            {
                if (this.isExistingDocument())
                {
                    if (branchGroupCodes.Where(p => new Guid(p) == new Guid(this.selectedDocumentData.pricingGroupCodeId)).FirstOrDefault() == null)
                    {
                        branchGroupCodes.Add(this.selectedDocumentData.pricingGroupCodeId);
                        branchGroupCodes = branchGroupCodes.DistinctBy(p => p).ToList();

                        results.Add(new Results { count = 1, id = this.selectedDocumentData.pricingGroupCodeId, name = this.selectedDocumentData.pricingGroupCodeName });
                        results = results.DistinctBy(p => p.id).ToList();

                    }

                }

            }

            //exclude equipments from outgoing reservations
            #region Exclude reservations from total
            var exludeReservations = calculateExcludeItems(branchGroupCodes);
            #endregion
            //evaluate final object
            var response = this.evaluateReturnObject(results, exludeReservations);

            Guid _corpId = Guid.Empty;
            _pricingType = string.IsNullOrEmpty(this.availabilityParameters.corporateCustomerId) ||
                                                   (Guid.TryParse(this.availabilityParameters.corporateCustomerId, out _corpId) &&
                                                   _corpId == Guid.Empty) ? PricingType.individual : PricingType.corporate;


            //todo --> check why groupCodeInformationName is sent
            response = this.calculatePrice(response,
                                           exludeReservations,
                                           this.selectedDocumentData?.pricingGroupCodeName,
                                           this.selectedDocumentData?.totalAmount,
                                           StaticHelper.RandomDigits(10));

            if (this.selectedDocumentData == null && !this.availabilityParameters.campaignId.HasValue)
            {
                var parametersTotalDuration = CommonHelper.calculateTotalDurationInMinutes(availabilityParameters.pickupDateTime,
                                                                                           availabilityParameters.dropoffDateTime);

                //sabit fiyat kampanyası(main price ) > kullanabilirlik fiyatını ezer ;
                CampaignRepository campaignRepository = new CampaignRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                               StaticHelper.GetConfiguration("MongoDBDatabaseName"));


                var campaign = campaignRepository.GetCampaign_MainPrice(new BsonTimestamp(this.availabilityParameters.pickupDateTime.converttoTimeStamp()),
                                                                        Convert.ToInt32(TimeSpan.FromMinutes(parametersTotalDuration).TotalDays),
                                                                        this.availabilityParameters.channel.ToString(),
                                                                        this.availabilityParameters.pickupBranchId.ToString(),
                                                                        this.availabilityParameters.processIndividualPrices_broker ? 1 : this.availabilityParameters.customerType);
                PriceCalculationSummariesRepository priceCalculationSummariesRepository = new PriceCalculationSummariesRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                                                  StaticHelper.GetConfiguration("MongoDBDatabaseName"));

                PriceCalculationSummariesBusiness priceCalculationSummariesBusiness = new PriceCalculationSummariesBusiness(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                                            StaticHelper.GetConfiguration("MongoDBDatabaseName"));

                var collection1 = priceCalculationSummariesBusiness.getCollection<PriceCalculationSummaryMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceCalculationSummary"));

                //insert response to mongodb
                MongoDBInstance mongoDBInstanceCamp = new MongoDBInstance(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                          StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var collection2 = mongoDBInstanceCamp.getCollection<CampaignAvailabilityMongoDB>("AvailabilityCampaings");

                if (campaign != null)
                {
                    foreach (var item in campaign.groupCodePrices)
                    {

                        var _prices = priceCalculationSummariesRepository.getPricesCalculationSummariesByTrackingNumberandGroupCode(item.groupcodeId, this.trackingNumber);
                        var totalPrice = decimal.Zero;
                        var totalPricePayNow = decimal.Zero;
                        foreach (var p in _prices)
                        {
                            if (this.availabilityParameters.processIndividualPrices_broker)
                            {
                                p.payLaterAmount = item.paynowPrice;
                                p.payNowAmount = item.paynowPrice;
                                totalPrice += item.paynowPrice;
                                totalPricePayNow += item.paynowPrice;
                                p.totalAmount = item.paynowPrice;
                            }
                            else
                            {
                                p.payLaterAmount = item.paylaterPrice;
                                p.payNowAmount = item.paynowPrice;
                                totalPrice += item.paylaterPrice;
                                totalPricePayNow += item.paynowPrice;
                                p.totalAmount = item.paylaterPrice;
                            }
                            
                            p.campaignId = new Guid(campaign.campaignId);

                            collection1.ReplaceOne(x => x._id == p._id, p, new UpdateOptions { IsUpsert = false });
                        }
                        if (totalPrice > decimal.Zero)
                        {
                            var v = response.Where(z => z.groupCodeInformationId == new Guid(item.groupcodeId)).FirstOrDefault();
                            calculateFinalPrices(v, decimal.Round(totalPrice, 2));
                            v.payNowTotalPrice = totalPricePayNow;
                            v.payLaterTotalPrice = totalPrice;
                        }

                        collection2.Insert(new CampaignAvailabilityMongoDB
                        {
                            _id = ObjectId.GenerateNewId(),
                            groupCodedId = item.groupcodeId,
                            campaignId = campaign.campaignId,
                            trackingNumber = this.trackingNumber
                        });
                    }
                }


            }

            response = response.OrderBy(p => p.totalPrice).ToList();

            //insert response to mongodb
            MongoDBInstance mongoDBInstance = new MongoDBInstance(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                  StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            var collection = mongoDBInstance.getCollection<AvailabilityRes>("AvailabilityResponses");
            collection.Insert(new AvailabilityRes
            {
                availabilityData = JsonConvert.SerializeObject(response),
                trackingNumber = this.trackingNumber
            });
            return response;
        }
        //todo will implement will more deeply
        public List<AvailabilityData> calculatePrice(List<AvailabilityData> response,
                                                    ExcludeAvailability excludeItems,
                                                    string relatedGroupCode,
                                                    decimal? totalAmount,
                                                    string randomCode)
        {
            if (Convert.ToBoolean(StaticHelper.GetConfiguration("usethread")))
            {
                List<Task> _tasks = new List<Task>();

                int i = 0;
                int divider = 3;
                var remaining = int.MaxValue;
                while (true == true)
                {
                    if (remaining <= 0)
                        break;
                    var l = new List<AvailabilityData>();
                    l.AddRange(response.Skip(i).Take(divider).ToList());
                    var aval = new Task(() =>
                    {
                        calculateAvailabilityItem(l, excludeItems, relatedGroupCode, totalAmount, randomCode);
                    });
                    _tasks.Add(aval);
                    aval.Start();
                    i += divider;
                    remaining = response.Count - i;
                }
                Task.WaitAll(_tasks.ToArray());
            }
            else
            {
                calculateAvailabilityItem(response, excludeItems, relatedGroupCode, totalAmount, randomCode);
            }

            return response;

        }
        private void calculateAvailabilityItem(List<AvailabilityData> response,
                                               ExcludeAvailability excludeItems,
                                               string relatedGroupCode,
                                               decimal? totalAmount,
                                               string randomCode)
        {
            foreach (var availability in response)
            {
                var dayAvailibilityResult = excludeItems.availabilityDayResults.Where(p => new Guid(p.Key).Equals(availability.groupCodeInformationId))
                                            .Select(p => p.Value).FirstOrDefault();
                if (dayAvailibilityResult == null)
                {
                    // get group code closure error for availability data
                    groupCodeClosureByAvailabilityFactorError(availability);
                }
                //if same group code and and dates are not changed
                else if (availability.groupCodeInformationName == relatedGroupCode &&
                         this.documentUpdateType == DocumentUpdateType.notChanged)
                {
                    calculateFinalPrices(availability, decimal.Round(totalAmount.HasValue ? totalAmount.Value : decimal.Zero, 2));
                    //if dates are not changed user always can has benefit
                    this.fillCampaignDataforNotChangedandAllowedShiftDocument(availability);
                    createCalculationSummariesforNotChangedandAllowedShiftDocument(availability);
                    availability.currencyCode = decideCurrencyExistingDocument().currencyCode;
                    availability.currencyId = decideCurrencyExistingDocument().currencyId;
                }
                //check reservation is less or extend or shift 
                //and existing reservation
                else if (availability.groupCodeInformationName == relatedGroupCode & this.isExistingDocument() &&
                         this.documentUpdateType != DocumentUpdateType.monthly)
                {

                    //if (!string.IsNullOrEmpty(this.availabilityParameters.contractId) && this.availabilityParameters.operationType == 50)
                    //{
                    //    ContractItemRepository contractItemRepository = new ContractItemRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                    //                                                                               StaticHelper.GetConfiguration("MongoDBDatabaseName"));

                    //    var item = contractItemRepository.getPriceDifference(this.availabilityParameters.contractId);
                    //    //erken getirme senaryosunda , pay broker ödemeli bir sözleşmenin tarihinden erken getiriyorsa pay broker'ın bitiş tarihini dikkate al.
                    //    if (item != null)
                    //    {
                    //        if (this.availabilityParameters.dropoffDateTime <= item.dropoffDateTime.Value)
                    //        {
                    //            this.trackingNumber = StaticHelper.RandomDigits(10);
                    //            this.fillCampaignDataforNotChangedandAllowedShiftDocument(availability);
                    //            createCalculationSummariesforNotChangedandAllowedShiftDocument(availability);
                    //            var contractItems = contractItemRepository.getContractItemsEquipment(this.selectedDocumentData.documentId);
                    //            calculateFinalPrices(availability, contractItems.Sum(p => p.totalAmount));
                    //            continue;
                    //        }
                    //    }
                    //}
                    var totalMinutesForReservation = CommonHelper.calculateTotalDurationInMinutes(this.selectedDocumentData.pickupTimeStamp.Value,
                                                                                                  this.selectedDocumentData.dropoffTimeStamp.Value);

                    var parametersTotalDuration = CommonHelper.calculateTotalDurationInMinutes(availabilityParameters.pickupDateTime,
                                                                                               availabilityParameters.dropoffDateTime);


                    #region Calculate Which dates are included in campaigns
                    if (this.selectedDocumentData.campaignId != null)
                    {
                        //var startPickUpDateTime = this.availabilityParameters.pickupDateTime.AddMinutes(StaticHelper.offset);
                        //var endDropoffDateTime = this.availabilityParameters.dropoffDateTime.AddMinutes(StaticHelper.offset);
                        var startPickUpDateTime = this.availabilityParameters.pickupDateTime;
                        var endDropoffDateTime = this.availabilityParameters.dropoffDateTime;
                        ContractItemRepository contractItemRepository = new ContractItemRepository(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                        var contractItems = contractItemRepository.getContractItemsEquipment(this.selectedDocumentData.documentId);

                        //araç değişikliği varsa pickup olarak ilk date'i al
                        var deliveredItems = contractItems.Where(p => p.statuscode == (int)rnt_contractitem_StatusCode.WaitingForDelivery).ToList();
                        var rentalItems = contractItems.Where(p => p.statuscode == (int)rnt_contractitem_StatusCode.Rental).ToList();
                        var completedItems = contractItems.Where(p => p.statuscode == (int)rnt_contractitem_StatusCode.Completed).ToList();
                        List<ContractItemDataMongoDB> allItems = new List<ContractItemDataMongoDB>();
                        if ((completedItems.Count > 0 && (deliveredItems.Count > 0 || rentalItems.Count > 0)) ||
                            (deliveredItems.Count == 1 && rentalItems.Count == 1))
                        {
                            allItems.AddRange(deliveredItems);
                            allItems.AddRange(rentalItems);
                            allItems.AddRange(completedItems);
                            startPickUpDateTime = allItems.Min(p => p.pickupDateTime).Value;

                            totalMinutesForReservation = CommonHelper.calculateTotalDurationInMinutes(startPickUpDateTime,
                                                                                                      endDropoffDateTime);

                            parametersTotalDuration = CommonHelper.calculateTotalDurationInMinutes(startPickUpDateTime,
                                                                                                   availabilityParameters.dropoffDateTime);
                        }
                        availability.documentHasCampaignBefore = true;
                        for (DateTime t = startPickUpDateTime; t < endDropoffDateTime; t += TimeSpan.FromDays(1))
                        {
                            var checkMaxDate = false;
                            var campaignRepository = new CampaignRepository(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                            var utcDate = t.AddMinutes(-StaticHelper.offset);
                            if (t > this.selectedDocumentData.dropoffDateTime.Value.AddDays(-1))
                            {
                                parametersTotalDuration = CommonHelper.calculateTotalDurationInMinutes(startPickUpDateTime, t.AddDays(1));
                                checkMaxDate = true;
                            }
                            //campaign dates will store as utc so , need to extract from the date                           

                            var campaign = campaignRepository.getCampaignByParameters(Convert.ToString(this.selectedDocumentData.campaignId.Value),
                                                                                      new BsonTimestamp(utcDate.converttoTimeStamp()),
                                                                                      Convert.ToInt32(TimeSpan.FromMinutes(parametersTotalDuration).TotalDays),
                                                                                      Convert.ToString(this.availabilityParameters.channel),
                                                                                      this.selectedDocumentData.pickupBranchId,
                                                                                      this.availabilityParameters.customerType,
                                                                                      this.selectedDocumentData.pricingGroupCodeId,
                                                                                      checkMaxDate);
                            //her seferinde kampanya
                            if (campaign != null)
                            {
                                this.campaignDates.Add(t.Date, true);
                            }
                            else
                            {
                                this.campaignDates.Add(t, false);
                            }

                            if (campaign != null && availability.CampaignInfo == null)
                            {
                                availability.CampaignInfo = new CampaignHelper().buildCampaignData(campaign);
                            }
                        }
                        this.userCanBenefitFromCampaign = this.campaignDates.Where(p => p.Value == true).ToList().Count > 0 ? true : false;
                        availability.canUserStillHasCampaignBenefit = this.userCanBenefitFromCampaign;


                    }
                    #endregion

                    #region Selected Document Price Operation

                    if (this.selectedDocumentData.isMonthly && !this.availabilityParameters.isMonthly)
                    {
                        var t = CommonHelper.calculateTotalDurationInMinutes(this.availabilityParameters.month_pickupdatetime,
                                                                              this.availabilityParameters.month_dropoffdatetime);
                        totalMinutesForReservation = totalMinutesForReservation % t;
                        parametersTotalDuration = parametersTotalDuration % t;
                        _totalMinutes = parametersTotalDuration - totalMinutesForReservation;
                    }

                    availability.documentHasCampaignBefore = this.selectedDocumentData.campaignId != null ? true : false;
                    //dayAvailibilityResult = checkDayAvailabilityDays(dayAvailibilityResult);
                    selectedDocumentPriceOperation(availability, dayAvailibilityResult, totalAmount);
                    availability.currencyCode = decideCurrencyExistingDocument().currencyCode;
                    availability.currencyId = decideCurrencyExistingDocument().currencyId;
                    #endregion
                }
                else if (availability.groupCodeInformationName == relatedGroupCode & this.isExistingDocument() &&
                        this.documentUpdateType == DocumentUpdateType.monthly)
                {
                    //rezervasyon
                    if (this.selectedDocumentData.type == DocumentType.reservation)
                    {
                        var difference = this.checkMonthlyDuration(this.selectedDocumentData.paymentPlans, this.availabilityParameters.priceCodeId).Count;
                        //no changed
                        if (this.selectedDocumentData.monthValue == this.availabilityParameters.monthValue &&
                            difference == 0)
                        {

                            var relatedPlan = this.selectedDocumentData
                                                  .paymentPlans
                                                  .Where(p => p.month == this.availabilityParameters.pickupDateTime.Month)
                                                  .FirstOrDefault();


                            var price = this.monthOperations(availability, this.selectedDocumentData.paymentPlans);
                            if (price == decimal.Zero)
                            {
                                pricesCouldntCalculated(availability);
                            }
                            else
                            {

                                availability.documentEquipmentPrice = price;
                                availability.payLaterTotalPrice = price;
                                availability.payNowTotalPrice = price;
                                availability.totalPrice = price;
                                availability.paymentPlanData = this.selectedDocumentData.paymentPlans;
                            }

                        }
                        //kesişen ayları koru yenileri yeniden hesapla
                        else
                        {
                            var price = this.monthOperations(availability, this.selectedDocumentData.paymentPlans);
                            if (price == decimal.Zero)
                            {
                                pricesCouldntCalculated(availability);
                            }
                            else
                            {
                                availability.documentEquipmentPrice = price;
                                availability.payLaterTotalPrice = price;
                                availability.payNowTotalPrice = price;
                                availability.totalPrice = price;
                                availability.documentItemId = new Guid(this.selectedDocumentData.documentItemId);
                            }

                        }
                    }
                    else if (this.selectedDocumentData.type == DocumentType.contract)
                    {
                        var price = this.monthOperations(availability, this.selectedDocumentData.paymentPlans);
                        if (price == decimal.Zero)
                        {
                            pricesCouldntCalculated(availability);
                        }
                        else
                        {
                            availability.documentItemId = new Guid(this.selectedDocumentData.documentItemId);
                            availability.documentEquipmentPrice = this.selectedDocumentData.totalAmount;
                            availability.payLaterTotalPrice = price;
                            availability.payNowTotalPrice = price;
                            availability.totalPrice = price;

                            //means contract update
                            if (availabilityParameters.operationType == 100)
                            {
                                availability.documentItemId = new Guid(this.selectedDocumentData.documentItemId);
                                availability.documentEquipmentPrice = this.selectedDocumentData.totalAmount;
                                availability.payLaterTotalPrice = price + this.selectedDocumentData.totalAmount;
                                availability.payNowTotalPrice = price + this.selectedDocumentData.totalAmount;
                                availability.totalPrice = price + this.selectedDocumentData.totalAmount;

                                //sözleşme güncellemeden aylık geliyorsa önceki fiyatlarıda alalım
                                foreach (var item in this.getDailyPricesforSelectedDocument())
                                {

                                    var relatedoldPriceCalculationSummary = new PriceCalculationSummaryMongoDB();
                                    relatedoldPriceCalculationSummary = relatedoldPriceCalculationSummary.Map(item);
                                    //todo will check null controls
                                    relatedoldPriceCalculationSummary.trackingNumber = this.trackingNumber;
                                    relatedoldPriceCalculationSummary._id = ObjectId.GenerateNewId();
                                    relatedoldPriceCalculationSummary.priceDate = item.priceDate;
                                    relatedoldPriceCalculationSummary.priceDateTimeStamp = new BsonTimestamp(item.priceDate.converttoTimeStamp());
                                    relatedoldPriceCalculationSummary.totalAmount = item.totalAmount;

                                    PriceCalculationSummariesBusiness priceCalculationSummariesBusiness = new PriceCalculationSummariesBusiness(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                                                                StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                                    priceCalculationSummariesBusiness.createPriceCalculationSummaryFromExisting(relatedoldPriceCalculationSummary);
                                }
                            }

                        }
                    }
                }
                else
                {
                    //dayAvailibilityResult = checkDayAvailabilityDays(dayAvailibilityResult);
                    var price = calculateStandartPrice(availability, dayAvailibilityResult);

                    //if any price factor definition coudlnt found or it faces with exception calculatePriceByGivenDayForIndividual return decimal.zero
                    //so we can not show wrong prices to customers
                    if (price != decimal.MinValue)
                    {
                        calculateFinalPrices(availability, decimal.Round(price, 2));
                    }
                    else
                    {
                        pricesCouldntCalculated(availability);
                    }
                }
                DurationHelper durationHelper = new DurationHelper(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                   StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                availability.totalDuration = durationHelper.calculateDocumentDurationByPriceHourEffect(availabilityParameters.pickupDateTime, availabilityParameters.dropoffDateTime);
            }

        }
        private Dictionary<DateTime, decimal> checkDayAvailabilityDays(Dictionary<DateTime, decimal> dayAvailibilityResult)
        {
            //calculate different price for everyday
            DurationHelper durationHelper = new DurationHelper(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            var d = durationHelper.calculateDocumentDurationByPriceHourEffect(this.availabilityParameters.pickupDateTime, this.availabilityParameters.dropoffDateTime);

            if (dayAvailibilityResult.Count > d)
            {
                var tmp = new Dictionary<DateTime, decimal>();
                for (int i = 0; i < d; i++)
                {
                    tmp.Add(dayAvailibilityResult.ElementAt(i).Key, dayAvailibilityResult.ElementAt(i).Value);
                }
                dayAvailibilityResult = tmp;
            }
            return dayAvailibilityResult;

        }

        private void calculateFinalPrices(AvailabilityData availability, decimal totalAmount)
        {
            if (this.availabilityParameters.operationType.HasValue &&
                this.availabilityParameters.operationType.Value == 50)
            {
                var _price = decimal.Round(totalAmount, 2);
                var _documentPrice = this.selectedDocumentData != null ?
                                     this.selectedDocumentData.totalAmount : decimal.Zero;
                //iade de para çıkarmaması için
                if (_price > _documentPrice && this.documentUpdateType == DocumentUpdateType.shorten)
                {
                    availability.totalPrice = decimal.Round(_documentPrice, 2);
                    availability.operationType = this.availabilityParameters.operationType.Value;
                }
                //iade de gün değişmiyorsa çektiğimiz parayı iade etmemek için
                else if (_price <= _documentPrice && this.documentUpdateType == DocumentUpdateType.shorten && this.selectedDocumentData != null)
                {
                    DurationHelper durationHelper = new DurationHelper(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                    var parameterDuration = durationHelper.calculateDocumentDurationByPriceHourEffect_Decimal(this.availabilityParameters.pickupDateTime, this.availabilityParameters.dropoffDateTime);

                    var documentDuration = durationHelper.calculateDocumentDurationByPriceHourEffect_Decimal(this.selectedDocumentData.pickupDateTime.Value, this.selectedDocumentData.dropoffDateTime.Value);

                    if (parameterDuration == documentDuration)
                    {
                        availability.totalPrice = decimal.Round(_documentPrice, 2);
                        availability.operationType = this.availabilityParameters.operationType.Value;
                    }
                    else
                    {
                        availability.totalPrice = decimal.Round(totalAmount, 2);
                    }

                }
                else
                {
                    availability.totalPrice = decimal.Round(totalAmount, 2);
                }
            }
            else
            {
                availability.totalPrice = decimal.Round(totalAmount, 2);
            }
            availability.documentEquipmentPrice = this.selectedDocumentData != null ?
                                                  this.selectedDocumentData.totalAmount : decimal.Zero;


            if (this.isExistingDocument())
            {
                availability.documentItemId = Guid.Parse(this.selectedDocumentData.documentItemId);
                availability.payLaterTotalPrice = availability.totalPrice;
                availability.payNowTotalPrice = availability.totalPrice;
            }
            else
            {

                availability.payLaterTotalPrice = availability.totalPrice;
                PriceCalculator priceCalculator = new PriceCalculator();
                if (!this.availabilityParameters.isMonthly)
                {
                    priceCalculator.applyPayMethod(availability.totalPrice,
                                              Convert.ToString(this.availabilityParameters.pickupBranchId),
                                              Convert.ToString(availability.groupCodeInformationId),
                                              this.availabilityParameters.customerType,
                                              this.availabilityParameters.accountGroup);
                }
                else
                {
                    priceCalculator.applyPayMethodMonthly(availability.totalPrice,
                                             Convert.ToString(this.availabilityParameters.pickupBranchId),
                                             Convert.ToString(availability.groupCodeInformationId),
                                             this.availabilityParameters.customerType,
                                             this.availabilityParameters.accountGroup);
                }

                availability.payNowTotalPrice = Decimal.Round(priceCalculator.Prices.payNowPrice, 2);
            }
        }
        private void pricesCouldntCalculated(AvailabilityData availability)
        {
            availability.isPriceCalculatedSafely = false;
            availability.hasError = true;
            availability.priceErrorMessage = "PricesCouldntCalculated";
            availability.totalPrice = decimal.MinValue;
            availability.payLaterTotalPrice = decimal.MinValue;
            availability.payNowTotalPrice = decimal.MinValue;
        }
        private List<AvailableEquipments> collectEquipments()
        {
            List<Task> _tasks = new List<Task>();
            List<EquipmentDataMongoDB> currentBranchs = new List<EquipmentDataMongoDB>();
            List<ReservationItemDataMongoDB> reservations = new List<ReservationItemDataMongoDB>();
            List<ContractItemDataMongoDB> contracts = new List<ContractItemDataMongoDB>();
            List<TransferDataMongoDB> transfers = new List<TransferDataMongoDB>();
            //first collect equipments from current branch
            #region Get Current Branches
            var currentBranchsTask = new Task(() =>
            {
                EquipmentRepository equipmentRepository = new EquipmentRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                              StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                currentBranchs = equipmentRepository.getCurrentEquipments(availabilityParameters.pickupBranchId);
            });
            _tasks.Add(currentBranchsTask);
            currentBranchsTask.Start();

            var reservationsTask = new Task(() =>
            {
                ReservationItemRepository reservationItemRepository = new ReservationItemRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                    StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                reservations = reservationItemRepository.getAvailableReservations(availabilityParameters.pickupDateTime,
                                                                                    availabilityParameters.pickupBranchId);
            });
            _tasks.Add(reservationsTask);
            reservationsTask.Start();

            var contractTask = new Task(() =>
            {
                ContractItemRepository contractItemRepository = new ContractItemRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                           StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                contracts = contractItemRepository.getAvailableContracts(availabilityParameters.pickupDateTime,
                                                                             availabilityParameters.pickupBranchId);
            });
            _tasks.Add(contractTask);
            contractTask.Start();

            var transferTask = new Task(() =>
            {
                TransferRepository transferRepository = new TransferRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                               StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                transfers = transferRepository.getAvailableTransfers(availabilityParameters.pickupDateTime,
                                                                     availabilityParameters.pickupBranchId);
            });
            _tasks.Add(transferTask);
            transferTask.Start();

            Task.WaitAll(_tasks.ToArray());
            //add them into the list(current branches)
            var equipments = currentBranchs.Select(p =>
               new AvailableEquipments
               {
                   equipmentId = p.EquipmentId,
                   equipmentName = p.Name,
                   groupCodeInfoId = p.GroupCodeInformationId,
                   groupCodeInfoName = p.GroupCodeInformationName
               }).ToList();
            #endregion

            // reservations
            var equipmentsByReservations = reservations.Select(p =>
            new AvailableEquipments
            {
                equipmentId = p.EquipmentId,
                equipmentName = p.EquipmentName,
                groupCodeInfoId = p.GroupCodeInformationId,
                groupCodeInfoName = p.GroupCodeInformationName
            }).ToList();

            equipments.AddRange(equipmentsByReservations);

            //contracts
            var equipmentsByContracts = contracts.Select(p =>
            new AvailableEquipments
            {
                equipmentId = p.equipmentId,
                equipmentName = p.equipmentName,
                groupCodeInfoId = p.groupCodeInformationsId,
                groupCodeInfoName = p.groupCodeInformationsName
            }).ToList();

            equipments.AddRange(equipmentsByContracts);

            //transfers 
            var equipmentsByTransfers = transfers.Select(p =>
            new AvailableEquipments
            {
                equipmentId = p.equipmentId,
                equipmentName = p.equipmentName,
                groupCodeInfoId = p.groupCodeId,
                groupCodeInfoName = p.groupCodeName
            }).ToList();

            equipments.AddRange(equipmentsByTransfers);

            return equipments;
        }
        private ExcludeAvailability calculateExcludeItems(List<String> branchGroupCodes)
        {
            List<ReservationItemDataMongoDB> excludedReservations = new List<ReservationItemDataMongoDB>();
            List<ContractItemDataMongoDB> excludedContracts = new List<ContractItemDataMongoDB>();
            List<TransferDataMongoDB> excludedTransfers = new List<TransferDataMongoDB>();

            List<Task> _tasks = new List<Task>();

            var reservationTask = new Task(() =>
            {
                excludedReservations.AddRange(getExcludedReservations());
            });
            _tasks.Add(reservationTask);
            reservationTask.Start();

            var contractTask = new Task(() =>
            {
                excludedContracts.AddRange(getExcludedContracts());
            });
            _tasks.Add(contractTask);
            contractTask.Start();

            var transferTask = new Task(() =>
            {
                excludedTransfers.AddRange(getExcludedTransfers());
            });
            _tasks.Add(transferTask);
            transferTask.Start();

            Task.WaitAll(_tasks.ToArray());
            return this.calculateExcludedItems(branchGroupCodes, excludedReservations, excludedContracts, excludedTransfers);
        }
        private List<ReservationItemDataMongoDB> getExcludedReservations()
        {
            ReservationItemRepository reservationItemRepository = new ReservationItemRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                               StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            return reservationItemRepository.getExcludedReservations(availabilityParameters.pickupDateTime,
                                                                                     availabilityParameters.dropoffDateTime,
                                                                                     availabilityParameters.pickupBranchId);
        }
        private List<ContractItemDataMongoDB> getExcludedContracts()
        {
            ContractItemRepository contractItemRepository = new ContractItemRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                       StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            return contractItemRepository.getExcludedContracts(availabilityParameters.pickupDateTime,
                                                                                availabilityParameters.dropoffDateTime,
                                                                                availabilityParameters.pickupBranchId);
        }
        private List<TransferDataMongoDB> getExcludedTransfers()
        {
            TransferRepository transferRepository = new TransferRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                           StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            return transferRepository.getExcludeTransfers(availabilityParameters.pickupDateTime,
                                                          availabilityParameters.dropoffDateTime,
                                                          availabilityParameters.pickupBranchId);
        }
        private List<AvailabilityData> evaluateReturnObject(List<Results> results, ExcludeAvailability excludeItems)
        {
            List<AvailabilityData> response = new List<AvailabilityData>();

            var pickupBranchId = this.availabilityParameters.pickupBranchId.ToString().ToLower().Replace("{", "").Replace("}", "");

            foreach (var item in results)
            {
                //AvailabilityFactorBusiness availabilityFactorBusiness = new AvailabilityFactorBusiness(StaticHelper.GetConfiguration("MongoDBHostName"),
                //                                                                                       StaticHelper.GetConfiguration("MongoDBDatabaseName"));

                var relatedGroupCodeId = item.id.ToLower().Replace("{", "").Replace("}", "");

                var c = 0;
                if (this.availabilityFactorIncreaseCapacity.Count > 0)
                {

                    foreach (var capacity in availabilityFactorIncreaseCapacity)
                    {
                        var groupCodeIsInList = capacity.groupCodeValues?.Replace("]", "").Replace("[", "").Replace("\"", "")
                                                .Contains(relatedGroupCodeId);
                        var branchIsInList = capacity.branchValues?.Replace("]", "").Replace("[", "").Replace("\"", "")
                                                    .Contains(pickupBranchId);

                        if (groupCodeIsInList.HasValue && groupCodeIsInList.Value &&
                            branchIsInList.HasValue && branchIsInList.Value)
                        {
                            c += capacity.capacityRatio;
                        }
                    }

                }

                decimal excludeAmount = 0;
                if (excludeItems.availabilityResults.TryGetValue(item.id, out excludeAmount))
                {
                    AvailabilityData availabilityResponse = new AvailabilityData();
                    availabilityResponse.groupCodeInformationName = item.name;
                    availabilityResponse.groupCodeInformationId = new Guid(item.id);
                    availabilityResponse.equipmentCount = item.count + c;
                    var afterExcludetotalEquipmentcount = availabilityResponse.equipmentCount - excludeAmount;
                    if (availabilityResponse.equipmentCount == 0)
                    {
                        availabilityResponse.ratio = decimal.Zero;
                    }
                    else
                    {
                        availabilityResponse.ratio = decimal.Round(100 - (100 * afterExcludetotalEquipmentcount / availabilityResponse.equipmentCount));
                    }

                    response.Add(availabilityResponse);

                }
                else
                {
                    AvailabilityData availabilityResponse = new AvailabilityData();
                    availabilityResponse.groupCodeInformationName = item.name;
                    availabilityResponse.groupCodeInformationId = new Guid(item.id);
                    availabilityResponse.equipmentCount = item.count + c;
                    availabilityResponse.ratio = 0;
                    response.Add(availabilityResponse);
                }

            }
            return response;
        }
        private void groupCodeClosureByAvailabilityFactorError(AvailabilityData availabilityData)
        {
            availabilityData.hasError = true;
            // set xml tag in errorMessage
            // errorMessage will change in action from errormessages xml
            availabilityData.errorMessage = "SelectedGroupCodeClosure";
        }
        private decimal calculatePriceForShortenReservation(AvailabilityData availability, Dictionary<DateTime, decimal> dayAvailibilityResult)
        {
            //shorten reservation rules -->
            //price recalculate due to pickup and dropoffdate respectively.
            return calculateStandartPrice(availability, dayAvailibilityResult);
        }
        private void createPriceCalculationSummary(PriceResponse priceResult, Guid groupCodeId)
        {
            var selectedDocumentDataRelatedGroupCodeId = this.selectedDocumentData == null ?
                                                         Guid.Empty :
                                                         new Guid(this.selectedDocumentData.groupCodeInformationId);
            PriceCalculationSummariesBusiness priceCalculationSummaries = new PriceCalculationSummariesBusiness(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                                StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            priceCalculationSummaries.createPriceCalculationSummary(priceResult, this.trackingNumber);

        }

        private decimal monthOperations(AvailabilityData availability, List<PaymentPlanData> paymentPlanDatas)
        {
            decimal price = decimal.Zero;
            var priceResponse = new PriceResponse();
            var dayCount = (this.availabilityParameters.pickupDateTime.AddMinutes(StaticHelper.offset).AddDays(30) -
                            this.availabilityParameters.pickupDateTime.AddMinutes(StaticHelper.offset)).TotalDays;


            PriceBusiness priceBusiness = new PriceBusiness();
            priceResponse = priceBusiness.calculatePriceByMonthly(availability.groupCodeInformationId,
                                                                  availabilityParameters.pickupBranchId.ToString(),
                                                                  this.availabilityParameters.pickupDateTime.AddMinutes(StaticHelper.offset),
                                                                  (int)this.availabilityParameters.customerType,
                                                                  this.selectedDocumentData,
                                                                  this.availabilityParameters.corporateType,
                                                                  this.availabilityParameters.processIndividualPrices_broker,
                                                                  this.availabilityParameters.accountGroup,
                                                                  this.availabilityParameters.priceCodeId,
                                                                  Convert.ToInt32(dayCount),
                                                                  paymentPlanDatas);
            if (priceResponse.totalAmount == decimal.MinValue)
            {
                return price;
            }

            for (DateTime i = this.availabilityParameters.pickupDateTime.AddMinutes(StaticHelper.offset);
                 i < this.availabilityParameters.pickupDateTime.AddMinutes(StaticHelper.offset).AddDays(30);
                 i += TimeSpan.FromDays(1))
            {
                price += priceResponse.totalAmount;
                if (this.selectedDocumentData != null)
                {
                    availability.currencyCode = decideCurrencyExistingDocument().currencyCode;
                    availability.currencyId = decideCurrencyExistingDocument().currencyId;
                }
                else
                {
                    availability.currencyCode = priceResponse.currencyCode;
                    availability.currencyId = priceResponse.currencyId;
                }
                priceResponse.relatedDay = i.AddMinutes(-StaticHelper.offset);

                createPriceCalculationSummary(priceResponse, availability.groupCodeInformationId);

            }


            var planDataResponse = generatePaymentPlan(availability.groupCodeInformationId, paymentPlanDatas, this.availabilityParameters.priceCodeId);
            if (planDataResponse.price == decimal.MinValue)
            {
                return decimal.Zero;
            }
            availability.paymentPlanData = planDataResponse.paymentPlan;
            return (decimal)planDataResponse.price;
        }
        private PaymentPlanResponse generatePaymentPlan(Guid groupCodeInformationId, List<PaymentPlanData> paymentPlanDatas, Guid priceCodeId)
        {
            var checkPrice = decimal.MinValue;
            PriceBusiness priceBusiness = new PriceBusiness();
            //now buiild the payment data
            PriceListDataMongoDB individualPriceList = null;
            if (priceCodeId == Guid.Empty)
            {
                individualPriceList = priceBusiness.getIndividualMonthlyPriceList();
            }
            else
            {
                individualPriceList = priceBusiness.getCorporateMonthlyPriceList(priceCodeId);
            }

            List<PaymentPlanData> paymentPlanData = new List<PaymentPlanData>();
            var pickupdateTime = this.selectedDocumentData != null ? this.selectedDocumentData.pickupDateTime_Header : this.availabilityParameters.pickupDateTime.AddMinutes(StaticHelper.offset);
            //sözleşme güncelleme hep 1 aylıktır
            if (this.availabilityParameters.operationType == 100)
            {
                pickupdateTime = this.availabilityParameters.pickupDateTime.AddMinutes(StaticHelper.offset);
            }
            var j = 0;
            for (int i = pickupdateTime.Value.Month;
                i < pickupdateTime.Value.Month + this.availabilityParameters.monthValue;
                i++)
            {
                var m = (i) % 12 == 0 ? i : (i) % 12;
                var relatedPlan = paymentPlanDatas.Where(p => p.month == m).FirstOrDefault();
                if (relatedPlan == null)
                {
                    var monthlyPrice = priceBusiness.getMonthlyRelatedGroupCodeList(individualPriceList.PriceListId,
                                                                               groupCodeInformationId,
                                                                               pickupdateTime.Value.AddMinutes(StaticHelper.offset).AddDays(30 * j));

                    if (monthlyPrice?.ListPrice == null || monthlyPrice?.ListPrice == decimal.MinValue)
                    {
                        checkPrice = decimal.MinValue;
                        break;
                    }
                    else
                    {
                        checkPrice = decimal.Zero;
                    }
                    PriceCalculator priceCalculator = new PriceCalculator(new Guid(individualPriceList.PriceListId), string.Empty);
                    priceCalculator.applyPayMethodMonthly(monthlyPrice.ListPrice,
                                                          Convert.ToString(this.availabilityParameters.pickupBranchId),
                                                          Convert.ToString(groupCodeInformationId),
                                                          this.availabilityParameters.customerType,
                                                          this.availabilityParameters.accountGroup);

                    paymentPlanData.Add(new PaymentPlanData
                    {
                        priceListName = individualPriceList.PriceListId,
                        priceListId = new Guid(individualPriceList.PriceListId),
                        groupCodeId = groupCodeInformationId,
                        month = m,
                        payLaterAmount = monthlyPrice.ListPrice,
                        payNowAmount = priceCalculator.Prices.payNowPrice
                    });
                }
                else
                {
                    checkPrice = decimal.Zero;
                    paymentPlanData.Add(new PaymentPlanData
                    {
                        priceListName = relatedPlan.priceListName,
                        priceListId = relatedPlan.priceListId,
                        groupCodeId = groupCodeInformationId,
                        month = m,
                        payLaterAmount = this.selectedDocumentData.paymentChoice == 10 ? relatedPlan.payNowAmount : relatedPlan.payLaterAmount,
                        payNowAmount = this.selectedDocumentData.paymentChoice == 10 ? relatedPlan.payNowAmount : relatedPlan.payLaterAmount
                    });
                }
                j++;

            }

            return new PaymentPlanResponse
            {
                price = checkPrice == decimal.MinValue ? checkPrice :
                        this.isExistingDocument() ? this.selectedDocumentData.paymentChoice == 10 ? paymentPlanData.FirstOrDefault()?.payNowAmount : paymentPlanData.FirstOrDefault()?.payLaterAmount :
                        paymentPlanData.FirstOrDefault()?.payLaterAmount,
                paymentPlan = paymentPlanData
            };
        }
        private decimal calculateStandartPrice(AvailabilityData availability, Dictionary<DateTime, decimal> dayAvailibilityResult)
        {
            var price = decimal.Zero;
            PriceBusiness priceBusiness = new PriceBusiness(availabilityParameters.pickupDateTime, availabilityParameters.dropoffDateTime);

            foreach (var day in dayAvailibilityResult)
            {

                //aylık olmayanlarda
                if (!availabilityParameters.isMonthly)
                {
                    var priceResponse = new PriceResponse();
                    priceResponse = priceBusiness.calculatePriceByGivenDay(availability.groupCodeInformationId,
                                                                           availability.equipmentCount,
                                                                           day.Key,
                                                                           Convert.ToInt32(day.Value),
                                                                           this.availabilityParameters.channel,
                                                                           this.availabilityParameters.segment,
                                                                           this.selectedDocumentData,
                                                                           Convert.ToString(this.availabilityParameters.pickupBranchId),
                                                                           (int)this.availabilityParameters.customerType,
                                                                           Convert.ToString(this.availabilityParameters.priceCodeId),
                                                                           this.availabilityParameters.corporateType,
                                                                           this.availabilityParameters.processIndividualPrices_broker,
                                                                           this.availabilityParameters.exchangeRate,
                                                                           this.availabilityParameters.accountGroup,
                                                                           this.availabilityParameters.operationType.HasValue ? this.availabilityParameters.operationType.Value : 0);



                    if (priceResponse.totalAmount == decimal.MinValue)
                    {
                        price = decimal.MinValue;
                        break;
                    }
                    this.applyCampaignPricesforExistingDocument(priceResponse);
                    price += priceResponse.totalAmount;
                    if (this.selectedDocumentData != null)
                    {
                        availability.currencyCode = decideCurrencyExistingDocument().currencyCode;
                        availability.currencyId = decideCurrencyExistingDocument().currencyId;
                    }
                    else
                    {
                        availability.currencyCode = priceResponse.currencyCode;
                        availability.currencyId = priceResponse.currencyId;
                    }

                    createPriceCalculationSummary(priceResponse, availability.groupCodeInformationId);
                }
                //aylıklarda
                else
                {
                    price = monthOperations(availability, new List<PaymentPlanData>());
                    if (price == decimal.Zero)
                    {
                        price = decimal.MinValue;
                    }
                    break;
                }
            }

            return price;
        }
        private decimal calculatePriceForExtendedReservation(AvailabilityData availability,
                                                            Dictionary<DateTime, decimal> dayAvailibilityResult)
        {
            List<DailyPriceDataMongoDB> intersectDays = new List<DailyPriceDataMongoDB>();

            List<PriceResponse> newDays = new List<PriceResponse>();

            var price = decimal.Zero;
            var dailyPrices = this.getDailyPricesforSelectedDocument();
            var dayDiff = Convert.ToInt32(CommonHelper.calculateTotalDurationInDays(this.availabilityParameters.pickupDateTime.AddMinutes(StaticHelper.offset),
                                                                                    this.availabilityParameters.dropoffDateTime.AddMinutes(StaticHelper.offset)));

            var counter = 0;
            for (DateTime i = this.availabilityParameters.pickupDateTime.AddMinutes(StaticHelper.offset); i < this.availabilityParameters.dropoffDateTime.AddMinutes(StaticHelper.offset); i += TimeSpan.FromDays(1))
            {

                var intersectDatePrice = dailyPrices.Where(p => p.priceDate.AddMinutes(StaticHelper.offset).Date.Equals(i.Date)).FirstOrDefault();


                //means we found the intersect price and need to calculate from the old price
                if (intersectDatePrice != null && !intersectDatePrice.isTickDay)
                {
                    if (dayDiff != counter)
                    {
                        price += intersectDatePrice.totalAmount;
                    }
                    //this means last item in the list
                    else
                    {
                        if (this.availabilityParameters.dropoffDateTime.AddMinutes(StaticHelper.offset) !=
                            this.selectedDocumentData.dropoffTimeStamp.Value.converttoDateTime().AddMinutes(StaticHelper.offset))
                        {
                            PriceHourEffectRepository priceHourEffectRepository = new PriceHourEffectRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                                StaticHelper.GetConfiguration("MongoDBDatabaseName"));

                            var totalMinutes = CommonHelper.calculateTotalDurationInMinutes(this.availabilityParameters.pickupDateTime, this.availabilityParameters.dropoffDateTime);
                            var tickMinute = totalMinutes % 1440;

                            if (tickMinute < 0)
                            {
                                tickMinute = tickMinute * -1;
                            }
                            var priceHourEffect = priceHourEffectRepository.getPriceHourEffectByDuration(tickMinute);
                            var tickPrice = decimal.Zero;

                            if (selectedDocumentData?.paymentChoice == (int)PaymentEnums.PaymentType.PayNow)
                            {
                                tickPrice = (priceHourEffect.effectRate * intersectDatePrice.payNowWithoutTickDayAmount) / 100;
                                intersectDatePrice.payNowAmount = tickPrice;
                            }
                            else if (selectedDocumentData?.paymentChoice == (int)PaymentEnums.PaymentType.PayLater)
                            {
                                if ((selectedDocumentData?.documentType == (int)rnt_ReservationTypeCode.Broker ||
                                    selectedDocumentData?.documentType == (int)rnt_ReservationTypeCode.Acente) &&
                                    selectedDocumentData.processIndividualPrices)
                                {
                                    tickPrice = (priceHourEffect.effectRate * intersectDatePrice.payNowWithoutTickDayAmount) / 100;
                                    intersectDatePrice.payNowAmount = tickPrice;
                                }
                                else
                                {
                                    tickPrice = (priceHourEffect.effectRate * intersectDatePrice.payLaterWithoutTickDayAmount) / 100;
                                    intersectDatePrice.payLaterAmount = tickPrice;

                                }

                            }
                            price += tickPrice;
                            intersectDatePrice.totalAmount = tickPrice;
                            intersectDatePrice.isTickDay = true;
                        }
                        else
                        {
                            price += intersectDatePrice.totalAmount;
                        }

                    }

                    intersectDays.Add(intersectDatePrice);
                }
                else
                {
                    Dictionary<DateTime, decimal> tempDayAvailibilityResult = new Dictionary<DateTime, decimal>();
                    foreach (var item in dayAvailibilityResult)
                    {
                        var newKey = item.Key.AddMinutes(StaticHelper.offset);

                        var d = item.Value;
                        tempDayAvailibilityResult.Add(newKey, d);
                    }

                    var newDayAvailability = tempDayAvailibilityResult.Where(p => p.Key.Date.Equals(i.Date)).ToList();
                    if (newDayAvailability.Count != 1)
                    {
                        // error finding new prices so system should stop everything
                        price = decimal.MinValue;
                        break;
                    }
                    var priceResponse = new PriceResponse();
                    PriceBusiness priceBusiness = new PriceBusiness(this.availabilityParameters.pickupDateTime, this.availabilityParameters.dropoffDateTime);
                    //aylıklarda 32 günse 2 gün gibi hesaplaması için
                    if (_totalMinutes != 0)
                    {
                        priceBusiness.totalDuration = Convert.ToInt32(_totalMinutes);
                    }
                    priceResponse = priceBusiness.calculatePriceByGivenDay(availability.groupCodeInformationId,
                                                                           availability.equipmentCount,
                                                                           i.AddMinutes(-StaticHelper.offset),
                                                                           (int)newDayAvailability.FirstOrDefault().Value,
                                                                           this.availabilityParameters.channel,
                                                                           this.availabilityParameters.segment,
                                                                           this.selectedDocumentData,
                                                                           Convert.ToString(this.availabilityParameters.pickupBranchId),
                                                                           (int)this.availabilityParameters.customerType,
                                                                           Convert.ToString(this.availabilityParameters.priceCodeId),
                                                                           this.availabilityParameters.corporateType,
                                                                           this.availabilityParameters.processIndividualPrices_broker,
                                                                           this.availabilityParameters.exchangeRate,
                                                                           this.availabilityParameters.accountGroup,
                                                                           this.availabilityParameters.operationType.HasValue ? this.availabilityParameters.operationType.Value : 0);


                    if (priceResponse.totalAmount == decimal.MinValue)
                    {
                        price = decimal.MinValue;
                        break;
                    }

                    this.applyCampaignPricesforExistingDocument(priceResponse);


                    price += priceResponse.totalAmount;
                    newDays.Add(priceResponse);
                }
                counter++;
            }
            //todo will check working properly with different cases
            if (price != decimal.MinValue || price != decimal.Zero)
            {
                //first collect old price calculation summaries by groupcode and trackingnumber
                //PriceCalculationSummariesRepository priceCalculationSummariesRepository = new PriceCalculationSummariesRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                //                                                                                                                  StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                //var oldPriceCalculationSummary = this.getOldPriceCalculationSummaries(availability);
                //lets create price calculation summaries safely--> prices calculated in a proper way
                foreach (var item in intersectDays)
                {

                    var relatedoldPriceCalculationSummary = dailyPrices.Where(p => p.priceDate.Date.Equals(item.priceDate.Date)).FirstOrDefault();
                    //pay brokerda tracking number eski kalmalı.
                    //uzamalarda eski kalem completed olup , yerine , yeni kalem acılacak.
                    relatedoldPriceCalculationSummary.trackingNumber = this.trackingNumber;
                    relatedoldPriceCalculationSummary._id = ObjectId.GenerateNewId();

                    //if (this.selectedDocumentData?.paymentMethod != (int)rnt_PaymentMethodCode.PayBroker)
                    //{
                    if (this.isExistingDocument())
                    {

                        //var diff = (this.availabilityParameters.pickupDateTime - this.selectedDocumentData.pickupTimeStamp.Value.converttoDateTime()).TotalMinutes;
                        ////18-19 sözleşme tarihi
                        ////19-20 cekildi. bu kontrol olmadan önce aynı güne 2 kayıt atıyordu.
                        //if (item.priceDate != this.availabilityParameters.pickupDateTime)
                        //{
                        relatedoldPriceCalculationSummary.priceDate = relatedoldPriceCalculationSummary.priceDate.AddHours(-relatedoldPriceCalculationSummary.priceDate.Hour).AddHours(this.availabilityParameters.pickupDateTime.Hour);
                        relatedoldPriceCalculationSummary.priceDate = relatedoldPriceCalculationSummary.priceDate.AddMinutes(-relatedoldPriceCalculationSummary.priceDate.Minute).AddMinutes(availabilityParameters.pickupDateTime.Minute);

                        relatedoldPriceCalculationSummary.priceDateTimeStamp = new BsonTimestamp(relatedoldPriceCalculationSummary.priceDate.converttoTimeStamp());
                        //}

                    }
                    else
                    {
                        relatedoldPriceCalculationSummary.priceDate = item.priceDate;
                        relatedoldPriceCalculationSummary.priceDateTimeStamp = item.priceDateTimeStamp;
                    }

                    relatedoldPriceCalculationSummary.totalAmount = item.totalAmount;

                    PriceCalculationSummariesBusiness priceCalculationSummariesBusiness = new PriceCalculationSummariesBusiness(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                                                StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                    priceCalculationSummariesBusiness.createPriceCalculationSummaryFromDailyPrices(relatedoldPriceCalculationSummary);
                    // }
                }
                //add new prices to price calculation summaries
                foreach (var item in newDays)
                {
                    createPriceCalculationSummary(item, availability.groupCodeInformationId);
                }
            }

            return price;
        }
        private void availabilityClosureByAvailabilityFactor(ref List<string> branchGroupCodes, ref List<Results> results, bool excludeReservation)
        {
            AvailabilityFactorBusiness availabilityFactorBusiness = new AvailabilityFactorBusiness(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                  StaticHelper.GetConfiguration("MongoDBDatabaseName"));

            var excludedGroupCodes = availabilityFactorBusiness.applyAvailabilityClosure(this.availabilityParameters.pickupDateTime,
                                                                                         this.availabilityParameters.dropoffDateTime,
                                                                                         Convert.ToString(this.availabilityParameters.pickupBranchId),
                                                                                         this.availabilityParameters.channel,
                                                                                         this.availabilityParameters.customerType,
                                                                                         this.availabilityParameters.accountGroup);
            //remove related group codes lists
            if (!excludeReservation && this.selectedDocumentData != null)
            {
                excludedGroupCodes.Remove(new Guid(this.selectedDocumentData.groupCodeInformationId));
            }
            branchGroupCodes = branchGroupCodes.Where(p => !excludedGroupCodes.Any(l => new Guid(p) == l)).ToList();
            //results = results.Where(p => !excludedGroupCodes.Any(l => new Guid(p.id) == l)).ToList();
        }
        private void decideUpdateReservationType()
        {
            var _selectDocumentDataPickupDateTime = this.selectedDocumentData?.pickupDateTime.Value.AddSeconds(-this.selectedDocumentData.pickupDateTime.Value.Second);
            var _selectDocumentDataDropoffDateTime = this.selectedDocumentData?.dropoffDateTime.Value.AddSeconds(-this.selectedDocumentData.dropoffDateTime.Value.Second);

            availabilityParameters.dropoffDateTime = availabilityParameters.dropoffDateTime.AddSeconds(-availabilityParameters.dropoffDateTime.Second);
            availabilityParameters.pickupDateTime = availabilityParameters.pickupDateTime.AddSeconds(-availabilityParameters.pickupDateTime.Second);

            if (_selectDocumentDataDropoffDateTime == availabilityParameters.dropoffDateTime &&
                _selectDocumentDataPickupDateTime == availabilityParameters.pickupDateTime)
            {
                this.documentUpdateType = DocumentUpdateType.notChanged;
            }
            else
            {
                var totalMinutesForReservation = CommonHelper.calculateTotalDurationInMinutes(this.selectedDocumentData.pickupTimeStamp.Value,
                                                                                              this.selectedDocumentData.dropoffTimeStamp.Value);

                var parametersTotalDuration = CommonHelper.calculateTotalDurationInMinutes(availabilityParameters.pickupDateTime,
                                                                                           availabilityParameters.dropoffDateTime);

                if (parametersTotalDuration < totalMinutesForReservation)
                {
                    this.documentUpdateType = DocumentUpdateType.shorten;
                }
                else if (parametersTotalDuration > totalMinutesForReservation)
                {
                    this.documentUpdateType = DocumentUpdateType.extended;
                }
                else if (parametersTotalDuration == totalMinutesForReservation)
                {
                    //if shifing duration is bigger than the parameter than we need to charge
                    var dropoffDifference = CommonHelper.calculateTotalDurationInMinutes(this.selectedDocumentData.dropoffTimeStamp.Value.converttoDateTime(),
                                                                                          this.availabilityParameters.dropoffDateTime);

                    dropoffDifference = dropoffDifference < 0 ? dropoffDifference * -1 : dropoffDifference;

                    var pickupDifference = CommonHelper.calculateTotalDurationInMinutes(this.selectedDocumentData.pickupTimeStamp.Value.converttoDateTime(),
                                                                 this.availabilityParameters.pickupDateTime);

                    pickupDifference = pickupDifference < 0 ? pickupDifference * -1 : pickupDifference;

                    if (pickupDifference <= this.availabilityParameters.shiftDuration &&
                          dropoffDifference <= this.availabilityParameters.shiftDuration)
                    {
                        this.documentUpdateType = DocumentUpdateType.shiftedWithAllowedDuration;
                        shiftDifference = Convert.ToInt32(pickupDifference);
                    }
                    else
                    {
                        this.documentUpdateType = DocumentUpdateType.shiftedWithNotAllowedDuration;
                    }
                }
            }

        }
        private List<String> prepareSelectedDocumentData(List<String> branchGroupCodes)
        {

            if (!string.IsNullOrEmpty(availabilityParameters.reservationId))
            {
                this.selectedDocumentData = this.getReservationItemByReservationId();
            }
            else if (!string.IsNullOrEmpty(availabilityParameters.contractId))
            {
                this.selectedDocumentData = this.getcontractItemByContractId();

            }
            if (!this.availabilityParameters.isMonthly || (this.selectedDocumentData != null && !this.selectedDocumentData.isMonthly))
            {
                if (this.selectedDocumentData != null)
                {
                    ReservationItemRepository reservationItemRepository = new RntCar.MongoDBHelper.Repository.ReservationItemRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                                                      StaticHelper.GetConfiguration("MongoDBDatabaseName"));

                    var reservation = reservationItemRepository.getReservationByPnrNumber(this.selectedDocumentData.pnrNumber);
                    this.availabilityParameters.channel = reservation.ReservationChannel;

                    this.decideUpdateReservationType();

                    ContractItemRepository contractItemRepository = new ContractItemRepository(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                    var contractItems = contractItemRepository.getContractItemsEquipment(this.selectedDocumentData.documentId);

                    //araç değişikliği yoksa tekrar kontrol edelim

                    var completedItems = contractItems.Where(p => p.statuscode == (int)rnt_contractitem_StatusCode.Completed).ToList();
                    if (completedItems.Count == 0)
                    {
                        //eğer gün değişmiyorsa not changed yap
                        DurationHelper durationHelper = new DurationHelper(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                           StaticHelper.GetConfiguration("MongoDBDatabaseName"));

                        var paramDuration = durationHelper.calculateDocumentDurationByPriceHourEffect_Decimal(this.availabilityParameters.pickupDateTime, this.availabilityParameters.dropoffDateTime);
                        var documentDuration = durationHelper.calculateDocumentDurationByPriceHourEffect_Decimal(this.selectedDocumentData.pickupDateTime.Value, this.selectedDocumentData.dropoffDateTime.Value);
                        if (paramDuration == documentDuration)
                        {
                            this.documentUpdateType = DocumentUpdateType.notChanged;
                        }
                    }
                    else
                    {
                        var deliveredItems = contractItems.Where(p => p.statuscode == (int)rnt_contractitem_StatusCode.WaitingForDelivery).ToList();
                        var rentalItems = contractItems.Where(p => p.statuscode == (int)rnt_contractitem_StatusCode.Rental).ToList();

                        List<ContractItemDataMongoDB> allItems = new List<ContractItemDataMongoDB>();
                        allItems.AddRange(deliveredItems);
                        allItems.AddRange(rentalItems);
                        allItems.AddRange(completedItems);

                        var startPickUpDateTime = allItems.Min(p => p.pickupDateTime).Value;
                        var endDropOffTime = allItems.Max(p => p.dropoffDateTime).Value;

                        //araç değişimi varsa 
                        //ve 1.25 günden 1 güne yada 1.5 günden 1. güne düşüyorsa fiyat hesaplama
                        DurationHelper durationHelper = new DurationHelper(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                          StaticHelper.GetConfiguration("MongoDBDatabaseName"));

                        var paramDuration = durationHelper.calculateDocumentDurationByPriceHourEffect_Decimal(startPickUpDateTime, this.availabilityParameters.dropoffDateTime);
                        var documentDuration = durationHelper.calculateDocumentDurationByPriceHourEffect_Decimal(startPickUpDateTime, endDropOffTime);

                        var diff = documentDuration - paramDuration;

                        if (diff > 0 && diff < 1)
                        {
                            if (documentDuration % 1 != 0 && paramDuration % 1 == 0)
                            {
                                this.documentUpdateType = DocumentUpdateType.notChanged;
                            }
                        }
                        else if (diff == 0)
                        {
                            this.documentUpdateType = DocumentUpdateType.notChanged;
                        }
                    }

                }
                if (this.documentUpdateType == DocumentUpdateType.notChanged ||
                    this.documentUpdateType == DocumentUpdateType.shiftedWithAllowedDuration)
                {
                    //remove group code
                    // so availibility will not calculate for selected groupCode
                    branchGroupCodes.Remove(this.selectedDocumentData.groupCodeInformationName.ToLower());
                    //if nothing is changed keep the same tracking number 
                    //this.trackingNumber = this.selectedDocumentData.trackingNumber;
                }
            }
            else
            {
                this.documentUpdateType = DocumentUpdateType.monthly;
                if (this.isExistingDocument())
                    branchGroupCodes.Remove(this.selectedDocumentData.pricingGroupCodeName.ToLower());
            }


            return branchGroupCodes;
        }
        private List<PaymentPlanData> checkMonthlyDuration(List<PaymentPlanData> paymentPlanDatas, Guid priceCodeId)
        {
            var queryPlans = this.generatePaymentPlan(new Guid(this.selectedDocumentData.groupCodeInformationId), paymentPlanDatas, priceCodeId);
            return this.selectedDocumentData.paymentPlans
                  .Except(queryPlans.paymentPlan, new PaymentPlanDataComparer()).ToList();
        }
        private void prepareParameters()
        {
            availabilityParameters.dropoffDateTime = availabilityParameters.dropoffDateTime.AddMilliseconds(-availabilityParameters.dropoffDateTime.Millisecond);
            availabilityParameters.pickupDateTime = availabilityParameters.pickupDateTime.AddMilliseconds(-availabilityParameters.pickupDateTime.Millisecond);
            availabilityParameters.pickupDateTime = availabilityParameters.pickupDateTime.AddSeconds(-availabilityParameters.pickupDateTime.Second);
            availabilityParameters.dropoffDateTime = availabilityParameters.dropoffDateTime.AddSeconds(-availabilityParameters.dropoffDateTime.Second);
        }
        private DocumentData getcontractItemByContractId()
        {
            ContractItemRepository contractItemRepository = new ContractItemRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                          StaticHelper.GetConfiguration("MongoDBDatabaseName"));

            var contract = contractItemRepository.getContractItemEquipment(availabilityParameters.contractId);

            return new DocumentData
            {
                isMonthly = contract.isMonthly,
                paymentPlans = contract.paymentPlans,
                monthValue = contract.monthValue,
                changeReason = contract.changeReason,
                documentChannel = contract.contractChannel,
                customerId = contract.customerId,
                customerName = contract.customerName,
                depositAmount = contract.depositAmount,
                documentId = contract.contractId,
                documentItemId = contract.contractItemId,
                documentNumber = contract.contractNumber,
                documentType = contract.contractType,
                dropoffBranchId = contract.dropoffBranchId,
                dropoffBranchName = contract.dropoffBranchName,
                dropoffDateTime = contract.dropoffDateTime,
                dropoffTimeStamp = contract.DropoffTimeStamp,
                equipmentId = contract.equipmentId,
                equipmentName = contract.equipmentName,
                groupCodeInformationId = contract.groupCodeInformationsId,
                groupCodeInformationName = contract.groupCodeInformationsName,
                pricingGroupCodeId = contract.pricingGroupCodeId,
                pricingGroupCodeName = contract.pricingGroupCodeName,
                itemTypeCode = contract.itemTypeCode,
                netAmount = contract.netAmount,
                offset = StaticHelper.offset,
                ownerId = contract.ownerId,
                ownerName = contract.ownerName,
                paymentChoice = contract.paymentChoice,
                pickupBranchId = contract.pickupBranchId,
                pickupBranchName = contract.pickupBranchName,
                pickupDateTime = contract.pickupDateTime,
                pickupTimeStamp = contract.PickupTimeStamp,
                pnrNumber = contract.pnrNumber,
                statecode = contract.statecode,
                statuscode = contract.statuscode,
                taxRatio = contract.taxRatio,
                totalAmount = contract.totalAmount,
                taxAmount = contract.totalAmount - contract.netAmount,
                trackingNumber = contract.trackingNumber,
                transactioncurrencyid = contract.transactioncurrencyid,
                _id = contract._id,
                type = DocumentType.contract,
                campaignId = contract.campaignId,
                paymentMethod = contract.paymentMethod,
                pickupDateTime_Header = contract.pickupDateTime_Header,
                processIndividualPrices = contract.processIndividualPrices,
            };
        }
        private DocumentData getReservationItemByReservationId()
        {
            ReservationItemRepository reservationItemRepository = new RntCar.MongoDBHelper.Repository.ReservationItemRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                                                StaticHelper.GetConfiguration("MongoDBDatabaseName"));

            var reservation = reservationItemRepository.getReservationItemByReservationId(availabilityParameters.reservationId);

            return new DocumentData
            {
                isMonthly = reservation.isMonthly,
                monthValue = reservation.monthValue,
                paymentPlans = reservation.paymentPlans,
                changeReason = reservation.ChangeReason,
                documentChannel = reservation.ReservationChannel,
                customerId = reservation.CustomerId,
                customerName = reservation.CustomerName,
                depositAmount = reservation.DepositAmount,
                documentId = reservation.ReservationId,
                documentItemId = reservation.ReservationItemId,
                documentNumber = reservation.ReservationNumber,
                documentType = reservation.ReservationType,
                dropoffBranchId = reservation.DropoffBranchId,
                dropoffBranchName = reservation.DropoffBranchName,
                dropoffDateTime = reservation.DropoffTime,
                dropoffTimeStamp = reservation.DropoffTimeStamp,
                equipmentId = reservation.EquipmentId,
                equipmentName = reservation.EquipmentName,
                groupCodeInformationId = reservation.GroupCodeInformationId,
                groupCodeInformationName = reservation.GroupCodeInformationName,
                pricingGroupCodeId = reservation.pricingGroupCodeId,
                pricingGroupCodeName = reservation.pricingGroupCodeName,
                itemTypeCode = reservation.ItemTypeCode,
                netAmount = reservation.NetAmount,
                offset = StaticHelper.offset,
                ownerId = reservation.OwnernId,
                ownerName = reservation.OwnerName,
                paymentChoice = reservation.PaymentChoice.Value,
                pickupBranchId = reservation.PickupBranchId,
                pickupBranchName = reservation.PickupBranchName,
                pickupDateTime = reservation.PickupTime,
                pickupTimeStamp = reservation.PickupTimeStamp,
                pnrNumber = reservation.PnrNumber,
                statecode = reservation.StateCode,
                statuscode = reservation.StatusCode,
                taxRatio = reservation.TaxRatio,
                totalAmount = reservation.TotalAmount,
                taxAmount = reservation.TotalAmount - reservation.NetAmount,
                trackingNumber = reservation.trackingNumber,
                transactioncurrencyid = reservation.CurrencyId,
                _id = reservation._id,
                type = DocumentType.reservation,
                campaignId = reservation.campaignId,
                paymentMethod = reservation.PaymentMethod,
                processIndividualPrices = reservation.processIndividualPrices,
                pickupDateTime_Header = reservation.PickupTime,
                currencyCode = reservation.transactionCurrencyCode,
            };
        }
        private ExcludeAvailability calculateExcludedItems(List<String> allGroupCodes,
                                                           List<ReservationItemDataMongoDB> allReservations,
                                                           List<ContractItemDataMongoDB> allContracts,
                                                           List<TransferDataMongoDB> allTransfers)
        {
            ExcludeAvailability excludeAvailability = new ExcludeAvailability();
            Dictionary<string, decimal> excludeRezervationsByGroupCodes = new Dictionary<string, decimal>();
            Dictionary<string, Dictionary<DateTime, decimal>> availibilityDayResults = new Dictionary<string, Dictionary<DateTime, decimal>>();

            //make unique
            allReservations = allReservations.DistinctBy(p => p.ReservationItemId).ToList();

            allContracts = allContracts.DistinctBy(p => p.contractItemId).ToList();

            allTransfers = allTransfers.DistinctBy(p => p.transferId).ToList();

            //remove itself
            if (this.selectedDocumentData != null)
            {
                allReservations = allReservations.Where(p => p.ReservationItemId != this.selectedDocumentData.documentItemId).ToList();
                allContracts = allContracts.Where(p => p.contractItemId != this.selectedDocumentData.documentItemId).ToList();
            }
            //add all contracts to reservation and make them one list

            var contractsAfterConversions = allContracts.ConvertAll(p => new ReservationItemDataMongoDB
            {
                CustomerId = p.customerId,
                CustomerName = p.customerName,
                ChangeReason = p.changeReason,
                DepositAmount = p.depositAmount,
                DropoffBranchId = p.dropoffBranchId,
                DropoffBranchName = p.dropoffBranchName,
                DropoffTime = p.dropoffDateTime.Value,
                DropoffTimeStamp = p.DropoffTimeStamp,
                EquipmentId = p.equipmentId,
                EquipmentName = p.equipmentName,
                GroupCodeInformationId = p.groupCodeInformationsId,
                GroupCodeInformationName = p.groupCodeInformationsName,
                ItemTypeCode = p.itemTypeCode,
                CurrencyId = p.transactioncurrencyid,
                NetAmount = p.netAmount,
                Offset = StaticHelper.offset,
                PaymentChoice = p.paymentChoice,
                PickupBranchId = p.pickupBranchId,
                PickupBranchName = p.pickupBranchName,
                PickupTime = p.pickupDateTime.Value,
                PickupTimeStamp = p.PickupTimeStamp,
                PnrNumber = p.pnrNumber,
                ReservationChannel = p.contractChannel,
                ReservationId = p.contractId,
                ReservationItemId = p.contractItemId,
                ReservationNumber = p.contractNumber,
                ReservationType = p.contractType,
                TaxRatio = p.taxRatio,
                TotalAmount = p.totalAmount,
                trackingNumber = p.trackingNumber,
                StateCode = p.statecode,
                StatusCode = p.statuscode,
                _id = p._id,
            }).ToList();
            allReservations.AddRange(contractsAfterConversions);

            var transfersAfterConversions = allTransfers.ConvertAll(p => new ReservationItemDataMongoDB
            {
                DropoffBranchId = p.dropoffBranchId,
                DropoffBranchName = p.dropoffBranchName,
                DropoffTime = p.estimatedDropoffDate,
                DropoffTimeStamp = new BsonTimestamp(p.estimatedDropoffDateTimeStamp),
                EquipmentId = p.equipmentId,
                EquipmentName = p.equipmentName,
                GroupCodeInformationId = p.groupCodeId,
                GroupCodeInformationName = p.groupCodeName,
                PickupBranchId = p.pickupBranchId,
                PickupBranchName = p.pickupBranchName,
                PickupTime = p.estimatedPickupDate,
                PickupTimeStamp = new BsonTimestamp(p.estimatedPickupDateTimeStamp),
                StateCode = p.statecode,
                StatusCode = p.statuscode,
                _id = p._id,
                ReservationNumber = p.transferNumber
            });
            allReservations.AddRange(transfersAfterConversions);
            //iterate group codes            
            foreach (var item in allGroupCodes)

            {
                var internalReservations = allReservations.Where(p => new Guid(p.GroupCodeInformationId).Equals(new Guid(item))).ToList();
                //iterate list and try to make a more clear list
                //this while iteration purpose is handle below scenerio;
                //R1 : 01.02.2019 13:00 - 05.01.2019 13:00
                //R2 : 05.02.2019 13:00 - 10.02.2019 13:00
                //will merge and a create a new rez like R3 : 01.02.2019 13:00 - 10.02.2019 13:00
                //while (true == true)
                //{
                //    var res = internalReservations.Where(p => p.PickupTime.Equals(p.DropoffTime)).ToList();
                //    //at least one proper path to prevent infinite loop
                //    if (res.Count < 2)
                //        break;
                //    //build a new list;

                //}
                Dictionary<DateTime, decimal> availibilityDays = new Dictionary<DateTime, decimal>();

                if (internalReservations.Count == 0)
                {
                    //add all days to zero for pricing

                    for (DateTime t = this.availabilityParameters.pickupDateTime; t < this.availabilityParameters.dropoffDateTime; t += TimeSpan.FromDays(1))
                    {

                        availibilityDays[t] = 0;
                    }

                    availibilityDayResults.Add(item, availibilityDays);
                    continue;
                }

                Dictionary<DateTime, decimal> counter = new Dictionary<DateTime, decimal>();
                //this foreach calculate availibility by group codes


                for (DateTime i = this.availabilityParameters.pickupDateTime; i <= this.availabilityParameters.dropoffDateTime; i += TimeSpan.FromDays(1))
                {

                    var list = internalReservations.Where(s => s.PickupTime <= i && s.DropoffTime >= i).ToList();

                    if (list.Count == 0)
                        continue;

                    decimal val;
                    if (counter.TryGetValue(i, out val))
                    {

                        counter[i] += list.Count;
                    }
                    else
                    {
                        counter[i] = list.Count;
                    }

                }

                var returnVal = counter.Count > 0 ? counter.Max(p => p.Value) : decimal.Zero;

                excludeRezervationsByGroupCodes.Add(item, returnVal);

                //calculate availiblity day by day

                for (DateTime t = this.availabilityParameters.pickupDateTime; t < this.availabilityParameters.dropoffDateTime; t += TimeSpan.FromDays(1))
                {

                    decimal val;
                    if (counter.TryGetValue(t, out val))
                    {

                        if (!availibilityDays.ContainsKey(t))
                        {
                            availibilityDays.Add(t, counter[t]);
                        }
                        else
                            availibilityDays[t] += counter[t];
                    }
                    else
                    {
                        if (!availibilityDays.ContainsKey(t))
                        {
                            availibilityDays.Add(t, 0);
                        }
                        else
                            availibilityDays[t] = 0;
                    }
                }

                availibilityDayResults.Add(item, availibilityDays);

                try
                {
                    decimal getvalue = decimal.Zero;
                    decimal currentValue = decimal.Zero;
                    var lastDate = availabilityParameters.dropoffDateTime.AddHours(-availabilityParameters.dropoffDateTime.Hour).AddMinutes(-availabilityParameters.dropoffDateTime.Minute);
                    lastDate = lastDate.AddHours(this.availabilityParameters.pickupDateTime.Hour).AddMinutes(this.availabilityParameters.pickupDateTime.Minute);
                    if (counter.TryGetValue(lastDate, out currentValue) &&
                        !availibilityDayResults[item].TryGetValue(lastDate, out getvalue))
                    {
                        var last = availibilityDayResults.Values.Last();
                        if (last != null)
                        {
                            if (currentValue > last[last.FirstOrDefault().Key])
                            {
                                last[last.FirstOrDefault().Key] = currentValue;
                            }

                        }
                    }
                }
                catch
                {
                }

            }
            return new ExcludeAvailability
            {

                availabilityResults = excludeRezervationsByGroupCodes,
                availabilityDayResults = availibilityDayResults
            };
        }
        private void selectedDocumentPriceOperation(AvailabilityData availability,
                                                    Dictionary<DateTime, decimal> dayAvailibilityResult,
                                                    decimal? totalAmount)
        {
            if (this.documentUpdateType == DocumentUpdateType.shorten)
            {
                var price = decimal.MinValue;
                if (this.selectedDocumentData?.paymentMethod == (int)rnt_PaymentMethodCode.PayBroker ||
                    this.selectedDocumentData?.paymentMethod == (int)rnt_PaymentMethodCode.PayOffice ||
                    this.selectedDocumentData?.paymentMethod == (int)rnt_PaymentMethodCode.FullCredit ||
                    this.selectedDocumentData?.paymentMethod == (int)rnt_PaymentMethodCode.LimitedCredit ||
                    (this.selectedDocumentData?.paymentMethod == (int)rnt_PaymentMethodCode.CreditCard &&
                     this.selectedDocumentData?.documentType == (int)rnt_ReservationTypeCode.Acente))
                {
                    price = calculatePriceForExtendedReservation(availability, dayAvailibilityResult);
                }
                else
                {
                    if (availabilityParameters.operationType == 50)
                    {
                        DurationHelper durationHelper = new DurationHelper(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                           StaticHelper.GetConfiguration("MongoDBDatabaseName"));

                        var parameterDuration = durationHelper.calculateDocumentDurationByPriceHourEffect_Decimal(availabilityParameters.pickupDateTime, availabilityParameters.dropoffDateTime);
                        var documentDuration = durationHelper.calculateDocumentDurationByPriceHourEffect_Decimal(this.selectedDocumentData.pickupDateTime.Value,
                                                                                                                 this.selectedDocumentData.dropoffDateTime.Value);
                        //30 gün rezlerde fiyatı koru
                        if (parameterDuration >= 30 && documentDuration >= 30)
                        {
                            price = calculatePriceForExtendedReservation(availability, dayAvailibilityResult);
                        }
                        else
                        {
                            price = calculatePriceForShortenReservation(availability, dayAvailibilityResult);
                        }
                    }
                    else
                    {
                        price = calculatePriceForShortenReservation(availability, dayAvailibilityResult);
                    }
                }
                if (price != decimal.MinValue)
                {
                    calculateFinalPrices(availability, decimal.Round(price, 2));
                }
                else
                {
                    pricesCouldntCalculated(availability);
                }
            }
            //if reservation has been extended
            else if (this.documentUpdateType == DocumentUpdateType.extended)
            {
                //keep the prices for same day , calculate new prices for new days
                var price = calculatePriceForExtendedReservation(availability, dayAvailibilityResult);
                if (price != decimal.MinValue)
                {
                    calculateFinalPrices(availability, decimal.Round(price, 2));
                }
                else
                {
                    pricesCouldntCalculated(availability);
                }
            }
            //check reservation is shifted with allowed duration
            else if (this.documentUpdateType == DocumentUpdateType.shiftedWithAllowedDuration)
            {
                fillCampaignDataforNotChangedandAllowedShiftDocument(availability);
                createCalculationSummariesforNotChangedandAllowedShiftDocument(availability);
                //same price
                calculateFinalPrices(availability, decimal.Round(totalAmount.HasValue ? totalAmount.Value : decimal.Zero, 2));
            }
            //check reservation is shifted with not allowed duration
            else if (this.documentUpdateType == DocumentUpdateType.shiftedWithNotAllowedDuration)
            {
                //keep the prices for same day , calculate new prices for new days
                var price = calculatePriceForExtendedReservation(availability, dayAvailibilityResult);
                if (price != decimal.MinValue)
                {
                    calculateFinalPrices(availability, decimal.Round(price, 2));
                }
                else
                {
                    pricesCouldntCalculated(availability);
                }

            }
        }
        private bool isExistingDocument()
        {
            return this.selectedDocumentData == null ? false : true;
        }
        private PriceResponse applyCampaignPricesforExistingDocument(PriceResponse priceResponse)
        {
            priceResponse.priceAfterPayMethodPayLater = priceResponse.payLaterAmount;
            priceResponse.priceAfterPayMethodPayNow = priceResponse.payNowAmount;
            var isExistingDocument = false;
            if (this.selectedDocumentData != null &&
               Guid.Parse(this.selectedDocumentData.pricingGroupCodeId) == Guid.Parse(priceResponse.selectedGroupCodeListPrice.GroupCodeInformationId))
            {
                isExistingDocument = true;
            }
            //because campaingn dates store  not in utc
            var d = priceResponse.relatedDay.Date;
            if (priceResponse.relatedDay.AddMinutes(StaticHelper.offset).Date > priceResponse.relatedDay.Date)
            {
                d = priceResponse.relatedDay.AddMinutes(StaticHelper.offset).Date;
            }
            if (this.checkDateCanBenefitFromCampaign(d) &&
            this.selectedDocumentData != null && isExistingDocument)
            {
                var campaignRepository = new CampaignRepository(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var campaign = campaignRepository.getCampaingById(Convert.ToString(this.selectedDocumentData.campaignId.Value));
                var camp = new CampaignHelper().buildCampaignData(campaign);


                var result = new PriceBusiness().CalculateCampaignPrice(
                               camp,
                               priceResponse.payNowAmount,
                               priceResponse.payLaterAmount,
                               priceResponse.isTickDay,
                               this.selectedDocumentData?.pricingGroupCodeId);

                priceResponse.payLaterAmount = result.payLaterDailyPrice.Value;
                priceResponse.payNowAmount = result.payNowDailyPrice.Value;
                if (this.selectedDocumentData.paymentChoice == (int)PaymentEnums.PaymentType.PayLater)
                {
                    priceResponse.totalAmount = priceResponse.payLaterAmount;
                }
                else if (this.selectedDocumentData.paymentChoice == (int)PaymentEnums.PaymentType.PayNow)
                {
                    priceResponse.totalAmount = priceResponse.payNowAmount;
                }
                priceResponse.campaignId = new Guid(campaign.campaignId);
            }
            //eğer ilgili gün kampanyadan faydalanmıyorsa fakat ilgili döküman hala kampanyadan faydalanıyorsa ;
            //price calculationdaki tarihleri konsolide etmek adına dummy campaignid ile güncellemek lazım
            else if (this.userCanBenefitFromCampaign && isExistingDocument)
            {
                priceResponse.campaignId = StaticHelper.dummyCampaignId;
            }
            //buraya geliyorsa bütün günler kampanyadan cıkmıs demektir ve kampanya alanını boşaltıyoruz
            else
            {
                priceResponse.campaignId = null;
            }
            return priceResponse;
        }
        private void getActiveIncreaseCapacityAvailabilityFactor()
        {
            AvailabilityFactorRepository availabilityFactorRepository = new AvailabilityFactorRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                             StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            this.availabilityFactorIncreaseCapacity = availabilityFactorRepository.getActiveIncreaseCapacityAvailabilitiesFactorByGivenChannel(availabilityParameters.pickupDateTime,
                                                                                                                                               availabilityParameters.dropoffDateTime,
                                                                                                                                               this.availabilityParameters.pickupBranchId.ToString(),
                                                                                                                                               availabilityParameters.channel,
                                                                                                                                               availabilityParameters.customerType,
                                                                                                                                               availabilityParameters.accountGroup);
        }
        private void createCalculationSummariesforNotChangedandAllowedShiftDocument(AvailabilityData availabilityData)
        {

            foreach (var item in this.getDailyPricesforSelectedDocument())
            {

                var relatedoldPriceCalculationSummary = new PriceCalculationSummaryMongoDB();
                relatedoldPriceCalculationSummary = relatedoldPriceCalculationSummary.Map(item);
                //todo will check null controls
                relatedoldPriceCalculationSummary.trackingNumber = this.trackingNumber;
                relatedoldPriceCalculationSummary._id = ObjectId.GenerateNewId();
                relatedoldPriceCalculationSummary.priceDate = item.priceDate.AddMinutes(this.shiftDifference);
                relatedoldPriceCalculationSummary.priceDateTimeStamp = new BsonTimestamp(item.priceDate.AddMinutes(this.shiftDifference).converttoTimeStamp());
                relatedoldPriceCalculationSummary.totalAmount = item.totalAmount;

                PriceCalculationSummariesBusiness priceCalculationSummariesBusiness = new PriceCalculationSummariesBusiness(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                                            StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                priceCalculationSummariesBusiness.createPriceCalculationSummaryFromExisting(relatedoldPriceCalculationSummary);
            }
        }
        private AvailabilityData fillCampaignDataforNotChangedandAllowedShiftDocument(AvailabilityData availabilityData)
        {
            if (this.isExistingDocument() && this.selectedDocumentData.campaignId.HasValue)
            {
                availabilityData.documentHasCampaignBefore = true;
                availabilityData.canUserStillHasCampaignBenefit = true;
                var campaignRepository = new CampaignRepository(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var campaign = campaignRepository.getCampaingById(Convert.ToString(this.selectedDocumentData.campaignId.Value));
                availabilityData.CampaignInfo = new CampaignHelper().buildCampaignData(campaign);
            }
            else
            {
                availabilityData.documentHasCampaignBefore = false;
                availabilityData.canUserStillHasCampaignBenefit = false;

            }
            return availabilityData;
        }
        private List<DailyPriceDataMongoDB> getDailyPricesforSelectedDocument()
        {
            var dailyPrices = new List<DailyPriceDataMongoDB>();
            if (this.selectedDocumentData?.type == DocumentType.reservation)
            {
                //first get the old prices ;
                DailyPricesRepository dailyPricesRepository = new DailyPricesRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                        StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                dailyPrices = dailyPricesRepository.getDailyPricesByReservationItemId(new Guid(this.selectedDocumentData?.documentItemId));
            }
            else if (this.selectedDocumentData?.type == DocumentType.contract)
            {
                ContractDailyPrices contractDailyPricesRepository = new ContractDailyPrices(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                            StaticHelper.GetConfiguration("MongoDBDatabaseName"));

                var contractDailyPrices = contractDailyPricesRepository.getDailyPricesByContractItemId(new Guid(this.selectedDocumentData?.documentItemId));
                foreach (var item in contractDailyPrices)
                {
                    DailyPriceDataMongoDB dailyPriceData = new DailyPriceDataMongoDB();
                    dailyPriceData = dailyPriceData.Map(item);
                    dailyPriceData.reservationItemId = item.contractItemId;
                    dailyPrices.Add(dailyPriceData);
                }
            }
            return dailyPrices;
        }
        private bool checkDateCanBenefitFromCampaign(DateTime relatedDate)
        {
            ////kampanya yoksa her zaman kampanyadan yararlanıyormus gibi hesapla
            //if (!this.selectedDocumentData.campaignId.HasValue)
            //{
            //    return true;
            //}
            bool useCampaign = false;
            //check selected document has campaign
            this.campaignDates.TryGetValue(relatedDate, out useCampaign);
            return useCampaign;
        }

        private currency decideCurrencyExistingDocument()
        {
            return new currency
            {
                currencyCode = string.IsNullOrEmpty(this.selectedDocumentData?.currencyCode) ? "TL" : this.selectedDocumentData?.currencyCode,
                currencyId = string.IsNullOrEmpty(this.selectedDocumentData?.transactioncurrencyid) ? "TL" : this.selectedDocumentData?.transactioncurrencyid,
            };
        }
    }
    public class PaymentPlanDataComparer : IEqualityComparer<PaymentPlanData>
    {
        public bool Equals(PaymentPlanData x, PaymentPlanData y)
        {
            return (string.Equals(x.month, y.month) && string.Equals(x.month, y.month));
        }

        public int GetHashCode(PaymentPlanData obj)
        {
            return obj.groupCodeId.GetHashCode();
        }
    }
    internal class currency
    {
        public string currencyCode { get; set; }
        public string currencyId { get; set; }
    }
    internal class Results
    {
        public string name { get; set; }
        public string id { get; set; }
        public int count { get; set; }

    }
    internal class PaymentPlanResponse
    {
        public List<PaymentPlanData> paymentPlan { get; set; }
        public decimal? price { get; set; }
    }
    internal static class AvailabilityStatic
    {
        internal static List<Results> groupByGroupCodes(this List<AvailableEquipments> availableEquipments)
        {
            return (from e in availableEquipments
                    group e by new
                    {
                        e.groupCodeInfoId,
                        e.groupCodeInfoName,
                    } into gcs
                    select new Results
                    {
                        name = gcs.Key.groupCodeInfoName,
                        id = gcs.Key.groupCodeInfoId,
                        count = gcs.Count()
                    }).ToList();
        }
    }
    public class AvailabilityRes
    {
        public string availabilityData { get; set; }
        public string trackingNumber { get; set; }
        public ObjectId _id { get; set; }
    }
}
