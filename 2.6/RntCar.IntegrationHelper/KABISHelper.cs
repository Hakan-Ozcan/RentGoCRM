using mshtml;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
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
            thread = new Thread(new ThreadStart(createWebBrowser));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
        void createWebBrowser()
        {
            webBrowser = new WebBrowser();
            webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;
            webBrowser.Navigate(baseURL);
            webBrowser.NewWindow += WebBrowser_NewWindow;


            webBrowser.ScriptErrorsSuppressed = true;
            isCompleted = false;
            do
            {
                Thread.Sleep(10);
                Application.DoEvents();
            } while (!isCompleted);
        }


        public KABISResponse RentVehicleThreadProcess(KABISIntegrationUser OfficeUser, KABISCustomer rentCustomer, string plate, int vehicleExitKm)
        {
            KABISResponse response = null;
            Thread thread = new Thread(() => { response = RentVehicle(OfficeUser, rentCustomer, plate, vehicleExitKm); });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            return response;

        }
        public KABISResponse ReturnVehicleThreadProcess(KABISIntegrationUser OfficeUser, string plate, int entranceKm)
        {
            KABISResponse response = null;
            Thread thread = new Thread(() => { response = ReturnVehicle(OfficeUser, plate, entranceKm); });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

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
                    if (HomePage != null)
                    {
                        Console.WriteLine("Giriş Yapıldı");
                        return true;
                    }
                    var dialogForm = webBrowser.Document.GetElementById("dialog-duyuru");
                    if (dialogForm != null)
                        dialogForm.OuterHtml = "";
                    webBrowser.Document.GetElementById("username").InnerText = kABISIntegrationUser.userName;
                    webBrowser.Document.GetElementById("password").InnerText = kABISIntegrationUser.Password;
                    webBrowser.Document.GetElementById("txtCaptcha").InnerText = ResolveCaptcha();
                    isCompleted = false;
                    webBrowser.Document.GetElementById("btn_giris").InvokeMember("Click");

                }
                Console.WriteLine("Giriş Yapılamadı");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        private KABISResponse RentVehicle(KABISIntegrationUser OfficeUser, KABISCustomer rentCustomer, string plate, int vehicleExitKm)
        {
            try
            {
                createWebBrowser();

                if (!Login(OfficeUser))
                    return new KABISResponse() { Code = "300", ResponseResult = ResponseResult.ReturnError("Giriş Yapılamadı!!!") };

                if (!currentUrl.Contains(baseURL + idleVehiclesPage))
                    WebBrowserNavigate(baseURL + idleVehiclesPage);

                if (!SearchVehicle(plate))
                {
                    Console.WriteLine("Araç Bulunamadı. Kiralanmış Araçlara Bakılıyor..");
                    if (!currentUrl.Contains(baseURL + rentedVehiclePage))
                        WebBrowserNavigate(baseURL + rentedVehiclePage);
                    if (!SearchVehicle(plate))
                    {
                        Console.WriteLine("Araç bu kabis hesabında bulunamadı.");
                        return new KABISResponse() { Code = "305", ResponseResult = ResponseResult.ReturnError("Araç bu kabis hesabında bulunamadı.") };
                    }
                    else
                    {
                        SelectVehicle(returnVehiclePage, plate);
                        if (webBrowser.DocumentText.Contains(rentCustomer.Identity))
                        {
                            Console.WriteLine("Araç kiraya verilmiş.");
                            return new KABISResponse() { Code = "200", ResponseResult = ResponseResult.ReturnSuccess() };
                        }
                        else
                        {
                            var rentedCustomerName = webBrowser.Document.GetElementById("lbl_adi").InnerText;
                            var rentedCustomerSurname = webBrowser.Document.GetElementById("lbl_soyadi").InnerText;
                            var rentedIdentity = webBrowser.Document.GetElementById("lbl_tcno").InnerText;
                            var message = $"Araç başka bir müşteride kirada gözüküyor. Şuan Kirada gözüken : ({rentedIdentity}) {rentedCustomerName + " " + rentedCustomerSurname} --- Kiralamaya çalışan : ({rentCustomer.Identity}) {rentCustomer.Name + " " + rentCustomer.Surname}";
                            Console.WriteLine(message);
                            Console.WriteLine("Araç Kiradan alınıyor..");

                            isCompleted = false;
                            webBrowser.Document.GetElementById("txt_donuskm").InnerText = vehicleExitKm.ToString();
                            webBrowser.Document.GetElementById("btn_kaydet").InvokeMember("Click");

                            HtmlDocument htmlDocument = webBrowser.Document;
                            htmlDocument.Window.Unload += new HtmlElementEventHandler(Window_Unload);
                            do
                            {
                                Thread.Sleep(10);
                                Application.DoEvents();
                                AddBlockAlerts();
                            } while (!isCompleted);

                            // Kiradan aldıktan sonra tekrar kiralamayı dene.. 
                            try
                            {
                                createWebBrowser();

                                if (!Login(OfficeUser))
                                    return new KABISResponse() { Code = "300", ResponseResult = ResponseResult.ReturnError("Giriş Yapılamadı!!!") };

                                if (!currentUrl.Contains(baseURL + idleVehiclesPage))
                                    WebBrowserNavigate(baseURL + idleVehiclesPage);
                            }
                            catch (Exception ex)
                            {
                            }

                        }
                    }
                }


                if (!SelectVehicle(rentVehiclePage, plate))
                {
                    Console.WriteLine("Araç Seçilemedi. ");
                    return new KABISResponse() { Code = "304", ResponseResult = ResponseResult.ReturnError("Araç listede Seçilemedi.") };
                }

                SelectNational(rentCustomer.Type);

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

                    HtmlDocument htmlDocument = webBrowser.Document;
                    htmlDocument.Window.Unload += new HtmlElementEventHandler(Window_Unload);
                    do
                    {
                        AddBlockAlerts();
                        Application.DoEvents();
                    } while (!isCompleted);

                    return GetWebPageResponse();
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private KABISResponse ReturnVehicle(KABISIntegrationUser OfficeUser, string plate, int entranceKm)
        {
            try
            {
                createWebBrowser();
                if (!Login(OfficeUser))
                    return new KABISResponse() { Code = "300", ResponseResult = ResponseResult.ReturnError("Giriş Yapılamadı!!!") };

                if (!currentUrl.Contains(baseURL + rentedVehiclePage))
                    WebBrowserNavigate(baseURL + rentedVehiclePage);

                if (!SearchVehicle(plate))
                {
                    if (!currentUrl.Contains(baseURL + idleVehiclesPage))
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

                if (!SelectVehicle(returnVehiclePage, plate))
                {
                    Console.WriteLine("Araç Seçilemedi. ");
                    return new KABISResponse() { Code = "303", ResponseResult = ResponseResult.ReturnError("Araç Seçilemedi.") };
                }

                var controlPage = webBrowser.Document.GetElementById("lbl_plaka");
                if (controlPage != null)
                {
                    isCompleted = false;
                    webBrowser.Document.GetElementById("txt_donuskm").InnerText = entranceKm.ToString();
                    webBrowser.Document.GetElementById("btn_kaydet").InvokeMember("Click");

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

                return null;
            }
            catch (Exception ex)
            {
                return new KABISResponse() { Code = "105", ResponseResult = ResponseResult.ReturnError(ex.Message) };
            }
        }
        private KABISResponse RemoveVehicle(KABISIntegrationUser OfficeUser, string plate)
        {
            try
            {
                createWebBrowser();

                if (!Login(OfficeUser))
                    return new KABISResponse() { Code = "300", ResponseResult = ResponseResult.ReturnError("Giriş Yapılamadı!!!") };

                if (!currentUrl.Contains(baseURL + addRemoveVehiclePage))
                    WebBrowserNavigate(baseURL + addRemoveVehiclePage);

                isCompleted = false;
                webBrowser.Document.GetElementsByTagName("option")[0].Document.GetElementsByTagName("option")[2].SetAttribute("selected", "selected");
                webBrowser.Document.InvokeScript("__doPostBack");
                do
                {
                    Thread.Sleep(10);
                    Application.DoEvents();
                } while (!isCompleted);
                 
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(webBrowser.DocumentText);
                string _tsortableId = "tSortable";
                for (int i = 1; i <=5; i++)
                {
                    if (doc.DocumentNode.SelectSingleNode("//table[@id='"+_tsortableId+ "']") != null)
                        break;
                    _tsortableId = "tSortable"+i.ToString();
                }
                var selectSingleNode = doc.DocumentNode.SelectSingleNode("//table[@id='" + _tsortableId + "']");
                selectSingleNode = selectSingleNode == null ? doc.DocumentNode.SelectSingleNode("//table[@id='tSortable']") : selectSingleNode;
                if (selectSingleNode != null)
                {
                    List<List<string>> table = selectSingleNode
                                .Descendants("tr")
                                .Skip(1)
                                .Where(tr => tr.Elements("td").Count() > 1)
                                .Select(tr => tr.Elements("td").Select(td => td.InnerHtml.Trim()).ToList())
                                .ToList();

                    var checkId = table.Where(t => t[1].Contains(plate)).Select(t1 => t1[0]).FirstOrDefault();
                    if (checkId != null)
                        checkId = checkId.Substring(checkId.IndexOf("id=\"") + 4, checkId.Length - checkId.IndexOf("id=\"") - 6);
                    else
                        return new KABISResponse() { Code = "302", ResponseResult = ResponseResult.ReturnError("Plaka bulunamadı.") };

                    var id = checkId.Replace("Bos_arac_liste_ctrl", "").Replace("_checkbox", "");
                    var pageNumber = Math.Floor(int.Parse(id) / 5.0);
                    var _paginate = webBrowser.Document.GetElementById(_tsortableId+"_paginate");

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
            catch (Exception ex)
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
            catch (Exception ex)
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
            var str = String.Format("{0}?plaka={1}", pageToRedirect, plate);
            foreach (HtmlElement a1 in a)
            {
                if (a1.GetAttribute("href").Contains(String.Format("{0}?plaka={1}", pageToRedirect, plate)))
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
            catch (Exception)
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
