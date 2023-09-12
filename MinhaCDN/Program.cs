using System.Net;
using MinhaCDN;
using MinhaCDN.Services;

class Program
{
    static async Task Main(string[] args)
    {
        var dadosInformados = "";
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: convert <sourceUrl> <targetPath>");
            dadosInformados = Console.ReadLine().ToString().Replace("convert ", "");
        }

        var campos = dadosInformados.Split("> <");
        try
        {
            
            string sourceUrl = campos[0].Replace("<", "");
            string targetPath = campos[1].Replace(">", "");
            
            var logs = await LogConverterService.ReadLogsFromSourceAsync(sourceUrl);
            
            var minhaCdn = new MinhaCdnLog();
            
            
            // Converter os logs para o formato Agora
            var agoraLogs = LogConverterService.ConvertToAgoraFormat(logs);

            // Escrever os logs convertidos no arquivo de destino
            File.WriteAllLines(targetPath, agoraLogs);
            

            Console.WriteLine("Conversion completed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}