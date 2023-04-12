using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class LoginParameters : RequestBase
    {
        public string userName { get; set; }
        public string password { get; set; }
        public string version { get; set; }
    }
}
