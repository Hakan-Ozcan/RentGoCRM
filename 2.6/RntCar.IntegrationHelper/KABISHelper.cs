using mshtml;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
        void createWebBrowser()
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
        {
            KABISResponse response = null;
            Thread thread = new Thread(() => { response = ReturnVehicle(OfficeUser, plate, entranceKm); });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();//Bu satır, ana iş parçacığını yeni thread'in tamamlanmasını beklemeye zorlar.

            return response;
        }
        public KABISResponse RemoveVehicleThreadProcess(KABISIntegrationUser OfficeUser, string plate)
        {
            KABISResponse response = null;
            Thread thread = new Thread(() => { response = RemoveVehicle(OfficeUser, plate); });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            return response;
        }
        public KABISResponse AddVehicleThreadProcess(KABISIntegrationUser OfficeUser, string plate, string registrationNumber)
        {
            KABISResponse response = null;
            Thread thread = new Thread(() => { response = AddVehicle(OfficeUser, plate, registrationNumber); });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            return response;
        }




        private bool Login(KABISIntegrationUser kABISIntegrationUser)
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

                if (!currentUrl.Contains(baseURL + addRemoveVehiclePage))
                    WebBrowserNavigate(baseURL + addRemoveVehiclePage);
                string _cityCode = "", _letterCode = "", _digitCode = "";

                foreach (var item in plate)
                {
                    if (isNumaric(item.ToString()))
                        _cityCode = _cityCode + item.ToString();
                    else
                        _letterCode += item.ToString();
                }

                _digitCode = _cityCode.Substring(2, _cityCode.Length - 2);
                _cityCode = _cityCode.Substring(0, 2);

                isCompleted = false;
                webBrowser.Document.GetElementsByTagName("option")[0].Document.GetElementsByTagName("option")[1].SetAttribute("selected", "selected");
                webBrowser.Document.InvokeScript("__doPostBack");
                do
                {
                    Thread.Sleep(10);
                    Application.DoEvents();
                    AddBlockAlerts();
                } while (!isCompleted);

                webBrowser.Document.GetElementsByTagName("select")[1].SetAttribute("SelectedIndex", _cityCode); ;
                webBrowser.Document.GetElementById("txt_harf").InnerText = _letterCode;
                webBrowser.Document.GetElementById("txt_rakam").InnerText = _digitCode;
                webBrowser.Document.GetElementById("txt_tescno").InnerText = registrationNumber;

                isCompleted = false;
                webBrowser.Document.GetElementById("btn_kaydet").InvokeMember("click");
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
            catch (System.Exception ex)
            {
                return new KABISResponse() { Code = "103", ResponseResult = ResponseResult.ReturnError(ex.Message) };
            }
        }






        private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            currentUrl = ((WebBrowser)sender).Url.ToString();
            isCompleted = true;
        }
        private void WebBrowserNavigate(string url)
        {
            isCompleted = false;
            webBrowser.Navigate(url);
            do
            {
                Application.DoEvents();
            } while (!isCompleted);
        }
        private string ResolveCaptcha()
        {
            IHTMLDocument2 doc = (IHTMLDocument2)webBrowser.Document.DomDocument;
            IHTMLControlRange imgler = (IHTMLControlRange)((HTMLBody)doc.body).createControlRange();
            foreach (IHTMLImgElement img in doc.images)
            {
                if (((IHTMLElement)img).id == "imgKod")
                {
                    byte[] bytes = Convert.FromBase64String(img.src.Replace("data:image/png;base64,", ""));
                    using (var engine = new TesseractEngine(StaticHelper.GetConfiguration("tessdataFolderPath"), "eng", EngineMode.Default))
                    {
                        using (var imgByte = Pix.LoadFromMemory(bytes))
                        {
                            using (var page = engine.Process(imgByte))
                            {
                                var txt = page.GetText();
                                return txt;
                            }
                        }
                    }
                }
            }
            return "";
        }
        private bool SearchVehicle(string plate)
        {
            Console.WriteLine("Araç listede aranıyor...");
            if (webBrowser.DocumentText.ToString().Contains(plate))
            {
                return true;
            }
            return false;
        }
        private bool SelectVehicle(string pageToRedirect, string plate)
        {
            HtmlElementCollection a = webBrowser.Document.GetElementsByTagName("a");
            var str = System.String.Format("{0}?plaka={1}", pageToRedirect, plate);
            foreach (HtmlElement a1 in a)
            {
                if (a1.GetAttribute("href").Contains(System.String.Format("{0}?plaka={1}", pageToRedirect, plate)))
                {
                    WebBrowserNavigate(a1.GetAttribute("href"));
                    Console.WriteLine("Araç bulundu yönlendirme işlem için " + pageToRedirect + "sayfasına yönlendiriliyor.");
                    return true;
                }
            }
            Console.WriteLine("Araç Seçilemedi. Listede bu plaka bulunamadı !!!");
            return false;

        }
        private void SelectNational(string type)
        {
            try
            {
                var typeValue = type == "TC" ? "1" :
                               type == "YU" ? "2" :
                               type == "TY" ? "3" : null;

                if (typeValue != null)
                    foreach (HtmlElement el in webBrowser.Document.GetElementsByTagName("input").GetElementsByName("list_vatandas"))
                    {
                        if (el.GetAttribute("value") == typeValue)
                        {
                            isCompleted = false;
                            el.InvokeMember("click");
                            do
                            {
                                Thread.Sleep(10);
                                Application.DoEvents();
                            } while (!isCompleted);
                            break;
                        }
                    }
                else
                    Console.WriteLine("Müşteri tipi yanlış.");
            }
            catch (System.Exception)
            {
                throw;
            }
        }
        private KABISResponse GetWebPageResponse()
        {
            var kABISResponse = new KABISResponse();

            // Eğer Error dialog message yok ise başarılı kabul edelim
            kABISResponse.Code = "200";
            kABISResponse.ResponseResult = ResponseResult.ReturnSuccess();

            var sonuc = webBrowser.DocumentText.ToString();
            if (sonuc != "")
            {
                var messageDialog = webBrowser.Document.GetElementById("dialog-message");
                if (messageDialog != null)
                {
                    if (messageDialog.InnerText.Contains("Ehliyet Bilgisine Ulaşılamadı"))
                    {
                        kABISResponse.Code = "300";
                        kABISResponse.ResponseResult = ResponseResult.ReturnError(messageDialog.InnerText);
                    }
                    else if (messageDialog.InnerText.Contains("Araç Sistemde Kayıtlı"))
                    {
                        kABISResponse.Code = "301";
                        kABISResponse.ResponseResult = ResponseResult.ReturnError(messageDialog.InnerText);
                    }
                    else
                    {
                        kABISResponse.Code = "400";
                        kABISResponse.ResponseResult = ResponseResult.ReturnError(messageDialog.InnerText);
                    }

                }

            }

            Console.WriteLine(kABISResponse.ResponseResult.Result.ToString() + " " + kABISResponse.ResponseResult.ExceptionDetail);
            return kABISResponse;
        }
        public void AddBlockAlerts()
        {
            if (webBrowser.Document != null)
            {
                var head = webBrowser.Document.GetElementsByTagName("head");
                HtmlElement headElement = head[0];
                HtmlElement scriptEl = webBrowser.Document.CreateElement("script");
                IHTMLScriptElement element = (IHTMLScriptElement)scriptEl.DomElement;
                element.text = "window.alert = function() { try { } catch ( e ) {} };";
                headElement.AppendChild(scriptEl);
            }

        }
        private bool isNumaric(string value)
        {
            return Regex.IsMatch(value, @"^\d+$") ? true : false;
        }
        void Window_Unload(object sender, HtmlElementEventArgs e)
        {
            HtmlElement head = webBrowser.Document.GetElementsByTagName("head")[0];
            HtmlElement scriptEl = webBrowser.Document.CreateElement("script");
            IHTMLScriptElement element = (IHTMLScriptElement)scriptEl.DomElement;
            element.text = "window.open('', '_self', ''); ";
            head.AppendChild(scriptEl);
        }
        private void WebBrowser_NewWindow(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            webBrowser.Navigate(webBrowser.StatusText);
        }

    }
}
