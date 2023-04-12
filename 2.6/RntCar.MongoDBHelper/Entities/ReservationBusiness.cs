using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Entities
{
    public class ReservationBusiness : MongoDBInstance
    {
        public ReservationBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }
    }
}
