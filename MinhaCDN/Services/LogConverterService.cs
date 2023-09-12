using System.Globalization;
using System.Net;

namespace MinhaCDN.Services;

public class LogConverterService
{
    
    private static HttpClient? _httpClient;

    public LogConverterService()
    {
        _httpClient = new HttpClient();
    }
    
    public static async Task<List<MinhaCdnLog>> ReadLogsFromSourceAsync(string sourceUrl)
    {
        try
        {
            var response = "";
            
            using (var client = new WebClient())
            {
                response =
                    client.DownloadString(sourceUrl);
            }

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

            return logs;
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

        // Adicionar cabeçalho Agora
        agoraLogs.Add("#Version: 1.0");
        agoraLogs.Add("#Date: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
        agoraLogs.Add("#Fields: provider http-method status-code uri-path time-taken response-size cache-status");

        foreach (var log in logs)
        {
            // Mapear os campos do log MINHA CDN para o formato Agora
            string agoraLog = $"\"MINHA CDN\" {GetHttpMethod(log.Request)} {log.HttpStatusCode} {GetUriPath(log.Request)} {log.TimeTaken} {log.ProviderId} {log.CacheStatus}";

            agoraLogs.Add(agoraLog);
        }

        return agoraLogs;
    }
    
    private static string GetHttpMethod(string request)
    {
        string[] requestParts = request.Split(' ');
        if (requestParts.Length >= 3)
        {
            // O método HTTP é a primeira parte da string de solicitação
            return requestParts[0].Trim();
        }
        else
        {
            // Caso não seja possível extrair o método HTTP, você pode retornar um valor padrão ou lidar com o erro de outra forma.
            return "UNKNOWN_METHOD";
        }
    }
    
    private static string GetUriPath(string request)
    {
        string[] requestParts = request.Split(' ');
        
        if (requestParts.Length >= 2)
        {
            // O caminho URI é a segunda parte da string de solicitação
            // É necessário remover qualquer query string que possa estar presente
            string uriWithQuery = requestParts[1].Trim();
        
            // Localizar o ponto de interrogação para remover qualquer query string
            int queryIndex = uriWithQuery.IndexOf('?');
        
            if (queryIndex != -1)
            {
                // Se uma query string for encontrada, remova-a
                return uriWithQuery.Substring(0, queryIndex);
            }
            else
            {
                // Se não houver query string, retorne o caminho URI original
                return uriWithQuery;
            }
        }
        else
        {
            // Caso não seja possível extrair o caminho URI, você pode retornar um valor padrão ou lidar com o erro de outra forma.
            return "UNKNOWN_URI";
        }
    }
}