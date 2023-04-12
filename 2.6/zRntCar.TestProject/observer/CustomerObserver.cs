using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zRntCar.TestProject.observer
{
    class CustomerObserver : Observer
    {
        public override void Update()
        {
            Console.WriteLine("Takip ettiğim ürünün stoğu değişti.");
            Console.ReadLine();
        }
    }
}
