using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class LoginResponse : ResponseBase
    {
        public string fullname { get; set; }
        public Branch userBranch { get; set; }
        public Guid userId { get; set; }
        public int earlyCloseTime { get; set; }
    }
}