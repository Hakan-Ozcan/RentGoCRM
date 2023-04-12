using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class GroupCodeImageData
    {
        public Guid groupCodeImageId { get; set; }
        public string imageUrl { get; set; }
        public Guid groupCodeInformationId { get; set; }
        public string groupCodeInformationName { get; set; }
    }
}
