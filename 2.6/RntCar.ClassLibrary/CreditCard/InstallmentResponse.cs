using System.Collections.Generic;

namespace RntCar.ClassLibrary
{
    public class InstallmentResponse : ResponseBase
    {
        public List<List<InstallmentData>> installmentData { get; set; }
    }
}
