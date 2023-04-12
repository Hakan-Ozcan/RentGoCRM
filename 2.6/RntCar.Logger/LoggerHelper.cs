using log4net;
using log4net.Appender;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.Logger
{
    public class LoggerHelper
    {
        private ILog logger { get; set; }
        private StackTrace stackTrace { get; set; }
        private bool isLogEnabled { get; set; }
        public LoggerHelper(string uniqueField = "")
        {
            stackTrace = new StackTrace();

            var logFileName = stackTrace.GetFrame(1).GetMethod().Name;
            if (!string.IsNullOrWhiteSpace(uniqueField))
            {
                logFileName = uniqueField;
            }
            var path = ConfigurationManager.AppSettings["logPath"];
            log4net.GlobalContext.Properties["LogName"] = path + "/" + logFileName;

            log4net.Config.XmlConfigurator.Configure();
            logger = LogManager.GetLogger(stackTrace.GetFrame(1).GetMethod().DeclaringType);
            var logEnabled = ConfigurationManager.AppSettings["isLogEnabled"];
            this.isLogEnabled = !string.IsNullOrEmpty(logEnabled) ? Convert.ToBoolean(logEnabled) : false;
        }

        public void traceInfo(string text)
        {
            if (isLogEnabled)
                logger.Info(DateTime.Now + " - " + text);
        }
        public void traceInputsInfo<T>(T item, string text = "") where T : class
        {
            if (isLogEnabled)
                logger.Info(DateTime.Now + " - " + text + JsonConvert.SerializeObject(item));
        }
        public void traceError(string text)
        {
            if (isLogEnabled)
                logger.Error(DateTime.Now + " - " + text);
        }
        public void traceFlush()
        {
            var test = logger.Logger.Repository;
            LogManager.Flush(50000);
            TextWriterAppender textWriterAppender = new TextWriterAppender();
            textWriterAppender.Flush(100);
            IAppender[] logAppenders = logger.Logger.Repository.GetAppenders();
            for (int i = 0; i < logAppenders.Length; i++)
            {
                if (logAppenders[i] != null)
                {
                    var buffered = logAppenders[i] as RollingFileAppender;
                    if (buffered is RollingFileAppender)
                    {
                        ((RollingFileAppender)buffered).Flush(1000);
                    }
                }
            }
        }
    }
}
