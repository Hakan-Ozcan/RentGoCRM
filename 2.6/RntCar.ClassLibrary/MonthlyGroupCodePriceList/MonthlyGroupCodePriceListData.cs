namespace RntCar.ClassLibrary.MonthlyGroupCodePriceList
{
    public class MonthlyGroupCodePriceListData
    {
        public string name { get; set; }
        public string monthlyGroupCodeListId { get; set; }
        public string groupCodeId { get; set; }
        public string groupCodeName { get; set; }
        public string monthlyPriceListId { get; set; }
        public string monthlyPriceListName { get; set; }
        public int month { get; set; }
        public decimal amount { get; set; }
        public int stateCode { get; set; }
        public int statusCode { get; set; }
    }
}
