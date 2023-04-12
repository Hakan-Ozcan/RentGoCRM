using RntCar.LogoServicesWebAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace RntCar.LogoServicesWebAPI.Helpers
{
    public static class ReportHelper
    {

        public static List<IncomeAndExpense> ConvertListIncomeAndExpense(DataTable dt)
        {
            var returnValue = new List<IncomeAndExpense>();

            try
            {
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        IncomeAndExpense listItem = new IncomeAndExpense();

                        listItem.GELIR_GIDER = dt.Rows[i]["GELIR_GIDER"] != DBNull.Value ? dt.Rows[i]["GELIR_GIDER"].ToString() : "";
                        listItem.KEBIR_KODU = dt.Rows[i]["KEBIR_KODU"] != DBNull.Value ? dt.Rows[i]["KEBIR_KODU"].ToString() : "";
                        listItem.KEBIR_ADI = dt.Rows[i]["KEBIR_ADI"] != DBNull.Value ? dt.Rows[i]["KEBIR_ADI"].ToString() : "";
                        listItem.ANA_GRUPKODU = dt.Rows[i]["ANA_GRUPKODU"] != DBNull.Value ? dt.Rows[i]["ANA_GRUPKODU"].ToString() : "";
                        listItem.ANA_GRUPADI = dt.Rows[i]["ANA_GRUPADI"] != DBNull.Value ? dt.Rows[i]["ANA_GRUPADI"].ToString() : "";
                        listItem.HESAP_KODU = dt.Rows[i]["HESAP_KODU"] != DBNull.Value ? dt.Rows[i]["HESAP_KODU"].ToString() : "";
                        listItem.HESAP_ADI = dt.Rows[i]["HESAP_ADI"] != DBNull.Value ? dt.Rows[i]["HESAP_ADI"].ToString() : "";
                        listItem.HESAP_OZELKODU = dt.Rows[i]["HESAP_OZELKODU"] != DBNull.Value ? dt.Rows[i]["HESAP_OZELKODU"].ToString() : "";
                        listItem.MM_KODU = dt.Rows[i]["MM_KODU"] != DBNull.Value ? dt.Rows[i]["MM_KODU"].ToString() : "";
                        listItem.MM_ADI = dt.Rows[i]["MM_ADI"] != DBNull.Value ? dt.Rows[i]["MM_ADI"].ToString() : "";
                        listItem.MM_OZELKODU = dt.Rows[i]["MM_OZELKODU"] != DBNull.Value ? dt.Rows[i]["MM_OZELKODU"].ToString() : "";
                        listItem.PRJ_KODU = dt.Rows[i]["PRJ_KODU"] != DBNull.Value ? dt.Rows[i]["PRJ_KODU"].ToString() : "";
                        listItem.PRJ_ADI = dt.Rows[i]["PRJ_ADI"] != DBNull.Value ? dt.Rows[i]["PRJ_ADI"].ToString() : "";
                        listItem.PRJ_OZELKODU = dt.Rows[i]["PRJ_OZELKODU"] != DBNull.Value ? dt.Rows[i]["PRJ_OZELKODU"].ToString() : "";
                        listItem.DONEM = dt.Rows[i]["DONEM"] != DBNull.Value ? dt.Rows[i]["DONEM"].ToString() : "";
                        listItem.DVZ = dt.Rows[i]["DVZ"] != DBNull.Value ? dt.Rows[i]["DVZ"].ToString() : "";
                        listItem.FISNO = dt.Rows[i]["FISNO"] != DBNull.Value ? dt.Rows[i]["FISNO"].ToString() : "";
                        listItem.ISYERI_ADI = dt.Rows[i]["ISYERI_ADI"] != DBNull.Value ? dt.Rows[i]["ISYERI_ADI"].ToString() : "";
                        listItem.LINEEXP = dt.Rows[i]["LINEEXP"] != DBNull.Value ? dt.Rows[i]["LINEEXP"].ToString() : "";
                        listItem.BELGENO = dt.Rows[i]["BELGE NO"] != DBNull.Value ? dt.Rows[i]["BELGE NO"].ToString() : "";

                        listItem.AY = dt.Rows[i]["AY"] != DBNull.Value ? (int)dt.Rows[i]["AY"] : 0;
                        listItem.YIL = dt.Rows[i]["YIL"] != DBNull.Value ? (int)dt.Rows[i]["YIL"] : 0;
                        listItem.ISYERI = dt.Rows[i]["ISYERI"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["ISYERI"]): 0;
                        listItem.FIS_TURU = dt.Rows[i]["FIS_TURU"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["FIS_TURU"]) : 0;
                        listItem.YEVMIYENO = dt.Rows[i]["YEVMIYE NO"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["YEVMIYE NO"]) : 0;
                        listItem.LOGICALREF = dt.Rows[i]["LOGICALREF"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["LOGICALREF"]) : 0;


                        listItem.BORC = dt.Rows[i]["BORC"] != DBNull.Value ? Convert.ToDecimal(dt.Rows[i]["BORC"]) : 0;
                        listItem.BAKIYE = dt.Rows[i]["BAKIYE"] != DBNull.Value ? Convert.ToDecimal(dt.Rows[i]["BAKIYE"]) : 0;
                        listItem.ALACAK = dt.Rows[i]["ALACAK"] != DBNull.Value ? Convert.ToDecimal(dt.Rows[i]["ALACAK"]) : 0;


                        listItem.TARIH = (DateTime)dt.Rows[i]["TARIH"];

                        returnValue.Add(listItem);
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return returnValue;
        }
    }
}