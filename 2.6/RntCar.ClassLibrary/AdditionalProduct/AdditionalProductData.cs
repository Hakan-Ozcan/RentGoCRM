using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class AdditionalProductData 
    {
        public  Guid productId { get; set; }
        public string productName { get; set; }
        public int productType { get; set; }
        public bool showOnContractUpdateForMonthly { get; set; }
        public bool showOnContractUpdate { get; set; }
        public string productCode { get; set; }
        public int maxPieces { get; set; }
        public bool showonWeb { get; set; }
        public bool showonWebsite { get; set; }
        public bool showonMobile { get; set; }
        public bool showonTabletForService { get; set; }
        public int? webRank { get; set; }
        public int? mobileRank { get; set; }
        public string productDescription { get; set; }
        public decimal? actualAmount { get; set; }
        public bool isChecked { get; set; }
        public int value { get; set; }
        public decimal actualTotalAmount { get; set; }
        public decimal paidAmount { get; set; }
        public decimal tobePaidAmount { get; set; }
        public bool isMandatory { get; set; }
        public int priceCalculationType { get; set; }
        public decimal monthlyPackagePrice { get; set; }
        public string webIconURL { get; set; }
        public string mobileIconURL { get; set; }
        public string currencySymbol { get; set; }
        public int? billingType { get; set; }
    }
}
