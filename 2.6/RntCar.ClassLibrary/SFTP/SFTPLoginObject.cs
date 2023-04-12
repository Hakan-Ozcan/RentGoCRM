using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class SFTPObject
    {
        public string sftphostName { get; set; }
        public int sftpPort { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        public string sourceFilePath { get; set; }
        public string targetFilePath { get; set; }
    }
}
