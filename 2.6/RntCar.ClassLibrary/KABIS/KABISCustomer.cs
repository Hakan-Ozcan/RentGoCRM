using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class KABISCustomer
    {
        public KABISCustomer(string type, string ıdentity, string licenseNo, string nationality, string name, string surname, string fatherName, string motherName, string placeOfBirth, int yearOfBirth)
        {
            Type = type;
            Identity = ıdentity;
            LicenseNo = licenseNo;
            Nationality = nationality;
            Name = name;
            Surname = surname;
            FatherName = fatherName;
            MotherName = motherName;
            PlaceOfBirth = placeOfBirth;
            YearOfBirth = yearOfBirth;
        }

        public KABISCustomer() { }

        public string Type { get; set; } //  Türk Vatandaşı Türk Ehliyeti Olan(TC), Yabancı Uyruklu Yabancı Ehliyeti Olan(YU), Türk Vatandaşı Yabancı Ehliyeti Olan(TY)  
        public string Identity { get; set; } // Pasaport/TC No 
        public string LicenseNo { get; set; }// Yabanci ülke ehliyet no
        public string Nationality { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string PlaceOfBirth { get; set; }
        public int YearOfBirth { get; set; }
    }
}
