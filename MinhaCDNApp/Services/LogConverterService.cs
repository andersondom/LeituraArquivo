using System.Globalization;
using System.Net;
using MinhaCDN.Models;

namespace MinhaCDN.Services;

public abstract class LogConverterService
{
    
    //private static HttpClient? _httpClient;

    //public LogConverterService()
    //{
        //_httpClient = new HttpClient();
    //}
    
    [Obsolete("Obsolete")]
    public static Task<List<MinhaCdnLog>> ReadLogsFromSourceAsync(string sourceUrl)
    {
        try
        {
            using var client = new WebClient();
            var response =
                client.DownloadString(sourceUrl);

            var logs = new List<MinhaCdnLog>();
            var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var logParts = line.Replace("\r", "").Replace("INVALIDATE", "REFRESH_HIT").Split('|');

                if (logParts.Length == 5)
                {
                    var log = new MinhaCdnLog
                    {
                        ProviderId = int.Parse(logParts[0]),
                        HttpStatusCode = int.Parse(logParts[1]),
                        CacheStatus = logParts[2],
                        Request = logParts[3],
                        TimeTaken = (int)Math.Round(double.Parse(logParts[4], CultureInfo.InvariantCulture))
                    };

                    logs.Add(log);
                }
            }

            return Task.FromResult(logs);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading logs from {sourceUrl}: {ex.Message}");
            throw;
        }
    }
    
    public static List<string> ConvertToAgoraFormat(List<MinhaCdnLog> logs)
    {
        var agoraLogs = new List<string>();

        agoraLogs.Add("#Version: 1.0");
        agoraLogs.Add("#Date: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
        agoraLogs.Add("#Fields: provider http-method status-code uri-path time-taken response-size cache-status");

        foreach (var log in logs)
        {
            string agoraLog = $"\"MINHA CDN\" {GetHttpMethod(log.Request)} {log.HttpStatusCode} {GetUriPath(log.Request)} {log.TimeTaken} {log.ProviderId} {log.CacheStatus}";

            agoraLogs.Add(agoraLog);
        }

        return agoraLogs;
    }
    
    private static string GetHttpMethod(string? request)
    {
        string[] requestParts = request!.Split(' ');
        if (requestParts.Length >= 3)
        {
            return requestParts[0].Trim();
        }
        else
        {
            return "UNKNOWN_METHOD";
        }
    }
    
    private static string GetUriPath(string? request)
    {
        string[] requestParts = request!.Split(' ');
        
        if (requestParts.Length >= 2)
        {
            string uriWithQuery = requestParts[1].Trim();
        
            int queryIndex = uriWithQuery.IndexOf('?');
        
            if (queryIndex != -1)
            {
                return uriWithQuery.Substring(0, queryIndex);
            }
            else
            {
                return uriWithQuery;
            }
        }
        else
        {
            return "UNKNOWN_URI";
        }
    }
}