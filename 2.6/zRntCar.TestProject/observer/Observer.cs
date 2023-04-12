using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zRntCar.TestProject.observer
{
    public abstract class Observer
    {
        // Herhangi bir değişimde gözlemleyiciler tarafından yapılması istenilen aksiyonlar.
        public abstract void Update();
    }
}
