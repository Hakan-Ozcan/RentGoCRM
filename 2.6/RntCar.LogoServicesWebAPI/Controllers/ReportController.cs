using RntCar.LogoServicesWebAPI.Helpers;
using RntCar.LogoServicesWebAPI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RntCar.LogoServicesWebAPI.Controllers
{
    public class ReportController : ApiController
    {
        [HttpGet]
        [Route("api/report/incomeandexpenseone")]
        public List<IncomeAndExpense> IncomeAndExpenseOne()
        {
            try
            {
                var connectionStr = ConfigurationManager.AppSettings["SQLConnectionString"];
                SqlDataAccess sda = new SqlDataAccess();
                sda.OpenConnection(connectionStr);
                var sqlQuery = "SELECT * FROM ARY_001_GELIR_GIDER";
                var dt = sda.GetDataTable(sqlQuery);

                var returnValue = ReportHelper.ConvertListIncomeAndExpense(dt);
                return returnValue;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpGet]
        [Route("api/report/incomeandexpensetwo")]
        public List<IncomeAndExpense> IncomeAndExpenseTwo()
        {
            try
            {
                var connectionStr = ConfigurationManager.AppSettings["SQLConnectionString"];
                SqlDataAccess sda = new SqlDataAccess();
                sda.OpenConnection(connectionStr);
                var sqlQuery = "SELECT * FROM ARY_002_GELIR_GIDER";
                var dt = sda.GetDataTable(sqlQuery);

                var returnValue = ReportHelper.ConvertListIncomeAndExpense(dt);
                return returnValue;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpGet]
        [Route("api/report/incomeandexpensethree")]
        public List<IncomeAndExpense> IncomeAndExpenseThree()
        {
            try
            {
                var connectionStr = ConfigurationManager.AppSettings["SQLConnectionString"];
                SqlDataAccess sda = new SqlDataAccess();
                sda.OpenConnection(connectionStr);
                var sqlQuery = "SELECT * FROM ARY_003_GELIR_GIDER";
                var dt = sda.GetDataTable(sqlQuery);

                var returnValue = ReportHelper.ConvertListIncomeAndExpense(dt);
                return returnValue;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
