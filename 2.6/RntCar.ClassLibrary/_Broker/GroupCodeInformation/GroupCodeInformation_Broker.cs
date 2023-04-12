using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Broker
{
    public class GroupCodeInformation_Broker
    {
        /// <summary>
        /// Grup kodu adı
        /// </summary>
        public string groupCodeName { get; set; }
        /// <summary>
        /// Grup koda ait benzersiz id değeri
        /// </summary>
        public Guid groupCodeId { get; set; }
        /// <summary>
        /// Grup kod açıklaması. Örnek: Nissan Micra ve benzeri
        /// </summary>
        public string groupCodeDescription { get; set; }
        /// <summary>
        /// Vites tipi adı
        /// </summary>
        public string transmissionName { get; set; }
        /// <summary>
        /// Yakıt tipi adı
        /// </summary>
        public string fuelTypeName { get; set; }
        /// <summary>
        /// Aracı alabilmek için gereken minimum findeks puanı
        /// </summary>
        public int? findeksPoint { get; set; }
        /// <summary>
        /// Aracı kiralamak için çift kredi kartı gerekiyor mu
        /// </summary>
        public bool isDoubleCard { get; set; }
        /// <summary>
        /// Araç grubunu kiralamak için gerekli minimum yaş sınırı
        /// </summary>
        public int minimumAge { get; set; }
        /// <summary>
        /// Araç grubunu kiralamak için gerekli minimum sürücü belgesi yılı
        /// </summary>
        public int minimumDriverLicense { get; set; }
        /// <summary>
        /// Ek sigorta paketleri ile beraber araç grubunu kiralayabilmek için gerekli minimum sürücü yaşı
        /// </summary>
        public int youngDriverAge { get; set; }
        /// <summary>
        /// Ek sigorta paketleri ile beraber araç grubunu kiralayabilmek için gerekli minimum sürücü ehliyeti. Sürücü genç sürücü yaşı ve genç sürücü belgesi yılından bir tanesini bile karşılamaması durumunda ilgili araç grubunu kiralayamaz. Genç sürücü yaşı ve ehliyet yılı karşılanıyorsa ek paketlerle beraber araç grubunu kiralayabilir
        /// </summary>
        public int youngDriverMinimumLicense { get; set; }
        /// <summary>
        /// Araç kiralama sırasında müşteriden alınacak teminat tutarı
        /// </summary>
        public decimal? depositAmount { get; set; }
        /// <summary>
        /// Vites tipi [ 1: Manuel, 2: Automatic ]
        /// </summary>
        public int transmission { get; set; }
        /// <summary>
        /// Yakıt tipi [ 1: Diesel, 2: Gasoline ]
        /// </summary>
        public int fuelType { get; set; }
        /// <summary>
        /// İlgili araç grubu segment tipi [ 10: Economy - Hatchback, 20: Economy - Sedan, 30: Mid-size - Hatchback, 40: Mid-size - Sedan, 50: Mid-size + Hatchback, 60: Mid-size + Sedan, 70: Luxury -Sedan, 80: Premium - Sedan, 90: SUV, 100: VAN ]
        /// </summary>
        public int segment { get; set; }
        /// <summary>
        /// İlgili araç grubu segment tipi adı
        /// </summary>
        public string segmentName { get; set; }
        /// <summary>
        /// İlgili araç grubu için belirlenen marka
        /// </summary>
        public string showRoomBrandName { get; set; }
        /// <summary>
        /// İlgili araç grubu için belirlenen model
        /// </summary>
        public string showRoomModelName { get; set; }
        /// <summary>
        /// İlgili araç grubu URL
        /// </summary>
        public string webImageURL { get; set; }

        public string SIPPCode { get; set; }
    }
}
