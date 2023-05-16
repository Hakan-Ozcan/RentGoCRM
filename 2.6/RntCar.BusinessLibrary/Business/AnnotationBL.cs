using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class AnnotationBL: BusinessHandler
    {
        public AnnotationBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public AnnotationBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public AnnotationBL()
        {
        }
        public AnnotationBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public AnnotationBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public Guid createNewAnnotation(AnnotationData annotationData)
        {
            Entity annotation = new Entity("annotation");//"annotation" adında bir Entity nesnesi oluşturuluyor. Bu nesne, CRM veritabanındaki "annotation" tablosuna karşılık gelir.
            annotation["subject"] = string.IsNullOrEmpty(annotationData.Subject) ? string.Empty : annotationData.Subject;//"annotation" nesnesinin "subject" özelliğine, parametre olarak gelen AnnotationData nesnesinin "Subject" özelliği atanıyor. Eğer bu özellik null veya boş ise, boş bir string atanır.
            annotation.Attributes["notetext"] = string.IsNullOrEmpty(annotationData.NoteText) ? string.Empty : annotationData.NoteText;//"annotation" nesnesinin "notetext" özelliğine, parametre olarak gelen AnnotationData nesnesinin "NoteText" özelliği atanıyor. Eğer bu özellik null veya boş ise, boş bir string atanır.

            annotation.Attributes["objectid"] = new EntityReference(annotationData.ObjectName, annotationData.ObjectId);//"annotation" nesnesinin "objectid" özelliğine, parametre olarak gelen AnnotationData nesnesinin "ObjectName" ve "ObjectId" özellikleri kullanılarak bir EntityReference nesnesi atanıyor. Bu nesne, notun bağlı olduğu nesnenin türünü ve ID'sini belirtir.
            if (!string.IsNullOrEmpty(annotationData.DocumentBody))
            {
                annotation["documentbody"] = annotationData.DocumentBody;//Eğer parametre olarak gelen AnnotationData nesnesinin "DocumentBody" özelliği null veya boş değilse, "annotation" nesnesinin "documentbody" özelliğine bu değer atanır.
            }
            return this.OrgService.Create(annotation);//"annotation" nesnesi, IOrganizationService arayüzünden türetilen "OrgService" özelliği kullanılarak CRM'e kaydedilir. Kaydedilen notun ID'si Guid türünde döndürülür.
        }
    }
    
}
