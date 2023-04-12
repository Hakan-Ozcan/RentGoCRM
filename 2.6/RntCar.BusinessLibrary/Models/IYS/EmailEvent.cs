using Newtonsoft.Json;

namespace RntCar.BusinessLibrary.Models.IYS
{
    public class EmailEvent
    {
        [JsonProperty("timestamp")]
        public long TimeStamp { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("event")]
        public string Event { get; set; }
        [JsonProperty("msg-id")]
        public string MessageId { get; set; }
        [JsonProperty("campaign_name")]
        public string CampaignName { get; set; }
        [JsonProperty("variation_id")]
        public int VariationId { get; set; }
        [JsonProperty("iid")]
        public string IID { get; set; }
        [JsonProperty("subject")]
        public string Subject { get; set; }
    }
}
