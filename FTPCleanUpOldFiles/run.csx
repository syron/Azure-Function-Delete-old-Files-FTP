using System;
using System.Configuration;
using System.IO;
using System.Net;

public static void Run(TimerInfo myTimer, TraceWriter log)
{
    string serverUri = ConfigurationManager.AppSettings["FTPServerURI"].ToString();
    string ftpUsername = ConfigurationManager.AppSettings["FTPUsername"].ToString();
    string ftpPassword = ConfigurationManager.AppSettings["FTPPassword"].ToString();

    string filter = ".zip";
    string numberOfDays = -3;

    FtpWebRequest directoryListRequest = (FtpWebRequest)WebRequest.Create(serverUri);

    directoryListRequest.Method = WebRequestMethods.Ftp.ListDirectory;
    directoryListRequest.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

    using (FtpWebResponse directoryListResponse = (FtpWebResponse)directoryListRequest.GetResponse())
    {
        using (StreamReader directoryListResponseReader = new StreamReader(directoryListResponse.GetResponseStream()))
        {
            string responseString = directoryListResponseReader.ReadToEnd();
            var results = responseString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList().Where(f => f.Contains(filter));
            log.Info(string.Format("Number of files containing '.zip': {0}.", results.Count()));
            foreach (var file in results) {
                string fileUri = string.Format("{0}/{1}", serverUri, file);
                DateTime lastModified = GetDateTimestampOnServer(fileUri, ftpUsername, ftpPassword);

                if (lastModified < DateTime.Now.AddDays(numberOfDays)) {
                    log.Info(string.Format("File '{0}' will be deleted.", file));
                    
                    try {
                        DeleteFileOnFtpServer(fileUri, ftpUsername, ftpPassword);
                        log.Info(string.Format("File '{0}' has been deleted.", file));
                    }
                    catch (Exception ex) {
                        log.Info(string.Format("File '{0}' has not been deleted. Exception: {1}.", file, ex.ToString()));
                    }
                }
            }
        }

    }
}

public static DateTime GetDateTimestampOnServer (string serverUri, string ftpUsername, string ftpPassword)
{
    FtpWebRequest request = (FtpWebRequest)WebRequest.Create (serverUri);
    request.Method = WebRequestMethods.Ftp.GetDateTimestamp;
    request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
    FtpWebResponse response = (FtpWebResponse)request.GetResponse ();

    return response.LastModified;
}

public static void DeleteFileOnFtpServer(string serverUri, string ftpUsername, string ftpPassword)
{
    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(serverUri);
    request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
    request.Method = WebRequestMethods.Ftp.DeleteFile;

    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
    response.Close();         
}