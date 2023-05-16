using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;

namespace RntCar.BusinessLibrary.Handlers
{
    public class RepositoryHandler
    {
        public CrmServiceClient CrmServiceClient;
        public IOrganizationService Service { get; set; }
        public Guid UserId { get; set; }
        public String OrganizationName { get; set; }

        public RepositoryHandler(IOrganizationService Service)
        {
            this.Service = Service;
        }
        public RepositoryHandler(CrmServiceClient _crmServiceClient)
        {
            this.CrmServiceClient = _crmServiceClient;
        }
        public RepositoryHandler(IOrganizationService Service, CrmServiceClient _crmServiceClient)
        {
            this.Service = Service;
            this.CrmServiceClient = _crmServiceClient;
        }
        public RepositoryHandler(IOrganizationService Service, Guid UserId)
        {
            this.Service = Service;
            this.UserId = UserId;
        }

        //retrieveById adlı Aşağıdaki üç overload metodu, belirtilen entity adı ve id ile belirtilen kaydı geri çekmek için kullanılır. Ancak farklı olan şey, hangi sütunların geri getirileceğidir.veriler  Microsoft Dynamics 365'ten geri getirildiği şeklindedir.
        public Entity retrieveById(string entityName, Guid id)
        {//İlk metot tüm sütunları geri getirir
            if (this.CrmServiceClient != null)
            {
                return this.CrmServiceClient.Retrieve(entityName, id, new ColumnSet(true));
            }
            else
            {
                return this.Service.Retrieve(entityName, id, new ColumnSet(true));
            }
        }
        public Entity retrieveById(string entityName, Guid id, string[] columns)
        {//sütunları belirtmek için ikinci metot kullanılabilir (string dizisi ile).
            if (this.CrmServiceClient != null)
            {
                return this.CrmServiceClient.Retrieve(entityName, id, new ColumnSet(columns));
            }
            else
            {
                return this.Service.Retrieve(entityName, id, new ColumnSet(columns));
            }
        }
        public Entity retrieveById(string entityName, Guid id, bool allColumns)
        {//tüm sütunları geri getirmek yerine "ColumnSet" sınıfı kullanarak belirtilen sütunları geri getirir.
            if (this.CrmServiceClient != null)
            {
                return this.CrmServiceClient.Retrieve(entityName, id, new ColumnSet(allColumns));
            }
            else
            {
                return this.Service.Retrieve(entityName, id, new ColumnSet(allColumns));
            }
        }
        public EntityCollection retrieveMultiple(QueryExpression queryExpression)
        // Bu sorgu nesnesi, filtreler, sıralama ve sütunlar gibi bir dizi özellikle birlikte CRM'deki bir varlık için bir sorgu tanımlar.
        {//Bu metod bir QueryExpression nesnesi kullanarak veritabanından birden fazla kayıt almak için kullanılır. QueryExpression, Entity tablosunda belirli bir filtreleme ve sütun kümesi kullanarak kayıtları getirmek için kullanılan bir sorgu yapısıdır.
            //Metodun içinde, CrmServiceClient adlı bir özellik kontrol edilir. Eğer bu özellik null değilse, CrmServiceClient nesnesi kullanılarak kayıtların çekildiği RetrieveMultiple metodu çağrılır. Eğer özellik null ise, yani CrmServiceClient kullanılamıyorsa, Service nesnesi (IOrganizationService) kullanılarak RetrieveMultiple metodu çağrılır.
            if (this.CrmServiceClient != null)
            {
                return this.CrmServiceClient.RetrieveMultiple(queryExpression);
            }
            else
            {
                return this.Service.RetrieveMultiple(queryExpression);
            }
        }
        public EntityCollection retrieveMultiple(QueryByAttribute queryExpression)
        {//Bu tür sorgu, belirli bir varlık türü için tek bir filtre ve sütun seti kullanarak bir sorgu oluşturmanıza olanak tanır.
            if (this.CrmServiceClient != null)
            {
                return this.CrmServiceClient.RetrieveMultiple(queryExpression);
            }
            else
            {
                return this.Service.RetrieveMultiple(queryExpression);
            }
        }
        public EntityCollection retrieveMultiple(FetchExpression fetchExpression)
        {// Bu tür sorgu, XML tabanlı bir sorgu dizesi kullanarak bir sorgu oluşturmanıza olanak tanır.
            if (this.CrmServiceClient != null)
            {
                return this.CrmServiceClient.RetrieveMultiple(fetchExpression);
            }
            else
            {
                return this.Service.RetrieveMultiple(fetchExpression);
            }
        }
        public RepositoryHandler(IOrganizationService Service, Guid UserId, String OrganizationName)
        {
            this.Service = Service;
            this.UserId = UserId;
            this.OrganizationName = OrganizationName;
        }
    }
}
