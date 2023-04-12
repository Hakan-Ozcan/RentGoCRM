using System;

namespace RntCar.ClassLibrary
{

    public class ResponseResult
    {
 
        public bool Result { get; set; }

        // Tolga AYKURT - 27.02.2019
        public String ExceptionDetail { get; set; }

       
        public string Id { get; set; }

        public static ResponseResult ReturnSuccess()
        {
            return new ResponseResult{ Result = true };
        }

        public static ResponseResult ReturnError(String Detail)
        {
            return new ResponseResult { Result = false, ExceptionDetail = Detail };
        }

        public static ResponseResult ReturnSuccessWithId(string id)
        {
            return new ResponseResult { Result = true, Id = id };
        }

    }
}
