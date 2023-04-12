using Microsoft.Xrm.Sdk;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.SDK.Mappers
{
    public class TaxOfficeMapper
    {
        public List<TaxOfficeData> createTaxOfficeList(List<Entity> creditCards)
        {
            var convertedData = creditCards.ConvertAll(item => new TaxOfficeData
            {
                cityId = item.Attributes.Contains("rnt_cityid") ? (Guid?)item.GetAttributeValue<EntityReference>("rnt_cityid").Id : null,
                cityName = item.Attributes.Contains("rnt_cityid") ? item.GetAttributeValue<EntityReference>("rnt_cityid").Name : null,
                taxOfficeId = item.Id,
                taxOfficeName = item.GetAttributeValue<string>("rnt_name")
            });
            return convertedData;
        }
    }
}
