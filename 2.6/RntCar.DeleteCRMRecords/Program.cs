using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.Logger;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.DeleteCRMRecords
{
    internal class Program
    {
        static void Main(string[] args)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            //string contractLastX = StaticHelper.GetConfiguration("contractLastX");
            // Set the number of records per page to retrieve.
            int queryCount = 5000;
            // Initialize the page number.
            int pageNumber = 1;
            string pagingCookie = null;
            string entityName = StaticHelper.GetConfiguration("entityname");
            DateTime lastDate = Convert.ToDateTime(StaticHelper.GetConfiguration("lastdate"));
            EntityCollection deleteList = new EntityCollection();
            while (true)
            {
                QueryExpression queryExpression = new QueryExpression(entityName);
                queryExpression.ColumnSet = new ColumnSet(true);
                queryExpression.Criteria.AddCondition("createdon", ConditionOperator.OnOrBefore, lastDate);
                queryExpression.AddOrder("createdon", OrderType.Ascending);
                queryExpression.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                var l = crmServiceHelper.IOrganizationService.RetrieveMultiple(queryExpression);
                if (l.MoreRecords)
                {
                    deleteList.Entities.AddRange(l.Entities);
                    pageNumber++;
                    pagingCookie = l.PagingCookie;
                }
                else
                {
                    deleteList.Entities.AddRange(l.Entities);
                    break;
                }

            }
            foreach (Entity deleteEntity in deleteList.Entities)
            {
                try
                {
                    crmServiceHelper.IOrganizationService.Delete(deleteEntity.LogicalName, deleteEntity.Id);
                }
                catch (Exception ex)
                {
                    loggerHelper.traceInfo("records not deleted entity : " + ex.Message);
                }
            }
        }
    }
}
