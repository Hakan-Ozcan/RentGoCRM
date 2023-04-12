using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.MongoDB
{
    public class OneWayFeeData
    {
        public string OneWayFeeId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string PickUpBranchId { get; set; }
        public string PickUpBranchName{ get; set; }
        public string DropoffBranchId { get; set; }
        public string DropoffBranchName { get; set; }
        public decimal Price { get; set; }
        public int StatusCode { get; set; }
        public int StateCode { get; set; }
        public DateTime beginDate { get; set; }
        public DateTime endDate { get; set; }
        public bool isEnabled { get; set; }

    }
}
