using System;

namespace RntCar.ClassLibrary
{    
    public class CampaignParameters
    {
        /// <summary>
        /// Kampanyalı fiyatları hesaplatmadan önce yaptığınız kullanılabilirlik fiyat hesabına ait traking number.
        /// </summary>
        public string calculatedPricesTrackingNumber { get; set; }
        public string campaignId { get; set; }
        public DateTime beginingDate { get; set; }
        public DateTime endDate { get; set; }
        public string reservationChannelCode { get; set; }
        public string branchId {get; set; }
        public string groupCodeInformationId { get; set; }
        public int customerType { get; set; }
        public string documentId { get; set; }
    }
}
