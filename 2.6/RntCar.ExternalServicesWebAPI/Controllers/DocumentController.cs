using FirebaseAdmin;
using GemBox.Document;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.Xrm.Sdk;
using MongoDB.Driver;
using Newtonsoft.Json;
using RntCar.AzureBlobStorage;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.Logger;
using RntCar.MongoDBHelper;
using RntCar.RedisCacheHelper.CachedItems;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Http;

namespace RntCar.ExternalServicesWebAPI.Controllers
{
    public class DocumentController : ApiController
    {
        [HttpPost]
        [Route("api/document/execute")]
        public void execute(DocumentOperationParameters documentOperationParameters)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            try
            {
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                loggerHelper.traceInfo("Document Execution start");
                loggerHelper.traceInfo("inputs " + JsonConvert.SerializeObject(documentOperationParameters));

                ContractRepository contractRepository = new ContractRepository(crmServiceHelper.IOrganizationService);
                var contract = contractRepository.getContractById(documentOperationParameters.contractId, new string[] { "rnt_contractnumber",
                                                                                                                         "rnt_pickupbranchid",
                                                                                                                         "rnt_customerid",
                                                                                                                         "rnt_pnrnumber"});

                var customerId = contract.GetAttributeValue<EntityReference>("rnt_customerid").Id;
                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper.IOrganizationService);
                var customer = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(customerId, new string[] { "rnt_citizenshipid" });
                var customerCitizenshipId = customer.GetAttributeValue<EntityReference>("rnt_citizenshipid").Id;

                ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                var turkeyGuid = configurationBL.GetConfigurationByName("TurkeyGuid");
                var documentTemplateGuid = new Guid(StaticHelper.GetConfiguration("DocumentTemplateGuid"));

                if (customerCitizenshipId != new Guid(turkeyGuid.Split(';')[0]))
                {
                    documentTemplateGuid = new Guid(StaticHelper.GetConfiguration("DocumentTemplateGuidEn"));
                }

                OrganizationRequest organizationRequest = new OrganizationRequest("SETWORDTEMPLATE");
                organizationRequest["Target"] = new EntityReference("rnt_contract", documentOperationParameters.contractId);
                organizationRequest["SelectedTemplate"] = new EntityReference("documenttemplate", documentTemplateGuid);
                var r = crmServiceHelper.IOrganizationService.Execute(organizationRequest);
                AnnotationRepository annotationRepository = new AnnotationRepository(crmServiceHelper.IOrganizationService);
                var annotation = annotationRepository.getLatestAnnotationByObjectIdByFileName(documentOperationParameters.contractId, documentOperationParameters.fileName);

                crmServiceHelper.IOrganizationService.Delete("annotation", annotation.Id);
                var body = annotation.GetAttributeValue<string>("documentbody");
                var content = Convert.FromBase64String(body);

                loggerHelper.traceInfo("Gembox operation start");
                ComponentInfo.SetLicense(StaticHelper.GetConfiguration("GemboxSerial"));
                DocumentModel document = DocumentModel.Load(new MemoryStream(content));
                var memoryStream = new MemoryStream();
                document.Save(memoryStream, SaveOptions.PdfDefault);
                loggerHelper.traceInfo("Gembox operation end");

                byte[] result;
                using (var streamReader = new MemoryStream())
                {
                    memoryStream.CopyTo(streamReader);
                    result = streamReader.ToArray();
                }

                AzureBlobStorageHelper azureBlobStorageHelper = new AzureBlobStorageHelper(StaticHelper.GetConfiguration("ConnectionString"));
                azureBlobStorageHelper.AddToBlobSTorage("digitalsignaturecontracts",
                                                        contract.GetAttributeValue<string>("rnt_contractnumber") + "\\unsigned",
                                                        contract.Id,
                                                        result,
                                                        "application/pdf");



