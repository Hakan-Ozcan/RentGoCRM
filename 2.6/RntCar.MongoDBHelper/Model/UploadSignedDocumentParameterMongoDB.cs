using MongoDB.Bson;
using RntCar.ClassLibrary;

namespace RntCar.MongoDBHelper.Model
{
    public class UploadSignedDocumentParameterMongoDB : UploadSignedDocumentParameter
    {
        public ObjectId _id { get; set; }
    }
}
