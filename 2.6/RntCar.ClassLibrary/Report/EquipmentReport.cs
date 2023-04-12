using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class EquipmentReport
    {
        public int equipmentCount { get; set; }
        public List<EquipmentDetailCount> equipmentDetailCount { get; set; }
        public List<EquipmentDetailCountByGroupCode> equipmentDetailCountByGroupCode { get; set; }

    }
    public class EquipmentDetailCount
    {
        public int status { get; set; }//available , rental , transfer
        public int count { get; set; }
    }
    public class EquipmentDetailCountByGroupCode
    {
        public Guid groupCodeInformationId { get; set; }
        public string groupCodeInformationName { get; set; }
        public string fuelTypeName { get; set; }
        public string transmissionTypeName { get; set; }
        public int availableCount { get; set; }
        public int rentalCount { get; set; }
        public int otherCount { get; set; }
        public List<EquipmentDetail> equipmentDetail { get; set; }
    }
    public class EquipmentDetail
    {
        public Guid equipmentId { get; set; }
        public string modelName { get; set; }
        public string brandName { get; set; }
        public string plateNumber { get; set; }
        public bool isEquipmentInTransfer { get; set; }
        public int transferType { get; set; }
        public string transferTypeName { get; set; }
        public int status { get; set; }
        public string statusName { get; set; }
    }
}
