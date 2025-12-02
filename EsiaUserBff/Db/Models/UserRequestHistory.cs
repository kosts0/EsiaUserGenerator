namespace EsiaUserGenerator.Db.Models;

public class UserRequestHistory
{
    public Guid UserId { get; set; }
    public ICollection<EsiaUser> GeneratedPofiles { get; set; }

    public Guid RequestId { get; set; }
    public string JsonRequest { get; set; } = null!;
}