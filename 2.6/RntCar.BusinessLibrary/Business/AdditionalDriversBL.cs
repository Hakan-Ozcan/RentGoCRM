using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;

namespace RntCar.BusinessLibrary.Business
{
    public class AdditionalDriversBL : BusinessHandler
    {
        public AdditionalDriversBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public AdditionalDriversBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public AdditionalDriversBL(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService) : base(initializer, orgService, tracingService)
        {
        }

        public void createAdditionalDrivers(List<Guid> additionalDriversList, Guid contractId)
        {
            foreach (var item in additionalDriversList)
            {
                AdditionalDriverRepository additionalDriverRepository = new AdditionalDriverRepository(this.OrgService);//Döngünün içinde, AdditionalDriverRepository sınıfından bir örnek oluşturulur. Bu sınıf, ek sürücülerin veri tabanından alınmasını sağlar.
                var additionalDriver = additionalDriverRepository.getActiveAdditionalDriverByContactIdandContractId(item, contractId);//getActiveAdditionalDriverByContactIdandContractId yöntemi, item ve contractId parametreleriyle çağrılır. Bu, item olarak geçilen kişinin, contractId olarak geçilen sözleşmeye bağlı ek sürücüsünün veri tabanından getirilmesini sağlar.

                if (additionalDriver == null)//Eğer null ise, yeni bir ek sürücü oluşturulur.
                {
                    Entity e = new Entity("rnt_additionaldriver");//Yeni bir Entity örneği oluşturulur ve rnt_additionaldriver varlığına ait bir kayıt oluşturmak için kullanılır. 
                    e.Attributes["rnt_contractid"] = new EntityReference("rnt_contract", contractId);//rnt_contractid ve rnt_contactid alanları, sözleşme ve ek sürücü bilgileri ile doldurulur.
                    e.Attributes["rnt_contactid"] = new EntityReference("contact", item);
                    this.OrgService.Create(e);//Son olarak, Create yöntemi çağrılarak, yeni ek sürücü kaydı oluşturulur.
                }               
            }
        }
        public AdditionalDriverDeactivateResponse deactivateAdditionalDriverByContractandContactId(string contactId, string contractId)  //Bu kod bloğu, bir CRM veri tabanında belirli bir sözleşme ve kişi için kaydedilmiş bir ek sürücü kaydını pasif duruma getirir.
        {
            //İlk olarak, AdditionalDriverRepository sınıfının bir örneği oluşturulur. Bu sınıf, ek sürücü kayıtlarına erişmek için IOrganizationService arayüzünü kullanır.
            AdditionalDriverRepository additionalDriverRepository = new AdditionalDriverRepository(this.OrgService);
            var additionalDriver = additionalDriverRepository.getActiveAdditionalDriverByContactIdandContractId(Guid.Parse(contactId), Guid.Parse(contractId));//getActiveAdditionalDriverByContactIdandContractId yöntemi kullanılarak, belirli bir kişi ve sözleşme için aktif bir ek sürücü kaydı var mı diye kontrol edilir.
            // no active records found
            if (additionalDriver != null)//Eğer kayıt varsa, XrmHelper sınıfının bir örneği oluşturulur. Bu sınıf, CRM kayıtlarının durumlarını ayarlamak için kullanılır.
            {
                this.Trace("additional driver entity found");
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                // deactivate additional driver 
                this.Trace("set state for deactivation start");
                xrmHelper.setState(additionalDriver.LogicalName, additionalDriver.Id, (int)GlobalEnums.StateCode.Passive, 2);//setState yöntemi kullanılarak, ilgili ek sürücü kaydının durumu pasif olarak ayarlanır.
                this.Trace("set state for deactivation end");
                return new AdditionalDriverDeactivateResponse//AdditionalDriverDeactivateResponse sınıfından bir örnek oluşturulur ve ek sürücü kaydının ID'si ve işlem sonucu bilgileri eklenir.
                {
                    additionalDriverId = additionalDriver.Id,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
            //No active records found
            return new AdditionalDriverDeactivateResponse//Eğer kayıt yoksa, sadece işlem sonucu bilgileri içeren bir AdditionalDriverDeactivateResponse örneği döndürülür.
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }
    }
}
