using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins._Common
{
    public class ExecuteGetUserTime : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)// Bu parametre, CRM'de çalışan eklentinin çalıştığı işlem ortamına erişim sağlar.
        {
            PluginInitializer pluginInitializer = new PluginInitializer(serviceProvider);//PluginInitializer, CRM'nin temel servislerine erişim sağlamak için gerekli olan işlem ortamını (context) temsil eder.
            XrmHelper xrmHelper = new XrmHelper(pluginInitializer.Service);//XrmHelper, CRM'nin dinamik olarak oluşturulan örneklerine erişim sağlamak için kullanılan yardımcı bir sınıftır.
            var result = xrmHelper.getCurrentUserTimeInfo();//xrmHelper nesnesi, getCurrentUserTimeInfo() metodunu çağırarak, kullanıcının zaman dilimi bilgilerini alır. Bu metod, Dynamics 365 API'si (Application Programming Interface) kullanılarak, kullanıcının profilindeki zaman dilimi ayarlarından bilgi alır ve bu bilgileri bir C# nesnesi olarak döndürür.

            pluginInitializer.PluginContext.OutputParameters["UserTimeInfo"] = JsonConvert.SerializeObject(result);//Son olarak, pluginInitializer.PluginContext.OutputParameters["UserTimeInfo"] ifadesi, kullanıcının zaman dilimi bilgilerinin JSON nesnesi olarak döndürülmesini sağlar. Bu nesne, eklentinin çağrıldığı yere (örneğin bir web uygulamasına) geri döndürülür ve kullanılabilir. Bu örnekte, JSON nesnesi, UserTimeInfo adlı bir çıkış parametresi olarak tanımlanmıştır.
        }
    }
}
