namespace MinhaCDN.Models;

public class MinhaCdnLog
{
    public int ProviderId { get; set; }
    public int HttpStatusCode { get; set; }
    public string? CacheStatus { get; set; }
    public string? Request { get; set; }
    public double TimeTaken { get; set; }
}