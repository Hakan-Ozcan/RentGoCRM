namespace RntCar.ClassLibrary.MongoDB
{
    public class MongoDBResponse
    {
        public bool Result { get; set; }
        public string ExceptionDetail { get; set; }
        public string campaignId { get; set; }
        public string Id { get; set; }

        public static MongoDBResponse ReturnSuccess()
        {
            return new MongoDBResponse { Result = true };
        }

        public static MongoDBResponse ReturnError(string Detail)
        {
            return new MongoDBResponse { Result = false, ExceptionDetail = Detail };
        }
        public static MongoDBResponse ReturnSuccessWithId(string id)
        {
            return new MongoDBResponse { Result = true, Id = id };
        }
    }
}
