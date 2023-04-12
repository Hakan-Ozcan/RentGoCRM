using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary.Report;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.CrmPlugins.Report.Actions
{
    public class ExecuteGetadditionalProductsforBranch : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer pluginInitializer = new PluginInitializer(serviceProvider);

            try
            {
                string GetadditionalProductsforBranchRequest;
                pluginInitializer.PluginContext.GetContextInputParameter<string>("GetadditionalProductsforBranchRequest", out GetadditionalProductsforBranchRequest);
                pluginInitializer.TraceMe("GetadditionalProductsforBranchRequest: " + GetadditionalProductsforBranchRequest);

                var serialized = JsonConvert.DeserializeObject<GetadditionalProductsforBranchRequest>(GetadditionalProductsforBranchRequest);

                SystemUserRepository systemUserRepository = new SystemUserRepository(pluginInitializer.Service);
                var user = systemUserRepository.getSystemUserByIdWithGivenColumns(new Guid(serialized.id), new string[] { "businessunitid", "rnt_businessrolecode" });
                pluginInitializer.TraceMe("user " + user.Id);

                BranchRepository branchRepository = new BranchRepository(pluginInitializer.Service);
                var branch = branchRepository.getBranchByBusinessUnitId(user.GetAttributeValue<EntityReference>("businessunitid").Id);

                pluginInitializer.TraceMe("branch " + branch.First().Id);

                var lastDay = int.MaxValue;
                if (serialized.month == DateTime.Now.Month)
                {
                    lastDay = DateTime.Now.Date.Day;
                }
                else
                {
                    lastDay = DateTime.DaysInMonth(DateTime.Now.Year, serialized.month);
                }
                pluginInitializer.TraceMe("lastDay " + lastDay);
                ContractItemRepository contractItemRepository = new ContractItemRepository(pluginInitializer.Service);
                var items = contractItemRepository.getAdditionalProductsByGivenDateByBranch(branch.FirstOrDefault().Id, lastDay.ToString(), serialized.month.ToString(), DateTime.Now.Year.ToString());

                pluginInitializer.TraceMe("items count " + items.Count);
                //get users belong this branch
                var users = systemUserRepository.getSystemUsersByBranch(branch.FirstOrDefault().Id);
                //rezervasyonu baska subeden acılmıs olabilir
                var allusers = systemUserRepository.getAlluserExceptWebUser();
                GetadditionalProductsforBranchResponse getadditionalProductsforBranchResponse = new GetadditionalProductsforBranchResponse
                {
                    showallRecords = user.GetAttributeValue<OptionSetValue>("rnt_businessrolecode").Value == (int)rnt_BonusBusinessRole.SatDanman ? false : true
                };
                foreach (var _user in users)
                {
                    getadditionalProductsforBranchResponse.users.Add(new GetadditionalProductsforBranch_User
                    {
                        userId = _user.Id,
                        userName = _user.GetAttributeValue<string>("fullname")
                    });
                    
                    var res = items.Where(p => p.GetAttributeValue<EntityReference>("createdby").Id == _user.Id).ToList();
                    var otherUsers = allusers.Where(p => p.Id != _user.Id).ToList();

                    if (res.Count > 0)
                    {
                        //rezervasyonu olusturan sözleşmeyi olusturandan farklıysa listeden cıkaralım
                        var resAdditionalProducts = res.Select(p => new
                        {
                            resCreatedId =p.Contains("a_a715b173c7a8e911a840000d3a2ddc03.createdby") ?
                                ((EntityReference)((AliasedValue)p["a_a715b173c7a8e911a840000d3a2ddc03.createdby"]).Value).Id : Guid.Empty,
                            additionalProductId = p.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id,
                            additionalProductName = p.GetAttributeValue<EntityReference>("rnt_additionalproductid").Name,
                            fullName = _user.GetAttributeValue<string>("fullname"),
                            userId = _user.Id,
                            amount = p.GetAttributeValue<Money>("rnt_totalamount").Value,
                            businessRoleCode = _user.GetAttributeValue<OptionSetValue>("rnt_businessrolecode").Value
                        }).ToList();

                        resAdditionalProducts = resAdditionalProducts.Where(p => !otherUsers.Any(l => p.resCreatedId == l.Id)).ToList();
                        
                        List<GetadditionalProductsforBranchData> d = new List<GetadditionalProductsforBranchData>();
                        foreach (var item in resAdditionalProducts)
                        {
                            GetadditionalProductsforBranchData _d = new GetadditionalProductsforBranchData
                            {
                                additionalProductId = item.additionalProductId,
                                additionalProductName = item.additionalProductName,
                                amount = item.amount,
                                userId = item.userId,
                                businessRoleCode = item.businessRoleCode,
                                fullName = item.fullName
                            };
                            d.Add(_d);
                        }

                        var grouped = d.GroupBy(y => y.additionalProductId)
                                     .Select(
                                         g => new GetadditionalProductsforBranchData
                                         {
                                             additionalProductId = g.Key,
                                             amount = g.Sum(s => s.amount),
                                             additionalProductName = g.First().additionalProductName,
                                             businessRoleCode = g.First().businessRoleCode,
                                             userId = g.First().userId,
                                             fullName = g.First().fullName,
                                         });


                        getadditionalProductsforBranchResponse.data.AddRange(grouped);
                    }

                }
                pluginInitializer.TraceMe("users count " + users.Count);                
                getadditionalProductsforBranchResponse.ownData.AddRange(getadditionalProductsforBranchResponse.data.Where(p => p.userId == new Guid(serialized.id)).ToList());


                pluginInitializer.TraceMe("getadditionalProductsforBranchResponse: " + JsonConvert.SerializeObject(getadditionalProductsforBranchResponse));

                pluginInitializer.PluginContext.OutputParameters["GetadditionalProductsforBranchResponse"] = JsonConvert.SerializeObject(getadditionalProductsforBranchResponse);
            }
            catch (Exception ex)
            {
                pluginInitializer.TraceMe("error : " + ex.Message);
                pluginInitializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
