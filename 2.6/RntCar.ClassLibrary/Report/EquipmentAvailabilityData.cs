using System;

namespace RntCar.ClassLibrary
{
    public class EquipmentAvailabilityData
    {
        public long PublishDate { get; set; }
        public Guid CurrentBranchId { get; set; }
        public string CurrentBranch { get; set; }
        public string RegionManager { get; set; }
        public Guid? RegionManagerId { get; set; }
        public Guid GroupCodeInformationId { get; set; }
        public string GroupCode { get; set; }
        public double Total { get { return AvailableCount + RentalCount + InServiceCount + InTransferCount +FirstTransferCount + LongTermTransferCount + WaitingForMaintenanceCount + LostStolenCount + MissingInventoriesCount + PertCount + SecondHandTransferCount + SecondHandTransferWaitingConfirmationCount; } set { } }
        public double RentalCount { get; set; }
        public double AvailableCount { get; set; }
        public double InServiceCount { get; set; }
        public double InTransferCount { get; set; }
        public double FirstTransferCount { get; set; }
        public double LongTermTransferCount { get; set; }
        public double LostStolenCount { get; set; }
        public double MissingInventoriesCount { get; set; }
        public double PertCount { get; set; }
        public double SecondHandTransferCount { get; set; }
        public double SecondHandTransferWaitingConfirmationCount { get; set; }
        public double WaitingForMaintenanceCount { get; set; }
        public double Availability { get { if (RentalCount + AvailableCount + WaitingForMaintenanceCount + InServiceCount + InTransferCount + MissingInventoriesCount + SecondHandTransferWaitingConfirmationCount == 0) { return 0; } else { return Math.Round((RentalCount / (RentalCount+ AvailableCount + WaitingForMaintenanceCount + InServiceCount + InTransferCount + MissingInventoriesCount + SecondHandTransferWaitingConfirmationCount)) * 100); } } set { } }
        public double RemainingReservationCount { get; set; }
        public double DailyReservationCount { get; set; }
        /// <summary>
        /// Deprecated
        /// </summary>
        public double ReservationCount { get; set; }
        public double OutgoingContractCount { get; set; }
        public double RentalContractCount { get; set; }
        public double FurtherReservationCount { get; set; }
        /// <summary>
        /// Sütunu olmayan değerlerin toplamı
        /// </summary>
        public double OthersCount { get { return LongTermTransferCount + PertCount + SecondHandTransferCount; } set { } }
        public decimal? ReachedRevenue { get; set; }
        public decimal? DailyRevenue { get; set; }
        public bool IsFranchise { get; set; }
    }
}
