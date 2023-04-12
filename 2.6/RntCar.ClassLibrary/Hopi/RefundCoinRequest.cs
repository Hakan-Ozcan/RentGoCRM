﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class RefundCoinRequest
    {
        public string licenceKey { get; set; }
        public string merchantCode { get; set; }
        public string storeCode { get; set; }
        public long provisionId { get; set; }
        public decimal amount { get; set; }
        public string cashDeskTag { get; set; }
    }
}
