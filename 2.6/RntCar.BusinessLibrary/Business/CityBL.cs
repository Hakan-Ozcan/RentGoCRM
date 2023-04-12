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
    public class CityBL : BusinessHandler
    {
        public CityBL(GlobalEnums.ConnectionType enums = GlobalEnums.ConnectionType.default_Service) : base(enums)
        {
        }
        public CityBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public List<CityData> GetCitiesWithCache(string cityKey)
        {
            List<CityData> data = new List<CityData>();
            CityRepository repository = new CityRepository(this.OrgService);

            if (CacheHelper.IsExist(cityKey))
            {
                data = CacheHelper.Get<List<CityData>>(cityKey);
            }
            else
            {
                data = repository.GetActiveCities();
                //Todo time can be configurable
                CacheHelper.Set(cityKey, data, 2880);
            }

            return data;
        }
    }
}
