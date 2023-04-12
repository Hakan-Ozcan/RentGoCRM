using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using Tamir.SharpSsh;

namespace RntCar.IntegrationHelper
{
    public class SFTPHelper
    {
        public bool CreateFTPEntegration(string configValue)
        {
            String ftpserver = "ftp://ftp.cihatkir.com/YKB/";
            FtpWebRequest reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpserver));
            reqFTP.UsePassive = false;
            reqFTP.UseBinary = true;
            reqFTP.Credentials = new NetworkCredential("cihatkir", "cihat123*.kir1");
            reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
            reqFTP.Proxy = GlobalProxySelection.GetEmptyWebProxy();

            FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
            //use the response like below
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);

            string[] allLines = reader.ReadToEnd().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            //string[] allLines = System.IO.File.ReadAllLines(@"D:\CSV\data.csv");

            return true;
        }

        public SFTPResponse GetFileInFTP(SFTPObject ftpLoginObject)
        {
            SFTPResponse sftpResponse = new SFTPResponse();
            sftpResponse.ResponseResult = new ResponseResult();
            try
            {
                var sftp = new Sftp(ftpLoginObject.sftphostName, ftpLoginObject.userName, ftpLoginObject.password);
                sftp.Connect(ftpLoginObject.sftpPort);
                try
                {
                    sftp.Get(ftpLoginObject.sourceFilePath, ftpLoginObject.targetFilePath);
                    sftpResponse.ResponseResult.Result = true;
                }
                catch (Exception ex)
                {
                    sftpResponse.ResponseResult.Result = false;
                    sftpResponse.ResponseResult.ExceptionDetail = ex.Message;
                }
                sftp.Close();
            }
            catch (Exception ex)
            {
                sftpResponse.ResponseResult.Result = false;
                sftpResponse.ResponseResult.ExceptionDetail = ex.Message;
            }

            return sftpResponse;
        }
    }
}
