using System;
using System.Collections.Generic;

namespace RntCar.ClassLibrary.Report
{
    public class GetadditionalProductsforBranchResponse
    {
        public List<GetadditionalProductsforBranchData> data { get; set; } = new List<GetadditionalProductsforBranchData>();
        public List<GetadditionalProductsforBranchData> ownData { get; set; } = new List<GetadditionalProductsforBranchData>();
        public List<GetadditionalProductsforBranch_User> users { get; set; } = new List<GetadditionalProductsforBranch_User>();
        public bool showallRecords { get; set; }
    }
    public class GetadditionalProductsforBranchData
    {        
        public Guid additionalProductId { get; set; }
        public Guid userId { get; set; }
        public string additionalProductName { get; set; }
        public string fullName { get; set; }
        public int businessRoleCode { get; set; }
        public decimal amount { get; set; }
    }

    public class GetadditionalProductsforBranch_User
    {
        public string userName { get; set; }
        public Guid userId { get; set; }
    }
}
