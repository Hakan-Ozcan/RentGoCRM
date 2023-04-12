using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RntCar.ClassLibrary.GlobalEnums;

namespace RntCar.MongoDBHelper.Model
{
    public class TransactionHistoryDataMongoDB
    {
        public ProcessType processType { get; set; }
        public int documentNumber { get; set; }
        public DateTime beginingDate { get; set; }
        public DateTime endDate { get; set; }
        public DateTime processDate { get; set; }
        public decimal documentAmonunt { get; set; }
        public decimal paymentAmount { get; set; }
        public decimal deposit { get; set; }
        public string id { get; set; }
        public string name { get; set; }
    }
}
