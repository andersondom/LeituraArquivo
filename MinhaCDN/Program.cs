using MinhaCDN.Services;

namespace MinhaCDN;

class Program
{
    [Obsolete("Obsolete")]
    static async Task Main(string[] args)
    {
        try
        {
            var dadosInformados = "";
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: convert <sourceUrl> <targetPath>");
                dadosInformados = Console.ReadLine()!.Replace("convert ", "");
            }

            var campos = dadosInformados.Split("> <");
            
            var sourceUrl = campos[0].Replace("<", "");
            var targetPath = campos[1].Replace(">", "");
            
            var logs = await LogConverterService.ReadLogsFromSourceAsync(sourceUrl);

            var agoraLogs = LogConverterService.ConvertToAgoraFormat(logs);

            await File.WriteAllLinesAsync(targetPath, agoraLogs);

            Console.WriteLine("Conversion completed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}