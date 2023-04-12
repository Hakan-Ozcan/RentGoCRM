using System.Collections.Generic;

namespace RntCar.ClassLibrary._Mobile
{
    public class RetrieveInstallmentsResponse_Mobile : ResponseBase
    {
        public AdditionalProductData_Mobile additionalProduct { get; set; }
        public List<InstallmentData_Mobile> installmentData  { get; set; }
    }
}
