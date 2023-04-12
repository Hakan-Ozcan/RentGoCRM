using System.Collections.Generic;

namespace RntCar.ClassLibrary
{
    public class MaturityDifferenceRequest : RequestBase
    {
        public bool isUpdate { get; set; }
        public decimal equipmentPrice { get; set; }
        public string binNumber { get; set; }
        public int installmentNumber { get; set; }
        public List<AdditionalProductData> additionalProducts { get; set; }
    }
}
