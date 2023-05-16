using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1055;
using RntCar.IntegrationHelper;
using RntCar.Logger;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static RntCar.ClassLibrary.ContractItemEnums;
using static RntCar.ClassLibrary.GlobalEnums;
using System.Security.Cryptography.Xml;
using System.Web.Services.Description;

namespace RntCar.KABISIntegration
{
    class Program
    {
        public static CrmServiceHelper crmServiceHelper;
        public static KABISHelper kabisHelper;
        public static KABISBL kabisBL;
        public static LoggerHelper loggerHelper;
        public static Guid branchId;
        public static Guid branchNot = new Guid("d191815c-18f6-ec11-bb3d-000d3a2d8250"); // bodrum merkez hariç
        public static Guid testContract = new Guid("1e80d2b6-6c0e-ed11-82e4-000d3a48b166");
        public static string greaterDatetime = StaticHelper.GetConfiguration("GreaterDatetime");//app.config deki greaterdatetime'ın value değerini çekiyoruz
        static void Main(string[] args)
        {
            loggerHelper = new LoggerHelper();
            string branchStringId = StaticHelper.GetConfiguration("branchid");
            branchId = new Guid();
            if (!string.IsNullOrWhiteSpace(branchStringId)) //Eğer branchStringId değeri null ya da boş değilse
            {
                branchId = new Guid(branchStringId);//branchStringId değerini Guid veri tipine dönüştürür ve bu değeri branchId değişkenine atar.
            }
            crmServiceHelper = new CrmServiceHelper();
            kabisHelper = new KABISHelper();
            kabisBL = new KABISBL(crmServiceHelper.IOrganizationService);//IOrganizationService arayüzü, Microsoft Dynamics CRM için işlemlerin yapıldığı çeşitli hizmetlerin API'sini sağlar. IOrganizationService örneği, KABISBL sınıfının yapıcı metoduna parametre olarak verilir. Bu, KABISBL sınıfının, IOrganizationService örneği ile çalışacağı anlamına gelir.

            Console.WriteLine("Transfers..");
            KABISTransferProcess();
            Console.WriteLine("Complete..");
            KabisCompleteProcess();
            Console.WriteLine("Rental..");
            KabisRentalProcess();
            Environment.Exit(0);
            //SendMail(new KABISMessage() { code = "300", officeMail = "ogun.karamahmut@rentgo.com", contractNumber = "20230000", KabisPeocessType = KABISProcessType.Rental, message = "Araç ofiste bulunamadı.", office = "", plateNumber = "34TEST123" });
        }

