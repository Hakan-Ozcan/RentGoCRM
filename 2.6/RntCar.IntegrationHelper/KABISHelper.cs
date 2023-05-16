using mshtml;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using Tamir.SharpSsh.java;
using Tesseract;

namespace RntCar.IntegrationHelper
{
    public class KABISHelper
    {
        private string baseURL = "https://arackiralama.egm.gov.tr";
        private string idleVehiclesPage = "/frm_genel.aspx";
        private string rentedVehiclePage = "/frm_kiradakiaraclar.aspx";
        private string rentVehiclePage = "/frm_arac_kirala.aspx";
        private string returnVehiclePage = "/frm_arac_iade.aspx";
        private string addRemoveVehiclePage = "/frm_arac_ekle.aspx";
        Thread thread;
        private WebBrowser webBrowser;
        bool isCompleted = false;

        private static string currentUrl = "";

        public KABISHelper()
        {
            thread = new Thread(new ThreadStart(createWebBrowser));// Yeni bir Thread örneği oluşturulur ve createWebBrowser adlı metod, ThreadStart delegesi kullanılarak Thread sınıfının yapıcısına aktarılır. Bu, yeni bir iş parçacığı oluşturmak ve web tarayıcısını bu iş parçacığında çalıştırmak için kullanılır.
            thread.SetApartmentState(ApartmentState.STA);//Yeni oluşturulan iş parçacığı, STA (Single-Threaded Apartment) apartman durumuna ayarlanır. STA, COM nesnelerinin oluşturulmasını ve kullanılmasını kolaylaştırır.
            thread.Start();//Yeni oluşturulan iş parçacığı başlatılır ve createWebBrowser metodu çağrılır. Bu, iş parçacığı tarafından çalıştırılan web tarayıcısının oluşturulmasını ve yüklenmesini sağlar.
        }
        void createWebBrowser()//Bu metodun amacı, WebBrowser nesnesi oluşturmak, belirtilen başlangıç URL'sine gitmek, ScriptErrorsSuppressed özelliğini ayarlamak, WebBrowser nesnesi için gerekli olayları işlemek ve web sayfasının tamamen yüklenmesini beklemektir. Bu metot, WebBrowser nesnesinin başlangıçta yüklenmesi ve kullanıma hazır olması için gereklidir
        {
            webBrowser = new WebBrowser();
            webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;//WebBrowser nesnesinin DocumentCompleted olayı WebBrowser_DocumentCompleted adlı bir işlevle eşleştirilir. DocumentCompleted olayı, web sayfası tamamen yüklendiğinde tetiklenir ve bu olayın işlenmesi gerekebilir.
            webBrowser.Navigate(baseURL);//WebBrowser nesnesi, belirtilen baseURL değişkenindeki URL'ye gitmek için kullanılır. Bu, WebBrowser nesnesinin görüntüleyeceği ilk sayfayı belirler.
            webBrowser.NewWindow += WebBrowser_NewWindow;//WebBrowser nesnesinin NewWindow olayı, WebBrowser_NewWindow adlı bir işlevle eşleştirilir. Bu olay, bir kullanıcının bir web sayfasında yeni bir pencere açması gerektiğinde tetiklenir.


            webBrowser.ScriptErrorsSuppressed = true;//WebBrowser nesnesinin ScriptErrorsSuppressed özelliği, web sayfalarında oluşabilecek hata mesajlarının gösterilip gösterilmeyeceğini belirler. Bu özelliğin true olarak ayarlanması, web sayfasındaki hata mesajlarının görüntülenmeyeceği anlamına gelir.
            isCompleted = false;
            do
            {
                Thread.Sleep(10);//Döngü her bir adımda 10 milisaniye bekler. Bu, döngünün çok hızlı çalışmasını önler ve sistem kaynaklarını serbest bırakır.
                Application.DoEvents(); //Bu yöntem, Windows işletim sistemi işlem döngüsünü yeniler ve WebBrowser nesnesinin yüklenmesi için gerekli olan olayları işler.
            } while (!isCompleted);// Döngü, isCompleted değişkeni true olduğunda sona erer. isCompleted değişkeni, WebBrowser_DocumentCompleted işlevi içinde belirtilir ve web sayfasının tamamen yüklenmesinin ardından true değerine ayarlanır. Bu nedenle, döngü web sayfasının tamamen yüklenmesini bekler.
        }


        public KABISResponse RentVehicleThreadProcess(KABISIntegrationUser OfficeUser, KABISCustomer rentCustomer, string plate, int vehicleExitKm)
        //Bu kod bloğu, bir araç kiralama işlemi yürütmek için yeni bir thread başlatır ve sonucu döndürür. 
        //Bu metot, bir KABISIntegrationUser, KABISCustomer, bir araç plakası ve çıkış kilometre bilgisi parametrelerini alır ve bir KABISResponse nesnesi döndürür.
        {
            KABISResponse response = null;
            Thread thread = new Thread(() => { response = RentVehicle(OfficeUser, rentCustomer, plate, vehicleExitKm); });// Yeni bir Thread nesnesi oluşturuluyor ve lambda ifadesi kullanılarak bir fonksiyon tanımlanıyor. Bu fonksiyon RentVehicle metodunu çağırarak response nesnesini doldurur. Bu, RentVehicle metodunun ayrı bir iş parçacığında çalıştırılmasını sağlar.
            thread.SetApartmentState(ApartmentState.STA);//Yeni oluşturulan iş parçacığı, STA (Single-Threaded Apartment) apartman durumuna ayarlanır. STA, COM nesnelerinin oluşturulmasını ve kullanılmasını kolaylaştırır.
            thread.Start();// Yeni oluşturulan iş parçacığı başlatılır ve RentVehicle metodu çağrılır.
            thread.Join();//İş parçacığı ana iş parçacığına katılmak üzere bekletilir. Bu, RentVehicle metodunun işlemi tamamlaması beklenir ve response nesnesinin doldurulmasını sağlar.

            return response;

        }
        public KABISResponse ReturnVehicleThreadProcess(KABISIntegrationUser OfficeUser, string plate, int entranceKm)
        {//Bu kod bloğu, bir araç dönüş işlemini yürütmek için yeni bir thread başlatır ve sonucu döndürür. 
            KABISResponse response = null;
            Thread thread = new Thread(() => { response = ReturnVehicle(OfficeUser, plate, entranceKm); });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();//Bu satır, ana iş parçacığını yeni thread'in tamamlanmasını beklemeye zorlar.

            return response;
        }
        public KABISResponse RemoveVehicleThreadProcess(KABISIntegrationUser OfficeUser, string plate)
        {
            //Bu metodun amacı, bir aracın KABIS sisteminden kaldırılması işlemini gerçekleştirmek için yeni bir thread oluşturmak ve bu işlemi asenkron olarak yürütmektir
            KABISResponse response = null;
            Thread thread = new Thread(() => { response = RemoveVehicle(OfficeUser, plate); });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            return response;
        }
        public KABISResponse AddVehicleThreadProcess(KABISIntegrationUser OfficeUser, string plate, string registrationNumber)
        //Bu kod bloğu, araç ekleme işlemini yeni bir iş parçacığında çalıştırmak için tasarlanmış bir yöntem içerir. 
        {
            KABISResponse response = null;
            Thread thread = new Thread(() => { response = AddVehicle(OfficeUser, plate, registrationNumber); });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            return response;
        }




