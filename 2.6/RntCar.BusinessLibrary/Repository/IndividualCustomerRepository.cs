using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Repository
{
    public class IndividualCustomerRepository : RepositoryHandler
    {
        public IndividualCustomerRepository(IOrganizationService Service) : base(Service)
        {
        }

        public IndividualCustomerRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public IndividualCustomerRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public IndividualCustomerRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public List<Entity> getAllIndividualCustomers()
        {
            List<Entity> result = new List<Entity>();

            int queryCount = 5000;
            int pageNumber = 1;
            string pagingCookie = null;

            while (true)
            {
                QueryExpression expression = new QueryExpression("contact");
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

        public List<Entity> getIndividualCustomerByGivenIds(object[] contactIds)
        {
            QueryExpression expression = new QueryExpression("contact");
            expression.ColumnSet = new ColumnSet(true);
            expression.Criteria.AddCondition("contactid", ConditionOperator.In, contactIds);
            expression.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            return this.retrieveMultiple(expression).Entities.ToList();
        }
        public List<IndividualCustomerData> getCustomerByGivenCriteriaWithGivenColumns(string searchText, string[] columns)
        {
            List<IndividualCustomerData> data = new List<IndividualCustomerData>();
            QueryExpression expression = new QueryExpression("contact");
            expression.ColumnSet = new ColumnSet(columns);
            FilterExpression filterExp = new FilterExpression(LogicalOperator.Or);
            foreach (var item in columns)
            {

                filterExp.AddCondition(item, ConditionOperator.Like, "%" + searchText + "%");
            }
            expression.Criteria.AddFilter(filterExp);
            var res = this.Service.RetrieveMultiple(expression);
            foreach (var item in res.Entities)
            {
                data.Add(new IndividualCustomerData
                {
                    FirstName = item.GetAttributeValue<String>("firstname")
                });
            }
            return data;
        }

        public List<IndividualCustomerData> CustomerSearchByFetchXML(string searchText, string[] columns)
        {
            List<IndividualCustomerData> data = new List<IndividualCustomerData>();
            var fetchExp = string.Format(@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                          <entity name='contact'>
                                            <attribute name='firstname' />
                                            <attribute name='lastname' />
                                            <attribute name='governmentid' />
                                            <attribute name='emailaddress1' />
                                            <attribute name='mobilephone' />
                                            <filter type='and'>
                                              <filter type='or'>
                                                <condition attribute='fullname' operator='like' value='%{0}%' />                                                
                                                <condition attribute='mobilephone' operator='like' value='%{0}%' />
                                                <condition attribute='governmentid' operator='like' value='%{0}%' />
                                                <condition attribute='rnt_passportnumber' operator='like' value='%{0}%' />
                                              </filter>
                                            </filter>
                                          </entity>
                                        </fetch>", searchText);
            var res = this.Service.RetrieveMultiple(new FetchExpression(fetchExp));
            foreach (var item in res.Entities)
            {
                data.Add(new IndividualCustomerData
                {
                    FirstName = item.GetAttributeValue<String>("firstname"),
                    LastName = item.GetAttributeValue<String>("lastname"),
                    GovernmentId = item.GetAttributeValue<String>("governmentid"),
                    MobilePhone = item.GetAttributeValue<String>("mobilephone"),
                    Email = item.GetAttributeValue<String>("emailaddress1"),
                    IndividualCustomerId = item.Id
                });
            }
            return data;
        }
        public List<Entity> getContactsByGiventDatePermissions(string createdon)
        {
            var str = string.Format(@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                      <entity name='rnt_marketingpermissions'>                      
                        <attribute name='rnt_permissionchannelcode' />
                        <attribute name='rnt_allowsms' />
                        <attribute name='rnt_allownotification' />
                        <attribute name='rnt_allowemails' />
                        <attribute name='createdon' />
                        <filter type='and'>
                          <condition attribute='statecode' operator='eq' value='0' />
                        </filter>
                        <link-entity name='contact' from='contactid' to='rnt_contactid' link-type='inner' alias='ac'>
                          <attribute name='rnt_segmentcode' />
                          <attribute name='rnt_passportnumber' />
                          <attribute name='createdon' />
                          <attribute name='rnt_lastrentalofficeid' />
                          <attribute name='mobilephone' />
                          <attribute name='lastname' />
                          <attribute name='birthdate' />
                          <attribute name='rnt_isturkishcitizen' />
                          <attribute name='governmentid' />
                          <attribute name='gendercode' />
                          <attribute name='firstname' />
                          <attribute name='rnt_findekspoint' />
                          <attribute name='emailaddress1' />                         
                          <attribute name='rnt_drivinglicensedate' />                      
                          <attribute name='rnt_customernumber' />
                          <link-entity name='rnt_contract' from='rnt_customerid' to='contactid' link-type='inner' alias='am'>
                            <filter type='and'>
                              <condition attribute='rnt_dropoffdatetime' operator='on' value='{0}' />
                            </filter>
                          </link-entity>
                        </link-entity>
                      </entity>
                </fetch>", createdon);

            return this.retrieveMultiple(new FetchExpression(str)).Entities.ToList();
        }

        public List<Entity> getContactsByMarketingDatePermissions(string createdon)
        {
            var str = string.Format(@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                      <entity name='rnt_marketingpermissions'>                      
                        <attribute name='rnt_permissionchannelcode' />
                        <attribute name='rnt_allowsms' />
                        <attribute name='rnt_allownotification' />
                        <attribute name='rnt_allowemails' />
                        <attribute name='createdon' />
                        <filter type='and'>
                          <condition attribute='statecode' operator='eq' value='0' />
                          <condition attribute='createdon' operator='on' value='{0}' />
                        </filter>
                        <link-entity name='contact' from='contactid' to='rnt_contactid' link-type='inner' alias='ac'>
                          <attribute name='rnt_segmentcode' />
                          <attribute name='rnt_passportnumber' />
                          <attribute name='createdon' />
                          <attribute name='rnt_lastrentalofficeid' />
                          <attribute name='mobilephone' />
                          <attribute name='lastname' />
                          <attribute name='birthdate' />
                          <attribute name='rnt_isturkishcitizen' />
                          <attribute name='governmentid' />
                          <attribute name='gendercode' />
                          <attribute name='firstname' />
                          <attribute name='rnt_findekspoint' />
                          <attribute name='emailaddress1' />                         
                          <attribute name='rnt_drivinglicensedate' />                      
                          <attribute name='rnt_customernumber' />
                        </link-entity>
                      </entity>
                </fetch>", createdon);

            return this.retrieveMultiple(new FetchExpression(str)).Entities.ToList();
        }

        public EntityCollection getCorporateEmployeesInformationByFetchXML(string corporateCustomerId)
        {
            //rnt_relationcode 
            //1 -->Responsible Employee
            //2 -->Employee
            string fetchXml = String.Format(@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                                  <entity name='contact'>
                                    <attribute name='firstname' />
                                            <attribute name='lastname' />
                                            <attribute name='governmentid' />
                                            <attribute name='emailaddress1' />
                                            <attribute name='mobilephone' />  
                                            <attribute name='contactid' />          
                                            <filter type='and'>
                                              <condition attribute='statecode' operator='eq' value='0' />
                                            </filter>
                                          <link-entity name='rnt_connection' from='rnt_contactid' to='contactid' alias='aa'>
                                          <filter type='and'>
                                            <condition attribute='rnt_relationcode' operator='in'>
                                              <value>2</value>
                                              <value>1</value>
                                            </condition>
                                            <condition attribute='rnt_accountid' operator='eq' uitype='account' value='{0}' />
                                            <condition attribute='statecode' operator='eq' value='0' />
                                          </filter>
                                        </link-entity>
                                      </entity>
                                    </fetch>", corporateCustomerId);

            return this.Service.RetrieveMultiple(new FetchExpression(fetchXml));
        }

        public Entity getExistingCustomerIdByGovernmentIdOrPassportNumberOrEmailAddress(string _id, string emailAddress, String[] columns)
        {
            List<IndividualCustomerData> data = new List<IndividualCustomerData>();
            QueryExpression expression = new QueryExpression("contact");
            expression.ColumnSet = new ColumnSet(columns);
            FilterExpression filterExp = new FilterExpression(LogicalOperator.Or);
            filterExp.AddCondition("governmentid", ConditionOperator.Equal, _id);
            filterExp.AddCondition("rnt_passportnumber", ConditionOperator.Equal, _id);
            filterExp.AddCondition("emailaddress1", ConditionOperator.Equal, emailAddress);
            expression.Criteria.AddFilter(filterExp);
            return this.Service.RetrieveMultiple(expression).Entities.FirstOrDefault();

        }

        public Guid? getExistingCustomerIdByGovernmentIdOrPassportNumber(string _id, String[] columns)
        {
            List<IndividualCustomerData> data = new List<IndividualCustomerData>();
            QueryExpression expression = new QueryExpression("contact");
            expression.ColumnSet = new ColumnSet(columns);
            FilterExpression filterExp = new FilterExpression(LogicalOperator.Or);
            filterExp.AddCondition("governmentid", ConditionOperator.Equal, _id);
            filterExp.AddCondition("rnt_passportnumber", ConditionOperator.Equal, _id);
            expression.Criteria.AddFilter(filterExp);
            var result = this.Service.RetrieveMultiple(expression).Entities.FirstOrDefault();
            if (result != null)
                return result.Id;

            return null;
        }

        public Entity getIndividualCustomerByIdWithGivenColumns(Guid id, string[] columns)
        {
            return this.retrieveById("contact", id, columns);
        }

        public Entity getIndividualCustomerById(Guid id)
        {
            QueryExpression expression = new QueryExpression("contact");
            expression.ColumnSet = new ColumnSet(true);
            expression.Criteria.AddCondition("contactid", ConditionOperator.Equal, id);

            return this.Service.RetrieveMultiple(expression).Entities.FirstOrDefault();
        }

        public Entity getCustomerByMobilePhoneWithGivenColumns(string mobilePhone, string[] columns)
        {
            List<IndividualCustomerData> data = new List<IndividualCustomerData>();
            QueryExpression expression = new QueryExpression("contact");
            if (columns.Length > 0)
            {
                expression.ColumnSet = new ColumnSet(columns);
            }
            else
            {
                expression.ColumnSet = new ColumnSet(true);
            }

            expression.Criteria.AddCondition("mobilephone", ConditionOperator.Equal, mobilePhone);
            return this.retrieveMultiple(expression).Entities.FirstOrDefault();
        }

        public Entity getIndividualCustomerByWebEmailAddress(string emailAddress)
        {
            QueryExpression queryExpression = new QueryExpression("contact");
            queryExpression.Criteria.AddCondition("rnt_webemailaddress", ConditionOperator.Equal, emailAddress);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public Entity getIndividualCustomerByWebEmailAddress(string emailAddress, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("contact");
            queryExpression.Criteria.AddCondition("rnt_webemailaddress", ConditionOperator.Equal, emailAddress);
            if (columns.Length > 0)
            {
                queryExpression.ColumnSet = new ColumnSet(columns);
            }
            else
            {
                queryExpression.ColumnSet = new ColumnSet(true);
            }
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }

        public Entity getIndividualCustomerByMobileEmailAddress(string emailAddress)
        {
            QueryExpression queryExpression = new QueryExpression("contact");
            queryExpression.Criteria.AddCondition("rnt_webemailaddress", ConditionOperator.Equal, emailAddress);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public Entity getIndividualCustomerByEmailAddress(string emailAddress, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("contact");
            queryExpression.Criteria.AddCondition("emailaddress1", ConditionOperator.Equal, emailAddress);
            if (columns.Length > 0)
            {
                queryExpression.ColumnSet = new ColumnSet(columns);
            }
            else
            {
                queryExpression.ColumnSet = new ColumnSet(true);
            }
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }

        public Entity getIndividualCustomerByMobileEmailAddress(string emailAddress, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("contact");
            queryExpression.Criteria.AddCondition("rnt_webemailaddress", ConditionOperator.Equal, emailAddress);
            if (columns.Length > 0)
            {
                queryExpression.ColumnSet = new ColumnSet(columns);
            }
            else
            {
                queryExpression.ColumnSet = new ColumnSet(true);
            }
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public Entity getWebUserInformation(string webEmailAddress, string webPassword)
        {
            QueryExpression queryExpression = new QueryExpression("contact");
            queryExpression.ColumnSet = new ColumnSet(new string[]
            {
                "rnt_customerexternalid",
                "gendercode",
                "firstname",
                "lastname",
                "birthdate",
                "governmentid",
                "rnt_passportnumber",
                "rnt_webemailaddress",
                "mobilephone",
                "rnt_drivinglicenseclasscode",
                "rnt_drivinglicensenumber",
                "rnt_drivinglicensedate",
                "rnt_drivinglicenseplace",
                "rnt_citizenshipid",
                "rnt_segmentcode",
                "rnt_dialcode"
            });
            queryExpression.Criteria.AddCondition("rnt_webemailaddress", ConditionOperator.Equal, webEmailAddress);
            queryExpression.Criteria.AddCondition("rnt_webpassword", ConditionOperator.Equal, webPassword);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();

        }

        public Entity getMobileUserInformation(string mobileEmailAddress, string mobilePassword)
        {
            QueryExpression queryExpression = new QueryExpression("contact");
            queryExpression.ColumnSet = new ColumnSet(new string[]
            {
                "rnt_customerexternalid",
                "rnt_distributionchannelcode",
                "gendercode",
                "firstname",
                "lastname",
                "birthdate",
                "governmentid",
                "rnt_passportnumber",
                "rnt_webemailaddress",
                "mobilephone",
                "rnt_drivinglicenseclasscode",
                "rnt_drivinglicensenumber",
                "rnt_drivinglicensedate",
                "rnt_drivinglicenseplace",
                "rnt_citizenshipid",
                "rnt_segmentcode",
                "statuscode"
            });
            queryExpression.Criteria.AddCondition("rnt_webemailaddress", ConditionOperator.Equal, mobileEmailAddress);
            queryExpression.Criteria.AddCondition("rnt_webpassword", ConditionOperator.Equal, mobilePassword);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
    }
}
