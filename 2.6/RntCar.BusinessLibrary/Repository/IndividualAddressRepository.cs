using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Repository
{
    public class IndividualAddressRepository : RepositoryHandler
    {
        public IndividualAddressRepository(IOrganizationService Service) : base(Service)
        {
        }

        public IndividualAddressRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public IndividualAddressRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public Entity getIndividualAddresById(Guid id)
        {
            return this.retrieveById("rnt_individualaddress", id, true);
        }

        public Entity getIndividualAddresByIdWithGivenColumns(Guid id, string[] columns)
        {
            return this.retrieveById("rnt_individualaddress", id, columns);
        }
        public Entity getDefaultIndividualAddressByCustomerId(Guid individualCustomerId)
        {
            QueryExpression expression = new QueryExpression("rnt_individualaddress");
            expression.ColumnSet = new ColumnSet(true);
            expression.Criteria.AddCondition("rnt_contactid", ConditionOperator.Equal, individualCustomerId);
            expression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            expression.Criteria.AddCondition("rnt_isdefaultaddress", ConditionOperator.Equal, true);
            return this.retrieveMultiple(expression).Entities.FirstOrDefault();
        }
        public Entity getDefaultIndividualAddressByCustomerIdByGivenColumns(Guid individualCustomerId, string[] columns)
        {
            QueryExpression expression = new QueryExpression("rnt_individualaddress");
            expression.ColumnSet = new ColumnSet(columns);
            expression.Criteria.AddCondition("rnt_contactid", ConditionOperator.Equal, individualCustomerId);
            expression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            expression.Criteria.AddCondition("rnt_isdefaultaddress", ConditionOperator.Equal, true);
            return this.retrieveMultiple(expression).Entities.FirstOrDefault();
        }
        public List<IndividualAddressData> getIndividualAddressesByCustomerId(Guid customerId)
        {
            List<IndividualAddressData> data = new List<IndividualAddressData>();
            QueryExpression query = new QueryExpression("rnt_individualaddress");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_contactid", ConditionOperator.Equal, customerId);

            var collection = this.retrieveMultiple(query);

            foreach (var item in collection.Entities)
            {
                data.Add(new IndividualAddressData
                {
                    individualAddressId = item.Id,
                    isDefaultAddress = item.Attributes.Contains("rnt_isdefaultaddress") ? item.GetAttributeValue<bool>("rnt_isdefaultaddress") : false,
                    name = item.Attributes.Contains("rnt_name") ? item.GetAttributeValue<string>("rnt_name") : string.Empty,
                    countryId = item.Attributes.Contains("rnt_countryid") ? item.GetAttributeValue<EntityReference>("rnt_countryid").Id : Guid.Empty,
                    countryName = item.Attributes.Contains("rnt_countryid") ? item.GetAttributeValue<EntityReference>("rnt_countryid").Name : string.Empty,
                    cityId = item.Attributes.Contains("rnt_cityid") ? item.GetAttributeValue<EntityReference>("rnt_cityid").Id : Guid.Empty,
                    cityName = item.Attributes.Contains("rnt_cityid") ? item.GetAttributeValue<EntityReference>("rnt_cityid").Name : string.Empty,
                    districtId = item.Attributes.Contains("rnt_districtid") ? item.GetAttributeValue<EntityReference>("rnt_districtid").Id : Guid.Empty,
                    districtName = item.Attributes.Contains("rnt_districtid") ? item.GetAttributeValue<EntityReference>("rnt_districtid").Name : string.Empty,
                    addressDetail = item.Attributes.Contains("rnt_addressdetail") ? item.GetAttributeValue<string>("rnt_addressdetail") : string.Empty,
                });
            }

            return data;
        }
        public List<Entity> getAllIndividualAddresses()
        {
            List<Entity> result = new List<Entity>();

            int queryCount = 5000;
            int pageNumber = 1;
            string pagingCookie = null;

            while (true)
            {
                QueryExpression expression = new QueryExpression("rnt_individualaddress");
                expression.ColumnSet = new ColumnSet(true);
                expression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
                expression.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };

                var results = this.retrieveMultiple(expression);
                result.AddRange(results.Entities.ToList());
                if (results.MoreRecords)
                {
                    pageNumber++;
                    pagingCookie = results.PagingCookie;
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        public List<Entity> getAllIndividualAddressesByGivenCustomerIds(List<string> customerIds)
        {
            var f = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                          <entity name='rnt_individualaddress'>
                            <attribute name='rnt_isdefaultaddress' />
                            <attribute name='rnt_name' />
                            <attribute name='rnt_countryid' />
                            <attribute name='rnt_cityid' />
                            <attribute name='rnt_districtid' />
                            <attribute name='rnt_addressdetail' />
                            <attribute name='rnt_contactid' />
                            <attribute name='rnt_individualaddressid' />
                            <filter type='and'>
                              <condition attribute='rnt_contactid' operator='in'>";
            foreach (var item in customerIds)
            {
                f += string.Format("<value uitype='contact'>{0}</value>", item);
            }
            f += @"</condition>
                            </filter>
                          </entity>
                        </fetch>";
            return this.retrieveMultiple(new FetchExpression(f)).Entities.ToList();            
        }
    }
}
