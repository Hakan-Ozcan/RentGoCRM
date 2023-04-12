using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.odata
{

    public class PriceDetail
    {
        public string mongodbId { get; set; }
        public string trackingNumber { get; set; }
        public DateTime priceDate { get; set; }
        public string userId { get; set; }
        public string userEntityLogicalName { get; set; }
        public decimal totalAmount { get; set; }
        public string selectedPriceListId { get; set; }
        public string selectedGroupCodePriceListId { get; set; }
        public decimal selectedGroupCodeAmount { get; set; }
        public string selectedAvailabilityPriceListId { get; set; }
        public decimal selectedAvailabilityPriceRate { get; set; }
        public string relatedGroupCodeId { get; set; }
        public string relatedGroupCodeName { get; set; }
        public decimal availabilityRate { get; set; }
        [Key]
        public Guid priceDetailId { get; set; }

    }
}
