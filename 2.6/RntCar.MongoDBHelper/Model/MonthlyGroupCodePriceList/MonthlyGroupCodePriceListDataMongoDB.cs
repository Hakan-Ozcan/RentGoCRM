using MongoDB.Bson;
using RntCar.ClassLibrary.MonthlyGroupCodePriceList;

namespace RntCar.MongoDBHelper.Model.MonthlyGroupCodePriceList
{
    public class MonthlyGroupCodePriceListDataMongoDB  : MonthlyGroupCodePriceListData
    {
        public ObjectId _id { get; set; }
    }
}
