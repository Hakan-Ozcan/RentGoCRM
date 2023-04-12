using System;

namespace RntCar.ClassLibrary._Web
{
    public class ResponseResult
    {
        public bool result { get; set; }
        public String exceptionDetail { get; set; }


        public static ResponseResult ReturnSuccess()
        {
            return new ResponseResult { result = true };
        }

        public static ResponseResult ReturnError(String Detail)
        {
            if(Detail.Contains("Object reference not set to an instance of an object"))
            {
                return new ResponseResult { result = false, exceptionDetail = "Sistemsel bir hata oluştu. Lütfen rezervasyon merkezi ile iletişime geçiniz." };
            }

            return new ResponseResult { result = false, exceptionDetail = Detail };
        }
    }
}
