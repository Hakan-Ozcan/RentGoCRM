using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class CreateBonusCalculationLogParameters
    {
        public Guid contractId { get; set; }
        public Guid contractItemId { get; set; }
        public Guid? userId { get; set; }
        public Guid? externalUserId { get; set; }
        public Guid? businessUnitId { get; set; }
        public int? businessRoleCode { get; set; }
        public decimal netAmount { get; set; }
        public decimal bonusAmount { get; set; }

    }
}
