using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.Report;
using RntCar.MongoDBHelper.Helper;
using RntCar.MongoDBHelper.Repository;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace RntCar.ExternalServicesWebAPI.Controllers
{
    public class ReportController : ApiController
    {
        private string mongoDBHostName { get; set; }
        private string mongoDBDatabaseName { get; set; }
        private List<EquipmentAvailabilityData> this_yearequipmentAvailabilityDatas = new List<EquipmentAvailabilityData>();
        private List<EquipmentAvailabilityData> last_yearequipmentAvailabilityDatas = new List<EquipmentAvailabilityData>();
        private List<RevenueItem> thisyear_revenue = new List<RevenueItem>();
        private List<RevenueItem> lastyear_revenue = new List<RevenueItem>();

        public ReportController()
        {
            mongoDBHostName = StaticHelper.GetConfiguration("MongoDBHostName");
            mongoDBDatabaseName = StaticHelper.GetConfiguration("MongoDBDatabaseName");
            //loggerHelper = new LoggerHelper();
        }

        [HttpPost]
        [HttpGet]
        [Route("api/report/testme")]
        public string testme()
        {
            return "ok";
        }

        [HttpPost]
        [Route("api/report/getBranchAvailabilityReport")]
        public BranchAvailabilityReportResponse getBranchAvailabilityReport([FromBody] ReportRequest equipmentReportRequest)
        {
            try
            {
                MongoDBHelper.Repository.EquipmentRepository equipmentRepository = new MongoDBHelper.Repository.EquipmentRepository(mongoDBHostName, mongoDBDatabaseName);
                var allActiveEquipments = equipmentRepository.getAllActiveEquipmentsByBranchId(Guid.Parse(equipmentReportRequest.branchId));

                var allRentalEquipments = allActiveEquipments.Where(item => (item.StatusCode == (int)ClassLibrary._Enums_1033.rnt_equipment_StatusCode.Rental)).ToList();

                var branchAvailabilityRatio = (float)((allRentalEquipments.Count * 100) / allActiveEquipments.Count);

                List<BranchAvailabilityByGroupCode> branchAvailabilityByGroupCodes = new List<BranchAvailabilityByGroupCode>();

                var groupedEquipmentsByGroupCode = allActiveEquipments.GroupBy(item => item.GroupCodeInformationId).Select(grp => grp.ToList()).ToList();

                groupedEquipmentsByGroupCode.ForEach(item =>
                {
                    var allEquipmentsCount = item.Count;
                    var rentalEquipmentsCount = item.Where(equipment => (equipment.StatusCode == (int)ClassLibrary._Enums_1033.rnt_equipment_StatusCode.Rental)).ToList().Count();
                    branchAvailabilityByGroupCodes.Add(new BranchAvailabilityByGroupCode
                    {
                        allEquipmentsCount = allEquipmentsCount,
                        rentalEquipmentsCount = rentalEquipmentsCount,
                        availabilityRatio = (rentalEquipmentsCount * 100) / allEquipmentsCount,
                        groupCodeInformationId = item.FirstOrDefault().GroupCodeInformationId,
                        groupCodeInformationName = item.FirstOrDefault().GroupCodeInformationName
                    });
                });

                return new BranchAvailabilityReportResponse
                {
                    branchAvailabilityReport = new BranchAvailabilityReport
                    {
                        branchAvailabilityRatio = branchAvailabilityRatio,
                        branchAvailabilityByGroupCode = branchAvailabilityByGroupCodes
                    },

                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new BranchAvailabilityReportResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/report/getEquipmentReport")]
        public EquipmentReportResponse getEquipmentReport([FromBody] ReportRequest equipmentReportRequest)
        {
            try
            {
                MongoDBHelper.Repository.EquipmentRepository equipmentRepository = new MongoDBHelper.Repository.EquipmentRepository(mongoDBHostName, mongoDBDatabaseName);
                var allActiveEquipments = equipmentRepository.getAllActiveEquipmentsByBranchId(Guid.Parse(equipmentReportRequest.branchId));

                var transferTypeOptionSetNames = XrmHelper.getEnumsAsOptionSetModelByLangId("rnt_TransferType", 1055);
                var statusOptionSetNames = XrmHelper.getEnumsAsOptionSetModelByLangId("rnt_equipment_StatusCode", 1055);
                var transmissionTypes = XrmHelper.getEnumsAsOptionSetModelByLangId("rnt_groupcodeinformations_rnt_gearboxcode", 1055);
                var fuelTypes = XrmHelper.getEnumsAsOptionSetModelByLangId("rnt_FuelTypeCode", 1055);

                #region Get Equipment Detail Count
                List<EquipmentDetailCount> groupedEquipmentDetailCount = new List<EquipmentDetailCount>();
                var groupedEquipmentsByStatus = allActiveEquipments.GroupBy(item => item.StatusCode).Select(grp => grp.ToList()).ToList();
                groupedEquipmentsByStatus.ForEach(item =>
                {
                    groupedEquipmentDetailCount.Add(new EquipmentDetailCount
                    {
                        count = item.Count,
                        status = item.FirstOrDefault().StatusCode
                    });
                });
                #endregion Get Equipment Detail Count

                #region Get Equipment Detail By Group Code
                List<EquipmentDetailCountByGroupCode> groupedEquipmentDetailCountByGroupCode = new List<EquipmentDetailCountByGroupCode>();
                var groupedEquipmentsByGroupCode = allActiveEquipments.GroupBy(item => new { item.GroupCodeInformationId }).Select(grp => grp.ToList()).ToList();
                groupedEquipmentsByGroupCode.ForEach(groupedItem =>
                {
                    List<EquipmentDetail> equipmentDetailInfo = new List<EquipmentDetail>();
                    var defaultItem = groupedItem.FirstOrDefault();
                    groupedItem.ForEach(item =>
                    {
                        MongoDBHelper.Repository.TransferRepository transferRepository = new MongoDBHelper.Repository.TransferRepository(mongoDBHostName, mongoDBDatabaseName);
                        var transfer = transferRepository.getTransferDataByEquipmentId(Guid.Parse(item.EquipmentId));
                        var transferType = 0;
                        var transferTypeName = string.Empty;
                        var isEquipmentInTransfer = false;
                        if (transfer != null)
                        {
                            isEquipmentInTransfer = true;
                            transferType = transfer.transferTypeCode;
                            transferTypeName = transferTypeOptionSetNames.Where(optionSetItem => optionSetItem.value == transfer.transferTypeCode).FirstOrDefault().label;
                        }
                        equipmentDetailInfo.Add(new EquipmentDetail
                        {
                            plateNumber = item.PlateNumber,
                            equipmentId = Guid.Parse(item.EquipmentId),
                            brandName = item.brandName,
                            modelName = item.modelName,
                            transferType = transferType,
                            isEquipmentInTransfer = isEquipmentInTransfer,
                            transferTypeName = transferTypeName,
                            status = item.StatusCode,
                            statusName = statusOptionSetNames.Where(statusOptionSetItem => statusOptionSetItem.value == defaultItem.StatusCode).FirstOrDefault()?.label
                        });
                    });

                    groupedEquipmentDetailCountByGroupCode.Add(new EquipmentDetailCountByGroupCode
                    {
                        rentalCount = groupedItem.Where(p => p.StatusCode.Equals((int)ClassLibrary._Enums_1033.rnt_equipment_StatusCode.Rental)).Count(),
                        availableCount = groupedItem.Where(p => p.StatusCode.Equals((int)ClassLibrary._Enums_1033.rnt_equipment_StatusCode.Available) ||
                                                              p.StatusCode.Equals((int)ClassLibrary._Enums_1033.rnt_equipment_StatusCode.InWashing) ||
                                                              p.StatusCode.Equals((int)ClassLibrary._Enums_1033.rnt_equipment_StatusCode.InFuelFilling)).Count(),
                        otherCount = groupedItem.Where(p => p.StatusCode.Equals((int)ClassLibrary._Enums_1033.rnt_equipment_StatusCode.LostStolen) ||
                                                              p.StatusCode.Equals((int)ClassLibrary._Enums_1033.rnt_equipment_StatusCode.MissingInventories) ||
                                                              p.StatusCode.Equals((int)ClassLibrary._Enums_1033.rnt_equipment_StatusCode.SecondHandTransferWaitingConfirmation)).Count(),
                        transmissionTypeName = transmissionTypes.Where(p => p.value.Equals(defaultItem.tranmissionType)).FirstOrDefault()?.label,
                        fuelTypeName = fuelTypes.Where(p => p.value.Equals(defaultItem.fuelType)).FirstOrDefault()?.label,
                        groupCodeInformationId = Guid.Parse(defaultItem.GroupCodeInformationId),
                        groupCodeInformationName = defaultItem.GroupCodeInformationName,
                        equipmentDetail = equipmentDetailInfo
                    });
                });
                #endregion Get Equipment Detail By Group Code

                var equipmentInfo = new EquipmentReport
                {
                    equipmentCount = allActiveEquipments.Count,
                    equipmentDetailCount = groupedEquipmentDetailCount,
                    equipmentDetailCountByGroupCode = groupedEquipmentDetailCountByGroupCode
                };

                return new EquipmentReportResponse
                {
                    equipmentReport = equipmentInfo,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };

            }
            catch (Exception ex)
            {
                return new EquipmentReportResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/report/getBranchStatusReport")]
        public BranchStatusReportResponse getBranchStatusReport([FromBody] ReportRequest reportRequest)
        {
            try
            {
                MongoDBHelper.Repository.ReservationItemRepository reservationItemRepository = new MongoDBHelper.Repository.ReservationItemRepository(mongoDBHostName, mongoDBDatabaseName);
                var dailyGlobalReservation = reservationItemRepository.getReservationsCreatedCurrentDate();
                var dailyBranchReservation = reservationItemRepository.getReservationsCreatedCurrentDateByBranchId(Guid.Parse(reportRequest.branchId));

                return new BranchStatusReportResponse
                {
                    branchStatusReport = new BranchStatusReport
                    {
                        dailyBranchReservationCount = dailyBranchReservation.Count,
                        dailyGlobalReservationCount = dailyGlobalReservation.Count,
                        branchStatusRatio = (dailyBranchReservation.Count * 100) / dailyGlobalReservation.Count
                    },
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new BranchStatusReportResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/report/getReservationReport")]
        public ReservationReportReponse getReservationReport([FromBody] ReportRequest reportRequest)
        {
            try
            {
                MongoDBHelper.Repository.ReservationItemRepository reservationItemRepository = new MongoDBHelper.Repository.ReservationItemRepository(mongoDBHostName, mongoDBDatabaseName);
                var allReservations = reservationItemRepository.getDailyAllReservationItemsByBranchId(Guid.Parse(reportRequest.branchId));

                var groupedReservations = allReservations.GroupBy(item => item.StatusCode).Select(grp => grp.ToList()).ToList();

                List<ReservationDetailCount> reservationDetailCount = new List<ReservationDetailCount>();
                groupedReservations.ForEach(groupedItem =>
                {
                    List<ReservationDetail> reservationDetailList = new List<ReservationDetail>();
                    groupedItem.ForEach(item =>
                    {
                        reservationDetailList.Add(new ReservationDetail
                        {
                            reservationId = item.ReservationId,
                            customerName = item.CustomerName,
                            reservationNumber = item.ReservationNumber,
                            reservationPNR = item.PnrNumber,
                            pickupDateTime = item.PickupTime,
                            dropoffDateTime = item.DropoffTime,
                            groupCodeInformationName = item.GroupCodeInformationName
                        });
                    });
                    reservationDetailCount.Add(new ReservationDetailCount
                    {
                        count = groupedItem.Count,
                        status = groupedItem.FirstOrDefault().StatusCode,
                        reservationDetails = reservationDetailList
                    });
                });
                var reservationReport = new ReservationReport
                {
                    count = allReservations.Count,
                    reservationDetailCount = reservationDetailCount,
                    currentDate = DateTime.UtcNow
                };
                return new ReservationReportReponse
                {
                    reservationReport = reservationReport,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new ReservationReportReponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/report/getContractReport")]
        public ContractReportReponse getContractReport([FromBody] ReportRequest reportRequest)
        {
            try
            {
                MongoDBHelper.Repository.ContractItemRepository contractItemRepository = new MongoDBHelper.Repository.ContractItemRepository(mongoDBHostName, mongoDBDatabaseName);
                var allAvailableContracts = contractItemRepository.getAllAvailableContractsByBranchId(Guid.Parse(reportRequest.branchId));

                MongoDBHelper.Repository.EquipmentRepository equipmentRepository = new MongoDBHelper.Repository.EquipmentRepository(mongoDBHostName, mongoDBDatabaseName);

                List<ContractDetail> contractDetailList = new List<ContractDetail>();
                allAvailableContracts.ForEach(item =>
                {
                    var plateNumber = string.Empty;
                    if (!string.IsNullOrEmpty(item.equipmentId))
                    {
                        var equipment = equipmentRepository.getEquipmentById(Guid.Parse(item.equipmentId));
                        if (equipment != null)
                            plateNumber = equipment.PlateNumber;
                    }
                    contractDetailList.Add(new ContractDetail
                    {
                        contractId = item.contractId,
                        contractNumber = item.contractNumber,
                        dropoffDateTime = item.dropoffDateTime.Value,
                        customerName = item.customerName,
                        plateNo = plateNumber,
                        groupCodeInformationId = item.groupCodeInformationsId,
                        groupCodeInformationName = item.groupCodeInformationsName,
                        contractPNR = item.pnrNumber,
                        statusCode = item.statuscode
                    });
                });

                ContractReport contractReportReponse = new ContractReport
                {
                    count = allAvailableContracts.Count,
                    contractDetail = contractDetailList,
                    currentDateTime = DateTime.Now
                };
                return new ContractReportReponse
                {
                    contractReport = contractReportReponse,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new ContractReportReponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/report/compareplatenumbers")]
        public ComparePlateNumberResponse comparePlateNumbers([FromBody] ComparePlateNumberRequest comparePlateNumberRequest)
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            MongoDBHelper.Repository.EquipmentRepository equipmentRepositoryMongo = new MongoDBHelper.Repository.EquipmentRepository(this.mongoDBHostName, this.mongoDBDatabaseName);
            BusinessLibrary.Repository.EquipmentRepository equipmentRepositoryCrm = new BusinessLibrary.Repository.EquipmentRepository(crmServiceHelper.IOrganizationService);

            List<MongoDBHelper.Model.EquipmentDataMongoDB> equipmentDataMongos = new List<MongoDBHelper.Model.EquipmentDataMongoDB>();

            try
            {
                #region Fetch data from MongoDB
                if (comparePlateNumberRequest.statusCodes.Any())
                {
                    equipmentDataMongos = equipmentRepositoryMongo.getAllActiveEquipmentsByStatusCodes(Guid.Parse(comparePlateNumberRequest.branchId), comparePlateNumberRequest.statusCodes);
                }
                else
                {
                    equipmentDataMongos = equipmentRepositoryMongo.getCurrentEquipments(Guid.Parse(comparePlateNumberRequest.branchId));
                }

                var transferTypeOptionSetNames = XrmHelper.getEnumsAsOptionSetModelByLangId("rnt_TransferType", 1055);
                var statusOptionSetNames = XrmHelper.getEnumsAsOptionSetModelByLangId("rnt_equipment_StatusCode", 1055);

                List<EquipmentItem> equipmentItemsMongo = new List<EquipmentItem>();

                equipmentDataMongos.ForEach(item =>
                {
                    MongoDBHelper.Repository.TransferRepository transferRepository = new MongoDBHelper.Repository.TransferRepository(mongoDBHostName, mongoDBDatabaseName);
                    var transfer = transferRepository.getTransferDataByEquipmentId(Guid.Parse(item.EquipmentId));
                    var transferType = 0;
                    var transferTypeName = string.Empty;

                    if (transfer != null)
                    {
                        transferType = transfer.transferTypeCode;
                        transferTypeName = transferTypeOptionSetNames.Where(optionSetItem => optionSetItem.value == transfer.transferTypeCode).FirstOrDefault().label;
                    }

                    equipmentItemsMongo.Add(new EquipmentItem
                    {
                        plateNumber = item.PlateNumber,
                        currentBranchId = Guid.Parse(item.CurrentBranchId),
                        statusCode = item.StatusCode,
                        statusName = statusOptionSetNames.Where(statusOptionSetItem => statusOptionSetItem.value == item.StatusCode).FirstOrDefault()?.label
                    });
                });
                #endregion

                #region Fetch data from CRM
                var equipmentDataCrm = equipmentRepositoryCrm.getEquipmentsByStatusCode(comparePlateNumberRequest.branchId, comparePlateNumberRequest.statusCodes);
                List<EquipmentItem> equipmentItemsCrm = new List<EquipmentItem>();

                foreach (var item in equipmentDataCrm.Entities)
                {
                    equipmentItemsCrm.Add(new EquipmentItem
                    {
                        plateNumber = Convert.ToString(item.Attributes["rnt_platenumber"]),
                        currentBranchId = item.GetAttributeValue<EntityReference>("rnt_currentbranchid").Id,
                        statusCode = ((OptionSetValue)item.Attributes["statuscode"]).Value,
                        statusName = Convert.ToString(Enum.GetName(typeof(RntCar.ClassLibrary._Enums_1033.rnt_equipment_StatusCode), ((OptionSetValue)item.Attributes["statuscode"]).Value))
                    });
                }
                #endregion

                #region Compare
                List<EquipmentItem> equipmentItemsCompared = new List<EquipmentItem>();

                if (equipmentItemsCrm.Count > equipmentItemsMongo.Count)
                {
                    equipmentItemsCompared = equipmentItemsCrm.Except(equipmentItemsMongo, new IntegrationHelper.EquipmentHelper.EquipmentItemDataComparer()).ToList();
                }
                else
                {
                    equipmentItemsCompared = equipmentItemsMongo.Except(equipmentItemsCrm, new IntegrationHelper.EquipmentHelper.EquipmentItemDataComparer()).ToList();
                }
                #endregion

                return new ComparePlateNumberResponse
                {
                    equipmentItemsMongo = equipmentItemsMongo,
                    equipmentItemsCrm = equipmentItemsCrm,
                    equipmentItemsCompared = equipmentItemsCompared,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new ComparePlateNumberResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/report/getEquipmentAvailabilityReport")]
        public EquipmentAvailabilityResponse getEquipmentAvailabilityReport([FromBody] EquipmentAvailabilityRequest reportRequest)
        {
            var ser = JsonConvert.SerializeObject(reportRequest);
            MongoDBHelper.Repository.EquipmentAvailabilityRepository equipmentAvailabilityRepository = new MongoDBHelper.Repository.EquipmentAvailabilityRepository(this.mongoDBHostName, this.mongoDBDatabaseName);
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            EquipmentAvailabilityBL equipmentAvailabilityBL = new EquipmentAvailabilityBL(crmServiceHelper.IOrganizationService);
            ReportBL reportBL = new ReportBL(crmServiceHelper.IOrganizationService);
            ContractItemBL contractItemBL = new ContractItemBL(crmServiceHelper.IOrganizationService);
            //reportRequest.StartDate = DateTime.Now.Ticks;
            //reportRequest.EndDate = DateTime.Now.Ticks;
            try
            {
                List<EquipmentAvailabilityData> equipmentAvailabilityDatas = new List<EquipmentAvailabilityData>();

                // If query date includes today, feth realtime data from CRM
                if (DateTime.Now.Date >= new DateTime(reportRequest.StartDate).Date && new DateTime(reportRequest.EndDate) >= DateTime.Now.Date)
                {
                    var todaysData = equipmentAvailabilityBL.calculateEquipmentAvailability(new string[] { });

                    if (new DateTime(reportRequest.StartDate) == DateTime.Now.Date)
                    {
                        equipmentAvailabilityDatas = todaysData;
                    }
                    else
                    {
                        // Önceki günlere ait datayı Mongo'dan çek
                        var previousDaysData = equipmentAvailabilityRepository.getEquipmentAvailabilityByDate(reportRequest.StartDate, new DateTime(reportRequest.EndDate).Date.AddDays(-1).Ticks);
                        equipmentAvailabilityDatas = todaysData.Concat(previousDaysData).ToList();
                    }
                }
                else
                {
                    var equipmentAvailabilities = equipmentAvailabilityRepository.getEquipmentAvailabilityByDate(reportRequest.StartDate, reportRequest.EndDate);

                    equipmentAvailabilities.ForEach(item =>
                    {
                        equipmentAvailabilityDatas.Add(new EquipmentAvailabilityData
                        {
                            Availability = item.Availability,
                            AvailableCount = item.AvailableCount,
                            CurrentBranch = item.CurrentBranch,
                            CurrentBranchId = item.CurrentBranchId,
                            GroupCode = item.GroupCode,
                            GroupCodeInformationId = item.GroupCodeInformationId,
                            InServiceCount = item.InServiceCount,
                            InTransferCount = item.InTransferCount,
                            FirstTransferCount = item.FirstTransferCount,
                            LongTermTransferCount = item.LongTermTransferCount,
                            LostStolenCount = item.LostStolenCount,
                            MissingInventoriesCount = item.MissingInventoriesCount,
                            PertCount = item.PertCount,
                            PublishDate = item.PublishDate,
                            RentalCount = item.RentalCount,
                            SecondHandTransferCount = item.SecondHandTransferCount,
                            SecondHandTransferWaitingConfirmationCount = item.SecondHandTransferWaitingConfirmationCount,
                            FurtherReservationCount = item.FurtherReservationCount,
                            OutgoingContractCount = item.OutgoingContractCount,
                            RentalContractCount = item.RentalContractCount,
                            RemainingReservationCount = item.RemainingReservationCount,
                            DailyReservationCount = item.DailyReservationCount,
                            WaitingForMaintenanceCount = item.WaitingForMaintenanceCount,
                            Total = item.Total,
                            OthersCount = item.OthersCount,
                            IsFranchise = item.IsFranchise
                        });
                    });
                }

                var contractItemList = contractItemBL.getContractEquipmentPriceWithPickupDate(new DateTime(reportRequest.StartDate), new DateTime(reportRequest.EndDate).Date.AddDays(1).AddSeconds(-1)); ;
                var revenueItems = reportBL.GetRevenueItems(new DateTime(reportRequest.StartDate), new DateTime(reportRequest.EndDate));
                var days = (new DateTime(reportRequest.EndDate) - new DateTime(reportRequest.StartDate)).Days;
                var result = reportBL.buildEquipmentAvailabilities(new EquipmentAvailabilityResponse { EquipmentAvailabilityDatas = equipmentAvailabilityDatas }, revenueItems, days + 1, contractItemList);
                var i = 1;

                var tempOffice = result.EquipmentAvailabilityDatas.Where(x => !x.IsFranchise).ToList();
                var tempFranchise = result.EquipmentAvailabilityDatas.Where(x => x.IsFranchise).ToList();
                foreach (var item in tempOffice)
                {
                    item.CurrentBranch = i + " . " + item.CurrentBranch;
                    i++;
                }
                foreach (var item in tempFranchise)
                {
                    item.CurrentBranch = i + " . " + item.CurrentBranch;
                    i++;
                }
                result.EquipmentAvailabilityDatas = new List<EquipmentAvailabilityData>();
                result.EquipmentAvailabilityDatas.AddRange(tempOffice);
                result.EquipmentAvailabilityDatas.AddRange(tempFranchise);

                BranchRepository branchRepository = new BranchRepository(crmServiceHelper.IOrganizationService);
                var branchs = branchRepository.getActiveBranchs();

                foreach (var item in result.EquipmentAvailabilityDatas)
                {
                    var currentBranch = branchs.Where(p => new Guid(p.BranchId) == item.CurrentBranchId).FirstOrDefault();
                    if (currentBranch != null)
                    {
                        item.RegionManager = currentBranch.regionManagerName;
                        item.RegionManagerId = currentBranch.regionManagerId.HasValue ? currentBranch.regionManagerId : null;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return new EquipmentAvailabilityResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/report/getRevenueReport")]
        public RevenueReportResponse getRevenueReport([FromBody] RevenueReportRequest revenueReportRequest)
        {
            MongoDBHelper.Repository.RevenueBonusRepository revenueBonusRepository = new MongoDBHelper.Repository.RevenueBonusRepository(this.mongoDBHostName, this.mongoDBDatabaseName);
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            BonusCalculationBL bonusCalculationBL = new BonusCalculationBL(crmServiceHelper.IOrganizationService);
            BonusCalculationHelper bonusCalculationHelper = new BonusCalculationHelper();
            List<RevenueReportData> revenueReportDatas = new List<RevenueReportData>();
            List<RevenueDetailsData> revenueDetailsDatas = new List<RevenueDetailsData>();

            try
            {
                var revenueBonusCalculationsMongoDB = revenueBonusRepository.getRevenueBonusCalculationsByDate(revenueReportRequest.StartDate, revenueReportRequest.EndDate);

                // Calculate branch revenues
                var bonusCalculations = bonusCalculationHelper.createPositionalBonusCalculationData(revenueBonusCalculationsMongoDB);
                var branchRevenues = bonusCalculationBL.calculateBranchRevenues(bonusCalculations);

                // Calculate positional bonus
                var bonusCalculationsGroupedByBranch = revenueBonusCalculationsMongoDB.GroupBy(p => p.PickupBranchId).ToList();
                bonusCalculationsGroupedByBranch.ForEach(groupedBranch =>
                {
                    string businessRole;
                    var branchName = groupedBranch.ToList().FirstOrDefault().PickupBranch;
                    var bonusCalculationsGroupedByRole = groupedBranch.GroupBy(p => p.BusinessRole).ToList();

                    bonusCalculationsGroupedByRole.ForEach(groupedRole =>
                    {
                        var defaultItem = groupedRole.FirstOrDefault();
                        businessRole = defaultItem.BusinessRole;

                        revenueDetailsDatas.Add(new RevenueDetailsData
                        {
                            BaseBonusRatio = defaultItem.BaseBonusRatio,
                            BusinessRole = businessRole,
                            PickupBranch = branchName,
                            PickupBranchId = groupedBranch.Key,
                            PositionalBonusRatio = defaultItem.PositionalBonusRatio,
                            RevenueAmount = branchRevenues.Where(p => p.branchId == groupedBranch.Key).Select(p => p.revenueAmount).FirstOrDefault(),
                            TotalAmount = branchRevenues.Where(p => p.branchId == groupedBranch.Key).Select(p => p.totalAmount).FirstOrDefault()
                        });
                    });
                });

                // Add net revenue and target revenue
                BranchTargetRevenueRepository branchTargetRevenueRepository = new BranchTargetRevenueRepository(crmServiceHelper.IOrganizationService);
                var branchTargetRevenues = branchTargetRevenueRepository.getBranchTargetRevenueByDate(revenueReportRequest.StartDate, revenueReportRequest.EndDate);

                bonusCalculationsGroupedByBranch.ForEach(groupedBranch =>
                {
                    var branchName = groupedBranch.ToList().FirstOrDefault().PickupBranch;
                    var targetRevenue = branchTargetRevenues.Where(p => p.GetAttributeValue<EntityReference>("rnt_branchid").Id == groupedBranch.Key).FirstOrDefault();

                    revenueReportDatas.Add(new RevenueReportData
                    {
                        BranchId = groupedBranch.Key,
                        BranchName = branchName,
                        GoalRevenue = targetRevenue != null ? targetRevenue.GetAttributeValue<Money>("rnt_targetrevenue").Value : 0,
                        ReachedRevenue = branchRevenues.Where(p => p.branchId == groupedBranch.Key).Select(p => p.revenueAmount).FirstOrDefault(),
                        RevenueDetailsDatas = revenueDetailsDatas.Where(p => p.PickupBranchId == groupedBranch.Key).ToList()
                    });
                });

                return new RevenueReportResponse
                {
                    RevenueReportDatas = revenueReportDatas,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new RevenueReportResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/report/getAdditionalProductReport")]
        public AdditionalProductReportResponse getAdditionalProductReport([FromBody] AdditionalProductReportRequest additionalProductReportRequest)
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            BonusCalculationBL bonusCalculationBL = new BonusCalculationBL(crmServiceHelper.IOrganizationService);
            BonusCalculationHelper bonusCalculationHelper = new BonusCalculationHelper();

            try
            {
                // Get master data
                UserContractItemsRepository userContractItemsRepository = new UserContractItemsRepository(this.mongoDBHostName, this.mongoDBDatabaseName);
                var contractItemsMongo = userContractItemsRepository.getUserContractItemsByDate(additionalProductReportRequest.startDate, additionalProductReportRequest.endDate);
                var contractItems = bonusCalculationHelper.createUserBasedBonusCalculationData(contractItemsMongo);

                RevenueBonusRepository revenueBonusRepository = new RevenueBonusRepository(this.mongoDBHostName, this.mongoDBDatabaseName);
                var revenueBonusCalculationsMongo = revenueBonusRepository.getRevenueBonusCalculationsByDate(additionalProductReportRequest.startDate, additionalProductReportRequest.endDate);
                var revenueBonusCalculations = bonusCalculationHelper.createPositionalBonusCalculationData(revenueBonusCalculationsMongo);

                // Calculate branch revenues
                var branchRevenues = bonusCalculationBL.calculateBranchRevenues(revenueBonusCalculations);

                // Prepare reports
                var additionalProductReport = bonusCalculationBL.createAdditionalProductReportData(contractItems, branchRevenues);

                return new AdditionalProductReportResponse
                {
                    additionalProductReport = additionalProductReport,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new AdditionalProductReportResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/report/getdailyreport")]
        public DailyReportResponse getDailyReport([FromBody] GetDailyReportRequest reportRequest)
        {
            try
            {
                MongoDBHelper.Repository.EquipmentAvailabilityRepository equipmentAvailabilityRepository = new MongoDBHelper.Repository.EquipmentAvailabilityRepository(this.mongoDBHostName, this.mongoDBDatabaseName);
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                EquipmentAvailabilityBL equipmentAvailabilityBL = new EquipmentAvailabilityBL(crmServiceHelper.IOrganizationService);
                DurationHelper durationHelper = new DurationHelper(this.mongoDBHostName, this.mongoDBDatabaseName);

                List<Task> _tasks = new List<Task>();
                var thisyear = new Task(() =>
                {
                    getEquipmentAvailability_thisyear(crmServiceHelper.IOrganizationService, reportRequest);
                });

                _tasks.Add(thisyear);
                thisyear.Start();

                var lastyear = new Task(() =>
                {
                    getEquipmentAvailability_lastyear(crmServiceHelper.IOrganizationService, reportRequest);
                });

                _tasks.Add(lastyear);
                lastyear.Start();

                var thisyearRevenue = new Task(() =>
                {
                    getRevenueThisYear(crmServiceHelper.IOrganizationService, reportRequest);
                });

                _tasks.Add(thisyearRevenue);
                thisyearRevenue.Start();

                var lastyearRevenue = new Task(() =>
                {
                    getRevenueLastYear(crmServiceHelper.IOrganizationService, reportRequest);
                });

                _tasks.Add(lastyearRevenue);
                lastyearRevenue.Start();
                Task.WaitAll(_tasks.ToArray());


                var days = (new DateTime(reportRequest.EndDate) - new DateTime(reportRequest.StartDate)).Days;
                ReportBL reportBL = new ReportBL(crmServiceHelper.IOrganizationService);
                List<DailyReportData> dailyReportDatas = new List<DailyReportData>();

                var result = reportBL.buildEquipmentAvailabilities(new EquipmentAvailabilityResponse { EquipmentAvailabilityDatas = this_yearequipmentAvailabilityDatas }, thisyear_revenue, days + 1);
                var lastyearresult = reportBL.buildEquipmentAvailabilities(new EquipmentAvailabilityResponse { EquipmentAvailabilityDatas = last_yearequipmentAvailabilityDatas }, lastyear_revenue, days + 1);

                BusinessLibrary.Repository.ContractRepository contractRepository = new BusinessLibrary.Repository.ContractRepository(crmServiceHelper.IOrganizationService);
                var contracts = contractRepository.getAllActiveContractsByBetweenGivenDates(new DateTime(reportRequest.StartDate), new DateTime(reportRequest.EndDate), new string[] { "rnt_totalduration", "rnt_pickupbranchid" });
                var lastyearcontracts = contractRepository.getAllActiveContractsByBetweenGivenDates(new DateTime(reportRequest.StartDate).AddYears(-1), new DateTime(reportRequest.EndDate).AddYears(-1), new string[] { "rnt_totalduration", "rnt_pickupbranchid" });

                var monthlyContract = contractRepository.getMonthlyContractsDuration(new DateTime(reportRequest.StartDate), new DateTime(reportRequest.EndDate), new string[] { "rnt_dropoffdatetime", "rnt_pickupdatetime" });
                var monthlyContractLastYear = contractRepository.getMonthlyContractsDuration(new DateTime(reportRequest.StartDate).AddYears(-1), new DateTime(reportRequest.EndDate).AddYears(-1), new string[] { "rnt_dropoffdatetime", "rnt_pickupdatetime" });

                BusinessLibrary.Repository.ContractInvoiceDateRepository contractInvoiceDateRepository = new BusinessLibrary.Repository.ContractInvoiceDateRepository(crmServiceHelper.IOrganizationService);
                var invoices = contractInvoiceDateRepository.getContractInvoicesByGivenDates(new DateTime(reportRequest.StartDate), new DateTime(reportRequest.EndDate));

                //calculate revenue for different currencies
                BusinessLibrary.Repository.TransactionCurrencyRepository transactionCurrencyRepository = new BusinessLibrary.Repository.TransactionCurrencyRepository(crmServiceHelper.IOrganizationService);
                var usd = transactionCurrencyRepository.getCurrencyByCode("USD");
                var rate = usd.GetAttributeValue<decimal>("exchangerate");

                var first = new DateTime(new DateTime(reportRequest.EndDate).Year, new DateTime(reportRequest.EndDate).Month, 1);
                var firstLastYear = new DateTime(new DateTime(reportRequest.EndDate).AddYears(-1).Year, new DateTime(reportRequest.EndDate).AddYears(-1).Month, 1);
                foreach (var item in result.EquipmentAvailabilityDatas)
                {
                    var i = invoices.Where(p => ((EntityReference)p.GetAttributeValue<AliasedValue>("af.rnt_pickupbranchid").Value).Id == item.CurrentBranchId).ToList();
                    var countEntity = i.DistinctBy(p => p.GetAttributeValue<EntityReference>("rnt_contractid").Id).ToList();

                    var totalDuration = contracts.Where(p => p.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id == item.CurrentBranchId)?.Sum(p => p.GetAttributeValue<decimal>("rnt_totalduration"));
                    var invoiceSum = i.Sum(p => durationHelper.calculateDocumentDurationByPriceHourEffect_Decimal(p.GetAttributeValue<DateTime>("rnt_pickupdatetime"), p.GetAttributeValue<DateTime>("rnt_dropoffdatetime")));

                    var totalDurationMonthly = monthlyContract.Where(p => ((EntityReference)p.GetAttributeValue<AliasedValue>("contract.rnt_pickupbranchid").Value).Id == item.CurrentBranchId);

                    foreach (var m in totalDurationMonthly)
                    {
                        totalDuration += (m.GetAttributeValue<DateTime>("rnt_dropoffdatetime") - m.GetAttributeValue<DateTime>("rnt_pickupdatetime")).Days;
                        //totalDuration += durationHelper.calculateDocumentDurationByPriceHourEffect_Decimal(first, m.GetAttributeValue<DateTime>("rnt_dropoffdatetime"));
                    }

                    var _totalDuration = totalDuration.HasValue ? totalDuration.Value : 0;
                    DailyReportData d = new DailyReportData
                    {
                        isFranchise = item.IsFranchise,
                        CurrentBranch = item.CurrentBranch,
                        CurrentBranchId = item.CurrentBranchId,
                        GroupCodeInformationId = item.GroupCodeInformationId,
                        Total = item.Total,
                        RentalCount = item.RentalCount,
                        Revenue = item.ReachedRevenue,
                        Revenue_USD = decimal.Round(item.ReachedRevenue.Value * rate, 2, MidpointRounding.AwayFromZero),
                        CompletedContractCount = contracts.Where(p => p.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id == item.CurrentBranchId).Count() + countEntity.Count,
                        totalday = invoiceSum + _totalDuration

                    };
                    dailyReportDatas.Add(d);
                }
                //get beto data
                EquipmentAvailabilityOldRepository equipmentAvailabilityOldRepository = new EquipmentAvailabilityOldRepository(this.mongoDBHostName, this.mongoDBDatabaseName);
                var lastyeardata = equipmentAvailabilityOldRepository.getEquipmentByDate(new DateTime(reportRequest.StartDate).Date.AddYears(-1), new DateTime(reportRequest.EndDate).Date.AddYears(-1));

                foreach (var item in lastyearresult.EquipmentAvailabilityDatas)
                {
                    var totalDuration = lastyearcontracts.Where(p => p.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id == item.CurrentBranchId)?.Sum(p => p.GetAttributeValue<decimal>("rnt_totalduration"));

                    var totalDurationMonthly = monthlyContractLastYear.Where(p => ((EntityReference)p.GetAttributeValue<AliasedValue>("contract.rnt_pickupbranchid").Value).Id == item.CurrentBranchId);


                    foreach (var m in totalDurationMonthly)
                    {
                        totalDuration += durationHelper.calculateDocumentDurationByPriceHourEffect_Decimal(firstLastYear, m.GetAttributeValue<DateTime>("rnt_dropoffdatetime"));
                    }
                    var i = dailyReportDatas.Where(p => p.CurrentBranchId == item.CurrentBranchId).FirstOrDefault();
                    var lastyearitem = lastyeardata.Where(p => p.CurrentBranchId == item.CurrentBranchId).ToList();

                    if (i != null)
                    {
                        var _total = lastyearitem.Count;
                        i.TotalPreviousYear = item.Total;
                        i.RevenuePrevious = item.ReachedRevenue + lastyearitem.Sum(p => p.revenue);
                        i.RevenuePrevious_USD = decimal.Round(i.RevenuePrevious.Value * rate, 2, MidpointRounding.AwayFromZero);
                        i.CompletedContractCounttprevious = lastyearcontracts.Where(p => p.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id == item.CurrentBranchId).Count() + _total;
                        i.totaldayprevious = totalDuration.HasValue ? totalDuration.Value + lastyearitem.Sum(p => p.totalDays) : 0 + lastyearitem.Sum(p => p.totalDays);
                    }
                }

                var j = 1;

                var tempOffice = dailyReportDatas.Where(x => !x.isFranchise).ToList();
                var tempFranchise = dailyReportDatas.Where(x => x.isFranchise).ToList();

                foreach (var item in tempOffice)
                {
                    item.CurrentBranch = j + " . " + item.CurrentBranch;
                    j++;
                }
                foreach (var item in tempFranchise)
                {
                    item.CurrentBranch = j + " . " + item.CurrentBranch;
                    j++;
                }

                DailyReportData totalCorporate = new DailyReportData
                {
                    CurrentBranch = "Kurumsal Toplam",
                    CurrentBranchId = Guid.NewGuid(),
                    Total = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.Total),
                    TotalPreviousYear = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.TotalPreviousYear),
                    growth = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.growth) / dailyReportDatas.Count,
                    RentalCount = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.RentalCount),
                    Availability = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.Availability) / dailyReportDatas.Count,
                    Revenue = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.Revenue),
                    RevenuePrevious = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.RevenuePrevious),
                    growthRevenue = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.growthRevenue) / dailyReportDatas.Count,

                    Revenue_USD = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.Revenue_USD),
                    RevenuePrevious_USD = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.RevenuePrevious_USD),
                    growthRevenue_USD = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.growthRevenue_USD) / dailyReportDatas.Count,

                    revenuepercar_USD = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.revenuepercar_USD),
                    revenuepercarprevious_USD = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.revenuepercarprevious_USD),
                    growthRevenuepercar_USD = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.growthRevenuepercar_USD) / dailyReportDatas.Count,

                    RevenuePerDay_USD = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.RevenuePerDay_USD),
                    RevenuePerDayPrevious_USD = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.RevenuePerDayPrevious_USD),
                    growthrevenueperday_USD = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.growthrevenueperday_USD) / dailyReportDatas.Count,

                    revenuepercar = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.revenuepercar),
                    revenuepercarprevious = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.revenuepercarprevious),
                    growthRevenuepercar = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.growthRevenuepercar) / dailyReportDatas.Count,
                    CompletedContractCount = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.CompletedContractCount),
                    CompletedContractCounttprevious = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.CompletedContractCounttprevious),
                    growthContractCount = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.growthContractCount) / dailyReportDatas.Count,
                    totalday = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.totalday),
                    totaldayprevious = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.totaldayprevious),
                    growthDayCount = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.growthDayCount) / dailyReportDatas.Count,
                    RevenuePerDay = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.RevenuePerDay),
                    RevenuePerDayPrevious = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.RevenuePerDayPrevious),
                    growthrevenueperday = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.growthrevenueperday) / dailyReportDatas.Count,
                    lor = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.lor),
                    lorprevious = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.lorprevious),
                    growthlor = dailyReportDatas.Where(p => p.isFranchise == false).Sum(p => p.growthlor) / dailyReportDatas.Count,
                };

                DailyReportData totalFranchise = new DailyReportData
                {
                    CurrentBranch = "BAYİ Toplam",
                    CurrentBranchId = Guid.NewGuid(),
                    Total = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.Total),
                    TotalPreviousYear = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.TotalPreviousYear),
                    growth = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.growth) / dailyReportDatas.Count,
                    RentalCount = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.RentalCount),
                    Availability = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.Availability) / dailyReportDatas.Count,

                    Revenue = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.Revenue),
                    RevenuePrevious = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.RevenuePrevious),
                    growthRevenue = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.growthRevenue) / dailyReportDatas.Count,

                    Revenue_USD = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.Revenue_USD),
                    RevenuePrevious_USD = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.RevenuePrevious_USD),
                    growthRevenue_USD = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.growthRevenue_USD) / dailyReportDatas.Count,

                    revenuepercar_USD = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.revenuepercar_USD),
                    revenuepercarprevious_USD = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.revenuepercarprevious_USD),
                    growthRevenuepercar_USD = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.growthRevenuepercar_USD) / dailyReportDatas.Count,

                    RevenuePerDay_USD = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.RevenuePerDay_USD),
                    RevenuePerDayPrevious_USD = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.RevenuePerDayPrevious_USD),
                    growthrevenueperday_USD = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.growthrevenueperday_USD) / dailyReportDatas.Count,

                    revenuepercar = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.revenuepercar),
                    revenuepercarprevious = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.revenuepercarprevious),
                    growthRevenuepercar = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.growthRevenuepercar) / dailyReportDatas.Count,
                    CompletedContractCount = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.CompletedContractCount),
                    CompletedContractCounttprevious = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.CompletedContractCounttprevious),
                    growthContractCount = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.growthContractCount) / dailyReportDatas.Count,
                    totalday = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.totalday),
                    totaldayprevious = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.totaldayprevious),
                    growthDayCount = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.growthDayCount) / dailyReportDatas.Count,
                    RevenuePerDay = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.RevenuePerDay),
                    RevenuePerDayPrevious = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.RevenuePerDayPrevious),
                    growthrevenueperday = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.growthrevenueperday) / dailyReportDatas.Count,
                    lor = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.lor),
                    lorprevious = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.lorprevious),
                    growthlor = dailyReportDatas.Where(p => p.isFranchise == true).Sum(p => p.growthlor) / dailyReportDatas.Count,
                };

                DailyReportData total = new DailyReportData
                {
                    CurrentBranch = "Toplam",
                    CurrentBranchId = Guid.NewGuid(),
                    Total = dailyReportDatas.Sum(p => p.Total),
                    TotalPreviousYear = dailyReportDatas.Sum(p => p.TotalPreviousYear),
                    growth = dailyReportDatas.Sum(p => p.growth) / dailyReportDatas.Count,
                    RentalCount = dailyReportDatas.Sum(p => p.RentalCount),
                    Availability = dailyReportDatas.Sum(p => p.Availability) / dailyReportDatas.Count,
                    Revenue = dailyReportDatas.Sum(p => p.Revenue),
                    RevenuePrevious = dailyReportDatas.Sum(p => p.RevenuePrevious),
                    growthRevenue = dailyReportDatas.Sum(p => p.growthRevenue) / dailyReportDatas.Count,

                    Revenue_USD = dailyReportDatas.Sum(p => p.Revenue_USD),
                    RevenuePrevious_USD = dailyReportDatas.Sum(p => p.RevenuePrevious_USD),
                    growthRevenue_USD = dailyReportDatas.Sum(p => p.growthRevenue_USD) / dailyReportDatas.Count,

                    revenuepercar_USD = dailyReportDatas.Sum(p => p.revenuepercar_USD),
                    revenuepercarprevious_USD = dailyReportDatas.Sum(p => p.revenuepercarprevious_USD),
                    growthRevenuepercar_USD = dailyReportDatas.Sum(p => p.growthRevenuepercar_USD) / dailyReportDatas.Count,

                    RevenuePerDay_USD = dailyReportDatas.Sum(p => p.RevenuePerDay_USD),
                    RevenuePerDayPrevious_USD = dailyReportDatas.Sum(p => p.RevenuePerDayPrevious_USD),
                    growthrevenueperday_USD = dailyReportDatas.Sum(p => p.growthrevenueperday_USD) / dailyReportDatas.Count,

                    revenuepercar = dailyReportDatas.Sum(p => p.revenuepercar),
                    revenuepercarprevious = dailyReportDatas.Sum(p => p.revenuepercarprevious),
                    growthRevenuepercar = dailyReportDatas.Sum(p => p.growthRevenuepercar) / dailyReportDatas.Count,
                    CompletedContractCount = dailyReportDatas.Sum(p => p.CompletedContractCount),
                    CompletedContractCounttprevious = dailyReportDatas.Sum(p => p.CompletedContractCounttprevious),
                    growthContractCount = dailyReportDatas.Sum(p => p.growthContractCount) / dailyReportDatas.Count,
                    totalday = dailyReportDatas.Sum(p => p.totalday),
                    totaldayprevious = dailyReportDatas.Sum(p => p.totaldayprevious),
                    growthDayCount = dailyReportDatas.Sum(p => p.growthDayCount) / dailyReportDatas.Count,
                    RevenuePerDay = dailyReportDatas.Sum(p => p.RevenuePerDay),
                    RevenuePerDayPrevious = dailyReportDatas.Sum(p => p.RevenuePerDayPrevious),
                    growthrevenueperday = dailyReportDatas.Sum(p => p.growthrevenueperday) / dailyReportDatas.Count,
                    lor = dailyReportDatas.Sum(p => p.lor),
                    lorprevious = dailyReportDatas.Sum(p => p.lorprevious),
                    growthlor = dailyReportDatas.Sum(p => p.growthlor) / dailyReportDatas.Count,
                };


                dailyReportDatas = new List<DailyReportData>();

                dailyReportDatas.AddRange(tempOffice);
                dailyReportDatas.Add(totalCorporate);
                dailyReportDatas.AddRange(tempFranchise);
                dailyReportDatas.Add(totalFranchise);
                dailyReportDatas.Add(total);

                return new DailyReportResponse
                {
                    ResponseResult = ResponseResult.ReturnSuccess(),
                    dailyReportDatas = dailyReportDatas
                };
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        [HttpGet]
        [Route("api/report/getfleetreport")]
        public FleetReportDataResponse getFleetReport(DateTime startDate, DateTime endDate)
        {
            DateTime starTimeToUse = new DateTime(startDate.Year, startDate.Month, startDate.Day, 4, 0, 0);
            DateTime endTimeToUse = new DateTime(endDate.Year, endDate.Month, endDate.Day, 23, 0, 0);


            try
            {
                MongoDBHelper.Repository.EquipmentRepository equipmentRepository = new MongoDBHelper.Repository.EquipmentRepository(mongoDBHostName, mongoDBDatabaseName);
                List<FleetReportData> fleetReportData = new List<FleetReportData>();

                var reportDatas = equipmentRepository.GetEquipmentForFleetReport(starTimeToUse.Ticks, endTimeToUse.Ticks);

                reportDatas.ForEach(item =>
                {
                    fleetReportData.Add(new FleetReportData
                    {
                        currentBranch = item.currentBranch,
                        groupCode = item.groupCode,
                        name = item.name,
                        brand = item.brand,
                        model = item.model,
                        product = item.product,
                        chassisNumber = item.chassisNumber,
                        modelYearCode = item.modelYearCode,
                        modelYearName = item.modelYearName,
                        gearBoxCode = item.gearBoxCode,
                        gearBoxName = item.gearBoxName,
                        statusCode = item.statusCode,
                        statusName = item.statusName,
                        transferTypeCode = item.transferTypeCode,
                        transferTypeName = item.transferTypeName,
                        maintenancePeriodCode = item.maintenancePeriodCode,
                        maintenancePeriodName = item.maintenancePeriodName,
                        currentKm = item.currentKm,
                        fuelCode = item.fuelCode,
                        licenseNumber = item.licenseNumber,
                        vehicleNo = item.vehicleNo,
                        equipmentColor = item.equipmentColor,
                        tireSize = item.tireSize,
                        tireTypeName = item.tireTypeName,
                        tireTypeCode = item.tireTypeCode,
                        firstRegistrationDate = item.firstRegistrationDate,
                        inspectionExpireDate = item.inspectionExpireDate,
                        licensePlace = item.licensePlace,
                        oldPlate = item.oldPlate,
                        hgsNumber = item.hgsNumber,
                        hgsLabel = item.hgsLabel,
                        mongoDbIntegrationTrigger = item.mongoDbIntegrationTrigger,
                        cost = item.cost,
                        publishDate = item.publishDate
                    });
                });

                return new FleetReportDataResponse
                {
                    ResponseResult = ResponseResult.ReturnSuccess(),
                    fleetReportDatas = fleetReportData
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //[HttpPost]
        //[Route("api/report/getAdditionalProductBonus")]
        //public DailyReportResponse getAdditionalProductBonus([FromBody] GetDailyReportRequest reportRequest)
        //{

        //}
        private void getRevenueThisYear(IOrganizationService service, GetDailyReportRequest reportRequest)
        {
            ReportBL reportBL = new ReportBL(service);
            thisyear_revenue = reportBL.GetRevenueItems(new DateTime(reportRequest.StartDate), new DateTime(reportRequest.EndDate), true);
        }
        private void getRevenueLastYear(IOrganizationService service, GetDailyReportRequest reportRequest)
        {
            ReportBL reportBL = new ReportBL(service);
            lastyear_revenue = reportBL.GetRevenueItems(new DateTime(reportRequest.StartDate).AddYears(-1), new DateTime(reportRequest.EndDate).AddYears(-1), true);
        }
        private void getEquipmentAvailability_thisyear(IOrganizationService service, GetDailyReportRequest reportRequest)
        {
            MongoDBHelper.Repository.EquipmentAvailabilityRepository equipmentAvailabilityRepository = new MongoDBHelper.Repository.EquipmentAvailabilityRepository(this.mongoDBHostName, this.mongoDBDatabaseName);
            EquipmentAvailabilityBL equipmentAvailabilityBL = new EquipmentAvailabilityBL(service);
            if (DateTime.Now.Date >= new DateTime(reportRequest.StartDate) && new DateTime(reportRequest.EndDate) >= DateTime.Now.Date)
            {
                var todaysData = equipmentAvailabilityBL.calculateEquipmentAvailability(new string[] { });

                if (new DateTime(reportRequest.StartDate) == DateTime.Now.Date)
                {
                    this_yearequipmentAvailabilityDatas = todaysData;
                }
                else
                {
                    // Önceki günlere ait datayı Mongo'dan çek
                    var previousDaysData = equipmentAvailabilityRepository.getEquipmentAvailabilityByDate(reportRequest.StartDate, new DateTime(reportRequest.EndDate).Date.AddDays(-1).Ticks);
                    this_yearequipmentAvailabilityDatas = todaysData.Concat(previousDaysData).ToList();
                }
            }
            else
            {
                var equipmentAvailabilities = equipmentAvailabilityRepository.getEquipmentAvailabilityByDate(reportRequest.StartDate, reportRequest.EndDate);

                equipmentAvailabilities.ForEach(item =>
                {
                    this_yearequipmentAvailabilityDatas.Add(new EquipmentAvailabilityData
                    {
                        Availability = item.Availability,
                        AvailableCount = item.AvailableCount,
                        CurrentBranch = item.CurrentBranch,
                        CurrentBranchId = item.CurrentBranchId,
                        GroupCode = item.GroupCode,
                        GroupCodeInformationId = item.GroupCodeInformationId,
                        InTransferCount = item.InTransferCount,
                        LongTermTransferCount = item.LongTermTransferCount,
                        LostStolenCount = item.LostStolenCount,
                        MissingInventoriesCount = item.MissingInventoriesCount,
                        PertCount = item.PertCount,
                        PublishDate = item.PublishDate,
                        RentalCount = item.RentalCount,
                        SecondHandTransferCount = item.SecondHandTransferCount,
                        SecondHandTransferWaitingConfirmationCount = item.SecondHandTransferWaitingConfirmationCount,
                        FurtherReservationCount = item.FurtherReservationCount,
                        OutgoingContractCount = item.OutgoingContractCount,
                        RentalContractCount = item.RentalContractCount,
                        RemainingReservationCount = item.RemainingReservationCount,
                        DailyReservationCount = item.DailyReservationCount,
                        WaitingForMaintenanceCount = item.WaitingForMaintenanceCount,
                        Total = item.Total,
                        OthersCount = item.OthersCount,
                        IsFranchise = item.IsFranchise
                    });
                });
            }
        }
        private void getEquipmentAvailability_lastyear(IOrganizationService service, GetDailyReportRequest reportRequest)
        {
            MongoDBHelper.Repository.EquipmentAvailabilityRepository equipmentAvailabilityRepository = new MongoDBHelper.Repository.EquipmentAvailabilityRepository(this.mongoDBHostName, this.mongoDBDatabaseName);

            var equipmentAvailabilities = equipmentAvailabilityRepository.getEquipmentAvailabilityByDate(new DateTime(reportRequest.StartDate).AddYears(-1).Ticks,
                                                                                                        new DateTime(reportRequest.EndDate).AddYears(-1).Ticks);

            equipmentAvailabilities.ForEach(item =>
            {
                last_yearequipmentAvailabilityDatas.Add(new EquipmentAvailabilityData
                {
                    Availability = item.Availability,
                    AvailableCount = item.AvailableCount,
                    CurrentBranch = item.CurrentBranch,
                    CurrentBranchId = item.CurrentBranchId,
                    GroupCode = item.GroupCode,
                    GroupCodeInformationId = item.GroupCodeInformationId,
                    InTransferCount = item.InTransferCount,
                    LongTermTransferCount = item.LongTermTransferCount,
                    LostStolenCount = item.LostStolenCount,
                    MissingInventoriesCount = item.MissingInventoriesCount,
                    PertCount = item.PertCount,
                    PublishDate = item.PublishDate,
                    RentalCount = item.RentalCount,
                    SecondHandTransferCount = item.SecondHandTransferCount,
                    SecondHandTransferWaitingConfirmationCount = item.SecondHandTransferWaitingConfirmationCount,
                    FurtherReservationCount = item.FurtherReservationCount,
                    OutgoingContractCount = item.OutgoingContractCount,
                    RentalContractCount = item.RentalContractCount,
                    RemainingReservationCount = item.RemainingReservationCount,
                    DailyReservationCount = item.DailyReservationCount,
                    WaitingForMaintenanceCount = item.WaitingForMaintenanceCount,
                    Total = item.Total,
                    OthersCount = item.OthersCount,
                    IsFranchise = item.IsFranchise
                });
            });

        }
    }
}
