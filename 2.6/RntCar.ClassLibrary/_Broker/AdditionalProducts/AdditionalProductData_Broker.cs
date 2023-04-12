using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Broker
{
    public class AdditionalProductData_Broker
    {
        /// <summary>
        /// Ek ürüne ait benzersiz id değeri
        /// </summary>
        public Guid productId { get; set; }
        /// <summary>
        /// Ek ürün adı
        /// </summary>
        public string productName { get; set; }
        /// <summary>
        /// Ek ürün türü [ 10: Services, 20: Accessories, 30: Fine ]
        /// </summary>
        public int productType { get; set; }
        /// <summary>
        /// Ek ürün kodu
        /// </summary>
        public string productCode { get; set; }
        /// <summary>
        /// Rezervasyona eklenebilecek maksimum adet sayısı
        /// </summary>
        public int maxPieces { get; set; }
        /// <summary>
        /// Ön yüzde gösterilmek istenen sıra numarası
        /// </summary>
        public int? webRank { get; set; }
        /// <summary>
        /// Ek ürün açıklaması
        /// </summary>
        public string productDescription { get; set; }
        public string webIconURL { get; set; }
        /// <summary>
        /// KDV dahil ek ürün tutar bilgisi
        /// </summary>
        public decimal? actualAmount { get; set; }
        public decimal? dailyAmount { get; set; }
        public int value { get; set; }
        public decimal tobePaidAmount { get; set; }
        public bool isMandatory { get; set; }
        /// <summary>
        /// 30 günlük paket fiyatı
        /// </summary>
        public decimal monthlyPackagePrice { get; set; }
        /// <summary>
        /// Ek ürün fiyat hesaplama türü. [ 1: Sabit fiyat, 2: Zamana bağlı fiyat ]
        /// </summary>
        public int priceCalculationType { get; set; }
    }
}
