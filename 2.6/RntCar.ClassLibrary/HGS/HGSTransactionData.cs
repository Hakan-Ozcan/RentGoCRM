using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class HGSTransactionData
    {
        public long tranDateTime { get; set; }
        public DateTime _tranDateTime { get; set; }
        public string tranCode { get; set; }
        public string tranDescription { get; set; }
        public decimal tranAmount { get; set; }
        public UserInfo user { get; set; }
    }

    public class UserInfo
    {
        public string BankCode { get; set; }
        public string SubOrganisation { get; set; }
        public string HeadOfficeNo { get; set; }
        public string BranchNo { get; set; }
        public string BranchName { get; set; }
        public string UserCode { get; set; }
        public string UserName { get; set; }
        public string UserSurname { get; set; }
        public string BoxOffice { get; set; }
        public string BranchCityCode { get; set; }
        public string BranchTownName { get; set; }
    }
}
