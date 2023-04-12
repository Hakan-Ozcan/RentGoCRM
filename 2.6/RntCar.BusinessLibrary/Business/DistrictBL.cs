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
    public class DistrictBL : BusinessHandler
    {
        public DistrictBL(GlobalEnums.ConnectionType enums = GlobalEnums.ConnectionType.default_Service) : base(enums)
        {
        }

        public DistrictBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public List<DistrictData> getDistricts()
        {
            List<DistrictData> data = new List<DistrictData>();
            DistrictRepository repository = new DistrictRepository(this.OrgService, this.CrmServiceClient);


            var collection = repository.GetActiveDistricts();

            foreach (var item in collection.Entities)
            {
                DistrictData d = new DistrictData
                {
                    cityId = item.Attributes.Contains("rnt_cityid") ? Convert.ToString(item.GetAttributeValue<EntityReference>("rnt_cityid").Id) : string.Empty,
                    districtId = Convert.ToString(item.Id),
                    districtName = item.GetAttributeValue<string>("rnt_name")
                };
                data.Add(d);
            }

            return data;
        }
    }
}
