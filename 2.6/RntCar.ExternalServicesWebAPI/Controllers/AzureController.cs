using RntCar.AzureBlobStorage;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace RntCar.ExternalServicesWebAPI.Controllers
{
    public class AzureController : ApiController
    {
        /// <summary>
        /// Ana veri servisi
        /// </summary>
        /// <param></param>
        /// <returns></returns> 
        [HttpPost]
        [HttpGet]
        [Route("api/azure/testme")]
        public string testme()
        {
            return "i am ok";
        }
        /// <summary>
        /// Ana veri servisi
        /// </summary>
        /// <param name="parameters">Parametre Bilgisi</param>
        /// <returns></returns> 
        [HttpPost]
        [Route("api/azure/getBlobUrlsByDirectory")]
        public GetBlobUrlsByDirectoryResponse getBlobUrlsByDirectory([FromBody]GetBlobUrlsByDirectoryParameter parameters)
        {
            try
            {
                var connString = StaticHelper.GetConfiguration("ConnectionString");
                AzureBlobStorageHelper azureBlobStorageHelper = new AzureBlobStorageHelper(connString);
                var documentPaths = azureBlobStorageHelper.getGetDocumentsUrlByContainerandDirectory(parameters.containerName, parameters.directoryPath);
                return new GetBlobUrlsByDirectoryResponse
                {
                    ResponseResult = ResponseResult.ReturnSuccess(),
                    documentPathList = documentPaths
                };
            }
            catch (Exception ex)
            {
                return new GetBlobUrlsByDirectoryResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        /// <summary>
        /// Ana veri servisi
        /// </summary>
        /// <param name="parameters">Parametre Bilgisi</param>
        /// <returns></returns> 
        [HttpPost]
        [Route("api/azure/generateImage")]
        public ImageGeneratorResponse generateImage([FromBody]ImageGeneratorParameter parameters)
        {
            try
            {
                var connString = StaticHelper.GetConfiguration("ConnectionString");
                AzureBlobStorageHelper azureBlobStorageHelper = new AzureBlobStorageHelper(connString);
                var response = azureBlobStorageHelper.AddToBlobSTorage("cmsimages", 
                                                                       parameters.Type.removeEmptyCharacters(), 
                                                                       Guid.NewGuid(), 
                                                                       Convert.FromBase64String(parameters.Image.Split(',')[1]));

                return new ImageGeneratorResponse
                {
                    Url = response,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new ImageGeneratorResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }
    }
}