                var baseURL = configurationBL.GetConfigurationByName("blobstorage_baseurl");
                var documentObj = new DocumentObject
                {
                    ContractNumber = contract.GetAttributeValue<string>("rnt_contractnumber"),
                    contractId = contract.Id,
                    pnrNumber = contract.GetAttributeValue<string>("rnt_pnrnumber"),
                    customerName = contract.GetAttributeValue<EntityReference>("rnt_customerid").Name,
                    pickupBranchId = contract.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id,
                    downloadURL = string.Format("{0}digitalsignaturecontracts/{1}/unsigned/{2}", baseURL, contract.GetAttributeValue<string>("rnt_contractnumber"), contract.Id)
                };
                System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", HttpContext.Current.Server.MapPath("~") + @"/Security/rentgo-7b5a5-firebase-adminsdk-5h9bz-ed47fd290b.json");
                try
                {
                    var app = FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.GetApplicationDefault()
                    });
                }
                catch
                {
                }
                var firebaseTask = insertFirebase(documentObj, false);

                loggerHelper.traceInfo("firebase operation edn");
            }
            catch (Exception ex)
            {
                loggerHelper.traceInfo("exception : " + ex.Message);
                loggerHelper.traceInfo("parameters : " + JsonConvert.SerializeObject(documentOperationParameters));
                throw;
            }
        }

        [HttpPost]
        [Route("api/document/uploadsigneddocument")]
        public UploadSignedDocumentResponse uploadSignedDocument(UploadSignedDocumentParameter uploadSignedDocumentParameter)
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            var client = new MongoClient(StaticHelper.GetConfiguration("MongoDBHostName"));
            var database = client.GetDatabase(StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            var collection = database.GetCollection<UploadSignedDocumentParameter>("SignedDocument");
            collection.Insert(uploadSignedDocumentParameter, "", "");

            if (string.IsNullOrEmpty(uploadSignedDocumentParameter.documentContent))
            {
                return new UploadSignedDocumentResponse
                {
                    responseResult = RntCar.ClassLibrary._Web.ResponseResult.ReturnError("Document is empty")
                };
            }

            try
            {
                var conractNumber = uploadSignedDocumentParameter.fileName.Split('.')[0];
                Guid contractId = Guid.Empty;
                Guid.TryParse(conractNumber, out contractId);
                ContractRepository contractRepository = new ContractRepository(crmServiceHelper.IOrganizationService);
                Entity contract = new Entity();
                if (contractId != Guid.Empty)
                {
                    contract = contractRepository.getContractById(contractId, new string[] { "rnt_contractnumber",
                                                                                                            "rnt_pickupbranchid",
                                                                                                            "rnt_customerid",
                                                                                                            "rnt_pnrnumber"});
                }
                else
                {
                    contract = contractRepository.getContractByContractNumber(conractNumber, new string[] { "rnt_contractnumber",
                                                                                                            "rnt_pickupbranchid",
                                                                                                            "rnt_customerid",
                                                                                                            "rnt_pnrnumber"});
                }

                //ContractCacheClient contractCacheClient = new ContractCacheClient();
                //contractCacheClient.setContractDocumentCache(contract.GetAttributeValue<string>("rnt_contractnumber"), uploadSignedDocumentParameter.documentContent);

                ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                var baseURL = configurationBL.GetConfigurationByName("blobstorage_baseurl");

                AzureBlobStorageHelper azureBlobStorageHelper = new AzureBlobStorageHelper(StaticHelper.GetConfiguration("ConnectionString"));
                azureBlobStorageHelper.AddToBlobSTorage("digitalsignaturecontracts",
                                                        contract.GetAttributeValue<string>("rnt_contractnumber") + "\\signed",
                                                        contract.Id,
                                                        Convert.FromBase64String(uploadSignedDocumentParameter.documentContent),
                                                        "application/pdf");

                var documentObj = new DocumentObject
                {
                    ContractNumber = contract.GetAttributeValue<string>("rnt_contractnumber"),
                    contractId = contract.Id,
                    pnrNumber = contract.GetAttributeValue<string>("rnt_pnrnumber"),
                    customerName = contract.GetAttributeValue<EntityReference>("rnt_customerid").Name,
                    pickupBranchId = contract.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id,
                    downloadURL = string.Format("{0}digitalsignaturecontracts/{1}/signed/{2}", baseURL, contract.GetAttributeValue<string>("rnt_contractnumber"), contract.Id)
                };

                System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", HttpContext.Current.Server.MapPath("~") + @"/Security/rentgo-7b5a5-firebase-adminsdk-5h9bz-ed47fd290b.json");
                try
                {
                    var app = FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.GetApplicationDefault()
                    });
                }
                catch
                {
                }
                var firebaseTask = insertFirebase(documentObj, true);

                //loggerHelper.traceInfo("firebase operation end" + JsonConvert.SerializeObject(firebaseTask));

                Entity e = new Entity("rnt_contract");
                e["rnt_digitalsignatureurl"] = documentObj.downloadURL;
                e.Id = contract.Id;
                crmServiceHelper.IOrganizationService.Update(e);

                //loggerHelper.traceInfo(JsonConvert.SerializeObject(uploadSignedDocumentParameter));
                return new UploadSignedDocumentResponse
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {

                return new UploadSignedDocumentResponse
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        private async System.Threading.Tasks.Task<IList<WriteResult>> insertFirebase(DocumentObject documentObject, bool isSigned)
        {
            FirestoreDb db = FirestoreDb.Create(StaticHelper.GetConfiguration("FireStoreDbName"));
            WriteBatch batch = db.StartBatch();

            DocumentReference branchRef = db.Collection("branches")
                                            .Document(Convert.ToString(documentObject.pickupBranchId));
            Dictionary<string, object> branchs = new Dictionary<string, object>
                {
                    {"updatedon" , DateTime.UtcNow.converttoTimeStamp()},

                };
            batch.Set(branchRef, branchs);

            DocumentReference contractRef = branchRef.Collection("contracts")
                                                     .Document(documentObject.contractId.ToString());

            Dictionary<string, object> contracts = new Dictionary<string, object>
                {
                    {"downloadPath", documentObject.downloadURL },
                    {"isSigned" ,isSigned},
                    {"pnrNumber" ,documentObject.pnrNumber },
                    {"customerName" , documentObject.customerName.ToString()},
                    {"contractNumber", documentObject.ContractNumber },
                };

            batch.Set(contractRef, contracts);

            var a = await batch.CommitAsync();
            return a;
        }
    }
}
