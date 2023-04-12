using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class EquipmentAvailabilityBL : BusinessHandler
    {
        public EquipmentAvailabilityBL(IOrganizationService orgService) : base(orgService)
        {
        }
        public EquipmentAvailabilityBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public EquipmentAvailabilityBL(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService) : base(initializer, orgService, tracingService)
        {
        }

        public EquipmentAvailabilityBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public List<EquipmentAvailabilityData> calculateEquipmentAvailability(string[] args)
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            EquipmentRepository equipmentRepository = new EquipmentRepository(crmServiceHelper.IOrganizationService);
            GroupCodeInformationRepository groupCodeInformationRepository = new GroupCodeInformationRepository(crmServiceHelper.IOrganizationService);
            BranchRepository branchRepository = new BranchRepository(crmServiceHelper.IOrganizationService);
            ProductRepository productRepository = new ProductRepository(crmServiceHelper.IOrganizationService);
            ReservationRepository reservationRepository = new ReservationRepository(crmServiceHelper.IOrganizationService);
            ContractRepository contractRepository = new ContractRepository(crmServiceHelper.IOrganizationService);

            var publishDate = DateTime.Now.Date;

            // If batch failed run manually for selected date
            if (args.Length != 0)
            {
                publishDate = DateTime.Parse(args[0]);
            }

            try
            {
                var products = productRepository.getAllProducts();
                var groupCodeInformations = groupCodeInformationRepository.getAllGroupCodeInformations();
                var branches = branchRepository.getActiveBranchs();
                var equipments = equipmentRepository.getAllActiveEquipments(new string[] { "rnt_currentbranchid", "statuscode", "rnt_product", "rnt_transfertype" });
                var furtherReservations = reservationRepository.getFurtherReservations(new string[] { "rnt_pickupdatetime", "rnt_pickupbranchid", "statuscode" }, publishDate);
                var rentalContracts = contractRepository.getRentalContractsByDate(DateTime.Now, new string[] { "rnt_dropoffbranchid" });
                var outgoingContracts = contractRepository.getOutgoingContractsByDate(DateTime.Now, new string[] { "rnt_pickupbranchid" });

                var reservations = new List<ReservationData>();
                furtherReservations.ForEach(reservation =>
                {
                    reservations.Add(new ReservationData
                    {
                        PickupBranchId = reservation.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id,
                        PickupDatetime = reservation.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                        Status = reservation.GetAttributeValue<OptionSetValue>("statuscode").Value
                    });
                });

                List<EquipmentAvailability> equipmentAvailabilityItems = new List<EquipmentAvailability>();
                equipments.ForEach(item =>
                {
                    if (item.Attributes.Contains("rnt_currentbranchid"))
                    {
                        var product = products.Where(p => p.Id == item.GetAttributeValue<EntityReference>("rnt_product").Id).FirstOrDefault();
                        var groupCodeInformation = groupCodeInformations.Where(p => p.Id == product.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id).FirstOrDefault();
                        var branch = branches.Where(p => new Guid(p.BranchId) == item.GetAttributeValue<EntityReference>("rnt_currentbranchid").Id).FirstOrDefault();

                        if (branch == null)
                        {
                            return;
                        }

                        equipmentAvailabilityItems.Add(new EquipmentAvailability
                        {
                            GroupCodeId = groupCodeInformation.Id,
                            GroupCode = groupCodeInformation.GetAttributeValue<string>("rnt_name"),
                            CurrentBranchId = branch.BranchId,
                            CurrentBranch = branch.BranchName,
                            StatusCode = item.GetAttributeValue<OptionSetValue>("statuscode").Value,
                            TransferType = item.Attributes.Contains("rnt_transfertype") ? item.GetAttributeValue<OptionSetValue>("rnt_transfertype").Value : 0,
                            IsFranchise = branch.branchType == (int)rnt_BranchType.Franchise ? true : false,
                        });
                    }
                });


                List<EquipmentAvailabilityData> equipmentAvailabilityData = new List<EquipmentAvailabilityData>();
                var groupedAssetsByBranchId = equipmentAvailabilityItems.GroupBy(grp => grp.CurrentBranchId).ToList();

                groupedAssetsByBranchId.ForEach(groupedItem =>
                {
                    var groupedAssetsByGroupCodeId = groupedItem.GroupBy(grp => grp.GroupCodeId).ToList();
                    var remainingReservationCount = reservations.Where(p => p.PickupBranchId == Guid.Parse(groupedItem.Key) && p.PickupDatetime.Date == publishDate && p.Status == (int)rnt_reservation_StatusCode.New).Count();
                    var furtherReservationCount = reservations.Where(p => p.PickupBranchId == Guid.Parse(groupedItem.Key) && p.PickupDatetime.Date > publishDate && p.Status == (int)rnt_reservation_StatusCode.New).Count();
                    var dailyReservationCount = reservations.Where(p => p.PickupBranchId == Guid.Parse(groupedItem.Key) && p.PickupDatetime.Date == publishDate && (p.Status != (int)rnt_reservation_StatusCode.CancelledByCustomer && p.Status != (int)rnt_reservation_StatusCode.CancelledByRentgo)).Count();
                    var outgoingContractsCount = outgoingContracts.Where(p => p.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id == Guid.Parse(groupedItem.Key)).Count();
                    var rentalContractsCount = rentalContracts.Where(p => p.GetAttributeValue<EntityReference>("rnt_dropoffbranchid").Id == Guid.Parse(groupedItem.Key)).Count();

                    groupedAssetsByGroupCodeId.ForEach(item =>
                    {
                        var defaultItem = item.FirstOrDefault();

                        equipmentAvailabilityData.Add(new EquipmentAvailabilityData
                        {
                            RentalCount = item.Where(p => p.StatusCode.Equals((int)rnt_equipment_StatusCode.Rental)).Count(),
                            AvailableCount = item.Where(p => p.StatusCode.Equals((int)rnt_equipment_StatusCode.Available) ||
                                                              p.StatusCode.Equals((int)rnt_equipment_StatusCode.InWashing) ||
                                                              p.StatusCode.Equals((int)rnt_equipment_StatusCode.InFuelFilling)).Count(),
                            InServiceCount = item.Where(p =>
                              (
                               p.StatusCode.Equals((int)rnt_equipment_StatusCode.InTransfer) &&
                               (p.TransferType == (int)rnt_TransferType.Ariza ||
                               p.TransferType == (int)rnt_TransferType.Bakim ||
                               p.TransferType == (int)rnt_TransferType.Hasar)
                              )
                               ).Count(),
                            InTransferCount = item.Where(p =>
                               (
                                p.StatusCode.Equals((int)rnt_equipment_StatusCode.InTransfer) &&
                                (p.TransferType == (int)rnt_TransferType.UcretsizBilet ||
                                p.TransferType == (int)rnt_TransferType.IkinciEl ||
                                p.TransferType == (int)rnt_TransferType.SubelerArasi)
                               )
                               ).Count(),
                            FirstTransferCount = item.Where(p => p.StatusCode.Equals((int)rnt_equipment_StatusCode.InTransfer) &&
                               p.TransferType == (int)rnt_TransferType.IlkTransfer).Count(),
                            LongTermTransferCount = item.Where(p => p.StatusCode.Equals((int)rnt_equipment_StatusCode.LongTermTransfer)).Count(),
                            LostStolenCount = item.Where(p => p.StatusCode.Equals((int)rnt_equipment_StatusCode.LostStolen)).Count(),
                            MissingInventoriesCount = item.Where(p => p.StatusCode.Equals((int)rnt_equipment_StatusCode.MissingInventories)).Count(),
                            PertCount = item.Where(p => p.StatusCode.Equals((int)rnt_equipment_StatusCode.Pert)).Count(),
                            SecondHandTransferCount = item.Where(p => p.StatusCode.Equals((int)rnt_equipment_StatusCode.SecondHandTransfer)).Count(),
                            SecondHandTransferWaitingConfirmationCount = item.Where(p => p.StatusCode.Equals((int)rnt_equipment_StatusCode.SecondHandTransferWaitingConfirmation)).Count(),
                            WaitingForMaintenanceCount = item.Where(p => p.StatusCode.Equals((int)rnt_equipment_StatusCode.WaitingMaintenance)).Count(),
                            RemainingReservationCount = remainingReservationCount,
                            DailyReservationCount = dailyReservationCount,
                            FurtherReservationCount = furtherReservationCount,
                            OutgoingContractCount = outgoingContractsCount,
                            RentalContractCount = rentalContractsCount,
                            GroupCodeInformationId = defaultItem.GroupCodeId,
                            GroupCode = defaultItem.GroupCode,
                            CurrentBranchId = new Guid(defaultItem.CurrentBranchId),
                            CurrentBranch = defaultItem.CurrentBranch,
                            IsFranchise = defaultItem.IsFranchise,
                            PublishDate = publishDate.Ticks
                        });
                    });
                });

                return equipmentAvailabilityData;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<EquipmentAvailabilityData> calculateEquipmentAvailabilityForCSV(List<EquipmentAvailabilityData> equipmentAvailabilityDatas, DateTime publishDate)
        {
            BranchRepository branchRepository = new BranchRepository(this.OrgService);
            ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
            ContractRepository contractRepository = new ContractRepository(this.OrgService);

            try
            {
                var branches = branchRepository.getActiveBranchs();
                var furtherReservations = reservationRepository.getFurtherReservations(new string[] { "rnt_pickupdatetime", "rnt_pickupbranchid", "statuscode" }, publishDate);
                var rentalContracts = contractRepository.getRentalContractsByDate(publishDate, new string[] { "rnt_dropoffbranchid" });
                var outgoingContracts = contractRepository.getOutgoingContractsByDate(publishDate, new string[] { "rnt_pickupbranchid" });

                var reservations = new List<ReservationData>();
                furtherReservations.ForEach(reservation =>
                {
                    reservations.Add(new ReservationData
                    {
                        PickupBranchId = reservation.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id,
                        PickupDatetime = reservation.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                        Status = reservation.GetAttributeValue<OptionSetValue>("statuscode").Value
                    });
                });

                
                foreach (var item in equipmentAvailabilityDatas)
                {
                    var remainingReservationCount = reservations.Where(p => p.PickupBranchId == item.CurrentBranchId && p.PickupDatetime.Date == publishDate && p.Status == (int)rnt_reservation_StatusCode.Completed).Count();
                    var furtherReservationCount = reservations.Where(p => p.PickupBranchId == item.CurrentBranchId && p.PickupDatetime.Date > publishDate && p.Status == (int)rnt_reservation_StatusCode.Completed).Count();
                    var dailyReservationCount = reservations.Where(p => p.PickupBranchId == item.CurrentBranchId && p.PickupDatetime.Date == publishDate && (p.Status != (int)rnt_reservation_StatusCode.CancelledByCustomer && p.Status != (int)rnt_reservation_StatusCode.CancelledByRentgo)).Count();
                    var outgoingContractsCount = outgoingContracts.Where(p => p.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id == item.CurrentBranchId).Count();
                    var rentalContractsCount = rentalContracts.Where(p => p.GetAttributeValue<EntityReference>("rnt_dropoffbranchid").Id == item.CurrentBranchId).Count();

                    item.IsFranchise = branches.Where(x => new Guid(x.BranchId) == item.CurrentBranchId).FirstOrDefault().branchType == (int)rnt_BranchType.Franchise ? true : false;
                    item.CurrentBranch = branches.Where(p => new Guid(p.BranchId) == item.CurrentBranchId).FirstOrDefault().BranchName;
                    item.RemainingReservationCount = remainingReservationCount;
                    item.DailyReservationCount = dailyReservationCount;
                    item.FurtherReservationCount = furtherReservationCount;
                    item.OutgoingContractCount = outgoingContractsCount;
                    item.RentalContractCount = rentalContractsCount;
                }

                return equipmentAvailabilityDatas;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
