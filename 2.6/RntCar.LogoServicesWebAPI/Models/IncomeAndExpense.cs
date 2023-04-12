using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RntCar.LogoServicesWebAPI.Models
{
    public class IncomeAndExpense
    {
        public string GELIR_GIDER { get; set; }
        public string KEBIR_KODU { get; set; }
        public string KEBIR_ADI { get; set; }
        public string ANA_GRUPKODU { get; set; }
        public string ANA_GRUPADI { get; set; }
        public string HESAP_KODU { get; set; }
        public string HESAP_ADI { get; set; }
        public string HESAP_OZELKODU { get; set; }
        public string MM_KODU { get; set; }
        public string MM_ADI { get; set; }
        public string MM_OZELKODU { get; set; }
        public string PRJ_KODU { get; set; }
        public string PRJ_ADI { get; set; }
        public string PRJ_OZELKODU { get; set; }
        public int AY { get; set; }
        public int YIL { get; set; }
        public string DONEM { get; set; }
        public string DVZ { get; set; }
        public decimal BORC { get; set; }
        public decimal ALACAK { get; set; }
        public decimal BAKIYE { get; set; }
        public string FISNO { get; set; }
        public int FIS_TURU { get; set; }
        public DateTime TARIH { get; set; }
        public int ISYERI { get; set; }
        public string ISYERI_ADI { get; set; }
        public string LINEEXP { get; set; }
        public int YEVMIYENO { get; set; }
        public string BELGENO { get; set; }
        public int LOGICALREF { get; set; }
    }
}