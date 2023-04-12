using System.Collections.Generic;

namespace RntCar.ClassLibrary._Web
{
    public class RetrieveInstallmentsResponse_Web : ResponseBase
    {
        public AdditionalProductData_Web additionalProduct { get; set; }
        public List<InstallmentData_Web> installmentData  { get; set; }
    }
}
