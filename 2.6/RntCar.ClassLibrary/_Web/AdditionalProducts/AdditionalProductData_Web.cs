using System;

namespace RntCar.ClassLibrary._Web
{
    public class AdditionalProductData_Web
    {
        public Guid productId { get; set; }
        public string productName { get; set; }
        public int productType { get; set; }
        public string productCode { get; set; }
        public int maxPieces { get; set; }
        public int? webRank { get; set; }
        public string productDescription { get; set; }
        public string webIconURL { get; set; }
        public decimal? actualAmount { get; set; }
        public decimal? actualTotalAmount { get; set; }
        public int value { get; set; }
        public decimal tobePaidAmount { get; set; }
        public bool isMandatory { get; set; }
        public decimal monthlyPackagePrice { get; set; }
        public int priceCalculationType { get; set; }
    }
}
