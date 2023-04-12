using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class TaxOfficeData
    {
        public string taxOfficeName { get; set; }
        public Guid taxOfficeId{ get; set; }
        public Guid? cityId { get; set; }
        public string cityName { get; set; }

    }
}
