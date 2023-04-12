using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.Reservation
{
    public class ReservationAdditionalProductParameters
    {
        public Guid productId { get; set; }
        public string productName { get; set; }
        public int productType { get; set; }
        public string productCode { get; set; }
        public int maxPieces { get; set; }
        public int totalDuration { get; set; }
        public decimal? actualTotalAmount { get; set; }
        public decimal? actualAmount { get; set; }
        public int value { get; set; }
        public int priceCalculationType { get; set; }
        public decimal monthlyPackagePrice { get; set; }
        public int? billingType { get; set; }

        public ReservationAdditionalProductParameters buildReservationAdditionalProductParameters(Entity additionalProduct, decimal amount)
        {

            return new ReservationAdditionalProductParameters
            {
                actualTotalAmount = amount ,
                actualAmount = amount,
                maxPieces = 1,
                value = 1,
                productId = additionalProduct.Id,
                priceCalculationType = additionalProduct.GetAttributeValue<OptionSetValue>("rnt_pricecalculationtypecode").Value,
                monthlyPackagePrice = decimal.Zero,
                productName = additionalProduct.GetAttributeValue<string>("rnt_name")                
            };
        }
    }
}
