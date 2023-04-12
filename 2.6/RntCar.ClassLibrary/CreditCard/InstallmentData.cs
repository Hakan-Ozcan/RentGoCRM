using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class InstallmentData
    {
        public decimal totalAmount { get; set; }
        public int installmentNumber { get; set; }
        public decimal installmentAmount { get; set; }
        public int virtualPosId { get; set; }
        public string bankName { get; set; }

    }
}
