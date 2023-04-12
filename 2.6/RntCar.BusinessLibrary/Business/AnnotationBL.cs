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
            Entity annotation = new Entity("annotation");
            annotation["subject"] = string.IsNullOrEmpty(annotationData.Subject) ? string.Empty : annotationData.Subject;
            annotation.Attributes["notetext"] = string.IsNullOrEmpty(annotationData.NoteText) ? string.Empty : annotationData.NoteText;
            annotation.Attributes["objectid"] = new EntityReference(annotationData.ObjectName, annotationData.ObjectId);
            if(!string.IsNullOrEmpty(annotationData.DocumentBody))
            {
                annotation["documentbody"] = annotationData.DocumentBody;
            }
            return this.OrgService.Create(annotation);
        }
    }
    
}
