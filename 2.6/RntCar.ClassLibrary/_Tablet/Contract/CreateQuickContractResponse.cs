using System;

namespace RntCar.ClassLibrary._Tablet
{
    public class CreateQuickContractResponse : ResponseBase
    {
        public Guid contractId { get; set; }
        public string contractPNR { get; set; }
    }
}
