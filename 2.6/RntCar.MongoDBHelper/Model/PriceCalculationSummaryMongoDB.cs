using MongoDB.Bson;
using RntCar.ClassLibrary;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RntCar.MongoDBHelper.Model
{
    public class PriceCalculationSummaryMongoDB : PriceCalculationSummaryData
    {
        public ObjectId _id { get; set; }     
        public string trackingNumber { get; set; }
        [NotMapped]
        public BsonTimestamp priceDateTimeStamp{ get; set; }
        [Key]
        public string ID { get; set; }
        //default false
        public bool isTickDay { get; set; } = false;
        //bu alanlar tick'in tick'ini hesaplarken kullanılır
        public decimal payNowWithoutTickDayAmount { get; set; }
        public decimal payLaterWithoutTickDayAmount { get; set; }
    }
}
