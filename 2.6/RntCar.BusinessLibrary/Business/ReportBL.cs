using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary.Report;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class ReportBL : BusinessHandler
    {
        public ReportBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public ReportBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public ReportBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public ReportBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public BranchAvailabilityReportResponse callGetBranchAvailabilityReportService(string branchId)
        {
            ConfigurationRepository configurationRepository = new ConfigurationRepository(this.OrgService);
            var baseServiceUrl = configurationRepository.GetConfigurationByKey("reportBaseServiceUrl");

            RestSharpHelper restSharpHelper = new RestSharpHelper(baseServiceUrl, "getBranchAvailabilityReport", RestSharp.Method.POST);
            var parameter = new ReportRequest
            {
                branchId = branchId
            };

            restSharpHelper.AddJsonParameter<ReportRequest>(parameter);
            return restSharpHelper.Execute<BranchAvailabilityReportResponse>();
        }
        public EquipmentReportResponse callGetEquipmentReportService(string branchId)
        {
            ConfigurationRepository configurationRepository = new ConfigurationRepository(this.OrgService);
            var baseServiceUrl = configurationRepository.GetConfigurationByKey("reportBaseServiceUrl");

            RestSharpHelper restSharpHelper = new RestSharpHelper(baseServiceUrl, "getEquipmentReport", RestSharp.Method.POST);
            var parameter = new ReportRequest
            {
                branchId = branchId
            };

            restSharpHelper.AddJsonParameter<ReportRequest>(parameter);
            return restSharpHelper.Execute<EquipmentReportResponse>();
        }
        public BranchStatusReportResponse callGetBranchStatusReportService(string branchId)
        {
            ConfigurationRepository configurationRepository = new ConfigurationRepository(this.OrgService);
            var baseServiceUrl = configurationRepository.GetConfigurationByKey("reportBaseServiceUrl");

            RestSharpHelper restSharpHelper = new RestSharpHelper(baseServiceUrl, "getBranchStatusReport", RestSharp.Method.POST);
            var parameter = new ReportRequest
            {
                branchId = branchId
            };

            restSharpHelper.AddJsonParameter<ReportRequest>(parameter);
            return restSharpHelper.Execute<BranchStatusReportResponse>();
        }
        public ReservationReportReponse callGetReservationReportService(string branchId)
        {
            ConfigurationRepository configurationRepository = new ConfigurationRepository(this.OrgService);
            var baseServiceUrl = configurationRepository.GetConfigurationByKey("reportBaseServiceUrl");

            RestSharpHelper restSharpHelper = new RestSharpHelper(baseServiceUrl, "getReservationReport", RestSharp.Method.POST);
            var parameter = new ReportRequest
            {
                branchId = branchId
            };

            restSharpHelper.AddJsonParameter<ReportRequest>(parameter);
            return restSharpHelper.Execute<ReservationReportReponse>();
        }
        public ContractReportReponse callGetContractReportService(string branchId)
        {
            ConfigurationRepository configurationRepository = new ConfigurationRepository(this.OrgService);
            var baseServiceUrl = configurationRepository.GetConfigurationByKey("reportBaseServiceUrl");

            RestSharpHelper restSharpHelper = new RestSharpHelper(baseServiceUrl, "getContractReport", RestSharp.Method.POST);
            var parameter = new ReportRequest
            {
                branchId = branchId
            };

            restSharpHelper.AddJsonParameter<ReportRequest>(parameter);
            return restSharpHelper.Execute<ContractReportReponse>();
        }
        public ContractFaultyInvoiceResponse getContractsWithFaultyInvoiceReport(string branchId, int dayFilter)
        {
            try
            {
                ContractRepository contractRepository = new ContractRepository(this.OrgService);
                var contracts = contractRepository.getCompletedContractsWithFaultyInvoice(branchId, dayFilter);

                List<FaultyContractDetail> faultyContractDetails = new List<FaultyContractDetail>();
                contracts.ForEach(item =>
                {
                    if (item.GetAttributeValue<Money>("rnt_billedamount")?.Value != item.GetAttributeValue<Money>("rnt_totalamount")?.Value)
                    {
                        faultyContractDetails.Add(new FaultyContractDetail
                        {
                            dropoffDateTimeStr = item.GetAttributeValue<DateTime>("rnt_dropoffdatetime").ToString("dd/MM/yyyy HH:mm"),
                            dropoffDateTime = item.GetAttributeValue<DateTime>("rnt_dropoffdatetime"),
                            contractId = item.Id.ToString(),
                            contractPNR = item.Attributes.Contains("rnt_name") ? item.GetAttributeValue<string>("rnt_name") : string.Empty,
                            customerName = item.Attributes.Contains("rnt_customerid") ? item.GetAttributeValue<EntityReference>("rnt_customerid").Name : string.Empty,
                            invoiceAmount = item.Attributes.Contains("rnt_billedamount") ? item.GetAttributeValue<Money>("rnt_billedamount").Value : decimal.Zero,
                            totalAmount = item.Attributes.Contains("rnt_totalamount") ? item.GetAttributeValue<Money>("rnt_totalamount").Value : decimal.Zero,
                        });
                    }
                });
                faultyContractDetails = faultyContractDetails.OrderByDescending(p => p.dropoffDateTime).ToList();
                return new ContractFaultyInvoiceResponse
                {
                    contractDetail = faultyContractDetails,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new ContractFaultyInvoiceResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        public ContractDepositRefundResponse getContractsDepositRefundReport(string branchId, int dayFilter)
        {
            try
            {
                ContractRepository contractRepository = new ContractRepository(this.OrgService);
                string[] columns = { "rnt_name", "rnt_dropoffdatetime", "rnt_pnrnumber", "rnt_customerid", "rnt_totalamount", "rnt_netpayment", "rnt_depositblockage" };
                var contracts = contractRepository.getContractsByDateFilterWithGivenStatusAndColumns(branchId, dayFilter, columns, (int)rnt_contract_StatusCode.Completed);
                this.Trace("contract lenght: " + contracts.Count);
                List<ContractDetailForDepositRefund> contractDetailForDeposits = new List<ContractDetailForDepositRefund>();
                contracts.ForEach(item =>
                {
                    if (item.GetAttributeValue<Money>("rnt_netpayment")?.Value > item.GetAttributeValue<Money>("rnt_totalamount")?.Value)
                    {
                        contractDetailForDeposits.Add(new ContractDetailForDepositRefund
                        {
                            dropoffDateTimeStr = item.GetAttributeValue<DateTime>("rnt_dropoffdatetime").ToString("dd/MM/yyyy HH:mm"),
                            dropoffDateTime = item.GetAttributeValue<DateTime>("rnt_dropoffdatetime"),
                            contractId = item.Id.ToString(),
                            contractPNR = item.Attributes.Contains("rnt_name") ? item.GetAttributeValue<string>("rnt_name") : string.Empty,
                            customerName = item.Attributes.Contains("rnt_customerid") ? item.GetAttributeValue<EntityReference>("rnt_customerid").Name : string.Empty,
                            totalAmount = item.Attributes.Contains("rnt_totalamount") ? item.GetAttributeValue<Money>("rnt_totalamount").Value : decimal.Zero,
                            netPayment = item.Attributes.Contains("rnt_netpayment") ? item.GetAttributeValue<Money>("rnt_netpayment").Value : decimal.Zero,
                            depositBlockage = item.Attributes.Contains("rnt_depositblockage") ? item.GetAttributeValue<bool?>("rnt_depositblockage") : null
                        });
                    }

                });

                contractDetailForDeposits = contractDetailForDeposits.OrderByDescending(p => p.dropoffDateTime).ToList();

                return new ContractDepositRefundResponse
                {
                    contractDetail = contractDetailForDeposits,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new ContractDepositRefundResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        public ComparePlateNumberResponse callComparePlateNumbersService(string branchId, List<int> statusCodes)
        {
            ConfigurationRepository configurationRepository = new ConfigurationRepository(this.OrgService);
            var comparePlateNumbersUrl = configurationRepository.GetConfigurationByKey("reportBaseServiceUrl");

            RestSharpHelper restSharpHelper = new RestSharpHelper(comparePlateNumbersUrl, "comparePlateNumbers", RestSharp.Method.POST);
            var parameter = new ComparePlateNumberRequest
            {
                branchId = branchId,
                statusCodes = statusCodes
            };

            restSharpHelper.AddJsonParameter<ComparePlateNumberRequest>(parameter);

            return restSharpHelper.Execute<ComparePlateNumberResponse>();
        }
        public EquipmentAvailabilityResponse callGetEquipmentAvailabilityService(DateTime startDateTime, DateTime endDateTime)
        {
            ConfigurationRepository configurationRepository = new ConfigurationRepository(this.OrgService);
            var equipmentAvailabilityUrl = configurationRepository.GetConfigurationByKey("reportBaseServiceUrl");

            RestSharpHelper restSharpHelper = new RestSharpHelper(equipmentAvailabilityUrl, "getEquipmentAvailabilityReport", RestSharp.Method.POST);
            var parameter = new EquipmentAvailabilityRequest
            {
                EndDate = endDateTime.Date.Ticks,
                StartDate = startDateTime.Date.Ticks
            };

            restSharpHelper.AddJsonParameter<EquipmentAvailabilityRequest>(parameter);
            var result = restSharpHelper.Execute<EquipmentAvailabilityResponse>();
            //var revenueItems = GetRevenueItems(startDateTime, endDateTime);
            return result;
            //return buildEquipmentAvailabilities(result, revenueItems);
        }
        public DailyReportResponse callDailyReport(DateTime startDateTime, DateTime endDateTime)
        {
            ConfigurationRepository configurationRepository = new ConfigurationRepository(this.OrgService);
            var equipmentAvailabilityUrl = configurationRepository.GetConfigurationByKey("reportBaseServiceUrl");

            RestSharpHelper restSharpHelper = new RestSharpHelper(equipmentAvailabilityUrl, "getdailyreport", RestSharp.Method.POST);
            var parameter = new GetDailyReportRequest
            {
                EndDate = endDateTime.Date.Ticks,
                StartDate = startDateTime.Date.Ticks
            };

            restSharpHelper.AddJsonParameter<GetDailyReportRequest>(parameter);
            var result = restSharpHelper.Execute<DailyReportResponse>();
            //var revenueItems = GetRevenueItems(startDateTime, endDateTime);
            return result;
            //return buildEquipmentAvailabilities(result, revenueItems);
        }
        public List<RevenueItem> GetRevenueItems(DateTime startDateTime, DateTime endDateTime, bool excludeItems = false)
        {
            InvoiceItemRepository invoiceItemRepository = new InvoiceItemRepository(this.OrgService);
            List<RevenueItem> revenueItems = new List<RevenueItem>();
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var excluded = configurationBL.GetConfigurationByName("dailyreportexclude").Split(';');
            var invoiceItems = invoiceItemRepository.getInvoiceItemsByDateByGivenColumns(startDateTime, endDateTime, new string[] { "rnt_netamount", "rnt_reservationitemid", "rnt_contractitemid" });
            //this.Trace("invoiceItems: " + JsonConvert.SerializeObject(invoiceItems));

            invoiceItems.ForEach(item =>
            {
                if (excludeItems && item.Contains("contractitem.rnt_additionalproductid"))
                {
                    var addId = ((EntityReference)item.GetAttributeValue<AliasedValue>("contractitem.rnt_additionalproductid").Value).Id;
                    if(excluded.Where(p=> new Guid(p) == addId).FirstOrDefault() != null)
                    {
                        return;
                    }
                }
                Guid pickUpBranchId = Guid.Empty;
                decimal? netAmount = decimal.Zero;

                try
                {
                    if (item.Attributes.Contains("contract.rnt_pickupbranchid"))
                    {
                        pickUpBranchId = ((EntityReference)(item.GetAttributeValue<AliasedValue>("contract.rnt_pickupbranchid").Value)).Id;

                    }
                    else
                    {
                        pickUpBranchId = ((EntityReference)(item.GetAttributeValue<AliasedValue>("reservation.rnt_pickupbranchid").Value)).Id;
                    }

                    netAmount = item.GetAttributeValue<decimal>("rnt_netamount");
                    revenueItems.Add(new RevenueItem() { branchId = pickUpBranchId, revenue = netAmount });
                }
                catch (Exception ex)
                {
                    throw;
                }
            });

            return revenueItems;
        }

        public EquipmentAvailabilityResponse buildEquipmentAvailabilities(EquipmentAvailabilityResponse response, List<RevenueItem> revenueItems, int days, List<Entity> contractItemList =null)
        {
            var result = new List<EquipmentAvailabilityData>();

            var groupedByBranch = response.EquipmentAvailabilityDatas.GroupBy(item => item.CurrentBranch).ToList();
            groupedByBranch.ForEach(grp =>
            {
                double remainingReservationCount = 0;
                double dailyReservationCount = 0;
                double rentalContractCount = 0;
                double outgoingContractCount = 0;
                double furtherReservationCount = 0;

                var groupedByPublishDate = grp.GroupBy(g => g.PublishDate).ToList();
                groupedByPublishDate.ForEach(grpDate =>
                {
                    remainingReservationCount += grpDate.First().RemainingReservationCount;
                    dailyReservationCount += grpDate.First().DailyReservationCount;
                    rentalContractCount += grpDate.First().RentalContractCount;
                    outgoingContractCount += grpDate.First().OutgoingContractCount;
                    furtherReservationCount += grpDate.First().FurtherReservationCount;
                });

                result.Add(new EquipmentAvailabilityData
                {
                    CurrentBranch = grp.Key,
                    CurrentBranchId = grp.First().CurrentBranchId,
                    IsFranchise = grp.First().IsFranchise,
                    Total = Math.Round(grp.Sum(s => s.Total) / days),
                    OthersCount = Math.Round(grp.Sum(s => s.OthersCount) / days),
                    RentalCount = Math.Round(grp.Sum(s => s.RentalCount) / days),
                    AvailableCount = Math.Round(grp.Sum(s => s.AvailableCount) / days),
                    InServiceCount = Math.Round(grp.Sum(s => s.InServiceCount) / days),
                    InTransferCount = Math.Round(grp.Sum(s => s.InTransferCount) / days),
                    FirstTransferCount = Math.Round(grp.Sum(s => s.FirstTransferCount) / days),
                    LongTermTransferCount = Math.Round(grp.Sum(s => s.LongTermTransferCount) / days),
                    LostStolenCount = Math.Round(grp.Sum(s => s.LostStolenCount) / days),
                    MissingInventoriesCount = Math.Round(grp.Sum(s => s.MissingInventoriesCount) / days),
                    PertCount = Math.Round(grp.Sum(s => s.PertCount) / days),
                    SecondHandTransferCount = Math.Round(grp.Sum(s => s.SecondHandTransferCount) / days),
                    SecondHandTransferWaitingConfirmationCount = Math.Round(grp.Sum(s => s.SecondHandTransferWaitingConfirmationCount) / days),
                    WaitingForMaintenanceCount = Math.Round(grp.Sum(s => s.WaitingForMaintenanceCount) / days),
                    RemainingReservationCount = Math.Round(remainingReservationCount / days),
                    DailyReservationCount = Math.Round(dailyReservationCount / days),
                    RentalContractCount = Math.Round(rentalContractCount / days),
                    OutgoingContractCount = Math.Round(outgoingContractCount / days),
                    FurtherReservationCount = Math.Round(furtherReservationCount / days),
                    ReachedRevenue = revenueItems.Where(r => r.branchId == grp.First().CurrentBranchId).Sum(s => s.revenue).Value,
                    DailyRevenue = contractItemList != null && contractItemList.Count > 0 ? contractItemList.Where(r => r.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id == grp.First().CurrentBranchId).Sum(s => s.GetAttributeValue<Money>("rnt_totalamount").Value) : 0
                });
            });

            this.Trace("result after build" + JsonConvert.SerializeObject(result));

            result = result.OrderBy(p => p.CurrentBranch).ToList();

            return new EquipmentAvailabilityResponse
            {
                EquipmentAvailabilityDatas = result,
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }

        public RevenueReportResponse callGetRevenueReportService(DateTime startDate, DateTime endDate)
        {
            ConfigurationRepository configurationRepository = new ConfigurationRepository(this.OrgService);
            var revenueReportUrl = configurationRepository.GetConfigurationByKey("reportBaseServiceUrl");
            RestSharpHelper restSharpHelper = new RestSharpHelper(revenueReportUrl, "getRevenueReport", RestSharp.Method.POST);

            var parameter = new RevenueReportRequest
            {
                StartDate = startDate,
                EndDate = endDate
            };

            restSharpHelper.AddJsonParameter<RevenueReportRequest>(parameter);

            return restSharpHelper.Execute<RevenueReportResponse>();
        }
        public AdditionalProductReportResponse callGetAdditionalProductReportService(DateTime startDate, DateTime endDate)
        {
            ConfigurationRepository configurationRepository = new ConfigurationRepository(this.OrgService);
            var additionalProductReportUrl = configurationRepository.GetConfigurationByKey("reportBaseServiceUrl");
            RestSharpHelper restSharpHelper = new RestSharpHelper(additionalProductReportUrl, "getAdditionalProductReport", RestSharp.Method.POST);

            var parameter = new AdditionalProductReportRequest
            {
                startDate = startDate,
                endDate = endDate
            };

            restSharpHelper.AddJsonParameter<AdditionalProductReportRequest>(parameter);

            return restSharpHelper.Execute<AdditionalProductReportResponse>();
        }

        public ContractDepositRefundResponse getContractsIsDebitReport(string branchId, int dayFilter)
        {
            this.Trace("getContractsIsDebitReport");
            try
            {
                ContractRepository contractRepository = new ContractRepository(this.OrgService);
                string[] columns = { "rnt_name", "rnt_dropoffdatetime", "rnt_pnrnumber", "rnt_customerid", "rnt_totalamount", "rnt_netpayment", "rnt_debit" };
                var contracts = contractRepository.getContractsByDateFilterWithGivenStatusAndColumns(branchId, dayFilter, columns);

                this.Trace("contract lenght: " + contracts.Count);
                List<ContractDetailForDepositRefund> contractDetailForDebits = new List<ContractDetailForDepositRefund>();
                contracts.ForEach(item =>
                {
                    this.Trace("Is Debit: " + item.GetAttributeValue<bool>("rnt_debit"));
                    if (item.GetAttributeValue<bool>("rnt_debit"))
                    {
                        contractDetailForDebits.Add(new ContractDetailForDepositRefund
                        {
                            dropoffDateTimeStr = item.GetAttributeValue<DateTime>("rnt_dropoffdatetime").ToString("dd/MM/yyyy HH:mm"),
                            dropoffDateTime = item.GetAttributeValue<DateTime>("rnt_dropoffdatetime"),
                            contractId = item.Id.ToString(),
                            contractPNR = item.Attributes.Contains("rnt_name") ? item.GetAttributeValue<string>("rnt_name") : string.Empty,
                            customerName = item.Attributes.Contains("rnt_customerid") ? item.GetAttributeValue<EntityReference>("rnt_customerid").Name : string.Empty,
                            totalAmount = item.Attributes.Contains("rnt_totalamount") ? item.GetAttributeValue<Money>("rnt_totalamount").Value : decimal.Zero,
                            netPayment = item.Attributes.Contains("rnt_netpayment") ? item.GetAttributeValue<Money>("rnt_netpayment").Value : decimal.Zero
                        });
                    }
                });

                contractDetailForDebits = contractDetailForDebits.OrderByDescending(p => p.dropoffDateTime).ToList();

                return new ContractDepositRefundResponse
                {
                    contractDetail = contractDetailForDebits,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new ContractDepositRefundResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }
    }
}
