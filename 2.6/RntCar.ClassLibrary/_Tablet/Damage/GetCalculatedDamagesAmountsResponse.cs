﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class GetCalculatedDamagesAmountsResponse : ResponseBase
    {
        public AdditonalProductDataTablet damageProduct { get; set; }
        public List<DamageData> damageList { get; set; }
    }
}
