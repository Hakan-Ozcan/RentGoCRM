using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using System;

namespace RntCar.BusinessLibrary.Business
{
    public class IndividualAddressBL : BusinessHandler
    {

        public IndividualAddressBL(GlobalEnums.ConnectionType enums = GlobalEnums.ConnectionType.default_Service) : base(enums)
        {
        }
        public IndividualAddressBL(IOrganizationService orgService) : base(orgService)
        {
        }
        public Guid createDefaultIndividualAddress(IndividualAddressCreateParameters individualAddressCreateParameters)
        {
            if(individualAddressCreateParameters.addressCountryId == Guid.Empty)
            {
                return Guid.Empty;
            }
            Entity individualAddress = new Entity("rnt_individualaddress");
            individualAddress["rnt_countryid"] = new EntityReference("rnt_country", individualAddressCreateParameters.addressCountryId);
            individualAddress["rnt_cityid"] = individualAddressCreateParameters.addressCityId.HasValue && individualAddressCreateParameters.addressCityId != Guid.Empty ?
                                              new EntityReference("rnt_city", individualAddressCreateParameters.addressCityId.Value) :
                                              null;
            if (individualAddressCreateParameters.addressDistrictId != null && individualAddressCreateParameters.addressDistrictId.HasValue)
                individualAddress["rnt_districtid"] = new EntityReference("rnt_district", individualAddressCreateParameters.addressDistrictId.Value);

            individualAddress["rnt_contactid"] = new EntityReference("contact", individualAddressCreateParameters.individualCustomerId);
            individualAddress["rnt_addressdetail"] = individualAddressCreateParameters.addressDetail;
            individualAddress["rnt_isdefaultaddress"] = true;
            return this.OrgService.Create(individualAddress);
        }

        public void updateDefaultIndividualAddress(IndividualAddresUpdateParameters individualAddresUpdateParameters)
        {
            Entity individualAddress = new Entity("rnt_individualaddress");
            individualAddress["rnt_countryid"] = new EntityReference("rnt_country", individualAddresUpdateParameters.addressCountryId);

            if (individualAddresUpdateParameters.addressCityId != null && individualAddresUpdateParameters.addressCityId.HasValue)
                individualAddress["rnt_cityid"] = new EntityReference("rnt_city", individualAddresUpdateParameters.addressCityId.Value);

            if (individualAddresUpdateParameters.addressDistrictId != null && individualAddresUpdateParameters.addressDistrictId.HasValue)
                individualAddress["rnt_districtid"] = new EntityReference("rnt_district", individualAddresUpdateParameters.addressDistrictId.Value);

            individualAddress["rnt_addressdetail"] = individualAddresUpdateParameters.addressDetail;
            individualAddress.Id = individualAddresUpdateParameters.individualAddressId;
            this.OrgService.Update(individualAddress);
        }

        public IndividualAddressResponse getIndividualAddressesByCustomerId(string customerId)
        {
            IndividualAddressRepository repository = new IndividualAddressRepository(this.OrgService);
            try
            {
                var data = repository.getIndividualAddressesByCustomerId(new Guid(customerId));
                return new IndividualAddressResponse
                {
                    ResponseResult = ResponseResult.ReturnSuccess(),
                    addressDatas = data
                };
            }
            catch (Exception ex)
            {
                return new IndividualAddressResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }
    }
}
