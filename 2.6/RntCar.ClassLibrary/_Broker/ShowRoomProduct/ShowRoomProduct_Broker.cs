using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Broker
{
    public class ShowRoomProduct_Broker
    { 
        public string name { get; set; }
        public string webURL { get; set; }
        public bool abs { get; set; }
        public bool airbag { get; set; }
        public bool cruiseControl { get; set; }
        public int bodyType { get; set; }
        public string bodyTypeName { get; set; }
        public decimal carbonEmission { get; set; }
        public string engineVolume { get; set; }
        public int fuelType { get; set; }
        public string fuelTypeName { get; set; }
        public decimal fuelConsumption { get; set; }
        public int gearbox { get; set; }
        public string gearboxName { get; set; }
        public Guid groupCodeInformationId { get; set; }
        public int numberofDoors { get; set; }
        public string numberofDoorsName { get; set; }
        public int numberofPerson { get; set; }
        public string numberofPersonName { get; set; }
        public string productDescription { get; set; }
        public string seoDescription { get; set; }
        public string seoTitle { get; set; }
        public string seoKeyword { get; set; }
        public string model { get; set; }
        public int trunkVolume { get; set; }
    }
}
