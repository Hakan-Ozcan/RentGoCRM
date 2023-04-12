using MongoDB.Bson;
using RntCar.ClassLibrary.MonthlyPriceList;

namespace RntCar.MongoDBHelper.Model.MonthlyPriceList
{
    public class MonthlyPriceListDataMongoDB : MonthlyPriceListData
    {
        public ObjectId _id { get; set; }
    }
}
