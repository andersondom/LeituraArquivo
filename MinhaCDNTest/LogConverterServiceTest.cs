using System.Diagnostics;
using MinhaCDN.Models;
using MinhaCDN.Services;
using Moq;

namespace MinhaCDNTest;

public class LogConverterServiceTest
{
    private static readonly ILogConverterService _logConverterService;
    
    [Fact]
        public async Task ReadLogsFromSourceAsync_ValidSourceUrl_ReturnsListOfLogs()
        {
            // Arrange
            var sourceUrl = "https://s3.amazonaws.com/uux-itaas-static/minha-cdn-logs/input-01.txt";
            if (sourceUrl == null) throw new ArgumentNullException(nameof(sourceUrl));
            var logContent = "312|200|HIT|\"GET /robots.txt HTTP/1.1\"|100.2\n101|200|MISS|\"POST /myImages HTTP/1.1\"|319.4\n";

            var webClientWrapperMock = new Mock<IWebClientWrapper>();
            if (true)
            {
                Debug.Assert(logContent != null, nameof(logContent) + " != null");
                webClientWrapperMock.Setup(client => client.DownloadString(sourceUrl))
                    .Returns(logContent);
            }

            var logConverterService = new LogConverterService(webClientWrapperMock.Object);

            // Act
            var logs = await logConverterService.ReadLogsFromSourceAsync(sourceUrl);

            // Assert
            Assert.NotNull(logs);
            Assert.Equal(4, logs.Count);
        }

        [Fact]
        public async void ConvertToAgoraFormat_ValidLogs_ReturnsListOfStringsInAgoraFormat()
        {
            // Arrange
            var logs = new List<MinhaCdnLog>
            {
                new MinhaCdnLog
                {
                    ProviderId = 312,
                    HttpStatusCode = 200,
                    CacheStatus = "HIT",
                    Request = "GET /robots.txt HTTP/1.1",
                    TimeTaken = 100.2
                },
                new MinhaCdnLog
                {
                    ProviderId = 101,
                    HttpStatusCode = 200,
                    CacheStatus = "MISS",
                    Request = "POST /myImages HTTP/1.1",
                    TimeTaken = 319.4
                },
                new MinhaCdnLog
                {
                    ProviderId = 199,
                    HttpStatusCode = 404,
                    CacheStatus = "MISS",
                    Request = "GET /not-found HTTP/1.1",
                    TimeTaken = 142.9
                },
                new MinhaCdnLog
                {
                    ProviderId = 312,
                    HttpStatusCode = 200,
                    CacheStatus = "REFRESH_HIT",
                    Request = "GET /robots.txt HTTP/1.1",
                    TimeTaken = 245.1
                }
            };

            var logConverterService = new LogConverterService();
            
            // Act
            var agoraLogs = await logConverterService.ConvertToAgoraFormat(logs);

            // Assert
            Assert.NotNull(agoraLogs);
            Assert.Equal(7, agoraLogs.Count);
        }
}