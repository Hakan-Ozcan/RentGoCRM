using System;
namespace RntCar.ClassLibrary
{
    public class ContractItemResponse
    {
        public Guid contractItemId { get; set; }
        public int itemTypeCode { get; set; }
        public int status { get; set; }
    }
}
