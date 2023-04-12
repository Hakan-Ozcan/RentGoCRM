using MongoDB.Bson;
using RntCar.ClassLibrary.MongoDB;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Model
{
    public class CouponCodeDataMongoDB : CouponCodeData
    {
        [Key]
        public string ID { get; set; }

        public ObjectId _id { get; set; }
    }
}
