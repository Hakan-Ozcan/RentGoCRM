using System;

namespace RntCar.ClassLibrary.MongoDB
{
    public class PriceListData
    {
        public string PriceListName { get; set; }
        public int Status { get; set; }
        public int State { get; set; }
        public int PriceType { get; set; }
        public string PriceListId { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public string PriceCodeId { get; set; }
        public Guid transactionCurrencyId { get; set; }
        public string transactionCurrencyName { get; set; }
        public string currencyCode { get; set; }
    }
}
