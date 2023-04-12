using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class GetBlobUrlsByDirectoryResponse : ResponseBase
    {
        public List<string> documentPathList { get; set; }
    }
}
