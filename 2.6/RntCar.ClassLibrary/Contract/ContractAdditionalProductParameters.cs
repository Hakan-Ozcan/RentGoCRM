using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ContractAdditionalProductParameters
    {
        public Guid productId { get; set; }
        public string productName { get; set; }
        public int productType { get; set; }
        public string productCode { get; set; }
        public int maxPieces { get; set; }
        public int totalDuration { get; set; }
        public decimal? actualTotalAmount { get; set; }
        public decimal? actualAmount { get; set; }
        public int value { get; set; }
        public int priceCalculationType { get; set; }
        public decimal monthlyPackagePrice { get; set; }
        public int? billingType { get; set; }
    }
}
