using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary.Translation;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class TranslationBL : BusinessHandler
    {
        public TranslationBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public TranslationBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public TranslationBL(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService) : base(initializer, orgService, tracingService)
        {
        }

        public List<TranslationData> getTranslationEntity(string entityName, int langCode)
        {
            List<TranslationData> translationList = new List<TranslationData>();

            TranslationRepository translationRepository = new TranslationRepository(this.OrgService);
            var translationEntityList = translationRepository.getTranslationAllEntity(entityName, langCode);

            foreach (var translationEntity in translationEntityList.Entities)
            {
                TranslationData translationData = new TranslationData
                {
                    TranslationDataId = translationEntity.Id,
                    RegardingObjectId = translationEntity.GetAttributeValue<EntityReference>("regardingobjectid").Id,
                    FieldName = translationEntity.GetAttributeValue<string>("subject"),
                    LangCode = translationEntity.GetAttributeValue<OptionSetValue>("rnt_langcode").Value,
                    TranslationHTMLText = translationEntity.GetAttributeValue<string>("description"),
                    TranslationText = translationEntity.GetAttributeValue<string>("activityadditionalparams"),
                };
                translationList.Add(translationData);
            }

            return translationList;
        }

        public List<TranslationData> getTranslationRecordByEntityName(string entityName, Guid recordId, int langCode)
        {
            List<TranslationData> translationList = new List<TranslationData>();

            TranslationRepository translationRepository = new TranslationRepository(this.OrgService);
            var translationEntityList = translationRepository.getTranslationRecordByEntityName(entityName, recordId, langCode);

            foreach (var translationEntity in translationEntityList.Entities)
            {
                TranslationData translationData = new TranslationData
                {
                    TranslationDataId = translationEntity.Id,
                    RegardingObjectId = translationEntity.GetAttributeValue<EntityReference>("regardingobjectid").Id,
                    FieldName = translationEntity.GetAttributeValue<string>("subject"),
                    LangCode = translationEntity.GetAttributeValue<OptionSetValue>("rnt_langcode").Value,
                    TranslationHTMLText = translationEntity.GetAttributeValue<string>("description"),
                    TranslationText = translationEntity.GetAttributeValue<string>("activityadditionalparams"),
                };
                translationList.Add(translationData);
            }

            return translationList;
        }

        public Entity getTranslateEntityAttributes(Entity entity, int langCode)
        {
            List<TranslationData> translationDataList = new List<TranslationData>();

            translationDataList = getTranslationEntity(entity.LogicalName, langCode);

            Entity returnEntity = new Entity();
            List<TranslationData> translationRecordList = translationDataList.Where(x => x.RegardingObjectId == entity.Id).ToList();
            foreach (var attributes in entity.Attributes)
            {
                ClassLibrary.Translation.TranslationData tempTranslation = translationRecordList.Where(x => x.FieldName == attributes.Key).FirstOrDefault();
                if (tempTranslation != null && (!string.IsNullOrEmpty(tempTranslation.TranslationText) || !string.IsNullOrEmpty(tempTranslation.TranslationHTMLText)))
                {
                    returnEntity.Attributes[attributes.Key] = string.IsNullOrEmpty(tempTranslation.TranslationHTMLText) ? tempTranslation.TranslationText : tempTranslation.TranslationHTMLText;
                }
                else
                {
                    returnEntity.Attributes[attributes.Key] = entity.Attributes[attributes.Key];
                }

            }

            return returnEntity;
        }
    }
}
