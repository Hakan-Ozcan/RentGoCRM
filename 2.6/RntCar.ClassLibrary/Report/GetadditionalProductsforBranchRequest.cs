using System.Collections.Generic;

namespace RntCar.ClassLibrary.Report
{
    public class GetadditionalProductsforBranchRequest
    {
        public string id { get; set; }
        public int month { get; set; }
    }
    public class GetadditionalProductsforBranchMainRequest
    {       
        public int month { get; set; }
        public List<GetadditionalProductsforBranchMainRequest_PikupBranch> pickupbranchId { get; set; }
    }
    public class GetadditionalProductsforBranchMainRequest_PikupBranch
    {
        public string value { get; set; }
        public string label { get; set; }
    }
}
