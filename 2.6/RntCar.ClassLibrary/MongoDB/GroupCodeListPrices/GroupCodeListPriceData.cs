using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.MongoDB
{
    public class GroupCodeListPriceData
    {
        public string GroupCodeListPriceId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string GroupCodeInformationId { get; set; }
        public string GroupCodeInformationName { get; set; }
        public int MinimumDay { get; set; }
        public int MaximumDay { get; set; }
        public decimal ListPrice { get; set; }
        public string PriceListId { get; set; }
        public string PriceListName { get; set; }
        public int Status { get; set; }
        public int State { get; set; }


    }
}
