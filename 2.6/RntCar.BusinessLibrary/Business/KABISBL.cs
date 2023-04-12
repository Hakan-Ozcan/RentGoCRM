using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class KABISBL : BusinessHandler
    {
        public KABISBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public KABISBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public KABISBL(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService) : base(initializer, orgService, tracingService)
        {
        }

        public KABISIntegrationUser GetKABISInfo(EntityReference branchRef)
        {
            KABISIntegrationUser kABISIntegrationUser = new KABISIntegrationUser();
            Entity branch = OrgService.Retrieve(branchRef.LogicalName, branchRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("rnt_kabisuser", "rnt_kabispassword", "rnt_emailaddress"));
            kABISIntegrationUser.userName = branch.GetAttributeValue<string>("rnt_kabisuser");
            kABISIntegrationUser.Password = branch.GetAttributeValue<string>("rnt_kabispassword");
            kABISIntegrationUser.officeEMail = branch.GetAttributeValue<string>("rnt_emailaddress");
            return kABISIntegrationUser;
        }

        public KABISCustomer GetKABISCustomerInfo(EntityReference customerRef)
        {
            KABISCustomer kABISCustomer = new KABISCustomer();
            Entity customer = OrgService.Retrieve(customerRef.LogicalName, customerRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("rnt_isturkishcitizen", "governmentid", "rnt_citizenshipid", "firstname", "lastname", "rnt_drivinglicensenumber", "birthdate"));
            bool _isTurkishCitizen = customer.GetAttributeValue<bool>("rnt_isturkishcitizen");
            if (_isTurkishCitizen)
            {
                kABISCustomer.Type = "TC";
            }
            else
            {
                kABISCustomer.Type = "YU";
                kABISCustomer.Name = customer.GetAttributeValue<string>("firstname");
                kABISCustomer.Surname = customer.GetAttributeValue<string>("lasttname");
                //kABISCustomer.FatherName = string.Empty;
                //kABISCustomer.MotherName = string.Empty;
                //kABISCustomer.PlaceOfBirth = string.Empty;
                kABISCustomer.YearOfBirth = customer.GetAttributeValue<DateTime>("birthdate").Year;
            }

            kABISCustomer.Identity = customer.GetAttributeValue<string>("governmentid");


            return kABISCustomer;
        }
        public KABISCustomer GetKABISCustomerInfoByLinkEntity(Entity customerLinkEntity)
        {
            KABISCustomer kABISCustomer = new KABISCustomer();
            EntityReference customerDrivingCountryRef = new EntityReference();

            bool _isTurkishCitizen = Convert.ToBoolean(customerLinkEntity.GetAttributeValue<AliasedValue>("customerAlias.rnt_isturkishcitizen").Value);
            var _turkeyCountryId = StaticHelper.turkeyCountryId;
            var customerDrivingCountryAlias = customerLinkEntity.GetAttributeValue<AliasedValue>("customerAlias.rnt_drivinglicensecountryid");
            if (customerDrivingCountryAlias != null)
            {
                customerDrivingCountryRef = (EntityReference)(customerDrivingCountryAlias.Value);
            }


            if (_isTurkishCitizen)
            {
                if (customerDrivingCountryRef.Id == Guid.Empty || customerDrivingCountryRef.Id == _turkeyCountryId)
                {
                    kABISCustomer.Type = "TC";
                    kABISCustomer.Name = customerLinkEntity.GetAttributeValue<AliasedValue>("customerAlias.firstname").Value.ToString();
                    kABISCustomer.Surname = customerLinkEntity.GetAttributeValue<AliasedValue>("customerAlias.lastname").Value.ToString();
                    kABISCustomer.Identity = customerLinkEntity.GetAttributeValue<AliasedValue>("customerAlias.governmentid").Value.ToString();
                }
                else
                {
                    kABISCustomer.Type = "TY";
                    kABISCustomer.Identity = customerLinkEntity.GetAttributeValue<AliasedValue>("customerAlias.governmentid").Value.ToString();
                    kABISCustomer.Nationality = customerLinkEntity.GetAttributeValue<AliasedValue>("drivingCountryAlias.rnt_kabiscode").Value.ToString();
                    kABISCustomer.LicenseNo = customerLinkEntity.GetAttributeValue<AliasedValue>("customerAlias.rnt_drivinglicensenumber").Value.ToString();
                }
            }
            else
            {
                kABISCustomer.Type = "YU";
                kABISCustomer.Name = customerLinkEntity.GetAttributeValue<AliasedValue>("customerAlias.firstname").Value.ToString();
                kABISCustomer.Surname = customerLinkEntity.GetAttributeValue<AliasedValue>("customerAlias.lastname").Value.ToString();
                kABISCustomer.YearOfBirth = Convert.ToDateTime(customerLinkEntity.GetAttributeValue<AliasedValue>("customerAlias.birthdate").Value).Year;
                kABISCustomer.Identity = customerLinkEntity.GetAttributeValue<AliasedValue>("customerAlias.rnt_passportnumber").Value.ToString();
                kABISCustomer.Nationality = customerLinkEntity.GetAttributeValue<AliasedValue>("countryAlias.rnt_kabiscode").Value.ToString();


                //kABISCustomer.FatherName = string.Empty;
                //kABISCustomer.MotherName = string.Empty;
                //kABISCustomer.PlaceOfBirth = string.Empty;
            }



            return kABISCustomer;
        }

        public int GetCurrentKm(EntityReference equipmentRef)
        {
            Entity equipment = OrgService.Retrieve(equipmentRef.LogicalName, equipmentRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("rnt_currentkm"));
            return equipment.GetAttributeValue<int>("rnt_currentkm");
        }
    }
}
