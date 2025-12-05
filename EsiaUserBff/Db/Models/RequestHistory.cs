namespace EsiaUserGenerator.Db.Models;

public class RequestHistory
{
    public Guid RequestId { get; set; }
    public string JsonRequest { get; set; } = null!;
    public string CurrentStatus { get; set; }
    
    public EsiaUser User { get; set; } = null!;
    
    public DateTime DateTimeCreated { get; set; }
    public DateTime LastModified { get; set; }
    public bool Finished { get; set; }
}