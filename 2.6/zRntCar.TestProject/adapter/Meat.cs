using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zRntCar.TestProject.adapter
{
    public class Meat
    {
        protected string MeatName;
        protected float SafeCookTempFahrenheit;
        protected float SafeCookTempCelsius;
        protected double CaloriesPerOunce;
        protected double ProteinPerOunce;

        // Constructor
        public Meat(string meat)
        {
            this.MeatName = meat;
        }

        public virtual void LoadData()
        {
            Console.WriteLine("\nMeat: {0} ------ ", MeatName);
        }
    }
}
