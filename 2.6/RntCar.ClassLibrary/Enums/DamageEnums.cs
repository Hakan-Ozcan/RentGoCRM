using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class DamageEnums
    {
        public enum StatusCode
        {
            Open = 1,
            Repaired = 100000001,
            Cancelled = 2 // inactive
        }
    }
}
