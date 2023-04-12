using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class BranchAvailabilityReport
    {
        public float branchAvailabilityRatio { get; set; }
        public List<BranchAvailabilityByGroupCode> branchAvailabilityByGroupCode { get; set; }
    }
    public class BranchAvailabilityByGroupCode
    {
        public string groupCodeInformationId { get; set; }
        public string groupCodeInformationName { get; set; }
        public int allEquipmentsCount { get; set; }
        public int rentalEquipmentsCount { get; set; }
        public decimal availabilityRatio { get; set; }
    }
}