        private static void KabisRentalProcess()
        {
            try
            {
                int i = 0;
                LinkEntity customerEntity = new LinkEntity();
                customerEntity.EntityAlias = "customerAlias";
                customerEntity.LinkFromEntityName = "rnt_contractitem";
                customerEntity.LinkFromAttributeName = "rnt_customerid";
                customerEntity.LinkToEntityName = "contact";
                customerEntity.LinkToAttributeName = "contactid";
                customerEntity.Columns = new ColumnSet("rnt_isturkishcitizen", "governmentid", "rnt_citizenshipid", "firstname", "lastname", "rnt_drivinglicensenumber", "birthdate", "rnt_passportnumber", "rnt_drivinglicensecountryid");

                LinkEntity equipmentEntity = new LinkEntity();
                equipmentEntity.EntityAlias = "equipmentAlias";
                equipmentEntity.LinkFromEntityName = "rnt_contractitem";
                equipmentEntity.LinkFromAttributeName = "rnt_equipment";
                equipmentEntity.LinkToEntityName = "rnt_equipment";
                equipmentEntity.LinkToAttributeName = "rnt_equipmentid";
                equipmentEntity.Columns = new ColumnSet("rnt_currentkm", "rnt_currentbranchid", "rnt_licensenumber");

                LinkEntity countryEntity = new LinkEntity();
                countryEntity.EntityAlias = "countryAlias";
                countryEntity.LinkFromEntityName = "contact";
                countryEntity.LinkFromAttributeName = "rnt_citizenshipid";
                countryEntity.LinkToEntityName = "rnt_country";
                countryEntity.LinkToAttributeName = "rnt_countryid";
                countryEntity.Columns = new ColumnSet("rnt_kabiscode", "rnt_name");

                LinkEntity drivingCountryEntity = new LinkEntity();
                drivingCountryEntity.EntityAlias = "drivingCountryAlias";
                drivingCountryEntity.LinkFromEntityName = "contact";
                drivingCountryEntity.LinkFromAttributeName = "rnt_drivinglicensecountryid";
                drivingCountryEntity.LinkToEntityName = "rnt_country";
                drivingCountryEntity.LinkToAttributeName = "rnt_countryid";
                drivingCountryEntity.Columns = new ColumnSet("rnt_kabiscode", "rnt_name");
                drivingCountryEntity.JoinOperator = JoinOperator.LeftOuter;

                QueryExpression getContractItem = new QueryExpression("rnt_contractitem");//Bu sorgu, "rnt_contractitem" tablosunda belirtilen sütunlarla filtrelenen kayıtları getirir.
                getContractItem.ColumnSet = new ColumnSet("rnt_pickupbranchid", "rnt_equipment", "rnt_customerid", "rnt_contractid");//, rnt_contractitem tablosundan hangi sütunların getirileceğini belirtir. 
                getContractItem.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)StatusCode.Rental);//getContractItem.Criteria ifadesi, sorgunun kriterlerini belirler. Criteria özelliği, FilterExpression sınıfından bir örnek döndürür. FilterExpression sınıfı, sorgunun koşullarını belirler.
                  //İlk koşul, "statuscode" sütununun Rental durum koduna eşit olması gerektiğini belirtir. ConditionOperator.Equal ifadesi, koşulun eşitlikle karşılaştırılacağını belirtir. İkinci koşul, "rnt_kabisstatus" sütununun KABISStatus.Waiting değerine eşit olması gerektiğini belirtir. Bu, KABISStatus adında bir enum sınıfı kullanarak gerçekleştirilir.ConditionOperator.Equal ifadesi, koşulun eşitlikle karşılaştırılacağını belirtir.

                getContractItem.Criteria.AddCondition("rnt_kabisstatus", ConditionOperator.Equal, (int)KABISStatus.Waiting);