        private bool Login(KABISIntegrationUser kABISIntegrationUser)
        //Bu kod bloğu, KABIS (Kara Araçları Bilgi Sistemi) entegrasyonu için kullanılan bir web sitesine oturum açmak için kullanılan Login() adlı bir metodun içeriğini içeriyor. Metod, belirtilen KABISIntegrationUser nesnesi ile kullanıcı adı ve şifre bilgilerini web sitesine gönderir ve ardından Captcha (otomatik test) doğrulamasını geçmek için ResolveCaptcha() adlı bir başka metod kullanır. Metod, web sayfası yüklemesinin tamamlanması ve giriş yapılması için tekrar tekrar deneme yapar ve sonuç olarak başarılı bir oturum açılıp açılmadığını belirten bir boolean değer döndürür.
        {
            try
            {
                isCompleted = false;
                WebBrowserNavigate(baseURL);
                for (int i = 0; i < 10; i++) // Login girişi başarısız ise 10kez tekrarlansın
                {
                    do
                    {
                        Thread.Sleep(10);
                        Application.DoEvents();
                    } while (!isCompleted);
                    var HomePage = webBrowser.Document.GetElementById("aspnetForm"); // Ana sayfa kontrolü için
                    //: Bu satır, aspnetForm ID'li bir öğeyi HomePage adlı bir değişkene atar. Bu, web sayfasında ana sayfanın yüklendiğini kontrol etmek için kullanılacak.
                    if (HomePage != null)
                    {
                        Console.WriteLine("Giriş Yapıldı");
                        return true;
                    }
                    var dialogForm = webBrowser.Document.GetElementById("dialog-duyuru");//Bu satır, dialog-duyuru ID'li bir öğeyi dialogForm adlı bir değişkene atar.
                    if (dialogForm != null)
                        dialogForm.OuterHtml = "";//Bu satır, dialogForm değişkeninin null olup olmadığını kontrol eder ve değişken null değilse OuterHtml özelliğine boş bir değer atar.
                    webBrowser.Document.GetElementById("username").InnerText = kABISIntegrationUser.userName;// Bu satır, username ID'li bir öğenin InnerText özelliğine kABISIntegrationUser nesnesinin kullanıcı adı değerini atar.
                    webBrowser.Document.GetElementById("password").InnerText = kABISIntegrationUser.Password;
                    webBrowser.Document.GetElementById("txtCaptcha").InnerText = ResolveCaptcha();
                    isCompleted = false; //isCompleted değişkeni, web sayfasının yüklenmesinin tamamlandığını belirtmek için kullanılır.
                    webBrowser.Document.GetElementById("btn_giris").InvokeMember("Click");//Bu satır, webBrowser adlı WebBrowser nesnesinin Document özelliğine erişerek, sayfadaki btn_giris ID'li öğenin Click olayını tetikler. Yani bu satır, web sayfasında giriş yapmak için kullanılan butonun tıklanmasını simüle eder. Bu şekilde kullanıcının giriş yapabilmesi için web sayfasının yüklenmesi ve butonun tıklanması gerekiyor.

                }
                Console.WriteLine("Giriş Yapılamadı");
                return false;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        private KABISResponse RentVehicle(KABISIntegrationUser OfficeUser, KABISCustomer rentCustomer, string plate, int vehicleExitKm)
        //Fonksiyon, KABISIntegrationUser tipinde bir kullanıcı, KABISCustomer tipinde bir müşteri, bir plaka numarası ve bir aracın kilometre cinsinden çıkış değerini alır ve bir KABISResponse tipinde yanıt döndürür.Fonksiyon, öncelikle bir WebBrowser nesnesi oluşturur ve ardından bu kullanıcının hesabına giriş yapar. Daha sonra, araç plakası girilerek boşta olan araçların sayfasında arama yapılır. Eğer araç bulunamazsa, kiralanmış araçların sayfasına gidilir ve orada da arama yapılır. Eğer araç burada da bulunamazsa, işlem başarısız sayılır ve hata mesajı ile birlikte KABISResponse nesnesi döndürülür.Araç kiralanmışsa, müşterinin kimlik bilgisi araç bilgileri ile eşleştirilir. Eğer eşleşirse, araç zaten kiralanmıştır ve başka bir müşteriye verilmiştir. Bu durumda, işlem başarısız sayılır ve KABISResponse nesnesi döndürülür.Eğer müşteri bilgileri eşleşmezse, araç geri alınır ve müşteri bilgileri yeniden girilir. Bu işlem için müşterinin kimlik tipine bağlı olarak farklı adımlar atılır. Eğer müşteri Türk ise, TC kimlik numarası ve çıkış kilometre bilgisi girilir. Eğer müşteri yabancı ise, pasaport numarası ve diğer kişisel bilgiler girilir.Müşteri bilgileri girildikten sonra, araç kiralanır ve işlem başarılı olarak tamamlanır.

        {
            try
            {
                createWebBrowser();

                if (!Login(OfficeUser))//Login() fonksiyonu, belirtilen OfficeUser kimlik bilgileri ile KABIS web sitesine giriş yapmaya çalışır. Başarılı bir giriş yapılırsa, true değeri döndürülür; aksi takdirde, false döndürülür.
                    return new KABISResponse() { Code = "300", ResponseResult = ResponseResult.ReturnError("Giriş Yapılamadı!!!") };

                if (!currentUrl.Contains(baseURL + idleVehiclesPage))//currentUrl değişkeni, webBrowser tarafından şu anda görüntülenen web sayfasının URL'sini içerir.
                    //idleVehiclesPage değişkeni, KABIS web sitesindeki boş araçlar sayfasının URL'sini içerir.
                    WebBrowserNavigate(baseURL + idleVehiclesPage);//WebBrowserNavigate() fonksiyonu, webBrowser örneği tarafından gösterilen web tarayıcısında belirtilen URL'yi açmaya çalışır.

                if (!SearchVehicle(plate))
                //SearchVehicle() fonksiyonu, KABIS hesabındaki araçları arar ve plate parametresindeki plaka numarasına sahip bir araç bulursa true döndürür. Aksi takdirde, false döndürülür.
                {
                    Console.WriteLine("Araç Bulunamadı. Kiralanmış Araçlara Bakılıyor..");
                    if (!currentUrl.Contains(baseURL + rentedVehiclePage))//rentedVehiclePage değişkeni, KABIS web sitesindeki kiralık araçlar sayfasının URL'sini içerir.
                        WebBrowserNavigate(baseURL + rentedVehiclePage);
                    if (!SearchVehicle(plate))
                    {
                        Console.WriteLine("Araç bu kabis hesabında bulunamadı.");
                        return new KABISResponse() { Code = "305", ResponseResult = ResponseResult.ReturnError("Araç bu kabis hesabında bulunamadı.") };
                    }
                    else
                    {
                        SelectVehicle(returnVehiclePage, plate);//SelectVehicle() fonksiyonu, KABIS hesabındaki bir aracı seçer ve returnVehiclePage parametresinde belirtilen sayfaya yönlendirir.
                        if (webBrowser.DocumentText.Contains(rentCustomer.Identity))//rentCustomer.Identity değişkeni, aracın kiralayan müşterinin kimlik numarasını içerir.
                        {
                            Console.WriteLine("Araç kiraya verilmiş.");
                            return new KABISResponse() { Code = "200", ResponseResult = ResponseResult.ReturnSuccess() };
                        }
                        else
                        {//aşağıdaki 3 satır Kirada olan müşterinin adını, soyadını ve kimlik numarasını web tarayıcısından çeker.
                            var rentedCustomerName = webBrowser.Document.GetElementById("lbl_adi").InnerText;
                            var rentedCustomerSurname = webBrowser.Document.GetElementById("lbl_soyadi").InnerText;
                            var rentedIdentity = webBrowser.Document.GetElementById("lbl_tcno").InnerText;
                            //daha sonra bu bilgileri ve kiralama işlemi yapmaya çalışan müşterinin bilgilerini içeren bir mesaj oluşturur ve konsola yazdırır.
                            var message = $"Araç başka bir müşteride kirada gözüküyor. Şuan Kirada gözüken : ({rentedIdentity}) {rentedCustomerName + " " + rentedCustomerSurname} --- Kiralamaya çalışan : ({rentCustomer.Identity}) {rentCustomer.Name + " " + rentCustomer.Surname}";
                            Console.WriteLine(message);
                            Console.WriteLine("Araç Kiradan alınıyor..");

                            isCompleted = false;

                            webBrowser.Document.GetElementById("txt_donuskm").InnerText = vehicleExitKm.ToString();  //"txt_donuskm" ID'sine sahip HTML öğesinin içeriğini aracın çıkış kilometresiyle değiştirir.
                            webBrowser.Document.GetElementById("btn_kaydet").InvokeMember("Click");//"btn_kaydet" ID'sine sahip HTML öğesine tıklama işlemi yapar.

                            HtmlDocument htmlDocument = webBrowser.Document;
                            htmlDocument.Window.Unload += new HtmlElementEventHandler(Window_Unload);//Web tarayıcısından HtmlDocument nesnesi alır ve "Window_Unload" olayına bir HtmlElementEventHandler olay işleyicisi ekler. Bu olay işleyicisi sayfanın yüklenmesi tamamlandığında tetiklenir ve isCompleted değişkenini true olarak ayarlar.
                            do
                            {
                                //"isCompleted" değişkeni true olana kadar, 10 ms bekleme süresi eklenen ve uygulama olaylarını işleyen bir döngü yürütür. Bu süre içinde, web tarayıcısında bloke edilmiş bir bildirim varsa, "AddBlockAlerts" metodunu kullanarak bu bildirimleri ele alır.
                                Thread.Sleep(10);
                                Application.DoEvents();
                                AddBlockAlerts();
                            } while (!isCompleted);

                            // Kiradan aldıktan sonra tekrar kiralamayı dene.. 
                            try
                            {
                                //Kiradan aracı aldıktan sonra tekrar kiralamayı denemek için yeni bir web tarayıcısı oluşturur, giriş yapar ve "idleVehiclesPage" sayfasına gider. Ancak, bu blok bir hata oluşursa, herhangi bir işlem yapmadan catch bloğuna geçer.
                                createWebBrowser();

                                if (!Login(OfficeUser))
                                    return new KABISResponse() { Code = "300", ResponseResult = ResponseResult.ReturnError("Giriş Yapılamadı!!!") };

                                if (!currentUrl.Contains(baseURL + idleVehiclesPage))
                                    WebBrowserNavigate(baseURL + idleVehiclesPage);
                            }
                            catch (System.Exception ex)
                            {
                            }

                        }
                    }
                }


                if (!SelectVehicle(rentVehiclePage, plate))//SelectVehicle() fonksiyonu kullanılarak  araç seçilmeye çalışılır
                {
                    Console.WriteLine("Araç Seçilemedi. ");
                    return new KABISResponse() { Code = "304", ResponseResult = ResponseResult.ReturnError("Araç listede Seçilemedi.") };
                }

                SelectNational(rentCustomer.Type);//SelectNational() fonksiyonu kullanılarak müşteri türü seçilir (TC kimlik, yabancı kimlik vb.).
                //Daha sonra, müşteri bilgileri web sayfasında doldurulur. Bu işlem, müşteri türüne bağlı olarak farklılık gösterir. Örneğin, TC kimlik numarası olan bir müşteri için "txt_tcno" alanı doldurulurken, yabancı kimlik numarası olan bir müşteri için "txt_pasaportno" alanı doldurulur. Ayrıca, müşteri bilgileri doldurulduktan sonra "btn_turk", "btn_yabanci" veya "btn_diger" düğmeleri tıklanarak işlem tamamlanır.
                if (webBrowser.Document.GetElementById("txt_tcno") != null || webBrowser.Document.GetElementById("txt_pasaportno") != null || webBrowser.Document.GetElementById("txt_yabancitcno") != null)

                {
                    isCompleted = false;
                    Console.WriteLine("Müşteri bilgileri dolduruluyor.");
                    if (rentCustomer.Type == "TC")
                    {
                        webBrowser.Document.GetElementById("txt_tcno").InnerText = rentCustomer.Identity;
                        webBrowser.Document.GetElementById("txt_cikiskm").InnerText = vehicleExitKm.ToString().Replace(".", "").Replace(",", "");

                        webBrowser.Document.GetElementById("btn_turk").InvokeMember("Click");
                    }
                    else if (rentCustomer.Type == "YU")
                    {
                        webBrowser.Document.GetElementById("txt_pasaportno").InnerText = rentCustomer.Identity;
                        webBrowser.Document.GetElementById("list_ulke").SetAttribute("value", rentCustomer.Nationality);
                        webBrowser.Document.GetElementById("txt_yabanciadi").InnerText = rentCustomer.Name;
                        webBrowser.Document.GetElementById("txt_yabancisoyadi").InnerText = rentCustomer.Surname;
                        webBrowser.Document.GetElementById("txt_yabancibaba").InnerText = rentCustomer.FatherName;
                        webBrowser.Document.GetElementById("txt_yabanciana").InnerText = rentCustomer.MotherName;

                        var _placeOfBirth = rentCustomer.PlaceOfBirth;
                        if (_placeOfBirth == null)
                            _placeOfBirth = rentCustomer.Type;

                        webBrowser.Document.GetElementById("txt_yabancidogyeri").InnerText = _placeOfBirth.ToString();
                        webBrowser.Document.GetElementById("txt_yabancidogyili").InnerText = rentCustomer.YearOfBirth.ToString();

                        webBrowser.Document.GetElementById("Text1").InnerText = vehicleExitKm.ToString().Replace(".", "").Replace(",", "");

                        webBrowser.Document.GetElementById("btn_yabanci").InvokeMember("Click");
                    }
                    else if (rentCustomer.Type == "TY")
                    {
                        webBrowser.Document.GetElementById("txt_yabancitcno").InnerText = rentCustomer.Identity;
                        webBrowser.Document.GetElementById("list_ulke2").SetAttribute("value", rentCustomer.Nationality);
                        webBrowser.Document.GetElementById("txt_yabanciehliyetno").InnerText = rentCustomer.LicenseNo;
                        webBrowser.Document.GetElementById("Text2").InnerText = vehicleExitKm.ToString().Replace(".", "").Replace(",", "");

                        webBrowser.Document.GetElementById("btn_diger").InvokeMember("Click");
                    }
                    //Müşteri bilgileri doldurulduktan sonra, HtmlDocument nesnesi kullanılarak sayfada bir olay dinleyicisi oluşturulur. Ardından, isCompleted değişkeni false olarak ayarlanır ve bir döngü içinde AddBlockAlerts() fonksiyonu kullanılarak sayfa yenilenir. Eğer sayfa yüklendikten sonra isCompleted değişkeni hala false ise, işlem devam eder.
                    HtmlDocument htmlDocument = webBrowser.Document;
                    htmlDocument.Window.Unload += new HtmlElementEventHandler(Window_Unload);
                    do
                    {
                        AddBlockAlerts();
                        Application.DoEvents();
                    } while (!isCompleted);

                    return GetWebPageResponse();//Son olarak, GetWebPageResponse() fonksiyonu çağrılır ve bir web sayfası yanıtı döndürülür. Eğer bir hata oluşursa, null döndürülür.
                }
                return null;
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }
        private KABISResponse ReturnVehicle(KABISIntegrationUser OfficeUser, string plate, int entranceKm)
        //Bu kod bloğu bir aracın KABIS (Karayolu Bilgi Sistemi) adlı sistemde iadesini gerçekleştiren bir metodu içermektedir
        {
            try
            {
                createWebBrowser();//createWebBrowser fonksiyonu, WebBrowser öğesi oluşturur ve webBrowser özelliğine atanır. Bu, web sayfası etkileşimi için gerekli bir adımdır.
                if (!Login(OfficeUser))//Login fonksiyonu, OfficeUser kimlik bilgilerini kullanarak KABIS'e giriş yapar.
                    return new KABISResponse() { Code = "300", ResponseResult = ResponseResult.ReturnError("Giriş Yapılamadı!!!") };

                if (!currentUrl.Contains(baseURL + rentedVehiclePage))     //currentUrl özelliği, mevcut URL'yi depolar.
                    WebBrowserNavigate(baseURL + rentedVehiclePage);//WebBrowserNavigate fonksiyonu, baseURL'ye eklenen belirli bir URL'yi yükler.

                if (!SearchVehicle(plate))//SearchVehicle fonksiyonu, verilen plate plaka numarası ile KABIS'te araç arar.
                {
                    if (!currentUrl.Contains(baseURL + idleVehiclesPage))//Eğer araç bulunamazsa, idleVehiclesPage sayfasına giderek boş araçları arar.
                        WebBrowserNavigate(baseURL + idleVehiclesPage);

                    Console.WriteLine("Araç Bulunamadı. Boş Araçlara Bakılıyor..");
                    if (SearchVehicle(plate))
                    {
                        Console.WriteLine("Araç kiradan alınmış.");

                    }
                    else
                    {
                        Console.WriteLine("Araç bu kabis hesabında bulunamadı.");
                    }
                    return new KABISResponse() { Code = "200", ResponseResult = ResponseResult.ReturnSuccess() };
                }

                if (!SelectVehicle(returnVehiclePage, plate))//Eğer araç bulunursa, SelectVehicle fonksiyonu kullanılarak araç seçilir.
                {
                    Console.WriteLine("Araç Seçilemedi. ");
                    return new KABISResponse() { Code = "303", ResponseResult = ResponseResult.ReturnError("Araç Seçilemedi.") };
                }
                //Ardından, controlPage özelliği kullanılarak aracın doğruluğu kontrol edilir.
                var controlPage = webBrowser.Document.GetElementById("lbl_plaka");
                if (controlPage != null)
                {
                    isCompleted = false;
                    webBrowser.Document.GetElementById("txt_donuskm").InnerText = entranceKm.ToString();//entranceKm değişkeni kullanılarak giriş kilometre bilgisi girilir.
                    webBrowser.Document.GetElementById("btn_kaydet").InvokeMember("Click");//btn_kaydet düğmesi tıklanarak araç iadesi gerçekleştirilir.

                    HtmlDocument htmlDocument = webBrowser.Document;
                    htmlDocument.Window.Unload += new HtmlElementEventHandler(Window_Unload);
                    //HtmlDocument öğesi kullanılarak sayfanın tamamlanması beklenir ve GetWebPageResponse fonksiyonu kullanılarak sayfanın yanıtı alınır.
                    do
                    {
                        Thread.Sleep(10);
                        Application.DoEvents();
                        AddBlockAlerts();
                    } while (!isCompleted);


                    return GetWebPageResponse();////Son olarak, GetWebPageResponse() fonksiyonu çağrılır ve bir web sayfası yanıtı döndürülür. Eğer bir hata oluşursa, null döndürülür.
                }

                return null;
            }
            catch (System.Exception ex)
            {
                return new KABISResponse() { Code = "105", ResponseResult = ResponseResult.ReturnError(ex.Message) };
            }
        }
        private KABISResponse RemoveVehicle(KABISIntegrationUser OfficeUser, string plate)
        //Bu kod bloğu,araç iade işlemi gerçekleştirir.

        {
            try
            {
                createWebBrowser();
                //Fonksiyon, öncelikle web tarayıcısı nesnesi oluşturarak başlar. Ardından, belirtilen kullanıcı bilgileriyle giriş yapar. Eğer giriş yapılamazsa, hata kodu ve açıklama içeren bir KABISResponse nesnesi döndürür.

                if (!Login(OfficeUser))
                    return new KABISResponse() { Code = "300", ResponseResult = ResponseResult.ReturnError("Giriş Yapılamadı!!!") };
                //Sonrasında, mevcut URL, temel URL ve addRemoveVehiclePage URL'sini içeren bir string ile karşılaştırılır. Eğer mevcut URL addRemoveVehiclePage URL'si değilse, WebBrowserNavigate fonksiyonu kullanılarak addRemoveVehiclePage'e yönlendirilir.
                if (!currentUrl.Contains(baseURL + addRemoveVehiclePage))
                    WebBrowserNavigate(baseURL + addRemoveVehiclePage);

                isCompleted = false;
                webBrowser.Document.GetElementsByTagName("option")[0].Document.GetElementsByTagName("option")[2].SetAttribute("selected", "selected");
                webBrowser.Document.InvokeScript("__doPostBack");
                do
                {
                    //Web sayfası tamamen yüklenene kadar beklemek için isCompleted adında bir değişken kullanılır ve bir döngü içinde Application.DoEvents() fonksiyonu kullanılarak döngü kesintiye uğramadan çalışır.
                    Thread.Sleep(10);
                    Application.DoEvents();
                } while (!isCompleted);
                //Daha sonra, HtmlAgilityPack.HtmlDocument sınıfının bir örneği oluşturulur ve tarayıcıdaki sayfanın HTML kodu yüklenir. Ardından, bir tablonun id'si tSortable olan sayfadaki tabloyu seçer. Eğer bu tablo yoksa, tSortable1, tSortable2, tSortable3, tSortable4 ve tSortable5 tablolarını sırayla arar. Tablo bulunamazsa, hata kodu ve açıklama içeren bir KABISResponse nesnesi döndürür.
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(webBrowser.DocumentText);
                string _tsortableId = "tSortable";
                for (int i = 1; i <= 5; i++)
                {
                    if (doc.DocumentNode.SelectSingleNode("//table[@id='" + _tsortableId + "']") != null)
                        break;
                    _tsortableId = "tSortable" + i.ToString();
                }
                //aşağıdaki kodlar, doc adlı bir HTML belgesinin belirli bir id değerine sahip bir tablo elementini seçmek için kullanılır. Öncelikle _tsortableId adlı bir değişken kullanılır, bu değişken tablo elementinin aranacak olan id değerini içerir. selectSingleNode değişkeni, doc belgesindeki id özelliği _tsortableId değerine eşit olan ilk <table> elemanını seçer. Eğer bu eleman null dönerse, aynı kod id özelliği "tSortable" olan bir sonraki<table> elemanını seçmek için kullanılır.Bu, eğer _tsortableId değeri geçerli bir id değeri değilse, varsayılan olarak tSortable adlı bir id ye sahip olan bir tablonun seçilmesini sağlar.


                var selectSingleNode = doc.DocumentNode.SelectSingleNode("//table[@id='" + _tsortableId + "']");
                selectSingleNode = selectSingleNode == null ? doc.DocumentNode.SelectSingleNode("//table[@id='tSortable']") : selectSingleNode;


                if (selectSingleNode != null)
                {

                    List<List<string>> table = selectSingleNode
                                .Descendants("tr")
                                .Skip(1)
                                .Where(tr => tr.Elements("td").Count() > 1)
                                .Select(tr => tr.Elements("td").Select(td => td.InnerHtml.Trim()).ToList())
                                .ToList();//Bu kod, seçili bir HTML elementindeki tablo verilerini çekmek için kullanılır.selectSingleNode.Descendants("tr") satır satır tüm tablo satırlarını içeren bir koleksiyon döndürür. .Skip(1) satırların ilkini (başlık satırı) atlayarak devam edilmesini sağlar. .Where(tr => tr.Elements("td").Count() > 1) satırda en az 2 td elementi olmasını sağlar, çünkü tek bir td elementi içeren satırlar gereksizdir. .Select(tr => tr.Elements("td").Select(td => td.InnerHtml.Trim()).ToList()) her satır için td elementlerinin içeriklerini alır ve bir List<string> olarak döndürür. Son olarak, .ToList() metodu tüm satırları bir List<List<string>> koleksiyonuna dönüştürür. Böylece, bu kod, belirli bir HTML elementindeki tablo verilerini List<List< string >> formatında depolar.

                    //aşağıdaki kısımda, plaka numarası verilen aracın mevcut araç listesi sayfasındaki checkbox'ının ID'sini almak için birkaç işlem gerçekleştiriliyor.  İlk olarak, table değişkeni selectSingleNode nesnesinden alınan HTML tablosunu içeren bir liste oluşturuyor. table değişkeni, tablonun tüm satırlarını ve sütunlarını içerir.   Daha sonra, table üzerinde bir Where sorgusu çalıştırılarak plate parametresi ile eşleşen aracın satırı seçiliyor. Ardından, bu satırın ilk sütunundaki ID değeri(checkId) alınarak, checkbox'ın ID'si elde ediliyor. Checkbox'ın ID'si, "id=" ile başlayan ve "_checkbox" ile biten bir dize içinde yer alıyor.Bu nedenle, checkId stringi öncelikle "id=" dizesinden sonra başlayan kısmı alınarak düzenleniyor(checkId.Substring(checkId.IndexOf("id=\"") +4, checkId.Length - checkId.IndexOf("id=\"") - 6)).Sonrasında, id değişkeni, checkbox ID'sinden "Bos_arac_liste_ctrl" ve "_checkbox" kelimelerini çıkararak elde ediliyor. Bu işlem sonucu, aracın sayfadaki hangi sayfada olduğunu hesaplamak için kullanılacak olan sayfa numarası (pageNumber) elde edilir. Son olarak, _paginate değişkeni, sayfalandırma bölümünün ID'sini içeren bir HTML elementi olarak alınır ve pageNumber değişkeni kadar sayfa geçişlerini gerçekleştirmek için döngü oluşturulur.



                    var checkId = table.Where(t => t[1].Contains(plate)).Select(t1 => t1[0]).FirstOrDefault();
                    if (checkId != null)
                        checkId = checkId.Substring(checkId.IndexOf("id=\"") + 4, checkId.Length - checkId.IndexOf("id=\"") - 6);
                    else
                        return new KABISResponse() { Code = "302", ResponseResult = ResponseResult.ReturnError("Plaka bulunamadı.") };

                    var id = checkId.Replace("Bos_arac_liste_ctrl", "").Replace("_checkbox", "");
                    var pageNumber = Math.Floor(int.Parse(id) / 5.0);
                    var _paginate = webBrowser.Document.GetElementById(_tsortableId + "_paginate");



                    //
                    for (int i = 0; i < pageNumber; i++)
                    {
                        _paginate.Children[3].InvokeMember("Click");
                    }

                    var check = webBrowser.Document.GetElementById(checkId);

                    if (check != null)
                    {
                        check.SetAttribute("checked", "1");
                        webBrowser.Document.InvokeScript("__doPostBack");

                        isCompleted = false;
                        webBrowser.Document.GetElementById("Button1").InvokeMember("click");
                        HtmlDocument htmlDocument = webBrowser.Document;
                        htmlDocument.Window.Unload += new HtmlElementEventHandler(Window_Unload);
                        do
                        {
                            Thread.Sleep(10);
                            Application.DoEvents();
                            AddBlockAlerts();
                        } while (!isCompleted);
                        return GetWebPageResponse();
                    }
                    return new KABISResponse() { Code = "302", ResponseResult = ResponseResult.ReturnError("Plaka bulunamadı.") };
                }

                return new KABISResponse() { Code = "105", ResponseResult = ResponseResult.ReturnError("Araçlar tablosu yüklenemedi.") };

            }
            catch (System.Exception ex)
            {

                return new KABISResponse() { Code = "104", ResponseResult = ResponseResult.ReturnError(ex.Message) };
            }
        }
        private KABISResponse AddVehicle(KABISIntegrationUser OfficeUser, string plate, string registrationNumber)
        {
            try
            {
                createWebBrowser();

                plate = plate.Trim();

                if (!Login(OfficeUser))
                    return new KABISResponse() { Code = "300", ResponseResult = ResponseResult.ReturnError("Giriş Yapılamadı!!!") };
                //currentUrl değişkeni baseURL ve addRemoveVehiclePage değişkenleri kullanılarak kontrol edilir ve kullanıcının araç ekleme sayfasında olup olmadığı belirlenir. Eğer kullanıcı bu sayfada değilse, WebBrowserNavigate() metodu kullanılarak sayfaya yönlendirilir.
                if (!currentUrl.Contains(baseURL + addRemoveVehiclePage))
                    WebBrowserNavigate(baseURL + addRemoveVehiclePage);
                string _cityCode = "", _letterCode = "", _digitCode = "";//Bu değişkenler, araç ekleme formundaki il, harf ve rakam alanlarının değerlerini doldurmak için kullanılacak.

                foreach (var item in plate)
                //Eğer karakter sayı ise, karakter _cityCode değişkenine eklenir. Eğer karakter bir harf ise, karakter _letterCode değişkenine eklenir. _cityCode değişkeni plakanın rakam kısmını içerirken, _letterCode değişkeni plakanın harf kısmını içerir.
                {
                    if (isNumaric(item.ToString()))
                        _cityCode = _cityCode + item.ToString();
                    else
                        _letterCode += item.ToString();
                }

                _digitCode = _cityCode.Substring(2, _cityCode.Length - 2);//_digitCode değişkeni, _cityCode değişkeninin ilk iki karakterini atlayarak kalan kısım olarak ayarlanır. 
                _cityCode = _cityCode.Substring(0, 2); //_cityCode değişkeni, sadece ilk iki karakteri koruyacak şekilde güncellenir.

                isCompleted = false;
                //webBrowser.Document kullanılarak WebBrowser nesnesinin yüklenmiş web sayfasına erişilir. Bu sayfada, bir HTML formu kullanılarak plaka bilgisinin girileceği bir alan açılır. Bu alana erişmek için, __doPostBack fonksiyonu tetiklenir. Bu fonksiyon, webBrowser.Document.InvokeScript("__doPostBack"); kod satırıyla tetiklenir.
                webBrowser.Document.GetElementsByTagName("option")[0].Document.GetElementsByTagName("option")[1].SetAttribute("selected", "selected");
                webBrowser.Document.InvokeScript("__doPostBack");
                do
                //Bu kod bloğu, işlem sırasında tarayıcının tamamlanmasını bekler.
                {
                    Thread.Sleep(10);
                    Application.DoEvents();
                    AddBlockAlerts();
                    //yukarıdaki metodlar, işlem sırasında diğer uygulama işlemlerini gerçekleştirir ve herhangi bir blokajı önler.
                } while (!isCompleted);// Tarayıcının yüklenmesi tamamlandığında, işlem döngüden çıkar ve devam eder. 

                webBrowser.Document.GetElementsByTagName("select")[1].SetAttribute("SelectedIndex", _cityCode); ; // plaka numarasının il kodunu ayrıştırır
                webBrowser.Document.GetElementById("txt_harf").InnerText = _letterCode;// plaka numarasının harf kodunu ayrıştırır
                webBrowser.Document.GetElementById("txt_rakam").InnerText = _digitCode;//HTML sayfasında ID'si "txt_rakam" olan bir metin kutusuna _digitCode değişkenindeki değeri atar. Bu sayede, _digitCode değişkenindeki veri, web sayfasındaki ilgili metin kutusuna otomatik olarak girilmiş olur
                webBrowser.Document.GetElementById("txt_tescno").InnerText = registrationNumber;//HTML sayfasında ID'si "txt_tescno" olan bir metin kutusuna registrationNumber değişkenindeki değeri atar. Bu sayede, registrationNumber değişkenindeki veri, web sayfasındaki ilgili metin kutusuna otomatik olarak girilmiş olur

                isCompleted = false;
                webBrowser.Document.GetElementById("btn_kaydet").InvokeMember("click");//Bu satır, web sayfasındaki "btn_kaydet" isimli düğmeye tıklamak için kullanılır. "InvokeMember" metodu, belirtilen düğmeyi etkinleştirir ve bu düğmeye tıklanması ile ilgili eylemleri tetikler. Bu sayede, kullanıcının elle düğmeye tıklaması gerekmeksizin otomatik olarak kaydetme işlemini gerçekleştirmek mümkün olur.
                HtmlDocument htmlDocument = webBrowser.Document;// Window_Unload olayı, HTML belgesi yüklendiğinde gerçekleşen olaydır. Bu nedenle, Unload olayı gerçekleştiğinde Window_Unload olayı tetiklenir ve belgede yapılan değişiklikler işlenebilir. htmlDocument.Window.Unload özelliği, belgenin Window özelliğine erişimi sağlar ve Window_Unload olayı, HTML belgesi yüklenirken işlenmek üzere bir olay dinleyiciye bağlanır.
                htmlDocument.Window.Unload += new HtmlElementEventHandler(Window_Unload);// Bu olay, yüklenen belge kapatıldığında tetiklenir ve bu durumda, işlev kaydedilen belge nesnesini siler ve önbellekteki tüm verileri temizler.
                // Bu işlemin amacı, web tarayıcısının tamamen yüklendiğinden emin olmak ve gerektiğinde sonuçları işlemek için bir fırsat sağlamaktır.
                do
                {
                    //Bu kod bloğu, işlem sırasında tarayıcının tamamlanmasını bekler.
                    Thread.Sleep(10);
                    Application.DoEvents();
                    AddBlockAlerts();
                    //yukarıdaki metodlar, işlem sırasında diğer uygulama işlemlerini gerçekleştirir ve herhangi bir blokajı önler.
                } while (!isCompleted);// Tarayıcının yüklenmesi tamamlandığında, işlem döngüden çıkar ve devam eder. 
                return GetWebPageResponse();// İşlem tamamlandığında, tarayıcı sayfasının yanıtını döndürmek için GetWebPageResponse () metodu kullanılır
                //İşlemler tamamlandıktan sonra GetWebPageResponse() metodu ile webden gelen mesaj çekilir.
            }
            catch (System.Exception ex)
            {
                return new KABISResponse() { Code = "103", ResponseResult = ResponseResult.ReturnError(ex.Message) };
            }
        }






        private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)//Bu kod bloğu, WebBrowser kontrolünün bir belge yüklemesini tamamladığında çalışır. 
        {
            //Bu kod bloğu, yüklenen belgenin URL'sini currentUrl değişkenine atar. 
            currentUrl = ((WebBrowser)sender).Url.ToString();//yüklenen belgenin URL'sini bir dize olarak alır ve currentUrl değişkenine atar.
            isCompleted = true;//isCompleted değişkeni, yüklemenin tamamlandığını belirtmek için kullanılır
        }
        private void WebBrowserNavigate(string url)//Bu yöntem, belirtilen URL'ye gezinmek ve sayfanın yüklenme işlemini beklemek için kullanılır. Bu nedenle, sayfanın tamamen yüklenmesi gerekli olduğunda kullanışlıdır.
        {
            isCompleted = false; //sayfa henüz yüklenmemiş kabul edilir
            webBrowser.Navigate(url); //WebBrowser kontrolünü belirtilen url ile yüklemek için kullanılır.
            do
            //isCompleted değişkeni true olana kadar, yani sayfa tamamen yüklenene kadar, Application.DoEvents() metodu çağrılır.  Bu döngü, sayfanın yüklenme işleminin tamamlanmasını bekler.
            {
                Application.DoEvents();//Application.DoEvents() metodunun kullanılması, formun donmasını önlemek için gereklidir.
            } while (!isCompleted);
        }
        private string ResolveCaptcha()
        //Bu kodlar bir CAPTCHA çözme işlemi gerçekleştiriyor. CAPTCHA, bilgisayarların yapay zeka ve otomasyon yazılımlarının bir web sitesinde spam göndermesini veya kötü amaçlı bir davranışta bulunmasını önlemek için kullanılan bir güvenlik mekanizmasıdır.Kodlar, webBrowser adlı bir System.Windows.Forms.WebBrowser nesnesi kullanarak CAPTCHA görüntüsünü buluyor ve bu görüntüyü Tesseract OCR kütüphanesi kullanarak okuyor.Bu işlem, CAPTCHA'nın içindeki metni çıkararak, CAPTCHA testini geçmek için gereken doğru cevabı verme imkanı sağlıyor.


        {
            IHTMLDocument2 doc = (IHTMLDocument2)webBrowser.Document.DomDocument;//İlk olarak, webBrowser.Document.DomDocument özelliği kullanılarak, web sayfasının DOM yapısını temsil eden bir IHTMLDocument2 nesnesi elde ediliyo
            IHTMLControlRange imgler = (IHTMLControlRange)((HTMLBody)doc.body).createControlRange();
            //Bu kod parçası, belirli bir HTML dokümanı içinde bulunan bir IMG etiketini seçmek için kullanılır.  Öncelikle, doc değişkeni HTML dokümanını temsil eden bir nesneyi içerir.Ardından, doc.body özelliği HTML dokümanının gövdesini temsil eden bir nesneyi döndürür. Daha sonra, HTMLBody sınıfının bir örneği olan doc.body nesnesi createControlRange() yöntemi çağrılır. Bu yöntem, sayfada bir kontrol seçim aralığı oluşturur.    Bu oluşturulan kontrol aralığı, IHTMLControlRange arabirimini uygulayan bir nesne olarak atandığı imgler değişkenine atanır.Bu arayüz, sayfada yer alan herhangi bir kontrolün seçilmesini sağlar.   Sonuç olarak, imgler değişkeni artık sayfadaki bir IMG etiketini seçmek için kullanılabilir.


            //Ardından, doc.images özelliği kullanılarak sayfadaki tüm resimler taranıyor ve her bir resim için aşağıdaki işlemler yapılıyor:
            foreach (IHTMLImgElement img in doc.images)
            {
                if (((IHTMLElement)img).id == "imgKod")
                {
                    //Resmin id özelliği "imgKod" ise,Resim URL'sinden data:image/png;base64, bölümü çıkarılıyor, 

                    byte[] bytes = Convert.FromBase64String(img.src.Replace("data:image/png;base64,", ""));//Convert.FromBase64String yöntemi kullanılarak resim verileri byte dizisine dönüştürülüyor, 
                    using (var engine = new TesseractEngine(StaticHelper.GetConfiguration("tessdataFolderPath"), "eng", EngineMode.Default))
                        //Tesseract OCR kütüphanesi kullanılarak resimden metin çıkarılıyor ve bu metin döndürülüyor.
                    {
                        using (var imgByte = Pix.LoadFromMemory(bytes))// Pix sınıfı, görüntü işleme kütüphanesi olan Leptonica tarafından sağlanan işlevlerin .NET bağlamında kullanılabilmesini sağlar. LoadFromMemory() yöntemi, verilen byte dizisinden bir Pix nesnesi oluşturur.
                        {
                            using (var page = engine.Process(imgByte))//Process() yöntemi, OCR işlemini gerçekleştirir ve metin içeriğini page nesnesine yazar. 
                            {
                                var txt = page.GetText();//GetText() yöntemi, elde edilen metni bir dize olarak döndürür
                                return txt;//CAPTCHA'nın içindeki metin döndürülüyor
                            }
                        }
                    }
                }
            }
            //veya Aşağıdaki gibi boş bir dize döndürülüyor.
            return "";
        }
        private bool SearchVehicle(string plate)//Bu fonksiyon, verilen plakanın araç listesi sayfasında var olup olmadığını kontrol eder.
        {
            Console.WriteLine("Araç listede aranıyor...");
            if (webBrowser.DocumentText.ToString().Contains(plate))
            {
                return true;
            }
            return false;
        }
        private bool SelectVehicle(string pageToRedirect, string plate)//Bu kod parçası, bir WebBrowser nesnesi kullanarak bir HTML sayfasındaki belirli bir plakaya sahip aracı seçmeye çalışır.
        {
            HtmlElementCollection a = webBrowser.Document.GetElementsByTagName("a");//İlk olarak, webBrowser değişkeni bir WebBrowser nesnesini temsil eder ve bu nesnenin Document özelliği kullanılarak HTML belgesine erişilir.
           // GetElementsByTagName yöntemi, HTML belgesindeki tüm<a> etiketlerini içeren bir koleksiyon döndürür ve bu koleksiyon a değişkenine atanır.
            var str = System.String.Format("{0}?plaka={1}", pageToRedirect, plate);//Daha sonra, str değişkeni, pageToRedirect ve plate parametreleri kullanılarak oluşturulur ve arama işleminde kullanılacak URL'yi temsil eder.
            foreach (HtmlElement a1 in a)
            //foreach döngüsü, a koleksiyonundaki her bir HtmlElement öğesini gezinir ve öğenin href özelliği, aranan URL'yi içerip içermediğini kontrol etmek için kullanılır.
            {
                if (a1.GetAttribute("href").Contains(System.String.Format("{0}?plaka={1}", pageToRedirect, plate)))
                // Eğer aranan URL bulunursa, WebBrowserNavigate yöntemi kullanılarak öğenin href özelliğinde belirtilen URL'ye yönlendirilir ve true değeri döndürülür.
                {
                    WebBrowserNavigate(a1.GetAttribute("href"));
                    Console.WriteLine("Araç bulundu yönlendirme işlem için " + pageToRedirect + "sayfasına yönlendiriliyor.");
                    return true;
                }
            }
            //Eğer aranan URL koleksiyon içinde bulunamazsa, false değeri döndürülür ve ekrana "Araç Seçilemedi. Listede bu plaka bulunamadı !!!" mesajı yazdırılır.
            Console.WriteLine("Araç Seçilemedi. Listede bu plaka bulunamadı !!!");
            return false;

        }
        private void SelectNational(string type)//Bu kod parçası, bir WebBrowser nesnesini kullanarak bir HTML sayfasındaki belirli bir müşteri tipini (TC, YU veya TY) seçmeye çalışır.
        {
            //İlk olarak, type parametresi, seçilecek müşteri tipini temsil eder ve typeValue değişkeni, bu parametreye bağlı olarak belirli bir değerle atama işlemi yapar.
            try
            {
                var typeValue = type == "TC" ? "1" :
                               type == "YU" ? "2" :
                               type == "TY" ? "3" : null;

                if (typeValue != null)//Daha sonra, if bloğu, typeValue değişkeninin null olup olmadığını kontrol eder. Eğer null değilse, HTML belgesindeki tüm input etiketlerinin bir koleksiyonunu alır ve list_vatandas adına sahip olanları gezinir. 
                    foreach (HtmlElement el in webBrowser.Document.GetElementsByTagName("input").GetElementsByName("list_vatandas"))
                    {
                        if (el.GetAttribute("value") == typeValue)//Bu öğelerin her biri için, value özelliği, typeValue değişkeni ile karşılaştırılır. Eğer eşitse, click yöntemi çağrılarak seçim işlemi gerçekleştirilir.
                        {
                            isCompleted = false;
                            el.InvokeMember("click");
                            do//Seçim işlemi gerçekleştirildikten sonra, do-while döngüsü, isCompleted değişkeni true olana kadar döner. Bu, seçim işlemi tamamlanmadan önce, uygulamanın beklemesi gerektiğini gösterir.
                            {
                                Thread.Sleep(10);
                                Application.DoEvents();
                            } while (!isCompleted);
                            break;
                        }
                    }
                else//Son olarak, else bloğu, typeValue değişkeninin null olduğu durumlarda çalışır ve "Müşteri tipi yanlış." mesajını ekrana yazdırır.
                    Console.WriteLine("Müşteri tipi yanlış.");
            }
            catch (System.Exception)
            {
                throw;
            }
        }
        private KABISResponse GetWebPageResponse()//Bu kod parçası, bir WebBrowser nesnesindeki belirli bir HTML sayfasının yanıtını almak için kullanılır.
        {
            var kABISResponse = new KABISResponse();

            // Eğer Error dialog message yok ise başarılı kabul edelim
            kABISResponse.Code = "200";
            kABISResponse.ResponseResult = ResponseResult.ReturnSuccess(); //ResponseResult özelliği ReturnSuccess() yöntemi tarafından oluşturulan bir başarılı yanıt nesnesiyle ayarlanır.

            var sonuc = webBrowser.DocumentText.ToString();//Ardından, webBrowser nesnesindeki HTML belgesinin metin içeriği sonuc adlı bir değişkene atanır. 
            if (sonuc != "")//Bu değişken boş değilse, 
            {
                var messageDialog = webBrowser.Document.GetElementById("dialog-message");// messageDialog adlı bir HTML öğesi, dialog-message kimliği ile alınır
                if (messageDialog != null) //mesaj null değilse ve,
                {
                    if (messageDialog.InnerText.Contains("Ehliyet Bilgisine Ulaşılamadı"))//mesaj "Ehliyet Bilgisine Ulaşılamadı" dizesi içeriyorsa
                    {
                        kABISResponse.Code = "300";
                        kABISResponse.ResponseResult = ResponseResult.ReturnError(messageDialog.InnerText);
                        // yanıt nesnesinin kodu ve ResponseResult özelliği, ilgili hata mesajı içeriğine göre ayarlanır.
                    }
                    else if (messageDialog.InnerText.Contains("Araç Sistemde Kayıtlı")) //Araç Sistemde Kayıtlı mesajı içeriyorsa
                    {
                        kABISResponse.Code = "301";
                        kABISResponse.ResponseResult = ResponseResult.ReturnError(messageDialog.InnerText);
                        // yanıt nesnesinin kodu ve ResponseResult özelliği, ilgili hata mesajı içeriğine göre ayarlanır.
                    }
                    else
                    {
                        kABISResponse.Code = "400";
                        kABISResponse.ResponseResult = ResponseResult.ReturnError(messageDialog.InnerText);
                    }

                }

            }

            Console.WriteLine(kABISResponse.ResponseResult.Result.ToString() + " " + kABISResponse.ResponseResult.ExceptionDetail);
            return kABISResponse; //Son olarak, yanıt nesnesi, konsol çıktısında ResponseResult özelliğinin durumunu ve ExceptionDetail özelliğini yazdırarak döndürülür.
        }
        public void AddBlockAlerts()
        //Bu kod parçası, bir WebBrowser nesnesindeki belirli bir HTML sayfasına ek blok uyarıları eklemek için kullanılır.
        {
            if (webBrowser.Document != null)//İlk olarak, if bloğu ile webBrowser.Document özelliği null olmadığı kontrol edilir.
            {
                var head = webBrowser.Document.GetElementsByTagName("head");//Daha sonra, head adlı bir HtmlElementCollection öğesi, webBrowser.Document.GetElementsByTagName yöntemi kullanılarak belgenin başlığına karşılık gelen HTML öğeleri alınır.
                HtmlElement headElement = head[0];//Sonra, headElement adlı bir HtmlElement değişkeni tanımlanır ve head[0] ile head koleksiyonunun ilk öğesi atandıktan sonra bu öğeye atanır.
                HtmlElement scriptEl = webBrowser.Document.CreateElement("script");//Ardından, scriptEl adında bir HtmlElement nesnesi oluşturulur ve webBrowser.Document.CreateElement("script") yöntemi kullanılarak bir script HTML öğesi oluşturulur.
                IHTMLScriptElement element = (IHTMLScriptElement)scriptEl.DomElement;//Daha sonra, element adında bir IHTMLScriptElement öğesi oluşturulur ve bu öğe, DomElement özelliği aracılığıyla scriptEl öğesine atanır.
                element.text = "window.alert = function() { try { } catch ( e ) {} };";//Son olarak, element.text özelliğine, JavaScript kodu atanır. Bu JavaScript kodu, tüm window.alert fonksiyonlarını bloke etmek için kullanılır. Yani, bu kod bloğu sayesinde, sayfadaki alert fonksiyonlarının engellenmesi sağlanır.
                headElement.AppendChild(scriptEl);
            }

        }
        private bool isNumaric(string value)// bu metod verilen string değerin tamamen rakam olup olmadığını kontrol eder ve sonucu boolean bir değer olarak döndürür.
        {
            return Regex.IsMatch(value, @"^\d+$") ? true : false;//Regex.IsMatch(value, @"^\d+$"): Bu ifade, verilen değerin tamamen rakam içerip içermediğini kontrol etmek için kullanılan bir regular expression kullanarak string ifadeyi kontrol eder. ^\d+$ ifadesi, ifadenin başından sonuna kadar sadece sayısal karakterleri içermesini sağlar.
            // ? true : false: Ternary operatörünü kullanarak, Regex.IsMatch sonucu true ise true, false ise false döndürür
        }
        void Window_Unload(object sender, HtmlElementEventArgs e)
        //Bu kod,mevcut sayfada yeni bir boş pencere açıyor.
        //Bu kod bloğu, WebBrowser nesnesi içinde yüklü bir web sayfası kapatıldığında (yani, Window.Unload olayı tetiklendiğinde) boş bir pencere açar. Bu yöntem, sayfaları spam veya istenmeyen pencereler açan kötü niyetli JavaScript kodlarından korumak için kullanılabilir.

        //object sender parametresi olayı tetikleyen nesneyi temsil eder. HtmlElementEventArgs e parametresi ise olay hakkında ek bilgi sağlar. Bu örnekte, olaya özel bir bilgi gerekli değil, bu nedenle bu parametre kullanılmamıştır.

        {
            HtmlElement head = webBrowser.Document.GetElementsByTagName("head")[0];//Bu satır, webBrowser adlı bir WebBrowser nesnesinin Document özelliği üzerinden head etiketini alır.
            HtmlElement scriptEl = webBrowser.Document.CreateElement("script");//: Bu satır, webBrowser nesnesinin Document özelliği üzerinden yeni bir script etiketi oluşturur.
            IHTMLScriptElement element = (IHTMLScriptElement)scriptEl.DomElement;//Bu satır, script etiketinin DomElement özelliğini IHTMLScriptElement nesnesine dönüştürür.
            element.text = "window.open('', '_self', ''); ";//Bu satır, text özelliği kullanılarak script etiketi içine bir JavaScript kodu ekler. Bu kod, window.open() metodunu kullanarak boş bir pencere açar.
            //window.open metodunun '_self' parametresi, mevcut pencerenin hedef pencere olarak belirlenmesini sağlar. '' parametresi, hedef pencerenin boş bir sayfa olmasını belirtir. Bu nedenle, sayfada açılan yeni pencerelerin yerine, boş bir sayfa açılır.
            head.AppendChild(scriptEl);//Bu satır, head etiketi içine yeni oluşturulan script etiketini ekler.
        }
        private void WebBrowser_NewWindow(object sender, CancelEventArgs e)
        {
            //Bu kod bloğu, WebBrowser kontrolünde yeni bir pencere açıldığında çalışır. NewWindow olayı, WebBrowser kontrolünde bir bağlantıyı tıklattığınızda veya window.open() metodu kullanarak bir popup penceresi açtığınızda tetiklenir.
            //Bu kod bloğu, e.Cancel özelliğini true olarak ayarlayarak, yeni pencerenin açılmasını engeller. Daha sonra, webBrowser kontrolü, webBrowser.StatusText değerine göre bir sayfa yükler. webBrowser.StatusText, yeni pencerenin URL'sini içeren bir dizedir. Bu, yeni pencerenin içeriğini mevcut pencereye yüklemek yerine, aynı URL'ye sahip bir sayfanın mevcut pencerede açılmasını sağlar.
            //Bu yöntem, kullanıcıların bir bağlantıyı tıkladığında veya popup penceresi açtığında yeni bir pencerede açılmasını bekledikleri sayfaların, mevcut pencerede açılmasını sağlar. Bu nedenle, kullanıcıların istenmeyen pencerelerle karşılaşma olasılığı azalır. Ancak, bu yöntem pop-up reklamları ve diğer pop-up pencerelerini tamamen engellemez, sadece açılma şeklini değiştirir.
            e.Cancel = true;//yeni pencerenin açılmasını engeller
            webBrowser.Navigate(webBrowser.StatusText);
        }

    }
}
