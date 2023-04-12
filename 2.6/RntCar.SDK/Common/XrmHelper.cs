using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RntCar.SDK.Common
{
    public class XrmHelper
    {
        public IOrganizationService IOrganizationService { get; set; }

        public Guid? UserId { get; set; }

        public XrmHelper(IOrganizationService _service)
        {
            IOrganizationService = _service;
        }

        public XrmHelper(IOrganizationService _service, Guid _userId)
        {
            UserId = _userId;
            IOrganizationService = _service;
        }

        public String GetXmlTagContent(Guid? userid, string tagname, string fileName = "rnt_/Data/Xml/ErrorMessages.xml")
        {
            String returnmessage = String.Empty;

            QueryExpression expression = new QueryExpression();
            expression.ColumnSet = new ColumnSet(true);
            expression.EntityName = "webresource";
            expression.Criteria.AddCondition(new ConditionExpression("name", ConditionOperator.Equal, fileName));

            EntityCollection collect = this.IOrganizationService.RetrieveMultiple(expression);
            if (collect.Entities.Count > 0)
            {
                byte[] binary = Convert.FromBase64String(collect.Entities[0]["content"].ToString());
                MemoryStream ms = new MemoryStream(binary);
                ms.Flush();
                ms.Position = 0;
                string content = UnicodeEncoding.UTF8.GetString(binary);

                XmlDocument doc = new XmlDocument();
                doc.Load(ms);
                int langId = this.GetLangId(userid);
                langId = langId == 0 ? 1055 : langId;

                string xPathExpression = "//LangId[@id='" + langId + "']//" + tagname;
                XmlElement item = (XmlElement)doc.SelectSingleNode(xPathExpression);

                if (item != null)
                {
                    returnmessage = item.InnerText;
                }
                else
                {
                    returnmessage = "Not Found";
                }
            }
            return returnmessage;
        }

        public string GetXmlTagContentByGivenLangId(string tagname, int? langId = 1033, string fileName = "rnt_/Data/Xml/ErrorMessages.xml")
        {
            String returnmessage = String.Empty;

            if (langId == null)
                langId = 1033;
            QueryExpression expression = new QueryExpression();
            expression.ColumnSet = new ColumnSet(true);
            expression.EntityName = "webresource";
            expression.Criteria.AddCondition(new ConditionExpression("name", ConditionOperator.Equal, fileName));

            EntityCollection collect = this.IOrganizationService.RetrieveMultiple(expression);
            if (collect.Entities.Count > 0)
            {
                byte[] binary = Convert.FromBase64String(collect.Entities[0]["content"].ToString());
                MemoryStream ms = new MemoryStream(binary);
                ms.Flush();
                ms.Position = 0;
                string content = UnicodeEncoding.UTF8.GetString(binary);

                XmlDocument doc = new XmlDocument();
                doc.Load(ms);
                int? _langId = langId;
                langId = langId == 0 ? 1055 : langId;

                string xPathExpression = "//LangId[@id='" + _langId + "']//" + tagname;
                XmlElement item = (XmlElement)doc.SelectSingleNode(xPathExpression);

                if (item != null)
                {
                    returnmessage = item.InnerText;
                }
                else
                {
                    returnmessage = "Not Found";
                }
            }
            return returnmessage;
        }

        public int GetLangId(Guid? userGuid)

        {
            int returnValue = 0;

            EntityCollection cols = new EntityCollection();

            string fetchXML = string.Format(@"<fetch distinct='false' top='1' mapping='logical' output-format='xml-platform' version='1.0'> 
                                  <entity name='systemuser'> 
                                    <attribute name='systemuserid'/> 
                                    <order descending='false' attribute='systemuserid'/> 
                                    <filter type='and'> 
                                      <condition attribute='systemuserid' value='{0}' operator='eq'/> 
                                    </filter>
                                     <link-entity name='usersettings' alias='a_4484069205d044a7bee3fb52c273d285' link-type='outer' visible='false' to='systemuserid' from='systemuserid'>
                                       <attribute name='uilanguageid'/>
                                     </link-entity>
                                  </entity> 
                                </fetch>", userGuid);

            cols = this.IOrganizationService
.RetrieveMultiple(new FetchExpression(fetchXML));

            foreach (Entity entity in cols.Entities)
            {
                if (entity.Attributes.Contains("a_4484069205d044a7bee3fb52c273d285.uilanguageid"))
                {
                    returnValue = int.Parse(((AliasedValue)(entity.Attributes["a_4484069205d044a7bee3fb52c273d285.uilanguageid"])).Value.ToString());
                }
            }

            return returnValue;
        }


        public List<OptionMetadata> GetoptionsetTextOnValue(string entityName, string attributeName)
        {
            RetrieveEntityRequest retrieveDetails = new RetrieveEntityRequest
            {
                EntityFilters = EntityFilters.All,
                LogicalName = entityName
            };
            RetrieveEntityResponse retrieveEntityResponseObj = (RetrieveEntityResponse)this.IOrganizationService.Execute(retrieveDetails);
            Microsoft.Xrm.Sdk.Metadata.EntityMetadata metadata = retrieveEntityResponseObj.EntityMetadata;
            Microsoft.Xrm.Sdk.Metadata.PicklistAttributeMetadata picklistMetadata = metadata.Attributes.FirstOrDefault(attribute => String.Equals(attribute.LogicalName, attributeName, StringComparison.OrdinalIgnoreCase)) as Microsoft.Xrm.Sdk.Metadata.PicklistAttributeMetadata;
            Microsoft.Xrm.Sdk.Metadata.OptionSetMetadata options = picklistMetadata.OptionSet;

            List<OptionMetadata> OptionsList = (from o in options.Options
                                                select o).ToList();
            return OptionsList;
            //string optionsetLabel = (OptionsList.First()).Label.UserLocalizedLabel.Label;
            //return optionsetLabel;
        }

        public string getConfigurationValueByName(string key)
        {
            QueryExpression expression = new QueryExpression("rnt_configuration");
            expression.ColumnSet = new ColumnSet(new string[] { "rnt_value" });
            expression.Criteria.AddCondition(new ConditionExpression("rnt_name", ConditionOperator.Equal, key));
            expression.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active));
            var result = this.IOrganizationService.RetrieveMultiple(expression).Entities.FirstOrDefault();
            return result != null ? result.GetAttributeValue<string>("rnt_value") : string.Empty;
        }

        public static string getRelatedLabelFromOptionSetMetadata(List<OptionMetadata> optionMetadatas, int value, int langId)
        {
            var labelCollection = optionMetadatas.Where(p => p.Value.Equals(value)).FirstOrDefault().Label.LocalizedLabels;

            var localizedLabel = labelCollection.Where(p => p.LanguageCode.Equals(langId)).FirstOrDefault();
            if (localizedLabel != null)
            {
                //return default language
                return localizedLabel.Label.ToString();
            }
            return labelCollection.FirstOrDefault().Label.ToString();
        }
        public CalculateRollupFieldResponse CalculateRollupField(string entityLogicalName, Guid entityId, string fieldName)
        {
            // create a request by passing an entity reference and the targeted rollup field name
            CalculateRollupFieldRequest request = new CalculateRollupFieldRequest
            {
                Target = new EntityReference(entityLogicalName, entityId),
                FieldName = fieldName
            };

            return (CalculateRollupFieldResponse)IOrganizationService.Execute(request);
        }

        public void CalculateRollupField(string entityLogicalName, Guid entityId, List<string> fields)
        {
            // create a request by passing an entity reference and the targeted rollup field name
            foreach (var fieldName in fields)
            {
                CalculateRollupFieldRequest request = new CalculateRollupFieldRequest
                {
                    Target = new EntityReference(entityLogicalName, entityId),
                    FieldName = fieldName
                };

                CalculateRollupFieldResponse response = (CalculateRollupFieldResponse)IOrganizationService.Execute(request);
            }
        }

        public void setState(string entityName, Guid entityId, int stateCode, int statusCode)
        {
            EntityReference target = new Microsoft.Xrm.Sdk.EntityReference(entityName, entityId);
            SetStateRequest request = new SetStateRequest();
            request.EntityMoniker = target;
            request.State = new Microsoft.Xrm.Sdk.OptionSetValue(stateCode);
            request.Status = new Microsoft.Xrm.Sdk.OptionSetValue(statusCode);

            this.IOrganizationService.Execute(request);
        }
        public UserTimeInfo getCurrentUserTimeInfo()
        {
            var currentUserSettings = this.IOrganizationService.RetrieveMultiple(
                 new QueryExpression("usersettings")
                 {
                     ColumnSet = new ColumnSet("timezonebias"),
                     Criteria = new FilterExpression
                     {
                         Conditions =
                        {
                            new ConditionExpression("systemuserid", ConditionOperator.EqualUserId)
                        }
                     }
                 }).Entities[0].ToEntity<Entity>();

            return new UserTimeInfo
            {
                //offset = (int)currentUserSettings.Attributes["timezonebias"],
                utcDateTime = DateTime.UtcNow

            };

        }

        public Entity initializeFromRequest(string sourceLogicalName, Guid sourceId, string targetLogicalName)
        {
            InitializeFromRequest initialize = new InitializeFromRequest
            {
                TargetEntityName = targetLogicalName,
                EntityMoniker = new EntityReference(sourceLogicalName, sourceId)
            };

            InitializeFromResponse initialized = (InitializeFromResponse)this.IOrganizationService.Execute(initialize);
            return initialized.Entity;
        }

        public static List<OptionSetModel> getOptionSetValuesFromEnum(Enum enumName)
        {
            List<OptionSetModel> optionSetData = new List<OptionSetModel>();
            foreach (var item in Enum.GetValues(enumName.GetType()))
            {
                optionSetData.Add(new OptionSetModel
                {
                    label = item.ToString(),
                    value = (int)item
                });
            }
            return optionSetData;
        }

        public static List<OptionSetModel> getEnumsAsOptionSetModelByLangId(string enumName, int langId = 1033)
        {
            if (langId == 1055)
            {
                Type enumType = Type.GetType("RntCar.ClassLibrary._Enums_1055." + enumName + ",RntCar.ClassLibrary");
                var values = GetEnumsAsOptionSetModel(enumType);
                return values;
            }
            else
            {
                Type enumType = Type.GetType("RntCar.ClassLibrary._Enums_1033." + enumName + ",RntCar.ClassLibrary");
                var values = GetEnumsAsOptionSetModel(enumType);
                return values;
            }
        }

        public static List<OptionSetModel> GetEnumsAsOptionSetModel(Type enumType)
        {
            if (enumType.BaseType != typeof(Enum))
            {
                throw new InvalidCastException();
            }
            List<OptionSetModel> optionSetData = new List<OptionSetModel>();
            foreach (var item in Enum.GetValues(enumType))
            {
                optionSetData.Add(new OptionSetModel
                {
                    label = item.ToString(),
                    value = (int)item
                });
            }
            return optionSetData;
        }
    }


}