                if (branchId != Guid.Empty)
                {
                    getContractItem.Criteria.AddCondition("rnt_pickupbranchid", ConditionOperator.Equal, branchId);
                }
                //getContractItem.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, testContract);
                getContractItem.Criteria.AddCondition("rnt_pickupbranchid", ConditionOperator.NotEqual, branchNot);
                getContractItem.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)GlobalEnums.ItemTypeCode.Equipment);
                getContractItem.Criteria.AddCondition("rnt_pickupdatetime", ConditionOperator.GreaterThan, Convert.ToDateTime(greaterDatetime));
                customerEntity.LinkEntities.Add(countryEntity);
                getContractItem.LinkEntities.Add(customerEntity);
                getContractItem.LinkEntities.Add(equipmentEntity);
                customerEntity.LinkEntities.Add(drivingCountryEntity);
                EntityCollection contractItemList = crmServiceHelper.IOrganizationService.RetrieveMultiple(getContractItem);
                Console.WriteLine($"Toplam Kayıt : {contractItemList.Entities.Count.ToString()}");
                foreach (var contractItem in contractItemList.Entities)
                {
                    i++;
                    var contractNumber = contractItem.GetAttributeValue<EntityReference>("rnt_contractid").Name;
                    try
                    {

                        EntityReference pickupBranchRef = contractItem.GetAttributeValue<EntityReference>("rnt_pickupbranchid");
                        EntityReference equipmentRef = contractItem.GetAttributeValue<EntityReference>("rnt_equipment");
                        var currentKmAlias = contractItem.GetAttributeValue<AliasedValue>("equipmentAlias.rnt_currentkm");
                        var currentKm = Convert.ToInt32(currentKmAlias.Value);
                        var currentBranchRefAlias = contractItem.GetAttributeValue<AliasedValue>("equipmentAlias.rnt_currentbranchid");
                        var currentBranchRef = (EntityReference)(currentBranchRefAlias.Value);

                        Console.WriteLine($"(Rental) {i.ToString()}. Kayıt - contractnumber:{contractNumber}, plate:{equipmentRef.Name}, office:{pickupBranchRef.Name}");

                        var kabisLogin = kabisBL.GetKABISInfo(pickupBranchRef);
                        var kabisCustomer = kabisBL.GetKABISCustomerInfoByLinkEntity(contractItem);
                        if (string.IsNullOrEmpty(kabisLogin.userName) || string.IsNullOrEmpty(kabisLogin.Password))
                        {
                            KABISMessage kABISMessage = new KABISMessage()
                            {
                                KabisPeocessType = KABISProcessType.Rental,
                                contractNumber = contractNumber,
                                plateNumber = equipmentRef.Name,
                                office = pickupBranchRef.Name,
                                code = "999",
                                message = "CRM'de Kabis bilgileriniz tanımlı değil",
                                officeMail = kabisLogin.officeEMail
                            };
                            SendMail(kABISMessage);
                        }
                        else
                        {
                            if (currentBranchRef.Id != pickupBranchRef.Id)
                            {
                                var licenseNumberAlias = contractItem.GetAttributeValue<AliasedValue>("equipmentAlias.rnt_licensenumber");
                                var licenseNumber = licenseNumberAlias.Value.ToString();
                                equipmentToTransferBranch(kabisLogin, equipmentRef, currentBranchRef, licenseNumber, contractNumber);

                            }

                            var kabisRentResponse = kabisHelper.RentVehicleThreadProcess(kabisLogin, kabisCustomer, equipmentRef.Name, currentKm);
                            if (kabisRentResponse.ResponseResult.Result)
                            {
                                Entity updateContract = new Entity(contractItem.LogicalName, contractItem.Id);
                                updateContract.Attributes["rnt_kabisstatus"] = new OptionSetValue((int)KABISStatus.Rental);
                                crmServiceHelper.IOrganizationService.Update(updateContract);
                            }
                            else
                            {

                                if (kabisRentResponse.Code == "303")
                                {
                                    crmServiceHelper.IOrganizationService.Retrieve(equipmentRef.LogicalName, equipmentRef.Id, new ColumnSet("rnt_currentbranchid"));
                                }
                                KABISMessage kABISMessage = new KABISMessage()
                                {
                                    KabisPeocessType = KABISProcessType.Rental,
                                    contractNumber = contractNumber,
                                    plateNumber = equipmentRef.Name,
                                    office = pickupBranchRef.Name,
                                    code = kabisRentResponse.Code,
                                    message = kabisRentResponse.ResponseResult.ExceptionDetail,
                                    officeMail = kabisLogin.officeEMail
                                };
                                SendMail(kABISMessage);
                                var errorMessage = $"(Rent) - contractnumber:{contractNumber}, plate:{equipmentRef.Name}, office:{pickupBranchRef.Name}, code:{kabisRentResponse.Code}, message:{kabisRentResponse.ResponseResult.ExceptionDetail}";
                                loggerHelper.traceError(errorMessage);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        loggerHelper.traceError(ex.Message + " trace " + ex.StackTrace);
                    }
                    Console.WriteLine("--------------------------------------------------------------------");

                }
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                loggerHelper.traceError(ex.Message + " trace " + ex.StackTrace);
            }

        }

        private static void KabisCompleteProcess()
        {
            loggerHelper = new LoggerHelper();
            try
            {
                LinkEntity equipmentEntity = new LinkEntity();
                equipmentEntity.EntityAlias = "equipmentAlias";
                equipmentEntity.LinkFromEntityName = "rnt_contractitem";
                equipmentEntity.LinkFromAttributeName = "rnt_equipment";
                equipmentEntity.LinkToEntityName = "rnt_equipment";
                equipmentEntity.LinkToAttributeName = "rnt_equipmentid";
                equipmentEntity.Columns = new ColumnSet("rnt_currentkm", "rnt_licensenumber");

                QueryExpression getContractItem = new QueryExpression("rnt_contractitem");
                getContractItem.ColumnSet = new ColumnSet("rnt_pickupbranchid", "rnt_equipment", "rnt_dropoffbranch", "rnt_contractid");
                //getContractItem.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, testContract);
                getContractItem.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)StatusCode.Completed);
                getContractItem.Criteria.AddCondition("rnt_kabisstatus", ConditionOperator.Equal, (int)KABISStatus.Rental);
                getContractItem.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)GlobalEnums.ItemTypeCode.Equipment);
                if (branchId != Guid.Empty)
                {
                    getContractItem.Criteria.AddCondition("rnt_dropoffbranch", ConditionOperator.Equal, branchId);
                }
                getContractItem.Criteria.AddCondition("rnt_dropoffbranch", ConditionOperator.NotEqual, branchNot);
                getContractItem.Criteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.GreaterThan, Convert.ToDateTime(greaterDatetime));
                getContractItem.LinkEntities.Add(equipmentEntity);

                EntityCollection contractItemList = crmServiceHelper.IOrganizationService.RetrieveMultiple(getContractItem);
                Console.WriteLine($"Toplam Kayıt : {contractItemList.Entities.Count.ToString()}");
                foreach (var contractItem in contractItemList.Entities)
                {
                    var contractNumber = contractItem.GetAttributeValue<EntityReference>("rnt_contractid").Name;
                    try
                    {
                        EntityReference pickupBranchRef = contractItem.GetAttributeValue<EntityReference>("rnt_pickupbranchid");
                        EntityReference equipmentRef = contractItem.GetAttributeValue<EntityReference>("rnt_equipment");
                        var currentKmAlias = contractItem.GetAttributeValue<AliasedValue>("equipmentAlias.rnt_currentkm");
                        var currentKm = Convert.ToInt32(currentKmAlias.Value);

                        Console.WriteLine($"(Return) - contractnumber:{contractNumber}, plate:{equipmentRef.Name}, office:{pickupBranchRef.Name}");

                        var kabisLogin = kabisBL.GetKABISInfo(pickupBranchRef);
                        if (string.IsNullOrEmpty(kabisLogin.userName) || string.IsNullOrEmpty(kabisLogin.Password))
                        {
                            KABISMessage kABISMessage = new KABISMessage()
                            {
                                KabisPeocessType = KABISProcessType.Complete,
                                contractNumber = contractNumber,
                                plateNumber = equipmentRef.Name,
                                office = pickupBranchRef.Name,
                                code = "999",
                                message = "CRM'de Kabis bilgileriniz tanımlı değil",
                                officeMail = kabisLogin.officeEMail
                            };
                            SendMail(kABISMessage);
                        }
                        else
                        {
                            var kabisReturnResponse = kabisHelper.ReturnVehicleThreadProcess(kabisLogin, equipmentRef.Name, currentKm);
                            if (kabisReturnResponse.ResponseResult.Result)
                            {
                                Entity updateContract = new Entity(contractItem.LogicalName, contractItem.Id);
                                updateContract.Attributes["rnt_kabisstatus"] = new OptionSetValue((int)KABISStatus.Complated);
                                crmServiceHelper.IOrganizationService.Update(updateContract);

                                EntityReference dropOffBranchRef = contractItem.GetAttributeValue<EntityReference>("rnt_dropoffbranch");
                                if (pickupBranchRef.Id != dropOffBranchRef.Id)
                                {

                                    var licenseNumberAlias = contractItem.GetAttributeValue<AliasedValue>("equipmentAlias.rnt_licensenumber");
                                    var licenseNumber = licenseNumberAlias.Value.ToString();
                                    equipmentToTransferBranch(kabisLogin, equipmentRef, dropOffBranchRef, licenseNumber, contractNumber);
                                }
                            }
                            else
                            {
                                KABISMessage kABISMessage = new KABISMessage()
                                {
                                    KabisPeocessType = KABISProcessType.Complete,
                                    contractNumber = contractNumber,
                                    plateNumber = equipmentRef.Name,
                                    office = pickupBranchRef.Name,
                                    code = kabisReturnResponse.Code,
                                    message = kabisReturnResponse.ResponseResult.ExceptionDetail,
                                    officeMail = kabisLogin.officeEMail
                                };
                                SendMail(kABISMessage);
                                var errorMessage = $"(Return) - contractnumber:{contractNumber}, plate:{equipmentRef.Name}, office:{pickupBranchRef.Name}, code:{kabisReturnResponse.Code}, message:{kabisReturnResponse.ResponseResult.ExceptionDetail}";
                                loggerHelper.traceError(errorMessage);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        loggerHelper.traceError(ex.Message + " trace " + ex.StackTrace);
                    }
                    Console.WriteLine("--------------------------------------------------------------------");
                }
            }
            catch (Exception ex)
            {
                loggerHelper.traceError(ex.Message + " trace " + ex.StackTrace);
            }
        }
        //pickup                                                        current
        private static void equipmentToTransferBranch(KABISIntegrationUser kabisSourceLogin, EntityReference equipmentRef, EntityReference targetBranchRef, string licenseNumber, string contractNumber)
        {
            if (!string.IsNullOrEmpty(kabisSourceLogin.userName) && !string.IsNullOrEmpty(kabisSourceLogin.Password))
            { 
                var kabisRemoveResponse = kabisHelper.RemoveVehicleThreadProcess(kabisSourceLogin, equipmentRef.Name);
                if (!kabisRemoveResponse.ResponseResult.Result)
                { 
                    var errorMessage = $"contractnumber:{contractNumber}, code:{kabisRemoveResponse.Code}, message:{kabisRemoveResponse.ResponseResult.ExceptionDetail}";
                    loggerHelper.traceError(errorMessage);
                }
            }

            var dropOffKabisLogin = kabisBL.GetKABISInfo(targetBranchRef);
            if (string.IsNullOrEmpty(dropOffKabisLogin.userName) || string.IsNullOrEmpty(dropOffKabisLogin.Password))
            {
                KABISMessage kABISMessage = new KABISMessage()
                {
                    KabisPeocessType = KABISProcessType.Transfer,
                    contractNumber = contractNumber,
                    plateNumber = equipmentRef.Name,
                    code = "999",
                    message = "CRM'de Kabis bilgileriniz tanımlı değil",
                    officeMail = dropOffKabisLogin.officeEMail
                };
                SendMail(kABISMessage);
            }
            else
            {
                var kabisAddResponse = kabisHelper.AddVehicleThreadProcess(dropOffKabisLogin, equipmentRef.Name, licenseNumber);
                if (!kabisAddResponse.ResponseResult.Result)
                {
                    KABISMessage kABISMessage = new KABISMessage()
                    {
                        KabisPeocessType = KABISProcessType.Transfer,
                        contractNumber = contractNumber,
                        plateNumber = equipmentRef.Name,
                        code = kabisAddResponse.Code,
                        message = kabisAddResponse.ResponseResult.ExceptionDetail,
                        officeMail = dropOffKabisLogin.officeEMail
                    };
                    SendMail(kABISMessage);
                    var errorMessage = $"contractnumber:{contractNumber}, code:{kabisAddResponse.Code}, message:{kabisAddResponse.ResponseResult.ExceptionDetail}";
                    loggerHelper.traceError(errorMessage);
                }
            }
        }

        private static void KABISTransferProcess()
        //Bu kod bloğu, belirli koşulları sağlayan transferlerin KABİS sistemi ile ilgili işlemlerini gerçekleştiren bir metodu içermektedir. Bu işlemler, transferin türüne (70 veya 50) bağlı olarak farklıdır.İlk olarak, belirli bir tarih zamanından sonra bekleyen ve Tamamlandı olarak işaretlenen transferleri sorgulayan bir sorgu ifadesi oluşturulur. Ayrıca, KABİS durumu "Bekliyor" olarak ayarlanmış olan ve (varsa) belirli bir şubeye (branchId) göre filtrelenmiş olan transferler de sorguya dahil edilir.Daha sonra, transferler döngüye sokulur ve her bir transferin türüne göre farklı işlemler gerçekleştirilir.Transfer türü 50 ise, transferin alındığı şubede bir araç varsa, aracın plaka numarası ve alınan şube bilgisi kullanılarak KABİS sistemi ile bir bağlantı kurulur. Bu işlem için, kabisBL sınıfının GetKABISInfo metodu kullanılır ve eğer KABİS bilgileri mevcut değilse bir hata mesajı oluşturulur. Eğer KABİS bilgileri mevcut ise, aracın hedef şubeye transferi gerçekleştirilir.Transfer türü 70 ise, transferin teslim edileceği şubenin KABİS bilgileri alınır ve eğer mevcut değilse bir hata mesajı oluşturulur.Ardından, araç KABİS sistemine eklenir ve işlem sonucu kontrol edilir.Eğer işlem başarısız olursa, bir hata mesajı oluşturulur.Son olarak, transferlerin KABİS durumu "Tamamlandı" olarak güncellenir ve herhangi bir hata durumunda ilgili kişilere bir e-posta gönderilir.

        {
            List<int> transferTypeList = new List<int>() { 70, 50 };
            foreach (var transferType in transferTypeList)
            {
                LinkEntity equipmentEntity = new LinkEntity();
                equipmentEntity.EntityAlias = "equipmentAlias";
                equipmentEntity.LinkFromEntityName = "rnt_transfer";
                equipmentEntity.LinkFromAttributeName = "rnt_equipmentid";
                equipmentEntity.LinkToEntityName = "rnt_equipment";
                equipmentEntity.LinkToAttributeName = "rnt_equipmentid";
                equipmentEntity.Columns = new ColumnSet("rnt_licensenumber");

                QueryExpression getTransferList = new QueryExpression("rnt_transfer");
                getTransferList.ColumnSet = new ColumnSet("rnt_pickupbranchid", "rnt_equipmentid", "rnt_dropoffbranchid", "rnt_transfernumber", "rnt_estimatedpickupdate");
                getTransferList.Criteria.AddCondition("rnt_estimatedpickupdate", ConditionOperator.GreaterThan, Convert.ToDateTime(greaterDatetime));
                getTransferList.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_transfer_StatusCode.Tamamland);
                getTransferList.Criteria.AddCondition("rnt_kabisstatus", ConditionOperator.Equal, (int)KABISStatus.Waiting);
                getTransferList.Criteria.AddCondition("rnt_transfertype", ConditionOperator.Equal, transferType);
                if (branchId != Guid.Empty)
                {
                    getTransferList.Criteria.AddCondition("rnt_dropoffbranchid", ConditionOperator.Equal, branchId);
                }
                getTransferList.LinkEntities.Add(equipmentEntity);
                EntityCollection transferList = crmServiceHelper.IOrganizationService.RetrieveMultiple(getTransferList);
                foreach (var transfer in transferList.Entities)
                {
                    try
                    {
                        EntityReference pickupBranchRef = transfer.GetAttributeValue<EntityReference>("rnt_pickupbranchid");
                        EntityReference equipmentRef = transfer.GetAttributeValue<EntityReference>("rnt_equipmentid");
                        EntityReference dropoffBranchRef = transfer.GetAttributeValue<EntityReference>("rnt_dropoffbranchid");
                        if (transferType == 50)
                        {
                            var kabisLogin = kabisBL.GetKABISInfo(pickupBranchRef);
                            if (string.IsNullOrEmpty(kabisLogin.userName) || string.IsNullOrEmpty(kabisLogin.Password))
                            {
                                KABISMessage kABISMessage = new KABISMessage()
                                {
                                    KabisPeocessType = KABISProcessType.Transfer,
                                    plateNumber = equipmentRef.Name,
                                    office = pickupBranchRef.Name,
                                    code = "999",
                                    message = "CRM'de Kabis bilgileriniz tanımlı değil",
                                    officeMail = kabisLogin.officeEMail
                                };
                                SendMail(kABISMessage);
                            }
                            else
                            {
                                if (dropoffBranchRef.Id != pickupBranchRef.Id)
                                {
                                    var licenseNumberAlias = transfer.GetAttributeValue<AliasedValue>("equipmentAlias.rnt_licensenumber");
                                    var licenseNumber = licenseNumberAlias.Value.ToString();
                                    var pnrNumber = transfer.GetAttributeValue<string>("rnt_pnrnumber");
                                    equipmentToTransferBranch(kabisLogin, equipmentRef, dropoffBranchRef, licenseNumber, pnrNumber);
                                }
                            }
                        }
                        else
                        {
                            var kabisLogin = kabisBL.GetKABISInfo(dropoffBranchRef);
                            if (string.IsNullOrEmpty(kabisLogin.userName) || string.IsNullOrEmpty(kabisLogin.Password))
                            {
                                KABISMessage kABISMessage = new KABISMessage()
                                {
                                    KabisPeocessType = KABISProcessType.Transfer, 
                                    plateNumber = equipmentRef.Name,
                                    office = pickupBranchRef.Name,
                                    code = "999",
                                    message = "CRM'de Kabis bilgileriniz tanımlı değil",
                                    officeMail = kabisLogin.officeEMail
                                };
                                SendMail(kABISMessage);
                            }
                            else
                            {
                                var licenseNumberAlias = transfer.GetAttributeValue<AliasedValue>("equipmentAlias.rnt_licensenumber");
                                var licenseNumber = licenseNumberAlias.Value.ToString();
                                var pnrNumber = transfer.GetAttributeValue<string>("rnt_pnrnumber");
                                var kabisAddResponse = kabisHelper.AddVehicleThreadProcess(kabisLogin, equipmentRef.Name, licenseNumber);

                                if (!kabisAddResponse.ResponseResult.Result)
                                {
                                    KABISMessage kABISMessage = new KABISMessage()
                                    {
                                        KabisPeocessType = KABISProcessType.Transfer,
                                        contractNumber = pnrNumber,
                                        plateNumber = equipmentRef.Name,
                                        code = kabisAddResponse.Code,
                                        message = kabisAddResponse.ResponseResult.ExceptionDetail,
                                        officeMail = kabisLogin.officeEMail
                                    };
                                    SendMail(kABISMessage);
                                }

                            }
                            Entity updateTransfer = new Entity(transfer.LogicalName, transfer.Id);
                            updateTransfer.Attributes["rnt_kabisstatus"] = new OptionSetValue((int)KABISStatus.Complated);
                            crmServiceHelper.IOrganizationService.Update(updateTransfer);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }
        private static void SendMail(KABISMessage kabisMessage)
        {
            try
            {
                var smtpSenderMail = ConfigurationManager.AppSettings.Get("SmtpSenderMail");
                //"ConfigurationManager" nesnesi ile uygulamanın yapılandırma dosyasındaki ("app.config" veya "web.config") değerlere erişiyoruz. Burada "SmtpSenderMail" adlı bir değerin alındığını görüyoruz. Bu değer, SMTP sunucusunda kullanılacak olan mail adresini içerir.
                var smtpSenderPassword = ConfigurationManager.AppSettings.Get("SmtpSenderPassword");
                var smtpCCMails = ConfigurationManager.AppSettings.Get("SmtpCCMails");
                var host = "smtp.office365.com";//SMTP sunucusunun adresi belirleniyor.
                var port = 587; // SMTP sunucusunun port numarası belirleniyor.
                var typeName = "";
                switch (kabisMessage.KabisPeocessType)
                {
                    case KABISProcessType.Rental:
                        typeName = "Kiralama";
                        break;
                    case KABISProcessType.Complete:
                        typeName = "Kiradan Alma";
                        break;
                    case KABISProcessType.Transfer:
                        typeName = "Transfer";
                        break;

                }
               
                var subject = $"KABİS Hatası Hakkında ({kabisMessage.plateNumber})";
                var body = "";
                if (kabisMessage.KabisPeocessType == KABISProcessType.Transfer)
                    body = $"Merhaba, <br /> <br /> <b>{kabisMessage.plateNumber}</b> plakalı aracın KABİS kaydı({typeName} işlemi) <b>yapılamamıştır.</b> <br /><br />";
                else
                    body = $"Merhaba, <br /> <br /> <b>{kabisMessage.contractNumber}</b> sözleşme içerisinde bulunan <b>{kabisMessage.plateNumber}</b> plakalı aracın KABİS kaydı({typeName} işlemi) <b>yapılamamıştır.</b> <br /><br />";

                body += $"<u>Hata Detayı</u> <br/> <b>Kod :</b> {kabisMessage.code} <br/> <b>Açıklama :</b> {kabisMessage.message} <br/><br/> Gerekli kontrollerin yapılmasını rica ederiz. <br/><br/>";


                var client = new SmtpClient(host, port)// SmtpClient sınıfı ile host ve port değerleri kullanılarak bir SMTP istemci örneği oluşturuluyor.
                {
                    //NetworkCredential sınıfı kullanılarak SMTP sunucusuna yetkilendirme bilgileri ekleniyor. Bu bilgiler smtpSenderMail ve smtpSenderPassword değişkenlerinden alınıyor.
                    Credentials = new NetworkCredential(smtpSenderMail, smtpSenderPassword),
                    EnableSsl = true
                };
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(smtpSenderMail);
                mailMessage.To.Add(kabisMessage.officeMail);
                mailMessage.CC.Add(smtpCCMails);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;
                client.Send(mailMessage);//En son olarak Send metodunu kullanarak e-posta SMTP sunucusuna gönderiliyor
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}

