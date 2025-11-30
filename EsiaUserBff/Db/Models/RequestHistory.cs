namespace EsiaUserGenerator.Db.Models;

public class RequestHistory
{
    public Guid UserId { get; set; }
    public EsiaUser User { get; set; } = null!;

    public Guid RequestId { get; set; }
    public string JsonRequest { get; set; } = null!;
}
