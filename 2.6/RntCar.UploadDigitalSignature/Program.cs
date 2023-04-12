using Microsoft.Xrm.Sdk.Query;
using MongoDB.Driver;
using RntCar.AzureBlobStorage;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.Logger;
using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using System.Linq;
using System.Net;

namespace RntCar.UploadDigitalSignature
{
    class Program
    {
        static void Main(string[] args)
        {
            LoggerHelper logger = new LoggerHelper();
            var client = new MongoClient(StaticHelper.GetConfiguration("MongoDBHostName"));
            var database = client.GetDatabase(StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            var collection = database.GetCollection<UploadSignedDocumentParameter>("SignedDocument");
            var s = new CrmServiceHelper().IOrganizationService;

            ContractRepository contractRepository = new ContractRepository(s);
            var contracts = contractRepository.getCreatedContractsByXlastDays(2);
            logger.traceInfo("started");
            logger.traceInfo("contracts found : "  + contracts.Count);

            foreach (var item in contracts)
            {
                try
                {
                    AzureBlobStorageHelper azureBlobStorageHelper = new AzureBlobStorageHelper("DefaultEndpointsProtocol=https;AccountName=rentgoproductionstorage;AccountKey=d3zPZ07IAZROSuLKQDYHH9jPzrqz8GJMGbbUojhSJONNDPEc8mpyloGZnrOoTDGATqrx+rpyU2k7RWZ7ZiZ5kw==;EndpointSuffix=core.windows.net");
                    var url = azureBlobStorageHelper.getGetDocumentsUrlByContainerandDirectory("digitalsignaturecontracts", item.GetAttributeValue<string>("rnt_name"));

                    foreach (var iUrl in url)
                    {
                        if (!iUrl.Contains("unsigned"))
                        {
                            using (WebClient wc = new WebClient())
                            {
                                byte[] data = wc.DownloadData(iUrl);

                                if (data.Length == 0)
                                {
                                    logger.traceInfo(item.GetAttributeValue<string>("rnt_name") + " is empty");
                                    var document = database.GetCollection<UploadSignedDocumentParameterMongoDB>("SignedDocument")
                                                   .AsQueryable()
                                                   .Where(p => p.fileName.Contains(item.GetAttributeValue<string>("rnt_name")) &&
                                                               !string.IsNullOrEmpty(p.documentContent)).FirstOrDefault();

                                    if (document != null)
                                    {
                                        var par = new UploadSignedDocumentParameter
                                        {
                                            fileName = document.fileName,
                                            documentContent = document.documentContent
                                        };

                                        RestSharpHelper restSharpHelper = new RestSharpHelper("http://localhost:6060/api/document", "uploadsigneddocument", RestSharp.Method.POST);


                                        restSharpHelper.AddJsonParameter<UploadSignedDocumentParameter>(par);

                                        var response = restSharpHelper.Execute<UploadSignedDocumentResponse>();

                                        logger.traceInfo("document recovered");
                                    }

                                    else
                                    {
                                        logger.traceInfo("document couldnt found in mongodb");
                                    }
                                }
                            }
                        }

                    }
                }
                catch
                {
                    continue;
                }            
            }

            logger.traceInfo("end");

        }
    }
}
