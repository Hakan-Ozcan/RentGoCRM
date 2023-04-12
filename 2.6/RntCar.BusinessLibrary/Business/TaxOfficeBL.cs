using Microsoft.Xrm.Sdk;
using RestSharp;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class TaxOfficeBL : BusinessHandler
    {
        public TaxOfficeBL(GlobalEnums.ConnectionType enums = GlobalEnums.ConnectionType.default_Service) : base(enums)
        {
        }

        public TaxOfficeBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public List<TaxOfficeData> getTaxOfficesByCache(string taxOfficeKey)
        {
            List<TaxOfficeData> data = new List<TaxOfficeData>();
            TaxOfficeRepository repository = new TaxOfficeRepository(this.OrgService);

            if (CacheHelper.IsExist(taxOfficeKey))
            {
                data = CacheHelper.Get<List<TaxOfficeData>>(taxOfficeKey);
            }
            else
            {
                var result = repository.getActiveTaxOffices();

                foreach (var item in result)
                {
                    TaxOfficeData d = new TaxOfficeData
                    {
                        cityId = item.Attributes.Contains("rnt_cityid") ? (Guid?)item.GetAttributeValue<EntityReference>("rnt_cityid").Id : null,
                        cityName = item.Attributes.Contains("rnt_cityid") ? item.GetAttributeValue<EntityReference>("rnt_cityid").Name : null,
                        taxOfficeId = item.Id,
                        taxOfficeName = item.GetAttributeValue<string>("rnt_name")
                    };
                    data.Add(d);
                }
                //Todo time can be configurable
                CacheHelper.Set(taxOfficeKey, data, 2880);
            }

            return data;
        }
    }
}
