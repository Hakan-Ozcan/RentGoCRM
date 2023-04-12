using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.t
{
    public class GetEIhlalFineResponse : ResponseBase
    {
        public List<EIhlalHighwayFineData> eihlalHighwayFineList { get; set; }
        public List<EIhlalTrafficFineData> eihlalTrafficFineList { get; set; }
    }
}
