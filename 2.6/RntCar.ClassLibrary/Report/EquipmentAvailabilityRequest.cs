﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class EquipmentAvailabilityRequest : ReportRequest
    {
        public long StartDate { get; set; }
        public long EndDate { get; set; }
    }
}