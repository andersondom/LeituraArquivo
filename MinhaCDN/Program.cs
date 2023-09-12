using System.Text.RegularExpressions;
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
                dadosInformados = Console.ReadLine();
            }
            string pattern = @"^convert\s+\S+\s+\S+$";

            bool isMatch = Regex.IsMatch(dadosInformados, pattern);

            if (isMatch)
            {
                var campos = dadosInformados!.Replace("convert ", "").Split("> <");
            
                var sourceUrl = campos[0].Replace("<", "");
                var targetPath = campos[1].Replace(">", "");
            
                var logs = await LogConverterService.ReadLogsFromSourceAsync(sourceUrl);

                var agoraLogs = LogConverterService.ConvertToAgoraFormat(logs);

                await File.WriteAllLinesAsync(targetPath, agoraLogs);
                
                Console.WriteLine("Conversion completed.");
            }
            else
            {
                Console.WriteLine("The string is not in the correct format.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}