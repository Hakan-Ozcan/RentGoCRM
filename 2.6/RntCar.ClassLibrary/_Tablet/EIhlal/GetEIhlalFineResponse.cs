using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class GetEIhlalFineResponse : ResponseBase
    {
        public List<EIhlalFineData> fineList { get; set; }
        public List<AdditonalProductDataTablet> fineAdditionalProducts { get; set; }
    }
}
