using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using System;

namespace RntCar.BusinessLibrary.Business
{
    // Tolga AYKURT - 04.03.2019
    public class BlackListBL: BusinessHandler
    {
        #region CONSTRUCTORS
        public BlackListBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public BlackListBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }
        #endregion

        #region METHODS
        // Tolga AYKURT - 04.03.2019
        public string BlackListValidationReturnString(string identityKey)
        {
            var response = new BlackListValidationResponse();
            response.ResponseResult.Result = true;
            response.BlackList = new BlackListData();

            try
            {
                var blackListRepository = new BlackListRepository(this.OrgService);
                var blackListEntity = blackListRepository.GetBlackList(identityKey);

                if(blackListEntity != null)
                {
                    response.BlackList.IsInBlackList = true;
                    response.BlackList.Note = blackListEntity.GetAttributeValue<string>("rnt_note");
                    response.BlackList.Reason = blackListEntity.GetAttributeValue<OptionSetValue>("rnt_blacklistreason").Value;
                }
                else
                {
                    response.BlackList.IsInBlackList = false;
                }
            }
            catch(Exception ex)
            {
                response.ResponseResult.Result = false;
                response.ResponseResult.ExceptionDetail = ex.Message;
            }

            return JsonConvert.SerializeObject(response);
        }

        public BlackListValidationResponse BlackListValidation(string identityKey)
        {
            var response = new BlackListValidationResponse();
            response.ResponseResult.Result = true;
            response.BlackList = new BlackListData();

            try
            {
                var blackListRepository = new BlackListRepository(this.OrgService);
                var blackListEntity = blackListRepository.GetBlackList(identityKey);

                if (blackListEntity != null)
                {
                    response.BlackList.IsInBlackList = true;
                    response.BlackList.Note = blackListEntity.GetAttributeValue<string>("rnt_note");
                    response.BlackList.Reason = blackListEntity.GetAttributeValue<OptionSetValue>("rnt_blacklistreason").Value;
                }
                else
                {
                    response.BlackList.IsInBlackList = false;
                }
            }
            catch (Exception ex)
            {
                response.ResponseResult.Result = false;
                response.ResponseResult.ExceptionDetail = ex.Message;
            }

            return response;
        }
        #endregion
    }
}
