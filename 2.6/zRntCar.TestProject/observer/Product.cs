using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zRntCar.TestProject.observer
{
    public class Product
    {
        // Gözlemleyicilerimizi tutacağımız listemiz.
        private List<Observer> _observers = new List<Observer>();

        public void Attach(Observer observer)
        {
            _observers.Add(observer);
        }

        public void Detach(Observer observer)
        {
            _observers.Remove(observer);
        }

        private void Notify()
        {
            // Herhangi bir değişiklik olduğunda gözlemleyicilerimizin Update metotunu tetikleterek istenilen aksiyonu gerçekleştirebiliriz. Örneğin: Kullanıcılara e-posta atmak gibi düşünebilirsiniz.
            _observers.ForEach(o => { o.Update(); });
        }

        public void ChangeStock()
        {
            //... stok değiştirilme işlemleri
            // Stok değiştirildiğinde gözlemcilerimize bildiriyoruz.
            this.Notify();
        }
    }
}
