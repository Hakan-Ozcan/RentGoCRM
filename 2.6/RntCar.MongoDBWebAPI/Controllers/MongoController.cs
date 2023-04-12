using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary.MongoDB;
using RntCar.ClassLibrary.MonthlyGroupCodePriceList;
using RntCar.ClassLibrary.MonthlyPriceList;
using RntCar.ClassLibrary.odata;
using RntCar.MongoDBHelper;
using RntCar.MongoDBHelper.Entities;
using RntCar.MongoDBHelper.Helper;
using RntCar.MongoDBHelper.Model;
using RntCar.MongoDBHelper.Repository;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;

namespace RntCar.MongoDBWebAPI.Controllers
{
    public class MongoController : ApiController
    {
        [HttpPost]
        [HttpGet]
        [Route("api/mongo/testme")]
        public string testme()
        {
            return "i am ok test from prod11";

        }

        [HttpPost]
        [Route("api/mongo/CreateReservationInMongoDB")]
        public MongoDBResponse CreateReservationInMongoDB([FromBody] ReservationItemData reservationItemData)
        {
            try
            {
                ReservationItemBusiness reservationItemBusiness = new ReservationItemBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var r = reservationItemBusiness.CreateReservation(reservationItemData);

                return r;
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }

        }
        [HttpPost]
        [Route("api/mongo/CreateEquipmentInMongoDB")]
        public MongoDBResponse CreateEquipmentInMongoDB([FromBody] EquipmentData equipmentData)
        {
            try
            {
                EquipmentBusiness equipmentBusiness = new EquipmentBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = equipmentBusiness.CreateEquipment(equipmentData);

                return response;
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/CreatePriceListInMongoDB")]
        public MongoDBResponse CreatePriceListInMongoDB([FromBody] PriceListData priceListData)
        {
            try
            {
                PriceListBusiness priceListBusiness = new PriceListBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = priceListBusiness.CreatePriceList(priceListData);

                return response;
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/CreateGroupCodeListPriceInMongoDB")]
        public MongoDBResponse CreateGroupCodeListPriceInMongoDB([FromBody] GroupCodeListPriceData groupCodeListPriceData)
        {
            try
            {
                GroupCodeListPriceBusiness groupCodeListPriceBusiness = new GroupCodeListPriceBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var r = groupCodeListPriceBusiness.CreateGroupCodeListPrice(groupCodeListPriceData);

                return r;
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }

        }
        [HttpPost]
        [Route("api/mongo/CreateAvailabilityPriceListInMongoDB")]
        public MongoDBResponse CreateAvailabilityPriceListInMongoDB([FromBody] AvailabilityPriceListData availabilityPriceListData)
        {
            try
            {
                AvailabilityPriceListBusiness availabilityPriceListBusiness = new AvailabilityPriceListBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var r = availabilityPriceListBusiness.CreateAvailabilityPriceList(availabilityPriceListData);

                return r;
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }

        }
        [HttpPost]
        [Route("api/mongo/CreateOneWayFeeInMongoDB")]
        public MongoDBResponse CreateOneWayFeeInMongoDB([FromBody] OneWayFeeData oneWayFeeData)
        {
            try
            {
                OneWayFeeBusiness oneWayFeeBusiness = new OneWayFeeBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var r = oneWayFeeBusiness.CreateOneWayFee(oneWayFeeData);

                return r;
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }

        }
        [HttpPost]
        [Route("api/mongo/CreateAvailabilityFactorInMongoDB")]
        public MongoDBResponse CreateAvailabilityFactorInMongoDB([FromBody] AvailabilityFactorData availabilityFactorData)
        {
            try
            {
                AvailabilityFactorBusiness availabilityFactorBusiness = new AvailabilityFactorBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = availabilityFactorBusiness.CreateAvailabilityFactor(availabilityFactorData);

                return response;
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/CreatePriceFactorInMongoDB")]
        public MongoDBResponse CreatePriceFactorInMongoDB([FromBody] PriceFactorData priceFactorData)
        {
            try
            {
                PriceFactorBusiness priceFactorBusiness = new PriceFactorBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = priceFactorBusiness.CreatePriceFactor(priceFactorData);
                return response;
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/CreateCampaignInMongoDB")]
        public MongoDBResponse CreateCampaignInMongoDB([FromBody] CampaignData campaignData)
        {
            try
            {
                CampaignBusiness campaignBusiness = new CampaignBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = campaignBusiness.createCampaign(campaignData);
                return response;
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/CreateContractItemInMongoDB")]
        public MongoDBResponse CreateContractItemInMongoDB([FromBody] ContractItemData contractItemData)
        {
            try
            {
                ContractItemBusiness contractItemBusiness = new ContractItemBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = contractItemBusiness.createContractItem(contractItemData);

                return response;
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/CreatePriceHourEffectInMongoDB")]
        public MongoDBResponse CreatePriceHourEffectInMongoDB([FromBody] PriceHourEffectData priceHourEffectData)
        {
            try
            {
                PriceHourEffectBusiness priceHourEffectBusiness = new PriceHourEffectBusiness(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                              StaticHelper.GetConfiguration("MongoDBDatabaseName"));

                var response = priceHourEffectBusiness.createPriceHourEffect(priceHourEffectData);

                return response;
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/CreateTransferInMongoDB")]
        public MongoDBResponse CreateTransferInMongoDB([FromBody] TransferData transferData)
        {
            try
            {
                TransferBusiness transferBusiness = new TransferBusiness(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                         StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = transferBusiness.CreateTransfer(transferData);

                return response;
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/CreateKilometerLimitInMongoDB")]
        public MongoDBResponse CreateKilometerLimitInMongoDB([FromBody] RntCar.ClassLibrary.MongoDB.KilometerLimitData kilometerLimitData)
        {
            try
            {
                KilometerLimitBusiness kilometerLimitBusiness = new KilometerLimitBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = kilometerLimitBusiness.CreateKilometerLimit(kilometerLimitData);
                return response;
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }

        [HttpPost]
        [Route("api/mongo/CreateBusinessClosureInMongoDB")]
        public MongoDBResponse CreateBusinessClosureInMongoDB([FromBody] RntCar.ClassLibrary.MongoDB.BusinessClosure.BusinessClosureData businessClosureData)
        {
            try
            {
                BusinessClosureBusiness businessClosureBusiness = new BusinessClosureBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = businessClosureBusiness.CreateBusinessClosure(businessClosureData);
                return response;
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/CreateCorporateCustomerInMongoDB")]
        public MongoDBResponse CreateCorporateCustomerInMongoDB([FromBody] ClassLibrary.MongoDB.CorporateCustomerData corporateCustomerData)
        {
            try
            {
                CorporateCustomerBusiness corporateCustomerBusiness = new CorporateCustomerBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = corporateCustomerBusiness.CreateCorporateCustomer(corporateCustomerData);
                return response;
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/CreateVirtualBranchInMongoDB")]
        public MongoDBResponse CreateVirtualBranchInMongoDB([FromBody] VirtualBranchData virtualBranchData)
        {
            try
            {
                VirtualBranchBusiness virtualBranchBusiness = new VirtualBranchBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = virtualBranchBusiness.CreateVirtualBranch(virtualBranchData);
                return response;
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [Route("api/mongo/CreateMonthlyPriceListInMongoDB")]
        public MongoDBResponse CreateMonthlyPriceListInMongoDB([FromBody] MonthlyPriceListData monthlyPriceListData)
        {
            try
            {
                MonthlyPriceListBusiness monthlyPriceListBusiness = new MonthlyPriceListBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = monthlyPriceListBusiness.createMonthlyPrice(monthlyPriceListData);
                return response;
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [Route("api/mongo/CreateMonthlyGroupCodePriceListInMongoDB")]
        public MongoDBResponse CreateMonthlyGroupCodePriceListInMongoDB([FromBody] MonthlyGroupCodePriceListData monthlyGroupCodePriceListData)
        {
            try
            {
                MonthlyGroupCodePriceListBusiness monthlyGroupCodePriceListBusiness = new MonthlyGroupCodePriceListBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = monthlyGroupCodePriceListBusiness.createMonthlyGroupCodePrice(monthlyGroupCodePriceListData);
                return response;
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/CreateCrmConfigurationInMongoDB")]
        public MongoDBResponse CreateCrmConfigurationInMongoDB([FromBody] CrmConfigurationData crmConfigurationData, [FromUri] string id)
        {
            try
            {
                CrmConfigurationBusiness crmConfigurationBusiness = new CrmConfigurationBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = crmConfigurationBusiness.createConfiguration(crmConfigurationData, id);
                return response;
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/UpdateTransferInMongoDB")]
        public MongoDBResponse UpdateTransferInMongoDB([FromBody] TransferData transferData, [FromUri] string id)
        {
            try
            {
                TransferBusiness transferBusiness = new TransferBusiness(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                         StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                transferBusiness.UpdateTransfer(transferData, id);
                return MongoDBResponse.ReturnSuccess();
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/UpdateContractItemInMongoDB")]
        public MongoDBResponse UpdateContractItemInMongoDB([FromBody] ContractItemData contractItemData, [FromUri] string id)
        {
            try
            {
                ContractItemBusiness contractItemBusiness = new ContractItemBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                contractItemBusiness.updateContractItem(contractItemData, id);
                return MongoDBResponse.ReturnSuccess();
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/UpdateCampaignInMongoDB")]
        public MongoDBResponse UpdateCampaignInMongoDB([FromBody] CampaignData campaignData, [FromUri] string id)
        {
            try
            {
                CampaignBusiness campaignBusiness = new CampaignBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = campaignBusiness.updateCampaign(campaignData, id);
                return MongoDBResponse.ReturnSuccess();
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/createCampaignPrices")]
        public CreateCampaignPricesResponse createCampaignPrices([FromBody] CreateCampaignPricesRequest createCampaignPricesRequest)
        {
            try
            {
                //*****************IMPORTANT**************** Naming Misunderstanding//
                //--> createCampaignPricesRequest.campaignParameters.beginingDate equals pickupdatetime
                //-->  createCampaignPricesRequest.campaignParameters.endDate equals dropoffdatetime
                var repo = new PriceCalculationSummariesRepository(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var priceCalculationSummaries = repo.getPricesCalculationSummariesByTrackingNumberandGroupCode(createCampaignPricesRequest.campaignParameters.groupCodeInformationId,
                                                                                                               createCampaignPricesRequest.campaignParameters.calculatedPricesTrackingNumber);
                //*****************IMPORTANT**************** Naming Misunderstanding//

                List<PriceCalculationSummaryData> priceCalculationSummaryDatas = new List<PriceCalculationSummaryData>();
                List<PriceCalculationSummaryMongoDB> priceCalculationSummaryMongoDBs = new List<PriceCalculationSummaryMongoDB>();
                var response = new List<CalculatedCampaignPrice>();

                var totalDays = CommonHelper.calculateTotalDurationInDays(createCampaignPricesRequest.campaignParameters.beginingDate,
                                                                          createCampaignPricesRequest.campaignParameters.endDate);
                var reservationChannelCode = createCampaignPricesRequest.campaignParameters.reservationChannelCode;
                var branchId = createCampaignPricesRequest.campaignParameters.branchId;
                var groupCodeInformationId = createCampaignPricesRequest.campaignParameters.groupCodeInformationId;

                Dictionary<Guid, List<long>> campaignData = new Dictionary<Guid, List<long>>();

                foreach (var summary in priceCalculationSummaries)
                {
                    if (createCampaignPricesRequest.campaignParameters.customerType == (int)GlobalEnums.CustomerType.Broker)
                    {
                        createCampaignPricesRequest.campaignParameters.customerType = 4;
                    }
                    else if (createCampaignPricesRequest.campaignParameters.customerType == (int)GlobalEnums.CustomerType.Agency)
                    {
                        createCampaignPricesRequest.campaignParameters.customerType = 3;
                    }
                    var campaignRepository = new CampaignRepository(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                    //todo check for double totaldays later
                    var campaigns = campaignRepository.GetCampaigns(summary.priceDateTimeStamp,
                                                                    Convert.ToInt32(totalDays),
                                                                    reservationChannelCode,
                                                                    branchId,
                                                                    createCampaignPricesRequest.campaignParameters.customerType,
                                                                    groupCodeInformationId);
                    //check intersect days for campaigns
                    //reservation 10-15
                    //campaign 12-16
                    //reservation price days --> 10 11 12 13 14
                    //campaign price days --> 12 13 14 15
                    // so logic is --> kesişen ve kesişmeyen tarihler için pricecalculationsummaries tablosuna campaign id'si dolu olan kayıtlar atılır
                    // kesişen tarihlerin fiyatı kampanyalı fiyat olarak atanır
                    //kesişmeyen tarihlerin fiyatı kullanabilirlikten gelen fiyat olacaktır
                    foreach (var item in campaigns)
                    {
                        List<long> internalDates = new List<long>();
                        campaignData.TryGetValue(new Guid(item.campaignId), out internalDates);
                        if (internalDates == null)
                        {
                            internalDates = new List<long>();
                            internalDates.Add(summary.priceDateTimeStamp.Value);
                            campaignData.Add(new Guid(item.campaignId), internalDates);
                        }
                        else
                        {
                            internalDates.Add(summary.priceDateTimeStamp.Value);
                            campaignData[new Guid(item.campaignId)] = internalDates;
                        }
                    }

                }
                //iterate for campaigns
                foreach (var campItem in campaignData)
                {
                    var campaignRepository = new CampaignRepository(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                    var campaign = campaignRepository.getCampaingById(campItem.Key.ToString());
                    var camp = new CampaignHelper().buildCampaignData(campaign);

                    var payNowPrice = decimal.Zero;
                    var payLaterPrice = decimal.Zero;

                    for (DateTime t = createCampaignPricesRequest.campaignParameters.beginingDate; t < createCampaignPricesRequest.campaignParameters.endDate; t += TimeSpan.FromDays(1))
                    {
                        var standartDailyPrice = priceCalculationSummaries.Where(p => p.priceDateTimeStamp == new BsonTimestamp(t.converttoTimeStamp())).FirstOrDefault();

                        var r = campItem.Value.Where(ps => ps == t.converttoTimeStamp()).ToList();
                        if (r.Count == 0)
                        {
                            payNowPrice += standartDailyPrice.payNowAmount;
                            payLaterPrice += standartDailyPrice.payLaterAmount;

                            PriceCalculationSummariesBusiness _priceCalculationSummariesBusiness = new PriceCalculationSummariesBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                            var campaingSummaryPrice = _priceCalculationSummariesBusiness.buildPriceCalculationSummaryObjectfromExisting(standartDailyPrice,
                                                                                                                                         StaticHelper.dummyCampaignId,
                                                                                                                                         standartDailyPrice.payNowAmount,
                                                                                                                                         standartDailyPrice.payLaterAmount);
                            priceCalculationSummaryMongoDBs.Add(campaingSummaryPrice);
                        }
                        else
                        {

                            var price = new PriceBusiness().CalculateCampaignPrice(
                                        camp,
                                        standartDailyPrice.payNowAmount,
                                        standartDailyPrice.payLaterAmount,
                                        standartDailyPrice.isTickDay,
                                        groupCodeInformationId);

                            payNowPrice += price.payNowDailyPrice.Value;
                            payLaterPrice += price.payLaterDailyPrice.Value;

                            //standartDailyPrice.totalAmount = price.payLaterDailyPrice.Value; 
                            PriceCalculationSummariesBusiness _priceCalculationSummariesBusiness = new PriceCalculationSummariesBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                            var campaingSummaryPrice = _priceCalculationSummariesBusiness.buildPriceCalculationSummaryObjectfromExisting(standartDailyPrice,
                                                                                                                                         campItem.Key,
                                                                                                                                         price.payNowDailyPrice.Value,
                                                                                                                                         price.payLaterDailyPrice.Value);
                            priceCalculationSummaryMongoDBs.Add(campaingSummaryPrice);
                        }
                    }
                    response.Add(new CalculatedCampaignPrice
                    {
                        CampaignInfo = camp,
                        payLaterDailyPrice = payLaterPrice,
                        payNowDailyPrice = payNowPrice
                    });
                }
                if (priceCalculationSummaryMongoDBs.Count > 0)
                {
                    //now create daily prices for campaigns in price calculation summaries
                    PriceCalculationSummariesBusiness priceCalculationSummariesBusiness = new PriceCalculationSummariesBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                    priceCalculationSummariesBusiness.bulkCreatePriceCalculationSummaries(priceCalculationSummaryMongoDBs);
                }
                response = response.Where(p => p.CampaignInfo != null && p.CampaignInfo.campaignType != (int)RntCar.ClassLibrary._Enums_1033.rnt_CampaignTypeCode.Advertisement).ToList();
                return new CreateCampaignPricesResponse
                {
                    ResponseResult = ResponseResult.ReturnSuccess(),
                    calculatedCampaignPrices = response
                };
            }
            catch (Exception ex)
            {
                return new CreateCampaignPricesResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/mongo/UpdatePriceFactorInMongoDB")]
        public MongoDBResponse UpdatePriceFactorInMongoDB([FromBody] PriceFactorData priceFactorData, [FromUri] string id)
        {
            try
            {
                PriceFactorBusiness priceFactorBusiness = new PriceFactorBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = priceFactorBusiness.UpdatePriceFactor(priceFactorData, id);

                return MongoDBResponse.ReturnSuccess();
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/UpdatePriceListInMongoDB")]
        public MongoDBResponse UpdatePriceListInMongoDB([FromBody] PriceListData priceListData, [FromUri] string id)
        {
            try
            {
                PriceListBusiness priceListBusiness = new PriceListBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = priceListBusiness.UpdatePriceList(priceListData, id);

                return MongoDBResponse.ReturnSuccess();
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/UpdateEquipmentInMongoDB")]
        public MongoDBResponse UpdateEquipmentInMongoDB([FromBody] EquipmentData equipmentData, [FromUri] string id)
        {
            try
            {
                EquipmentBusiness equipmentBusiness = new EquipmentBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = equipmentBusiness.UpdateEquipment(equipmentData, id);

                return MongoDBResponse.ReturnSuccess();
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/UpdateReservationInMongoDB")]
        public MongoDBResponse UpdateReservationInMongoDB([FromBody] ReservationItemData reservationItemData, [FromUri] string id)
        {
            try
            {
                ReservationItemBusiness reservationItemBusiness = new ReservationItemBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                return reservationItemBusiness.UpdateReservation(reservationItemData, id);
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }

        }
        [HttpPost]
        [Route("api/mongo/UpdateReservationDeposit")]
        public MongoDBResponse UpdateReservationDeposit([FromUri] decimal depositAmount, [FromUri] string id)
        {
            try
            {
                ReservationItemBusiness reservationItemBusiness = new ReservationItemBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                return reservationItemBusiness.updateDeposit(depositAmount, id);
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }

        }
        [HttpPost]
        [Route("api/mongo/UpdateGroupCodeListPriceInMongoDB")]
        public MongoDBResponse UpdateGroupCodeListPriceInMongoDB([FromBody] GroupCodeListPriceData groupCodeListPriceData, [FromUri] string id)
        {
            try
            {
                GroupCodeListPriceBusiness groupCodeListPriceBusiness = new GroupCodeListPriceBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = groupCodeListPriceBusiness.UpdateGroupCodeListPrice(groupCodeListPriceData, id);

                return MongoDBResponse.ReturnSuccess();
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/UpdateAvailabilityPriceListInMongoDB")]
        public MongoDBResponse UpdateAvailabilityPriceLsistInMongoDB([FromBody] AvailabilityPriceListData availabilityPriceListData, [FromUri] string id)
        {
            try
            {
                AvailabilityPriceListBusiness availabilityPriceListBusiness = new AvailabilityPriceListBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = availabilityPriceListBusiness.UpdateAvailabilityPriceList(availabilityPriceListData, id);

                return MongoDBResponse.ReturnSuccess();
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/UpdateOneWayFeeInMongoDB")]
        public MongoDBResponse UpdateOneWayFeeInMongoDB([FromBody] OneWayFeeData oneWayFeeData, [FromUri] string id)
        {
            try
            {
                OneWayFeeBusiness oneWayFeeBusiness = new OneWayFeeBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = oneWayFeeBusiness.UpdateOneWayFee(oneWayFeeData, id);

                return MongoDBResponse.ReturnSuccess();
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/UpdateAvailabilityFactorInMongoDB")]
        public MongoDBResponse UpdateAvailabilityFactorInMongoDB([FromBody] AvailabilityFactorData availabilityFactorData, [FromUri] string id)
        {
            try
            {
                AvailabilityFactorBusiness availabilityFactorBusiness = new AvailabilityFactorBusiness(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                       StaticHelper.GetConfiguration("MongoDBDatabaseName"));

                var response = availabilityFactorBusiness.UpdateAvailabilityFactor(availabilityFactorData, id);

                return MongoDBResponse.ReturnSuccess();
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/UpdatePriceHourEffectInMongoDB")]
        public MongoDBResponse UpdatePriceHourEffectInMongoDB([FromBody] PriceHourEffectData priceHourEffectData, [FromUri] string id)
        {
            try
            {
                PriceHourEffectBusiness priceHourEffectBusiness = new PriceHourEffectBusiness(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                              StaticHelper.GetConfiguration("MongoDBDatabaseName"));

                var response = priceHourEffectBusiness.updatePriceHourEffect(priceHourEffectData, id);

                return MongoDBResponse.ReturnSuccess();
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/UpdateKilometerLimitInMongoDB")]
        public MongoDBResponse UpdateKilometerLimitInMongoDB([FromBody] RntCar.ClassLibrary.MongoDB.KilometerLimitData kilometerLimitData, [FromUri] string id)
        {
            try
            {
                KilometerLimitBusiness kilometerLimitBusiness = new KilometerLimitBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = kilometerLimitBusiness.UpdateKilometerLimit(kilometerLimitData, id);

                return MongoDBResponse.ReturnSuccess();
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/UpdateBusinessClosureInMongoDB")]
        public MongoDBResponse UpdateBusinessClosureInMongoDB([FromBody] RntCar.ClassLibrary.MongoDB.BusinessClosure.BusinessClosureData businessClosureData, [FromUri] string id)
        {
            try
            {
                BusinessClosureBusiness businessClosureBusiness = new BusinessClosureBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = businessClosureBusiness.UpdateBusinessClosure(businessClosureData, id);

                return MongoDBResponse.ReturnSuccess();
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/UpdateCorporateCustomerInMongoDB")]
        public MongoDBResponse UpdateCorporateCustomerInMongoDB([FromBody] ClassLibrary.MongoDB.CorporateCustomerData corporateCustomerData, [FromUri] string id)
        {
            try
            {
                CorporateCustomerBusiness corporateCustomerBusiness = new CorporateCustomerBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = corporateCustomerBusiness.UpdateCorporateCustomer(corporateCustomerData, id);
                return MongoDBResponse.ReturnSuccess();
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/UpdateVirtualBranchInMongoDB")]
        public MongoDBResponse UpdateVirtualBranchInMongoDB([FromBody] VirtualBranchData virtualBranchData, [FromUri] string id)
        {
            try
            {
                VirtualBranchBusiness virtualBranchBusiness = new VirtualBranchBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = virtualBranchBusiness.UpdateVirtualBranch(virtualBranchData, id);
                return MongoDBResponse.ReturnSuccess();
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/UpdateCrmConfigurationInMongoDB")]
        public MongoDBResponse UpdateCrmConfigurationInMongoDB([FromBody] CrmConfigurationData crmConfigurationData, [FromUri] string id)
        {
            try
            {
                CrmConfigurationBusiness crmConfigurationBusiness = new CrmConfigurationBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = crmConfigurationBusiness.updateConfiguration(crmConfigurationData, id);
                return MongoDBResponse.ReturnSuccess();
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/UpdateMonthlyPriceListInMongoDB")]
        public MongoDBResponse UpdateMonthlyPriceListInMongoDB([FromBody] MonthlyPriceListData monthlyPriceListData, [FromUri] string id)
        {
            try
            {
                MonthlyPriceListBusiness monthlyPriceListBusiness = new MonthlyPriceListBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = monthlyPriceListBusiness.updateMonthlyPrice(monthlyPriceListData, id);
                return MongoDBResponse.ReturnSuccess();
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }

        [Route("api/mongo/UpdateMonthlyGroupCodePriceListInMongoDB")]
        public MongoDBResponse UpdateMonthlyGroupCodePriceListInMongoDB([FromBody] MonthlyGroupCodePriceListData monthlyGroupCodePriceListData, [FromUri] string id)
        {
            try
            {
                MonthlyGroupCodePriceListBusiness monthlyGroupCodePriceListBusiness = new MonthlyGroupCodePriceListBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = monthlyGroupCodePriceListBusiness.updateMonthlyGroupCodePrice(monthlyGroupCodePriceListData, id);
                return MongoDBResponse.ReturnSuccess();
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/calculateavailability")]
        public AvailabilityResponse CalculateAvailability([FromBody] AvailabilityParameters availabilityParameters)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var availabilityResponse = new AvailabilityResponse();
            try
            {
                AvailabilityBusiness availabilityBusiness = new AvailabilityBusiness(availabilityParameters);
                availabilityResponse.availabilityData = availabilityBusiness.calculateAvailability();

                availabilityResponse.trackingNumber = availabilityBusiness.trackingNumber;
                availabilityResponse.ResponseResult = ResponseResult.ReturnSuccess();

                return availabilityResponse;
            }
            catch (Exception ex)
            {
                return new AvailabilityResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)

                };
            }
            finally
            {
                stopwatch.Stop();

                AvailabilityQueryAnalyzerBusiness availabilityQueryAnalyzerBusiness = new AvailabilityQueryAnalyzerBusiness(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                                            StaticHelper.GetConfiguration("MongoDBDatabaseName"));

                availabilityQueryAnalyzerBusiness.createAvailabilityQueryAnalyzer(new MongoDBHelper.Model.AvailabilityQueryAnalyzer
                {
                    trackingNumber = availabilityResponse.trackingNumber,
                    elapsedMiliSeconds = stopwatch.Elapsed.TotalMilliseconds
                });

            }
        }
        [HttpPost]
        [Route("api/mongo/searchreservation")]
        public ReservationItemSearchResponse SearchReservations([FromBody] ReservationItemSearchParameters reservationItemSearchParameters)
        {
            try
            {
                ReservationItemBusiness reservationItemBusiness = new ReservationItemBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                reservationItemBusiness.searchReservation(reservationItemSearchParameters);

                return new ReservationItemSearchResponse
                {

                    ResponseResult = ResponseResult.ReturnSuccess()

                };
            }
            catch (Exception ex)
            {
                return new ReservationItemSearchResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)

                };
            }


        }
        [HttpPost]
        [Route("api/mongo/getFineAmountforReservation")]
        public ReservationFineAmountResponse getFineAmountforReservation([FromUri] Guid reservationItemId)
        {
            try
            {
                var reservationItemBusiness = new ReservationItemBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                return reservationItemBusiness.getFirstDayPrice(reservationItemId);
            }
            catch (Exception ex)
            {
                return new ReservationFineAmountResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/mongo/getFineAmountforContract")]
        public ContractFineAmountResponse getFineAmountforContract([FromUri] Guid contractItemId)
        {
            try
            {
                var contractItemBusiness = new ContractItemBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                return contractItemBusiness.getFirstDayPrice(contractItemId);
            }
            catch (Exception ex)
            {
                return new ContractFineAmountResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/mongo/getPriceCalculationSummaries")]
        public GetPriceCalculationSummariesResponse getPriceCalculationSummaries([FromBody] GetPriceCalculationSummariesRequest getPriceCalculationSummariesRequest)
        {
            try
            {
                List<PriceCalculationSummaryMongoDB> priceCalculationSummaries = new List<PriceCalculationSummaryMongoDB>();

                if (string.IsNullOrEmpty(getPriceCalculationSummariesRequest.campaignId))
                {
                    var repo = new PriceCalculationSummariesRepository(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                    priceCalculationSummaries = repo.getPricesCalculationSummariesByTrackingNumberandGroupCode(getPriceCalculationSummariesRequest.groupCodeInformationId,
                                                                                                               getPriceCalculationSummariesRequest.trackingNumber);
                }
                else
                {
                    var repo = new PriceCalculationSummariesRepository(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                    priceCalculationSummaries = repo.getPricesCalculationSummariesByTrackingNumberCampaignIdandGroupCode(getPriceCalculationSummariesRequest.groupCodeInformationId,
                                                                                                                         getPriceCalculationSummariesRequest.trackingNumber,
                                                                                                                         new Guid(getPriceCalculationSummariesRequest.campaignId));
                }
                var dailyPrices = new List<DailyPrice>();
                foreach (var item in priceCalculationSummaries)
                {
                    DailyPrice p = new DailyPrice();
                    p = p.Map(item);
                    p._priceDateTimeStamp = item.priceDateTimeStamp.Value;
                    p.dailyPricesId = new Guid(item.ID);
                    dailyPrices.Add(p);
                }
                return new GetPriceCalculationSummariesResponse
                {
                    ResponseResult = ResponseResult.ReturnSuccess(),
                    dailyPrices = dailyPrices
                };

            }
            catch (Exception ex)
            {
                return new GetPriceCalculationSummariesResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/mongo/getPriceCalculationSummariesReel")]
        public GetPriceCalculationSummariesReelResponse getPriceCalculationSummariesReel([FromBody] GetPriceCalculationSummariesRequest getPriceCalculationSummariesRequest)
        {
            List<PriceCalculationSummaryMongoDB> priceCalculationSummaries = new List<PriceCalculationSummaryMongoDB>();
            try
            {
                var repo = new PriceCalculationSummariesRepository(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                priceCalculationSummaries = repo.getPriceCalculationSummariesByTrackingNumberAndPricingGroup(getPriceCalculationSummariesRequest.trackingNumber,
                                                                                                             getPriceCalculationSummariesRequest.groupCodeInformationId);

                var priceCalculationReelSummaries = new List<PriceCalculationSummaryDataReel>();

                foreach (var item in priceCalculationSummaries)
                {
                    PriceCalculationSummaryDataReel p = new PriceCalculationSummaryDataReel();
                    p = p.Map(item);
                    p._id = Convert.ToString(item._id.Pid);
                    priceCalculationReelSummaries.Add(p);
                }
                return new GetPriceCalculationSummariesReelResponse
                {
                    ResponseResult = ResponseResult.ReturnSuccess(),
                    priceCalculationSummaries = priceCalculationReelSummaries
                };

            }
            catch (Exception ex)
            {
                return new GetPriceCalculationSummariesReelResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/mongo/createContractDailyPricesFromScratch")]
        public CreateContractDailyPricesFromScratchResponse createContractDailyPricesFromScratch([FromBody] CreateContractDailyPricesFromScratchParameters createContractDailyPricesFromScratchParameters)
        {
            try
            {
                if (!string.IsNullOrEmpty(createContractDailyPricesFromScratchParameters.contractItemId))
                {
                    //delete process  
                    //var contract = new ContractDailyPrices(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                    //var deleteResult = contract.deleteDailyPricesByContractItemId(new Guid(createContractDailyPricesFromScratchParameters.contractItemId));

                    //if (deleteResult)
                    //{
                    //create process
                    if (createContractDailyPricesFromScratchParameters.dailyPriceList.Count > 0)
                    {
                        //var dailyPriceList = createContractDailyPricesFromScratchParameters.dailyPriceList;
                        //var createResult = contract.createContractDailyPricesFromGivenList(dailyPriceList);
                        //Also insert to price calculation summaries

                        List<PriceCalculationSummaryMongoDB> priceCalculationSummaryMongoDBs = new List<PriceCalculationSummaryMongoDB>();
                        foreach (var item in createContractDailyPricesFromScratchParameters.dailyPriceList)
                        {
                            PriceCalculationSummaryMongoDB tempItem = new PriceCalculationSummaryMongoDB();
                            tempItem = tempItem.Map(item);
                            tempItem.campaignId = item.campaignId.HasValue ? item.campaignId.Value : Guid.Empty;
                            tempItem._id = ObjectId.GenerateNewId();
                            tempItem.ID = Guid.NewGuid().ToString();
                            tempItem.priceDateTimeStamp = new BsonTimestamp(item._priceDateTimeStamp);
                            priceCalculationSummaryMongoDBs.Add(tempItem);
                        }
                        if (priceCalculationSummaryMongoDBs.Count > 0)
                        {
                            PriceCalculationSummariesBusiness priceCalculationSummariesBusiness = new PriceCalculationSummariesBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                            priceCalculationSummariesBusiness.bulkCreatePriceCalculationSummaries(priceCalculationSummaryMongoDBs);
                        }
                        return new CreateContractDailyPricesFromScratchResponse
                        {
                            ResponseResult = ResponseResult.ReturnSuccess()
                        };

                    }
                    else
                    {
                        return new CreateContractDailyPricesFromScratchResponse
                        {
                            ResponseResult = ResponseResult.ReturnError("Parameter dailyPriceList does not contain data.")
                        };
                    }
                    //}
                    //else
                    //{
                    //    return new CreateContractDailyPricesFromScratchResponse
                    //    {
                    //        ResponseResult = ResponseResult.ReturnError("Delete Process isn't acknowledged.")
                    //    };
                    //}
                }
                else
                {
                    return new CreateContractDailyPricesFromScratchResponse
                    {
                        ResponseResult = ResponseResult.ReturnError("Parameter tracking number is null.")
                    };
                }
            }
            catch (Exception ex)
            {
                return new CreateContractDailyPricesFromScratchResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message),
                };
            }
        }
        [HttpPost]
        [Route("api/mongo/bulkCreateCouponCodeInMongoDB")]
        public MongoDBResponse bulkCreateCouponCodeInMongoDB([FromBody] CouponCodeParameter couponCodeParameter)
        {
            try
            {
                if (!string.IsNullOrEmpty(couponCodeParameter.couponCodeDefinitionId))
                {
                    if (couponCodeParameter.couponCodes.Count > 0)
                    {
                        List<CouponCodeDataMongoDB> couponCodeMongoDBs = new List<CouponCodeDataMongoDB>();
                        foreach (var item in couponCodeParameter.couponCodes)
                        {
                            CouponCodeDataMongoDB tempItem = new CouponCodeDataMongoDB();
                            tempItem.couponCode = item;
                            tempItem.statusCode = 1; // means generated
                            tempItem._id = ObjectId.GenerateNewId();
                            tempItem.couponCodeDefinitionId = couponCodeParameter.couponCodeDefinitionId;
                            couponCodeMongoDBs.Add(tempItem);
                        }
                        if (couponCodeMongoDBs.Count > 0)
                        {
                            CouponCodeBusiness couponCodeBusiness = new CouponCodeBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                            couponCodeBusiness.bulkCreateCouponCodes(couponCodeMongoDBs);
                        }
                        return new MongoDBResponse
                        {
                            Result = true
                        };
                    }
                    else
                    {
                        return new MongoDBResponse
                        {
                            Result = false,
                            ExceptionDetail = "Parameter couponCodes does not contain data."
                        };
                    }
                }
                else
                {
                    return new MongoDBResponse
                    {
                        Result = false,
                        ExceptionDetail = "Parameter couponCodeDefinitionId number is null."
                    };
                }
            }
            catch (Exception ex)
            {
                return new MongoDBResponse
                {
                    Result = false,
                    ExceptionDetail = ex.Message
                };
            }
        }
        [HttpPost]
        [Route("api/mongo/updateCouponCodeInMongoDB")]
        public MongoDBResponse updateCouponCodeInMongoDB([FromBody]CouponCodeData couponCodeData, [FromUri] string id)
        {
            try
            {
                CouponCodeBusiness couponCodeBusiness = new CouponCodeBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = couponCodeBusiness.UpdateCouponCode(couponCodeData, id);
                return MongoDBResponse.ReturnSuccess();
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/getAvailableCouponCodeDetailsByCouponCode")]
        public CouponCodeResponse getAvailableCouponCodeDetailsByCouponCode([FromUri] string couponCode)
        {
            try
            {
                CouponCodeRepository couponCodeRepository = new CouponCodeRepository(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = couponCodeRepository.getNotBurnedCouponCodeListByCouponCode(couponCode);

                var couponCodeDataList = new List<CouponCodeData>();
                foreach (var item in response)
                {
                    var couponCodeData = new CouponCodeData();
                    couponCodeData = couponCodeData.Map(item);
                    couponCodeData.mongoId = item._id.ToString();
                    couponCodeDataList.Add(couponCodeData);
                }

                return new CouponCodeResponse
                {
                    couponCodeList = couponCodeDataList,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new CouponCodeResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/mongo/getCouponCodeDetailsByCouponCode")]
        public CouponCodeResponse getCouponCodeDetailsByCouponCode([FromUri] string couponCode)
        {
            try
            {
                CouponCodeRepository couponCodeRepository = new CouponCodeRepository(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = couponCodeRepository.getCouponCodeDetailsByCouponCode(couponCode);

                var couponCodeDataList = new List<CouponCodeData>();
                foreach (var item in response)
                {
                    var couponCodeData = new CouponCodeData();
                    couponCodeData = couponCodeData.Map(item);
                    couponCodeData.mongoId = item._id.ToString();
                    couponCodeDataList.Add(couponCodeData);
                }

                return new CouponCodeResponse
                {
                    couponCodeList = couponCodeDataList,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new CouponCodeResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/mongo/createCouponCodeDefinitionInMongoDB")]
        public MongoDBResponse createCouponCodeDefinitionInMongoDB([FromBody] CouponCodeDefinitionData couponCodeDefinitionData)
        {
            try
            {
                CouponCodeDefinitionBusiness couponCodeDefinitionBusiness = new CouponCodeDefinitionBusiness(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                             StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = couponCodeDefinitionBusiness.CreateCouponCodeDefinition(couponCodeDefinitionData);

                return response;
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/mongo/updateCouponCodeDefinitionInMongoDB")]
        public MongoDBResponse updateCouponCodeDefinitionInMongoDB([FromBody] CouponCodeDefinitionData couponCodeDefinitionData, [FromUri] string id)
        {
            try
            {
                CouponCodeDefinitionBusiness couponCodeDefinitionBusiness = new CouponCodeDefinitionBusiness(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                             StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                var response = couponCodeDefinitionBusiness.UpdateCouponCodeDefinition(couponCodeDefinitionData, id);

                return MongoDBResponse.ReturnSuccess();
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        [Route("api/mongo/updatePriceCalculationSummaries")]
        public MongoDBResponse updatePriceCalculationSummaries([FromBody] List<PriceCalculationSummaryData> priceCalculationSummariesList)
        {
            try
            {
                PriceCalculationSummariesBusiness priceCalculationSummariesBusiness = new PriceCalculationSummariesBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));

                List<PriceCalculationSummaryMongoDB> priceCalculationSummariesListMongoDB = new List<PriceCalculationSummaryMongoDB>();
                foreach (var item in priceCalculationSummariesList)
                {
                    PriceCalculationSummaryMongoDB mongoItem = new PriceCalculationSummaryMongoDB();
                    mongoItem = mongoItem.Map(item);
                    priceCalculationSummariesListMongoDB.Add(mongoItem);
                }
                var response = priceCalculationSummariesBusiness.bulkUpdatePriceCalculationSummaries(priceCalculationSummariesListMongoDB);
                return MongoDBResponse.ReturnSuccess();
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }

        [Route("api/mongo/updatePriceCalculationSummariesCampaign")]
        public MongoDBResponse updatePriceCalculationSummariesCampaign([FromBody] UpdatePriceCalculationSummariesRequest updatePriceCalculationSummariesRequest)
        {
            List<PriceCalculationSummaryMongoDB> priceCalculationSummaries = new List<PriceCalculationSummaryMongoDB>();

            try
            {
                var repo = new PriceCalculationSummariesRepository(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                priceCalculationSummaries = repo.getPriceCalculationSummariesByTrackingNumberAndPricingGroup(updatePriceCalculationSummariesRequest.trackingNumber,
                                                                                                             updatePriceCalculationSummariesRequest.groupCodeInformationId);

                PriceCalculationSummariesBusiness priceCalculationSummariesBusiness = new PriceCalculationSummariesBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                Guid campaignId = !string.IsNullOrEmpty(updatePriceCalculationSummariesRequest.campaignId) ? new Guid(updatePriceCalculationSummariesRequest.campaignId) : Guid.Empty;
                foreach (var item in priceCalculationSummaries)
                {
                    item.campaignId = campaignId;
                }
                var response = priceCalculationSummariesBusiness.bulkUpdatePriceCalculationSummaries(priceCalculationSummaries);
                return MongoDBResponse.ReturnSuccess();
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }

        [Route("api/mongo/updatePriceCalculationSummariesPricingGroup")]
        public MongoDBResponse updatePriceCalculationSummariesPricingGroup([FromBody] UpdatePriceCalculationSummariesRequest updatePriceCalculationSummariesRequest)
        {
            List<PriceCalculationSummaryMongoDB> priceCalculationSummaries = new List<PriceCalculationSummaryMongoDB>();

            try
            {
                var repo = new PriceCalculationSummariesRepository(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                priceCalculationSummaries = repo.getPriceCalculationSummariesByTrackingNumberAndPricingGroup(updatePriceCalculationSummariesRequest.trackingNumber,
                                                                                                             updatePriceCalculationSummariesRequest.groupCodeInformationId);

                if (priceCalculationSummaries == null || priceCalculationSummaries.Count == 0)
                {
                    PriceCalculationSummariesBusiness priceCalculationSummariesBusiness = new PriceCalculationSummariesBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));

                    priceCalculationSummaries = repo.getPriceCalculationSummariesByTrackingNumberAndPricingGroup(updatePriceCalculationSummariesRequest.trackingNumber,
                                                                                                             updatePriceCalculationSummariesRequest.preGroupCodeInformationId);

                    priceCalculationSummariesBusiness.addPriceCalculationSummaryWithSourceGroupCodeId(priceCalculationSummaries, updatePriceCalculationSummariesRequest.trackingNumber, updatePriceCalculationSummariesRequest.groupCodeInformationId, updatePriceCalculationSummariesRequest.groupCodeInformationName);
                }

                return MongoDBResponse.ReturnSuccess();
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }

    }
}
