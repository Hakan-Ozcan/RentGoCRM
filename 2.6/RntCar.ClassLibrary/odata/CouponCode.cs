﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.odata
{
    public class CouponCode
    {
        [Key]
        public Guid couponCodeId { get; set; }
                   
        public string accountId { get; set; }
                   
        public string contactId { get; set; }
                   
        public string contractId { get; set; }
                   
        public string reservationId { get; set; }
                   
        public string couponCodeDefinitionId { get; set; }

        public string name { get; set; }

        public string couponCode { get; set; }

        public int statusCode { get; set; }

    }
}
